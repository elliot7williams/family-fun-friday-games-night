using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Lea.Win
{
    // Jb owns the native Job Object that bounds one Lea image worker.
    internal sealed class Jb : IDisposable
    {
        private const uint ActiveProcess = 0x00000008;
        private const uint ProcessMemory = 0x00000100;
        private const uint DieOnException = 0x00000400;
        private const uint KillOnClose = 0x00002000;
        private const uint CpuEnable = 0x00000001;
        private const uint CpuHardCap = 0x00000004;
        private readonly object gate = new object();
        private IntPtr handle;

        public int C { get; private set; }

        private Jb(IntPtr value, int cpu) { handle = value; C = cpu; }

        public static Jb Add(IntPtr process, long memory, int cpu)
        {
            if (process == IntPtr.Zero) throw new ArgumentException("Image-worker process handle is required.", "process");
            if (cpu < 100 || cpu > 10000) throw new ArgumentOutOfRangeException("cpu");
            var value = CreateJobObject(IntPtr.Zero, null);
            if (value == IntPtr.Zero) throw Error("Lea could not create the image-worker job.");
            try
            {
                var limits = new ExtendedLimits();
                limits.Basic.LimitFlags = ActiveProcess | ProcessMemory | DieOnException | KillOnClose;
                limits.Basic.ActiveProcessLimit = 1;
                limits.ProcessMemoryLimit = new UIntPtr((ulong)memory);
                if (!SetInformationJobObject(value, 9, ref limits, (uint)Marshal.SizeOf(typeof(ExtendedLimits))))
                    throw Error("Lea could not limit the image-worker job.");
                var cpuLimit = new CpuLimits { ControlFlags = CpuEnable | CpuHardCap, CpuRate = (uint)cpu };
                if (!SetCpuInformation(value, 15, ref cpuLimit, (uint)Marshal.SizeOf(typeof(CpuLimits))))
                    throw Error("Lea could not limit image-worker CPU rate.");
                var applied = new CpuLimits();
                if (!QueryCpuInformation(value, 15, ref applied, (uint)Marshal.SizeOf(typeof(CpuLimits)), IntPtr.Zero))
                    throw Error("Lea could not verify image-worker CPU rate.");
                if ((applied.ControlFlags & (CpuEnable | CpuHardCap)) != (CpuEnable | CpuHardCap) || applied.CpuRate != (uint)cpu)
                    throw new InvalidOperationException("Lea image-worker CPU rate was not applied.");
                if (!AssignProcessToJobObject(value, process))
                    throw Error("Lea could not assign the image worker to its job.");
                return new Jb(value, cpu);
            }
            catch
            {
                CloseHandle(value);
                throw;
            }
        }

        public long Peak
        {
            get
            {
                lock (gate)
                {
                    if (handle == IntPtr.Zero) return 0;
                    var limits = new ExtendedLimits();
                    if (!QueryInformationJobObject(handle, 9, ref limits,
                        (uint)Marshal.SizeOf(typeof(ExtendedLimits)), IntPtr.Zero)) return 0;
                    return unchecked((long)limits.PeakProcessMemoryUsed.ToUInt64());
                }
            }
        }

        public void Dispose()
        {
            lock (gate)
            {
                var value = handle;
                handle = IntPtr.Zero;
                if (value != IntPtr.Zero) CloseHandle(value);
            }
        }

        private static Win32Exception Error(string message)
        {
            return new Win32Exception(Marshal.GetLastWin32Error(), message);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BasicLimits
        {
            public long PerProcessUserTimeLimit;
            public long PerJobUserTimeLimit;
            public uint LimitFlags;
            public UIntPtr MinimumWorkingSetSize;
            public UIntPtr MaximumWorkingSetSize;
            public uint ActiveProcessLimit;
            public UIntPtr Affinity;
            public uint PriorityClass;
            public uint SchedulingClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IoCounters
        {
            public ulong ReadOperationCount;
            public ulong WriteOperationCount;
            public ulong OtherOperationCount;
            public ulong ReadTransferCount;
            public ulong WriteTransferCount;
            public ulong OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ExtendedLimits
        {
            public BasicLimits Basic;
            public IoCounters Io;
            public UIntPtr ProcessMemoryLimit;
            public UIntPtr JobMemoryLimit;
            public UIntPtr PeakProcessMemoryUsed;
            public UIntPtr PeakJobMemoryUsed;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CpuLimits
        {
            public uint ControlFlags;
            public uint CpuRate;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateJobObject(IntPtr attributes, string name);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetInformationJobObject(IntPtr job, int kind, ref ExtendedLimits limits, uint length);

        [DllImport("kernel32.dll", EntryPoint = "SetInformationJobObject", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCpuInformation(IntPtr job, int kind, ref CpuLimits limits, uint length);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool QueryInformationJobObject(IntPtr job, int kind, ref ExtendedLimits limits,
            uint length, IntPtr returned);

        [DllImport("kernel32.dll", EntryPoint = "QueryInformationJobObject", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool QueryCpuInformation(IntPtr job, int kind, ref CpuLimits limits,
            uint length, IntPtr returned);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr value);
    }
}
