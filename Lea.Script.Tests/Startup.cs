using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Script;
using Lea.Script.Worker;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class Startup
    {
        internal static void ProfileErrors()
        {
            foreach (var assembly in new[] { typeof(Jp).Assembly, typeof(V).Assembly })
            {
                var method = assembly.GetType("Lea.Win.Rp", true).GetMethod("ProfileResult", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                ProfileErrors((Action<int>)Delegate.CreateDelegate(typeof(Action<int>), method), assembly.GetName().Name);
            }
        }

        private static void ProfileErrors(Action<int> report, string owner)
        {
            report(0);
            Check(true, owner + ": successful profile HRESULT remains successful");
            foreach (var code in new[] { unchecked((int)0x80070070), unchecked((int)0x80004005) })
            {
                IOException failure = null;
                try { report(code); }
                catch (IOException error) { failure = error; }
                var native = failure == null ? null : failure.InnerException as COMException;
                Check(native != null && native.ErrorCode == code, owner + ": profile COM failure becomes I/O with original HRESULT: " + code.ToString("X8"));
                Check(failure != null && native != null && failure.Message.Contains("0x" + code.ToString("X8")) && failure.Message.Contains(native.Message),
                    owner + ": profile I/O report retains native code and description: " + code.ToString("X8"));
            }
            try { report(unchecked((int)0x80070005)); throw new Exception("Access denial was ignored"); }
            catch (UnauthorizedAccessException) { Check(true, owner + ": profile access denial keeps its typed exception"); }
            try { report(unchecked((int)0x80070057)); throw new Exception("Invalid argument was ignored"); }
            catch (ArgumentException) { Check(true, owner + ": profile invalid argument keeps its typed exception"); }
            try { report(unchecked((int)0x8007000E)); throw new Exception("Native memory failure was ignored"); }
            catch (OutOfMemoryException) { Check(true, owner + ": critical profile memory failure is not hidden as I/O"); }
        }

        // Opt-in evidence for an already disk-full machine; never fills a drive to create the failure.
        internal static void Unavailable(string fixture)
        {
            UnavailableCore(fixture, false).GetAwaiter().GetResult();
            View.Pump(() => UnavailableCore(fixture, true));
            View.Pump(() => UnavailableView(fixture));
        }

        internal static void LaunchFailures(string fixture)
        {
            UnavailableCore(fixture, false, true).GetAwaiter().GetResult();
            View.Pump(() => UnavailableCore(fixture, true, true));
            View.Pump(() => UnavailableView(fixture, true));
        }

        private static async Task UnavailableCore(string fixture, bool asynchronous, bool simulated = false)
        {
            var engine = new E();
            var page = engine.Go(fixture);
            var title = page.T;
            var runtime = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker");
            var files = Directory.GetFiles(runtime).OrderBy(file => file).ToArray();
            var rules = files.Select(ProfileRules).ToArray();
            var launches = 0;
            using (var script = simulated ? new Jp(engine, page, runtime, supplied =>
                {
                    launches++;
                    if (supplied.Count != 8) throw new Exception("Unexpected worker file list");
                    Rp.ProfileResult(unchecked((int)0x80070070));
                    throw new Exception("Profile failure was ignored");
                }) : new Jp(engine, page))
                for (var attempt = 0; attempt < 2; attempt++)
                {
                    var result = asynchronous ? await script.ExA("document.title='must not run';") : script.Ex("document.title='must not run';");
                    var label = "simulated=" + simulated + "; async=" + asynchronous + "; attempt=" + attempt;
                    Console.WriteLine("Unavailable worker " + label + ": " + result.S + "; " + result.M);
                    Check(result.S == "setup" && result.M.Contains("0x80070070") && result.M.Contains("phase=launch;") && result.M.Length <= 512,
                        "disk-full launch returns bounded setup diagnostics: " + label);
                    Check(result.N == 0 && page.T == title && script.Pid == 0 && !script.Ac && !script.Ne && !script.Nd && !script.Fd,
                        "unavailable isolation cannot execute page code or fall back: " + label);
                    var leases = (System.Collections.ICollection)typeof(Jp).GetField("leases", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(script);
                    var running = (bool)typeof(Jp).GetField("running", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(script);
                    Check(leases.Count == 0 && !running && (!simulated || launches == attempt + 1) && files.Select(ProfileRules).SequenceEqual(rules),
                        "failed launch releases leases, clears running state and preserves ACLs: " + label);
                }
        }

        private static string ProfileRules(string path)
        { return File.GetAccessControl(path, AccessControlSections.Access).GetSecurityDescriptorSddlForm(AccessControlSections.Access); }

        private static async Task UnavailableView(string fixture, bool simulated = false)
        {
            var errors = 0;
            ThreadExceptionEventHandler failed = (sender, args) => errors++;
            Application.ThreadException += failed;
            try
            {
                using (var form = new Form { ClientSize = new System.Drawing.Size(940, 820), ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new System.Drawing.Point(-20000, -20000) })
                using (var view = new V { Dock = DockStyle.Fill })
                {
                    form.Controls.Add(view);
                    Lea.Script.Win.W.On(view);
                    form.Show();
                    var page = await view.GoA(fixture);
                    var title = page.T;
                    Check(view.Xo != null && view.Xo.Ok && !view.Busy,
                        "script-free page remains usable when worker isolation is unavailable");
                    if (simulated)
                    {
                        var adapter = (Lea.Script.Win.W)view.Xs;
                        var engine = (E)typeof(Lea.Script.Win.W).GetField("engine", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(adapter);
                        adapter.At.Dispose();
                        var replacement = new Jp(engine, page, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker"), supplied =>
                        {
                            Rp.ProfileResult(unchecked((int)0x80070070));
                            throw new Exception("Profile failure was ignored");
                        });
                        typeof(Lea.Script.Win.W).GetProperty("At").SetValue(adapter, replacement, null);
                    }
                    for (var attempt = 0; attempt < 2; attempt++)
                    {
                        var result = await view.JsA("document.title='must not run';");
                        Check(result.S == "setup" && result.M.Contains("0x80070070") && result.M.Length <= 512,
                            "native view receives typed disk-full status: " + attempt);
                        Check(view.At == page && view.T == title && view.Xn == 0 && !view.Busy && errors == 0,
                            "native view remains idle without late writes or UI exception: " + attempt);
                    }
                    View.Paint(view, null);
                }
            }
            finally { Application.ThreadException -= failed; }
        }

        internal static void Compare(string fixture, string baseline)
        {
            baseline = Path.GetFullPath(baseline);
            var current = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker");
            var names = Directory.GetFiles(current).Select(Path.GetFileName).OrderBy(name => name).ToArray();
            Check(names.SequenceEqual(Directory.GetFiles(baseline).Select(Path.GetFileName).OrderBy(name => name)),
                "bootstrap comparison runtime inventories match");
            foreach (var name in names.Where(name => name != "Lea.Script.Worker.exe"))
                Check(Digest(Path.Combine(current, name)) == Digest(Path.Combine(baseline, name)),
                    "bootstrap comparison dependency matches: " + name);
            Console.WriteLine("Bootstrap baseline worker: " + Digest(Path.Combine(baseline, "Lea.Script.Worker.exe")));
            Console.WriteLine("Bootstrap current worker: " + Digest(Path.Combine(current, "Lea.Script.Worker.exe")));
            var trace = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE");
            var passed = true;
            try
            {
                Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", null);
                for (var pair = 0; pair < 3; pair++)
                    foreach (var source in pair % 2 == 0 ? new[] { baseline, current } : new[] { current, baseline })
                    {
                        var directory = Path.Combine(Path.GetTempPath(), "Lea-bootstrap-compare-" + Guid.NewGuid().ToString("N"));
                        Directory.CreateDirectory(directory);
                        var files = names.Select(name => Path.Combine(directory, name)).ToArray();
                        foreach (var file in files) File.Copy(Path.Combine(source, Path.GetFileName(file)), file);
                        var engine = new E();
                        var page = engine.Go(fixture);
                        var script = new Jp(engine, page, directory);
                        string sid = null;
                        int pid = 0;
                        try
                        {
                            var result = script.Ex("document.title='bootstrap comparison';");
                            Console.WriteLine("Bootstrap " + (source == baseline ? "baseline" : "current") + " pair=" + pair + ": " +
                                result.S + "; " + result.M + "; " + script.Sd + "; runtime=" + directory);
                            if (!result.Ok || page.T != "bootstrap comparison" || result.N != 1)
                            {
                                passed = false;
                                Console.WriteLine("Retaining failed comparison runtime: " + directory);
                                continue;
                            }
                            Check(true,
                                "bootstrap comparison first execution: " + pair + "; " + result.S + "; " + result.M);
                            Check(script.Ac && script.Ne && script.Nd && script.Fd && script.Mg == 15 &&
                                script.Cp == 2500 && script.Mm == 128L * 1024 * 1024 && script.Tm == 2000,
                                "bootstrap comparison preserves isolation and defaults");
                            sid = script.Sid;
                            pid = script.Pid;
                        }
                        finally { script.Dispose(); }
                        Console.WriteLine("Bootstrap native peak: " + script.Mp + " bytes");
                        Check(Exited(pid) && files.All(file => !HasSid(file, sid)), "bootstrap comparison retires worker and exact ACLs");
                        foreach (var file in files) File.Delete(file);
                        Directory.Delete(directory, false);
                    }
                Check(passed, "bootstrap comparison all six launches pass without retries");
            }
            finally { Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", trace); }
        }

        private static string Digest(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        internal static void ParseMarker(string fixture)
        {
            var options = new Jint.Options();
            using (var vm = new Jint.Engine(options))
            {
                var calls = 0;
                Jint.Runtime.Debugger.DebugHandler.BeforeEvaluateEventHandler parsed = (sender, ast) => calls++;
                vm.Debugger.BeforeEvaluate += parsed;
                vm.Evaluate("1;");
                Check(calls == 1 && !options.Debugger.Enabled, "parsed AST callback works without stepping mode");
                vm.Debugger.BeforeEvaluate -= parsed;
                vm.Evaluate("2;");
                Check(calls == 1, "parsed AST callback is removed before subsequent page evaluation");
            }
            var previous = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE");
            try
            {
                foreach (var timing in new[] { false, true })
                {
                    Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", timing ? "1" : null);
                    var engine = new E();
                    var page = engine.Go(fixture);
                    using (var script = new Jp(engine, page))
                    {
                        var result = script.Ex("1;");
                        Check(result.Ok && script.Ac && script.Ne && script.Nd && script.Fd && script.Mg == 15,
                            "phase worker preserves verified isolation: timing=" + timing + "; " + result.S + "; " + result.M + "; " + script.Sd);
                        var report = script.Sd;
                        const string marker = "startup parsed";
                        Check(report.Contains(marker) && report.IndexOf(marker, StringComparison.Ordinal) == report.LastIndexOf(marker, StringComparison.Ordinal) &&
                            report.Length <= 512 && script.Tm == 2000 && script.Cp == 2500 && script.Mm == 128L * 1024 * 1024,
                            "one bounded post-parse marker with unchanged defaults: timing=" + timing + "; " + report);
                        var warm = script.Ex("1;");
                        Thread.Sleep(50);
                        var endpoint = warm.Ok ? typeof(Jp).GetField("worker", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(script) : null;
                        var stderr = endpoint == null ? "" : (string)endpoint.GetType().GetProperty("E").GetValue(endpoint, null);
                        Check(warm.Ok && script.Sd == report && stderr.Contains(marker) &&
                            stderr.IndexOf(marker, StringComparison.Ordinal) == stderr.LastIndexOf(marker, StringComparison.Ordinal),
                            "page evaluation retains the report without another startup marker: timing=" + timing + "; " + warm.S + "; " + warm.M);
                    }
                }
            }
            finally { Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", previous); }
        }

        internal static void Run(string fixture)
        {
            ProfileErrors();
            LaunchFailures(fixture);
            Permissions.Run();
            BlockedIo(fixture);
            ParseMarker(fixture);
            foreach (var port in new[] { 0, 65536 })
                using (var probe = new NetworkProbe(port))
                    Check(probe.Denied() == 0, "invalid probe port fails closed: " + port);
            foreach (var delay in new[] { 0, 550 })
            {
                var listener = new TcpListener(IPAddress.Loopback, 0);
                try
                {
                    listener.Start(1);
                    using (var probe = new NetworkProbe(((IPEndPoint)listener.LocalEndpoint).Port))
                    {
                        Thread.Sleep(delay);
                        Check(probe.Denied() == 0 && listener.Pending(), "successful probe is not denial after overlap: " + delay + " ms");
                        using (listener.AcceptTcpClient()) { }
                    }
                }
                finally { listener.Stop(); }
            }

            var source = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker");
            var trace = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE");
            try
            {
                for (var index = 0; index < 3; index++)
                {
                    var directory = Path.Combine(Path.GetTempPath(), "Lea-script-startup-" + Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(directory);
                    var files = Directory.GetFiles(source).Select(file => Path.Combine(directory, Path.GetFileName(file))).ToArray();
                    foreach (var file in files) File.Copy(Path.Combine(source, Path.GetFileName(file)), file);
                    var engine = new E();
                    var page = engine.Go(fixture);
                    string sid;
                    int pid;
                    Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", trace == "1" || index == 1 ? "1" : null);
                    using (var script = new Jp(engine, page, directory))
                    {
                        Check(script.Tm == 2000 && script.Cp == 2500 && script.Mm == 128L * 1024 * 1024,
                            "fresh-path defaults remain bounded: " + index);
                        var result = script.Ex("document.title='fresh startup';");
                        Console.WriteLine("Fresh worker " + index + ": " + result.S + "; " + result.M + "; " + script.Sd);
                        Check(result.Ok && page.T == "fresh startup" && result.N == 1, "fresh-path worker first execution: " + index);
                        Check(script.Ac && script.Ne && script.Nd && script.Fd && script.Mg == 15,
                            "fresh-path isolation checks precede page code: " + index);
                        Check(script.Sd.StartsWith("phase=ready;", StringComparison.Ordinal) && script.Sd.Length <= 512,
                            "bounded startup diagnostics retained: " + index);
                        Check(script.Sd.Contains("startup evaluate: wall=") == (trace == "1" || index == 1) &&
                            !script.Sd.Contains("startup parse:"), "tracing observes the production bootstrap path: " + index);
                        Check(script.Sd.Contains("startup entry"), "compact startup phase retained without requiring timing trace: " + index);
                        var diagnostics = script.Sd;
                        Check(script.Ex("1;").Ok && script.Sd == diagnostics, "warm execution does not replace startup report: " + index);
                        sid = script.Sid;
                        pid = script.Pid;
                    }
                    Check(Exited(pid) && files.All(file => !HasSid(file, sid)), "fresh-path worker and exact ACLs retired: " + index);
                    // Delete only this run's known files; preserve a failed runtime for diagnosis.
                    foreach (var file in files) File.Delete(file);
                    Directory.Delete(directory, false);
                }
                Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", trace == "1" ? "1" : null);
                Ui(fixture, "run");
                Ui(fixture, "off");
                Ui(fixture, "stale");
                Ui(fixture, "cancel");
                Ui(fixture, "off-launch");
                Ui(fixture, "cancel-launch");
                StopWithoutPump(fixture);
            }
            finally { Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", trace); }
        }

        internal static void BlockedIo(string fixture)
        {
            foreach (var mode in new[] { "off", "stale", "deadline", "late" })
                View.Pump(() => BlockedIoCore(fixture, mode));
        }

        private static async Task BlockedIoCore(string fixture, string mode)
        {
            // A producer barrier tests the parent wait independently of native startup timing.
            var engine = new E();
            var page = engine.Go(fixture);
            using (var entered = new ManualResetEvent(false))
            using (var release = new ManualResetEvent(false))
            using (var exited = new ManualResetEvent(false))
            using (var script = new Jp(engine, page))
            {
                Func<bool> action = () =>
                {
                    try { entered.Set(); release.WaitOne(); return true; }
                    finally { exited.Set(); }
                };
                var method = typeof(Jp).GetMethod("Io", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .MakeGenericMethod(typeof(bool));
                var clock = Stopwatch.StartNew();
                var pending = (Task<bool>)method.Invoke(script, new object[] { action, clock, mode == "off" || mode == "stale" ? 5000 : 100, true });
                var retired = false;
                Task late = null;
                try
                {
                    while (!entered.WaitOne(0) && clock.ElapsedMilliseconds < 1000) await Task.Delay(5);
                    Check(entered.WaitOne(0), "controlled I/O action is blocked: " + mode);
                    var observation = Stopwatch.StartNew();
                    if (mode == "off") engine.Je = false;
                    else if (mode == "stale") engine.Go(fixture);
                    else if (mode == "late")
                    {
                        late = Task.Factory.StartNew(() => { Thread.Sleep(150); release.Set(); },
                            CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                        // The deadline must keep winning after late I/O finishes during an owner pause.
                        Thread.Sleep(250);
                        if (!exited.WaitOne(1500)) throw new InvalidOperationException("Late I/O producer did not finish during the owner pause.");
                    }
                    Exception failure = null;
                    try { await pending; }
                    catch (Exception error) { failure = error; }
                    var state = failure == null ? null : failure.GetType().GetField("Status", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    Check(mode == "off" || mode == "stale" ? state != null && (string)state.GetValue(failure) == mode : failure is TimeoutException,
                        "blocked I/O preserves state/deadline result: " + mode + "; " + (failure == null ? "success" : failure.GetType().Name));
                    Check(observation.ElapsedMilliseconds < (mode == "late" ? 3000 : 1000),
                        "blocked I/O observation is bounded: " + mode + "; " + observation.ElapsedMilliseconds + " ms");
                }
                finally
                {
                    release.Set();
                    retired = exited.WaitOne(2000);
                    if (late != null) await late;
                }
                Check(retired && script.Pid == 0, "controlled I/O action retires without a worker leak: " + mode);
            }
        }

        internal static void Ui(string fixture, string mode)
        {
            var engine = new E();
            var page = engine.Go(fixture);
            var title = page.T;
            var previous = SynchronizationContext.Current;
            Exception failure = null;
            using (var context = new ApplicationContext())
            using (var timer = new System.Windows.Forms.Timer { Interval = 5 })
            using (var script = new Jp(engine, page))
            {
                var started = false;
                var changed = false;
                var ticks = 0;
                var launchTicks = 0;
                var watch = Stopwatch.StartNew();
                long last = 0, gap = 0, changedAt = 0;
                timer.Tick += async (sender, args) =>
                {
                    ticks++;
                    gap = Math.Max(gap, watch.ElapsedMilliseconds - last);
                    last = watch.ElapsedMilliseconds;
                    if (started)
                    {
                        if (script.Pid == 0) launchTicks++;
                        if (!changed && mode != "run" && (script.Pid != 0 || mode.EndsWith("-launch", StringComparison.Ordinal)))
                        {
                            changed = true;
                            changedAt = watch.ElapsedMilliseconds;
                            if (mode == "stale") engine.Go(fixture);
                            else if (mode.StartsWith("off", StringComparison.Ordinal)) engine.Je = false;
                            else script.St();
                        }
                        return;
                    }
                    started = true;
                    try
                    {
                        var result = await script.ExA("document.title='startup executed';");
                        Console.WriteLine("UI startup " + mode + ": " + result.S + "; launch ticks=" + launchTicks + "; max tick gap=" + gap + " ms; " + script.Sd);
                        Check(ticks > 1, "first asynchronous startup yields to UI: " + mode);
                        var expected = mode.StartsWith("off", StringComparison.Ordinal) ? "off" : mode == "stale" ? "stale" : "cancel";
                        Check(mode == "run" ? result.Ok && page.T == "startup executed" :
                            changed && result.S == expected && result.N == 0 && page.T == title && script.Pid == 0,
                            "startup permission/cancellation gate: " + mode + "; " + result.S);
                        Check(script.Tm == 2000 && script.Sd.Length <= 512, "startup keeps defaults and report bound: " + mode);
                        if (mode == "off" || mode == "stale") Check(watch.ElapsedMilliseconds - changedAt < 1000,
                            "published worker invalidation completes before handshake deadline: mode=" + mode + "; " +
                            (watch.ElapsedMilliseconds - changedAt) + " ms");
                    }
                    catch (Exception error) { failure = error; }
                    finally { timer.Stop(); context.ExitThread(); }
                };
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
                timer.Start();
                try { Application.Run(context); }
                finally { SynchronizationContext.SetSynchronizationContext(previous); }
            }
            if (failure != null) throw failure;
        }

        private static bool HasSid(string path, string sid)
        {
            return File.GetAccessControl(path, AccessControlSections.Access)
                .GetAccessRules(true, true, typeof(SecurityIdentifier)).Cast<FileSystemAccessRule>()
                .Any(rule => rule.IdentityReference.Value == sid);
        }

        internal static void StopWithoutPump(string fixture)
        {
            var engine = new E();
            var page = engine.Go(fixture);
            var title = page.T;
            var previous = SynchronizationContext.Current;
            Exception failure = null;
            using (var context = new ApplicationContext())
            using (var timer = new System.Windows.Forms.Timer { Interval = 5 })
            using (var script = new Jp(engine, page))
            {
                timer.Tick += async (sender, args) =>
                {
                    timer.Stop();
                    try
                    {
                        var execution = script.ExA("document.title='must not run';");
                        script.St();
                        // Intentionally do not pump owner continuations during native creation.
                        var clock = Stopwatch.StartNew();
                        while (script.Pid == 0 && clock.ElapsedMilliseconds < 5000) Thread.Sleep(5);
                        var pid = script.Pid;
                        var killed = pid != 0 && Exited(pid);
                        var result = await execution;
                        Check(killed, "Stop during launch kills child without an owner continuation: pid=" + pid);
                        Check(result.S == "cancel" && result.N == 0 && page.T == title && script.Pid == 0,
                            "no-pump startup cancellation completes without writes: " + result.S);
                        var failedMessage = result.M;
                        Check(failedMessage.Contains("phase=launch;") && failedMessage.Length <= 512,
                            "startup failure snapshots bounded phase diagnostics: " + failedMessage);
                        Check(script.Ex("1;").Ok, "recovery after unpublished-launch cancellation");
                        Check(result.M == failedMessage && script.Sd.StartsWith("phase=ready;", StringComparison.Ordinal),
                            "startup failure result retains its report after a new successful start");
                    }
                    catch (Exception error) { failure = error; }
                    finally { context.ExitThread(); }
                };
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
                timer.Start();
                try { Application.Run(context); }
                finally { SynchronizationContext.SetSynchronizationContext(previous); }
            }
            if (failure != null) throw failure;
        }

        private static bool Exited(int pid)
        {
            try { using (var process = Process.GetProcessById(pid)) return process.WaitForExit(1000); }
            catch (ArgumentException) { return true; }
        }

        private static void Check(bool value, string name) { Program.Check(value, name); }
    }
}
