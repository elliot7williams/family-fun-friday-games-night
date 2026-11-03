using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Lea.Print;

internal static class PrintWpfImageChecks
{
    internal static int Run(Ps snapshot, IReadOnlyDictionary<int, Pi> images, Pg geometry, string folder)
    {
        int count = 0;
        Action<bool, string> check = (value, name) => { if (!value) throw new Exception(name); count++; };
        Action<Action, string> reject = (action, name) => { bool denied = false; try { action(); } catch (ArgumentException) { denied = true; } catch (InvalidOperationException) { denied = true; } check(denied, name); };
        Pi first = images.First().Value;
        var source = Mi.Cp(first, false); var gray = Mi.Cp(first, true);
        check(source.IsFrozen && gray.IsFrozen, "raw-pixel WPF sources frozen");
        var colorPixels = new byte[first.Width * first.Height * 4]; var grayPixels = new byte[colorPixels.Length];
        source.CopyPixels(colorPixels, first.Width * 4, 0); gray.CopyPixels(grayPixels, first.Width * 4, 0);
        check(colorPixels[2] == 255 && colorPixels[0] == 0 && colorPixels[(first.Width - 1) * 4] == 255, "WPF raw pixel channel order");
        check(Enumerable.Range(0, first.Width * first.Height).All(i => grayPixels[i * 4] == grayPixels[i * 4 + 1] && grayPixels[i * 4 + 1] == grayPixels[i * 4 + 2] && grayPixels[i * 4 + 3] == colorPixels[i * 4 + 3]), "WPF grayscale preserves alpha");
        var flow = Pm.Ci(snapshot, geometry, images); int imageElements = 0;
        for (var at = flow.ContentStart; at != null && at.CompareTo(flow.ContentEnd) < 0; at = at.GetNextContextPosition(LogicalDirection.Forward))
            if (at.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart && at.GetAdjacentElement(LogicalDirection.Forward) is InlineUIContainer) imageElements++;
        check(imageElements == images.Count, "semantic flow contains actual image elements");
        check(new TextRange(flow.ContentStart, flow.ContentEnd).Text.Contains("DECODER_ARTICLE"), "image flow preserves article text");
        reject(() => Pm.Ci(snapshot, geometry, new Dictionary<int, Pi>()), "missing WPF resource fails closed");
        reject(() => Pm.Ci(snapshot, geometry, new Dictionary<int, Pi> { { 0, first } }), "foreign WPF map entry");
        reject(() => Pm.Ci(snapshot, geometry, null), "null WPF image map");
        reject(() => Task.Run(() => Pm.Ci(snapshot, geometry, images)).GetAwaiter().GetResult(), "WPF image flow wrong thread");
        string path = Path.Combine(folder, "image-layout.xps");
        using (var plan = Pw.Di(snapshot, geometry, images))
        {
            check(plan.UnresolvedImages == 0 && plan.Structured && plan.PageText.Count > 0, "resolved image plan provenance");
            var pages = new Pp(plan, "", "");
            using (var page = pages.GetPage(0))
            {
                var bitmap = new RenderTargetBitmap(816, 1056, 96, 96, PixelFormats.Pbgra32); bitmap.Render(page.Visual);
                var pixels = new byte[816 * 1056 * 4]; bitmap.CopyPixels(pixels, 816 * 4, 0); int red = 0, blue = 0;
                for (int i = 0; i < pixels.Length; i += 4)
                { if (pixels[i + 2] > 200 && pixels[i + 1] < 40 && pixels[i] < 40) red++; if (pixels[i] > 200 && pixels[i + 1] < 40 && pixels[i + 2] < 40) blue++; }
                check(red >= 100 && blue >= 100, "paginated WPF actual image color pixels");
                var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using (var file = new FileStream(Path.Combine(folder, "image-layout.png"), FileMode.CreateNew)) encoder.Save(file);
            }
            using (var file = new FileStream(path, FileMode.CreateNew)) check(Px.Wr(pages, file) == pages.PageCount, "image plan XPS export");
        }
        using (var package = Package.Open(path, FileMode.Open, FileAccess.Read))
            check(package.GetParts().Any(p => p.ContentType.StartsWith("image/", StringComparison.Ordinal)), "XPS contains embedded image resource");
        var grayGeometry = Pg.Cp(new Po(false, .75, 100, true, false, false, false), 850, 1100, new Pb(0, 0, 850, 1100));
        using (var plan = Pw.Di(snapshot, grayGeometry, images))
        using (var page = new Pp(plan, "", "").GetPage(0))
        {
            var bitmap = new RenderTargetBitmap(816, 1056, 96, 96, PixelFormats.Pbgra32); bitmap.Render(page.Visual);
            var pixels = new byte[816 * 1056 * 4]; bitmap.CopyPixels(pixels, 816 * 4, 0); int colored = 0, redGray = 0, blueGray = 0;
            for (int i = 0; i < pixels.Length; i += 4)
            {
                if (pixels[i] != pixels[i + 1] || pixels[i + 1] != pixels[i + 2]) colored++;
                if (pixels[i] == 76 && pixels[i + 1] == 76 && pixels[i + 2] == 76) redGray++;
                if (pixels[i] == 29 && pixels[i + 1] == 29 && pixels[i + 2] == 29) blueGray++;
            }
            check(colored == 0, "paginated grayscale image has no colored pixels");
            check(redGray >= 100 && blueGray >= 100, "paginated grayscale image retains two luminance regions");
            var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (var file = new FileStream(Path.Combine(folder, "image-layout-gray.png"), FileMode.CreateNew)) encoder.Save(file);
        }
        var imageOnly = PrintImageChecks.Snapshot(1);
        using (var plan = Pw.Di(imageOnly, geometry, new Dictionary<int, Pi> { { 1, first } }))
            check(plan.PageText.Count > 0 && plan.UnresolvedImages == 0, "image-only page plan");
        return count;
    }
}
