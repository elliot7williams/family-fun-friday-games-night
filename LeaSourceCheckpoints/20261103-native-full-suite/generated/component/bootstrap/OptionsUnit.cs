using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Jint;
#if CORE_OPTIONS
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;
#else
using System.Web.Script.Serialization;
#endif

internal static class OptionsUnit
{
    private static bool Scalar(Type type) { return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(TimeSpan); }
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
    private static void Check(bool condition) { if (!condition) throw new InvalidOperationException("Options unit comparison failed."); }
    internal static int Execute(string[] args)
    {
        try
        {
            if (args.Length != 1) throw new ArgumentException("One pinned component path required.");
            var path = Path.GetFullPath(args[0]);
            string digest;
            using (var file = File.OpenRead(path)) using (var sha = SHA256.Create()) digest = BitConverter.ToString(sha.ComputeHash(file)).Replace("-", "");
#if CORE_OPTIONS
            Check(digest == "F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0");
            using (var file = File.OpenRead(path)) using (var pe = new PEReader(file))
            {
                var reader = pe.GetMetadataReader();
                foreach (var handle in reader.TypeDefinitions)
                {
                    var type = reader.GetTypeDefinition(handle);
                    var name = reader.GetString(type.Name);
                    if (name != "<Module>" && name != "Vo") continue;
                    foreach (var method in type.GetMethods()) Check(reader.GetString(reader.GetMethodDefinition(method).Name) != ".cctor");
                }
            }
#else
            Check(digest == "18B713C1141AE7BA5DB86D3A1074BEE71C8DB76AA5446C19C34528DD8CE94C61");
#endif
            var component = Assembly.LoadFrom(path);
            var typeInfo = component.GetType("Lea.Script.Vo", true);
            var factory = typeInfo.GetMethod("Of", BindingFlags.NonPublic | BindingFlags.Static);
            var configure = typeInfo.GetMethod("At", BindingFlags.NonPublic | BindingFlags.Static);
            var rows = new List<object>();
            foreach (var milliseconds in new[] { 1000, 5000 })
            {
                var options = (Options)factory.Invoke(null, new object[] { milliseconds });
                string control = null;
                // Same independent constructor callback used by the original VmOptions test; no page evaluation.
                using (var vm = new Jint.Engine(old => {
                    old.MaxStatements(20000).LimitMemory(8 * 1024 * 1024)
                        .TimeoutInterval(TimeSpan.FromMilliseconds(milliseconds)).LimitRecursion(64)
                        .RegexTimeoutInterval(TimeSpan.FromMilliseconds(100)).MaxArraySize(65536);
                    old.Constraints.StackOverflowGuard = true;
                    old.Interop.Enabled = false; old.Interop.AllowGetType = false; old.Interop.AllowWrite = false;
                    control = Snapshot(old);
                })) { }
                var other = (Options)factory.Invoke(null, new object[] { milliseconds });
                var snapshot = Snapshot(options);
                var restricted = options.Constraints.StackOverflowGuard && !options.Interop.Enabled && !options.Interop.AllowGetType && !options.Interop.AllowWrite && !options.AgentCanSuspend;
                var fresh = !ReferenceEquals(options, other) && !ReferenceEquals(options.Constraints, other.Constraints) && !ReferenceEquals(options.Interop, other.Interop);
                Check(snapshot == control && restricted && fresh);
                rows.Add(new { milliseconds, snapshot, control, restricted, fresh });
            }
            foreach (var value in new[] { 0, 999, 1001, 5001 })
            {
                var rejected = false;
                try { factory.Invoke(null, new object[] { value }); }
                catch (TargetInvocationException error) { rejected = error.InnerException is ArgumentOutOfRangeException; }
                Check(rejected);
            }
            var nullRejected = false;
            try { configure.Invoke(null, new object[] { null, 1000 }); }
            catch (TargetInvocationException error) { nullRejected = error.InnerException is ArgumentNullException; }
            Check(nullRejected);
            var report = new { passed = true, componentSha256 = digest, configurationOnly = true, compiledOptionsFactoryInvoked = true,
                workerEntryPointExecuted = false, pageCodeEvaluated = false, nativeIsolationTest = false, invalidBudgetsRejected = 4, nullRejected, rows };
#if CORE_OPTIONS
            Console.WriteLine(JsonSerializer.Serialize(report));
#else
            Console.WriteLine(new JavaScriptSerializer().Serialize(report));
#endif
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
