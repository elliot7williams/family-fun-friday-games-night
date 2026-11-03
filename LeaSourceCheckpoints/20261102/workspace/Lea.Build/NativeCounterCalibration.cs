using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Web.Script.Serialization;

namespace Lea.Script.Tests
{
    internal static class NativeCounterCalibration
    {
        public static int Main(string[] args)
        {
            using (var process = Process.GetCurrentProcess())
            {
                var before = NativeProcessSample.Read(process);
                var allocatedBefore = NativeProcessSample.AllocatedHere();
                var clock = Stopwatch.StartNew();
                while (clock.ElapsedMilliseconds < 5000 && NativeProcessSample.Cpu(process.Handle) - before.NativeCpuAfterTicks < 1000000)
                    Thread.SpinWait(100000);
                var afterCpu = NativeProcessSample.Read(process);
                var cpuControlWallMs = clock.ElapsedMilliseconds;
                var bytes = new byte[1024 * 1024];
                for (var i = 0; i < bytes.Length; i += 4096) bytes[i] = 37;
                var path = Path.Combine(args[0], "counter-calibration.bin");
                using (var file = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
                {
                    file.Write(bytes, 0, bytes.Length); file.Flush(true); file.Position = 0;
                    var read = 0;
                    while (read < bytes.Length)
                    {
                        var n = file.Read(bytes, read, bytes.Length - read);
                        if (n == 0) throw new EndOfStreamException();
                        read += n;
                    }
                }
                var afterIo = NativeProcessSample.Read(process);
                var allocatedAfter = NativeProcessSample.AllocatedHere();
                GC.KeepAlive(bytes);
                var idleBefore = NativeProcessSample.Read(process);
                var idleAllocatedBefore = NativeProcessSample.AllocatedHere();
                var idleWatch = Stopwatch.StartNew();
                Thread.Sleep(2000);
                var idleAfter = NativeProcessSample.Read(process);
                var idleAllocatedAfter = NativeProcessSample.AllocatedHere();
                var idleWallMs = idleWatch.ElapsedMilliseconds;
                var rejectedInvalidHandle = false;
                try { NativeProcessSample.Cpu(IntPtr.Zero); }
                catch (Win32Exception) { rejectedInvalidHandle = true; }
                var cpuAdvanced = afterCpu.NativeCpuBeforeTicks - before.NativeCpuAfterTicks >= 1000000;
                var brackets = Bracket(before) && Bracket(afterCpu) && Bracket(afterIo) && Bracket(idleBefore) && Bracket(idleAfter);
                var ioAdvanced = afterIo.ReadBytes - afterCpu.ReadBytes >= (ulong)bytes.Length && afterIo.WriteBytes - afterCpu.WriteBytes >= (ulong)bytes.Length;
                var monotonic = afterIo.PageFaults >= before.PageFaults && afterIo.NativeCpuBeforeTicks >= afterCpu.NativeCpuAfterTicks && idleAfter.PageFaults >= idleBefore.PageFaults && idleAfter.NativeCpuBeforeTicks >= idleBefore.NativeCpuAfterTicks;
                var gcMonotonic = afterIo.Gen0Collections >= before.Gen0Collections && afterIo.Gen1Collections >= before.Gen1Collections && afterIo.Gen2Collections >= before.Gen2Collections && afterIo.ManagedHeapBytes >= 0;
                var allocationSupported = allocatedBefore.HasValue && allocatedAfter.HasValue;
                var allocationAdvanced = !allocationSupported || allocatedAfter.Value - allocatedBefore.Value >= bytes.Length;
                var result = new { passed = cpuAdvanced && brackets && ioAdvanced && monotonic && rejectedInvalidHandle && gcMonotonic && allocationAdvanced,
                    cpuAdvanced, managedCpuInsideNativeBrackets = brackets, ioAdvanced, countersMonotonic = monotonic, rejectedInvalidHandle,
                    gcMonotonic, allocationSupported, allocationAdvanced, allocatedBefore, allocatedAfter,
                    before, afterCpu, afterIo, cpuControlWallMs, totalControlWallMs = clock.ElapsedMilliseconds,
                    idleBefore, idleAfter, idleWallMs, idleRequestedMs = 2000,
                    idleAllocationBytes = idleAllocatedBefore.HasValue && idleAllocatedAfter.HasValue ? (long?)(idleAllocatedAfter.Value - idleAllocatedBefore.Value) : null,
                    scope = "Standalone .NET Framework counter calibration; I/O is process accounting and page faults are not classified as hard faults; not interpreter or release qualification" };
                Console.WriteLine(new JavaScriptSerializer().Serialize(result));
                return result.passed ? 0 : 1;
            }
        }
        private static bool Bracket(NativeProcessSample value)
        { return value.NativeCpuBeforeTicks <= value.ManagedCpuTicks && value.ManagedCpuTicks <= value.NativeCpuAfterTicks; }
    }
}
