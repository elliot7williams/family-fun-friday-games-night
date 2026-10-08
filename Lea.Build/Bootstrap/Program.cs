using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Acornima;

try
{
    if (args.Length != 0)
        args[0] = args[0] switch { "-c" => "--check", "-w" => "--write", "-t" => "--self-test", _ => args[0] };
    if (args.Length == 1 && args[0] == "--self-test")
    {
        var samples = new[]
        {
            "function f(){ return\n{ value: 1 }; }",
            "var x=8/2/2; var r=/[ /]+/g; x = x + +x - -x;",
            "var n=2; var s=`line\n${n + 1}`;",
            "var value=1; var o={value, get x(){return value;}, ['y']:2, m(a){return a;}};",
            "for(let i=0;i<2;i++){ i; } // keep the statement boundary\n1;",
            "'use strict'; { const hidden=1; (function dispatch(){ return hidden; }); }",
            "var escaped='\\u0061'; var unicode='\u00e9'; /* comment */ escaped + unicode;"
        };
        foreach (var sample in samples) Compact(sample);
        if (new Parser().ParseScript("var x=1;").ToJson() == new Parser().ParseScript("var x=2;").ToJson())
            throw new InvalidOperationException("AST comparison failed to distinguish a changed literal.");
        Console.WriteLine("PASS bootstrap generator: 7 round trips and changed-literal control.");
        return 0;
    }
    if (args.Length != 2 || (args[0] != "--check" && args[0] != "--write"))
        throw new ArgumentException("Use -c/--check or -w/--write followed by the Lea repository directory, or -t/--self-test.");

    var directory = Path.Combine(Path.GetFullPath(args[1]), "Lea.Script");
    var input = File.ReadAllBytes(Path.Combine(directory, "Dom.js"));
    var utf8 = new UTF8Encoding(false, true);
    if (input.AsSpan().StartsWith(new byte[] { 0xef, 0xbb, 0xbf }))
        throw new InvalidOperationException("Dom.js must be UTF-8 without a BOM.");
    var output = utf8.GetBytes(Compact(utf8.GetString(input)) + "\r\n");
    var properties = utf8.GetBytes(
        "<Project>\r\n  <PropertyGroup>\r\n" +
        "    <LeaDomSourceHash>" + Hash(input) + "</LeaDomSourceHash>\r\n" +
        "    <LeaDomCompactHash>" + Hash(output) + "</LeaDomCompactHash>\r\n" +
        "  </PropertyGroup>\r\n</Project>\r\n");
    Artifact("Dom.compact.js", output);
    Artifact("Dom.g.props", properties);
    Console.WriteLine("Bootstrap " + args[0] + ": " + input.Length + " -> " + output.Length +
        " UTF-8 bytes; " + (100.0 * (input.Length - output.Length) / input.Length).ToString("F1", CultureInfo.InvariantCulture) +
        "% smaller; matching AST JSON; source SHA256=" + Hash(input) + "; compact SHA256=" + Hash(output));
    return 0;

    void Artifact(string name, byte[] expected)
    {
        var path = Path.Combine(directory, name);
        if (args[0] == "--write") File.WriteAllBytes(path, expected);
        else if (!File.Exists(path) || !File.ReadAllBytes(path).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException(name + " is stale. Regenerate with --write and review the source change.");
    }
}
catch (Exception error)
{
    Console.Error.WriteLine(error);
    return 1;
}

static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));

static string Compact(string source)
{
    var ast = new Parser().ParseScript(source);
    var compact = ast.ToJavaScript();
    var reparsed = new Parser().ParseScript(compact);
    // Default JSON excludes locations but retains names, literals, directives and statement order.
    if (ast.ToJson() != reparsed.ToJson())
        throw new InvalidOperationException("Generated bootstrap changed the parsed structure.");
    if (reparsed.ToJavaScript() != compact)
        throw new InvalidOperationException("Compact generation is not a fixed point.");
    return compact;
}
