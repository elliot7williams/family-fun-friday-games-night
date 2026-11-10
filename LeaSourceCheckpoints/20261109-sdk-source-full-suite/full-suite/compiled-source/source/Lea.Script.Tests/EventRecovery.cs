using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using Lea.Engine;
using Lea.Script;

namespace Lea.Script.Tests
{
    internal static class EventRecovery
    {
        internal static void Run(string fixture, string reportPath)
        {
            reportPath = Lea.Diagnostics.WorkerObservation.ValidateReport(reportPath);
            var rows = new List<object>();
            var clock = Stopwatch.StartNew();
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            Exception failure = null;
            try
            {
                using (var script = new Jp(engine, page) { Tm = 5000 })
                {
                    Jr Code(string code, int cycle, string stage)
                    {
                        var before = clock.ElapsedMilliseconds;
                        var result = script.Ex(code);
                        rows.Add(new { cycle, stage, beganMs = before, elapsedMs = clock.ElapsedMilliseconds - before,
                            status = result.S, changes = result.N, message = result.M, startup = script.Sd,
                            pid = script.Pid, isolated = script.Ac, guards = script.Mg,
                            noLoopback = script.Ne, networkDenied = script.Nd, fileDenied = script.Fd });
                        return result;
                    }
                    for (var cycle = 0; cycle < 6; cycle++)
                    {
                        var setup = Code("if(typeof once!=='undefined')throw Error('old realm');var once=0;for(var i=0;i<128;i++)window.addEventListener('click',function(){});", cycle, "listeners");
                        Program.Check(setup.Ok && script.Ac && script.Ne && script.Nd && script.Fd && script.Mg == 15,
                            "event recovery: fresh realm reaches listener ceiling with unchanged isolation; " + setup.S + "; " + setup.M);
                        var dropped = Code("window.onclick=function(){};", cycle, "ceiling");
                        Program.Check(dropped.S == "js" && dropped.N == 0 && script.Pid == 0,
                            "event recovery: DOM0 listener overflow retires the exact realm");
                        var recovered = Code("if(typeof once!=='undefined')throw Error('old realm');document.title='fresh';", cycle, "recovery");
                        Program.Check(recovered.Ok && page.T == "fresh" && recovered.N == 1 && script.Pid != 0,
                            "event recovery: fresh realm executes after listener overflow; " + recovered.S + "; " + recovered.M);
                        var reset = Code("throw Error('reset');", cycle, "reset");
                        Program.Check(reset.S == "js" && reset.N == 0 && script.Pid == 0,
                            "event recovery: explicit error retires recovered worker before the next cycle");
                    }
                    Program.Check(script.Tm == 5000 && script.Cp == 2500 && script.Mm == 134217728,
                        "event recovery: six cycles preserve page/CPU/memory budgets");
                }
            }
            catch (Exception error) { failure = error; }
            finally
            {
                using (var output = new FileStream(reportPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
                using (var writer = new StreamWriter(output, new UTF8Encoding(false)))
                    writer.Write(new JavaScriptSerializer { MaxJsonLength = 1024 * 1024 }.Serialize(new {
                        kind = "event-realm-recovery", passed = failure == null, error = failure == null ? "" : failure.ToString(),
                        cycles = 6, calls = rows, elapsedMs = clock.ElapsedMilliseconds, startupMs = 5000,
                        executionMs = 5000, cpuRate = 2500, memoryBytes = 134217728, retries = 0,
                        fullSuiteGate = false, productionRuntimeChanged = false
                    }));
            }
            if (failure != null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }
}
