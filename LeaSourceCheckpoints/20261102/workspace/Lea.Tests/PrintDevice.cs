using System;
using System.IO;
using System.Printing;
using System.Xml.Linq;
using Lea.Print;

internal static class PrintDevice
{
    private static int checks;
    private static readonly XNamespace Psf = "http://schemas.microsoft.com/windows/2003/08/printing/printschemaframework";
    private static XElement Property(string name, int value)
    { return new XElement(Psf + "Property", new XAttribute("name", "psk:" + name), new XElement(Psf + "Value", new XAttribute(XName.Get("type", "http://www.w3.org/2001/XMLSchema-instance"), "xsd:integer"), value)); }
    private static PrintCapabilities Caps(int width = 215900, int height = 279400, int x = 6350, int y = 6350,
        int extentWidth = 203200, int extentHeight = 266700, bool area = true, bool heightPresent = true)
    {
        var size = new XElement(Psf + "Property", new XAttribute("name", "psk:PageImageableSize"), Property("ImageableSizeWidth", width));
        if (heightPresent) size.Add(Property("ImageableSizeHeight", height));
        if (area) size.Add(new XElement(Psf + "Property", new XAttribute("name", "psk:ImageableArea"),
            Property("OriginWidth", x), Property("OriginHeight", y), Property("ExtentWidth", extentWidth), Property("ExtentHeight", extentHeight)));
        var xml = new XDocument(new XElement(Psf + "PrintCapabilities", new XAttribute("version", "1"),
            new XAttribute(XNamespace.Xmlns + "psf", Psf.NamespaceName),
            new XAttribute(XNamespace.Xmlns + "psk", "http://schemas.microsoft.com/windows/2003/08/printing/printschemakeywords"),
            new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"), size));
        using (var memory = new MemoryStream()) { xml.Save(memory); memory.Position = 0; return new PrintCapabilities(memory); }
    }
    private static void Check(bool condition, string name)
    { if (!condition) throw new Exception(name); checks++; Console.WriteLine("PASS " + name); }
    private static bool Near(double a, double b) { return Math.Abs(a - b) < .000001; }
    private static void Reject(Action action, string name)
    {
        bool denied = false;
        try { action(); } catch (ArgumentException) { denied = true; } catch (InvalidOperationException) { denied = true; }
        Check(denied, name);
    }
    public static void Run()
    {
        checks = 0;
        var options = new Po(false, 0, 100, false, false, false, false);
        var caps = Caps();
        Check(Near(caps.OrientedPageMediaWidth.Value, 816) && Near(caps.PageImageableArea.OriginWidth, 24), "device real PrintCapabilities schema units");
        var page = Pd.Cp(options, caps);
        Check(Near(page.PaperWidth, 850) && Near(page.PaperHeight, 1100), "device DIP to hundredth inch paper conversion");
        Check(Near(page.Body.X, 25) && Near(page.Body.Y, 25) && Near(page.Body.Width, 800) && Near(page.Body.Height, 1050), "device hard margins preserved");
        Check(Near(page.LayoutWidth, 768) && Near(page.LayoutHeight, 1008), "device CSS viewport conversion");
        var wide = Pd.Cp(new Po(true, 0, 100, false, false, false, false), Caps(279400, 215900, 6350, 6350, 266700, 203200));
        Check(Near(wide.PaperWidth, 1100) && Near(wide.PaperHeight, 850) && Near(wide.Body.Width, 1050), "device already oriented landscape not rotated twice");
        var borderless = Pd.Cp(options, Caps(215900, 279400, 0, 0, 215900, 279400));
        Check(Near(borderless.Body.Width, 850) && Near(borderless.Body.Height, 1100), "device borderless bounds");
        var metric = Pd.Cp(options, Caps(210000, 297000, 5000, 7000, 200000, 283000));
        Check(Near(metric.PaperWidth, 210000.0 / 254) && Near(metric.Body.Y, 7000.0 / 254), "device metric paper and asymmetric origin");
        var styled = Pd.Cp(new Po(false, .75, 125, true, true, true, false), caps);
        Check(Near(styled.Body.X, 75) && Near(styled.Body.Y, 105) && Near(styled.Body.Height, 890), "device user margins and bands intersect hard margins");
        Check(styled.Grayscale && styled.LayoutWidth < page.LayoutWidth, "device scale and grayscale retained");
        Reject(() => Pd.Cp(null, caps), "device null options denied");
        Reject(() => Pd.Cp(options, null), "device null capabilities denied");
        Reject(() => Pd.Cp(options, Caps(area:false)), "device missing imageable area denied");
        Reject(() => Pd.Cp(options, Caps(heightPresent:false)), "device missing paper dimension denied");
        Reject(() => Pd.Cp(options, Caps(extentWidth:215900)), "device area beyond paper denied");
        Reject(() => Pd.Cp(new Po(false, 5, 100, false, true, true, false), caps), "device impossible user margins denied");
        Check(Near(caps.PageImageableArea.OriginWidth, 24) && Near(caps.OrientedPageMediaHeight.Value, 1056), "device capabilities not mutated");
        Console.WriteLine("PASS Lea printer geometry: " + checks);
    }
}
