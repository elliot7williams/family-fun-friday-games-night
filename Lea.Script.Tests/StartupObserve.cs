using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Lea.Script;
using Lea.Script.Win;
using Lea.Win;
using Microsoft.Win32.SafeHandles;

namespace Lea.Script.Tests
{
    internal static class StartupObserve
    {
        internal static void Run(string fixture, string reportPath)
        {
            reportPath = Path.GetFullPath(reportPath);
            if (File.Exists(reportPath)) throw new ArgumentException("The observation report must not already exist.", "reportPath");
            if (!Directory.Exists(Path.GetDirectoryName(reportPath))) throw new ArgumentException("The observation report directory must already exist.", "reportPath");
            var trace = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE");
            var clock = Stopwatch.StartNew();
            var probe = new Probe(clock);
            Exception failure = null;
            bool retired;
            try
            {
                Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", "1");
                probe.Start();
                try { View.Run(fixture, reportPath + "-view"); }
                catch (Exception error) { failure = error; }
            }
            finally
            {
                retired = probe.Stop();
                Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", trace);
            }
            var samples = probe.Samples();
            var exception = failure == null ? "" : failure.ToString();
            var report = new { kind = "native-view-worker-observation", existingViewTestPassed = failure == null,
                exception = Limit(exception, 8192), exceptionTruncated = exception.Length > 8192,
                intervalMs = 125, sampleCapacity = 512, threadCapacity = 16, samplerRetired = retired,
                droppedRequests = probe.DroppedRequests, droppedSamples = probe.DroppedSamples,
                observerErrors = probe.Errors(), elapsedMs = clock.ElapsedMilliseconds, samples,
                worker = probe.WorkerPath, parentPid = probe.ParentPid, timingTrace = true,
                flow = "existing View.Run without retries or altered limits", startupBudgetMs = 5000,
                coldCacheControlled = false, productionRuntimeModified = false };
            using (var output = new FileStream(reportPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(output, new UTF8Encoding(false)))
                writer.Write(new JavaScriptSerializer { MaxJsonLength = 2 * 1024 * 1024, RecursionLimit = 16 }.Serialize(report));
            if (failure != null) ExceptionDispatchInfo.Capture(failure).Throw();
            Program.Check(retired, "native observation sampler retires after the existing view flow");
            Program.Check(samples.Any(sample => sample.Error == null && sample.Threads.Length > 0), "native observation captures an owned worker with thread/counter data");
        }

        private static string Limit(string value, int maximum) { return value.Length <= maximum ? value : value.Substring(0, maximum); }

        private sealed class Request
        {
            internal int Pid;
            internal long QueuedMs;
            internal string Phase;
        }

        private sealed class Shot
        {
            public int Pid;
            public long QueuedMs, ObservedMs, SamplingMs, CpuMs;
            public string Phase, Error;
            public uint PageFaults;
            public ulong WorkingSetBytes, CommitBytes, ReadOperations, ReadBytes, WriteOperations, WriteBytes;
            public object[] Threads = new object[0];
            public bool ThreadsTruncated;
        }

        private sealed class Probe
        {
            private static readonly FieldInfo Endpoint = typeof(Jp).GetField("worker", BindingFlags.Instance | BindingFlags.NonPublic);
            private static readonly PropertyInfo Diagnostics = Endpoint.FieldType.GetProperty("E");
            private readonly BlockingCollection<Request> pending = new BlockingCollection<Request>(1);
            private readonly Queue<Shot> samples = new Queue<Shot>();
            private readonly List<string> errors = new List<string>();
            private readonly object gate = new object();
            private readonly Stopwatch clock;
            private readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 125 };
            private readonly Thread consumer;
            private bool started;
            internal readonly int ParentPid = Process.GetCurrentProcess().Id;
            internal readonly string WorkerPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker", "Lea.Script.Worker.exe"));
            internal int DroppedRequests, DroppedSamples;

            internal Probe(Stopwatch clock)
            {
                this.clock = clock;
                consumer = new Thread(Consume) { IsBackground = true, Name = "Lea worker observation" };
                timer.Tick += Tick;
            }

            internal void Start() { consumer.Start(); started = true; timer.Start(); }
            internal bool Stop()
            {
                timer.Stop();
                timer.Dispose();
                pending.CompleteAdding();
                var retired = !started || consumer.Join(5000);
                if (retired) pending.Dispose();
                return retired;
            }
            internal Shot[] Samples() { lock (gate) return samples.ToArray(); }
            internal string[] Errors() { lock (gate) return errors.ToArray(); }

            private static IEnumerable<Control> Controls(Control parent)
            {
                yield return parent;
                foreach (Control child in parent.Controls)
                    foreach (var nested in Controls(child)) yield return nested;
            }

            private void Tick(object sender, EventArgs args)
            {
                try
                {
                    foreach (Form form in Application.OpenForms)
                        foreach (var view in Controls(form).OfType<V>())
                        {
                            var adapter = view.Xs as W;
                            if (adapter == null || adapter.At.Pid == 0) continue;
                            var endpoint = Endpoint.GetValue(adapter.At);
                            var text = endpoint == null ? "" : ((string)Diagnostics.GetValue(endpoint, null)).TrimEnd('\r', '\n');
                            var index = text.LastIndexOf('\n');
                            var phase = index < 0 ? text : text.Substring(index + 1);
                            if (!pending.TryAdd(new Request { Pid = adapter.At.Pid, QueuedMs = clock.ElapsedMilliseconds, Phase = Limit(phase, 160) }))
                                Interlocked.Increment(ref DroppedRequests);
                        }
                }
                catch (Exception error) when (error is InvalidOperationException || error is TargetInvocationException || error is ArgumentException)
                { lock (gate) if (errors.Count < 32) errors.Add(Limit(error.GetType().Name + ": " + error.Message, 256)); }
            }

            private void Consume()
            {
                foreach (var request in pending.GetConsumingEnumerable())
                {
                    var shot = new Shot { Pid = request.Pid, QueuedMs = request.QueuedMs, Phase = request.Phase, ObservedMs = clock.ElapsedMilliseconds };
                    var duration = Stopwatch.StartNew();
                    try { Read(shot); }
                    catch (Exception error) when (error is Win32Exception || error is InvalidOperationException || error is ArgumentException || error is NotSupportedException || error is EntryPointNotFoundException || error is DllNotFoundException)
                    { shot.Error = Limit(error.GetType().Name + ": " + error.Message, 256); }
                    shot.SamplingMs = duration.ElapsedMilliseconds;
                    lock (gate)
                    {
                        if (samples.Count == 512) { samples.Dequeue(); DroppedSamples++; }
                        samples.Enqueue(shot);
                    }
                }
            }

            private void Read(Shot shot)
            {
                // Keep the identity-checked process alive as an object until sampling finishes.
                using (var handle = OpenProcess(0x1000, false, shot.Pid))
                {
                    if (handle.IsInvalid) throw new Win32Exception(Marshal.GetLastWin32Error());
                    var path = new StringBuilder(4096);
                    uint size = 4096;
                    if (!QueryFullProcessImageName(handle, 0, path, ref size)) throw new Win32Exception(Marshal.GetLastWin32Error());
                    var basic = new BasicInfo();
                    int returned;
                    var status = NtQueryInformationProcess(handle, 0, ref basic, Marshal.SizeOf(typeof(BasicInfo)), out returned);
                    if (status != 0) throw new InvalidOperationException("Process identity query failed: 0x" + status.ToString("X8"));
                    if (basic.Pid.ToInt64() != shot.Pid || basic.ParentPid.ToInt64() != ParentPid ||
                        !String.Equals(Path.GetFullPath(path.ToString()), WorkerPath, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Observed PID no longer identifies this test's worker.");
                    var io = new IoCounters();
                    var memory = new MemoryCounters { Size = (uint)Marshal.SizeOf(typeof(MemoryCounters)) };
                    if (!GetProcessIoCounters(handle, out io) || !GetProcessMemoryInfo(handle, ref memory, memory.Size))
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    shot.PageFaults = memory.PageFaults;
                    shot.WorkingSetBytes = memory.WorkingSet.ToUInt64();
                    shot.CommitBytes = memory.Commit.ToUInt64();
                    shot.ReadOperations = io.ReadOperations; shot.ReadBytes = io.ReadBytes;
                    shot.WriteOperations = io.WriteOperations; shot.WriteBytes = io.WriteBytes;
                    long created, exited, kernel, user;
                    if (!GetProcessTimes(handle, out created, out exited, out kernel, out user))
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    shot.CpuMs = (kernel + user) / 10000;
                    using (var process = Process.GetProcessById(shot.Pid))
                    {
                        var threads = process.Threads;
                        try
                        {
                            shot.ThreadsTruncated = threads.Count > 16;
                            shot.Threads = threads.Cast<ProcessThread>().Take(16).Select(thread => ReadThread(thread, shot.Pid)).ToArray();
                        }
                        finally { foreach (ProcessThread thread in threads) thread.Dispose(); }
                    }
                }
            }

            private static object ReadThread(ProcessThread thread, int pid)
            {
                using (var handle = OpenThread(0x0800, false, thread.Id))
                {
                    if (handle.IsInvalid) throw new Win32Exception(Marshal.GetLastWin32Error());
                    var owner = GetProcessIdOfThread(handle);
                    if (owner == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
                    if (owner != pid) throw new InvalidOperationException("Observed thread no longer belongs to this test's worker.");
                    long created, exited, kernel, user;
                    if (!GetThreadTimes(handle, out created, out exited, out kernel, out user))
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    var state = thread.ThreadState;
                    return new { id = thread.Id, state = state.ToString(),
                        wait = state == System.Diagnostics.ThreadState.Wait ? thread.WaitReason.ToString() : null,
                        cpuMs = (kernel + user) / 10000, startedUtc = DateTime.FromFileTimeUtc(created).ToString("o") };
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BasicInfo { internal IntPtr Reserved, Peb, Reserved2, Reserved3, Pid, ParentPid; }
        [StructLayout(LayoutKind.Sequential)]
        private struct IoCounters { internal ulong ReadOperations, WriteOperations, OtherOperations, ReadBytes, WriteBytes, OtherBytes; }
        [StructLayout(LayoutKind.Sequential)]
        private struct MemoryCounters
        {
            internal uint Size, PageFaults;
            internal UIntPtr PeakWorkingSet, WorkingSet, PeakPagedPool, PagedPool, PeakNonPagedPool, NonPagedPool, Commit, PeakCommit;
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern SafeProcessHandle OpenProcess(uint access, bool inherit, int pid);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern SafeWaitHandle OpenThread(uint access, bool inherit, int tid);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint GetProcessIdOfThread(SafeWaitHandle thread);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetProcessTimes(SafeProcessHandle process, out long created, out long exited, out long kernel, out long user);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetThreadTimes(SafeWaitHandle thread, out long created, out long exited, out long kernel, out long user);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool QueryFullProcessImageName(SafeProcessHandle process, uint flags, StringBuilder path, ref uint size);
        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(SafeProcessHandle process, int kind, ref BasicInfo info, int size, out int returned);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetProcessIoCounters(SafeProcessHandle process, out IoCounters counters);
        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetProcessMemoryInfo(SafeProcessHandle process, ref MemoryCounters counters, uint size);
    }
}
