using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Web.Script.Serialization;

namespace Lea.Script.Tests
{
    internal static class FrameworkGuardCost
    {
        private static long sink;
        private static int Main()
        {
            var rows = new List<object>();
            bool passed = true;
            using (var process = Process.GetCurrentProcess())
            {
                // Separate API controls, not interpreter execution or a security bypass.
                for (int round = 0; round < 2; round++)
                {
                    foreach (string mode in new[] { "empty", "stack", "allocation" })
                    {
                        var before = NativeProcessSample.Read(process);
                        var clock = Stopwatch.StartNew();
                        for (int i = 0; i < 10000; i++)
                        {
                            if (mode == "stack") RuntimeHelpers.EnsureSufficientExecutionStack();
                            else if (mode == "allocation") sink = NativeProcessSample.AllocatedHere() ?? -1;
                            else sink = i;
                        }
                        clock.Stop();
                        var after = NativeProcessSample.Read(process);
                        bool monotonic = after.NativeCpuAfterTicks >= before.NativeCpuAfterTicks && after.PageFaults >= before.PageFaults;
                        passed &= monotonic && clock.ElapsedMilliseconds < 10000 && sink >= 0;
                        rows.Add(new { round, mode, iterations = 10000, elapsedMs = clock.ElapsedMilliseconds, before, after, monotonic });
                    }
                }
            }
            Console.WriteLine(new JavaScriptSerializer().Serialize(new { passed, rows, interpreterExecuted = false, securitySettingsChanged = false }));
            return passed ? 0 : 1;
        }
    }
}
