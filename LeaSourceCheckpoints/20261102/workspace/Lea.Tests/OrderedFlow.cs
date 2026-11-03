using System;
using System.IO;
using System.Linq;
using Lea.Engine;

internal static class OrderedFlow
{
    private static int checks, failures;
    private static void Check(bool value, string name)
    { checks++; if (!value) { failures++; Console.WriteLine("FAIL " + name); } }
    private static P Load(string directory, string html)
    {
        string path = Path.Combine(directory, "ordered.html");
        File.WriteAllText(path, "<!doctype html><html><head><title>Order</title></head><body>" + html + "</body></html>");
        return new E().Go(path);
    }
    private static void Order(P page, params string[] tokens)
    {
        string content = String.Join("|", page.Bs.Select(block => block.I == null ? block.X : "FIELD-" + block.I.N));
        int at = -1;
        foreach (string token in tokens)
        {
            int next = content.IndexOf(token, at + 1, StringComparison.Ordinal);
            Check(next > at, "DOM order " + token + "; " + content); if (next >= 0) at = next;
        }
    }
    public static int Main(string[] args)
    {
        var page = Load(args[0], "<div><h2>Heading</h2><label>First</label><input name='one'><label>Second</label><input name='two'>Tail<p>After</p></div>");
        Order(page, "Heading", "First", "FIELD-one", "Second", "FIELD-two", "Tail", "After");
        Check(page.Fs.Count == 2, "fields emitted exactly once");
        page = Load(args[0], "<section>Lead <strong>Bold</strong><h2>Middle</h2>Trail <a href='next.html'>Link</a><p>Final</p>Last</section>");
        Order(page, "Lead", "Bold", "Middle", "Trail", "Link", "Final", "Last");
        Check(page.Ls.Count == 1, "inline link recorded once");
        Check(page.Bs.SelectMany(block => block.Is).Any(run => run.X.Contains("Bold") && run.S.B), "inline bold preserved");
        page = Load(args[0], "<div><em>Before<h3>Center</h3>After</em> End</div>");
        Order(page, "Before", "Center", "After", "End");
        Check(page.Bs.SelectMany(block => block.Is).Where(run => run.X.Contains("Before") || run.X.Contains("After")).All(run => run.S.I), "split inline inheritance retained");
        page = Load(args[0], "<style>#box{padding:10px;border:2px solid green;color:red}#box::before{content:'PREFIX'}#box::after{content:'SUFFIX'}</style><div id='box'>Lead<h2>Heading</h2>Trail</div>");
        Order(page, "PREFIX", "Lead", "Heading", "Trail", "SUFFIX");
        var owner = page.Bs.First(block => block.El != null && block.El.Av("id") == "box" && block.K == "div");
        Check(owner.X == "", "visual context does not hoist inline text");
        Check(page.Bs.Where(block => block != owner && (block.X.Contains("Lead") || block.X.Contains("Trail") || block.X.Contains("Heading"))).All(block => block.Up == owner), "anonymous groups and heading owned by context");
        Check(page.Bs.Count(block => block.K == "div") == 1, "one visual context");
        page = Load(args[0], "<div>First<h2 hidden>Hidden</h2>Second<input type='hidden' name='secret'>Third</div>");
        Order(page, "First", "Second", "Third");
        Check(!page.Bs.Any(block => block.X.Contains("Hidden")), "hidden content stays absent");
        Check(page.Bs.Count(block => block.X.Length > 0) == 1, "hidden nodes do not split visible inline flow");
        page = Load(args[0], "<div>Only <strong>inline</strong> text</div>");
        Check(page.Bs.Count == 1 && page.Bs[0].X == "Only inline text", "inline-only container keeps single block");
        page = Load(args[0], "<style>#grid{display:grid}</style><div id='grid'><div id='cell'>Lead<h2>Heading</h2>Trail</div></div>");
        var cell = page.Bs.FirstOrDefault(block => block.K == "div" && block.El.Av("id") == "cell");
        Check(cell != null, "unstyled mixed container retains grid item ownership");
        Check(cell != null && page.Bs.Where(block => block != cell && (block.X.Contains("Lead") || block.X.Contains("Heading") || block.X.Contains("Trail"))).All(block => block.Up == cell), "mixed grid item keeps its own children");
        page = Load(args[0], "<div style='width:60%'><p id='inherited' style='width:inherit'>Inherited</p>Tail<input name='inherited-field' style='width:inherit'></div>");
        Check(page.Bs.First(block => block.El.Av("id") == "inherited").S.Wp == 60, "explicit block percentage inheritance retained");
        Check(page.Fs.Single(item => item.N == "inherited-field").S.Wp == 60, "explicit field percentage inheritance retained");
        page = Load(args[0], "<style>section{white-space:pre-line}</style><section><p>Inherited     words\nInherited line</p></section>");
        Check(page.Bs.Count == 1, "unpainted section does not introduce an empty block");
        Check(page.Bs.Last().S.Ws == "pl", "omitted context retains white-space inheritance");
        Check(page.Bs.Last().X == "Inherited     words\nInherited line", "omitted context preserves inherited raw text and line breaks");
        page = Load(args[0], "<section><p>First</p><p>Second</p></section>");
        Check(page.Bs.Count == 2 && page.Bs[0].X == "First" && page.Bs[1].X == "Second", "plain block-only container preserves compact model");
        Console.WriteLine("Ordered flow: checks=" + checks + "; failures=" + failures);
        return failures == 0 ? 0 : 1;
    }
}
