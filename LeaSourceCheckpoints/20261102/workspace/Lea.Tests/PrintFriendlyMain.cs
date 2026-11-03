using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Lea.Engine;
using Lea.Print;

internal static class PrintFriendlyMain
{
    private static int checks;
    private static void Check(bool value, string name) { if (!value) throw new Exception(name); checks++; }
    private static void Reject(Action action, string name)
    {
        bool denied = false;
        try { action(); } catch (ArgumentException) { denied = true; } catch (InvalidOperationException) { denied = true; }
        Check(denied, name);
    }
    private static N Node(string kind, string text = "")
    {
        var node = (N)Activator.CreateInstance(typeof(N), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { kind }, null);
        typeof(N).GetProperty("X").SetValue(node, text, null); return node;
    }
    private static P Page(N root)
    {
        var page = (P)Activator.CreateInstance(typeof(P), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new object[] { new Uri("https://example.test/article") }, null);
        typeof(P).GetProperty("D").SetValue(page, root, null);
        typeof(P).GetProperty("U").SetValue(page, new Uri("https://example.test/article"), null); return page;
    }
    private static string Text(Ps document) { return String.Concat(document.Nodes.Select(n => n.Text)); }
    [STAThread]
    private static int Main(string[] args)
    { return Run(args); }
    internal static int Run(string[] args)
    {
        try
        {
            int baseline = PrintSnapshotChecks.Run();
            foreach (string kind in new[] { "nav", "aside", "footer", "form", "menu", "dialog", "video", "audio", "canvas" })
            {
                N root = Node("article"), excluded = Node(kind); excluded.C.Add(Node("#text", "chrome"));
                root.C.Add(excluded); root.C.Add(Node("#text", "article"));
                var page = Page(root);
                Check(Text(Ps.Cf(page)) == "article", "semantic exclusion " + kind);
                Check(Text(Ps.Cp(page)) == "chromearticle", "ordinary snapshot unchanged " + kind);
            }
            foreach (string token in new[] { "ad", "ADS", "advert", "advertisement", "cookie", "cookies", "popup", "modal", "share", "social" })
            {
                N node = Node("div"); node.A["class"] = "layout\t" + token + "\nwidget"; node.C.Add(Node("#text", "chrome"));
                Check(Ps.Cf(Page(node)).Nodes.Count == 0, "whole class token " + token);
            }
            foreach (string token in new[] { "navigation", "BANNER", "complementary" })
            {
                N node = Node("section"); node.A["role"] = " " + token + " "; node.C.Add(Node("#text", "chrome"));
                Check(Ps.Cf(Page(node)).Nodes.Count == 0, "role exclusion " + token);
            }
            N content = Node("article"), link = Node("a"), image = Node("img");
            content.A["class"] = "address shadow socialized cookie-recipe"; content.A["role"] = "main";
            content.C.Add(Node("#text", "Literal < & content"));
            link.A["href"] = "/reference"; link.C.Add(Node("#text", "reference")); content.C.Add(link);
            image.A["src"] = "/figure.png"; image.A["alt"] = "Figure"; content.C.Add(image);
            P pageContent = Page(content); Ps captured = Ps.Cf(pageContent);
            Check(Text(captured) == "Literal < & contentreference", "near-match classes preserve content");
            Check(captured.Nodes.First(n => n.Kind == "a").Link == "https://example.test/reference", "link retained safely: " + captured.Nodes.First(n => n.Kind == "a").Link);
            Check(captured.Nodes.First(n => n.Kind == "img").ImagePolicy == "same-origin-unfetched", "image remains unfetched");
            Check(content.C.Count == 3 && content.A["role"] == "main", "live DOM unchanged");
            for (int i = 0; i < captured.Nodes.Count; i++) Check(captured.Nodes[i].Parent < i, "parent order");
            var map = new Dictionary<N, S>();
            foreach (N n in new[] { content, content.C[0], link, link.C[0], image })
                map.Add(n, (S)Activator.CreateInstance(typeof(S), true));
            Check(Ps.Cf(pageContent, map).Styled, "complete print style map accepted");
            map.Remove(image); Reject(() => Ps.Cf(pageContent, map), "incomplete styles rejected");
            Reject(() => Ps.Cf(pageContent, null), "null styles rejected");
            N huge = Node("div"); huge.A["class"] = new string('x', Ps.MaxCharacters);
            Reject(() => Ps.Cf(Page(huge)), "friendly metadata bounded");
            N cycle = Node("nav"); cycle.C.Add(cycle); Reject(() => Ps.Cf(Page(cycle)), "excluded cycle rejected");
            Reject(() => Ps.Cf(null), "null document rejected");
            string json = "{\"passed\":true,\"baselineChecks\":" + baseline + ",\"friendlyChecks\":" + checks + ",\"browserRouted\":false,\"printJobSubmitted\":false}";
            using (var file = new FileStream(Path.Combine(args[0], "friendly.json"), FileMode.CreateNew))
            using (var writer = new StreamWriter(file)) writer.Write(json);
            Console.WriteLine(json); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
