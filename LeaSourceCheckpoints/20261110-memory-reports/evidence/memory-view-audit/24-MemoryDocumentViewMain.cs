using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Win;

internal static class MemoryDocumentViewMain
{
    private static int checks;
    private static void Check(bool ok, string name) { if (!ok) throw new Exception(name); checks++; }
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            var output = Path.GetFullPath(args[0]);
            var temp = Path.Combine(output, "private-temp");
            Directory.CreateDirectory(temp);
            Environment.SetEnvironmentVariable("TEMP", temp);
            Environment.SetEnvironmentVariable("TMP", temp);
            foreach (var width in new[] { 720, 360 })
            {
                using (var form = new Form { ClientSize = new Size(width, 480) })
                using (var view = new V { Dock = DockStyle.Fill, Pv = true })
                {
                    form.Controls.Add(view); form.Show(); Application.DoEvents();
                    var done = 0;
                    view.Done += delegate { done++; };
                    var first = view.Ht("<title>Memory report</title><style>body{color:black;background:white}h1{color:red}</style><h1>Accessibility Inspector</h1><p>Private memory report</p>");
                    Application.DoEvents();
                    Check(view.At == first && view.T == "Memory report" && first.X.Contains("Private memory report"), "shown memory content");
                    Check(done == 1 && !view.Busy, "completion and input retirement");
                    Check(view.Pv && first.Og.Op && !first.Og.Lc && !view.U.IsFile, "private opaque nonlocal view");
                    using (var bitmap = new Bitmap(width, 480))
                    {
                        view.DrawToBitmap(bitmap, new Rectangle(0, 0, width, 480));
                        var ink = 0;
                        for (var y = 0; y < bitmap.Height; y += 2)
                            for (var x = 0; x < bitmap.Width; x += 2)
                            { var pixel = bitmap.GetPixel(x, y); if (pixel.R < 180 || pixel.G < 180 || pixel.B < 180) ink++; }
                        Check(ink > 50, "nonblank report pixels");
                        bitmap.Save(Path.Combine(output, "memory-view-" + width + ".png"));
                    }
                    var second = view.Ht("<title>Second</title><p>Second memory report</p>");
                    Check(view.Bk() == first && view.Fw() == second, "view history");
                    Check(view.Rl().T == "Second" && !view.Busy, "view reload");
                    var retained = view.At;
                    view.Ng = uri => false;
                    var blocked = false;
                    try { view.Ht("<p>Blocked</p>"); } catch (Er error) { blocked = error.K == "blocked"; }
                    Check(blocked && view.At == retained && !view.Busy, "view navigation denial");
                    view.Ng = null;
                    view.Dispose();
                    var disposed = false;
                    try { view.Ht("<p>Disposed</p>"); } catch (ObjectDisposedException) { disposed = true; }
                    Check(disposed, "disposed view rejects memory load");
                }
            }
            Check(Directory.GetFiles(temp, "*.html", SearchOption.AllDirectories).Length == 0, "no HTML report in isolated TEMP");
            Console.WriteLine("PASS Lea memory view: " + checks + " checks at720/360; no HTML in isolated TEMP");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
