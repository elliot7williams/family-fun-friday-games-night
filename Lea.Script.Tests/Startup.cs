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
using System.Web.Script.Serialization;
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
            OversizedFailure(fixture, false).GetAwaiter().GetResult();
            View.Pump(() => OversizedFailure(fixture, true));
            UnavailableCore(fixture, false, true).GetAwaiter().GetResult();
            View.Pump(() => UnavailableCore(fixture, true, true));
            View.Pump(() => UnavailableView(fixture, true));
        }

        private static async Task OversizedFailure(string fixture, bool asynchronous)
        {
            var engine = new E();
            var page = engine.Go(fixture);
            var title = page.T;
            var runtime = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker");
            var files = Directory.GetFiles(runtime).OrderBy(file => file).ToArray();
            var rules = files.Select(ProfileRules).ToArray();
            var launches = 0;
            using (var script = new Jp(engine, page, runtime, supplied =>
            {
                launches++;
                throw new IOException(new string('e', 700));
            }))
            {
                var result = asynchronous ? await script.ExA("document.title='must not run';") : script.Ex("document.title='must not run';");
                Check(result.S == "setup" && result.M.Length <= 512 &&
                    result.M.StartsWith(new string('e', 125) + "... phase=launch;", StringComparison.Ordinal),
                    "oversized startup failure retains launch summary: async=" + asynchronous + "; " + result.M);
                Check(result.N == 0 && page.T == title && script.Pid == 0 && !script.Ac && !script.Ne && !script.Nd && !script.Fd &&
                    script.Tm == 2000 && script.Cp == 2500 && script.Mm == 128L * 1024 * 1024,
                    "oversized startup failure cannot execute page code or alter isolation defaults: async=" + asynchronous);
                var leases = (System.Collections.ICollection)typeof(Jp).GetField("leases", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(script);
                Check(launches == 1 && leases.Count == 0 && files.Select(ProfileRules).SequenceEqual(rules),
                    "oversized startup failure releases leases and leaves runtime ACLs unchanged after one launch: async=" + asynchronous);
            }
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

        internal static void Volumes(string fixture, string alternateRoot, string reportPath)
        {
            var roots = new[] { Path.GetFullPath(Path.GetTempPath()), Path.GetFullPath(alternateRoot) };
            if (String.Equals(Path.GetPathRoot(roots[0]), Path.GetPathRoot(roots[1]), StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("The staging roots must be on different volumes.", "alternateRoot");
            foreach (var root in roots)
            {
                if (!Directory.Exists(root)) throw new ArgumentException("Staging roots must already exist.");
                var drive = new DriveInfo(Path.GetPathRoot(root));
                if (!drive.IsReady || drive.DriveType != DriveType.Fixed && drive.DriveType != DriveType.Removable)
                    throw new ArgumentException("Staging roots must be on ready local volumes.");
                for (var folder = new DirectoryInfo(root); folder != null; folder = folder.Parent)
                    if ((folder.Attributes & FileAttributes.ReparsePoint) != 0)
                        throw new ArgumentException("Staging roots must not use reparse points.");
            }
            reportPath = Path.GetFullPath(reportPath);
            if (File.Exists(reportPath)) throw new ArgumentException("The diagnostic report must not already exist.", "reportPath");
            var source = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker");
            var names = Directory.GetFiles(source).Select(Path.GetFileName).OrderBy(name => name).ToArray();
            if (names.Length != 8) throw new InvalidDataException("Expected exactly eight worker runtime files.");
            var hashes = names.ToDictionary(name => name, name => Digest(Path.Combine(source, name)));
            var bytes = names.Sum(name => new FileInfo(Path.Combine(source, name)).Length);
            var records = new System.Collections.Generic.List<object>();
            var trace = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE");
            var passed = true;
            Console.WriteLine("Alternating three volume pairs; identical workers; original limits; no retries; fresh copied paths are not cold OS/CLR caches.");
            try
            {
                Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", "1");
                for (var pair = 0; pair < 3; pair++)
                    foreach (var root in pair % 2 == 0 ? roots : roots.Reverse())
                    {
                        var drive = new DriveInfo(Path.GetPathRoot(root));
                        var free = drive.AvailableFreeSpace;
                        var reserve = String.Equals(drive.Name.TrimEnd('\\'), Environment.GetEnvironmentVariable("SystemDrive"), StringComparison.OrdinalIgnoreCase)
                            ? 64L * 1024 * 1024 : 32L * 1024 * 1024;
                        if (free < reserve + bytes) throw new IOException("Insufficient free space for bounded volume staging: " + drive.Name);
                        var directory = Path.Combine(root, "Lea-startup-volume-" + Guid.NewGuid().ToString("N"));
                        Directory.CreateDirectory(directory);
                        var files = names.Select(name => Path.Combine(directory, name)).ToArray();
                        var copy = Stopwatch.StartNew();
                        foreach (var file in files) File.Copy(Path.Combine(source, Path.GetFileName(file)), file);
                        var copyMs = copy.ElapsedMilliseconds;
                        Check(files.All(file => Digest(file) == hashes[Path.GetFileName(file)]), "volume runtime bytes match: pair=" + pair + "; " + drive.Name);
                        var normalized = files.Count(NormalizeVolumeAcl);
                        var rules = files.Select(ProfileRules).ToArray();
                        var engine = new E { Jb = false };
                        var page = engine.Go(fixture);
                        var title = page.T;
                        var script = new Jp(engine, page, directory);
                        Jr result;
                        var clock = Stopwatch.StartNew();
                        var pid = 0;
                        bool isolation;
                        string diagnostics;
                        try
                        {
                            Check(script.Tm == 2000 && script.Cp == 2500 && script.Mm == 128L * 1024 * 1024,
                                "volume diagnostic preserves limits: pair=" + pair + "; " + drive.Name);
                            result = script.Ex("document.title='volume diagnostic';");
                            diagnostics = script.Sd;
                            pid = script.Pid;
                            isolation = script.Ac && script.Ne && script.Nd && script.Fd && script.Mg == 15;
                        }
                        finally { script.Dispose(); }
                        var wall = clock.ElapsedMilliseconds;
                        var restored = files.Select(ProfileRules).SequenceEqual(rules);
                        records.Add(new { pair, volume = drive.Name, type = drive.DriveType.ToString(), fileSystem = drive.DriveFormat,
                            freeBefore = free, runtimeBytes = bytes, directory, copyMs, aclNormalizationCount = normalized, callAndDisposeMs = wall,
                            status = result.S, message = result.M, changes = result.N, startup = diagnostics,
                            startupReady = diagnostics.StartsWith("phase=ready;", StringComparison.Ordinal),
                            isolationVerified = isolation, exactAclsRestored = restored,
                            successfulChildExitVerified = pid != 0 && Exited(pid), peakBytes = script.Mp });
                        Console.WriteLine("Volume pair=" + pair + "; " + drive.Name + "; " + drive.DriveType + "; copy=" + copyMs +
                            " ms; status=" + result.S + "; " + result.M + "; " + diagnostics);
                        Check(restored && script.Pid == 0, "volume diagnostic releases exact ACLs and session ownership: pair=" + pair + "; " + drive.Name);
                        if (!result.Ok)
                        {
                            passed = false;
                            if (!diagnostics.StartsWith("phase=ready;", StringComparison.Ordinal))
                                Check(result.N == 0 && page.T == title, "failed volume startup cannot execute supplied page code: pair=" + pair + "; " + drive.Name);
                            Console.WriteLine("Retaining failed volume runtime: " + directory);
                            continue;
                        }
                        Check(isolation && result.N == 1 && page.T == "volume diagnostic" && Exited(pid),
                            "volume launch verifies isolation, first mutation and child retirement: pair=" + pair + "; " + drive.Name);
                        // Delete only this diagnostic's known files; failed runtimes remain available.
                        foreach (var file in files) File.Delete(file);
                        Directory.Delete(directory, false);
                    }
            }
            finally
            {
                Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", trace);
                var audit = new { kind = "startup-volume-diagnostic", roots, source, hashes, records,
                    plannedSamples = 6, allLaunchesPassed = passed && records.Count == 6, startupBudgetMs = 5000,
                    executionDefaultMs = 2000, memoryBytes = 128L * 1024 * 1024, cpuRate = 2500,
                    retries = 0, timingTrace = true, coldCacheControlled = false, productionRuntimeModified = false };
                File.WriteAllText(reportPath, new JavaScriptSerializer { MaxJsonLength = 128 * 1024, RecursionLimit = 16 }.Serialize(audit));
            }
            Check(passed && records.Count == 6, "volume diagnostic all six launches pass without retries");
        }

        private static bool NormalizeVolumeAcl(string path)
        {
            var security = File.GetAccessControl(path, AccessControlSections.Access);
            var original = security.GetSecurityDescriptorSddlForm(AccessControlSections.Access);
            security.SetSecurityDescriptorSddlForm(original, AccessControlSections.Access);
            File.SetAccessControl(path, security);
            var normalized = ProfileRules(path);
            var before = new RawSecurityDescriptor(original);
            var after = new RawSecurityDescriptor(normalized);
            if (before.DiscretionaryAcl == null || after.DiscretionaryAcl == null)
                throw new InvalidDataException("Volume diagnostic requires a non-null file DACL.");
            var oldAcl = new byte[before.DiscretionaryAcl.BinaryLength];
            var newAcl = new byte[after.DiscretionaryAcl.BinaryLength];
            before.DiscretionaryAcl.GetBinaryForm(oldAcl, 0);
            after.DiscretionaryAcl.GetBinaryForm(newAcl, 0);
            if (!oldAcl.SequenceEqual(newAcl) ||
                ((before.ControlFlags ^ after.ControlFlags) & ~ControlFlags.DiscretionaryAclAutoInherited) != 0)
                throw new InvalidDataException("Volume ACL normalization changed permissions or other control flags.");
            return original != normalized;
        }

        internal static void Compare(string fixture, string baseline)
        {
            baseline = Path.GetFullPath(baseline);
            var current = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker");
            // Symbols are not among Jp's leased runtime inputs and differ after recompilation.
            Func<string, bool> runtimeFile = name => !name.Equals("Lea.Script.Worker.pdb", StringComparison.OrdinalIgnoreCase);
            var names = Directory.GetFiles(current).Select(Path.GetFileName).Where(runtimeFile).OrderBy(name => name).ToArray();
            Check(names.SequenceEqual(Directory.GetFiles(baseline).Select(Path.GetFileName).Where(runtimeFile).OrderBy(name => name)),
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

        internal static void Report()
        {
            var method = typeof(Jp).GetMethod("StartupReport", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var format = (Func<string, string, string>)Delegate.CreateDelegate(typeof(Func<string, string, string>), method);
            const string header = "phase=hello; launch=191; probe=75; wall=5005; cpu=765 ms";
            Check(format(header, "") == header, "startup report preserves a header without worker diagnostics");
            Check(format(header, "startup parsed") == header + "; startup parsed", "startup report preserves a short complete trace");
            var exact = new string('x', 512 - header.Length - 2);
            Check(format(header, exact) == header + "; " + exact, "startup report retains an exactly fitting trace without a truncation marker");
            const string latest = "startup probes: wall=4815; cpu=750 ms\n";
            var trace = String.Concat(Enumerable.Repeat("startup evaluate: wall=1948; cpu=250 ms\n", 14)) + latest;
            var report = format(header, trace);
            Check(report.Length == 512 && report.StartsWith(header + "; ", StringComparison.Ordinal), "startup report preserves the parent header within the existing 512-character bound");
            Check(report.EndsWith(latest, StringComparison.Ordinal) && report.Contains("..."), "startup report retains the latest captured worker phase and marks omitted earlier text");
            Check(Enumerable.Range(507, 6).All(length =>
            {
                var prefix = new string('h', length);
                var bounded = format(prefix, "worker trace ending");
                return bounded.Length <= 512 && bounded.StartsWith(prefix, StringComparison.Ordinal) &&
                    (length >= 510 ? bounded == prefix : bounded.EndsWith("g", StringComparison.Ordinal));
            }), "startup report handles tiny diagnostic space without an incomplete separator or overflow");
            Check(format(new string('h', 600), latest) == new string('h', 512), "startup report bounds an oversized parent header defensively");
            var failureMethod = typeof(Jp).GetMethod("StartupFailure", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            Check(failureMethod != null, "startup failure formatter is present");
            var failure = (Func<string, string, string, string>)Delegate.CreateDelegate(typeof(Func<string, string, string, string>), failureMethod);
            const string message = "Worker startup deadline exceeded.";
            Check(failure(message, header, "startup ready") == message + " " + header + "; startup ready",
                "startup failure keeps its error, parent summary and short worker trace together");
            Check(failure(message, header, "") == message + " " + header,
                "startup failure without worker diagnostics retains its error and parent summary");
            var boundedFailure = failure(message, header, trace);
            Check(boundedFailure.Length == 512 && boundedFailure.StartsWith(message + " " + header + "; ", StringComparison.Ordinal),
                "startup failure reserves space for the error and parent summary within the final result bound");
            Check(boundedFailure.EndsWith(latest, StringComparison.Ordinal) && boundedFailure.Contains("..."),
                "startup failure retains the newest trace instead of truncating it a second time");
            var oversized = failure(new string('e', 700), header, trace);
            Check(oversized.Length == 512 && oversized.StartsWith(new string('e', 125) + "... " + header + "; ", StringComparison.Ordinal) &&
                oversized.EndsWith(latest, StringComparison.Ordinal), "oversized startup errors cannot crowd out the parent summary or newest worker trace");
            Check(failure(new string('e', 128), header, "") == new string('e', 128) + " " + header,
                "startup error text exactly at its reserved bound is not marked as truncated");
            Check(failure(new string('e', 129), header, "") == new string('e', 125) + "... " + header,
                "startup error text over its reserved bound marks the omitted suffix");
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
                        var drain = Stopwatch.StartNew();
                        while (endpoint != null && !stderr.Contains("startup ready") && drain.ElapsedMilliseconds < 1000)
                        {
                            Thread.Sleep(5);
                            stderr = (string)endpoint.GetType().GetProperty("E").GetValue(endpoint, null);
                        }
                        var probes = stderr.IndexOf("startup probes", StringComparison.Ordinal);
                        var hello = stderr.IndexOf("startup hello", StringComparison.Ordinal);
                        var ready = stderr.IndexOf("startup ready", StringComparison.Ordinal);
                        Check(probes >= 0 && hello > probes && ready > hello &&
                            hello == stderr.LastIndexOf("startup hello", StringComparison.Ordinal) &&
                            ready == stderr.LastIndexOf("startup ready", StringComparison.Ordinal),
                            "worker records one hello-write entry and completion after the denial probes: timing=" + timing + "; " + stderr);
                        Check(report.StartsWith("phase=ready;", StringComparison.Ordinal) && warm.Ok &&
                            script.Ac && script.Ne && script.Nd && script.Fd && script.Mg == 15,
                            "parent ready phase follows the verified handshake before page execution: timing=" + timing);
                    }
                }
            }
            finally { Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", previous); }
        }

        internal static void Run(string fixture)
        {
            Report();
            ProfileErrors();
            LaunchFailures(fixture);
            Permissions.Run();
            ParentIo.Run(fixture);
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
                        Check(script.Sd.Contains("startup entry") || script.Sd.Contains("startup parsed"),
                            "bounded startup trace retains an entry or post-parse checkpoint without requiring timing: " + index);
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
                        var recovery = script.Ex("1;");
                        Console.WriteLine("Unpublished-launch recovery: " + recovery.S + "; changes=" + recovery.N +
                            "; " + recovery.M + "; " + script.Sd);
                        Check(recovery.Ok, "recovery after unpublished-launch cancellation: " + recovery.S + "; " + recovery.M);
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
