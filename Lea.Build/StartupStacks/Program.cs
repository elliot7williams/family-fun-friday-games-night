using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Win32.SafeHandles;

namespace Lea.Diagnostics
{
    internal static class Program
    {
        private static string Limit(string text) { return text == null || text.Length <= 512 ? text : text.Substring(0, 512); }
        private static string Hash(string path) { using var input = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(input)); }

        private static int Main(string[] args)
        {
            if (args.Length != 7 || (args[0] != "--sample" && args[0] != "--suspend") || !Int32.TryParse(args[1], NumberStyles.None, CultureInfo.InvariantCulture, out var pid) || pid <= 0 ||
                !Int32.TryParse(args[2], NumberStyles.None, CultureInfo.InvariantCulture, out var parent) || parent <= 0 ||
                !Int64.TryParse(args[5], NumberStyles.None, CultureInfo.InvariantCulture, out var birth) || birth <= 0)
            { Console.Error.WriteLine("Supply --sample or --suspend, PID PARENT IMAGE SHA256 CREATED-FILETIME NEW-REPORT."); return 2; }
            var suspend = args[0] == "--suspend";
            FileStream output;
            try { output = new FileStream(Path.GetFullPath(args[6]), FileMode.CreateNew, FileAccess.Write, FileShare.Read); }
            catch (Exception error) { Console.Error.WriteLine(Limit(error.Message)); return 2; }
            using (output)
            {
                var clock = Stopwatch.StartNew();
                var started = DateTime.UtcNow;
                var attempted = false;
                var captured = false;
                var disposed = false;
                var capturedMs = 0L;
                DateTime? requestedUtc = null, capturedUtc = null;
                var totalFrames = 0;
                string failure = null, dacHash = null, runtimeVersion = null;
                var threads = new List<object>();
                try
                {
                    if (!OperatingSystem.IsWindows() || !Environment.Is64BitProcess) throw new NotSupportedException("Windows x64 is required.");
                    var expected = Path.GetFullPath(args[3]);
                    if (Path.GetFileName(expected) != "Lea.Script.Worker.exe" && Path.GetFileName(expected) != "Lea.StackFixture.exe")
                        throw new InvalidOperationException("Only Lea's controlled worker or stack fixture may be inspected.");
                    using var handle = OpenProcess(0x1000, false, pid);
                    if (handle.IsInvalid) throw new Win32Exception(Marshal.GetLastWin32Error());
                    var name = new StringBuilder(4096); uint length = 4096;
                    if (!QueryFullProcessImageName(handle, 0, name, ref length)) throw new Win32Exception(Marshal.GetLastWin32Error());
                    var basic = new BasicInfo();
                    if (NtQueryInformationProcess(handle, 0, ref basic, Marshal.SizeOf<BasicInfo>(), out _) != 0)
                        throw new InvalidOperationException("Process identity query failed.");
                    if (!GetProcessTimes(handle, out var created, out _, out _, out _)) throw new Win32Exception(Marshal.GetLastWin32Error());
                    if (basic.Pid.ToInt64() != pid || basic.ParentPid.ToInt64() != parent || created != birth ||
                        !String.Equals(Path.GetFullPath(name.ToString()), expected, StringComparison.OrdinalIgnoreCase) ||
                        !String.Equals(Hash(expected), args[4], StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("PID, parent, image, creation time or image hash does not match the controlled target.");
                    var dac = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Microsoft.NET", "Framework64", "v4.0.30319", "mscordacwks.dll");
                    dacHash = Hash(dac);
                    var options = new DataTargetOptions
                    {
                        FileLocator = null,
                        Limits = new DataTargetLimits { MaxThreads = 64, MaxStackFrames = 32, MaxModules = 512, MaxAppDomains = 16 }
                    };
                    // Holding the checked native handle prevents PID reuse during snapshot capture.
                    attempted = true;
                    DataTarget snapshot = null;
                    try
                    {
                        requestedUtc = DateTime.UtcNow;
                        snapshot = suspend ? DataTarget.AttachToProcess(pid, suspend: true, options: options) :
                            DataTarget.CreateSnapshotAndAttach(pid, options);
                        capturedUtc = DateTime.UtcNow;
                        capturedMs = clock.ElapsedMilliseconds;
                        captured = true;
                        var info = snapshot.ClrVersions.Single();
                        if (info.Flavor != ClrFlavor.Desktop) throw new NotSupportedException("Only the desktop CLR test target is supported.");
                        runtimeVersion = info.Version.ToString();
                        using var runtime = info.CreateRuntime(dac);
                        foreach (var thread in runtime.Threads.Where(item => item.IsAlive).Take(16))
                        {
                            var frames = thread.EnumerateStackTrace(false, 32).Take(32).Select(frame => new
                            {
                                kind = frame.Kind.ToString(), method = Limit(frame.ToString()),
                                stackPointer = frame.StackPointer.ToString("X16"), instructionPointer = frame.InstructionPointer.ToString("X16")
                            }).ToArray();
                            totalFrames += frames.Length;
                            threads.Add(new { id = thread.OSThreadId, managedId = thread.ManagedThreadId, frames, frameLimitReached = frames.Length == 32 });
                        }
                        if (totalFrames == 0) throw new InvalidOperationException("No managed stack frames were captured.");
                    }
                    finally { if (snapshot != null) { snapshot.Dispose(); disposed = true; } }
                }
                catch (Exception error) { failure = Limit(error.GetType().Name + ": " + error.Message); }
                var result = new
                {
                    kind = "lea-controlled-clr-snapshot", startedUtc = started, finishedUtc = DateTime.UtcNow,
                    pid, expectedParent = parent, expectedCreatedFileTime = birth,
                    expectedImage = args[3], expectedImageSHA256 = args[4], snapshotAttempted = attempted && !suspend,
                    snapshotCaptured = captured && !suspend, snapshotDisposed = disposed && !suspend, captureCompletedMs = capturedMs,
                    captureMode = suspend ? "suspended-target" : "snapshot", captureAttempted = attempted,
                    captureReady = captured, captureReleased = disposed,
                    requestedCaptureUtc = requestedUtc, completedCaptureUtc = capturedUtc, totalFrames,
                    elapsedMs = clock.ElapsedMilliseconds, runtimeVersion, localDacSHA256 = dacHash,
                    threads, threadLimit = 16, frameLimit = 32, error = failure, passed = failure == null,
                    mayPerturbTargetTiming = true, benchmark = false, rootCauseEstablished = false,
                    heapOrArgumentValuesCollected = false, symbolDownloadsEnabled = false
                };
                JsonSerializer.Serialize(output, result, new JsonSerializerOptions { WriteIndented = true });
                return failure == null ? 0 : 1;
            }
        }

        [StructLayout(LayoutKind.Sequential)] private struct BasicInfo { internal IntPtr Reserved, Peb, Reserved2, Reserved3, Pid, ParentPid; }
        [DllImport("kernel32.dll", SetLastError = true)] private static extern SafeProcessHandle OpenProcess(uint access, bool inherit, int pid);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool QueryFullProcessImageName(SafeProcessHandle handle, uint flags, StringBuilder name, ref uint size);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetProcessTimes(SafeProcessHandle process, out long created, out long exited, out long kernel, out long user);
        [DllImport("ntdll.dll")] private static extern int NtQueryInformationProcess(SafeProcessHandle process, int kind, ref BasicInfo info, int size, out int returned);
    }
}
