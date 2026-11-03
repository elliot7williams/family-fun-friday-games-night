using System;
using Lea.Print;

public static class PrintGeometryChecks
{
    private static int checks;
    private static void Check(bool value, string name)
    { if (!value) throw new Exception(name); checks++; }
    private static bool Near(double a, double b) { return Math.Abs(a - b) < 0.000001; }
    private static void Reject(Action action, string name)
    {
        bool rejected = false;
        try { action(); } catch (ArgumentException) { rejected = true; }
        catch (InvalidOperationException) { rejected = true; }
        Check(rejected, name);
    }
    private static Po Options(double margin = .75, int scale = 100, bool header = true, bool footer = true)
    { return new Po(false, margin, scale, false, header, footer, false); }
    public static int Run()
    {
        checks = 0;
        var paper = new Pb(25, 25, 800, 1050);
        Pg page = Pg.Cp(Options(), 850, 1100, paper);
        Check(Near(page.Body.X, 75) && Near(page.Body.Y, 105), "margin and header placement");
        Check(Near(page.Body.Width, 700) && Near(page.Body.Height, 890), "body dimensions");
        Check(Near(page.Header.Height, 30) && Near(page.Footer.Y, 995), "header footer reserved");
        Check(Near(page.LayoutWidth, 672) && Near(page.UnitsPerPixel, 100.0 / 96), "CSS pixel conversion");
        Pg plain = Pg.Cp(Options(header:false, footer:false), 850, 1100, paper);
        Check(Near(plain.Body.Height, 950) && plain.Header.Height == 0 && plain.Footer.Height == 0, "optional bands");
        Pg hard = Pg.Cp(Options(0), 850, 1100, paper);
        Check(Near(hard.Body.X, 25) && Near(hard.Body.Width, 800), "hard margins honored");
        Pg scaled = Pg.Cp(Options(scale:125), 850, 1100, paper);
        Check(scaled.Body.Width == page.Body.Width && scaled.LayoutWidth < page.LayoutWidth, "scale not margin");
        var landscape = new Po(true, .75, 100, true, true, true, true);
        Pg wide = Pg.Cp(landscape, 1100, 850, new Pb(25, 25, 1050, 800));
        Check(Near(wide.Body.Width, 950) && Near(wide.Body.Height, 640), "oriented landscape geometry");
        Check(landscape.Landscape && landscape.Grayscale && landscape.SelectionOnly, "options retained");
        foreach(double margin in new[]{.35,.5,.75,1.0,1.25})
            foreach(int scale in new[]{75,90,100,110,125})
                if(Pg.Cp(Options(margin,scale),850,1100,paper).LayoutHeight <= 0) throw new Exception("dialog combination");
        Check(true, "all dialog margin scale combinations");
        Reject(() => Options(Double.NaN), "NaN margin");
        Reject(() => Options(Double.PositiveInfinity), "infinite margin");
        Reject(() => Options(-1), "negative margin");
        Reject(() => Options(scale:0), "zero scale");
        Reject(() => Options(scale:401), "excess scale");
        Reject(() => Pg.Cp(Options(5),850,1100,paper), "no remaining body");
        Reject(() => Pg.Cp(Options(),850,1100,new Pb(0,0,900,1000)), "area outside paper");
        Reject(() => Pg.Cp(Options(),Double.NaN,1100,paper), "nonfinite paper");
        Reject(() => Pg.Cp(Options(),850,1100,new Pb(0,0,Double.MaxValue,100)), "oversized printable");
        Reject(() => Pg.Cp(null,850,1100,paper), "null options");
        return checks;
    }
}
