using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Jint;
using Lea.Engine;
using Lea.Script;

namespace Lea.Script.Tests
{
    internal static class BootstrapCache
    {
        private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
        private static readonly FieldInfo Vm = typeof(Js).GetField("vm", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static void Contract()
        {
            var field = typeof(Js).GetField("prepared", PrivateStatic);
            Program.Check(field != null && field.IsInitOnly && field.FieldType == typeof(Lazy<Prepared<Acornima.Ast.Script>>),
                "trusted bootstrap owns one read-only lazy prepared holder");
            var cache = (Lazy<Prepared<Acornima.Ast.Script>>)field.GetValue(null);
            Program.Check(!cache.IsValueCreated, "prepared bootstrap is not parsed merely by loading Js metadata");
            var options = (ScriptPreparationOptions)typeof(Js).GetField("preparation", PrivateStatic).GetValue(null);
            Program.Check(options.StaticAnalysis && !options.FoldConstants && !options.CollectReferencedGlobals &&
                options.ParsingOptions.CompileRegex == false && options.ParsingOptions.RegexTimeout == TimeSpan.FromMilliseconds(100),
                "prepared bootstrap uses explicit analysis and interpreted 100-ms regexes without constant folding");
            var first = cache.Value;
            Program.Check(first.IsValid && first.Program.Strict && ReferenceEquals(first.Program, cache.Value.Program),
                "prepared bootstrap publishes one strict syntax tree for subsequent realms");
            var start = typeof(Js).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
            var evaluate = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Where(VmOptions.Calls(start),
                item => item.DeclaringType == typeof(Jint.Engine) && item.Name == "Evaluate"));
            Program.Check(evaluate.Length == 1 && evaluate[0].GetParameters().Length == 1 &&
                evaluate[0].GetParameters()[0].ParameterType == typeof(Prepared<Acornima.Ast.Script>).MakeByRefType(),
                "trusted compiled bootstrap evaluates the prepared tree instead of the source string");
        }

        internal static void Cold(string fixture)
        {
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            var code = engine.Q(page, "script").C[0].X;
            using (var process = Process.GetCurrentProcess())
            {
                var cpu = process.TotalProcessorTime;
                var clock = Stopwatch.StartNew();
                Jr result;
                using (var script = new Js(engine, page)) result = script.Ex(code);
                clock.Stop();
                Console.WriteLine("Trusted first call: " + result.S + "; changes=" + result.N + "; wall=" +
                    clock.ElapsedMilliseconds + "; cpu=" + (long)(process.TotalProcessorTime - cpu).TotalMilliseconds +
                    " ms; " + result.M);
                Program.Check(result.Ok && result.N == 6 && page.T == "Lea 7",
                    "fresh-process trusted first call completes the unmodified runtime fixture; " + result.S + "; " + result.M);
            }
        }

        internal static void Run(string fixture)
        {
            var cache = (Lazy<Prepared<Acornima.Ast.Script>>)typeof(Js).GetField("prepared", PrivateStatic).GetValue(null);
            var offEngine = new E { Jb = false, Je = false };
            using (var off = new Js(offEngine, offEngine.Go(fixture)))
                Program.Check(off.Ex("1;").S == "off" && !cache.IsValueCreated, "disabled scripting does not prepare the fixed bootstrap");
            object a = null, b = null;
            Func<Action<Options>, Jint.Engine> Factory(Action<object> observed)
            {
                return configure =>
                {
                    var vm = new Jint.Engine(configure);
                    var first = true;
                    vm.Debugger.BeforeEvaluate += (sender, ast) => { if (first) { first = false; observed(ast); } };
                    return vm;
                };
            }
            var ea = new E { Jb = false }; var pa = ea.Go(fixture);
            var eb = new E { Jb = false }; var pb = eb.Go(fixture);
            using (var sa = new Js(ea, pa, Factory(ast => a = ast)))
            using (var sb = new Js(eb, pb, Factory(ast => b = ast)))
            {
                Code(sa, "var retained=document.getElementById('target');retained.classList.add('alpha');document.title='alpha';globalThis.privateMark=13;", "first realm uses its bound model");
                Code(sb, "if(typeof privateMark!=='undefined')throw Error('global leaked');document.getElementById('target').classList.add('beta');document.title='beta';", "second realm has independent globals and bridge");
                Program.Check(ReferenceEquals(a, b) && ReferenceEquals(a, cache.Value.Program), "both realms evaluate the identical prepared syntax tree");
                Program.Check(pa.T == "alpha" && pb.T == "beta" && ea.Id(pa, "target").Av("class") == "alpha" && eb.Id(pb, "target").Av("class") == "beta",
                    "shared syntax does not share titles nodes or token state");
                Code(sa, "retained.classList.add('retained');globalThis.__lea=()=>{throw Error('fake bridge');};let call=43;document.title='alpha retained';", "private closures survive page collisions");
                Code(sb, "if(typeof call!=='undefined'||typeof __lea!=='undefined'||typeof privateMark!=='undefined')throw Error('binding leaked');document.title='beta retained';", "page collisions remain confined to their realm");
                Program.Check(pa.T == "alpha retained" && pb.T == "beta retained", "both models remain independently writable");
                var limited = sa.Ex("while(true){}");
                Program.Check(limited.S == "limit" && limited.N == 0, "prepared bootstrap does not remove the statement limit");
                Code(sa, "if(typeof call!=='undefined'||typeof __lea!=='undefined'||typeof privateMark!=='undefined')throw Error('reset leaked');document.title='fresh alpha';", "reset creates fresh globals from the same prepared tree");
                Program.Check(pa.T == "fresh alpha" && pb.T == "beta retained" && ReferenceEquals(a, cache.Value.Program), "reset preserves tree identity and the other realm's model");
            }
            var errors = new Exception[2];
            using (var start = new ManualResetEvent(false))
            {
                var threads = new Thread[2];
                for (var index = 0; index < threads.Length; index++)
                {
                    var slot = index;
                    threads[index] = new Thread(() =>
                    {
                        try
                        {
                            var engine = new E { Jb = false }; var page = engine.Go(fixture);
                            using (var script = new Js(engine, page))
                            {
                                if (!start.WaitOne(5000)) throw new InvalidOperationException("Concurrent cache start barrier exceeded.");
                                var result = script.Ex("if(typeof privateMark!=='undefined')throw Error('concurrent leaked');document.title='thread " + slot + "';");
                                if (!result.Ok || page.T != "thread " + slot) throw new InvalidOperationException(result.S + "; " + result.M);
                            }
                        }
                        catch (Exception error) { errors[slot] = error; }
                    });
                    threads[index].IsBackground = true;
                    threads[index].Start();
                }
                start.Set();
                foreach (var thread in threads)
                    if (!thread.Join(10000)) throw new InvalidOperationException("Concurrent cache session exceeded its watchdog.");
            }
            Program.Check(errors[0] == null && errors[1] == null, "concurrent owning-thread realms share syntax without sharing globals; " + errors[0] + "; " + errors[1]);
            var retired = Retired(fixture);
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            Program.Check(!retired.IsAlive, "shared prepared syntax does not retain a disposed interpreter");
        }

        private static void Code(Js script, string code, string name)
        {
            var result = script.Ex(code);
            Program.Check(result.Ok, name + "; " + result.S + "; " + result.M);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference Retired(string fixture)
        {
            var engine = new E { Jb = false };
            using (var script = new Js(engine, engine.Go(fixture)))
            {
                Code(script, "document.title='collectable realm';", "additional prepared realm executes before disposal");
                return new WeakReference(Vm.GetValue(script));
            }
        }
    }
}
