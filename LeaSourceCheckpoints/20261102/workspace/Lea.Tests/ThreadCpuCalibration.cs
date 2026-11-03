using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.Script.Serialization;
using Lea.Build;

internal static class ThreadCpuCalibration
{
    [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
    [DllImport("kernel32.dll")] private static extern IntPtr GetCurrentThread();
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool GetThreadTimes(IntPtr thread, out long created, out long exited, out long kernel, out long user);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool QueryThreadCycleTime(IntPtr thread, out ulong cycles);
    private static long sink;
    private static int checks;
    private static void Check(bool value, string name) { if (!value) throw new Exception(name); checks++; }
    private static long Cpu()
    {
        long birth, exit, kernel, user;
        if (!GetThreadTimes(GetCurrentThread(), out birth, out exit, out kernel, out user)) throw new System.ComponentModel.Win32Exception();
        return checked(kernel + user);
    }
    private sealed class Point
    {
        public long NativeBefore, NativeAfter, SnapshotCpu;
        public ulong Cycles;
        public uint Switches;
    }
    private static Point Read(Process process, uint thread, long birth)
    {
        long before = Cpu();
        var sampled = OwnedThreadSnapshot.Read(process.Handle, (uint)process.Id, birth).Threads.Single(item => item.Id == thread);
        long after = Cpu(); ulong cycles;
        if (!QueryThreadCycleTime(GetCurrentThread(), out cycles)) throw new System.ComponentModel.Win32Exception();
        // Native accounting can be quantized; allow one 15.625ms tick around the query.
        Check(sampled.CpuTicks >= before - 156250 && sampled.CpuTicks <= after + 156250, "snapshot CPU bracket agrees with GetThreadTimes");
        return new Point { NativeBefore = before, NativeAfter = after, SnapshotCpu = sampled.CpuTicks, Cycles = cycles, Switches = sampled.ContextSwitches };
    }
    private static int Main(string[] args)
    {
        try
        {
            var rows = new List<object>();
            using (var process = Process.GetCurrentProcess())
            {
                uint thread = GetCurrentThreadId(); long birth = process.StartTime.ToUniversalTime().ToFileTimeUtc();
                var initial = OwnedThreadSnapshot.Read(process.Handle, (uint)process.Id, birth);
                Check(initial.Threads.OrderBy(item => item.Created).ThenBy(item => item.Id).First().Id == thread, "earliest native thread is this control Main");
                for (int round = 0; round < 2; round++)
                foreach (string mode in new[] { "arithmetic", "spin", "sleep" })
                {
                    Point first = Read(process, thread, birth); var clock = Stopwatch.StartNew();
                    if (mode == "sleep") Thread.Sleep(300);
                    else while (clock.ElapsedMilliseconds < 300)
                    {
                        if (mode == "spin") Thread.SpinWait(1000);
                        else for (int i = 0; i < 4096; i++) sink = unchecked(sink * 1664525L + 1013904223L);
                    }
                    Point last = Read(process, thread, birth); long wall = clock.ElapsedMilliseconds;
                    long nativeTicks = last.NativeAfter - first.NativeAfter, snapshotTicks = last.SnapshotCpu - first.SnapshotCpu;
                    Check(nativeTicks >= 0 && snapshotTicks >= 0 && last.Cycles >= first.Cycles && last.Switches >= first.Switches, "monotonic native counters");
                    Check(Math.Abs(nativeTicks - snapshotTicks) <= 312500, "CPU deltas agree within two accounting ticks");
                    Check(wall >= 250, "control workload duration observed");
                    if (mode == "arithmetic") Check(nativeTicks > 0 && last.Cycles > first.Cycles, "observable arithmetic advances CPU and cycle counters");
                    if (mode == "sleep") Check(nativeTicks <= wall * 10000L / 4 + 312500, "sleep has low reported CPU");
                    rows.Add(new { round, mode, wallMs = wall, getThreadTimesMs = nativeTicks / 10000D, snapshotCpuMs = snapshotTicks / 10000D, cycles = last.Cycles - first.Cycles, contextSwitches = last.Switches - first.Switches });
                }
            }
            File.WriteAllText(args[0], new JavaScriptSerializer().Serialize(new { passed = true, checks, rows, sink, interpreterExecuted = false, cycleCountsConvertedToTime = false }));
            Console.WriteLine("THREAD_CPU_CALIBRATION PASS " + checks); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
