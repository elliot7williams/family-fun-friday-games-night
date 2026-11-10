using System;
using System.Threading.Tasks;
using Lea.Engine;

internal static class MemoryDocumentMain
{
    private static int checks;
    private static void Check(bool ok, string name) { if (!ok) throw new Exception(name); checks++; }
    private static int Main()
    {
        try
        {
            var engine = new E();
            var first = engine.Ht("<title>First</title><style>p{color:red}</style><p>Memory report</p>");
            Check(engine.At == first && first.T == "First" && first.X.Contains("Memory report"), "memory content");
            Check(first.U.Scheme == "about" && !first.U.IsFile && first.Bu == first.U, "non-file origin");
            Check(!first.Je && !first.Ca, "non-script/non-cache document");
            Check(first.Og.Op && !first.Og.Lc, "opaque nonlocal origin");
            var second = engine.Ht("<title>Second</title><p>Other</p>");
            Check(first.U != second.U, "unique document identity");
            Check(engine.Bk() == first && engine.Fw() == second, "history retained memory pages");
            var reloaded = engine.Rl();
            Check(reloaded.T == "Second" && reloaded.U == second.U && reloaded.H == second.H, "memory reload");
            Check(engine.RlA().GetAwaiter().GetResult().T == "Second", "async memory reload");
            var safe = engine.Ht("<base href='file:///C:/secret/'><link rel='stylesheet' href='file:///C:/nonexistent-lea-memory.css'><p>Safe</p>");
            Check(safe.Bu.Scheme == "about" && safe.X.Contains("Safe"), "no inherited base or external stylesheet read");
            var before = engine.At;
            engine.Ng = uri => false;
            var blocked = false;
            try { engine.Ht("<p>Blocked</p>"); } catch (Er error) { blocked = error.K == "blocked"; }
            Check(blocked && engine.At == before, "navigation policy denial is atomic");
            engine.Ng = null;
            var rejected = false;
            try { engine.Ht(null); } catch (ArgumentNullException) { rejected = true; }
            Check(rejected && engine.At == before, "null rejection is atomic");
            rejected = false;
            try { engine.Ht(new string('x', 4 * 1024 * 1024 + 1)); } catch (Er error) { rejected = error.K == "large"; }
            Check(rejected && engine.At == before && !engine.Busy, "size limit and input retirement");
            Console.WriteLine("PASS Lea memory documents: " + checks + " model checks");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
