using System;
using Lea.Engine;

internal static class FontInitialMain
{
    private static S Find(P page, string id)
    {
        foreach (var block in page.Bs)
            if (block.El != null && block.El.Av("id") == id) return block.S;
        throw new Exception("Missing font initial fixture: " + id);
    }
    private static bool Initial(S value)
    { return value.Fs == 16 && value.Ff == "" && !value.B && !value.I && value.Lh == 0 && value.Lm == 0; }
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 1) return 2;
            var page = new E().Go(args[0]);
            int failures = 0;
            foreach (string id in new[] { "reset", "inherited", "important", "variable", "case" })
            {
                bool passed = Initial(Find(page, id));
                if (!passed) failures++;
                Console.WriteLine("FONT_INITIAL " + id + " passed=" + passed);
            }
            var later = Find(page, "later");
            if (later.Fs != 25 || later.Ff != "" || later.B || later.I || later.Lh != 0 || later.Lm != 0) failures++;
            var protectedSize = Find(page, "protected");
            if (protectedSize.Fs != 25 || protectedSize.Ff != "" || protectedSize.B || protectedSize.I || protectedSize.Lh != 0 || protectedSize.Lm != 0) failures++;
            foreach (string id in new[] { "invalid-size", "invalid-family", "invalid-prefix", "invalid-variable" })
            {
                var value = Find(page, id);
                if (value.Fs != 29 || value.Ff != "Arial" || !value.B || !value.I || value.Lh != 41) throw new Exception("Invalid initial form changed style: " + id);
            }
            if (Find(page, "reset").F != Find(page, "color-control").F) throw new Exception("Font reset changed unrelated color");
            Console.WriteLine("FONT_INITIAL controls=5 cases=7 failures=" + failures);
            return failures == 0 ? 0 : 1;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 2; }
    }
}
