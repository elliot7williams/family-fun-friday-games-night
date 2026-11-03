using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lea.Script.Win;

namespace Lea.Embed.Sample
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            var smoke = args.Length > 0 && args[0] == "--smoke";
            var root = AppDomain.CurrentDomain.BaseDirectory;
            var page = smoke || args.Length == 0 ? Path.Combine(root, "sample-page.html") : args[0];
            var exit = 0;
            using (var form = new BrowserForm())
            {
                form.Shown += async (s, e) =>
                {
                    try
                    {
                        var work = smoke ? Smoke(form, root, args[1]) : form.Go(page);
                        if (smoke && await Task.WhenAny(work, Task.Delay(20000)) != work)
                            throw new TimeoutException("Embedding smoke deadline exceeded.");
                        await work;
                    }
                    catch (Exception error) { exit = 1; Console.Error.WriteLine(error); }
                    finally { if (smoke) form.Close(); }
                };
                Application.Run(form);
            }
            Console.WriteLine("Embedding window disposed; exit=" + exit);
            return exit;
        }

        private static async Task Smoke(BrowserForm form, string root, string screenshot)
        {
            await form.Go(Path.Combine(root, "sample-page.html"));
            Check(form.Vw.T == "Lea Embed" && form.Vw.At.N > 5, "local HTML and CSS loaded");
            var adapter = form.Vw.Xs as W;
            Check(adapter != null && adapter.At.Pid == 0, "script adapter attached without eager worker startup");
            await form.Go(Path.Combine(root, "second-page.html"));
            Check(form.Vw.T == "Second Page" && form.Vw.CanBk, "second navigation and back availability");
            form.Vw.Bk();
            Check(form.Vw.T == "Lea Embed" && form.Vw.CanFw, "back restores first document");
            form.Vw.Fw();
            Check(form.Vw.T == "Second Page", "forward restores second document");
            await form.Vw.RlA();
            Check(form.Vw.T == "Second Page", "reload completes");
            await form.Go(Path.Combine(root, "sample-page.html"));
            form.Refresh();
            using (var bitmap = new Bitmap(form.Width, form.Height))
            {
                form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
                bitmap.Save(screenshot, System.Drawing.Imaging.ImageFormat.Png);
            }
            Check(form.Vw.Xo != null && form.Vw.Xo.Ok, "view lifecycle completed");
            Console.WriteLine("PASS Lea embedding smoke: 7 checks; screenshot=" + screenshot);
        }

        private static void Check(bool ok, string name)
        {
            if (!ok) throw new InvalidOperationException("FAIL " + name);
            Console.WriteLine("PASS " + name);
        }
    }
}
