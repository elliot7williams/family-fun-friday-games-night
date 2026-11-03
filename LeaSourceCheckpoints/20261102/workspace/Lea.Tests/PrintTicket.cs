using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Lea.Engine;
using Lea.Print;

internal static class PrintTicketChecks
{
    private static int checks;
    private static void Check(bool value, string name)
    { if (!value) throw new Exception(name); checks++; Console.WriteLine("PASS " + name); }
    private static bool Denied(Action action)
    {
        try { action(); } catch (ArgumentException) { return true; }
        catch (InvalidOperationException) { return true; } catch (NotSupportedException) { return true; }
        return false;
    }
    private static N Node(string kind, string text = "")
    {
        var node = (N)Activator.CreateInstance(typeof(N), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { kind }, null);
        typeof(N).GetProperty("X").SetValue(node, text, null); return node;
    }
    private static Ps ImageDocument()
    {
        N root = Node("#doc"), body = Node("body"), image = Node("img");
        image.A["src"] = "https://example.test/unfetched.png";
        root.C.Add(body); body.C.Add(Node("#text", "Prepared image guard fixture")); body.C.Add(image);
        var page = (P)Activator.CreateInstance(typeof(P), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, new object[] { new Uri("https://example.test/") }, null);
        typeof(P).GetProperty("D").SetValue(page, root, null); return Ps.Cp(page);
    }
    public static void Run(Pj selection)
    {
        checks = 0;
        var options = new Po(false, .75, 100, false, true, true, false);
        Check(Denied(() => selection.Cp(null)), "ticket null options denied");
        Check(Denied(() => selection.Cp(new Po(false, .75, 100, false, true, true, true))), "ticket ordinary selection rejected before preparation");
        using (var prepared = selection.Cp(options))
        {
            Check(prepared.Name == "Microsoft XPS Document Writer", "ticket prepared for accepted printer");
            var geometry = prepared.Geometry;
            Check(geometry.PaperWidth > 0 && geometry.PaperHeight > 0 && geometry.LayoutWidth > 0 && geometry.LayoutHeight > 0, "ticket real driver capabilities produce geometry");
            Check(geometry.Body.X >= 75 && geometry.Body.Y >= 105, "ticket user margins and header retained");
            var ticket = prepared.Tk;
            Check(ticket.CopyCount == selection.St.Copies, "ticket validated copies match native selection");
            ticket.CopyCount = ticket.CopyCount == 1 ? 2 : 1;
            Check(prepared.Tk.CopyCount == selection.St.Copies, "ticket defensive clone retains prepared settings");
            Check(Task.Run(() => Denied(() => { var value = prepared.Geometry; })).Result, "ticket geometry dispatcher guard");
            Check(Task.Run(() => Denied(() => prepared.Dispose())).Result, "ticket wrong thread disposal denied");
            Check(Denied(() => prepared.Rg(null)), "ticket null pages rejected");
            Check(Denied(() => prepared.Wr(null)), "ticket null submission rejected before dispatch");
            using (var other = Pw.Cp("Different capture geometry", Pg.Cp(options, 850, 1100, new Pb(0, 0, 850, 1100))))
            {
                var wrong = new Pp(other, "", "");
                Check(Denied(() => prepared.Rg(wrong)), "ticket foreign geometry rejected");
                Check(Denied(() => prepared.Wr(wrong)), "ticket foreign geometry cannot dispatch");
            }
            using (var plan = Pw.Cp(String.Join("\n", Enumerable.Range(1, 120).Select(i => "Prepared driver page line " + i)), geometry))
            {
                var pages = new Pp(plan, "Prepared fixture", "");
                var range = prepared.Rg(pages);
                Check(range.PageCount == pages.PageCount && range.PageSize == pages.PageSize, "ticket all-page paginator bound to prepared geometry");
                using (var memory = new MemoryStream())
                {
                    Check(Px.Rg(range, memory) == pages.PageCount && memory.Length > 1000, "ticket prepared pages export without queue submission");
                    using (var output = new FileStream("prepared-ticket.xps", FileMode.CreateNew, FileAccess.Write)) memory.WriteTo(output);
                }
                using (var images = Pw.Dc(ImageDocument(), geometry))
                {
                    var unresolved = new Pp(images, "", "");
                    Check(images.UnresolvedImages > 0 && Denied(() => prepared.Rg(unresolved)), "ticket unresolved images rejected");
                    Check(Denied(() => prepared.Wr(unresolved)), "ticket unresolved images cannot dispatch");
                }
                prepared.Dispose(); prepared.Dispose();
                Check(Denied(() => { var value = prepared.Tk; }), "ticket disposed settings denied");
                Check(Denied(() => { var value = prepared.Name; }), "ticket disposed printer denied");
                Check(Denied(() => prepared.Rg(pages)), "ticket disposed range denied");
                Check(Denied(() => prepared.Wr(pages)), "ticket disposed submission rejected");
                Check(pages.PageCount > 0, "ticket disposal does not own caller plan");
            }
        }
        Console.WriteLine("PASS Lea prepared printer ticket: " + checks);
    }
}
