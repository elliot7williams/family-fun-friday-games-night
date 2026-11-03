using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Lea.Print;

internal static class PrintHtmlChecks
{
    internal static int Run(Ps snapshot, string folder)
    {
        int count = 0;
#if DOCUMENT_SELECTION
        count += DocumentSelectionChecks.Run(folder);
#endif
#if FRIENDLY_IMAGES
        count += PrintImageChecks.Run(folder);
#endif
#if FRIENDLY_CACHE
        count += PrintCacheImageChecks.Run(folder);
#endif
#if FRIENDLY_DECODER
        count += PrintDecoderRoundTrip.Run(folder);
#endif
        Action<bool, string> check = (value, name) => { if (!value) throw new Exception(name); count++; };
        Action<Action, string> reject = (action, name) => {
            bool denied = false;
            try { action(); } catch (ArgumentException) { denied = true; } catch (InvalidOperationException) { denied = true; }
            check(denied, name);
        };
        var options = new Po(true, .35, 125, true, true, true, false);
        string html = Ht.Cp(snapshot, options, null);
        var document = XDocument.Parse(html);
        var body = document.Root.Element("body");
        check(document.Root.Name.LocalName == "html" && body != null, "HTML structure");
        check(body.Value.Contains("ARTICLE_BODY") && !body.Value.Contains("NAV_CHROME") && !body.Value.Contains("secret-password"), "HTML snapshot content/privacy");
        check(body.Value.Contains("PRINT_ONLY_CONTENT") && !body.Value.Contains("SCREEN_ONLY_SECRET"), "HTML print-media content");
        check(document.Descendants("script").Count() == 0 && document.Descendants("base").Count() == 0, "no active script/base");
        check(!document.Descendants().Attributes().Any(a => a.Name.LocalName.StartsWith("on", StringComparison.OrdinalIgnoreCase) || a.Name.LocalName == "src" || a.Name.LocalName == "srcset"), "no event/resource attributes");
        check(document.Descendants("meta").Any(n => (string)n.Attribute("http-equiv") == "Content-Security-Policy"), "deny resource policy");
        check(document.Descendants("a").Any(n => (string)n.Attribute("href") == "https://example.test/reference"), "safe reference link");
        check(!body.Value.Contains("file:///"), "local path not in header");
        string css = document.Descendants("style").Single().Value;
        check(css.Contains("size:landscape") && css.Contains("margin:0.35in") && css.Contains("zoom:1.25") && css.Contains("grayscale(100%)"), "HTML options");
        check(body.Element("header") != null && body.Element("footer") != null, "HTML metadata bands");
        check(body.Element("footer").Value == "Firespider Internet" && css.Contains(".page-number{display:none}"), "no unpaginated screen page number");
        string selected = "<script>alert('x')</script> & selected\nsecond line";
        var selection = new Po(false, .75, 100, false, false, false, true);
        var selectedDocument = XDocument.Parse(Ht.Cp(snapshot, selection, selected));
        check(selectedDocument.Descendants("pre").Single().Value == selected, "selection escaping/line preservation");
        check(!selectedDocument.Descendants("script").Any() && !selectedDocument.Root.Element("body").Value.Contains("ARTICLE_BODY"), "selection excludes document");
        check(!selectedDocument.Descendants("header").Any() && !selectedDocument.Descendants("footer").Any(), "HTML metadata off");
        reject(() => Ht.Cp(null, options, null), "null HTML snapshot");
        reject(() => Ht.Cp(snapshot, null, null), "null HTML options");
        reject(() => Ht.Cp(snapshot, selection, " \n"), "empty selection");
        reject(() => Ht.Cp(snapshot, selection, new string('x', Ps.MaxCharacters + 1)), "selection input bound");
        reject(() => Ht.Cp(snapshot, selection, new string('&', Ps.MaxCharacters)), "escaped output bound");
        using (var file = new FileStream(Path.Combine(folder, "friendly-copy.html"), FileMode.CreateNew))
        using (var writer = new StreamWriter(file)) writer.Write(html);
        using (var form = new System.Windows.Forms.Form { ClientSize = new System.Drawing.Size(800, 600) })
        using (var view = new Lea.Win.V { Dock = System.Windows.Forms.DockStyle.Fill })
        {
            form.Controls.Add(view); form.Show();
            view.Go(Path.Combine(folder, "friendly-copy.html")); System.Windows.Forms.Application.DoEvents();
            var geometry = Pg.Cp(options, 1100, 850, new Pb(0, 0, 1100, 850));
            string roundTrip = String.Concat(Pc.Cs(view, geometry).Document.Nodes.Select(n => n.Text));
            check(roundTrip.Contains("ARTICLE_BODY") && roundTrip.Contains("PRINT_ONLY_CONTENT"), "HTML reopened by Lea");
            check(!roundTrip.Contains("NAV_CHROME") && !roundTrip.Contains("SCREEN_ONLY_SECRET") && !roundTrip.Contains("secret-password"), "HTML reopen privacy");
            using (var image = new System.Drawing.Bitmap(view.Width, view.Height))
            {
                view.DrawToBitmap(image, view.ClientRectangle);
                image.Save(Path.Combine(folder, "friendly-copy.png"), System.Drawing.Imaging.ImageFormat.Png);
            }
        }
        return count;
    }
}
