using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Jint;
using Lea.Script;

namespace Lea.Script.Tests
{
    internal static class VmOptions
    {
        internal static void Run()
        {
            foreach (var milliseconds in new[] { 1000, 5000 })
            {
                var options = Vo.Of(milliseconds);
                string legacy = null;
                using (var vm = new Jint.Engine(old =>
                {
                    // Independent pre-change configuration is the comparison control.
                    old.MaxStatements(20000).LimitMemory(8 * 1024 * 1024)
                        .TimeoutInterval(TimeSpan.FromMilliseconds(milliseconds)).LimitRecursion(64)
                        .RegexTimeoutInterval(TimeSpan.FromMilliseconds(100)).MaxArraySize(65536);
                    old.Constraints.StackOverflowGuard = true;
                    old.Interop.Enabled = false;
                    old.Interop.AllowGetType = false;
                    old.Interop.AllowWrite = false;
                    legacy = Snapshot(old);
                })) { }
                Program.Check(Snapshot(options) == legacy, "explicit VM options preserve preexisting scalar and constraint fields: " + milliseconds);
                Program.Check(options.Constraints.StackOverflowGuard && !options.Interop.Enabled && !options.Interop.AllowGetType && !options.Interop.AllowWrite,
                    "explicit VM options preserve stack and CLR restrictions: " + milliseconds);
                Program.Check(!options.AgentCanSuspend, "window VM options forbid blocking atomic waits: " + milliseconds);
                var other = Vo.Of(milliseconds);
                Program.Check(!ReferenceEquals(options, other) && !ReferenceEquals(options.Constraints, other.Constraints) && !ReferenceEquals(options.Interop, other.Interop),
                    "explicit VM options are fresh per realm: " + milliseconds);
            }
            foreach (var milliseconds in new[] { 0, 999, 1001, 5001 })
            {
                var rejected = false;
                try { Vo.Of(milliseconds); } catch (ArgumentOutOfRangeException) { rejected = true; }
                Program.Check(rejected, "VM option builder rejects undeclared interpreter budget: " + milliseconds);
            }
            var nullRejected = false;
            try { Vo.At(null, 1000); } catch (ArgumentNullException) { nullRejected = true; }
            Program.Check(nullRejected, "VM configuration rejects a missing options object");
            Constructors(typeof(Js).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic), "trusted");
            var worker = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker", "Lea.Script.Worker.exe"));
            var run = worker.EntryPoint.DeclaringType.GetMethod("Run", BindingFlags.Static | BindingFlags.NonPublic);
            Constructors(run, "worker");
            var workerFactory = worker.GetType("Lea.Script.Vo", true).GetMethod("Of", BindingFlags.Static | BindingFlags.NonPublic);
            Program.Check(Snapshot((Options)workerFactory.Invoke(null, new object[] { 5000 })) == Snapshot(Vo.Of(5000)),
                "trusted and worker assemblies contain the same options configuration");
        }

        private static string Snapshot(Options options)
        {
            var text = new List<string>();
            foreach (var pair in new[] { Tuple.Create("constraints", (object)options.Constraints), Tuple.Create("interop", (object)options.Interop) })
            {
                foreach (var property in pair.Item2.GetType().GetProperties().OrderBy(item => item.Name))
                {
                    if (property.GetIndexParameters().Length != 0) continue;
                    var value = property.GetValue(pair.Item2);
                    if (Scalar(property.PropertyType)) text.Add(pair.Item1 + "." + property.Name + "=" + Convert.ToString(value, CultureInfo.InvariantCulture));
                    else if (value is IEnumerable && property.Name == "Constraints")
                        foreach (var constraint in (IEnumerable)value)
                        {
                            text.Add(constraint.GetType().FullName);
                            foreach (var field in constraint.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).OrderBy(item => item.Name))
                                if (Scalar(field.FieldType)) text.Add(field.Name + "=" + Convert.ToString(field.GetValue(constraint), CultureInfo.InvariantCulture));
                        }
                }
            }
            return String.Join(";", text);
        }
        private static bool Scalar(Type type) { return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(TimeSpan); }

        private static void Constructors(MethodInfo method, string label)
        {
            var constructors = Calls(method).OfType<ConstructorInfo>().Where(item => item.DeclaringType == typeof(Jint.Engine)).ToArray();
            Program.Check(constructors.Length == 1 && constructors[0].GetParameters().Select(item => item.ParameterType).SequenceEqual(new[] { typeof(Options) }),
                label + " compiled startup calls the explicit Options constructor, not a callback adapter");
        }
        internal static IEnumerable<MethodBase> Calls(MethodInfo method)
        {
            var codes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static).Where(item => item.FieldType == typeof(OpCode))
                .Select(item => (OpCode)item.GetValue(null)).ToDictionary(item => unchecked((ushort)item.Value));
            var il = method.GetMethodBody().GetILAsByteArray();
            for (var offset = 0; offset < il.Length;)
            {
                var value = (ushort)il[offset++]; if (value == 0xfe) value = (ushort)(0xfe00 | il[offset++]);
                var code = codes[value];
                if (code.OperandType == OperandType.InlineMethod)
                    yield return method.Module.ResolveMethod(BitConverter.ToInt32(il, offset));
                switch (code.OperandType)
                {
                    case OperandType.InlineNone: break;
                    case OperandType.ShortInlineBrTarget: case OperandType.ShortInlineI: case OperandType.ShortInlineVar: offset++; break;
                    case OperandType.InlineVar: offset += 2; break;
                    case OperandType.InlineI8: case OperandType.InlineR: offset += 8; break;
                    case OperandType.InlineSwitch: offset += 4 + BitConverter.ToInt32(il, offset) * 4; break;
                    default: offset += 4; break;
                }
            }
        }
    }
}
