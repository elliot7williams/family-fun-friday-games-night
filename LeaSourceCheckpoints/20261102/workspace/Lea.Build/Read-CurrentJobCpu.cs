using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public static class CurrentJobCpu
{
    public sealed class Report
    {
        public bool MembershipQuerySucceeded, InJob, CpuQuerySucceeded;
        public int MembershipError, CpuQueryError;
        public uint ControlFlags, RawRateOrWeight;
        public double? ImmediateHardCapPercent;
        public string Scope = "Read-only calling process immediate job; outer jobs and system scheduling not inspected; no settings changed";
    }
    public static Report Read()
    {
        var report = new Report();
        using (var process = Process.GetCurrentProcess())
        {
            bool inJob;
            report.MembershipQuerySucceeded = IsProcessInJob(process.Handle, IntPtr.Zero, out inJob);
            if (!report.MembershipQuerySucceeded) { report.MembershipError = Marshal.GetLastWin32Error(); return report; }
            report.InJob = inJob;
            if (!inJob) return report;
            Cpu control;
            uint returned;
            report.CpuQuerySucceeded = QueryInformationJobObject(IntPtr.Zero, 15, out control, (uint)Marshal.SizeOf(typeof(Cpu)), out returned);
            if (!report.CpuQuerySucceeded) { report.CpuQueryError = Marshal.GetLastWin32Error(); return report; }
            report.ControlFlags = control.Flags; report.RawRateOrWeight = control.Value;
            if ((control.Flags & 5) == 5 && (control.Flags & 18) == 0 && control.Value > 0 && control.Value <= 10000)
                report.ImmediateHardCapPercent = control.Value / 100.0;
            return report;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct Cpu { public uint Flags, Value; }
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsProcessInJob(IntPtr process, IntPtr job, [MarshalAs(UnmanagedType.Bool)] out bool result);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool QueryInformationJobObject(IntPtr job, int kind, out Cpu information, uint length, out uint returned);
}
