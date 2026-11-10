using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using Lea.Engine;
using Lea.Win;
using Forms = System.Windows.Forms;

internal static class EmbeddingSample
{
    private static int checks;
    private static void Check(bool value, string name)
    { if (!value) throw new Exception(name); checks++; }

    private static Bitmap Paint(V view, string path)
    {
        view.Refresh(); Forms.Application.DoEvents();
        var bitmap = new Bitmap(view.Width, view.Height);
        view.DrawToBitmap(bitmap, view.ClientRectangle);
        bitmap.Save(path, ImageFormat.Png); return bitmap;
    }

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            bool test = args.Length == 3 && args[0] == "--test";
            if (!test && args.Length > 1) return 2;
            string input = test ? args[1] : args.Length == 1 ? args[0] : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "embedding.html");
            if (!File.Exists(input)) throw new FileNotFoundException("Local HTML is required", input);
            if (test)
            {
                var page = new E().Go(input);
                S style = null;
                foreach (var block in page.Bs)
                    if (block.El != null && block.El.Av("id") == "sample") { style = block.S; break; }
                Check(style != null && style.Fs == 30, "relative font size through SDK core");
                Check(style.Ff == "Arial", "font family through SDK core");
            }
            Forms.Application.EnableVisualStyles();
            using (var form = new Forms.Form { Text = "Lea embedding sample", AutoScaleMode = Forms.AutoScaleMode.None, ClientSize = new Size(720, 420) })
            using (var view = new V { Dock = Forms.DockStyle.Fill })
            {
                form.Controls.Add(view);
                if (!test) { view.Go(input); Forms.Application.Run(form); return 0; }
                form.Show();
                foreach (int width in new[] { 720, 360 })
                {
                    form.ClientSize = new Size(width, 420); Forms.Application.DoEvents();
                    view.Go(input); Forms.Application.DoEvents();
                    Check(view.Width == width, "embedded viewport width");
                    string text = view.Dtx();
                    Check(Regex.Replace(text, "\\s+", " ").Trim() == "Lea embedding sample", "embedded visible text");
                    using (var plain = Paint(view, Path.Combine(args[2], "embedding-" + width + ".png")))
                    {
                        int ink = 0;
                        for (int y = 0; y < plain.Height; y++)
                            for (int x = 0; x < plain.Width; x++)
                            { Color pixel = plain.GetPixel(x, y); if (pixel.R < 80 && pixel.G < 80 && pixel.B < 80) ink++; }
                        Check(ink > 100, "embedded rendering is nonblank");
                        int start = text.IndexOf("Lea", StringComparison.Ordinal);
                        view.Sel(start, start + 3);
                        Check(view.Sds() == "Lea", "embedded selection payload");
                        using (var selected = Paint(view, Path.Combine(args[2], "embedding-" + width + "-selected.png")))
                        {
                            int changed = 0, highlighted = 0;
                            for (int y = 0; y < selected.Height; y++)
                                for (int x = 0; x < selected.Width; x++)
                                {
                                    Color pixel = selected.GetPixel(x, y);
                                    if (pixel != plain.GetPixel(x, y)) changed++;
                                    if (Math.Abs(pixel.R - 183) <= 3 && Math.Abs(pixel.G - 217) <= 3 && Math.Abs(pixel.B - 244) <= 3) highlighted++;
                                }
                            Check(changed > 20, "selection changes embedded pixels");
                            Check(highlighted > 20, "embedded selection highlight");
                        }
                    }
                }
            }
            Console.WriteLine("PASS LEA SDK EMBEDDING " + checks); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
