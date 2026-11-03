using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
using System.Xml.Linq;
using Lea.Engine;
using Lea.Print;
using Lea.Win;
using Forms = System.Windows.Forms;

internal static class PrintDocumentMain
{
    private static void Check(bool value, string name) { if (!value) throw new Exception(name); Console.WriteLine("PASS " + name); }
    private static void Reject(Action action, string name)
    {
        bool denied = false;
        try { action(); } catch (ArgumentException) { denied = true; } catch (InvalidOperationException) { denied = true; }
        Check(denied, name);
    }
    private static IEnumerable<Block> Blocks(BlockCollection collection)
    {
        foreach (Block block in collection)
        {
            yield return block;
            IEnumerable<Block> nested = new Block[0];
            if (block is Section) nested = Blocks(((Section)block).Blocks);
            if (block is System.Windows.Documents.List) nested = ((System.Windows.Documents.List)block).ListItems.Cast<ListItem>().SelectMany(item => Blocks(item.Blocks));
            if (block is Table) nested = ((Table)block).RowGroups.Cast<TableRowGroup>().SelectMany(g => g.Rows.Cast<TableRow>()).SelectMany(r => r.Cells.Cast<TableCell>()).SelectMany(c => Blocks(c.Blocks));
            foreach (Block child in nested) yield return child;
        }
    }
    private static IEnumerable<Inline> Inlines(InlineCollection collection)
    {
        foreach (Inline inline in collection) { yield return inline; if (inline is Span) foreach (Inline nested in Inlines(((Span)inline).Inlines)) yield return nested; }
    }
    private static N Node(string kind, string text = "")
    {
        var node = (N)Activator.CreateInstance(typeof(N), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { kind }, null);
        typeof(N).GetProperty("X").SetValue(node, text, null); return node;
    }
    private static Ps Snapshot(N node)
    {
        var page = (P)Activator.CreateInstance(typeof(P), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new object[] { new Uri("https://example.test/") }, null);
        typeof(P).GetProperty("D").SetValue(page, node, null); return Ps.Cp(page);
    }
    private static void Image(DocumentPage page, Size size, Pb body, string path)
    {
        var element = page.Visual as UIElement;
        if (element != null) { element.Measure(size); element.Arrange(new Rect(size)); element.UpdateLayout(); }
        var bitmap = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32); bitmap.Render(page.Visual);
        var pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4]; bitmap.CopyPixels(pixels, bitmap.PixelWidth * 4, 0);
        int ink = 0;
        for (int y = (int)(body.Y * .96); y < (body.Y + body.Height) * .96; y++)
            for (int x = (int)(body.X * .96); x < (body.X + body.Width) * .96; x++)
                if (pixels[(y * bitmap.PixelWidth + x) * 4 + 3] > 0 && pixels[(y * bitmap.PixelWidth + x) * 4] < 200) ink++;
        var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
        using (var output = new FileStream(path, FileMode.CreateNew)) png.Save(output);
        Check(ink > 1000, "document body visible " + Path.GetFileName(path));
    }
    private static void Image(Pp pages, int index, Pb body, string path)
    {
        using (var page = pages.GetPage(index))
        {
            Image(page, pages.PageSize, body, path);
        }
    }
    [STAThread]
    private static int Main(string[] args)
    { return Run(args); }
    internal static int Run(string[] args)
    {
        try
        {
            if (PrintHostMain.Run(args) != 0) return 1;
            string directory = Path.GetDirectoryName(args[0]); Ps snapshot;
            using (var form = new Forms.Form())
            using (var view = new V { Dock = Forms.DockStyle.Fill })
            {
                form.Controls.Add(view); form.Show(); view.Go(Path.Combine(directory, "print-document.html")); Forms.Application.DoEvents();
                snapshot = Pc.Cp(view).Document;
                Check(snapshot.Title == "Lea structured print", "document captured from actual native view");
            }
            FlowDocument flow = Pm.Cp(snapshot);
            var blocks = Blocks(flow.Blocks).ToArray();
            var inlines = blocks.OfType<Paragraph>().SelectMany(p => Inlines(p.Inlines)).ToArray();
            string normalized = new TextRange(flow.ContentStart, flow.ContentEnd).Text;
            Check(blocks.OfType<Section>().Any(s => s.FontSize == 32 && s.FontWeight == FontWeights.Bold), "document heading typography");
            Check(inlines.OfType<Span>().Any(s => s.FontWeight == FontWeights.Bold) && inlines.OfType<Span>().Any(s => s.FontStyle == FontStyles.Italic), "document inline emphasis");
            Check(!inlines.OfType<Hyperlink>().Any(), "document links have no activation object");
            Check(blocks.OfType<System.Windows.Documents.List>().Any(l => l.MarkerStyle == TextMarkerStyle.Decimal) && blocks.OfType<System.Windows.Documents.List>().Any(l => l.MarkerStyle == TextMarkerStyle.Disc), "document ordered and nested bullet lists");
            Table table = blocks.OfType<Table>().Single();
            var cells = table.RowGroups.Cast<TableRowGroup>().SelectMany(g => g.Rows.Cast<TableRow>()).SelectMany(r => r.Cells.Cast<TableCell>()).ToArray();
            Check(table.Columns.Count == 3 && cells.Any(c => c.ColumnSpan == 2) && cells.Any(c => c.RowSpan == 2), "document table cells and spans");
            Check(normalized.Contains("Alpha beta gamma") && normalized.Contains("After break\u00a0kept."), "document whitespace and nonbreaking space");
            Check(normalized.Replace("\r", "").Contains("  first\n    second"), "document preformatted whitespace");
            Check(normalized.Contains("<Button/> & text") && normalized.Contains("[Image: Lea logo]"), "document literal markup and explicit image placeholder");
            Check(!normalized.Contains("HIDDEN_SECRET") && !normalized.Contains("PASSWORD_SECRET"), "document excluded snapshot content stays excluded");
            var geometry = Pg.Cp(new Po(false, .5, 100, true, true, true, false), 850, 600, new Pb(0, 0, 850, 600));
            int expectedPages;
            using (var plan = Pw.Dc(snapshot, geometry))
            {
                Check(plan.Structured && plan.UnresolvedImages == 1, "document plan records structured mode and unresolved image");
                Check(plan.PageText.Count > 1 && String.Concat(plan.PageText) == plan.DocumentText, "document contiguous multi-page text");
                Check(Enumerable.Range(1, 8).All(i => plan.DocumentText.Contains("Document paragraph " + i)) && plan.DocumentText.Contains("Final document marker."), "document all paragraphs retained");
                var pages = new Pp(plan, snapshot.Title, snapshot.Source);
                expectedPages = pages.PageCount;
                Image(pages, 0, geometry.Body, Path.Combine(directory, "document-first.png"));
                Image(pages, pages.PageCount - 1, geometry.Body, Path.Combine(directory, "document-last.png"));
                using (var output = new FileStream(Path.Combine(directory, "document.xps"), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
                    Check(Px.Wr(pages, output) == pages.PageCount, "document structured XPS export");
            }
            using (var saved = new XpsDocument(Path.Combine(directory, "document.xps"), FileAccess.Read))
            {
                var paginator = ((IDocumentPaginatorSource)saved.GetFixedDocumentSequence()).DocumentPaginator;
                paginator.ComputePageCount();
                Check(paginator.PageCount == expectedPages, "document reopened XPS pagination");
                using (var page = paginator.GetPage(0)) Image(page, new Size(816, 576), geometry.Body, Path.Combine(directory, "document-readback.png"));
            }
            using (var package = Package.Open(Path.Combine(directory, "document.xps"), FileMode.Open, FileAccess.Read))
            {
                var text = new System.Text.StringBuilder(); int count = 0;
                foreach (var part in package.GetParts().Where(p => p.ContentType == "application/vnd.ms-package.xps-fixedpage+xml"))
                { using (var stream = part.GetStream()) text.Append(String.Concat(XDocument.Load(stream).Descendants().Attributes("UnicodeString").Select(a => a.Value))); count++; }
                Check(count > 1 && text.ToString().Contains("Final document marker.") && text.ToString().Contains("Inventory") && text.ToString().Contains("A2"), "document persisted XPS structure text");
            }
            Reject(() => Pm.Cp(null), "document null snapshot");
            Reject(() => Pw.Dc(snapshot, null), "document null geometry");
            Check(Task.Run(() => { try { Pm.Cp(snapshot); return false; } catch (InvalidOperationException) { return true; } }).Result, "document MTA rejected");
            Reject(() => Pw.Dc(Snapshot(Node("div")), geometry), "document empty content rejected");
            Reject(() => Pm.Cp(Snapshot(Node("#text", new string('x', Pw.MaxText + 1)))), "document text cap");
            var wide = Node("div"); for (int i = 0; i < Pm.MaxNodes; i++) wide.C.Add(Node("span"));
            Reject(() => Pm.Cp(Snapshot(wide)), "document node cap");
            var badTable = Node("table"); var row = Node("tr"); var cell = Node("td"); cell.A["colspan"] = "129"; row.C.Add(cell); badTable.C.Add(row);
            Reject(() => Pm.Cp(Snapshot(badTable)), "document table span cap");
            var manyTables = Node("div");
            for (int i = 0; i < Pm.MaxNodes / Pm.MaxColumns + 1; i++)
            {
                var tableNode = Node("table"); var rowNode = Node("tr"); var cellNode = Node("td");
                cellNode.A["colspan"] = Pm.MaxColumns.ToString(); rowNode.C.Add(cellNode); tableNode.C.Add(rowNode); manyTables.C.Add(tableNode);
            }
            Reject(() => Pm.Cp(Snapshot(manyTables)), "document aggregate column allocation cap");
            Console.WriteLine("PASS Lea structured document and foundation"); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
