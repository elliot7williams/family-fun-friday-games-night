using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Lea.Engine;
using Lea.Script;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class FrameProgress
    {
        internal static void Run(string fixture)
        {
            var previous = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE");
            try
            {
                foreach (var trace in new[] { false, true })
                {
                    Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", trace ? "1" : null);
                    Sample(fixture, trace);
                }
            }
            finally { Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", previous); }
        }

        private static void Sample(string fixture, bool trace)
        {
            var engine = new E();
            var page = engine.Go(fixture);
            using (var script = new Jp(engine, page))
            {
                Check(script.Tm == 2000 && script.Cp == 2500 && script.Mm == 134217728,
                    "first-command diagnostic preserves default budgets: " + trace);
                var first = script.Ex("var privateFrameCanary=17;document.title='private-frame-canary';setTimeout(function(){document.title='timer-frame-canary';throw new Error('frame failure');});");
                Check(first.Ok && page.T == "private-frame-canary" && script.Tn == 1,
                    "fresh worker executes and schedules the first command: " + trace + "; " + first.S + "; " + first.M + "; " + script.Sd);
                Check(script.Ac && script.Ne && script.Nd && script.Fd && script.Mg == 15,
                    "first-command diagnostic retains verified isolation: " + trace);
                var endpoint = Endpoint(script);
                var pid = script.Pid;
                Wait(endpoint, trace ? "frame end: i=1;" : "startup ready");
                var stderr = endpoint.E;
                var probes = stderr.IndexOf("startup probes", StringComparison.Ordinal);
                var closed = stderr.IndexOf("startup closed", StringComparison.Ordinal);
                var hello = stderr.IndexOf("startup hello", StringComparison.Ordinal);
                var ready = stderr.IndexOf("startup ready", StringComparison.Ordinal);
                Check(probes >= 0 && closed > probes && hello > closed && ready > hello,
                    "probe disposal completes before hello and ready: " + trace + "; " + stderr);
                if (trace)
                {
                    var invoke = stderr.IndexOf("frame invoke: i=0;", StringComparison.Ordinal);
                    var enter = stderr.IndexOf("frame enter: i=0;", StringComparison.Ordinal);
                    var read = stderr.IndexOf("frame read: i=0;", StringComparison.Ordinal);
                    var exec = stderr.IndexOf("frame exec: i=1;", StringComparison.Ordinal);
                    var call = stderr.IndexOf("frame call: i=1; r=1;", StringComparison.Ordinal);
                    var reply = stderr.IndexOf("frame reply: i=1; r=1;", StringComparison.Ordinal);
                    var tasks = stderr.IndexOf("frame tasks: i=1;", StringComparison.Ordinal);
                    var end = stderr.IndexOf("frame end: i=1;", StringComparison.Ordinal);
                    Check(invoke > ready && enter > invoke && read > enter && exec > read && call > exec &&
                        reply > call && tasks > reply && end > tasks,
                        "actual native trace distinguishes invocation, entry, receipt, bridge replies and completion; " + stderr);
                }
                else Check(!stderr.Contains("frame "), "ordinary commands do not emit frame progress");
                Check(!stderr.Contains("private-frame-canary") && !stderr.Contains("privateFrameCanary"),
                    "progress trace never includes page source, titles or bridge payloads: " + trace);

                var failed = script.Tk();
                Check(failed.S == "js" && failed.M.Contains("frame failure") && page.T == "timer-frame-canary" && script.Tn == 0 &&
                    script.Pid == 0 && Exited(pid), "timer failure retires the exact worker and pending timer state: " + trace + "; " + failed.M);
                if (trace)
                {
                    Wait(endpoint, "frame error: i=2;");
                    Check(endpoint.E.Contains("frame timer: i=2;") && endpoint.E.Contains("frame error: i=2;"),
                        "failed timer retains receipt and error-boundary evidence");
                }
                var recovery = script.Ex("if(typeof privateFrameCanary!=='undefined')throw new Error('old realm');for(var n=0;n<12;n++)document.title=String(n);setTimeout(function(){document.title='recovered timer';});");
                Check(recovery.Ok && script.Tn == 1 && script.Pid != 0 && script.Pid != pid,
                    "post-error first command uses a new realm and native worker: " + trace + "; " + recovery.S + "; " + recovery.M + "; " + script.Sd);
                var recovered = Endpoint(script);
                var recoveredPid = script.Pid;
                var timer = script.Tk();
                Check(timer.Ok && script.Tn == 0 && page.T == "recovered timer",
                    "fresh-realm timer executes after recovery: " + trace + "; " + timer.S + "; " + timer.M);
                if (trace)
                {
                    Wait(recovered, "frame end: i=4;");
                    Check(recovered.E.Contains("frame exec: i=3;") && recovered.E.Contains("frame timer: i=4;"),
                        "recovery traces retain parent command sequences instead of resetting them");
                    Check(recovered.E.Contains("frame call: i=3; r=4;") && recovered.E.Contains("frame reply: i=3; r=4;") &&
                        !recovered.E.Contains("frame call: i=3; r=5;") && !recovered.E.Contains("frame reply: i=3; r=5;"),
                        "bridge progress limits optional logging after the first four requests");
                }
                script.Dispose();
                Check(script.Pid == 0 && Exited(recoveredPid), "diagnostic disposes its recovered worker: " + trace);
            }
        }

        private static Rp Endpoint(Jp script)
        { return (Rp)typeof(Jp).GetField("worker", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(script); }

        private static void Wait(Rp endpoint, string marker)
        {
            if (!SpinWait.SpinUntil(() => endpoint.E.Contains(marker), 2000))
                throw new Exception("Native stderr did not drain: " + marker + "; " + endpoint.E);
        }

        private static bool Exited(int pid)
        { try { using (var process = Process.GetProcessById(pid)) return process.WaitForExit(1000); } catch (ArgumentException) { return true; } }

        private static void Check(bool condition, string message) { Program.Check(condition, message); }
    }
}
