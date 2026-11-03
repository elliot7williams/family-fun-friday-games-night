namespace Lea.Script
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;

    internal static class BootstrapResourceCounters
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Memory
        {
            internal uint Size, Faults;
            internal UIntPtr PeakWorking, Working, PeakPaged, Paged, PeakNonpaged, Nonpaged, Commit, PeakCommit;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct Io
        {
            internal ulong ReadOperations, WriteOperations, OtherOperations, ReadBytes, WriteBytes, OtherBytes;
        }
        [DllImport("kernel32.dll")] private static extern IntPtr GetCurrentProcess();
        [DllImport("kernel32.dll")] private static extern IntPtr GetCurrentThread();
        [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
        [DllImport("kernel32.dll", SetLastError = true)] private static extern bool GetThreadTimes(IntPtr thread, out long creation, out long exit, out long kernel, out long user);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern bool GetProcessTimes(IntPtr process, out long creation, out long exit, out long kernel, out long user);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern bool GetProcessIoCounters(IntPtr process, out Io counters);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern bool QueryThreadCycleTime(IntPtr thread, out ulong cycles);
        [DllImport("psapi.dll", SetLastError = true)] private static extern bool GetProcessMemoryInfo(IntPtr process, ref Memory counters, uint bytes);

        private static void Require(bool ok) { if (!ok) throw new Win32Exception(Marshal.GetLastWin32Error()); }

        internal static string Capture(string label, long wall)
        {
            long creation, exit, kernel, user;
            var process = GetCurrentProcess();
            var thread = GetCurrentThread();
            Require(GetThreadTimes(thread, out creation, out exit, out kernel, out user));
            var threadCpu = checked(kernel + user);
            Require(GetProcessTimes(process, out creation, out exit, out kernel, out user));
            var processCpu = checked(kernel + user);
            var memory = new Memory { Size = (uint)Marshal.SizeOf(typeof(Memory)) };
            Require(GetProcessMemoryInfo(process, ref memory, memory.Size));
            Io io;
            Require(GetProcessIoCounters(process, out io));
            ulong cycles;
            Require(QueryThreadCycleTime(thread, out cycles));
            // Sequential, non-atomic snapshots; faults do not distinguish hard from soft.
            return string.Format(CultureInfo.InvariantCulture,
                "LEA_BOOT_RESOURCE {0} wall={1} thread={2} process={3} faults={4} read={5} write={6} other={7} cycles={8} working={9} commit={10} tid={11}",
                label, wall, threadCpu, processCpu, memory.Faults, io.ReadBytes, io.WriteBytes, io.OtherBytes, cycles,
                memory.Working.ToUInt64(), memory.Commit.ToUInt64(), GetCurrentThreadId());
        }
    }
}
