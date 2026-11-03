using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Lea.Print;

internal static class PrintHostMain
{
    private static void Check(bool value, string name) { if (!value) throw new Exception(name); Console.WriteLine("PASS " + name); }
    private static void Reject(Action action, string name)
    {
        bool denied = false;
        try { action(); } catch (ArgumentException) { denied = true; } catch (InvalidOperationException) { denied = true; }
        Check(denied, name);
    }
    [STAThread]
    private static int Main(string[] args)
    { return Run(args); }
    internal static int Run(string[] args)
    {
        try
        {
            if (PrintPreviewMain.Run(args) != 0) return 1;
            var geometry = Pg.Cp(new Po(false, .5, 100, true, true, true, true), 850, 400, new Pb(0, 0, 850, 400));
            using (var plan = Pw.Cp(String.Join("\n", Enumerable.Range(1, 36).Select(i => "Lea WinForms preview line " + i)), geometry))
            using (var owner = new Form { Text = "Lea Preview Owner Test", ClientSize = new System.Drawing.Size(500, 350) })
            {
                var pages = new Pp(plan, "Lea WinForms preview", "https://example.test/article");
                owner.Show();
                using (var form = new Ph(pages))
                {
                    var preview = form.Vw;
                    form.Show(owner); form.Update(); preview.UpdateLayout();
                    Check(form.IsHandleCreated && form.Visible && form.Owner == owner, "WinForms preview owned visible window");
                    var host = form.Controls.OfType<ElementHost>().Single();
                    Check(host.Child == preview && host.IsHandleCreated && host.Dock == DockStyle.Fill, "WinForms ElementHost child bridge");
                    Check(preview.Count > 1 && preview.Page == 0, "WinForms preview initial pagination");
                    preview.Pg(preview.Count - 1); Check(preview.Page == preview.Count - 1, "WinForms preview navigation");
                    preview.Pg(0); preview.Fit(); preview.UpdateLayout();
                    Check(preview.ActualWidth > 900 && preview.ActualHeight > 600, "WinForms preview fills host");
                    int width = (int)preview.ActualWidth, height = (int)preview.ActualHeight;
                    var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32); bitmap.Render(preview);
                    var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
                    using (var file = new FileStream(Path.Combine(Path.GetDirectoryName(args[0]), "preview-winforms.png"), FileMode.CreateNew)) png.Save(file);
                    Check(Task.Run(() => { try { var view = form.Vw; return false; } catch (InvalidOperationException) { return true; } }).Result, "WinForms preview thread affinity");
                    form.Close();
                    Check(form.IsDisposed && host.IsDisposed, "WinForms preview releases native controls");
                    Reject(() => preview.Pg(0), "WinForms close disposes WPF preview");
                    Reject(() => { var view = form.Vw; }, "WinForms disposed view accessor");
                    Check(!owner.IsDisposed && owner.Visible && pages.PageCount > 0, "WinForms close retains owner and plan");
                    using (var repeated = new Ph(pages)) { repeated.Show(owner); repeated.Close(); }
                    Check(pages.PageCount > 0, "WinForms preview can reopen same plan");
                }
                owner.Close();
                plan.Dispose(); Reject(() => new Ph(pages), "WinForms disposed plan denied");
            }
            Reject(() => new Ph(null), "WinForms null paginator denied");
            Console.WriteLine("PASS Lea WinForms preview and foundation"); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
