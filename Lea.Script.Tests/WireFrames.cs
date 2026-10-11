using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using Lea.Script;

namespace Lea.Script.Tests
{
    internal static class WireFrames
    {
        internal static void Run()
        {
            var worker = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker", "Lea.Script.Worker.exe"));
            Sample(typeof(Jp).Assembly, "Lea.Script.Jp", 5);
            Sample(worker, "Lea.Script.Worker.Program", 4);
        }

        private static void Sample(Assembly assembly, string owner, int callCount)
        {
            var wire = assembly.GetType("Lea.Script.Wire", true);
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var factory = wire.GetMethod("Of", flags);
            var label = assembly.GetName().Name;
            Check(factory != null && factory.ReturnType == typeof(Dictionary<string, object>) &&
                factory.GetParameters().Length == 1 && factory.GetParameters()[0].ParameterType == typeof(object[]) &&
                factory.GetParameters()[0].IsDefined(typeof(ParamArrayAttribute), false),
                "compact frame factory is compiled into " + label);
            Check(!assembly.GetTypes().Any(type => type.Name.StartsWith("<>f__AnonymousType", StringComparison.Ordinal)),
                "production runtime has no anonymous frame types: " + label);
            var calls = assembly.GetTypes().Where(type => type.FullName == owner || type.FullName.StartsWith(owner + "+", StringComparison.Ordinal))
                .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                .Where(method => !method.ContainsGenericParameters && method.GetMethodBody() != null)
                .SelectMany(VmOptions.Calls).Count(method => method.DeclaringType == wire && method.Name == "Of");
            Check(calls == callCount, "all production frame writes call the compact factory: " + label + "; " + calls);

            Dictionary<string, object> Of(params object[] fields)
            { return (Dictionary<string, object>)factory.Invoke(null, new object[] { fields }); }

            foreach (var fields in new[] { null, new object[0], new object[] { "k" }, new object[18],
                new object[] { 1, "exec" }, new object[] { "", "exec" }, new object[] { "kind", "exec" },
                new object[] { "z", "exec" }, new object[] { "k", "exec", "k", "timer" } })
            {
                var rejected = false;
                try { Of(fields); }
                catch (TargetInvocationException error) when (error.InnerException is ArgumentException) { rejected = true; }
                Check(rejected, "factory rejects invalid arity, keys or duplicates: " + label + "; length=" + (fields == null ? "null" : fields.Length.ToString()));
            }
            var full = Of("k", "exec", "i", 1, "r", 1, "b", null, "v", null, "s", "ok", "m", "", "d", false);
            Check(full.Count == 8 && full.ContainsKey("b") && full["b"] == null && full.Comparer.Equals(StringComparer.Ordinal),
                "factory admits eight exact case-sensitive fields and null payloads: " + label);
            var other = Of("k", "exec");
            full["k"] = "changed";
            Check((string)other["k"] == "exec", "factory never shares mutable frame dictionaries: " + label);

            const string text = "quoted \"text\";\\path\r\n\u0000\u00e9\u2028";
            var rows = new[]
            {
                Tuple.Create((object)new { k = "init", p = 1234, f = text }, new object[] { "k", "init", "p", 1234, "f", text }),
                Tuple.Create((object)new { k = "hello", g = 15, n = 2, f = 1 }, new object[] { "k", "hello", "g", 15, "n", 2, "f", 1 }),
                Tuple.Create((object)new { k = "exec", i = 17, c = text }, new object[] { "k", "exec", "i", 17, "c", text }),
                Tuple.Create((object)new { k = "timer", i = 18, t = 1 }, new object[] { "k", "timer", "i", 18, "t", 1 }),
                Tuple.Create((object)new { k = "event", i = 19, t = "click", p = new object[] { 1, 2 } }, new object[] { "k", "event", "i", 19, "t", "click", "p", new object[] { 1, 2 } }),
                Tuple.Create((object)new { k = "call", i = 20, r = 1, b = new object[] { "put", null, "t", text } }, new object[] { "k", "call", "i", 20, "r", 1, "b", new object[] { "put", null, "t", text } }),
                Tuple.Create((object)new { k = "reply", i = 20, r = 1, v = (object)null }, new object[] { "k", "reply", "i", 20, "r", 1, "v", null }),
                Tuple.Create((object)new { k = "end", i = 20, s = "ok", m = "", d = true }, new object[] { "k", "end", "i", 20, "s", "ok", "m", "", "d", true }),
                Tuple.Create((object)new { k = "end", i = 21, s = "limit", m = text, d = false }, new object[] { "k", "end", "i", 21, "s", "limit", "m", text, "d", false })
            };
            var write = wire.GetMethod("Write", flags);
            var read = wire.GetMethod("Read", flags);
            foreach (var row in rows)
            {
                using (var legacy = new MemoryStream())
                using (var current = new MemoryStream())
                {
                    write.Invoke(null, new object[] { legacy, row.Item1 });
                    var frame = Of(row.Item2);
                    write.Invoke(null, new object[] { current, frame });
                    Check(current.ToArray().SequenceEqual(legacy.ToArray()), "dictionary frame preserves exact legacy wire bytes: " + label + "; " + frame["k"]);
                    current.Position = 0;
                    var decoded = (Dictionary<string, object>)read.Invoke(null, new object[] { current });
                    var serializer = new JavaScriptSerializer();
                    Check(serializer.Serialize(decoded) == serializer.Serialize(frame),
                        "dictionary frame retains primitive, null, array and escaped-string round trip: " + label + "; " + frame["k"]);
                }
            }
        }

        private static void Check(bool condition, string message) { Program.Check(condition, message); }
    }
}
