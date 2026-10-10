using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using Lea.Engine;
using Lea.Script;
using Lea.Win;
using Microsoft.Win32.SafeHandles;

namespace Lea.Script.Tests
{
    internal static class NativeCpu
    {
        internal static void Run(string fixture)
        {
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker");
            var helper = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lea.NativeCpu.Worker.exe");
            var files = new[] { helper };
            var rules = files.Select(Rules).ToArray();
            var engine = new E();
            var page = engine.Go(fixture);
            var title = page.T;
            long expected = -1;
            using (var script = new Jp(engine, page, directory, supplied =>
            {
                var worker = Rp.Open(helper, "", AppDomain.CurrentDomain.BaseDirectory, 128L * 1024 * 1024,
                    2500, "Lea.Script.Cpu." + Guid.NewGuid().ToString("N"), files, true);
                try { expected = Retire(worker, "startup"); return worker; }
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
            Program.Check(files.Select(Rules).SequenceEqual(rules), "exited startup worker retires its exact file grants");

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
                    "", AppDomain.CurrentDomain.BaseDirectory, 134217728L, 2500, "Lea.Script.Cpu." + Guid.NewGuid().ToString("N"), files, true });
                try
                {
                    var cpu = Retire(worker, assembly.GetName().Name);
                    var property = type.GetProperty("Ct", BindingFlags.Instance | BindingFlags.NonPublic);
                    Program.Check(property != null && (long?)property.GetValue(worker, null) == cpu,
                        assembly.GetName().Name + ": retained handle CPU matches an independent native reading after exit");
                    worker.Dispose();
                    worker.Dispose();
                    Program.Check(property.GetValue(worker, null) == null,
                        assembly.GetName().Name + ": disposed CPU handle is unavailable and repeated cleanup is harmless");
                }
                finally { worker.Dispose(); }
                Program.Check(files.Select(Rules).SequenceEqual(rules), assembly.GetName().Name + ": native CPU worker file grants retire");
            }
        }

        // A separate query/synchronize handle verifies the exact child without anchoring Process.Handle.
        private static long Retire(object worker, string owner)
        {
            var type = worker.GetType();
            var process = (Process)type.GetProperty("P").GetValue(worker, null);
            using (var watch = OpenProcess(0x00101000, false, process.Id))
            {
                if (watch.IsInvalid) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                Program.Check(new[] { "R", "L", "S", "A", "M", "C", "N", "F", "B" }
                    .All(name => (bool)type.GetProperty(name).GetValue(worker, null)), owner + ": native CPU worker retains restricted launch protections");
                ((Stream)type.GetProperty("I").GetValue(worker, null)).Dispose();
                if (WaitForSingleObject(watch, 10000) != 0) throw new TimeoutException("Controlled EOF worker did not exit.");
                long created, exited, kernel, user;
                if (!GetProcessTimes(watch, out created, out exited, out kernel, out user))
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                var cpu = (kernel + user) / TimeSpan.TicksPerMillisecond;
                uint exitCode;
                var exitRead = GetExitCodeProcess(watch, out exitCode);
                Console.WriteLine("CPU fixture " + owner + ": exitRead=" + exitRead + "; exit=0x" + exitCode.ToString("X8") +
                    "; created=" + created + "; exited=" + exited + "; kernel=" + kernel + "; user=" + user +
                    "; cpu=" + cpu + " ms; captured stderr=" + type.GetProperty("E").GetValue(worker, null));
                Program.Check(exitRead && exitCode == 0 && exited > created && cpu >= 100,
                    owner + ": independent native counter confirms guarded bounded CPU work and exact process exit");
                process.Dispose();
                return cpu;
            }
        }

        private static string Rules(string path)
        { return File.GetAccessControl(path, AccessControlSections.Access).GetSecurityDescriptorSddlForm(AccessControlSections.Access); }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern SafeProcessHandle OpenProcess(uint access, [MarshalAs(UnmanagedType.Bool)] bool inherit, int pid);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(SafeProcessHandle process, uint milliseconds);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetExitCodeProcess(SafeProcessHandle process, out uint exitCode);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetProcessTimes(SafeProcessHandle process, out long created, out long exited, out long kernel, out long user);
    }
}
