using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Lea.Print;

internal static class PrintTextMain
{
    private static void Check(bool value, string name)
    { if (!value) throw new Exception(name); Console.WriteLine("PASS " + name); }
    private static void Reject(Action action, string name)
    {
        bool denied = false;
        try { action(); } catch (ArgumentException) { denied = true; }
        catch (InvalidOperationException) { denied = true; }
        Check(denied, name);
    }
    private static void Pixels(Pt plan, Pg geometry, string path, int page, float dpi)
    {
        using (var bitmap = new Bitmap((int)(850 * dpi / 100), (int)(1100 * dpi / 100)))
        {
            bitmap.SetResolution(dpi, dpi);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White); g.TranslateTransform(3, 4);
                float before = g.Transform.OffsetX;
                plan.Dr(g, page);
                Check(g.Transform.OffsetX == before, "graphics state restored " + dpi);
            }
            int ink = 0, outside = 0;
            for (int y = 0; y < bitmap.Height; y++) for (int x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).R > 240) continue;
                ink++;
                if (x < geometry.Body.X * dpi / 100 - 1 || y < geometry.Body.Y * dpi / 100 - 1 ||
                    x > (geometry.Body.X + geometry.Body.Width) * dpi / 100 + 1 ||
                    y > (geometry.Body.Y + geometry.Body.Height) * dpi / 100 + 1) outside++;
            }
            Check(ink > 100, "nonblank native text " + dpi);
            Check(outside == 0, "ink stays inside body " + dpi);
            bitmap.Save(path, ImageFormat.Png);
        }
    }
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (PrintSnapshotChecks.Run() != 40 || PrintGeometryChecks.Run() != 20 || PrintCaptureProbe.Run(args) != 0)
                throw new Exception("Foundation failed");
            Pg geometry = Pg.Cp(new Po(false, .75, 100, true, true, true, true), 850, 1100, new Pb(25,25,800,1050));
            var text = new StringBuilder();
            for (int i = 0; i < 120; i++) text.Append("Paragraph " + i + ": Lea native text pagination. Unicode e\u0301 and \uD83D\uDE00 remain intact.\r\n");
            using (Pt plan = Pt.Cp(text.ToString(), geometry))
            {
                Check(plan.Pages.Count > 1, "multiple text pages");
                Check(String.Concat(plan.Pages) == text.ToString(), "pagination preserves every character");
                Check(plan.Pages.All(p => p.Length > 0 && !Char.IsLowSurrogate(p[0]) && p[0] != '\u0301' && p[0] != '\n'), "safe page boundaries");
                string folder = Path.GetDirectoryName(args[0]);
                Pixels(plan, geometry, Path.Combine(folder,"text-first-96.png"),0,96);
                Pixels(plan, geometry, Path.Combine(folder,"text-last-144.png"),plan.Pages.Count-1,144);
                Reject(() => plan.Dr(null,0), "null graphics rejected");
                using (var bitmap = new Bitmap(1,1)) using(Graphics g = Graphics.FromImage(bitmap))
                    Reject(() => plan.Dr(g,-1), "invalid page rejected");
            }
            Reject(() => Pt.Cp(" ",geometry), "blank text rejected");
            Reject(() => Pt.Cp(new string('x',Pt.MaxText+1),geometry), "text cap enforced");
            Reject(() => Pt.Cp("x",geometry,points:Single.NaN), "invalid font rejected");
            var tiny = Pg.Cp(new Po(false,0,100,false,false,false,true),1,1,new Pb(0,0,1,1));
            Reject(() => Pt.Cp("cannot fit",tiny), "unfittable body rejected");
            using (Pt plan = Pt.Cp("disposed",geometry))
            using (var bitmap = new Bitmap(1,1)) using(Graphics g = Graphics.FromImage(bitmap))
            { plan.Dispose(); Reject(() => plan.Dr(g,0), "disposed renderer rejected"); }
            Console.WriteLine("PASS Lea native text print and foundation"); return 0;
        }
        catch(Exception error){Console.Error.WriteLine(error);return 1;}
    }
}
