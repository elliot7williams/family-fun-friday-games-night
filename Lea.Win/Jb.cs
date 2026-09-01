using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Lea.Win
{
    // Jb owns the native Job Object that bounds one Lea image worker.
    internal sealed class Jb : IDisposable
    {
        private const uint ActiveProcess = 0x00000008;
        private const uint ProcessMemory = 0x00000100;
        private const uint DieOnException = 0x00000400;
        private const uint KillOnClose = 0x00002000;
        private IntPtr handle;

        private Jb(IntPtr value) { handle = value; }

        public static Jb Add(Process process, long memory)
        {
            if (process == null) throw new ArgumentNullException("process");
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
                if (!AssignProcessToJobObject(value, process.Handle))
                    throw Error("Lea could not assign the image worker to its job.");
                return new Jb(value);
            }
            catch
            {
                CloseHandle(value);
                throw;
            }
        }

        public void Dispose()
        {
            var value = Interlocked.Exchange(ref handle, IntPtr.Zero);
            if (value != IntPtr.Zero) CloseHandle(value);
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

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateJobObject(IntPtr attributes, string name);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetInformationJobObject(IntPtr job, int kind, ref ExtendedLimits limits, uint length);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr value);
    }
}
