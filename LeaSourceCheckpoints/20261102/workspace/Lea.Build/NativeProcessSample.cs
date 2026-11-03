using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Lea.Script.Tests
{
    internal sealed class NativeProcessSample
    {
        public long NativeCpuBeforeTicks, ManagedCpuTicks, NativeCpuAfterTicks;
        public ulong ReadOperations, WriteOperations, OtherOperations, ReadBytes, WriteBytes, OtherBytes, WorkingSetBytes;
        public uint PageFaults;
        public long ManagedHeapBytes;
        public int Gen0Collections, Gen1Collections, Gen2Collections;
        private static readonly Func<long> allocatedHere = FindAllocationCounter();

        private static Func<long> FindAllocationCounter()
        {
            var method = typeof(GC).GetMethod("GetAllocatedBytesForCurrentThread", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
            return method == null ? null : (Func<long>)Delegate.CreateDelegate(typeof(Func<long>), method);
        }
        // Call only on the thread whose allocations are being measured.
        internal static long? AllocatedHere() { return allocatedHere == null ? (long?)null : allocatedHere(); }

        internal static long Cpu(IntPtr handle)
        {
            long created, exited, kernel, user;
            if (!GetProcessTimes(handle, out created, out exited, out kernel, out user)) throw new Win32Exception(Marshal.GetLastWin32Error(), "GetProcessTimes");
            return checked(kernel + user);
        }
        internal static NativeProcessSample Read(Process process)
        {
            var handle = process.Handle;
            var sample = new NativeProcessSample();
            sample.NativeCpuBeforeTicks = Cpu(handle);
            sample.ManagedCpuTicks = process.TotalProcessorTime.Ticks;
            sample.NativeCpuAfterTicks = Cpu(handle);
            Io io;
            if (!GetProcessIoCounters(handle, out io)) throw new Win32Exception(Marshal.GetLastWin32Error(), "GetProcessIoCounters");
            var memory = new Memory { Size = (uint)Marshal.SizeOf(typeof(Memory)) };
            if (!GetProcessMemoryInfo(handle, ref memory, memory.Size)) throw new Win32Exception(Marshal.GetLastWin32Error(), "GetProcessMemoryInfo");
            sample.ReadOperations = io.ReadOperations; sample.WriteOperations = io.WriteOperations; sample.OtherOperations = io.OtherOperations;
            sample.ReadBytes = io.ReadBytes; sample.WriteBytes = io.WriteBytes; sample.OtherBytes = io.OtherBytes;
            sample.WorkingSetBytes = memory.WorkingSet.ToUInt64(); sample.PageFaults = memory.PageFaults;
            sample.ManagedHeapBytes = GC.GetTotalMemory(false);
            sample.Gen0Collections = GC.CollectionCount(0); sample.Gen1Collections = GC.CollectionCount(1); sample.Gen2Collections = GC.CollectionCount(2);
            return sample;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct Io { public ulong ReadOperations, WriteOperations, OtherOperations, ReadBytes, WriteBytes, OtherBytes; }
        [StructLayout(LayoutKind.Sequential)]
        private struct Memory
        {
            public uint Size, PageFaults;
            public UIntPtr PeakWorkingSet, WorkingSet, PeakPagedPool, PagedPool, PeakNonPagedPool, NonPagedPool, Pagefile, PeakPagefile;
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetProcessTimes(IntPtr process, out long created, out long exited, out long kernel, out long user);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetProcessIoCounters(IntPtr process, out Io counters);
        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetProcessMemoryInfo(IntPtr process, ref Memory counters, uint size);
    }
}
