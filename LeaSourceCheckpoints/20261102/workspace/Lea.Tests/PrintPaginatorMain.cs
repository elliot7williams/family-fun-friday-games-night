using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using Lea.Print;

internal static class PrintPaginatorMain
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
            if (PrintFrameMain.Run(args) != 0) return 1;
            var geometry = Pg.Cp(new Po(false, .5, 100, true, true, true, true), 850, 400, new Pb(0, 0, 850, 400));
            using (var plan = Pw.Cp(String.Join("\n", Enumerable.Range(1, 120).Select(i => "Lea paginator line " + i)), geometry))
            {
                var pages = new Pp(plan, "Paginator test", "https://example.test/");
                Check(pages.IsPageCountValid && pages.PageCount == plan.PageText.Count && pages.PageCount > 1, "paginator complete multi-page count");
                Check(pages.PageSize == new Size(816, 384), "paginator oriented paper size");
                pages.PageSize = pages.PageSize;
                Check(((IDocumentPaginatorSource)pages.Source).DocumentPaginator == pages, "paginator source identity");
                Reject(() => pages.PageSize = new Size(384, 816), "paginator refuses implicit repagination");
                Reject(() => pages.PageSize = new Size(0, 0), "paginator refuses empty paper");
                Reject(() => pages.GetPage(-1), "paginator negative page");
                Check(pages.GetPage(pages.PageCount) == DocumentPage.Missing, "paginator missing sentinel");
                Check(pages.GetPage(Int32.MaxValue) == DocumentPage.Missing, "paginator huge index sentinel");
                for (int i = 0; i < pages.PageCount; i++)
                    using (var page = pages.GetPage(i))
                        Check(page.Visual != null && page.Size == pages.PageSize && page.ContentBox == new Rect(pages.PageSize), "paginator page geometry " + i);
                using (var first = pages.GetPage(0))
                using (var repeated = pages.GetPage(0))
                    Check(!Object.ReferenceEquals(first.Visual, repeated.Visual), "paginator independent page wrappers");
                Check(Task.Run(() => { try { var count = pages.PageCount; return false; } catch (InvalidOperationException) { return true; } }).Result, "paginator thread affinity");
                plan.Dispose();
                Reject(() => pages.GetPage(0), "paginator disposed plan page");
                Reject(() => { var count = pages.PageCount; }, "paginator disposed plan metadata");
                Reject(() => new Pp(plan, "", ""), "paginator disposed construction");
            }
            Reject(() => new Pp(null, "", ""), "paginator null plan");
            Console.WriteLine("PASS Lea paginator and foundation");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
