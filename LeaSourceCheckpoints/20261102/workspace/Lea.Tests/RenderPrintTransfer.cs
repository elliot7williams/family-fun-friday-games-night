using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
using System.Web.Script.Serialization;

internal static class RenderPrintTransfer
{
    private sealed class Image
    {
        internal byte[] Pixels;
        internal int Width, Height;
        internal long Visible;
        internal long BodyVisible;
    }
    private static Image Render(DocumentPaginator paginator, int index, string directory, string prefix)
    {
        using (DocumentPage page = paginator.GetPage(index))
        {
            // Normalize floating-point serialization noise before choosing raster dimensions.
            double w = Math.Ceiling(Math.Round(page.Size.Width, 6)), h = Math.Ceiling(Math.Round(page.Size.Height, 6));
            if (Double.IsNaN(w) || Double.IsNaN(h) || w < 1 || h < 1 || w > 2048 || h > 2048)
                throw new InvalidDataException("Bounded page dimensions required");
            var result = new Image { Width = (int)w, Height = (int)h };
            var element = page.Visual as UIElement;
            if (element != null) { element.Measure(page.Size); element.Arrange(new Rect(page.Size)); element.UpdateLayout(); }
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(delegate { }));
            var bitmap = new RenderTargetBitmap(result.Width, result.Height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(page.Visual);
            result.Pixels = new byte[result.Width * result.Height * 4];
            bitmap.CopyPixels(result.Pixels, result.Width * 4, 0);
            for (int i = 0; i < result.Pixels.Length; i += 4)
                if (result.Pixels[i + 3] != 0 && (result.Pixels[i] < 245 || result.Pixels[i + 1] < 245 || result.Pixels[i + 2] < 245))
                {
                    result.Visible++;
                    int x = (i / 4) % result.Width, y = (i / 4) / result.Width;
                    if (x >= 72 && x < 744 && y >= 101 && y < 955) result.BodyVisible++;
                }
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (var stream = new FileStream(Path.Combine(directory, prefix + "-" + (index + 1) + ".png"), FileMode.CreateNew))
                encoder.Save(stream);
            return result;
        }
    }
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 3) throw new ArgumentException("Source, returned and output directory required");
            using (var a = new XpsDocument(args[0], FileAccess.Read))
            using (var b = new XpsDocument(args[1], FileAccess.Read))
            {
                DocumentPaginator left = a.GetFixedDocumentSequence().DocumentPaginator;
                DocumentPaginator right = b.GetFixedDocumentSequence().DocumentPaginator;
                left.ComputePageCount(); right.ComputePageCount();
                if (left.PageCount != 3 || right.PageCount != 3) throw new InvalidDataException("Expected three pages");
                var rows = new List<object>();
                bool identical = true;
                for (int p = 0; p < 3; p++)
                {
                    Image l = Render(left, p, args[2], "source"), r = Render(right, p, args[2], "returned");
                    if (l.Width != r.Width || l.Height != r.Height || l.Visible == 0 || r.Visible == 0 || l.BodyVisible < 1000 || r.BodyVisible < 1000)
                        throw new InvalidDataException("Blank body or mismatched render: source body=" + l.BodyVisible + ", returned body=" + r.BodyVisible);
                    long different = 0;
                    for (int i = 0; i < l.Pixels.Length; i += 4)
                        if (l.Pixels[i] != r.Pixels[i] || l.Pixels[i+1] != r.Pixels[i+1] || l.Pixels[i+2] != r.Pixels[i+2] || l.Pixels[i+3] != r.Pixels[i+3]) different++;
                    identical &= different == 0;
                    rows.Add(new { page = p+1, width = l.Width, height = l.Height,
                        sourceVisiblePixels = l.Visible, returnedVisiblePixels = r.Visible,
                        sourceBodyPixels = l.BodyVisible, returnedBodyPixels = r.BodyVisible, differingPixels = different });
                }
                string json = new JavaScriptSerializer().Serialize(new { rendered = true, pages = rows, printJobSubmitted = false, pixelIdentical = identical });
                using (var file = new FileStream(Path.Combine(args[2], "render.json"), FileMode.CreateNew))
                using (var writer = new StreamWriter(file)) writer.Write(json);
                Console.WriteLine(json);
            }
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
