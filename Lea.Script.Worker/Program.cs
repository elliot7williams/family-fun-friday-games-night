using System;
using System.Diagnostics;
using System.IO;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Lea.Script;

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
                    // Only trusted bootstrap overlaps the denial probe; page frames follow the verified hello.
                    using (var probe = new NetworkProbe(Wire.Number(init, "p")))
                    {
                        Trace("network", clock, trace);
                        vm = new Jint.Engine(options =>
                        {
                            options.MaxStatements(20000).LimitMemory(8 * 1024 * 1024)
                                .TimeoutInterval(TimeSpan.FromSeconds(5)).LimitRecursion(64)
                                .RegexTimeoutInterval(TimeSpan.FromMilliseconds(100)).MaxArraySize(65536);
                            options.Constraints.StackOverflowGuard = true;
                            options.Interop.Enabled = false;
                            options.Interop.AllowGetType = false;
                            options.Interop.AllowWrite = false;
                        });
                        Trace("vm", clock, trace);
                        vm.SetValue("__lea", new ClrFunction(vm, "__lea", (_, values) =>
                        {
                            if (++request > 256) throw new InvalidDataException("Script host-call budget exceeded.");
                            Wire.Write(output, new { k = "call", i = id, r = request, b = Codec.Arguments(values) });
                            var reply = Wire.Read(input);
                            Wire.Fields(reply, "k", "i", "r", "v");
                            if (Wire.Text(reply, "k", 8) != "reply" || Wire.Number(reply, "i") != id ||
                                Wire.Number(reply, "r") != request || !reply.ContainsKey("v"))
                                throw new InvalidDataException("Invalid script bridge reply.");
                            return Codec.Value(vm, reply["v"]);
                        }));
                        vm.SetValue("__jobs", new ClrFunction(vm, "__jobs", (_, values) =>
                        { vm.Advanced.ProcessTasks(); return JsValue.Undefined; }));
                        Trace("bridge", clock, trace);
                        string source;
                        using (var stream = typeof(Program).Assembly.GetManifestResourceStream("Lea.Script.Dom.js"))
                        using (var reader = new StreamReader(stream)) source = reader.ReadToEnd();
                        Trace("source", clock, trace);
                        // Tracing observes the production parser instead of selecting a different bootstrap path.
                        Trace("evaluate", clock, trace);
                        Jint.Runtime.Debugger.DebugHandler.BeforeEvaluateEventHandler parsed =
                            (sender, ast) => Trace("parsed", clock, trace);
                        vm.Debugger.BeforeEvaluate += parsed;
                        try { dispatch = vm.Evaluate(source); }
                        finally { vm.Debugger.BeforeEvaluate -= parsed; }
                        Trace("dom", clock, trace);
                        var network = probe.Denied();
                        Trace("probes", clock, trace);
                        Wire.Write(output, new { k = "hello", g = guards, n = network, f = file ? 1 : 0 });
                    }
                    while (true)
                    {
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
                        try
                        {
                            var prevented = false;
                            if (kind == "exec")
                            {
                                vm.Execute(Wire.Text(frame, "c", 16 * 1024));
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
                            Wire.Write(output, new { k = "end", i = id, s = "ok", m = "", d = prevented });
                        }
                        catch (Exception error) when (error is JavaScriptException || error is JintException ||
                            error is Acornima.ParseErrorException || error is TimeoutException || error is InvalidDataException)
                        {
                            var limited = error is TimeoutException || error is StatementsCountOverflowException ||
                                error is MemoryLimitExceededException || error is RecursionDepthOverflowException ||
                                error is InvalidDataException;
                            var message = limited ? "Script budget exceeded: " + error.GetType().Name + "." : error.Message;
                            Wire.Write(output, new { k = "end", i = id, s = limited ? "limit" : "js",
                                m = message.Length <= 512 ? message : message.Substring(0, 512), d = false });
                            return 0;
                        }
                    }
                }
            }
            catch (Exception error)
            {
                try { Console.Error.WriteLine(error.GetType().Name + ": " + error.Message); } catch { }
                return 4;
            }
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
