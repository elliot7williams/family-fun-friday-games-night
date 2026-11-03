using System;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Xml.Linq;
using Lea.Print;

internal static class PrintRange
{
    private static int checks;
    private static void Check(bool condition, string name)
    { if (!condition) throw new Exception(name); checks++; Console.WriteLine("PASS " + name); }
    private static void Reject(Action action, string name)
    {
        bool denied = false;
        try { action(); } catch (ArgumentException) { denied = true; }
        catch (InvalidOperationException) { denied = true; }
        Check(denied, name);
    }
    [STAThread]
    private static int Main()
    {
        try
        {
            PrintDevice.Run();
            PrintDialogChecks.Run();
            var geometry = Pg.Cp(new Po(false, .5, 100, false, true, true, false), 850, 400, new Pb(0, 0, 850, 400));
            string text = String.Join("\n", Enumerable.Range(1, 120).Select(i => "Range body line " + i));
            using (var plan = Pw.Cp(text, geometry))
            {
                var pages = new Pp(plan, "Lea range test", "https://user:password@example.test/article?token=secret#private");
                Check(pages.PageCount >= 4, "range fixture has at least four pages");
                var range = new Pr(pages, 2, 3);
                Check(range.PageCount == 2 && range.IsPageCountValid, "inclusive middle range count");
                Check(range.Source == range && ((IDocumentPaginatorSource)range).DocumentPaginator == range, "range source identity");
                Check(range.PageSize == pages.PageSize, "range paper preserved");
                range.PageSize = pages.PageSize;
                Reject(() => range.PageSize = new Size(384, 816), "range geometry mutation denied");
                Reject(() => range.GetPage(-1), "range negative page denied");
                Check(range.GetPage(2) == DocumentPage.Missing && range.GetPage(Int32.MaxValue) == DocumentPage.Missing, "range missing sentinels");
                using (var first = range.GetPage(0))
                using (var repeated = range.GetPage(0))
                    Check(first.Visual != null && !Object.ReferenceEquals(first.Visual, repeated.Visual), "range fresh wrappers");
                Check(new Pr(pages, 1, pages.PageCount).PageCount == pages.PageCount, "full range accepted");
                Check(new Pr(pages, pages.PageCount, pages.PageCount).PageCount == 1, "last single page accepted");
                Reject(() => new Pr(null, 1, 1), "range null source denied");
                Reject(() => new Pr(pages, 0, 1), "range zero first denied");
                Reject(() => new Pr(pages, -1, 1), "range negative first denied");
                Reject(() => new Pr(pages, 3, 2), "range reversed denied");
                Reject(() => new Pr(pages, 1, pages.PageCount + 1), "range outside source denied");
                Reject(() => new Pr(pages, Int32.MinValue, Int32.MaxValue), "range arithmetic extremes denied");
                Check(Task.Run(() => { try { var n = range.PageCount; return false; } catch (InvalidOperationException) { return true; } }).Result, "range thread affinity");
                using (var memory = new MemoryStream())
                {
                    Check(Px.Rg(range, memory) == 2 && memory.CanRead && memory.CanWrite, "range export count and borrowed stream");
                    byte[] bytes = memory.ToArray();
                    Reject(() => Px.Rg(range, memory), "range nonempty export denied");
                    Check(bytes.SequenceEqual(memory.ToArray()), "range rejected destination unchanged");
                    using (var readOnly = new MemoryStream(new byte[0], false)) Reject(() => Px.Rg(range, readOnly), "range readonly destination denied");
                    Reject(() => Px.Rg(range, null), "range null destination denied");
                    Reject(() => Px.Rg(null, memory), "range null export denied");
                    memory.Position = 0;
                    using (var package = Package.Open(memory, FileMode.Open, FileAccess.Read))
                    {
                        var parts = package.GetParts().Where(p => p.ContentType == "application/vnd.ms-package.xps-fixedpage+xml")
                            .OrderBy(p => Int32.Parse(Path.GetFileNameWithoutExtension(p.Uri.OriginalString))).ToArray();
                        Check(parts.Length == 2, "range stored page count");
                        for (int i = 0; i < parts.Length; i++)
                        {
                            XDocument xml; using (var stream = parts[i].GetStream()) xml = XDocument.Load(stream);
                            string glyphs = String.Concat(xml.Descendants().Attributes("UnicodeString").Select(a => a.Value));
                            Check(glyphs.Contains("Page " + (i + 2) + " of " + pages.PageCount), "range retains original footer " + i);
                            string firstLine = plan.PageText[i + 1].Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0];
                            Check(glyphs.Contains(firstLine), "range retains selected body " + i);
                            Check(!glyphs.Contains("password") && !glyphs.Contains("token=") && !glyphs.Contains("#private"), "range address privacy " + i);
                        }
                    }
                    using (var output = new FileStream("range.xps", FileMode.CreateNew, FileAccess.Write)) output.Write(bytes, 0, bytes.Length);
                }
                plan.Dispose();
                Reject(() => { var n = range.PageCount; }, "disposed range count denied");
                Reject(() => range.GetPage(0), "disposed range page denied");
                Reject(() => { var size = range.PageSize; }, "disposed range size denied");
                Reject(() => { var source = range.Source; }, "disposed range source denied");
                Reject(() => new Pr(pages, 1, 1), "disposed range construction denied");
                using (var empty = new MemoryStream())
                {
                    Reject(() => Px.Rg(range, empty), "disposed range export denied");
                    Check(empty.Length == 0, "disposed range destination untouched");
                }
            }
            Console.WriteLine("PASS Lea print range: " + checks); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
