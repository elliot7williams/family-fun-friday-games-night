using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Lea.Script;

#if LEA_PREPARED_BOOTSTRAP
[assembly: System.Reflection.AssemblyMetadata("Lea.BootstrapEntry", "prepared")]
#elif LEA_INDIRECT_BOOTSTRAP
[assembly: System.Reflection.AssemblyMetadata("Lea.BootstrapEntry", "indirect")]
#else
[assembly: System.Reflection.AssemblyMetadata("Lea.BootstrapEntry", "direct")]
#endif

namespace Lea.Script.Worker
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length != 1 || args[0] != "--pipe") return 2;
            try
            {
                var trace = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE") == "1";
                var clock = Stopwatch.StartNew();
                Trace("entry", clock, trace);
                var guards = Lea.Image.Mg.Apply();
                Trace("guards", clock, trace);
                return Run(guards, clock, trace);
            }
            catch (Exception error)
            {
                try { Console.Error.WriteLine(error.GetType().Name + ": " + error.Message); } catch { }
                return 4;
            }
        }

        // Keep interpreter binding/JIT inside Main's guarded, reported startup path.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int Run(int guards, Stopwatch clock, bool trace)
        {
            using (var input = Console.OpenStandardInput())
            using (var output = Console.OpenStandardOutput())
            {
                var init = Wire.Read(input);
                Wire.Fields(init, "k", "p", "f");
                if (Wire.Text(init, "k", 8) != "init") return 3;
                Trace("init", clock, trace);
                var file = FileDenied(Wire.Text(init, "f", 1024));
                Trace("canary", clock, trace);
                var id = 0;
                var request = 0;
                Jint.Engine vm;
                JsValue dispatch;
                int network;
                // Only trusted bootstrap overlaps the denial probe; page frames follow the verified hello.
                using (var probe = new NetworkProbe(Wire.Number(init, "p")))
                {
                    Trace("network", clock, trace);
                    Trace("options", clock, trace);
                    var options = Vo.Of(5000);
                    Trace("configured", clock, trace);
                    vm = new Jint.Engine(options);
                    Trace("vm", clock, trace);
                    vm.SetValue("__lea", (JsValue)new ClrFunction(vm, "__lea", (_, values) =>
                    {
                        if (++request > 256) throw new InvalidDataException("Script host-call budget exceeded.");
                        Frame("call", id, request, clock, trace);
                        Wire.Write(output, new { k = "call", i = id, r = request, b = Codec.Arguments(values) });
                        var reply = Wire.Read(input);
                        Wire.Fields(reply, "k", "i", "r", "v");
                        if (Wire.Text(reply, "k", 8) != "reply" || Wire.Number(reply, "i") != id ||
                            Wire.Number(reply, "r") != request || !reply.ContainsKey("v"))
                            throw new InvalidDataException("Invalid script bridge reply.");
                        Frame("reply", id, request, clock, trace);
                        return Codec.Value(vm, reply["v"]);
                    }));
                    var markers = new Bm();
                    vm.SetValue("__jobs", (JsValue)new ClrFunction(vm, "__jobs", (_, values) =>
                    {
                        var stage = markers.Of(values);
                        if (stage == null) vm.Advanced.ProcessTasks();
                        else Trace(stage, clock, trace);
                        return JsValue.Undefined;
                    }));
                    Trace("bridge", clock, trace);
                    string source;
                    using (var stream = typeof(Program).Assembly.GetManifestResourceStream("Lea.Script.Dom.js"))
                    using (var reader = new StreamReader(stream)) source = reader.ReadToEnd();
                    Trace("source", clock, trace);
#if LEA_PREPARED_BOOTSTRAP
                    Trace("prepare", clock, trace);
                    var bootstrap = Jint.Engine.PrepareScript(source, options: Preparation());
                    Trace("prepared", clock, trace);
#endif
                    // Tracing observes the production parser instead of selecting a different bootstrap path.
                    Trace("evaluate", clock, trace);
#if LEA_INDIRECT_BOOTSTRAP
                    dispatch = vm.Invoke(vm.Intrinsics.Eval, JsValue.Undefined, new[] { (JsValue)source });
                    Trace("eval-end", clock, trace);
#else
                    Jint.Runtime.Debugger.DebugHandler.BeforeEvaluateEventHandler parsed =
                        (sender, ast) => Trace("parsed", clock, trace);
                    vm.Debugger.BeforeEvaluate += parsed;
                    try
                    {
#if LEA_PREPARED_BOOTSTRAP
                        dispatch = vm.Evaluate(bootstrap);
#else
                        dispatch = vm.Evaluate(source);
#endif
                    }
                    finally { vm.Debugger.BeforeEvaluate -= parsed; }
#endif
                    markers.End();
                    Trace("dom", clock, trace);
                    network = probe.Denied();
                    Trace("probes", clock, trace);
                }
                // Complete trusted probe cleanup before announcing readiness for page frames.
                Trace("closed", clock, trace);
                Trace("hello", clock, trace);
                Wire.Write(output, new { k = "hello", g = guards, n = network, f = file ? 1 : 0 });
                Trace("ready", clock, trace);
                Frame("invoke", id, 0, clock, trace);
                return Frames();

                // Page-only code is translated after the guarded startup handshake.
                [MethodImpl(MethodImplOptions.NoInlining)]
                int Frames()
                {
                    Frame("enter", id, 0, clock, trace);
                    while (true)
                    {
                        Frame("read", id, 0, clock, trace);
                        var frame = Wire.Read(input);
                        var kind = Wire.Text(frame, "k", 8);
                        if (kind == "exec") Wire.Fields(frame, "k", "i", "c");
                        else if (kind == "timer") Wire.Fields(frame, "k", "i", "t");
                        else if (kind == "event") Wire.Fields(frame, "k", "i", "t", "p");
                        else return 3;
                        var next = Wire.Number(frame, "i");
                        if (next <= id) return 3;
                        id = next;
                        request = 0;
                        Frame(kind, id, 0, clock, trace);
                        try
                        {
                            var prevented = false;
                            if (kind == "exec")
                            {
                                vm.Execute(Wire.Text(frame, "c", 16 * 1024));
                                Frame("tasks", id, request, clock, trace);
                                vm.Invoke(dispatch, JsValue.Undefined, new[] { (JsValue)"mt", Codec.Value(vm, new object[0]) });
                            }
                            else if (kind == "timer")
                            {
                                var timer = Wire.Number(frame, "t");
                                if (timer < 1) throw new InvalidDataException("Invalid timer handle.");
                                vm.Invoke(dispatch, JsValue.Undefined, new[] { (JsValue)"tm", Codec.Value(vm, new object[] { timer }) });
                            }
                            else
                            {
                                var type = Wire.Text(frame, "t", 32);
                                if (type != "click" && type != "input" && type != "change" && type != "submit" && type != "DOMContentLoaded" && type != "load")
                                    throw new InvalidDataException("Unsupported event type.");
                                var path = frame["p"] as object[];
                                if (path == null || path.Length > 128) throw new InvalidDataException("Invalid event path.");
                                foreach (var key in path)
                                    if (!(key is int) || (int)key < 0 || (int)key >= 512) throw new InvalidDataException("Invalid event handle.");
                                prevented = vm.Invoke(dispatch, JsValue.Undefined, new[] { (JsValue)type, Codec.Value(vm, path) }).AsBoolean();
                            }
                            Frame("end", id, request, clock, trace);
                            Wire.Write(output, new { k = "end", i = id, s = "ok", m = "", d = prevented });
                        }
                        catch (Exception error) when (error is JavaScriptException || error is JintException ||
                            error is Acornima.ParseErrorException || error is TimeoutException || error is InvalidDataException)
                        {
                            var limited = error is TimeoutException || error is StatementsCountOverflowException ||
                                error is MemoryLimitExceededException || error is RecursionDepthOverflowException ||
                                error is InvalidDataException;
                            var message = limited ? "Script budget exceeded: " + error.GetType().Name + "." : error.Message;
                            Frame("error", id, request, clock, trace);
                            Wire.Write(output, new { k = "end", i = id, s = limited ? "limit" : "js",
                                m = message.Length <= 512 ? message : message.Substring(0, 512), d = false });
                            return 0;
                        }
                    }
                }
            }
        }

