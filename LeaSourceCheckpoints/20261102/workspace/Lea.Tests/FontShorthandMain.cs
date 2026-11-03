using System;
using Lea.Engine;

internal static class FontShorthandMain
{
    private static int controls, failures, cases;
    private static S Style(P page, string id)
    {
        foreach (var block in page.Bs)
            if (block.El != null && block.El.Av("id") == id) return block.S;
        throw new Exception("Font fixture block missing: " + id);
    }
    private static void Check(P page, string id, Func<S, bool> test, bool control)
    {
        var style = Style(page, id);
        bool passed = test(style);
        if (control) { if (!passed) throw new Exception("Font control failed: " + id); controls++; }
        else { cases++; if (!passed) failures++; }
        Console.WriteLine("FONT_CASE " + id + " passed=" + passed + " size=" + style.Fs + " family=" + style.Ff + " bold=" + style.B + " italic=" + style.I + " line=" + style.Lh + " multiple=" + style.Lm);
    }
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 1) return 2;
            var page = new E().Go(args[0]);
            Check(page, "control", s => s.Fs == 24 && s.Ff == "Arial" && s.B && s.I && s.Lh == 36, true);
            Check(page, "invalid", s => s.Fs == 19 && s.Ff == "Arial", true);
            foreach (string id in new[] { "duplicate-style", "duplicate-weight", "bad-line", "empty-line", "trailing-family", "bad-unit", "bad-family", "oversize", "normal-overflow", "extra-slash" })
                Check(page, id, s => s.Fs == 19 && s.Ff == "Arial" && s.B && s.I && s.Lh == 0 && s.Lm == 200, true);
            Check(page, "plain", s => s.Fs == 24 && s.Ff == "Arial", false);
            Check(page, "styled", s => s.Fs == 22 && s.Ff == "Arial" && s.B && s.I, false);
            Check(page, "pixels", s => s.Fs == 24 && s.Lh == 36 && s.Lm == 0, false);
            Check(page, "multiple", s => s.Fs == 24 && s.Lh == 0 && s.Lm == 150, false);
            Check(page, "quoted", s => s.Fs == 24 && s.Ff == "Times New Roman|s", false);
            Check(page, "reset", s => s.Fs == 24 && !s.B && !s.I && s.Lh == 0 && s.Lm == 0, false);
            Check(page, "later", s => s.Fs == 30 && s.Ff == "Arial", false);
            Check(page, "earlier", s => s.Fs == 24 && s.Ff == "Arial", false);
            Check(page, "important", s => s.Fs == 24 && s.Ff == "Arial", false);
            Check(page, "important-long", s => s.Fs == 30 && s.Ff == "Arial", false);
            Check(page, "sheet", s => s.Fs == 24 && s.Ff == "Arial", false);
            Check(page, "variable", s => s.Fs == 22 && s.Ff == "Arial" && s.B && s.I, false);
            Check(page, "inherited", s => s.Fs == 24 && s.Ff == "Arial" && s.B && s.I && s.Lh == 36, false);
            Check(page, "unset", s => s.Fs == 24 && s.Ff == "Arial" && s.B && s.I && s.Lh == 36, false);
            Console.WriteLine("FONT_SHORTHAND controls=" + controls + " cases=" + cases + " failures=" + failures);
            return failures == 0 ? 0 : 1;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 2; }
    }
}
