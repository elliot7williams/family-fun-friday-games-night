using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Runtime.Interop;
using Lea.Script;

namespace Lea.Script.Tests
{
    internal static class BootstrapPreparation
    {
        internal static void Run(string strategy, string reportPath)
        {
            if (strategy != "direct" && strategy != "parsed" && strategy != "analyzed")
                throw new ArgumentException("Use direct, parsed or analyzed preparation.");
            reportPath = Path.GetFullPath(reportPath);
            if (File.Exists(reportPath) || !Directory.Exists(Path.GetDirectoryName(reportPath)))
                throw new ArgumentException("Supply a new report path in an existing directory.");

            byte[] bytes;
            using (var stream = typeof(Js).Assembly.GetManifestResourceStream("Lea.Script.Dom.js"))
            using (var copy = new MemoryStream())
            {
                if (stream == null) throw new InvalidOperationException("Missing bootstrap resource.");
                stream.CopyTo(copy);
                bytes = copy.ToArray();
            }
            var source = Encoding.UTF8.GetString(bytes);
            string digest;
            using (var hash = SHA256.Create()) digest = BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "");
            var stages = new List<object>();
            Exception failure = null;
            var calls = 0;
            var parsedCount = 0;
            using (var process = Process.GetCurrentProcess())
            {
                var cpu = process.TotalProcessorTime;
                var clock = Stopwatch.StartNew();
                object Mark(string stage)
                {
                    var sample = new { stage, wallMs = clock.ElapsedMilliseconds,
                        cpuMs = (long)(process.TotalProcessorTime - cpu).TotalMilliseconds };
                    stages.Add(sample);
                    return sample;
                }
                Mark("begin");
                try
                {
                    // A single fresh-process CLR diagnostic, not an AppContainer startup gate.
                    using (var vm = new Jint.Engine(options =>
                    {
                        options.MaxStatements(20000).LimitMemory(8 * 1024 * 1024)
                            .TimeoutInterval(TimeSpan.FromSeconds(5)).LimitRecursion(64)
                            .RegexTimeoutInterval(TimeSpan.FromMilliseconds(100)).MaxArraySize(65536);
                        options.Constraints.StackOverflowGuard = true;
                        options.Interop.Enabled = false;
                        options.Interop.AllowGetType = false;
                        options.Interop.AllowWrite = false;
                    }))
                    {
                        vm.SetValue("__lea", new ClrFunction(vm, "__lea", (_, values) =>
                        {
                            calls++;
                            throw new InvalidOperationException("Bootstrap made an unexpected host call.");
                        }));
                        vm.SetValue("__jobs", new ClrFunction(vm, "__jobs", (_, values) => JsValue.Undefined));
                        Mark("vm");
                        var prepared = default(Prepared<Acornima.Ast.Script>);
                        if (strategy != "direct")
                            prepared = Jint.Engine.PrepareScript(source, options:
                                new ScriptPreparationOptions
                                {
                                    StaticAnalysis = strategy == "analyzed",
                                    ParsingOptions = new ScriptParsingOptions
                                    {
                                        CompileRegex = false,
                                        RegexTimeout = TimeSpan.FromMilliseconds(100)
                                    }
                                });
                        Mark("preparation");
                        Jint.Runtime.Debugger.DebugHandler.BeforeEvaluateEventHandler parsed = (sender, ast) =>
                        {
                            parsedCount++;
                            Mark("parsed");
                        };
                        vm.Debugger.BeforeEvaluate += parsed;
                        JsValue dispatch;
                        try { dispatch = strategy == "direct" ? vm.Evaluate(source) : vm.Evaluate(prepared); }
                        finally { vm.Debugger.BeforeEvaluate -= parsed; }
                        Mark("complete");
                        Program.Check(dispatch is Function, "preparation returns the private dispatcher: " + strategy);
                        Program.Check(calls == 0 && parsedCount == 1, "preparation has no host calls and one evaluation: " + strategy);
                        var probe = vm.Evaluate(@"(function(){
if(window!==globalThis||typeof document!=='object'||'__lea' in globalThis||'__jobs' in globalThis)throw Error('bootstrap globals');
const global=Object.getOwnPropertyDescriptor(globalThis,'document');
if(global.writable||global.enumerable||global.configurable)throw Error('document metadata');
const error=new DOMException('sample','SyntaxError');
if(!(error instanceof Error)||error.code!==12||DOMException.SYNTAX_ERR!==12||DOMException.prototype.SYNTAX_ERR!==12)throw Error('exception bindings');
for(const name of ['addEventListener','removeEventListener']){
  const method=document[name];
  if(method!==window[name]||Object.hasOwn(method,'prototype'))throw Error('event metadata');
  let error;try{new method('click',null);}catch(value){error=value;}
  if(!(error instanceof TypeError))throw Error('event construction');
}
return true;
}());");
                        Program.Check(probe.AsBoolean() && calls == 0,
                            "preparation preserves bridge-free globals, descriptors and non-constructible event methods: " + strategy);
                        Mark("validated");
                    }
                }
                catch (Exception error) { failure = error; throw; }
                finally
                {
                    clock.Stop();
                    var report = new
                    {
                        kind = "bootstrap-preparation-clr-diagnostic", strategy,
                        sourceBytes = bytes.Length, sourceSHA256 = digest,
                        passed = failure == null, error = failure == null ? null : failure.ToString(),
                        hostCalls = calls, bootstrapEvaluations = parsedCount, stages,
                        interpreterBudgetMs = 5000, statements = 20000, allocationBytes = 8 * 1024 * 1024,
                        recursion = 64, regexBudgetMs = 100, arraySize = 65536, clrInterop = false,
                        preparationRegexBudgetMs = strategy == "direct" ? (int?)null : 100,
                        preparationRegexMode = strategy == "direct" ? null : "interpreted",
                        nativeStartupGate = false, coldCacheBenchmark = false, retries = 0
                    };
                    var json = new JavaScriptSerializer().Serialize(report);
                    using (var stream = new FileStream(reportPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    using (var writer = new StreamWriter(stream, new UTF8Encoding(false))) writer.Write(json);
                    Console.WriteLine("Bootstrap preparation strategy=" + strategy + "; passed=" + (failure == null) +
                        "; report=" + reportPath + "; fresh-process CLR diagnostic only.");
                }
            }
        }
    }
}
