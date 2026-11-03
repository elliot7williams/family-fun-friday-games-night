using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;

namespace Lea.Script.Tests
{
    internal static class FrameworkDeepGuardCost
    {
        [StructLayout(LayoutKind.Sequential, Size = 256)]
        private struct Frame { internal long Value; }
        private static long sink;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static long AtDepth(int depth, Action action)
        {
            Frame frame = new Frame { Value = depth };
            long child = 0;
            if (depth == 0) action();
            else child = AtDepth(depth - 1, action);
            Keep(ref frame);
            return frame.Value + child;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Keep(ref Frame frame) { sink = frame.Value; }

        private static int Main()
        {
            var rows = new List<object>();
            bool passed = true;
            using (var process = Process.GetCurrentProcess())
                for (int round = 0; round < 2; round++)
                    foreach (int depth in new[] { 0, 64, 128 })
                        foreach (string mode in new[] { "empty", "stack", "allocation" })
                        {
                            bool rowPassed = false;
                            long checksum = AtDepth(depth, () => {
                                var before = NativeProcessSample.Read(process);
                                var allocation = NativeProcessSample.AllocatedHere();
                                var clock = Stopwatch.StartNew();
                                for (int i = 0; i < 10000; i++)
                                {
                                    if (mode == "stack") RuntimeHelpers.EnsureSufficientExecutionStack();
                                    else if (mode == "allocation") sink = NativeProcessSample.AllocatedHere() ?? -1;
                                    else sink = i;
                                }
                                clock.Stop();
                                var allocatedAfter = NativeProcessSample.AllocatedHere();
                                var after = NativeProcessSample.Read(process);
                                bool monotonic = after.NativeCpuAfterTicks >= before.NativeCpuAfterTicks && after.PageFaults >= before.PageFaults;
                                rowPassed = monotonic && clock.ElapsedMilliseconds < 10000 && sink >= 0 && allocation.HasValue && allocatedAfter >= allocation;
                                rows.Add(new { round, depth, mode, iterations = 10000, elapsedMs = clock.ElapsedMilliseconds,
                                    cpuTicks = after.NativeCpuAfterTicks - before.NativeCpuAfterTicks,
                                    faults = after.PageFaults - before.PageFaults, allocatedBytes = allocatedAfter - allocation,
                                    monotonic, rowPassed });
                            });
                            passed &= rowPassed && checksum == (long)depth * (depth + 1) / 2;
                        }
            passed &= rows.Count == 18;
            Console.WriteLine(new JavaScriptSerializer().Serialize(new {
                passed, rows, depthLimit = 128, framePaddingBytes = 256, samplesInsideDeepFrame = true,
                interpreterExecuted = false, securitySettingsChanged = false,
                scope = "Bounded synthetic managed-frame guard controls, not measured interpreter stack shape or proof of fault causality"
            }));
            return passed ? 0 : 1;
        }
    }
}
