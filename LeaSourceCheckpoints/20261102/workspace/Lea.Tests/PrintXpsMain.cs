using System;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
using System.Xml.Linq;
using Lea.Print;

internal static class PrintXpsMain
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
            if (PrintPaginatorMain.Run(args) != 0) return 1;
            var geometry = Pg.Cp(new Po(false, .5, 100, true, true, true, true), 850, 400, new Pb(0, 0, 850, 400));
            string text = String.Join("\n", Enumerable.Range(1, 36).Select(i => "Lea XPS line " + i + " accent e\u0301 emoji \uD83D\uDE00"));
            using (var plan = Pw.Cp(text, geometry))
            using (var memory = new MemoryStream())
            {
                var pages = new Pp(plan, "Lea XPS test", "https://user:password@example.test/article?token=secret#private");
                Check(Px.Wr(pages, memory) == pages.PageCount && pages.PageCount > 1, "XPS export page count");
                Check(memory.CanRead && memory.CanWrite && memory.Length > 1000, "XPS caller stream retained");
                byte[] bytes = memory.ToArray();
                string path = Path.Combine(Path.GetDirectoryName(args[0]), "lea-export.xps");
                using (var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None)) output.Write(bytes, 0, bytes.Length);
                Reject(() => Px.Wr(pages, memory), "XPS nonempty destination denied");
                Check(bytes.SequenceEqual(memory.ToArray()), "XPS denied destination unchanged");
                using (var readOnly = new MemoryStream(new byte[0], false)) Reject(() => Px.Wr(pages, readOnly), "XPS readonly destination denied");
                Reject(() => Px.Wr(pages, null), "XPS null destination denied");
                Reject(() => Px.Wr(null, memory), "XPS null paginator denied");
                memory.Position = 0;
                using (var package = Package.Open(memory, FileMode.Open, FileAccess.Read))
                {
                    var fixedPages = package.GetParts().Where(p => p.ContentType == "application/vnd.ms-package.xps-fixedpage+xml").ToArray();
                    Check(fixedPages.Length == pages.PageCount, "XPS persisted fixed page count");
                    using (var log = new StreamWriter(new FileStream(Path.Combine(Path.GetDirectoryName(args[0]), "xps-parts.txt"), FileMode.CreateNew)))
                        foreach (var part in package.GetParts()) log.WriteLine(part.Uri + " " + part.ContentType);
                    Check(package.GetParts().Any(p => p.ContentType == "application/vnd.ms-opentype" || p.ContentType == "application/vnd.ms-package.obfuscated-opentype"), "XPS embedded font resources");
                    int index = 0;
                    var allText = new StringBuilder();
                    foreach (var part in fixedPages.OrderBy(p => Int32.Parse(Path.GetFileNameWithoutExtension(p.Uri.OriginalString))))
                    {
                        XDocument xml; using (var stream = part.GetStream(FileMode.Open, FileAccess.Read)) xml = XDocument.Load(stream);
                        Check((double)xml.Root.Attribute("Width") == pages.PageSize.Width && (double)xml.Root.Attribute("Height") == pages.PageSize.Height, "XPS stored paper size " + index);
                        string glyphText = String.Concat(xml.Descendants().Attributes("UnicodeString").Select(a => a.Value));
                        allText.Append(glyphText);
                        Check(glyphText.Contains("Lea XPS line") && glyphText.Contains("Lea XPS test") && glyphText.Contains("Page " + (index + 1) + " of " + pages.PageCount), "XPS stored body header footer " + index);
                        Check(!glyphText.Contains("password") && !glyphText.Contains("token=") && !glyphText.Contains("#private"), "XPS displayed address privacy " + index);
                        index++;
                    }
                    Check(Enumerable.Range(1, 36).All(i => allText.ToString().Contains("Lea XPS line " + i + " accent")), "XPS every fixture line retained");
                }
                using (var document = new XpsDocument(path, FileAccess.Read))
                {
                    var paginator = ((IDocumentPaginatorSource)document.GetFixedDocumentSequence()).DocumentPaginator;
                    paginator.ComputePageCount();
                    Check(paginator.PageCount == pages.PageCount, "XPS reopened pagination");
                    using (var first = paginator.GetPage(0))
                    {
                        var element = first.Visual as System.Windows.UIElement;
                        if (element != null)
                        {
                            element.Measure(first.Size);
                            element.Arrange(new System.Windows.Rect(first.Size));
                            element.UpdateLayout();
                        }
                        var bitmap = new RenderTargetBitmap(816, 384, 96, 96, PixelFormats.Pbgra32); bitmap.Render(first.Visual);
                        var pixels = new byte[816 * 384 * 4]; bitmap.CopyPixels(pixels, 816 * 4, 0);
                        Check(Enumerable.Range(0, pixels.Length / 4).Count(i => pixels[i * 4 + 3] > 0 && pixels[i * 4] < 200) > 500, "XPS reopened page visible");
                        var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
                        using (var output = new FileStream(Path.Combine(Path.GetDirectoryName(args[0]), "xps-readback.png"), FileMode.CreateNew)) png.Save(output);
                        int bodyInk = 0;
                        for (int y = (int)(geometry.Body.Y * .96); y < (geometry.Body.Y + geometry.Body.Height) * .96; y++)
                            for (int x = (int)(geometry.Body.X * .96); x < (geometry.Body.X + geometry.Body.Width) * .96; x++)
                                if (pixels[(y * 816 + x) * 4 + 3] > 0 && pixels[(y * 816 + x) * 4] < 200) bodyInk++;
                        Check(bodyInk > 1000, "XPS reopened body visible independently of bands");
                    }
                }
                plan.Dispose();
                using (var empty = new MemoryStream())
                {
                    Reject(() => Px.Wr(pages, empty), "XPS disposed plan denied");
                    Check(empty.Length == 0, "XPS plan failure leaves destination empty");
                }
            }
            Console.WriteLine("PASS Lea XPS export and foundation"); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
