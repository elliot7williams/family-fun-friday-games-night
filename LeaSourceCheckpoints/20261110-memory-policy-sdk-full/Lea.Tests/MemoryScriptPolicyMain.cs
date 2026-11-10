using System;
using System.IO;
using Lea.Engine;

internal static class MemoryScriptPolicyMain
{
    private static int checks;
    private static void Check(bool ok, string name) { if (!ok) throw new Exception(name); checks++; }
    private static int Main()
    {
        try
        {
            var engine = new E { Je = true };
            var page = engine.Ht("<title>Safe memory</title><script type='text/lea'>t('AUTO_RAN');a('AUTO_ALERT');</script><p>Report</p>");
            var automaticBlocked = page.T == "Safe memory" && page.Jc == 0 && page.Ms.Count == 0;
            Console.WriteLine("OBSERVE automatic title=" + page.T + "; commands=" + page.Jc + "; alerts=" + page.Ms.Count);
            var manual = engine.Js(page, "t('MANUAL_RAN');");
            Console.WriteLine("OBSERVE manual commands=" + manual);
            engine.Vz(360, 480);
            var resizeDisabled = !page.Je;
            engine.Je = false; engine.Je = true;
            var toggleDisabled = !page.Je;
            Console.WriteLine("OBSERVE resize scripting=" + !resizeDisabled + "; toggle scripting=" + !toggleDisabled);
            Check(automaticBlocked, "memory automatic Lea commands executed");
            Check(manual == 0 && page.T == "Safe memory", "memory manual Lea commands executed");
            Check(resizeDisabled && toggleDisabled, "memory scripting metadata re-enabled");
            var reload = engine.Rl();
            Check(reload.T == "Safe memory" && reload.Jc == 0 && reload.Ms.Count == 0 && !reload.Je, "memory reload executed commands");
            Check(reload.Og.Op && !reload.Og.Lc, "memory origin changed");
            var control = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "script-control.html");
            File.WriteAllText(control, "<title>Before</title><script type='text/lea'>t('CONTROL_RAN');</script><p>Control</p>");
            var regular = engine.Go(new Uri(control));
            Check(regular.T == "CONTROL_RAN" && regular.Jc == 1 && regular.Je, "ordinary page scripting was disabled");
            Check(engine.Bk().T == "Safe memory" && !engine.At.Je, "memory history restored scripting");
            Console.WriteLine("PASS Lea memory script policy: " + checks + " checks");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
