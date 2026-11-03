using System;
using System.Reflection;
using Lea.Engine;
using Lea.Print;

public static class PrintSnapshotChecks
{
    private static int checks;
    private static N Node(string kind, string text = "")
    {
        N node = (N)Activator.CreateInstance(typeof(N), BindingFlags.Instance | BindingFlags.NonPublic,
            null, new object[] { kind }, null);
        typeof(N).GetProperty("X").SetValue(node, text, null);
        return node;
    }
    private static P Page(N root, string address = "https://example.test/docs/page")
    {
        P page = (P)Activator.CreateInstance(typeof(P), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, new object[] { new Uri(address) }, null);
        typeof(P).GetProperty("D").SetValue(page, root, null);
        typeof(P).GetProperty("U").SetValue(page, new Uri(address), null);
        return page;
    }
    private static void Check(bool value, string name)
    { if (!value) throw new Exception(name); checks++; }
    private static void Reject(Action action, string name)
    {
        bool rejected = false;
        try { action(); } catch (InvalidOperationException) { rejected = true; }
        catch (NotSupportedException) { rejected = true; }
        Check(rejected, name);
    }
    private static Pn Image(string source, string address = "https://example.test/docs/page")
    {
        N image = Node("img"); image.A["src"] = source;
        return Ps.Cp(Page(image, address)).Nodes[0];
    }
    public static int Run()
    {
        checks = 0;
        N root = Node("#doc"), body = Node("body"), text = Node("#text", "A < B & C");
        root.C.Add(body); body.C.Add(text);
        body.A["onclick"] = "doSomething()"; body.A["title"] = "Article";
        Ps snapshot = Ps.Cp(Page(root));
        Check(snapshot.Nodes.Count == 3, "node count");
        Check(snapshot.Nodes[2].Parent == 1 && snapshot.Nodes[1].Parent == 0, "parent order");
        Check(snapshot.Nodes[2].Text == "A < B & C", "literal decoded text");
        Check(snapshot.Nodes[1].Attributes["title"] == "Article", "allowed attribute");
        Check(!snapshot.Nodes[1].Attributes.ContainsKey("onclick"), "event inert");
        typeof(N).GetProperty("X").SetValue(text, "changed", null); body.A["title"] = "changed";
        Check(snapshot.Nodes[2].Text == "A < B & C" && snapshot.Nodes[1].Attributes["title"] == "Article", "deep snapshot");
        Reject(() => ((System.Collections.Generic.IList<Pn>)snapshot.Nodes).Clear(), "nodes readonly");
        Reject(() => ((System.Collections.Generic.IDictionary<string,string>)snapshot.Nodes[1].Attributes).Clear(), "attributes readonly");
        foreach (string kind in new[] { "input", "textarea", "script", "iframe", "head", "template", "object", "embed", "select", "button", "style" })
        {
            N excluded = Node(kind); excluded.A["value"] = "secret"; excluded.C.Add(Node("#text", "secret"));
            Ps hidden = Ps.Cp(Page(excluded)); Check(hidden.Nodes.Count == 0 && hidden.Omitted == 2, "omit " + kind);
        }
        N concealed = Node("div"); concealed.A["hidden"] = ""; concealed.C.Add(Node("#text", "private"));
        Check(Ps.Cp(Page(concealed)).Nodes.Count == 0, "hidden subtree");
        Check(Image("/image.png").ImagePolicy == "same-origin-unfetched", "same origin");
        Check(Image("https://other.test/x").ImagePolicy == "denied", "cross host");
        Check(Image("http://example.test/x").ImagePolicy == "denied", "scheme");
        Check(Image("https://example.test:444/x").ImagePolicy == "denied", "port");
        Check(Image("https://user:secret@example.test/x").Image == "", "credentials");
        Check(Image("javascript:alert(1)").ImagePolicy == "denied", "active uri");
        Check(Image("data:image/png;base64,AAAA").ImagePolicy == "denied", "data uri");
        Check(Image("file:///C:/private.png").ImagePolicy == "requires-file-policy", "file not granted");
        Check(Image("/x", "file:///C:/page.html").ImagePolicy == "requires-file-policy", "file relative");
        N duplicate = Node("div"); duplicate.C.Add(text); duplicate.C.Add(text);
        Reject(() => Ps.Cp(Page(duplicate)), "shared node");
        N cycle = Node("div"); cycle.C.Add(cycle); Reject(() => Ps.Cp(Page(cycle)), "cycle");
        N malformed = Node("div"); malformed.C.Add(null); Reject(() => Ps.Cp(Page(malformed)), "null child");
        N deep = Node("div"), tail = deep;
        for (int i = 0; i <= Ps.MaxDepth; i++) { N next = Node("div"); tail.C.Add(next); tail = next; }
        Reject(() => Ps.Cp(Page(deep)), "depth");
        Reject(() => Ps.Cp(Page(Node("#text", new string('x', Ps.MaxCharacters)))), "characters");
        Reject(() => Image(new string('x', Ps.MaxCharacters)), "resource characters");
        N wide = Node("div"); for (int i = 0; i < Ps.MaxNodes; i++) wide.C.Add(Node("br"));
        Reject(() => Ps.Cp(Page(wide)), "node limit");
        wide.C.RemoveAt(wide.C.Count - 1);
        Check(Ps.Cp(Page(wide)).Nodes.Count == Ps.MaxNodes, "exact node limit accepted");
        P exact = Page(Node("#text"));
        typeof(N).GetProperty("X").SetValue(exact.D,
            new string('x', Ps.MaxCharacters - exact.U.AbsoluteUri.Length - "#text".Length), null);
        Check(Ps.Cp(exact).Nodes.Count == 1, "exact character limit accepted");
        Check(Ps.Cp(Page(Node("div"), "https://user:secret@example.test/path")).Source == "", "source credentials redacted");
        P based = Page(Node("img")); based.D.A["src"] = "picture.png";
        typeof(P).GetProperty("Bu").SetValue(based, new Uri("https://other.test/base/"), null);
        Check(Ps.Cp(based).Nodes[0].ImagePolicy == "denied", "base cannot change origin");
        return checks;
    }
}
