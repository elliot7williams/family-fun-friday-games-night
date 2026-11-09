using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Lea.Build
{
    public static class CurrentSchedulingPolicy
    {
        [StructLayout(LayoutKind.Sequential)] private struct Memory { public uint Priority; }
        [StructLayout(LayoutKind.Sequential)] private struct Power { public uint Version, Control, State; }
        public sealed class Snapshot
        {
            public int Pid;
            public uint Tid, PriorityClass, ProcessMemoryPriority, ThreadMemoryPriority;
            public long BirthFileTime;
            public bool ProcessPowerQuery, ThreadPowerQuery;
            public int ProcessPowerError, ThreadPowerError;
            public uint ProcessPowerControl, ProcessPowerState, ThreadPowerControl, ThreadPowerState;
        }
        [DllImport("kernel32.dll")] private static extern IntPtr GetCurrentProcess();
        [DllImport("kernel32.dll")] private static extern IntPtr GetCurrentThread();
        [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
        [DllImport("kernel32.dll", SetLastError = true)] private static extern uint GetPriorityClass(IntPtr process);
        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "GetProcessInformation")]
        [return: MarshalAs(UnmanagedType.Bool)] private static extern bool ProcessMemory(IntPtr process, int kind, out Memory value, uint size);
        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "GetThreadInformation")]
        [return: MarshalAs(UnmanagedType.Bool)] private static extern bool ThreadMemory(IntPtr thread, int kind, out Memory value, uint size);
        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "GetProcessInformation")]
        [return: MarshalAs(UnmanagedType.Bool)] private static extern bool ProcessPower(IntPtr process, int kind, ref Power value, uint size);
        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "GetThreadInformation")]
        [return: MarshalAs(UnmanagedType.Bool)] private static extern bool ThreadPower(IntPtr thread, int kind, ref Power value, uint size);

        public static Snapshot Capture()
        {
            if (Marshal.SizeOf(typeof(Memory)) != 4 || Marshal.SizeOf(typeof(Power)) != 12)
                throw new InvalidOperationException("Unexpected scheduling policy structure size.");
            var result = new Snapshot { Tid = GetCurrentThreadId() };
            using (var self = Process.GetCurrentProcess())
            { result.Pid = self.Id; result.BirthFileTime = self.StartTime.ToUniversalTime().ToFileTimeUtc(); }
            var process = GetCurrentProcess(); var thread = GetCurrentThread();
            result.PriorityClass = GetPriorityClass(process);
            if (result.PriorityClass == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
            Memory memory;
            if (!ProcessMemory(process, 0, out memory, 4)) throw new Win32Exception(Marshal.GetLastWin32Error());
            result.ProcessMemoryPriority = memory.Priority;
            if (!ThreadMemory(thread, 0, out memory, 4)) throw new Win32Exception(Marshal.GetLastWin32Error());
            result.ThreadMemoryPriority = memory.Priority;
            var power = new Power { Version = 1 };
            result.ProcessPowerQuery = ProcessPower(process, 4, ref power, 12);
            result.ProcessPowerError = result.ProcessPowerQuery ? 0 : Marshal.GetLastWin32Error();
            if (result.ProcessPowerQuery) { result.ProcessPowerControl = power.Control; result.ProcessPowerState = power.State; }
            power = new Power { Version = 1 };
            result.ThreadPowerQuery = ThreadPower(thread, 3, ref power, 12);
            result.ThreadPowerError = result.ThreadPowerQuery ? 0 : Marshal.GetLastWin32Error();
            if (result.ThreadPowerQuery) { result.ThreadPowerControl = power.Control; result.ThreadPowerState = power.State; }
            return result;
        }
    }
}
