using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Web.Script.Serialization;
using Lea.Script;

namespace Lea.Script.Tests
{
    internal static class BootstrapCostObserver
    {
        internal static Jr Run(Js script, string code)
        {
            var field = typeof(Js).GetField("startupStage", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) throw new InvalidOperationException("Expected startup stage field");
            var samples = new List<object>();
            var stop = new ManualResetEvent(false);
            var watch = Stopwatch.StartNew();
            var process = Process.GetCurrentProcess();
            var beforeCounters = NativeProcessSample.Read(process);
            var cpuStart = TimeSpan.FromTicks(beforeCounters.ManagedCpuTicks);
            var allocationThread = Thread.CurrentThread.ManagedThreadId;
            var allocatedBefore = NativeProcessSample.AllocatedHere();
            var bracketFailures = 0;
            string observerError = null;
            bool overflow = false, durationExceeded = false;
            var observer = new Thread(() =>
            {
                try
                {
                    string previous = null;
                    long last = -100;
                    while (!stop.WaitOne(10))
                    {
                        var elapsed = watch.ElapsedMilliseconds;
                        if (elapsed > 10000) { durationExceeded = true; break; }
                        var stage = (string)field.GetValue(script);
                        if (stage != previous || elapsed - last >= 100)
                        {
                            var counterWatch = Stopwatch.StartNew();
                            var counters = NativeProcessSample.Read(process);
                            if (counters.ManagedCpuTicks < counters.NativeCpuBeforeTicks || counters.ManagedCpuTicks > counters.NativeCpuAfterTicks) bracketFailures++;
                            if (samples.Count < 128) samples.Add(new { wallMs = elapsed,
                                cpuMs = (long)TimeSpan.FromTicks(counters.ManagedCpuTicks - beforeCounters.ManagedCpuTicks).TotalMilliseconds,
                                nativeCpuMs = (long)TimeSpan.FromTicks(counters.NativeCpuAfterTicks - beforeCounters.NativeCpuAfterTicks).TotalMilliseconds,
                                counterSamplingMs = counterWatch.Elapsed.TotalMilliseconds, counters, stage });
                            else overflow = true;
                            previous = stage; last = elapsed;
                        }
                    }
                }
                catch (Exception error) { observerError = error.ToString(); }
            }) { IsBackground = true, Name = "Lea bounded same-context bootstrap observer" };
            Jr result = null;
            bool retired = false;
            try
            {
                observer.Start();
                result = script.Ex(code);
                return result;
            }
            finally
            {
                var wall = watch.ElapsedMilliseconds;
                stop.Set(); retired = observer.Join(2000);
                var allocatedAfter = NativeProcessSample.AllocatedHere();
                var allocationThreadMatched = Thread.CurrentThread.ManagedThreadId == allocationThread;
                var allocationBytes = allocationThreadMatched && allocatedBefore.HasValue && allocatedAfter.HasValue
                    ? (long?)(allocatedAfter.Value - allocatedBefore.Value) : null;
                var afterCounters = NativeProcessSample.Read(process);
                var report = new
                {
                    wallMs = wall, cpuMs = (long)(process.TotalProcessorTime - cpuStart).TotalMilliseconds,
                    status = result == null ? null : result.S, message = result == null ? null : result.M,
                    executionOk = result != null && result.Ok, mutations = result == null ? -1 : result.N,
                    observerRetired = retired, observerError, samplesOverflow = overflow, durationExceeded,
                    beforeCounters, afterCounters, cpuBracketsValid = bracketFailures == 0,
                    allocationThreadMatched, allocationBytes, allocationIncludesWrapperOverhead = true,
                    samples = retired ? samples : null, budgetChanged = false,
                    scope = "Same suite prefix, first computed call only; original current-process handle counters; logical I/O and all page faults, not hard faults; CPU includes observer; not exclusive timing or full-suite qualification"
                };
                Console.WriteLine("LEA_BOOTSTRAP_COST " + new JavaScriptSerializer().Serialize(report));
                if (retired) { stop.Dispose(); process.Dispose(); }
                if (!retired || observerError != null || overflow || durationExceeded)
                    throw new InvalidOperationException("Bootstrap observer did not finish cleanly");
            }
        }
    }
}
