using System;
using System.Reflection;
using System.Threading;
using Jint;
using Lea.Engine;
using Lea.Script;

namespace Lea.Script.Tests
{
    internal static class BudgetPhases
    {
        internal static void Run(string fixture)
        {
            foreach (var target in new[] { 1, 2 })
            {
                var engine = new E { Jb = false };
                var page = engine.Go(fixture);
                var parses = 0;
                using (var script = new Js(engine, page, configure =>
                {
                    var vm = new Jint.Engine(configure);
                    // The real one-second constraint expires inside the selected evaluation.
                    vm.Debugger.BeforeEvaluate += (sender, ast) => { if (++parses == target) Thread.Sleep(1100); };
                    return vm;
                }))
                {
                    var result = script.Ex("for(var i=0;i<100;i++){};document.title='must not run';");
                    var phase = target == 1 ? "bootstrap" : "script";
                    Check(result.S == "limit" && result.M == "Script budget exceeded. phase=" + phase + "; reason=TimeoutException",
                        "real deadline identifies " + phase + " and TimeoutException");
                    Check(result.N == 0 && page.T != "must not run", "expired " + phase + " cannot report page writes");
                    Retired(script, "deadline " + phase);
                    Check(script.Ex("if(typeof i!=='undefined')throw Error('old realm');document.title='recovered';").Ok && page.T == "recovered",
                        "deadline " + phase + " recovery creates a clean realm without another delay");
                }
            }

            var host = new E { Jb = false };
            var bound = host.Go(fixture);
            using (var script = new Js(host, bound))
            {
                Check(script.Ex("document.title='phase setup';").Ok, "ordinary trusted setup keeps its existing budget");
                Limited(script.Ex("while(true){}"), "script");
                Retired(script, "script loop");
                var microtask = script.Ex("queueMicrotask(function(){while(true){}});");
                Limited(microtask, "script");
                Retired(script, "microtask drained by Execute");
                Check(script.Ex("setTimeout(function(){while(true){};});").Ok, "timer phase setup");
                Limited(script.Tk(), "timer");
                Retired(script, "timer loop");
                Check(script.Ex("window.addEventListener('load',function(){while(true){}});").Ok, "event phase setup");
                Limited(script.Ev(null, "load"), "event");
                Retired(script, "event loop");
                Check(script.Ex("throw Error('ordinary script error');").M == "ordinary script error", "ordinary JavaScript error text remains unchanged");
                Retired(script, "ordinary error");
                Check(script.Ex("document.title='final recovery';").Ok && bound.T == "final recovery", "all diagnostic failure paths permit a fresh successful realm");
            }

            var fault = new CheckpointFault();
            using (var script = new Js(host, bound, configure => new Jint.Engine(options =>
            {
                configure(options);
                options.Constraint(fault);
            })))
            {
                var result = script.Ex("1;");
                Limited(result, "checkpoint");
                Check(!result.M.Contains("private checkpoint detail"), "budget diagnostics omit raw exception detail");
                Retired(script, "injected checkpoint fault");
                Check(script.Ex("document.title='checkpoint recovery';").Ok && bound.T == "checkpoint recovery",
                    "checkpoint fault permits a fresh successful realm");
            }
        }

        private static void Limited(Jr result, string phase)
        {
            var reasons = new[] { "StatementsCountOverflowException", "TimeoutException" };
            Check(result.S == "limit" && Array.Exists(reasons, reason => result.M == "Script budget exceeded. phase=" + phase + "; reason=" + reason),
                "budget failure identifies " + phase + " and its type; " + result.S + "; " + result.M);
            Check(result.M.Length <= 512, phase + " diagnostic remains bounded");
        }

        private static void Retired(Js script, string label)
        {
            var vm = typeof(Js).GetField("vm", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(script);
            var running = (bool)typeof(Js).GetField("running", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(script);
            Check(vm == null && script.Tn == 0 && !running, label + " still retires the realm, timers, and running state");
        }

        private static void Check(bool condition, string name) { Program.Check(condition, "trusted budget phase: " + name); }

        private sealed class CheckpointFault : Jint.Constraint
        {
            private int resets;
            public override void Check() { }
            public override void Reset()
            {
                // Pinned Jint resets at entry/exit: bootstrap, page, then checkpoint entry.
                if (++resets == 5) throw new TimeoutException("private checkpoint detail");
            }
        }
    }
}
