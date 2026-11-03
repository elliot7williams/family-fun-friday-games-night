using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Lea.Engine;
using Lea.Print;

internal static class PrintStyledDocumentMain
{
    private static readonly string[] Secrets = { "DISPLAY_SECRET", "DISPLAY_CHILD_SECRET", "VISIBILITY_SECRET", "DESCENDANT_SECRET", "OPACITY_SECRET", "OPACITY_CHILD_SECRET", "SCREEN_SECRET", "CLOSED_SECRET" };
    private static void Check(bool value, string name) { if (!value) throw new Exception(name); Console.WriteLine("PASS " + name); }
    private static void Reject(Action action, string name)
    {
        bool denied = false;
        try { action(); } catch (ArgumentException) { denied = true; } catch (InvalidOperationException) { denied = true; }
        Check(denied, name);
    }
    private static IEnumerable<Run> Runs(FlowDocument flow)
    {
        for (var at = flow.ContentStart; at != null && at.CompareTo(flow.ContentEnd) < 0; at = at.GetNextContextPosition(LogicalDirection.Forward))
            if (at.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart && at.GetAdjacentElement(LogicalDirection.Forward) is Run)
                yield return (Run)at.GetAdjacentElement(LogicalDirection.Forward);
    }
    private static void Image(Pp pages, string path, bool gray)
    {
        using (var page = pages.GetPage(0))
        {
            var bitmap = new RenderTargetBitmap(816, 1056, 96, 96, PixelFormats.Pbgra32); bitmap.Render(page.Visual);
            var pixels = new byte[816 * 1056 * 4]; bitmap.CopyPixels(pixels, 816 * 4, 0); int colored = 0, ink = 0;
            for (int y = 77; y < 980; y++) for (int x = 48; x < 768; x++)
            {
                int i = (y * 816 + x) * 4;
                if (pixels[i] < 200 || pixels[i + 1] < 200 || pixels[i + 2] < 200) ink++;
                if (Math.Max(pixels[i], Math.Max(pixels[i + 1], pixels[i + 2])) - Math.Min(pixels[i], Math.Min(pixels[i + 1], pixels[i + 2])) > 5) colored++;
            }
            var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap)); using (var file = new FileStream(path, FileMode.CreateNew)) png.Save(file);
            Check(ink > 1000, "styled document body pixels");
            Check(gray ? colored == 0 : colored > 100, "styled document grayscale flag pixels");
        }
    }
    [STAThread]
    private static int Main(string[] args)
    { return Run(args); }
    internal static int Run(string[] args)
    {
        try
        {
            if (PrintStylesMain.Run(args) != 0) return 1;
            string directory = Path.GetDirectoryName(args[0]); var engine = new E { Je = false };
            var page = engine.Go(Path.Combine(directory, "print-styled-document.html"));
            var styles = engine.Pst(page, 720, 900);
            Ps snapshot = Ps.Cs(page, styles);
            string text = String.Concat(snapshot.Nodes.Select(n => n.Text));
            Check(snapshot.Styled && snapshot.Nodes.All(n => n.Style != null), "styled snapshot complete detached style records");
            Check(Secrets.All(secret => !text.Contains(secret)), "styled snapshot excludes all secret text");
            Check(text.Contains("VISIBLE_OVERRIDE") && text.Contains("PRINT_VISIBLE"), "styled snapshot visible override and print-only content");
            var recorded = snapshot.Nodes.Single(n => n.Text == "STYLED_VISIBLE").Style;
            S mutable = styles.Single(pair => pair.Key.K == "#text" && pair.Key.X == "STYLED_VISIBLE").Value;
            typeof(S).GetProperty("Fs").SetValue(mutable, 99, null);
            Check(recorded.FontSize == 24 && recorded.Bold && recorded.Italic, "styled snapshot detached from mutable query styles");
            Check(typeof(Py).GetProperties().All(p => p.GetSetMethod(true).IsPrivate), "styled snapshot scalar properties readonly");
            var flow = Pm.Cp(snapshot); var runs = Runs(flow).ToArray();
            var styled = runs.Single(r => r.Text.Contains("STYLED_VISIBLE"));
            Check(styled.FontSize == 24 && styled.FontWeight == FontWeights.Bold && styled.FontStyle == FontStyles.Italic && styled.FontFamily.Source == "Times New Roman", "styled renderer typography and safe family");
            Check(((SolidColorBrush)styled.Foreground).Color == System.Windows.Media.Color.FromRgb(204, 34, 68) && styled.TextDecorations.Count > 0, "styled renderer color and decoration");
            Check(((SolidColorBrush)((Section)((Paragraph)styled.Parent).Parent).Background).Color == System.Windows.Media.Color.FromRgb(224, 244, 255), "styled renderer background");
            var alpha = runs.Single(r => r.Text.Contains("ALPHA_VISIBLE"));
            Check(((SolidColorBrush)alpha.Foreground).Color.A == 64, "styled renderer alpha and inherited opacity");
            Check(runs.Single(r => r.Text.Contains("LOCAL_FONT_FALLBACK")).FontFamily.Source == "Consolas", "styled renderer rejects URI font families and maps local fallback");
            Check(runs.Single(r => r.Text.Contains("SAFE_COLOR_FALLBACK")).Foreground is SolidColorBrush, "styled renderer rejects color-profile URI syntax");
            string rendered = new TextRange(flow.ContentStart, flow.ContentEnd).Text;
            Check(rendered.Replace("\r", "").Contains("  CSS space\n    CSS indented") && rendered.Contains("INLINE_A INLINE_B"), "styled renderer CSS whitespace and inline display");
            Check(Secrets.All(secret => !rendered.Contains(secret)), "styled renderer secret text absent");
            foreach (bool gray in new[] { false, true })
            {
                var geometry = Pg.Cp(new Po(false, .5, 100, gray, true, true, false), 850, 1100, new Pb(0, 0, 850, 1100));
                using (var plan = Pw.Dc(snapshot, geometry))
                {
                    Check(plan.Styled && plan.Structured, "styled plan provenance");
                    var pages = new Pp(plan, snapshot.Title, snapshot.Source);
                    Image(pages, Path.Combine(directory, gray ? "styled-gray.png" : "styled-color.png"), gray);
                    if (!gray) using (var file = new FileStream(Path.Combine(directory, "styled.xps"), FileMode.CreateNew, FileAccess.ReadWrite)) Px.Wr(pages, file);
                }
            }
            using (var package = Package.Open(Path.Combine(directory, "styled.xps"), FileMode.Open, FileAccess.Read))
            {
                var glyphs = new List<string>();
                foreach (var part in package.GetParts().Where(p => p.ContentType == "application/vnd.ms-package.xps-fixedpage+xml"))
                    using (var stream = part.GetStream()) glyphs.AddRange(XDocument.Load(stream).Descendants().Attributes("UnicodeString").Select(a => a.Value));
                string saved = String.Concat(glyphs);
                Check(Secrets.All(secret => !saved.Contains(secret)) && saved.Contains("VISIBLE_OVERRIDE") && saved.Contains("PRINT_VISIBLE") && saved.Contains("Final styled marker."), "styled XPS hidden text excluded and visible text retained");
            }
            Reject(() => Ps.Cs(page, null), "styled snapshot null map");
            var incomplete = styles.ToDictionary(pair => pair.Key, pair => pair.Value); incomplete.Remove(page.D);
            Reject(() => Ps.Cs(page, incomplete), "styled snapshot missing node style");
            var foreign = new E { Je = false }; var foreignPage = foreign.Go(Path.Combine(directory, "print-styled-document.html"));
            Reject(() => Ps.Cs(page, foreign.Pst(foreignPage, 720, 900)), "styled snapshot foreign map");
            Check(!Ps.Cp(page).Styled, "unstyled snapshot compatibility explicit");
            Console.WriteLine("PASS Lea styled document and foundation"); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
