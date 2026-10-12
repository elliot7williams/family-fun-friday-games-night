using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using Lea.Engine;
using Lea.Script;
using Lea.Win;
using Microsoft.Win32.SafeHandles;

namespace Lea.Script.Tests
{
    internal static class NativeCpu
    {
        internal static void Run(string fixture, bool trace = false)
        {
            Diagnostics();
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker");
            var helper = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lea.NativeCpu.Worker.exe");
            var files = new[] { helper };
            // Establish Windows inheritance metadata before capturing the exact cleanup baseline.
            Console.WriteLine("CPU ACL baseline: initialized=" + AclBaseline.Initialize(helper));
            var rules = files.Select(Rules).ToArray();
            var engine = new E();
            var page = engine.Go(fixture);
            var title = page.T;
            long expected = -1;
            using (var script = new Jp(engine, page, directory, supplied =>
            {
                var worker = Rp.Open(helper, trace ? "--trace" : "", AppDomain.CurrentDomain.BaseDirectory, 128L * 1024 * 1024,
                    2500, "Lea.Script.Cpu." + Guid.NewGuid().ToString("N"), files, true);
                try { expected = Retire(worker, "startup", trace); return worker; }
                catch { worker.Dispose(); throw; }
            }))
            {
                var result = script.Ex("document.title='must not run';");
                Console.WriteLine("Exited worker CPU expected=" + expected + " ms; " + script.Sd);
                Program.Check(script.Sd.Contains("; cpu=" + expected.ToString(CultureInfo.InvariantCulture) + " ms"),
                    "startup CPU survives managed Process disposal and exact worker exit");
                Program.Check(result.S == "setup" && result.N == 0 && page.T == title && script.Pid == 0 &&
                    script.Sd.Length <= 512 && script.Tm == 2000 && script.Cp == 2500 && script.Mm == 134217728,
                    "controlled exited worker cannot execute page code or change limits");
            }
            CheckRules(files, rules, "exited startup worker retires its exact file grants");

            using (var script = new Jp(engine, page, directory, supplied => { throw new IOException("controlled missing worker"); }))
            {
                var result = script.Ex("document.title='must not run';");
                Program.Check(script.Sd.Contains("; cpu=unavailable") && !script.Sd.Contains("; cpu=0 ms"),
                    "missing worker CPU is unavailable instead of fabricated zero");
                Program.Check(result.S == "setup" && result.N == 0 && page.T == title && script.Pid == 0 && script.Sd.Length <= 512,
                    "unavailable CPU preserves bounded setup failure without page execution");
            }

            foreach (var assembly in new[] { typeof(Jp).Assembly, typeof(V).Assembly })
            {
                var type = assembly.GetType("Lea.Win.Rp", true);
                var open = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                    .Single(method => method.Name == "Open" && method.GetParameters().Length == 8);
                var worker = (IDisposable)open.Invoke(null, new object[] { helper,
                    trace ? "--trace" : "", AppDomain.CurrentDomain.BaseDirectory, 134217728L, 2500, "Lea.Script.Cpu." + Guid.NewGuid().ToString("N"), files, true });
                try
                {
                    var cpu = Retire(worker, assembly.GetName().Name, trace);
                    var property = type.GetProperty("Ct", BindingFlags.Instance | BindingFlags.NonPublic);
                    Program.Check(property != null && (long?)property.GetValue(worker, null) == cpu,
                        assembly.GetName().Name + ": retained handle CPU matches an independent native reading after exit");
                    worker.Dispose();
                    worker.Dispose();
                    Program.Check(property.GetValue(worker, null) == null,
                        assembly.GetName().Name + ": disposed CPU handle is unavailable and repeated cleanup is harmless");
                }
                finally { worker.Dispose(); }
                CheckRules(files, rules, assembly.GetName().Name + ": native CPU worker file grants retire");
            }
        }

