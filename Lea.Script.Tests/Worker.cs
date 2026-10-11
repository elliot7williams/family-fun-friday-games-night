using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Script;

namespace Lea.Script.Tests
{
    internal static class Worker
    {
        internal static void Run(string fixture)
        {
            Protocol();
            var engine = new E();
            var page = engine.Go(fixture);
            Bridge(engine, page);
            var field = page.Fs.Single(item => item.El.Av("id") == "field");
            var result = engine.Id(page, "result");
            int pid;
            string sid;
            using (var script = new Jp(engine, page))
            {
                var execution = script.Ex(engine.Q(page, "script").C[0].X);
                Check(execution.Ok && execution.N == 6, "isolated computed script: " + execution.S + "; " + execution.M + "; " + script.Sd);
                pid = script.Pid;
                sid = script.Sid;
                Check(typeof(Jp).GetField("io", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(script) == null,
                    "completed native execution retains no owned parent I/O dispatcher");
                Check(pid != 0 && pid != Process.GetCurrentProcess().Id, "script runs outside the browser process");
                Check(script.Ac && script.Ne && script.Nd && script.Fd && script.Mg == 15,
                    "AppContainer, no loopback exemption, controlled network/file probes, and mitigations: network=" + script.Np);
                Check(script.Mm == 128L * 1024 * 1024 && script.Cp == 2500, "verified memory and CPU Job limits");
                Check(page.T == "Lea 7" && result.C[0].X == "Total: 7" && field.V == "{\"total\":7}", "worker DOM and retained fields");
                Check(script.Ex("if (result !== document.querySelector('#result')) throw new Error('identity'); var privateState = 37; document.querySelector('.item').textContent = 'worker';").Ok, "worker stable wrappers and anonymous-node writes");
                Check(script.Ex("if (privateState !== 37 || document.querySelector('.item').textContent !== 'worker') throw new Error('state'); document.title = '${literal}; computed ' + (2 + 3);").Ok && page.T == "${literal}; computed 5", "worker persistent state and literal values");
                Check(script.Pid == pid && typeof(Jp).GetField("io", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(script) == null,
                    "later native execution retains its realm without an idle parent I/O dispatcher");
                Check(script.Ex("if (typeof System !== 'undefined' || typeof __lea !== 'undefined' || typeof fetch !== 'undefined' || typeof document.GetType !== 'undefined') throw new Error('capability');").Ok, "worker exposes no CLR or network API");
                var runtime = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker");
                var files = Directory.GetFiles(runtime);
                Check(files.All(file => HasSid(file, sid)), "exact worker runtime files have session ACLs");
                Check(!HasSid(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lea.Script.dll"), sid) &&
                    !HasSid(runtime, sid), "no folder or parent-runtime permission grant");
                try { using (File.Open(files[0], FileMode.Open, FileAccess.Write, FileShare.ReadWrite)) { } throw new Exception("worker file was writable"); }
                catch (IOException) { Check(true, "worker binaries locked during execution"); }
                using (var other = new Jp(engine, page))
                {
                    var separate = other.Ex("if (typeof privateState !== 'undefined') throw new Error('session leak');");
                    Check(separate.Ok && other.Sid != sid, "independent session SID and realm: " + separate.S + "; " + separate.M);
                    Check(script.Ex("if (privateState !== 37) throw new Error('other session');").Ok && script.Pid == pid, "one session does not reset another");
                }
                var child = engine.Id(page, "child");
                Check(script.Ex("var oldChild = document.getElementById('child'); document.getElementById('parent').textContent = 'replaced';").Ok && child.Up == null, "worker subtree detachment");
                var stale = script.Ex("oldChild.textContent = 'invalid';");
                Check(stale.S == "js" && script.Pid == 0 && Exited(pid), "stale mutation fails and worker exits");
                Check(script.Ex("if (typeof privateState !== 'undefined') throw new Error('failed realm');").Ok, "worker failure recovery creates a fresh realm");
                var partial = script.Ex("document.title = 'partial'; throw new Error('failure');");
                Check(partial.S == "js" && partial.N == 1 && page.T == "partial", "isolated failure retains completed mutations");
                // Allow cooperative limits to win even on a cold, CPU-capped CLR worker.
                // The independent short hard-deadline test below still uses 25 ms.
                script.Tm = 5000;
                var cooperativeWarmup = script.Ex("function warm(n) { return n ? warm(n-1) : 0; } warm(16);");
                Check(cooperativeWarmup.Ok, "cooperative function warmup: " + cooperativeWarmup.S + "; " + cooperativeWarmup.M + "; " + script.Sd);
                var loop = script.Ex("while (true) {}");
                Check(loop.S == "limit", "isolated infinite loop budget: " + loop.S + "; " + loop.M);
                cooperativeWarmup = script.Ex("function warm(n) { return n ? warm(n-1) : 0; } warm(16);");
                Check(cooperativeWarmup.Ok, "fresh-worker recursion warmup: " + cooperativeWarmup.S + "; " + cooperativeWarmup.M + "; " + script.Sd);
                var recursion = script.Ex("function f() { return f(); } f();");
                Check(recursion.S == "limit", "isolated recursion budget: " + recursion.S + "; " + recursion.M);
                var calls = script.Ex("for (let i=0; i<300; i++) document.title;");
                Check(calls.S == "limit", "isolated bridge-call budget: " + calls.S + "; " + calls.M);
                Check(script.Ex("let = ;").S == "js", "isolated syntax failure");
                script.Tm = 2000;
                Check(script.Ex(new string(' ', 16385)).S == "code", "isolated source-size limit");
                Check(script.Ex("var revoked = 1;").Ok, "worker permission setup");
                pid = script.Pid;
                engine.Je = false;
                engine.Je = true;
                Check(script.Ex("if (typeof revoked !== 'undefined') throw new Error('permission realm');").Ok && Exited(pid), "between-call permission revocation kills worker state");
                engine.Je = false;
                Check(script.Ex("document.title='off';").S == "off" && script.At.N == 0 && page.T == "partial", "disabled worker execution and clean result");
                engine.Je = true;
                var deadlineWarmup = script.Ex("1;");
                Check(deadlineWarmup.Ok, "worker warmup for hard deadline: " + deadlineWarmup.S + "; " + deadlineWarmup.M + "; " + script.Sd);
                pid = script.Pid;
                script.Tm = 25;
                var watch = Stopwatch.StartNew();
                var timeout = script.Ex("/(a+)+$/.test('a'.repeat(120) + '!');");
                Check(timeout.S == "timeout" && timeout.M.StartsWith("Worker execution deadline exceeded. phase=", StringComparison.Ordinal) && timeout.M.Length <= 512 &&
                    script.Ed.StartsWith("phase=", StringComparison.Ordinal) && watch.ElapsedMilliseconds < 2000 && script.Pid == 0 && Exited(pid), "parent deadline kills native regex: " + timeout.S + "; " + timeout.M + "; " + watch.ElapsedMilliseconds + " ms");
                script.Tm = 2000;
                Check(script.Ex("document.title='recovered';").Ok, "recovery after hard worker kill");
                Check(script.Ex("queueMicrotask(function(){});").Ok, "microtask hard-deadline warmup");
                pid = script.Pid;
                script.Tm = 25;
                watch.Restart();
                timeout = script.Ex("queueMicrotask(function(){/(a+)+$/.test('a'.repeat(120)+'!');document.title='late microtask';});");
                Check(timeout.S == "timeout" && timeout.M.StartsWith("Worker execution deadline exceeded. phase=", StringComparison.Ordinal) && timeout.M.Length <= 512 &&
                    script.Ed.StartsWith("phase=", StringComparison.Ordinal) && watch.ElapsedMilliseconds < 2000 && script.Pid == 0 && Exited(pid) && page.T == "recovered",
                    "parent deadline also kills native work inside microtask: " + timeout.S);
                script.Tm = 2000;
                Check(script.Ex("document.title='recovered';").Ok, "fresh worker after microtask hard deadline");
                pid = script.Pid;
                var pressure = script.Ex("'x'.repeat(100000000);");
                Check(!pressure.Ok && script.Pid == 0 && Exited(pid), "oversized native allocation cannot take down parent: " + pressure.S);
                Check(script.Ex("document.title='memory recovered';").Ok && page.T == "memory recovered", "recovery after memory pressure");
                Exception crossThread = null;
                var thread = new Thread(() => { try { script.Ex("1;"); } catch (Exception error) { crossThread = error; } });
                thread.Start();
                thread.Join();
                Check(crossThread is InvalidOperationException, "isolated session thread affinity");
                var stop = Task.Run(() => { Thread.Sleep(20); script.St(); });
                pid = script.Pid;
                var stopped = script.Ex("/(a+)+$/.test('a'.repeat(120) + '!');");
                stop.GetAwaiter().GetResult();
                Check(stopped.S == "cancel" && Exited(pid), "thread-safe cancellation terminates worker");
                var recovery = script.Ex("1;");
                Check(recovery.Ok, "recovery after cancellation: " + recovery.S + "; " + recovery.M + "; " + script.Sd);
                Check(script.Mp > 0 && script.Mp <= script.Mm, "recorded worker Job peak: " + script.Mp + " bytes");
                sid = script.Sid;
                pid = script.Pid;
            }
            Check(Exited(pid), "dispose kills persistent script worker");
            Check(Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker")).All(file => !HasSid(file, sid)), "dispose removes exact session ACLs");
            using (var bad = new Jp(engine, page, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missing-worker")))
                Check(bad.Ex("1;").S == "setup" && bad.Pid == 0, "missing worker fails closed without in-process fallback");
            using (var abandoned = new Jp(engine, page))
            {
                var setup = abandoned.Ex("1;");
                Check(setup.Ok, "active-page test setup: " + setup.S + "; " + setup.M + "; " + abandoned.Sd);
                pid = abandoned.Pid;
                engine.Go(fixture);
                Check(abandoned.Ex("document.title='abandoned';").S == "stale" && Exited(pid) && page.T == "memory recovered", "navigation invalidates bound worker page");
            }
            var disposed = new Jp(engine, engine.At);
            disposed.Dispose();
            try { disposed.Ex("1;"); throw new Exception("disposed worker accepted code"); }
            catch (ObjectDisposedException) { Check(true, "disposed worker session rejected"); }
            using (var noContext = new Jp(engine, engine.At))
            {
                try { noContext.ExA("1;"); throw new Exception("async without context accepted"); }
                catch (InvalidOperationException) { Check(true, "async without owner context rejected"); }
            }
            Ui(fixture);
        }

        internal static void Ui(string fixture)
        {
            var engine = new E();
            var page = engine.Go(fixture);
            var previous = SynchronizationContext.Current;
            Exception failure = null;
            using (var context = new ApplicationContext())
            using (var timer = new System.Windows.Forms.Timer { Interval = 5 })
            using (var script = new Jp(engine, page))
            {
                var warmup = script.Ex("1;");
                Check(warmup.Ok, "async worker warmup: " + warmup.S + "; " + warmup.M + "; " + script.Sd);
                var ticks = 0;
                var started = false;
                var revoke = false;
                timer.Tick += async (sender, args) =>
                {
                    ticks++;
                    if (revoke) { revoke = false; engine.Je = false; }
                    if (started) return;
                    started = true;
                    try
                    {
                        var before = ticks;
                        var execution = script.ExA("for(let i=0;i<200;i++) document.title; document.title='async updated';");
                        try { script.Ex("1;"); throw new Exception("reentrant execution accepted"); }
                        catch (InvalidOperationException) { Check(true, "async reentrant execution rejected"); }
                        try { script.Dispose(); throw new Exception("running dispose accepted"); }
                        catch (InvalidOperationException) { Check(true, "running async disposal rejected"); }
                        var result = await execution;
                        Check(result.Ok && result.N == 1 && page.T == "async updated" && ticks > before, "async RPC keeps UI ticking and mutates on owner thread: " + result.S);
                        var pid = script.Pid;
                        revoke = true;
                        result = await script.ExA("/(a+)+$/.test('a'.repeat(120) + '!'); document.title='must not apply';");
                        Check(result.S == "off" && page.T == "async updated" && Exited(pid), "live UI permission revocation prevents later mutation");
                        engine.Je = true;
                        Check((await script.ExA("document.title='async recovered';")).Ok, "async worker recovery");
                        pid = script.Pid;
                        var running = script.ExA("/(a+)+$/.test('a'.repeat(120) + '!');");
                        var stop = Task.Run(() => { Thread.Sleep(20); script.St(); });
                        result = await running;
                        await stop;
                        Check(result.S == "cancel" && Exited(pid), "async cancellation completes without blocking UI");
                    }
                    catch (Exception error) { failure = error; }
                    finally { timer.Stop(); context.ExitThread(); }
                };
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
                timer.Start();
                Application.Run(context);
            }
            SynchronizationContext.SetSynchronizationContext(previous);
            if (failure != null) throw failure;
        }

        internal static void Protocol()
        {
            using (var stream = new MemoryStream())
            {
                Wire.Write(stream, new { k = "call", i = 17, r = 3, b = new object[] { "put", null, "t", "${x}; data" } });
                stream.Position = 0;
                var frame = Wire.Read(stream);
                Wire.Fields(frame, "k", "i", "r", "b");
                Check(Wire.Number(frame, "i") == 17 && ((object[])frame["b"])[3].Equals("${x}; data"), "framed primitive protocol round trip");
                var bytes = stream.ToArray();
                bytes[0] ^= 1;
                Reject(() => Wire.Read(new MemoryStream(bytes)), "protocol magic rejected");
                bytes[0] ^= 1;
                bytes[4] = 1;
                Reject(() => Wire.Read(new MemoryStream(bytes)), "protocol version rejected");
                bytes[4] = 2;
                Reject(() => Wire.Read(new MemoryStream(bytes)), "pre-timer worker protocol version rejected");
                bytes[4] = Wire.Version;
                Array.Copy(BitConverter.GetBytes(Wire.MaxFrame + 1), 0, bytes, 8, 4);
                Reject(() => Wire.Read(new MemoryStream(bytes.Take(12).ToArray())), "oversized frame rejected before payload read");
                Reject(() => Wire.Read(new MemoryStream(new byte[5])), "truncated protocol header rejected");
                Reject(() => Wire.Fields(frame, "k", "i"), "unexpected protocol fields rejected");
                Reject(() => Wire.Number(new Dictionary<string, object> { { "i", 1.5 } }, "i"), "noninteger execution ID rejected");
                Reject(() => Wire.Text(new Dictionary<string, object> { { "k", "123456789" } }, "k", 8), "protocol string bound");
                Reject(() => Wire.Write(new MemoryStream(), new { data = new string('x', Wire.MaxFrame) }), "oversized outgoing frame rejected");
            }
            Reject(() => Wire.Read(Frame("[]")), "nonobject protocol root rejected");
            Reject(() => Wire.Read(Frame("{")), "malformed protocol JSON rejected");
            Reject(() => Wire.Read(Frame("{\"x\":[[[[[[[[[[1]]]]]]]]]]}")), "protocol recursion bound");
            var invalid = Frame("{\"x\":\"z\"}").ToArray();
            invalid[18] = 255;
            Reject(() => Wire.Read(new MemoryStream(invalid)), "invalid protocol UTF-8 rejected");
            var request = 0;
            var call = new Dictionary<string, object> { { "k", "call" }, { "i", 1 }, { "r", 1 }, { "b", new object[] { "t" } } };
            Check(Wire.Step(call, 1, ref request) == "call" && request == 1, "parent accepts ordered execution requests");
            Reject(() => Wire.Step(call, 1, ref request), "replayed request rejected");
            call["r"] = 3;
            Reject(() => Wire.Step(call, 1, ref request), "skipped request rejected");
            call["r"] = 2;
            Reject(() => Wire.Step(call, 2, ref request), "foreign execution ID rejected");
            call["b"] = "not arguments";
            Reject(() => Wire.Step(call, 1, ref request), "nonarray request arguments rejected");
            call["b"] = new object[] { "t" };
            request = 256;
            call["r"] = 257;
            Reject(() => Wire.Step(call, 1, ref request), "parent request ceiling enforced independently");
            var end = new Dictionary<string, object> { { "k", "end" }, { "i", 1 }, { "s", "unknown" }, { "m", "" }, { "d", false } };
            Reject(() => Wire.Step(end, 1, ref request), "unknown worker result rejected");
            end["s"] = "ok";
            end["m"] = new string('x', 513);
            Reject(() => Wire.Step(end, 1, ref request), "worker message size bound");
            end["m"] = "";
            end["d"] = "false";
            Reject(() => Wire.Step(end, 1, ref request), "worker cancellation flag is strictly boolean");
            end.Remove("d");
            Reject(() => Wire.Step(end, 1, ref request), "worker cancellation flag is required");
        }

        private static void Bridge(E engine, P page)
        {
            var bridge = new Ja(engine, page);
            var key = bridge.Call(new object[] { "id", "result" });
            Reject(() => bridge.Call(new object[] { "put", -1, "tx", "forged" }), "negative node handle rejected by parent");
            Reject(() => bridge.Call(new object[] { "put", 999, "tx", "forged" }), "unknown node handle rejected by parent");
            Reject(() => bridge.Call(new object[] { "put", 0.0, "tx", "forged" }), "noninteger node handle rejected by parent");
            Reject(() => bridge.Call(new object[] { "fs", key }), "unknown capability rejected by parent");
            Reject(() => bridge.Call(new object[] { "t", key }), "capability arity enforced by parent");
            Check(!(bool)bridge.Call(new object[] { "put", key, "go", "https://example.com" }) &&
                !(bool)bridge.Call(new object[] { "put", key, "tx", new string('x', 513) }), "typed mutation whitelist and text bounds in parent");
            try { bridge.Call(new object[] { "id", new string('x', 1025) }); throw new Exception("large argument accepted"); }
            catch (Ja.LimitException) { Check(true, "parent bridge string bound"); }
            bridge.Begin();
            for (var i = 0; i < 256; i++) bridge.Call(new object[] { "t" });
            try { bridge.Call(new object[] { "t" }); throw new Exception("large call count accepted"); }
            catch (Ja.LimitException) { Check(true, "parent bridge-call ceiling independent of interpreter"); }
        }

        private static MemoryStream Frame(string json)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            var bytes = Encoding.UTF8.GetBytes(json);
            writer.Write(0x5341454c); writer.Write(Wire.Version); writer.Write(bytes.Length); writer.Write(bytes);
            stream.Position = 0;
            return stream;
        }

        private static void Reject(Action action, string name)
        {
            try { action(); }
            catch (IOException) { Check(true, name); return; }
            catch (InvalidDataException) { Check(true, name); return; }
            throw new InvalidOperationException("FAIL " + name);
        }

        private static bool HasSid(string path, string sid)
        {
            var security = File.GetAccessControl(path, AccessControlSections.Access);
            return security.GetAccessRules(true, true, typeof(SecurityIdentifier)).Cast<FileSystemAccessRule>()
                .Any(rule => rule.IdentityReference.Value == sid);
        }

        private static bool Exited(int pid)
        {
            try { using (var process = Process.GetProcessById(pid)) return process.WaitForExit(1000); }
            catch (ArgumentException) { return true; }
        }

        private static void Check(bool condition, string name) { Program.Check(condition, name); }
    }
}