#if LEA_PREPARED_BOOTSTRAP
        private static ScriptPreparationOptions Preparation()
        {
            return new ScriptPreparationOptions
            {
                StaticAnalysis = true,
                FoldConstants = false,
                ParsingOptions = new ScriptParsingOptions
                {
                    CompileRegex = false,
                    RegexTimeout = TimeSpan.FromMilliseconds(100)
                }
            };
        }
#endif

        private static void Frame(string stage, int id, int request, Stopwatch clock, bool enabled)
        {
            if (!enabled || ((stage == "call" || stage == "reply") && request > 4 && request != 256)) return;
            Console.Error.WriteLine("frame " + stage + ": i=" + id + "; r=" + request + "; wall=" + clock.ElapsedMilliseconds + " ms");
        }

        private static void Trace(string stage, Stopwatch clock, bool enabled)
        {
            if (!enabled) { Console.Error.WriteLine("startup " + stage); return; }
            using (var process = Process.GetCurrentProcess())
                Console.Error.WriteLine("startup " + stage + ": wall=" + clock.ElapsedMilliseconds +
                    "; cpu=" + (long)process.TotalProcessorTime.TotalMilliseconds + " ms");
        }

        private static bool FileDenied(string path)
        {
            try { using (var file = File.OpenRead(path)) file.ReadByte(); return false; }
            catch (UnauthorizedAccessException) { return true; }
        }
    }
}
