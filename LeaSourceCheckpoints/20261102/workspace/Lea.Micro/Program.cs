using System.Net;
using System.Text.RegularExpressions;

namespace Lea.Micro;

record L(string T, Uri U);
record P(Uri U, string T, string X, List<L> Ls);

static partial class Program
{
    static readonly HttpClient H = new() { Timeout = TimeSpan.FromSeconds(10) };
    static readonly List<P> S = [];
    static int I = -1;

    static async Task Main(string[] a)
    {
        Console.Title = "Lea Micro";
        Console.WriteLine("Lea Micro: u site | g n | b | f | r | l | q");
        while (true)
        {
            Console.Write("lm> ");
            var s = Console.ReadLine();
            if (s is null || !await C(s)) return;
        }
    }

    static async Task<bool> C(string s)
    {
        var p = s.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (p.Length == 0) return true;
        var c = p[0].ToLowerInvariant();
        var v = p.Length > 1 ? p[1] : "";

        try
        {
            switch (c)
            {
                case "u": D(await O(v)); break;
                case "g": D(await O(N(int.Parse(v)).U.ToString())); break;
                case "b": if (I > 0) I--; D(Q()); break;
                case "f": if (I < S.Count - 1) I++; D(Q()); break;
                case "r": if (Q() is { } q) D(await R(q.U)); break;
                case "l": Ls(Q()); break;
                case "q": return false;
                default: Console.WriteLine("u/g/b/f/r/l/q"); break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("No: " + e.Message);
        }

        return true;
    }

    static async Task<P> O(string s)
    {
        var u = U(s);
        var x = await H.GetStringAsync(u);
        var p = A(u, x);
        if (I < S.Count - 1) S.RemoveRange(I + 1, S.Count - I - 1);
        S.Add(p);
        I = S.Count - 1;
        return p;
    }

    static async Task<P> R(Uri u)
    {
        var p = A(u, await H.GetStringAsync(u));
        S[I] = p;
        return p;
    }

    static P A(Uri u, string h)
    {
        h = J().Replace(h, " ");
        var ls = new List<L>();
        foreach (Match m in K().Matches(h))
        {
            var r = WebUtility.HtmlDecode(m.Groups[1].Value);
            var t = W(G().Replace(m.Groups[2].Value, " "));
            if (Uri.TryCreate(u, r, out var z)) ls.Add(new L(t.Length == 0 ? z.Host : M(t, 60), z));
        }

        var title = W(Regex.Match(h, "<title[^>]*>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value);
        return new P(u, title.Length == 0 ? u.Host : title, M(W(G().Replace(h, " ")), 2500), ls);
    }

    static void D(P? p)
    {
        if (p is null) return;
        Console.WriteLine();
        Console.WriteLine(p.T);
        Console.WriteLine(p.U);
        Console.WriteLine(new string('-', 56));
        Console.WriteLine(p.X);
        Ls(p);
    }

    static void Ls(P? p)
    {
        if (p is null) return;
        for (var i = 0; i < Math.Min(15, p.Ls.Count); i++) Console.WriteLine($"{i + 1}. {p.Ls[i].T}");
    }

    static P? Q() => I >= 0 && I < S.Count ? S[I] : null;
    static L N(int n) => Q()!.Ls[n - 1];
    static Uri U(string s) => Uri.TryCreate(s, UriKind.Absolute, out var u) ? u : new("https://" + s);
    static string W(string s) => Z().Replace(WebUtility.HtmlDecode(s), " ").Trim();
    static string M(string s, int n) => s.Length <= n ? s : s[..n].TrimEnd() + "...";

    [GeneratedRegex("<(script|style|noscript)[^>]*>.*?</\\1>|<!--.*?-->", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex J();

    [GeneratedRegex("<a\\s+[^>]*href\\s*=\\s*[\"']?([^\"'\\s>]+)[\"']?[^>]*>(.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex K();

    [GeneratedRegex("<[^>]+>", RegexOptions.Singleline)]
    private static partial Regex G();

    [GeneratedRegex("\\s+")]
    private static partial Regex Z();
}
