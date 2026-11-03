using System;
using Lea.Engine;

internal static class FontRelativeMain
{
    private static S Find(P page, string id)
    {
        foreach (var block in page.Bs)
            if (block.El != null && block.El.Av("id") == id) return block.S;
        throw new Exception("Missing relative font fixture: " + id);
    }
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 1) return 2;
            var page = new E().Go(args[0]);
            string[] names = { "short-em", "short-percent", "long-em", "long-percent", "nested", "order-short", "order-long", "decimal-percent", "uppercase", "half", "default", "styled" };
            int[] expected = { 30, 30, 30, 30, 36, 30, 30, 25, 30, 10, 24, 30 };
            int failures = 0;
            for (int i = 0; i < names.Length; i++)
            {
                var value = Find(page, names[i]);
                bool passed = value.Fs == expected[i];
                if (names[i] == "styled") passed = passed && value.B && value.I && value.Ff == "Arial" && value.Lm == 150;
                if (!passed) failures++;
                Console.WriteLine("FONT_RELATIVE " + names[i] + " expected=" + expected[i] + " actual=" + value.Fs + " passed=" + passed);
            }
            foreach (string id in new[] { "negative", "zero", "oversize", "bad-number", "bad-unit", "invalid-short" })
            {
                var value = Find(page, id);
                if (value.Fs != 19 || value.Ff != "Arial" || !value.B || !value.I || value.Lh != 41) throw new Exception("Relative font rejection changed style: " + id);
            }
            Console.WriteLine("FONT_RELATIVE controls=6 cases=12 failures=" + failures);
            return failures == 0 ? 0 : 1;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 2; }
    }
}
