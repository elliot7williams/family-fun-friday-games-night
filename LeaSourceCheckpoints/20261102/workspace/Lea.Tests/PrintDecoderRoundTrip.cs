using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Lea.Print;
using Lea.Win;
using Forms = System.Windows.Forms;

internal static class PrintDecoderRoundTrip
{
    private static void Ready(V view)
    {
        view.Refresh(); Forms.Application.DoEvents();
        var clock = System.Diagnostics.Stopwatch.StartNew();
        while (view.Busy && clock.ElapsedMilliseconds < 8000) { Forms.Application.DoEvents(); System.Threading.Thread.Sleep(10); }
        if (view.Busy) throw new Exception("Decoder fixture not ready within owned run");
    }
    internal static int Run(string folder)
    {
        int checks = 0;
        Action<bool, string> check = (value, name) => { if (!value) throw new Exception(name); checks++; };
        string picture = Path.Combine(folder, "decoder-source.png");
        using (var bitmap = new Bitmap(32, 16))
        {
            using (var graphics = Graphics.FromImage(bitmap))
            { graphics.Clear(Color.Red); graphics.FillRectangle(Brushes.Blue, 16, 0, 16, 16); }
            using (var output = new FileStream(picture, FileMode.CreateNew)) bitmap.Save(output, System.Drawing.Imaging.ImageFormat.Png);
        }
        string fixture = Path.Combine(folder, "decoder-source.html");
        using (var file = new FileStream(fixture, FileMode.CreateNew))
        using (var writer = new StreamWriter(file)) writer.Write("<!doctype html><html><head><title>Image Round Trip</title></head><body><article><p>DECODER_ARTICLE</p><img src=\"decoder-source.png\" width=\"32\" height=\"16\" alt=\"two colors\"></article></body></html>");
        string copy = Path.Combine(folder, "decoder-copy.html");
        var geometry = Pg.Cp(new Po(false, .75, 100, false, false, false, false), 850, 1100, new Pb(0, 0, 850, 1100));
        using (var form = new Forms.Form { ClientSize = new Size(800, 600) })
        using (var view = new V { Dock = Forms.DockStyle.Fill, Ie = Path.Combine(folder, "Lea.Image.exe") })
        {
            form.Controls.Add(view); form.Show(); view.Go(fixture); Ready(view);
            Console.WriteLine("DECODER source cache=" + view.Ic + " failed=" + view.If + " starts=" + view.In + " workerPresent=" + view.Ii + " diagnostic=" + view.Idg);
            check(view.Ic == 1 && view.If == 0 && view.In > 0, "actual decoder populated image cache");
            check(view.Ij && view.Irt && view.Ili && view.Isp && view.Ihl && view.Ias && view.Icp && view.Iac && view.Ind && view.Ial, "source decoder hardening flags");
            check(view.Icr == 2500, "source decoder CPU budget unchanged");
            using (var pixels = view.Pix(view.At, new Uri(picture)))
                check(pixels.GetPixel(0, 0).ToArgb() == Color.Red.ToArgb() && pixels.GetPixel(31, 0).ToArgb() == Color.Blue.ToArgb(), "actual source decoder pixels");
            var capture = Pc.Cf(view, geometry); int before = view.In;
            var images = Ci.Cp(view, capture);
#if FRIENDLY_LAYOUT
            checks += PrintWpfImageChecks.Run(capture.Document, images, geometry, folder);
#endif
            check(images.Count == 1 && view.In == before, "bridge reused cache without new decode");
            string html = Ht.Cp(capture.Document, new Po(false, .75, 100, false, false, false, false), null, images);
            check(!html.Contains("decoder-source.png") && html.Contains("data:image/png;base64,"), "copy has embedded PNG not source path");
            using (var file = new FileStream(copy, FileMode.CreateNew))
            using (var writer = new StreamWriter(file)) writer.Write(html);
        }
        using (var form = new Forms.Form { ClientSize = new Size(800, 600) })
        using (var view = new V { Dock = Forms.DockStyle.Fill, Ie = Path.Combine(folder, "Lea.Image.exe") })
        {
            form.Controls.Add(view); form.Show(); view.Go(copy); Ready(view);
            check(view.Ic == 1 && view.If == 0 && view.In > 0, "embedded PNG decoded into new cache");
            check(view.Ij && view.Irt && view.Ili && view.Isp && view.Ihl && view.Ias && view.Icp && view.Iac && view.Ind && view.Ial, "embedded decoder hardening flags");
#if FRIENDLY_DATA
            var recaptured = Pc.Cf(view, geometry);
            check(recaptured.Document.Nodes.Single(n => n.Kind == "img").Image == "", "data image public address remains redacted");
            check(typeof(Pn).GetProperty("CacheKey") == null, "cache resource key is not public metadata");
            int decodes = view.In;
            var recapturedImages = Ci.Cp(view, recaptured);
            check(recapturedImages.Count == 1 && view.In == decodes, "embedded image recapture reuses real cache");
            string nextCopy = Ht.Cp(recaptured.Document, new Po(false, .75, 100, false, false, false, false), null, recapturedImages);
            check(nextCopy.Contains("data:image/png;base64,"), "second generation retains embedded image");
            using (var file = new FileStream(Path.Combine(folder, "decoder-second-copy.html"), FileMode.CreateNew))
            using (var writer = new StreamWriter(file)) writer.Write(nextCopy);
#endif
            var page = Pc.Cs(view, geometry);
            check(String.Concat(page.Document.Nodes.Select(n => n.Text)).Contains("DECODER_ARTICLE"), "reopened embedded document content");
            using (var image = new Bitmap(view.Width, view.Height))
            {
                view.DrawToBitmap(image, view.ClientRectangle); int red = 0, blue = 0;
                for (int y = 0; y < image.Height; y++) for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    if (pixel.R > 200 && pixel.G < 40 && pixel.B < 40) red++;
                    if (pixel.B > 200 && pixel.G < 40 && pixel.R < 40) blue++;
                }
                check(red >= 100 && blue >= 100, "reopened embedded image visible two-color pixels");
                image.Save(Path.Combine(folder, "decoder-copy.png"), System.Drawing.Imaging.ImageFormat.Png);
            }
        }
        return checks;
    }
}