        private static void Diagnostics()
        {
            var complete = CpuDelta.Report("complete", 400000, 1500000, 210, 8);
            Program.Check(complete == "CPU work: result=complete; startTicks=400000; finishTicks=1500000; deltaTicks=1100000; wallMs=210; spins=8; targetTicks=1000000; wallLimitMs=3000",
                "CPU delta: completed report separates the loop from startup CPU");
            var timeout = CpuDelta.Report("timeout", 500000, 1400000, 3000, 17);
            Program.Check(timeout.Contains("result=timeout") && timeout.Contains("deltaTicks=900000") && timeout.Contains("wallMs=3000") && timeout.Contains("spins=17"),
                "CPU delta: synthetic timeout retains a below-target loop despite positive total CPU");
            Program.Check(CpuDelta.Report("timeout", 0, 0, 3000, 0).Contains("deltaTicks=0;"),
                "CPU delta: a real zero reading remains zero");
            Program.Check(CpuDelta.Report("timeout", -1, 4, 3000, 0).Contains("deltaTicks=unavailable;"),
                "CPU delta: invalid initial counter is not fabricated zero");
            Program.Check(CpuDelta.Report("timeout", 4, 3, 3000, 0).Contains("deltaTicks=unavailable;"),
                "CPU delta: decreasing counter is not unsigned or negative CPU work");
            Program.Check(CpuDelta.Report("complete", 0, Int64.MaxValue, Int64.MaxValue, Int64.MaxValue).Length < 512 &&
                CpuDelta.Report("timeout", Int64.MinValue, Int64.MaxValue, 3000, 0).Length < 512,
                "CPU delta: extreme numeric reports remain bounded without subtraction overflow");
            Program.Check(CpuDelta.Report("complete", 400000, 1400000, 3000, 8).Contains("deltaTicks=1000000;"),
                "CPU delta: exact 100 ms threshold is retained");
            var previous = System.Threading.Thread.CurrentThread.CurrentCulture;
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
                Program.Check(CpuDelta.Report("complete", 400000, 1500000, 210, 8) == complete,
                    "CPU delta: report numbers are culture independent");
            }
            finally { System.Threading.Thread.CurrentThread.CurrentCulture = previous; }
        }

