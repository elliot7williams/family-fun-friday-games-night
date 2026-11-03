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

internal static class PrintFriendlyLiveMain
{
    private static int checks;
    private static void Check(bool value, string name) { if (!value) throw new Exception(name); checks++; }
    private static void Reject(Action action, string name)
    {
        bool denied = false;
        try { action(); } catch (ArgumentException) { denied = true; } catch (InvalidOperationException) { denied = true; }
        Check(denied, name);
    }
    private static string Text(Ps snapshot) { return String.Concat(snapshot.Nodes.Select(n => n.Text)); }
    private static void Options(V view, string folder)
    {
        var variants = new[] {
            new Po(true, .35, 75, true, false, false, false),
            new Po(false, 1.25, 125, false, true, false, false),
            new Po(true, .5, 110, true, false, true, false)
        };
        for (int index = 0; index < variants.Length; index++)
        {
            var option = variants[index];
            double width = option.Landscape ? 1100 : 850, height = option.Landscape ? 850 : 1100;
            var geometry = Pg.Cp(option, width, height, new Pb(0, 0, width, height));
            var capture = Pc.Cf(view, geometry);
            Check(capture.StyleWidth == (int)Math.Floor(geometry.LayoutWidth), "option print viewport " + index);
            using (var plan = Pw.Dc(capture.Document, geometry))
            {
                var pages = new Pp(plan, "OPTION_HEADER", "");
                Check(Math.Abs(pages.PageSize.Width - width * .96) < .01 && Math.Abs(pages.PageSize.Height - height * .96) < .01, "option orientation " + index);
                string path = Path.Combine(folder, "friendly-option-" + index + ".xps");
                using (var file = new FileStream(path, FileMode.CreateNew)) Px.Wr(pages, file);
                using (var package = Package.Open(path, FileMode.Open, FileAccess.Read))
                {
                    var glyphs = package.GetParts().Where(p => p.Uri.OriginalString.EndsWith(".fpage", StringComparison.OrdinalIgnoreCase))
                        .SelectMany(p => { using (var input = p.GetStream()) return XDocument.Load(input).Descendants().Where(n => n.Name.LocalName == "Glyphs").ToArray(); }).ToArray();
                    string text = String.Concat(glyphs.Select(n => (string)n.Attribute("UnicodeString")));
                    Check(text.Contains("ARTICLE_BODY") && !text.Contains("NAV_CHROME"), "option filtered content " + index);
                    Check(text.Contains("OPTION_HEADER") == option.Header, "option header " + index);
                    Check(text.Contains("Firespider Internet") == option.Footer, "option footer " + index);
                    if (option.Grayscale)
                    {
                        var fills = glyphs.Select(n => (string)n.Attribute("Fill")).ToArray();
                        Check(fills.Length > 0 && fills.All(fill => fill != null && fill.Length == 9 && fill.Substring(3, 2) == fill.Substring(5, 2) && fill.Substring(5, 2) == fill.Substring(7, 2)), "option grayscale glyphs " + index);
                    }
                }
            }
        }
    }
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 2) throw new ArgumentException("Fixture and output directory required");
            if (PrintFriendlyMain.Run(new[] { args[1] }) != 0) return 1;
            var geometry = Pg.Cp(new Po(false, .75, 100, false, true, true, false), 850, 1100, new Pb(0, 0, 850, 1100));
            Reject(() => Pc.Cf(null, geometry), "null view");
            using (var uncreated = new V())
            {
                Reject(() => Pc.Cf(uncreated, null), "null geometry");
                Reject(() => Pc.Cf(uncreated, geometry), "uncreated view");
            }
            Pc captured;
            using (var form = new Forms.Form { ClientSize = new System.Drawing.Size(640, 480) })
            using (var view = new V { Dock = Forms.DockStyle.Fill })
            {
                form.Controls.Add(view); form.Show();
                Reject(() => Pc.Cf(view, geometry), "unloaded view");
                view.Go(args[0]); Forms.Application.DoEvents();
                var page = view.At; var size = view.ClientSize; var focus = form.ActiveControl;
                captured = Pc.Cf(view, geometry);
#if FRIENDLY_HTML
                checks += PrintHtmlChecks.Run(captured.Document, args[1]);
#endif
                Check(captured.Document.Styled && captured.StyleWidth == (int)Math.Floor(geometry.LayoutWidth), "styled print viewport");
                Check(Object.ReferenceEquals(page, view.At) && size == view.ClientSize && focus == form.ActiveControl, "live page/viewport/focus unchanged");
                string text = Text(captured.Document);
                Check(text.Contains("ARTICLE_BODY") && text.Contains("NEAR_CLASS_CONTENT") && text.Contains("REFERENCE_CONTENT"), "article retained");
                foreach (string chrome in new[] { "NAV_CHROME", "ASIDE_CHROME", "AD_CHROME", "ROLE_CHROME", "FORM_CHROME", "FOOTER_CHROME" })
                    Check(!text.Contains(chrome), "excluded " + chrome);
                Check(text.Contains("PRINT_ONLY_CONTENT") && !text.Contains("SCREEN_ONLY_SECRET"), "print media preserved");
                Check(Text(Pc.Cs(view, geometry).Document).Contains("NAV_CHROME"), "ordinary capture unaffected");
                Options(view, args[1]);
                Reject(() => Task.Run(() => Pc.Cf(view, geometry)).GetAwaiter().GetResult(), "cross-thread capture");
                var pending = typeof(V).GetField("activationPending", BindingFlags.Instance | BindingFlags.NonPublic);
                int prior = (int)pending.GetValue(view); pending.SetValue(view, prior + 1);
                try { Reject(() => Pc.Cf(view, geometry), "busy capture"); } finally { pending.SetValue(view, prior); }
                PrintCaptureProbe.Activate(view, "plain"); var editor = PrintCaptureProbe.Editor(view); editor.Select(6, 5);
                focus = form.ActiveControl;
                Check(Pc.Cf(view, geometry).NativeSelection == "bravo" && editor.SelectionStart == 6 && editor.SelectionLength == 5 && focus == form.ActiveControl, "field selection preserved");
                PrintCaptureProbe.Activate(view, "password"); PrintCaptureProbe.Editor(view).SelectAll();
                Check(Pc.Cf(view, geometry).NativeSelection == "", "password selection suppressed");
                view.Dispose();
                Reject(() => Pc.Cf(view, geometry), "disposed capture");
            }
            using (var plan = Pw.Dc(captured.Document, geometry))
            {
                var pages = new Pp(plan, captured.Document.Title, "");
                using (var preview = new Ph(pages))
                {
                    preview.Show(); preview.Update(); preview.Vw.UpdateLayout();
                    Check(preview.IsHandleCreated && preview.Vw.Count == pages.PageCount, "detached preview opens"); preview.Close();
                }
                using (var page = pages.GetPage(0))
                {
                    var image = new RenderTargetBitmap(816, 1056, 96, 96, PixelFormats.Pbgra32); image.Render(page.Visual);
                    var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(image));
                    using (var file = new FileStream(Path.Combine(args[1], "friendly-live.png"), FileMode.CreateNew)) png.Save(file);
                }
                string path = Path.Combine(args[1], "friendly-live.xps");
                using (var file = new FileStream(path, FileMode.CreateNew)) Px.Wr(pages, file);
                using (var package = Package.Open(path, FileMode.Open, FileAccess.Read))
                {
                    string glyphs = "";
                    foreach (var part in package.GetParts().Where(p => p.Uri.OriginalString.EndsWith(".fpage", StringComparison.OrdinalIgnoreCase)))
                        using (var input = part.GetStream()) glyphs += String.Concat(XDocument.Load(input).Descendants().Attributes("UnicodeString").Select(a => a.Value));
                    Check(glyphs.Contains("ARTICLE_BODY") && glyphs.Contains("PRINT_ONLY_CONTENT"), "detached XPS content");
                    Check(!glyphs.Contains("NAV_CHROME") && !glyphs.Contains("SCREEN_ONLY_SECRET") && !glyphs.Contains("secret-password"), "XPS chrome/privacy omissions");
                }
            }
            string json = "{\"passed\":true,\"liveChecks\":" + checks + ",\"printJobSubmitted\":false,\"hostRouted\":false}";
            using (var file = new FileStream(Path.Combine(args[1], "live.json"), FileMode.CreateNew))
            using (var writer = new StreamWriter(file)) writer.Write(json);
            Console.WriteLine(json); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
