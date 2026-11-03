using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Lea.Build
{
    public static class OwnedThreadSnapshotV2
    {
        [StructLayout(LayoutKind.Sequential)] private struct Unicode { public ushort Length, Maximum; public IntPtr Buffer; }
        [StructLayout(LayoutKind.Sequential)] private struct ProcessRow
        {
            public uint Next, Threads;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)] public byte[] Reserved1;
            public Unicode Name;
            public int Priority;
            public IntPtr Id, Reserved2;
            public uint Handles, Session;
            public IntPtr Reserved3;
            public UIntPtr PeakVirtual, Virtual;
            public uint Reserved4;
            public UIntPtr PeakWorkingSet, WorkingSet;
            public IntPtr Reserved5;
            public UIntPtr Paged;
            public IntPtr Reserved6;
            public UIntPtr NonPaged, Pagefile, PeakPagefile, PrivatePages;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public long[] Reserved7;
        }
        [StructLayout(LayoutKind.Sequential)] private struct ThreadRow
        {
            public long Kernel, User, Created;
            public uint WaitTime;
            public IntPtr Start, Process, Thread;
            public int Priority, BasePriority;
            public uint Switches, State, Reason;
        }
        public sealed class ThreadData
        {
            public uint Id, State, WaitReason, ContextSwitches;
            public long Created, CpuTicks;
        }
        public sealed class Sample
        {
            public uint ProcessId;
            public long ProcessCreated;
            public ThreadData[] Threads;
            public bool OriginalProcessHandleVerified;
        }
        [DllImport("ntdll.dll")] private static extern int NtQuerySystemInformation(int kind, IntPtr buffer, int size, out int needed);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern uint GetProcessId(IntPtr process);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern bool GetProcessTimes(IntPtr process, out long created, out long exited, out long kernel, out long user);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);
        private static void Identity(IntPtr handle, uint pid, long created)
        {
            if (handle == IntPtr.Zero || handle == new IntPtr(-1) || pid == 0 || created <= 0) throw new ArgumentException("Original process identity required");
            long birth, exit, kernel, user;
            if (WaitForSingleObject(handle, 0) != 258 || GetProcessId(handle) != pid || !GetProcessTimes(handle, out birth, out exit, out kernel, out user) || birth != created)
                throw new InvalidDataException("Original process is not the live expected identity");
        }
        public static bool LayoutSupported()
        { return IntPtr.Size == 8 && Marshal.SizeOf(typeof(ProcessRow)) == 256 && Marshal.SizeOf(typeof(ThreadRow)) == 80 && Marshal.OffsetOf(typeof(ProcessRow), "Id").ToInt32() == 80; }
        private static ThreadData[] Parse(IntPtr buffer, int used, uint pid)
        {
            if (!LayoutSupported()) throw new PlatformNotSupportedException("Checked Windows x64 layout required");
            int offset = 0, records = 0;
            while (true)
            {
                if (++records > 65536 || offset < 0 || used - offset < 256) throw new InvalidDataException("Process record bounds");
                var row = (ProcessRow)Marshal.PtrToStructure(IntPtr.Add(buffer, offset), typeof(ProcessRow));
                int span = row.Next == 0 ? used - offset : checked((int)row.Next);
                if (span < 256 || span > used - offset || (row.Next != 0 && row.Next % 8 != 0) || 256L + row.Threads * 80L > span)
                    throw new InvalidDataException("Thread record bounds");
                if (row.Id.ToInt64() == pid)
                {
                    // Bound target allocation without rejecting a large unrelated process.
                    if (row.Threads > 4096) throw new InvalidDataException("Target thread count bound");
                    var values = new List<ThreadData>();
                    var ids = new HashSet<uint>();
                    for (int i = 0; i < row.Threads; i++)
                    {
                        var thread = (ThreadRow)Marshal.PtrToStructure(IntPtr.Add(buffer, checked(offset + 256 + i * 80)), typeof(ThreadRow));
                        long id = thread.Thread.ToInt64();
                        if (thread.Process.ToInt64() != pid || id <= 0 || id > UInt32.MaxValue || thread.Created <= 0 || thread.Kernel < 0 || thread.User < 0 || thread.State > 9 || !ids.Add((uint)id))
                            throw new InvalidDataException("Thread identity/value bounds");
                        values.Add(new ThreadData { Id = (uint)id, Created = thread.Created, CpuTicks = checked(thread.Kernel + thread.User), State = thread.State, WaitReason = thread.Reason, ContextSwitches = thread.Switches });
                    }
                    if (values.Count == 0) throw new InvalidDataException("No live process threads");
                    return values.ToArray();
                }
                if (row.Next == 0) throw new InvalidDataException("Live owned process missing from snapshot");
                offset = checked(offset + span);
            }
        }
        public static void RejectTruncatedControl()
        {
            IntPtr memory = Marshal.AllocHGlobal(255);
            try { Parse(memory, 255, 1); }
            finally { Marshal.FreeHGlobal(memory); }
        }
        public static Sample Read(IntPtr originalHandle, uint pid, long created)
        {
            Identity(originalHandle, pid, created);
            int capacity = 65536;
            for (int attempt = 0; attempt < 6; attempt++)
            {
                if (capacity < 65536 || capacity > 16 * 1024 * 1024) throw new InvalidDataException("System snapshot allocation bound");
                IntPtr buffer = Marshal.AllocHGlobal(capacity);
                try
                {
                    int used;
                    int status = NtQuerySystemInformation(5, buffer, capacity, out used);
                    if (status == unchecked((int)0xC0000004)) { capacity = checked(Math.Max(capacity * 2, used + 65536)); continue; }
                    if (status != 0) throw new Win32Exception("NtQuerySystemInformation status " + status.ToString("X8"));
                    if (used < 256 || used > capacity) throw new InvalidDataException("System snapshot length bound");
                    ThreadData[] threads = Parse(buffer, used, pid);
                    Identity(originalHandle, pid, created);
                    return new Sample { ProcessId = pid, ProcessCreated = created, Threads = threads, OriginalProcessHandleVerified = true };
                }
                finally { Marshal.FreeHGlobal(buffer); }
            }
            throw new InvalidDataException("System snapshot retry bound");
        }
    }
}