        internal static void Trace(string fixture)
        {
            using (var receiver = new System.IO.Pipes.AnonymousPipeServerStream(System.IO.Pipes.PipeDirection.In, HandleInheritability.None))
            using (var sender = new System.IO.Pipes.AnonymousPipeClientStream(System.IO.Pipes.PipeDirection.Out, receiver.ClientSafePipeHandle))
            using (var handle = new SafeFileHandle(receiver.SafePipeHandle.DangerousGetHandle(), false))
            {
                var bytes = Encoding.ASCII.GetBytes("imecx");
                sender.Write(bytes, 0, bytes.Length); sender.Flush();
                var buffer = new byte[5];
                uint read, available;
                Program.Check(PeekNamedPipe(handle, buffer, 5, out read, out available, IntPtr.Zero) && read == 5 && available == 5 &&
                    Encoding.ASCII.GetString(buffer) == "imecx", "EOF pipe: bounded snapshot uses byte counts, not message remainder");
                Program.Check(PeekNamedPipe(handle, buffer, 5, out read, out available, IntPtr.Zero) && read == 5 && available == 5,
                    "EOF pipe: repeated snapshot does not consume markers");
            }
            using (var invalid = new SafeProcessHandle(IntPtr.Zero, false))
            {
                var wait = WaitForSingleObject(invalid, 0);
                var error = Marshal.GetLastWin32Error();
                var failure = WaitError(wait, error, "controlled invalid handle");
                Program.Check(wait == UInt32.MaxValue && failure is System.ComponentModel.Win32Exception &&
                    ((System.ComponentModel.Win32Exception)failure).NativeErrorCode == error && error != 0,
                    "EOF wait: native API failure is not relabeled timeout");
            }
            Program.Check(WaitError(0x102, 0, "controlled timeout") is TimeoutException,
                "EOF wait: only WAIT_TIMEOUT becomes timeout");
            Program.Check(WaitError(0x80, 0, "controlled unexpected result") is InvalidOperationException,
                "EOF wait: unexpected wait result remains distinct");
            Run(fixture, true);

            var helper = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lea.NativeCpu.Worker.exe");
            var files = new[] { helper };
            var rules = files.Select(Rules).ToArray();
            foreach (var assembly in new[] { typeof(Jp).Assembly, typeof(V).Assembly })
            {
                var type = assembly.GetType("Lea.Win.Rp", true);
                var open = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                    .Single(method => method.Name == "Open" && method.GetParameters().Length == 8);
                var owner = assembly.GetName().Name;
                var worker = (IDisposable)open.Invoke(null, new object[] { helper, "--trace --hold",
                    AppDomain.CurrentDomain.BaseDirectory, 134217728L, 2500,
                    "Lea.Script.Cpu." + Guid.NewGuid().ToString("N"), files, true });
                var process = (Process)type.GetProperty("P").GetValue(worker, null);
                using (var watch = OpenProcess(0x00101000, false, process.Id))
                {
                    try
                    {
                        if (watch.IsInvalid) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                        try { Retire(worker, owner, true); throw new Exception("Held worker exited"); }
                        catch (TimeoutException failure)
                        {
                            Program.Check((uint)failure.Data["wait"] == 0x102 && (string)failure.Data["owner"] == owner &&
                                (string)failure.Data["markers"] == "imech" && (long?)failure.Data["cpu"] >= 100 &&
                                (uint?)failure.Data["exit"] == 259 && failure.Message.Length < 512,
                                owner + ": EOF timeout identifies completed CPU work and deliberately held exit");
                        }
                    }
                    finally { worker.Dispose(); }
                    Program.Check(WaitForSingleObject(watch, 1000) == 0,
                        owner + ": held-worker disposal retires the independently watched exact child");
                }
                CheckRules(files, rules, owner + ": held-worker file grants retire");
            }
        }

        // A separate query/synchronize handle verifies the exact child without anchoring Process.Handle.
        private static long Retire(object worker, string owner, bool trace = false)
        {
            var type = worker.GetType();
            var process = (Process)type.GetProperty("P").GetValue(worker, null);
            using (var watch = OpenProcess(0x00101000, false, process.Id))
            {
                if (watch.IsInvalid) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                Program.Check(new[] { "R", "L", "S", "A", "M", "C", "N", "F", "B" }
                    .All(name => (bool)type.GetProperty(name).GetValue(worker, null)), owner + ": native CPU worker retains restricted launch protections");
                ((Stream)type.GetProperty("I").GetValue(worker, null)).Dispose();
                var clock = Stopwatch.StartNew();
                var wait = WaitForSingleObject(watch, 10000);
                var waitError = wait == UInt32.MaxValue ? Marshal.GetLastWin32Error() : 0;
                var markers = Markers(worker, trace);
                if (wait != 0)
                {
                    long createdAt, exitedAt, kernelAt, userAt;
                    var times = GetProcessTimes(watch, out createdAt, out exitedAt, out kernelAt, out userAt);
                    long? cpuAt = times && kernelAt >= 0 && userAt >= 0 && kernelAt <= Int64.MaxValue - userAt ?
                        (long?)((kernelAt + userAt) / TimeSpan.TicksPerMillisecond) : null;
                    uint codeAt;
                    uint? exitAt = GetExitCodeProcess(watch, out codeAt) ? (uint?)codeAt : null;
                    var detail = "owner=" + owner + "; pid=" + process.Id + "; wait=0x" + wait.ToString("X8") +
                        "; elapsed=" + clock.ElapsedMilliseconds + " ms; cpu=" +
                        (cpuAt.HasValue ? cpuAt.Value.ToString(CultureInfo.InvariantCulture) + " ms" : "unavailable") +
                        "; exit=" + (exitAt.HasValue ? "0x" + exitAt.Value.ToString("X8") : "unavailable") +
                        "; markers=" + markers;
                    Console.WriteLine("CPU fixture wait: " + detail);
                    var failure = WaitError(wait, waitError, detail);
                    failure.Data["owner"] = owner; failure.Data["wait"] = wait;
                    failure.Data["cpu"] = cpuAt; failure.Data["exit"] = exitAt; failure.Data["markers"] = markers;
                    throw failure;
                }
                long created, exited, kernel, user;
                if (!GetProcessTimes(watch, out created, out exited, out kernel, out user))
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                var cpu = (kernel + user) / TimeSpan.TicksPerMillisecond;
                uint exitCode;
                var exitRead = GetExitCodeProcess(watch, out exitCode);
                Console.WriteLine("CPU fixture " + owner + ": exitRead=" + exitRead + "; exit=0x" + exitCode.ToString("X8") +
                    "; created=" + created + "; exited=" + exited + "; kernel=" + kernel + "; user=" + user +
                    "; cpu=" + cpu + " ms; markers=" + markers + "; captured stderr=" + type.GetProperty("E").GetValue(worker, null));
                if (trace) Program.Check(markers == "imecx", owner + ": controlled helper enters, mitigates, receives EOF, completes CPU work and reaches exit");
                Program.Check(exitRead && exitCode == 0 && exited > created && cpu >= 100,
                    owner + ": independent native counter confirms guarded bounded CPU work and exact process exit");
                process.Dispose();
                return cpu;
            }
        }

