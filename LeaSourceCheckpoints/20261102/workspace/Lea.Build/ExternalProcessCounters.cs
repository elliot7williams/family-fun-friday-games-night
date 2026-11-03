using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Lea.Build
{
    public static class ExternalProcessCounters
    {
        public sealed class Sample
        {
            public long CpuTicks;
            public ulong ReadBytes, WriteBytes, WorkingSetBytes, CommitBytes, MinimumWorkingSet, MaximumWorkingSet, AvailablePhysicalBytes, TotalPhysicalBytes;
            public uint PageFaults, WorkingSetFlags, MemoryLoadPercent;
        }
        public static Sample Read(IntPtr originalHandle)
        {
            long created, exited, kernel, user;
            if (!GetProcessTimes(originalHandle, out created, out exited, out kernel, out user)) Fail("GetProcessTimes");
            Io io;
            if (!GetProcessIoCounters(originalHandle, out io)) Fail("GetProcessIoCounters");
            var memory = new Memory { Size = (uint)Marshal.SizeOf(typeof(Memory)) };
            if (!GetProcessMemoryInfo(originalHandle, ref memory, memory.Size)) Fail("GetProcessMemoryInfo");
            UIntPtr minimum, maximum; uint flags;
            if (!GetProcessWorkingSetSizeEx(originalHandle, out minimum, out maximum, out flags)) Fail("GetProcessWorkingSetSizeEx");
            var global = new Global { Length = (uint)Marshal.SizeOf(typeof(Global)) };
            if (!GlobalMemoryStatusEx(ref global)) Fail("GlobalMemoryStatusEx");
            return new Sample { CpuTicks = checked(kernel + user), ReadBytes = io.ReadBytes, WriteBytes = io.WriteBytes,
                WorkingSetBytes = memory.WorkingSet.ToUInt64(), CommitBytes = memory.Pagefile.ToUInt64(), PageFaults = memory.PageFaults,
                MinimumWorkingSet = minimum.ToUInt64(), MaximumWorkingSet = maximum.ToUInt64(), WorkingSetFlags = flags,
                AvailablePhysicalBytes = global.AvailablePhysical, TotalPhysicalBytes = global.TotalPhysical, MemoryLoadPercent = global.Load };
        }
        private static void Fail(string api) { throw new Win32Exception(Marshal.GetLastWin32Error(), api); }
        [StructLayout(LayoutKind.Sequential)]
        private struct Io { public ulong ReadOperations, WriteOperations, OtherOperations, ReadBytes, WriteBytes, OtherBytes; }
        [StructLayout(LayoutKind.Sequential)]
        private struct Memory
        {
            public uint Size, PageFaults;
            public UIntPtr PeakWorkingSet, WorkingSet, PeakPagedPool, PagedPool, PeakNonPagedPool, NonPagedPool, Pagefile, PeakPagefile;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct Global
        {
            public uint Length, Load;
            public ulong TotalPhysical, AvailablePhysical, TotalPagefile, AvailablePagefile, TotalVirtual, AvailableVirtual, AvailableExtendedVirtual;
        }
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetProcessTimes(IntPtr process, out long created, out long exited, out long kernel, out long user);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetProcessIoCounters(IntPtr process, out Io io);
        [DllImport("psapi.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetProcessMemoryInfo(IntPtr process, ref Memory memory, uint size);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetProcessWorkingSetSizeEx(IntPtr process, out UIntPtr minimum, out UIntPtr maximum, out uint flags);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx(ref Global memory);
    }
}
