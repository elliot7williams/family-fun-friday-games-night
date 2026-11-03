using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Lea.Engine;
using Lea.Script;
using Lea.Script.Win;
using Lea.Win;
using Forms = System.Windows.Forms;

internal static class SdkScriptLocalMain
{
    private static int checks;
    private static void Check(bool value, string name)
    { if (!value) throw new Exception(name); checks++; Console.WriteLine("PASS " + name); }
    private static Xr Await(Task<Xr> task)
    {
        var stop = DateTime.UtcNow.AddSeconds(5);
        while (!task.IsCompleted && DateTime.UtcNow < stop) { Forms.Application.DoEvents(); Thread.Sleep(5); }
        if (!task.IsCompleted) throw new TimeoutException("Adapter operation deadline");
        return task.GetAwaiter().GetResult();
    }
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 1) return 2;
            var engine = new E(); var page = engine.Go(args[0]);
            Check(page.T == "SDK local script", "document loaded without automatic script");
            using (var script = new Js(engine, page))
            {
                var first = script.Ex("var retained = 7; document.title = 'SDK ' + retained; document.getElementById('result').textContent = 'Computed';");
                var timeline = typeof(Js).GetProperty("BootstrapTimeline", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (timeline != null) Console.WriteLine("BOOTSTRAP_TIMELINE " + timeline.GetValue(script, null));
                Check(first.Ok, "cold trusted execution: " + first.S + "; " + first.M);
                Check(page.T == "SDK 7", "computed title");
                Check(engine.Q(page, "#result").C[0].X == "Computed", "computed DOM text");
                Check(script.Ex("if (retained !== 7 || document.querySelector('#result') !== document.getElementById('result')) throw Error('identity');").Ok, "persistent realm and wrapper identity");
                Check(script.Ex("if (typeof System !== 'undefined' || typeof importNamespace !== 'undefined' || typeof __lea !== 'undefined' || typeof fetch !== 'undefined' || typeof document.GetType !== 'undefined') throw Error('host exposure');").Ok, "no CLR bridge or fetch exposure");
                Check(script.Ex(new string(' ', 16385)).S == "code", "source size bound");
                Check(script.Ex("let = ;").S == "js", "syntax rejected");
                Check(script.Ex("if (typeof retained !== 'undefined') throw Error('realm survived'); document.title = 'Recovered';").Ok && page.T == "Recovered", "syntax failure resets realm");
                Check(script.Ex("while (true) {}").S == "limit", "infinite loop bounded");
                Check(script.Ex("document.title = 'After limit';").Ok && page.T == "After limit", "recovery after limit");
                Check(script.Ex("var secret = 19;").Ok, "permission state setup");
                engine.Je = false;
                Check(script.Ex("document.title='Denied';").S == "off" && page.T == "After limit", "permission revocation denies mutation");
                engine.Je = true;
                Check(script.Ex("if (typeof secret !== 'undefined') throw Error('permission realm');").Ok, "permission reset drops globals");
                Exception wrongThread = null;
                var thread = new Thread(() => { try { script.Ex("1;"); } catch (Exception error) { wrongThread = error; } });
                thread.Start(); Check(thread.Join(2000) && wrongThread is InvalidOperationException, "trusted thread affinity");
            }
            using (var fresh = new Js(engine, page))
                Check(fresh.Ex("if (typeof secret !== 'undefined') throw Error('session leak');").Ok, "new session isolation");
            var adapterEngine = new E(); var adapterPage = adapterEngine.Go(args[0]);
            using (var view = new V())
            {
                Check(view.Xf == null, "view scripting adapter opt-in");
                W.On(view, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missing-worker"));
                Check(view.Xf != null, "adapter attached");
                using (var adapter = (W)view.Xf(adapterEngine, adapterPage))
                {
                    Check(Await(adapter.PgA()).Ok && adapter.At.Pid == 0, "script-free page stays dormant without worker");
                    Check(Await(adapter.PgA()).Ok && adapter.At.Pid == 0, "dormant repeat does not launch worker");
                }
                W.Off(view); Check(view.Xf == null, "adapter detached");
            }
            Console.WriteLine("PASS SDK LOCAL SCRIPT " + checks); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
