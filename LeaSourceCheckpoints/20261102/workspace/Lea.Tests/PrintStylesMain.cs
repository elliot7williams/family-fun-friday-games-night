using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lea.Engine;

internal static class PrintStylesMain
{
    private static void Check(bool value, string name) { if (!value) throw new Exception(name); Console.WriteLine("PASS " + name); }
    private static void Reject(Action action, string name)
    {
        bool denied = false;
        try { action(); } catch (ArgumentException) { denied = true; } catch (InvalidOperationException) { denied = true; } catch (NotSupportedException) { denied = true; }
        Check(denied, name);
    }
    [STAThread]
    private static int Main(string[] args)
    { return Run(args); }
    internal static int Run(string[] args)
    {
        try
        {
            if (PrintDocumentMain.Run(args) != 0) return 1;
            var engine = new E { Je = false };
            var page = engine.Go(Path.Combine(Path.GetDirectoryName(args[0]), "print-styles.html"));
            var blocks = page.Bs.ToArray(); var styles = page.Bs.Select(b => b.S).ToArray();
            int width = page.Vw, height = page.Vh, commands = page.Jc; string text = page.X;
            var print = engine.Pst(page, 800, 1000);
            Func<string, S> style = id => print[engine.Id(page, id)];
            Check(print.Count > 20 && print.ContainsKey(page.D), "print styles complete node map");
            Check(style("mode").F == "green" || style("mode").F == "#008000", "print styles print media cascade");
            Check(style("cascade").F == "red" || style("cascade").F == "#ff0000", "print styles important beats inline normal");
            Check(style("screenOnly").D == "none" && style("printOnly").D != "none", "print styles print and screen visibility");
            Check(style("large").Fs == 24 && style("cascade").Fs == 18, "print styles font sizes");
            Check(style("notScreen").B && !style("notPrint").I, "print styles negated media");
            Check(style("composite").I, "print styles recursive composite media");
            Check(style("sized").Fs == 30 && !style("oriented").I, "print styles portrait width media");
            Check(style("viewport").Fs == 16, "print styles isolated viewport units");
            Check(style("heightfont").Fs == 20 && style("minfont").Fs == 16 && style("maxfont").Fs == 20, "print styles vh vmin vmax fonts");
            Check(style("calcfont").Fs == 20 && style("decimalfont").Fs == 20, "print styles calculated decimal and case-insensitive fonts");
            Check(style("badfont").Fs == 16 && style("bigfont").Fs == 16, "print styles invalid viewport font fallback");
            Check(style("visibility").Vi == "h" && style("override").Vi != "h", "print styles inherited visibility override");
            Check(style("opaque").Op == 0, "print styles opacity retained");
            Check(style("closed").D == "none" && style("summary").D != "none", "print styles closed disclosure state");
            var narrow = engine.Pst(page, 600, 1000);
            Check(narrow[engine.Id(page, "sized")].Fs != 30 && !narrow[engine.Id(page, "composite")].I, "print styles width and composite negatives");
            var landscape = engine.Pst(page, 1200, 800);
            Check(landscape[engine.Id(page, "oriented")].I, "print styles landscape media");
            Check(landscape[engine.Id(page, "viewport")].Fs == 24 && narrow[engine.Id(page, "viewport")].Fs == 12, "print styles distinct viewport font widths");
            Check(page.Bs.First(b => b.El == engine.Id(page, "viewport")).S.Fs == (int)Math.Round(page.Vw * .02, MidpointRounding.AwayFromZero), "print styles screen viewport fonts also supported");
            Check(page.Vw == width && page.Vh == height && page.Jc == commands && page.X == text && Object.ReferenceEquals(engine.At, page), "print styles live page state unchanged");
            Check(blocks.SequenceEqual(page.Bs) && styles.SequenceEqual(page.Bs.Select(b => b.S)), "print styles live paint objects unchanged");
            Check(!engine.Mq("print") && engine.Mq("screen"), "print styles screen media default unchanged");
            Reject(() => ((IDictionary<N, S>)print).Clear(), "print styles map readonly");
            Reject(() => engine.Pst(null, 800, 1000), "print styles null page");
            Reject(() => new E().Pst(page, 800, 1000), "print styles foreign page");
            Reject(() => engine.Pst(page, 0, 1000), "print styles empty geometry");
            Reject(() => engine.Pst(page, 9000, 1000), "print styles viewport cap");
            var first = page.D.C[0]; page.D.C.Add(first);
            try { Reject(() => engine.Pst(page, 800, 1000), "print styles duplicate tree rejected"); }
            finally { page.D.C.RemoveAt(page.D.C.Count - 1); }
            Check(engine.Pst(page, 800, 1000)[engine.Id(page, "large")].Fs == 24, "print styles query recovers after rejected tree");
            Console.WriteLine("PASS Lea print styles and foundation"); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
