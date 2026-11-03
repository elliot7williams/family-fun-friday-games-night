using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Lea.Engine;

internal static class NumericReferences
{
    private static int checks, failures;
    private static readonly MethodInfo Tree = typeof(E).GetMethod("Tree", BindingFlags.NonPublic | BindingFlags.Static);
    private static void Eq(string expected, string actual, string name)
    {
        checks++;
        if (expected == actual) return;
        failures++;
        Console.WriteLine("FAIL " + name + "; expected=" + Escape(expected) + "; actual=" + Escape(actual));
    }
    private static string Escape(string value) { return BitConverter.ToString(Encoding.Unicode.GetBytes(value ?? "")); }
    private static N Id(N node, string id)
    {
        if (node.Av("id") == id) return node;
        foreach (var child in node.C) { var found = Id(child, id); if (found != null) return found; }
        return null;
    }
    private static string Text(N node)
    {
        if (node.K == "#text") return node.X;
        var text = new StringBuilder(); foreach (var child in node.C) text.Append(Text(child)); return text.ToString();
    }
    private static void Case(string input, string expected)
    {
        var tree = (N)Tree.Invoke(null, new object[] { "<p id='t' data-x='" + input + "'>" + input + "</p>" });
        var node = Id(tree, "t");
        Eq(expected, Text(node), "DOM " + input.Substring(0, Math.Min(40, input.Length)));
        Eq(expected, node.Av("data-x"), "attribute " + input.Substring(0, Math.Min(40, input.Length)));
    }
    public static int Main(string[] args)
    {
        foreach (var pair in new[] {
            new[]{"&#65;", "A"}, new[]{"&#65", "A"}, new[]{"&#x41;", "A"}, new[]{"&#X41", "A"},
            new[]{"&#65z", "Az"}, new[]{"&#x41g", "Ag"}, new[]{"&#x80=", "\u20ac="},
            new[]{"&#0;", "\ufffd"}, new[]{"&#xD800;", "\ufffd"}, new[]{"&#xDFFF;", "\ufffd"},
            new[]{"&#x110000;", "\ufffd"}, new[]{"&#1114112", "\ufffd"},
            new[]{"&#x1F642;", "\ud83d\ude42"}, new[]{"&#x10FFFF;", "\udbff\udfff"},
            new[]{"&#xFFFF;", "\uffff"}, new[]{"&#xFDD0;", "\ufdd0"}, new[]{"&#13;", "\r"},
            new[]{"&#;", "&#;"}, new[]{"&#x;", "&#x;"}, new[]{"&#Xg;", "&#Xg;"},
            new[]{"&#-1;", "&#-1;"}, new[]{"&#+65;", "&#+65;"}, new[]{"&# 65;", "&# 65;"},
            new[]{"&amp;#65;", "&#65;"}, new[]{"&#38;#65;", "&#65;"}, new[]{"&#x26;amp;", "&amp;"},
            new[]{"&lt;&#65;&amp;", "<A&"}, new[]{"&unknown;&#65;", "&unknown;A"},
            new[]{"&#x&#65;", "&#xA"}, new[]{"&#65;&#66;&#67", "ABC"},
            new[]{"&#" + new string('9', 10000) + ";", "\ufffd"},
            new[]{"&#x" + new string('f', 10000) + ";", "\ufffd"},
            new[]{"&#" + new string('0', 10000) + "65;", "A"}
        }) Case(pair[0], pair[1]);
        var remap = new[] { 0x20ac,0x81,0x201a,0x192,0x201e,0x2026,0x2020,0x2021,0x2c6,0x2030,0x160,0x2039,0x152,0x8d,0x17d,0x8f,
            0x90,0x2018,0x2019,0x201c,0x201d,0x2022,0x2013,0x2014,0x2dc,0x2122,0x161,0x203a,0x153,0x9d,0x17e,0x178 };
        for (var i = 0; i < remap.Length; i++) Case("&#" + (i + 128) + ";", Char.ConvertFromUtf32(remap[i]));
        var raw = (N)Tree.Invoke(null, new object[] { "<script id='s'>&#65;&amp;</script><style id='c'>&#65;&amp;</style><textarea id='r'>&#x80;</textarea>" });
        Eq("&#65;&amp;", Text(Id(raw, "s")), "script raw text"); Eq("&#65;&amp;", Text(Id(raw, "c")), "style raw text");
        Eq("\u20ac", Text(Id(raw, "r")), "textarea RCDATA");
        var path = Path.Combine(args[0], "single-pass.html");
        File.WriteAllText(path, "<!doctype html><title>&amp;lt;</title><p id='a'>&amp;lt;</p><p id='b'><span>&amp;lt;</span></p><pre id='c'><span>&amp;lt;</span></pre><p id='d'>&#38;#x80;</p><input id='f' value='&#x80;'><p id='e'>&#x80;</p>");
        var page = new E().Go(path);
        Eq("&lt;", page.T, "title decoded once");
        var presentationBlocks = 0;
        foreach (var block in page.Bs)
        {
            if (block.El == null) continue;
            var id = block.El.Av("id");
            if (id == "a" || id == "b" || id == "c" || id == "d" || id == "e") presentationBlocks++;
            if (id == "a" || id == "b" || id == "c") Eq("&lt;", block.X, "single-pass block " + id);
            if (id == "d") Eq("&#x80;", block.X, "numeric output not reparsed");
            if (id == "e") Eq("\u20ac", block.X, "numeric presentation");
        }
        Eq("5", presentationBlocks.ToString(), "all presentation fixtures visited");
        Eq("\u20ac", Id(page.D, "f").Av("value"), "input DOM value");
        Console.WriteLine("Numeric references: checks=" + checks + "; failures=" + failures);
        return failures == 0 ? 0 : 1;
    }
}
