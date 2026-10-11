using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Win32.SafeHandles;

namespace Lea.Diagnostics;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length != 5 || !int.TryParse(args[0], out var pid) || pid < 1 ||
            !int.TryParse(args[1], out var parent) || parent < 1 ||
            !long.TryParse(args[2], NumberStyles.None, CultureInfo.InvariantCulture, out var created) || created < 1 ||
            !Path.IsPathRooted(args[3]) || Path.GetFileName(args[3]) != "Lea.Script.Worker.exe" ||
            !Path.IsPathRooted(args[4]) || !Directory.Exists(Path.GetDirectoryName(args[4])) || File.Exists(args[4]))
        {
            Console.Error.WriteLine("Supply owned worker PID, test parent PID, UTC creation ticks, exact worker path and a new report path.");
            return 2;
        }
        var clock = Stopwatch.StartNew();
        var frames = new List<object>();
        string? error = null;
        var identityVerified = false;
        try
        {
            using var handle = OpenProcess(0x00101010, false, pid);
            if (handle.IsInvalid) throw new Win32Exception(Marshal.GetLastWin32Error());
            var image = new StringBuilder(4096);
            uint length = (uint)image.Capacity;
            var basic = new BasicInfo();
            if (!QueryFullProcessImageName(handle, 0, image, ref length) ||
                !GetProcessTimes(handle, out var birth, out _, out _, out _))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            var status = NtQueryInformationProcess(handle, 0, ref basic, Marshal.SizeOf<BasicInfo>(), out _);
            if (status != 0 || basic.Pid.ToInt64() != pid || basic.ParentPid.ToInt64() != parent ||
                DateTime.FromFileTimeUtc(birth).Ticks != created ||
                !string.Equals(Path.GetFullPath(image.ToString()), Path.GetFullPath(args[3]), StringComparison.OrdinalIgnoreCase) ||
                WaitForSingleObject(handle, 0) != 0x102)
                throw new InvalidOperationException("PID, parent, creation time, image or live process identity differs.");
            identityVerified = true;

            // No suspension, dumps, locals or symbol-server lookup; the DAC is from this Windows installation.
            using var target = DataTarget.AttachToProcess(pid, suspend: false);
            var info = target.ClrVersions.Single();
            var framework = target.DataReader.PointerSize == 8 ? "Framework64" : "Framework";
            var dac = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Microsoft.NET", framework, "v4.0.30319", "mscordacwks.dll");
            using var runtime = info.CreateRuntime(dac);
            foreach (var thread in runtime.Threads.Take(16))
            {
                var stack = thread.EnumerateStackTrace().Take(64).Select(frame => new
                {
                    kind = frame.Kind.ToString(), ip = frame.InstructionPointer, sp = frame.StackPointer,
                    method = Limit(frame.Method?.Signature ?? frame.ToString() ?? "", 256)
                }).ToArray();
                frames.Add(new { osThreadId = thread.OSThreadId, managedThreadId = thread.ManagedThreadId,
                    alive = thread.IsAlive, gcMode = thread.GCMode.ToString(), stack });
            }
            if (frames.Count == 0) throw new InvalidOperationException("No managed thread snapshots available.");
            if (WaitForSingleObject(handle, 0) != 0x102) throw new InvalidOperationException("Worker exited during the live snapshot.");
        }
        catch (Exception failure) { error = Limit(failure.GetType().Name + ": " + failure.Message, 1024); }
        var report = new { kind = "owned-lea-worker-managed-stacks", pid, parentPid = parent,
            createdUtcTicks = created, worker = Path.GetFullPath(args[3]), identityVerified,
            suspended = false, dumpWritten = false, symbolServerUsed = false,
            consistentSnapshotClaim = false, elapsedMs = clock.ElapsedMilliseconds,
            maxThreads = 16, maxFramesPerThread = 64, error, threads = frames };
        using (var output = new FileStream(args[4], FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            JsonSerializer.Serialize(output, report, new JsonSerializerOptions { WriteIndented = true });
        return error == null ? 0 : 1;
    }

    private static string Limit(string value, int maximum) => value.Length <= maximum ? value : value[..maximum];
    [StructLayout(LayoutKind.Sequential)] private struct BasicInfo
    {
        public IntPtr Reserved, Peb, Reserved2, Reserved3, Pid, ParentPid;
    }
    [DllImport("kernel32.dll", SetLastError = true)] private static extern SafeProcessHandle OpenProcess(uint access, [MarshalAs(UnmanagedType.Bool)] bool inherit, int pid);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool QueryFullProcessImageName(SafeProcessHandle handle, uint flags, StringBuilder path, ref uint length);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetProcessTimes(SafeProcessHandle handle, out long created, out long exited, out long kernel, out long user);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern uint WaitForSingleObject(SafeProcessHandle handle, uint timeout);
    [DllImport("ntdll.dll")] private static extern int NtQueryInformationProcess(SafeProcessHandle handle, int kind, ref BasicInfo info, int size, out int returned);
}
