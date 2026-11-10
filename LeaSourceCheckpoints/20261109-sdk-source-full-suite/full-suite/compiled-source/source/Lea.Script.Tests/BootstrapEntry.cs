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
        internal static string Kind(string path = null) { return (string)ModernComponents.Bootstrap(path)["bootstrapKind"]; }

        internal static void Contract(string path = null) { ModernComponents.Bootstrap(path); }

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
            ModernComponents.Bootstrap(path);
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
