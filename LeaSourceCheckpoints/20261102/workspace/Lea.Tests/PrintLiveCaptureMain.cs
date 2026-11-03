using System;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Lea.Print;
using Lea.Win;
using Forms = System.Windows.Forms;

internal static class PrintLiveCaptureMain
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
    {
        try
        {
            if (PrintStyledDocumentMain.Run(args) != 0) return 1;
            string directory = Path.GetDirectoryName(args[0]);
            var geometry = Pg.Cp(new Po(false, .5, 100, false, true, true, false), 850, 1100, new Pb(0, 0, 850, 1100));
            Reject(() => Pc.Cs(null, geometry), "live styled capture null view");
            using (var uncreated = new V())
            {
                Reject(() => Pc.Cs(uncreated, null), "live styled capture null geometry");
                Reject(() => uncreated.Pst(720, 900), "live styles uncreated view denied");
                Reject(() => Pc.Cs(uncreated, geometry), "live styled capture uncreated view denied");
            }
            Pc captured;
            using (var form = new Forms.Form { ClientSize = new System.Drawing.Size(640, 480) })
            using (var view = new V { Dock = Forms.DockStyle.Fill })
            {
                form.Controls.Add(view); form.Show();
                Reject(() => Pc.Cs(view, geometry), "live styled capture unloaded page denied");
                view.Go(Path.Combine(directory, "print-styled-document.html")); Forms.Application.DoEvents();
                var page = view.At; var size = view.ClientSize; var focus = form.ActiveControl;
                captured = Pc.Cs(view, geometry);
                Check(captured.Document.Styled && captured.StyleWidth == (int)Math.Floor(geometry.LayoutWidth) && captured.StyleHeight == (int)Math.Floor(geometry.LayoutHeight), "live styled capture uses print layout viewport");
                Check(Object.ReferenceEquals(page, view.At) && view.ClientSize == size && form.ActiveControl == focus, "live styled capture preserves page viewport and focus");
                Check(String.Concat(captured.Document.Nodes.Select(n => n.Text)).Contains("PRINT_VISIBLE") && !String.Concat(captured.Document.Nodes.Select(n => n.Text)).Contains("SCREEN_SECRET"), "live styled capture print media policy");
                Reject(() => Task.Run(() => view.Pst(720, 900)).GetAwaiter().GetResult(), "live styles cross thread denied without invoke");
                Reject(() => Task.Run(() => Pc.Cs(view, geometry)).GetAwaiter().GetResult(), "live styled capture cross thread denied without invoke");
                var pending = typeof(V).GetField("activationPending", BindingFlags.Instance | BindingFlags.NonPublic);
                int prior = (int)pending.GetValue(view); pending.SetValue(view, prior + 1);
                try
                {
                    Reject(() => view.Pst(720, 900), "live styles busy view denied");
                    Reject(() => Pc.Cs(view, geometry), "live styled capture busy view denied");
                }
                finally { pending.SetValue(view, prior); }
                Reject(() => view.Pst(9000, 900), "live styles viewport cap");
                view.Go(args[0]); Forms.Application.DoEvents();
                PrintCaptureProbe.Activate(view, "plain"); var editor = PrintCaptureProbe.Editor(view); editor.Select(6, 5);
                focus = form.ActiveControl;
                Pc selected = Pc.Cs(view, geometry);
                Check(selected.NativeSelection == "bravo" && editor.SelectionStart == 6 && editor.SelectionLength == 5 && form.ActiveControl == focus, "live styled capture preserves native field selection");
                PrintCaptureProbe.Activate(view, "password"); PrintCaptureProbe.Editor(view).SelectAll();
                Check(Pc.Cs(view, geometry).NativeSelection == "", "live styled capture password selection denied");
                Check(captured.Document.Title == "Lea Styled Print" && captured.Document.Nodes.Any(n => n.Text == "STYLED_VISIBLE"), "live styled capture detached after navigation");
                view.Dispose();
                Reject(() => view.Pst(720, 900), "live styles disposed view denied");
                Reject(() => Pc.Cs(view, geometry), "live styled capture disposed view denied");
            }
            using (var plan = Pw.Dc(captured.Document, geometry))
            {
                var pages = new Pp(plan, captured.Document.Title, captured.Document.Source);
                using (var preview = new Ph(pages))
                {
                    preview.Show(); preview.Update(); preview.Vw.UpdateLayout();
                    Check(preview.IsHandleCreated && preview.Vw.Count == pages.PageCount && plan.Styled, "live captured document opens WinForms preview after view disposal");
                    preview.Close();
                }
                using (var page = pages.GetPage(0))
                {
                    var image = new RenderTargetBitmap(816, 1056, 96, 96, PixelFormats.Pbgra32); image.Render(page.Visual);
                    var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(image));
                    using (var output = new FileStream(Path.Combine(directory, "live-styled-capture.png"), FileMode.CreateNew)) png.Save(output);
                }
                using (var output = new FileStream(Path.Combine(directory, "live-styled-capture.xps"), FileMode.CreateNew, FileAccess.ReadWrite))
                    Check(Px.Wr(pages, output) == pages.PageCount, "live captured document exports after view disposal");
            }
            using (var package = Package.Open(Path.Combine(directory, "live-styled-capture.xps"), FileMode.Open, FileAccess.Read))
            {
                string text = "";
                foreach (var part in package.GetParts().Where(p => p.ContentType == "application/vnd.ms-package.xps-fixedpage+xml"))
                    using (var stream = part.GetStream()) text += String.Concat(XDocument.Load(stream).Descendants().Attributes("UnicodeString").Select(a => a.Value));
                Check(text.Contains("STYLED_VISIBLE") && text.Contains("VISIBLE_OVERRIDE") && !text.Contains("SECRET"), "live captured XPS retains visible content and excludes fixture secrets");
            }
            Console.WriteLine("PASS Lea live styled capture and foundation"); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
