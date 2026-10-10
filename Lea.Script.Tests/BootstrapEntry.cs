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
            if (entries.Length != 1 || entries[0] != "direct" && entries[0] != "indirect")
                throw new InvalidDataException("Worker bootstrap entry metadata is missing or invalid.");
            return entries[0];
        }

        internal static void Contract(string path = null)
        {
            var assembly = Worker(path);
            var kind = Kind(path);
            Program.Check(kind == "direct" || kind == "indirect", "worker identifies one supported compiled bootstrap entry: " + kind);
            var run = assembly.EntryPoint.DeclaringType.GetMethod("Run", BindingFlags.Static | BindingFlags.NonPublic);
            var calls = VmOptions.Calls(run).ToArray();
            var evaluate = calls.Count(item => item.DeclaringType == typeof(Jint.Engine) && item.Name == "Evaluate");
            var eval = calls.Count(item => item.DeclaringType == typeof(Jint.Runtime.Intrinsics) && item.Name == "get_Eval");
            var invoke = calls.Count(item => item.DeclaringType == typeof(Jint.Engine) && item.Name == "Invoke" &&
                item.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(
                    new[] { typeof(JsValue), typeof(object), typeof(object[]) }));
            Program.Check(kind == "direct" ? evaluate == 1 && eval == 0 && invoke == 0 : evaluate == 0 && eval == 1 && invoke == 1,
                "worker compiled calls agree with its bootstrap entry metadata: " + kind);
            Program.Check(calls.Count(item => item.DeclaringType.FullName == "Lea.Script.Vo" && item.Name == "Of") == 1,
                "worker bootstrap retains the shared explicit options factory");
            Program.Check(calls.Count(item => item is ConstructorInfo && item.DeclaringType == typeof(Jint.Engine) &&
                item.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Options) })) == 1,
                "worker bootstrap retains its explicit constrained engine constructor");
        }

        private static Assembly Worker(string path)
        {
            path = path == null ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker", "Lea.Script.Worker.exe") : Path.GetFullPath(path);
            return Assembly.LoadFrom(path);
        }
    }
}
