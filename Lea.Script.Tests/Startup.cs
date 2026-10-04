using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Script;
using Lea.Script.Worker;

namespace Lea.Script.Tests
{
    internal static class Startup
    {
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
                            Check(result.Ok && page.T == "bootstrap comparison" && result.N == 1,
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
            }
            finally { Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", trace); }
        }

        private static string Digest(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        internal static void Run(string fixture)
        {
            Permissions.Run();
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
                Ui(fixture, "cancel");
                Ui(fixture, "off-launch");
                Ui(fixture, "cancel-launch");
                StopWithoutPump(fixture);
            }
            finally { Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", trace); }
        }

        private static void Ui(string fixture, string mode)
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
                long last = 0, gap = 0;
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
                            if (mode.StartsWith("off", StringComparison.Ordinal)) engine.Je = false;
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
                        var expected = mode.StartsWith("off", StringComparison.Ordinal) ? "off" : "cancel";
                        Check(mode == "run" ? result.Ok && page.T == "startup executed" :
                            changed && result.S == expected && result.N == 0 && page.T == title && script.Pid == 0,
                            "startup permission/cancellation gate: " + mode + "; " + result.S);
                        Check(script.Tm == 2000 && script.Sd.Length <= 512, "startup keeps defaults and report bound: " + mode);
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

        private static void StopWithoutPump(string fixture)
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
                        Check(script.Ex("1;").Ok, "recovery after unpublished-launch cancellation");
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
