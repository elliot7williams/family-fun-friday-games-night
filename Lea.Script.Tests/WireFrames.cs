using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;
using Lea.Script;

namespace Lea.Script.Tests
{
    internal static class WireFrames
    {
        internal static void Run()
        {
            var worker = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker", "Lea.Script.Worker.exe"));
            Sample(typeof(Jp).Assembly, "Lea.Script.Jp", 6);
            Sample(worker, "Lea.Script.Worker.Program", 3);
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
            Check(calls == callCount, "production command frames call the compact factory: " + label + "; " + calls);

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
                Tuple.Create((object)new { k = "frame", i = 18, t = 16.25, p = new object[] { 1, 2 } }, new object[] { "k", "frame", "i", 18, "t", 16.25, "p", new object[] { 1, 2 } }),
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
                using (var current = new PacketStream())
                {
                    write.Invoke(null, new object[] { legacy, row.Item1 });
                    var frame = Of(row.Item2);
                    write.Invoke(null, new object[] { current, frame });
                    Check(current.Writes == 1 && current.Flushes == 1,
                        "one stream write and flush for a bounded packet: " + label + "; " + frame["k"]);
                    Check(current.ToArray().SequenceEqual(legacy.ToArray()), "dictionary frame preserves exact legacy wire bytes: " + label + "; " + frame["k"]);
                    Check(current.ToArray().SequenceEqual(LegacyBytes(frame)),
                        "packet preserves independently encoded version-three header and JSON: " + label + "; " + frame["k"]);
                    current.Position = 0;
                    var decoded = (Dictionary<string, object>)read.Invoke(null, new object[] { current });
                    var serializer = new JavaScriptSerializer();
                    Check(serializer.Serialize(decoded) == serializer.Serialize(frame),
                        "dictionary frame retains primitive, null, array and escaped-string round trip: " + label + "; " + frame["k"]);
                }
            }
            Prepared(wire, label);
            using (var output = new PacketStream())
            {
                var rejected = false;
                try { write.Invoke(null, new object[] { output, Of("k", "exec", "c", new string('x', 131072)) }); }
                catch (TargetInvocationException error) when (error.InnerException is InvalidDataException) { rejected = true; }
                Check(rejected && output.Writes == 0 && output.Flushes == 0 && output.Length == 0,
                    "oversized packet is rejected before any output: " + label);
            }
        }

        private static void Prepared(Type wire, string label)
        {
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var prepare = wire.GetMethod("Hi", flags);
            var send = wire.GetMethod("Hr", flags);
            var read = wire.GetMethod("Read", flags);
            Check(prepare != null && send != null && prepare.ReturnType.IsSealed,
                "bounded prepared hello is compiled into " + label);
            foreach (var guards in new[] { 0, 15 })
                foreach (var file in new[] { false, true })
                {
                    var hello = prepare.Invoke(null, new object[] { guards, file });
                    foreach (var network in new[] { 0, 1, 2 })
                    {
                        var expected = new Dictionary<string, object> { { "k", "hello" }, { "g", guards }, { "n", network }, { "f", file ? 1 : 0 } };
                        using (var output = new PacketStream())
                        {
                            send.Invoke(null, new object[] { output, hello, network });
                            Check(output.Writes == 1 && output.Flushes == 1 && output.ToArray().SequenceEqual(LegacyBytes(expected)),
                                "prepared hello preserves single-write bytes without inventing denial: " + label + "; " + guards + "/" + file + "/" + network);
                            output.Position = 0;
                            var decoded = (Dictionary<string, object>)read.Invoke(null, new object[] { output });
                            Check(decoded.Count == 4 && (string)decoded["k"] == "hello" && (int)decoded["g"] == guards &&
                                (int)decoded["n"] == network && (int)decoded["f"] == (file ? 1 : 0),
                                "prepared hello retains exact typed guard fields: " + label);
                        }
                    }
                }
            foreach (var guards in new[] { -1, 16 })
            {
                var rejected = false;
                try { prepare.Invoke(null, new object[] { guards, true }); }
                catch (TargetInvocationException error) when (error.InnerException is ArgumentOutOfRangeException) { rejected = true; }
                Check(rejected, "prepared hello rejects out-of-range mitigation mask: " + label + "; " + guards);
            }
            var prepared = prepare.Invoke(null, new object[] { 15, true });
            foreach (var network in new[] { -1, 3 })
                using (var output = new PacketStream())
                {
                    var rejected = false;
                    try { send.Invoke(null, new object[] { output, prepared, network }); }
                    catch (TargetInvocationException error) when (error.InnerException is InvalidDataException) { rejected = true; }
                    Check(rejected && output.Writes == 0 && output.Flushes == 0 && output.Length == 0,
                        "prepared hello rejects unknown probe outcomes before output: " + label + "; " + network);
                }
            using (var output = new PacketStream())
            {
                var rejected = false;
                try { send.Invoke(null, new object[] { output, null, 1 }); }
                catch (TargetInvocationException error) when (error.InnerException is ArgumentNullException) { rejected = true; }
                Check(rejected && output.Writes == 0, "null prepared hello cannot write: " + label);
            }
        }

        private static byte[] LegacyBytes(object value)
        {
            var payload = new UTF8Encoding(false, true).GetBytes(new JavaScriptSerializer { MaxJsonLength = 131072, RecursionLimit = 8 }.Serialize(value));
            using (var output = new MemoryStream())
            using (var writer = new BinaryWriter(output))
            {
                writer.Write(0x5341454c); writer.Write(3); writer.Write(payload.Length); writer.Write(payload);
                return output.ToArray();
            }
        }

        private sealed class PacketStream : MemoryStream
        {
            internal int Writes, Flushes;
            public override void Write(byte[] buffer, int offset, int count) { Writes++; base.Write(buffer, offset, count); }
            public override void Flush() { Flushes++; base.Flush(); }
            public override int Read(byte[] buffer, int offset, int count) { return base.Read(buffer, offset, Math.Min(count, 1)); }
        }

        private static void Check(bool condition, string message) { Program.Check(condition, message); }
    }
}
