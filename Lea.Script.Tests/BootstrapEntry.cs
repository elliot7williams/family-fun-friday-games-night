using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Jint;
using Jint.Native;

namespace Lea.Script.Tests
{
    internal static class BootstrapEntry
    {
        internal static string Kind(string path = null)
        {
            var assembly = Worker(path);
            var entries = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                .Where(item => item.Key == "Lea.BootstrapEntry").Select(item => item.Value).ToArray();
            if (entries.Length != 1 || entries[0] != "direct" && entries[0] != "indirect" && entries[0] != "prepared")
                throw new InvalidDataException("Worker bootstrap entry metadata is missing or invalid.");
            return entries[0];
        }

        internal static void Contract(string path = null)
        {
            var assembly = Worker(path);
            var kind = Kind(path);
            Program.Check(kind == "direct" || kind == "indirect" || kind == "prepared", "worker identifies one supported compiled bootstrap entry: " + kind);
            var run = assembly.EntryPoint.DeclaringType.GetMethod("Run", BindingFlags.Static | BindingFlags.NonPublic);
            var calls = VmOptions.Calls(run).ToArray();
            var evaluate = calls.Count(item => item.DeclaringType == typeof(Jint.Engine) && item.Name == "Evaluate");
            var eval = calls.Count(item => item.DeclaringType == typeof(Jint.Runtime.Intrinsics) && item.Name == "get_Eval");
            var invoke = calls.Count(item => item.DeclaringType == typeof(Jint.Engine) && item.Name == "Invoke" &&
                item.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(
                    new[] { typeof(JsValue), typeof(object), typeof(object[]) }));
            var prepare = calls.Count(item => item.DeclaringType == typeof(Jint.Engine) && item.Name == "PrepareScript");
            var prepared = calls.Count(item => item.DeclaringType == typeof(Jint.Engine) && item.Name == "Evaluate" &&
                item.GetParameters().Length == 1 && item.GetParameters()[0].ParameterType == typeof(Prepared<Acornima.Ast.Script>).MakeByRefType());
            Program.Check(kind == "indirect" ? evaluate == 0 && eval == 1 && invoke == 1 && prepare == 0 :
                evaluate == 1 && eval == 0 && invoke == 0 && (kind == "prepared" ? prepare == 1 && prepared == 1 : prepare == 0 && prepared == 0),
                "worker compiled calls agree with its bootstrap entry metadata: " + kind);
            Program.Check(calls.Count(item => item.DeclaringType.FullName == "Lea.Script.Vo" && item.Name == "Of") == 1,
                "worker bootstrap retains the shared explicit options factory");
            Program.Check(calls.Count(item => item is ConstructorInfo && item.DeclaringType == typeof(Jint.Engine) &&
                item.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Options) })) == 1,
                "worker bootstrap retains its explicit constrained engine constructor");
        }

        internal static void PreparedContract(string path = null)
        {
            Contract(path);
            Program.Check(Kind(path) == "prepared", "worker uses the compiled prepared bootstrap entry");
            var factory = Worker(path).EntryPoint.DeclaringType.GetMethod("Preparation", BindingFlags.Static | BindingFlags.NonPublic);
            var options = (ScriptPreparationOptions)factory.Invoke(null, null);
            Program.Check(options.StaticAnalysis && !options.FoldConstants && !options.CollectReferencedGlobals &&
                options.ParsingOptions.CompileRegex == false && options.ParsingOptions.RegexTimeout == TimeSpan.FromMilliseconds(100),
                "worker prepares the trusted asset with explicit analysis and interpreted 100-ms regexes without constant folding");
        }

        private static Assembly Worker(string path)
        {
            path = path == null ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker", "Lea.Script.Worker.exe") : Path.GetFullPath(path);
            return Assembly.LoadFrom(path);
        }

        internal static void Checkpoints(string path = null)
        {
            var assembly = Worker(path);
            var kind = Kind(path);
            Program.Check(kind == "direct", "startup hooks contract selects the ordinary direct worker");
            var run = assembly.EntryPoint.DeclaringType.GetMethod("Run", BindingFlags.Static | BindingFlags.NonPublic);
            var tokens = VmOptions.Tokens(run).ToArray();
            var trace = run.DeclaringType.GetMethod("Trace", BindingFlags.Static | BindingFlags.NonPublic);
            var markers = Enumerable.Range(1, tokens.Length - 1)
                .Where(index => Equals(tokens[index].Value, trace) && tokens[index - 1].Value is string).ToArray();
            foreach (var stage in new[] { "evaluate", "debugger", "subscribed" })
                Program.Check(markers.Count(index => (string)tokens[index - 1].Value == stage) == 1,
                    "worker contains exactly one fixed startup checkpoint: " + stage);
            var calls = tokens.Select((token, index) => token.Value is MethodBase ?
                (Equals(token.Value, trace) && index > 0 && tokens[index - 1].Value is string ?
                    "trace:" + tokens[index - 1].Value : ((MethodBase)token.Value).DeclaringType.FullName + "." + ((MethodBase)token.Value).Name) : null)
                .Where(value => value != null).ToArray();
            var evaluate = Array.IndexOf(calls, "trace:evaluate");
            var start = Array.IndexOf(calls, "trace:debugger");
            var handler = "Jint.Runtime.Debugger.DebugHandler.";
            Program.Check(evaluate >= 0 && start > evaluate && calls.Skip(start).Take(7).SequenceEqual(new[]
                { "trace:debugger", "Jint.Engine.get_Debugger", handler + "add_BeforeEvaluate",
                    "trace:subscribed", "Jint.Engine.Evaluate", "Jint.Engine.get_Debugger", handler + "remove_BeforeEvaluate" }),
                "compiled worker brackets hook setup before evaluation without changing the call path");
            var remove = tokens.Where(token => token.Value is MethodBase && ((MethodBase)token.Value).Name == "remove_BeforeEvaluate").ToArray();
            Program.Check(remove.Length == 1 && run.GetMethodBody().ExceptionHandlingClauses.Any(clause =>
                clause.Flags == ExceptionHandlingClauseOptions.Finally && remove[0].Key >= clause.HandlerOffset &&
                remove[0].Key < clause.HandlerOffset + clause.HandlerLength), "parsed hook removal remains in a finally block");
            var previous = Console.Error;
            using (var output = new StringWriter())
            {
                try
                {
                    Console.SetError(output);
                    trace.Invoke(null, new object[] { "debugger", Stopwatch.StartNew(), false });
                    trace.Invoke(null, new object[] { "subscribed", Stopwatch.StartNew(), false });
                }
                finally { Console.SetError(previous); }
                Program.Check(output.ToString() == "startup debugger" + Environment.NewLine + "startup subscribed" + Environment.NewLine,
                    "ordinary checkpoints contain no timings, page data or variable labels");
            }
            var failureMethod = typeof(Lea.Script.Jp).GetMethod("StartupFailure", BindingFlags.Static | BindingFlags.NonPublic);
            var failure = (Func<string, string, string, string>)Delegate.CreateDelegate(typeof(Func<string, string, string, string>), failureMethod);
            const string message = "Worker startup deadline exceeded.";
            const string header = "phase=hello; launch=191; probe=75; wall=5005; cpu=765 ms";
            var prefix = String.Concat(Enumerable.Repeat("startup evaluate\n", 40));
            Program.Check(new[] { "debugger", "subscribed" }.All(stage =>
            {
                var tail = "startup " + stage + "\n";
                var report = failure(message, header, prefix + tail);
                return report.Length == 512 && report.StartsWith(message + " " + header + "; ", StringComparison.Ordinal) &&
                    report.Contains("...") && report.EndsWith(tail, StringComparison.Ordinal);
            }), "bounded startup failures preserve the last hook checkpoint and parent summary");
        }
    }
}
