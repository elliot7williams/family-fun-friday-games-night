using System;
using System.Reflection;
using Jint;
using Jint.Native;
using Jint.Runtime.Interop;
using Lea.Engine;
using Lea.Script;

namespace Lea.Script.Tests
{
    internal static class BootstrapStages
    {
        internal static void Run(string fixture)
        {
            foreach (var target in new[] { "factory", "evaluate", "declarations", "scope", "events", "interfaces", "elements", "document" })
            {
                var engine = new E { Jb = false };
                var page = engine.Go(fixture);
                var title = page.T;
                var attempts = 0;
                string configuration = null;
                var nonblocking = false;
                using (var script = new Js(engine, page, configure =>
                {
                    attempts++;
                    var options = new Options();
                    configure(options);
                    configuration = VmOptions.Snapshot(options);
                    nonblocking = !options.AgentCanSuspend;
                    if (attempts == 1 && target == "factory") throw new TimeoutException("private startup detail");
                    var vm = new Jint.Engine(options);
                    if (attempts == 1)
                    {
                        var first = true;
                        vm.Debugger.BeforeEvaluate += (sender, ast) =>
                        {
                            if (!first) return;
                            first = false;
                            if (target == "evaluate") throw new TimeoutException("private startup detail");
                            var original = vm.GetValue("__jobs");
                            vm.SetValue("__jobs", (JsValue)new ClrFunction(vm, "__jobs", (_, args) =>
                            {
                                var result = vm.Invoke(original, JsValue.Undefined, args);
                                if (args.Length == 1 && args[0].IsString() && args[0].AsString() == target)
                                    throw new TimeoutException("private startup detail");
                                return result;
                            }));
                        };
                    }
                    return vm;
                }))
                {
                    var result = script.Ex("globalThis.oldRealm=1;setTimeout(function(){document.title='late';},0);requestAnimationFrame(function(){document.title='late frame';});document.title='must not run';");
                    Program.Check(configuration == VmOptions.Snapshot(Vo.Of(1000)) && nonblocking,
                        "startup stage: controlled " + target + " preserves the exact production options");
                    Program.Check(result.S == "limit" && result.M == "Script budget exceeded. phase=bootstrap; reason=TimeoutException; stage=" + target && result.M.Length <= 512 && !result.M.Contains("private startup detail"),
                        "startup stage: controlled " + target + " reports its exact last boundary without raw details");
                    Program.Check(result.N == 0 && page.T == title && attempts == 1,
                        "startup stage: controlled " + target + " blocks all page work without an automatic retry");
                    Program.Check(Field(script, "vm") == null && Field(script, "startupStage") == null && !(bool)Field(script, "running") && script.Tn == 0 && script.Rn == 0,
                        "startup stage: controlled " + target + " retires realm, stage, timers, frames and running state");
                    var recovered = script.Ex("if(typeof oldRealm!=='undefined')throw Error('old realm');document.title='fresh';");
                    Program.Check(recovered.Ok && page.T == "fresh" && attempts == 2,
                        "startup stage: controlled " + target + " permits a new explicitly requested clean realm");
                    Program.Check(Field(script, "startupStage") == null && !script.Ex("throw Error('ordinary error');").M.Contains("stage="),
                        "startup stage: controlled " + target + " clears startup metadata and preserves ordinary errors");
                }
            }
            var disabled = new E { Jb = false, Je = false };
            var calls = 0;
            using (var script = new Js(disabled, disabled.Go(fixture), configure => { calls++; throw new Exception("must not launch"); }))
            {
                Program.Check(script.Ex("1;").S == "off" && calls == 0 && Field(script, "startupStage") == null,
                    "startup stage: disabled scripting does not begin initialization");
                disabled.Je = true;
                Program.Check(script.Ex(new string('x', 16385)).S == "code" && calls == 0 && Field(script, "startupStage") == null,
                    "startup stage: rejected page code does not begin initialization");
            }
        }

        private static object Field(Js script, string name)
        {
            return typeof(Js).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(script);
        }
    }
}
