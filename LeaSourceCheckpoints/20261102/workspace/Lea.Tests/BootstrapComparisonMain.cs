using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Script.Serialization;
using Lea.Engine;
using Lea.Script;

internal static class BootstrapComparisonMain
{
    public static int Main(string[] args)
    {
        var process = Process.GetCurrentProcess();
        var report = new Dictionary<string, object>();
        try
        {
            var engine = new E { Jb = false };
            var page = engine.Go(args[0]);
            var changes = page.Jc;
            var cpu = process.TotalProcessorTime;
            var clock = Stopwatch.StartNew();
            using (var script = new Js(engine, page))
            {
                var result = script.Ex("if(!document.contains(document))throw Error('document containment');");
                report["ok"] = result.Ok && result.N == 0 && page.Jc == changes;
                report["status"] = result.S;
                report["message"] = result.M;
                report["elapsedMs"] = clock.ElapsedMilliseconds;
                report["cpuMs"] = (process.TotalProcessorTime - cpu).TotalMilliseconds;
            }
        }
        catch (Exception error) { report["ok"] = false; report["exception"] = error.ToString(); }
        Console.WriteLine("LEA_BOOTSTRAP_COMPARISON " + new JavaScriptSerializer().Serialize(report));
        process.Dispose();
        return (bool)report["ok"] ? 0 : 1;
    }
}
