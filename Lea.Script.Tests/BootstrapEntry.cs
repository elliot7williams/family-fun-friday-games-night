using System;
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
    }
}
