using System;
using System.IO;
using System.Reflection;
using System.Text;
using Lea.Engine;

internal static class NamedReferences
{
    private static int checks, failures;
    private static readonly MethodInfo Tree = typeof(E).GetMethod("Tree", BindingFlags.NonPublic | BindingFlags.Static);
    private static N Find(N node, string id)
    {
        if (node.Av("id") == id) return node;
        foreach (var child in node.C) { var found = Find(child, id); if (found != null) return found; }
        return null;
    }
    private static string Text(N node)
    {
        if (node.K == "#text") return node.X;
        var result = new StringBuilder(); foreach (var child in node.C) result.Append(Text(child)); return result.ToString();
    }
    private static void Eq(string expected, string actual, string name)
    {
        checks++;
        if (expected == actual) return;
        failures++;
        if (failures <= 25) Console.WriteLine("FAIL " + name + "; expected=" + BitConverter.ToString(Encoding.Unicode.GetBytes(expected)) + "; actual=" + BitConverter.ToString(Encoding.Unicode.GetBytes(actual ?? "")));
    }
    private static void Case(string input, string text, string attribute)
    {
        var tree = (N)Tree.Invoke(null, new object[] { "<p id='t' data-x='" + input + "'>" + input + "</p>" });
        var node = Find(tree, "t");
        Eq(text, Text(node), "text " + input); Eq(attribute, node.Av("data-x"), "attribute " + input);
    }
    public static int Main(string[] args)
    {
        int entries = 0;
        foreach (string line in File.ReadAllLines(args[1]))
        {
            var fields = line.Split('\t');
            string name = fields[0], expected = Encoding.UTF8.GetString(Convert.FromBase64String(fields[1]));
            entries++;
            Case(name, expected, expected);
            if (!name.EndsWith(";", StringComparison.Ordinal))
                foreach (string suffix in new[] { "Z9", "=0", "!" })
                    Case(name + suffix, expected + suffix, suffix == "!" ? expected + suffix : name + suffix);
        }
        Eq("2231", entries.ToString(), "complete standard mapping inventory");
        Case("&notit;", "\u00acit;", "&notit;");
        Case("&notin;", "\u2209", "\u2209");
        Case("&NotEqualTilde;", "\u2242\u0338", "\u2242\u0338");
        Case("&fjlig;", "fj", "fj");
        Case("&bogus;&;&&amp;", "&bogus;&;&&", "&bogus;&;&&");
        Case("&amp;copy;&#38;copy;", "&copy;&copy;", "&copy;&copy;");
        Case("&COPY;&copy;&Copy;", "\u00a9\u00a9&Copy;", "\u00a9\u00a9&Copy;");
        Case("&" + new string('Z', 10000) + ";", "&" + new string('Z', 10000) + ";", "&" + new string('Z', 10000) + ";");
        var raw = (N)Tree.Invoke(null, new object[] { "<script id='s'>&copy;</script><style id='c'>&copy;</style><textarea id='r'>&NotEqualTilde;</textarea><input id='i' value=&copyZ>" });
        Eq("&copy;", Text(Find(raw, "s")), "script raw text");
        Eq("&copy;", Text(Find(raw, "c")), "style raw text");
        Eq("\u2242\u0338", Text(Find(raw, "r")), "textarea RCDATA");
        Eq("&copyZ", Find(raw, "i").Av("value"), "unquoted ambiguity");
        Console.WriteLine("Named references: checks=" + checks + "; failures=" + failures);
        return failures == 0 ? 0 : 1;
    }
}