        private static Exception WaitError(uint wait, int error, string detail)
        {
            if (wait == 0x102) return new TimeoutException("Controlled EOF worker did not exit; " + detail);
            if (wait == UInt32.MaxValue) return new System.ComponentModel.Win32Exception(error, "Controlled EOF worker wait failed; " + detail);
            return new InvalidOperationException("Unexpected EOF wait result; " + detail);
        }

        // Only this test fixture writes to stdout; peek a bounded snapshot without a reader or retry.
        private static string Markers(object worker, bool trace)
        {
            if (!trace) return "off";
            var output = (FileStream)worker.GetType().GetProperty("O").GetValue(worker, null);
            var buffer = new byte[5];
            uint read, available;
            if (!PeekNamedPipe(output.SafeFileHandle, buffer, (uint)buffer.Length, out read, out available, IntPtr.Zero))
                return "unavailable:" + Marshal.GetLastWin32Error().ToString(CultureInfo.InvariantCulture);
            if (read > 5 || available > 5) return "invalid";
            var value = Encoding.ASCII.GetString(buffer, 0, (int)read);
            return value.Length == 0 ? "not-entered" : value;
        }

        private static string Rules(string path)
        { return File.GetAccessControl(path, AccessControlSections.Access).GetSecurityDescriptorSddlForm(AccessControlSections.Access); }

        private static void CheckRules(string[] files, string[] before, string name)
        {
            var after = files.Select(Rules).ToArray();
            for (var index = 0; index < files.Length; index++)
                if (before[index] != after[index])
                    Console.WriteLine("CPU ACL cleanup mismatch: file=" + files[index] + "; before=" + before[index] + "; after=" + after[index]);
            Program.Check(after.SequenceEqual(before), name);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern SafeProcessHandle OpenProcess(uint access, [MarshalAs(UnmanagedType.Bool)] bool inherit, int pid);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(SafeProcessHandle process, uint milliseconds);

        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PeekNamedPipe(SafeFileHandle pipe, [Out] byte[] buffer, uint size,
            out uint read, out uint available, IntPtr remaining);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetExitCodeProcess(SafeProcessHandle process, out uint exitCode);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetProcessTimes(SafeProcessHandle process, out long created, out long exited, out long kernel, out long user);
    }
}
