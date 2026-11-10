using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Lea.Win;
using Forms = System.Windows.Forms;

internal static class SdkImageViewMain
{
    private static int checks;
    private static void Check(bool value, string name)
    { if (!value) throw new Exception(name); checks++; }
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 1) return 2;
            string folder = args[0], picture = Path.Combine(folder, "red-blue.png");
            using (var bitmap = new Bitmap(32, 16))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                { graphics.Clear(Color.Red); graphics.FillRectangle(Brushes.Blue, 16, 0, 16, 16); }
                using (var file = new FileStream(picture, FileMode.CreateNew)) bitmap.Save(file, ImageFormat.Png);
            }
            string data = "data:image/png;base64," + Convert.ToBase64String(File.ReadAllBytes(picture));
            Forms.Application.EnableVisualStyles();
            foreach (string mode in new[] { "file", "data" })
            foreach (int width in new[] { 720, 360 })
            {
                string fixture = Path.Combine(folder, mode + "-" + width + ".html");
                using (var file = new FileStream(fixture, FileMode.CreateNew))
                using (var writer = new StreamWriter(file)) writer.Write("<!doctype html><html><head><title>SDK image</title></head><body style='background:white;color:black'><p>SDK_IMAGE_VIEW</p><img src='" + (mode == "file" ? "red-blue.png" : data) + "' width='32' height='16' alt='two colors'></body></html>");
                using (var form = new Forms.Form { ClientSize = new Size(width, 420), AutoScaleMode = Forms.AutoScaleMode.None, Text = "Lea SDK image view" })
                using (var view = new V { Dock = Forms.DockStyle.Fill })
                {
                    form.Controls.Add(view); form.Show(); view.Go(fixture);
                    var clock = Stopwatch.StartNew();
                    do { Forms.Application.DoEvents(); Thread.Sleep(10); } while (view.Busy && clock.ElapsedMilliseconds < 8000);
                    Check(view.Width == width, "image viewport");
                    Check(Path.GetFullPath(view.Ie) == Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lea.Image.exe"), "default bundled worker path");
                    Check(!view.Busy, "image load deadline");
                    Check(view.Ic == 1 && view.If == 0 && view.In > 0, "real worker populated cache: " + view.Idg);
                    Check(view.Ij && view.Irt && view.Ili && view.Isp && view.Ihl && view.Ias && view.Icp && view.Iac && view.Ind && view.Ial, "view worker hardening flags");
                    Check(view.Icr == 2500, "unchanged image CPU budget");
                    Check(view.Dtx().Contains("SDK_IMAGE_VIEW"), "image document text");
                    if (mode == "file")
                    using (var pixels = view.Pix(view.At, new Uri(picture)))
                    {
                        Check(pixels.Width == 32 && pixels.Height == 16, "cached image dimensions");
                        Check(pixels.GetPixel(0, 0).ToArgb() == Color.Red.ToArgb(), "cached red pixel");
                        Check(pixels.GetPixel(31, 0).ToArgb() == Color.Blue.ToArgb(), "cached blue pixel");
                    }
                    using (var rendered = new Bitmap(view.Width, view.Height))
                    {
                        view.DrawToBitmap(rendered, view.ClientRectangle); int red = 0, blue = 0;
                        for (int y = 0; y < rendered.Height; y++)
                            for (int x = 0; x < rendered.Width; x++)
                            {
                                Color pixel = rendered.GetPixel(x, y);
                                if (pixel.R > 200 && pixel.G < 40 && pixel.B < 40) red++;
                                if (pixel.B > 200 && pixel.G < 40 && pixel.R < 40) blue++;
                            }
                        Check(red >= 100, "visible red image pixels");
                        Check(blue >= 100, "visible blue image pixels");
                        rendered.Save(Path.Combine(folder, mode + "-" + width + ".png"), ImageFormat.Png);
                    }
                }
            }
            Console.WriteLine("PASS SDK VIEW IMAGES " + checks); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
