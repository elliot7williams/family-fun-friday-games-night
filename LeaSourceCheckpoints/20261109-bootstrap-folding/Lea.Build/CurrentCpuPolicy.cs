using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Lea.Build
{
    public static class CurrentCpuPolicy
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Rate { public uint Flags, Value; }

        public sealed class Snapshot
        {
            public int ProcessId;
            public bool InJob, QuerySucceeded;
            public int QueryError;
            public uint RawFlags, RawValue;
            public string Mode;
            public double? Percent;
            public uint? Weight, MinimumRate, MaximumRate;
        }

        [DllImport("kernel32.dll")] private static extern IntPtr GetCurrentProcess();
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsProcessInJob(IntPtr process, IntPtr job, [MarshalAs(UnmanagedType.Bool)] out bool result);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool QueryInformationJobObject(IntPtr job, int kind, out Rate rate, uint size, out uint returned);

        public static Snapshot Decode(uint flags, uint value)
        {
            if ((flags & ~31u) != 0 || ((flags & 1) == 0 && flags != 0))
                throw new ArgumentException("Invalid CPU control flags.");
            bool weighted = (flags & 2) != 0, hard = (flags & 4) != 0, range = (flags & 16) != 0;
            if ((weighted && (hard || range)) || (range && hard))
                throw new ArgumentException("Contradictory CPU control modes.");
            var result = new Snapshot { RawFlags = flags, RawValue = value };
            if (flags == 0) { result.Mode = "disabled"; return result; }
            if (weighted)
            {
                if (value < 1 || value > 9) throw new ArgumentOutOfRangeException("value");
                result.Mode = "weight"; result.Weight = value;
            }
            else if (range)
            {
                uint minimum = value & 65535, maximum = value >> 16;
                if (minimum > maximum || maximum == 0 || maximum > 10000)
                    throw new ArgumentOutOfRangeException("value");
                result.Mode = "min-max"; result.MinimumRate = minimum; result.MaximumRate = maximum;
            }
            else
            {
                if (value == 0 || value > 10000) throw new ArgumentOutOfRangeException("value");
                result.Mode = hard ? "hard-cap" : "rate"; result.Percent = value / 100.0;
            }
            return result;
        }

        public static Snapshot Capture()
        {
            if (Marshal.SizeOf(typeof(Rate)) != 8) throw new InvalidOperationException("Unexpected CPU policy layout.");
            bool inJob;
            if (!IsProcessInJob(GetCurrentProcess(), IntPtr.Zero, out inJob))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            int pid;
            using (var self = Process.GetCurrentProcess()) pid = self.Id;
            if (!inJob) return new Snapshot { ProcessId = pid, Mode = "no-job", InJob = false };
            Rate rate; uint returned;
            if (!QueryInformationJobObject(IntPtr.Zero, 15, out rate, 8, out returned))
                return new Snapshot { ProcessId = pid, InJob = true, Mode = "query-failed", QueryError = Marshal.GetLastWin32Error() };
            if (returned != 8) throw new InvalidOperationException("Truncated CPU policy query.");
            var result = Decode(rate.Flags, rate.Value);
            result.ProcessId = pid; result.InJob = true; result.QuerySucceeded = true;
            return result;
        }
    }
}
