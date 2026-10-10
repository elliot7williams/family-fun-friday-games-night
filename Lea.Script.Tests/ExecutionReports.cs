using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lea.Engine;
using Lea.Script;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class ExecutionReports
    {
        internal static void Contract()
        {
            var property = typeof(Jp).GetProperty("Ed");
            Program.Check(property != null && property.PropertyType == typeof(string) && property.GetSetMethod() == null,
                "isolated execution exposes the read-only short Ed report");
            var summary = typeof(Jp).GetMethod("ExecutionSummary", BindingFlags.Static | BindingFlags.NonPublic);
            Program.Check(summary != null, "execution summary has its own measured CPU delta boundary");
            var clock = Stopwatch.StartNew();
            foreach (var pair in new[] { new long?[] { 150, 100 }, new long?[] { 100, 100 }, new long?[] { null, 100 },
                new long?[] { 100, null }, new long?[] { 99, 100 } })
            {
                var actual = (string)summary.Invoke(null, new object[] { "read", clock, pair[0], pair[1] });
                var cpu = pair[0].HasValue && pair[1].HasValue && pair[0] >= pair[1] ?
                    (pair[0] - pair[1]).Value + " ms" : "unavailable";
                Program.Check(actual.StartsWith("phase=read; wall=", StringComparison.Ordinal) && actual.EndsWith("; cpu=" + cpu, StringComparison.Ordinal),
                    "execution CPU subtracts the task baseline or stays unavailable: " + cpu);
            }
            var failure = typeof(Jp).GetMethod("StartupFailure", BindingFlags.Static | BindingFlags.NonPublic);
            var message = (string)failure.Invoke(null, new object[] { new string('e', 700), "phase=read; wall=25; cpu=unavailable",
                new string('x', 2000) + " newest execution detail" });
            Program.Check(message.Length == 512 && message.StartsWith(new string('e', 125) + "... phase=read;", StringComparison.Ordinal) &&
                message.EndsWith(" newest execution detail", StringComparison.Ordinal), "execution failure reserves its prefix, summary and newest bounded detail");
        }

        internal static void Run(string fixture)
        {
            Contract();
            foreach (var mode in new[] { "send", "read", "verify", "bridge", "reply", "done", "limit", "eof" })
                Sample(fixture, mode, false).GetAwaiter().GetResult();
            View.Pump(() => Sample(fixture, "cancel", true));
            View.Pump(() => Sample(fixture, "off", true));
        }

        // Controlled transports bypass startup only in tests; they prove no native isolation property.
        private static async Task Sample(string fixture, string mode, bool asynchronous)
        {
            var engine = new E();
            var page = engine.Go(fixture);
            var title = page.T;
            using (var entered = new ManualResetEvent(false))
            using (var release = new ManualResetEvent(false))
            using (var exited = new ManualResetEvent(false))
            using (var script = new Jp(engine, page))
            {
                var blocked = mode == "read" || mode == "cancel" || mode == "off";
                var response = new MemoryStream();
                if (mode == "verify") Wire.Write(response, new { k = "end", i = 99, s = "ok", m = "", d = false });
                else if (mode == "bridge" || mode == "reply") Wire.Write(response, new { k = "call", i = 1, r = 1,
                    b = mode == "bridge" ? new object[0] : new object[] { "t" } });
                else if (mode != "eof") Wire.Write(response, new { k = "end", i = 1, s = mode == "limit" ? "limit" : "ok", m = "", d = false });
                response.Position = 0;
                var output = blocked ? (Stream)new WaitingRead(entered, release, exited) : response;
                var endpoint = Inject(script, new Writes(mode), output);
                if (mode == "read") script.Tm = 25;
                Jr result;
                try
                {
                    if (asynchronous)
                    {
                        var pending = script.ExA("1;");
                        if (!await Task.Run(() => entered.WaitOne(2000))) throw new Exception("Controlled read never entered");
                        if (mode == "cancel") script.St(); else engine.Je = false;
                        result = await pending;
                    }
                    else result = script.Ex("1;");
                    var phase = mode == "limit" ? "done" : mode == "eof" || mode == "cancel" || mode == "off" ? "read" : mode;
                    var status = mode == "send" || mode == "reply" || mode == "eof" ? "worker" :
                        mode == "verify" || mode == "bridge" ? "protocol" : mode == "read" ? "timeout" :
                        mode == "done" ? "ok" : mode;
                    Console.WriteLine("Execution report " + mode + ": " + result.S + "; " + result.M + "; " + script.Ed);
                    Program.Check(result.S == status && result.N == 0 && page.T == title, "execution reporting preserves result and document state: " + mode);
                    Program.Check(script.Ed.StartsWith("phase=" + phase + "; wall=", StringComparison.Ordinal) && script.Ed.Length <= 512,
                        "execution report records the actual parent boundary: " + mode);
                    Program.Check(script.Ed.Contains("; cpu=unavailable") && !script.Ed.Contains("; cpu=0 ms"),
                        "controlled missing CPU handle is unavailable: " + mode);
                    Program.Check(script.Ed.EndsWith(" newest execution detail", StringComparison.Ordinal), "execution report preserves the stderr tail: " + mode);
                    if (!result.Ok)
                        Program.Check(script.Pid == 0 && endpoint.J == null && result.M.Length <= 512 &&
                            (mode == "limit" || result.M.Contains("phase=" + phase + ";")), "failed execution snapshots diagnostics before retirement: " + mode);
                    else Program.Check(result.M == "", "successful execution does not add diagnostics to its result message");
                    var snapshot = result.M;
                    engine.Je = true;
                    var rejected = script.Ex(new string(' ', 16385));
                    Program.Check(rejected.S == "code" && script.Ed == "" && result.M == snapshot,
                        "a non-execution clears Ed without changing the previous result: " + mode);
                    if (blocked) Program.Check(exited.WaitOne(2000), "controlled blocked producer retires: " + mode);
                }
                finally { release.Set(); script.Dispose(); if (blocked && !exited.WaitOne(2000)) throw new Exception("Controlled producer leaked"); response.Dispose(); }
            }
        }

        private static Rp Inject(Jp script, Stream input, Stream output)
        {
            var job = (Jb)typeof(Jb).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single().Invoke(new object[] { IntPtr.Zero, 2500 });
            var errors = new MemoryStream(Encoding.UTF8.GetBytes(new string('x', 1500) + " newest execution detail"));
            var endpoint = (Rp)typeof(Rp).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single()
                .Invoke(new object[] { null, input, output, errors, job, true, true, true, true, true });
            if (!SpinWait.SpinUntil(() => endpoint.E.EndsWith(" newest execution detail", StringComparison.Ordinal), 2000))
                throw new Exception("Controlled stderr never drained");
            typeof(Jp).GetField("worker", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(script, endpoint);
            return endpoint;
        }

        internal static void Native(string fixture)
        {
            var engine = new E();
            var page = engine.Go(fixture);
            using (var script = new Jp(engine, page))
            {
                var warm = script.Ex("1;");
                Program.Check(warm.Ok, "native execution report warmup: " + warm.S + "; " + warm.M + "; " + script.Sd);
                Program.Check(script.Ed.StartsWith("phase=done;", StringComparison.Ordinal) && !script.Ed.Contains("unavailable") && script.Ed.Length <= 512,
                    "native successful execution retains measured task CPU separately from startup");
                Program.Check(script.Ac && script.Ne && script.Nd && script.Fd && script.Mg == 15 && script.Cp == 2500 && script.Mm == 134217728 && script.Tm == 2000,
                    "native execution reporting preserves isolation and default budgets");
                var startup = script.Sd;
                var pid = script.Pid;
                script.Tm = 25;
                var clock = Stopwatch.StartNew();
                var timeout = script.Ex("/(a+)+$/.test('a'.repeat(120)+'!');");
                Console.WriteLine("Native execution timeout: " + timeout.S + "; " + timeout.M + "; " + script.Ed);
                Program.Check(timeout.S == "timeout" && timeout.M.StartsWith("Worker execution deadline exceeded. phase=", StringComparison.Ordinal) &&
                    timeout.M.Length <= 512 && script.Ed.Length <= 512 && clock.ElapsedMilliseconds < 2000,
                    "native parent deadline retains bounded execution phase diagnostics");
                Program.Check(script.Pid == 0 && Exited(pid) && script.Sd == startup,
                    "native deadline kills the exact child without overwriting startup diagnostics");
                script.Tm = 2000;
                var recovery = script.Ex("1;");
                Program.Check(recovery.Ok && script.Ed.StartsWith("phase=done;", StringComparison.Ordinal),
                    "native fresh-worker recovery creates a new execution report: " + recovery.S + "; " + recovery.M + "; " + script.Sd);
                Program.Check(timeout.M.Contains("Worker execution deadline exceeded. phase=") && timeout.M.Length <= 512,
                    "native failure snapshot remains available after successful recovery");
            }
        }

        private static bool Exited(int pid)
        { try { using (var process = Process.GetProcessById(pid)) return process.WaitForExit(1000); } catch (ArgumentException) { return true; } }
        private sealed class Writes : MemoryStream
        {
            private readonly string mode;
            private int frames;
            internal Writes(string mode) { this.mode = mode; }
            public override void Write(byte[] buffer, int offset, int count)
            { if (mode == "send" || mode == "reply" && frames > 0) throw new IOException("controlled " + mode + " failure"); base.Write(buffer, offset, count); }
            public override void Flush() { frames++; base.Flush(); }
        }
        private sealed class WaitingRead : Stream
        {
            private readonly ManualResetEvent entered, release, exited;
            internal WaitingRead(ManualResetEvent entered, ManualResetEvent release, ManualResetEvent exited)
            { this.entered = entered; this.release = release; this.exited = exited; }
            public override int Read(byte[] buffer, int offset, int count)
            { entered.Set(); try { release.WaitOne(); return 0; } finally { exited.Set(); } }
            protected override void Dispose(bool disposing) { release.Set(); base.Dispose(disposing); }
            public override bool CanRead { get { return true; } }
            public override bool CanWrite { get { return false; } }
            public override bool CanSeek { get { return false; } }
            public override long Length { get { throw new NotSupportedException(); } }
            public override long Position { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }
            public override void Flush() { }
            public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
            public override void SetLength(long value) { throw new NotSupportedException(); }
            public override void Write(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }
        }
    }
}
