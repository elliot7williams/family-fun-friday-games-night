using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using Lea.Win;
using Microsoft.Win32.SafeHandles;

namespace Lea.Diagnostics
{
    internal sealed class WorkerObservation
    {
        private sealed class Request { internal int Pid; internal long QueuedMs; internal string Phase; }
        private sealed class Shot
        {
            public int Pid;
            public long QueuedMs, ObservedMs, SamplingMs, CounterSamplingMs, ThreadSamplingMs, CpuMs;
            public string Phase, Error, ThreadError, StartedUtc;
            public uint PageFaults;
            public ulong WorkingSetBytes, CommitBytes, ReadOperations, ReadBytes, WriteOperations, WriteBytes;
            public object[] Threads = new object[0];
            public bool ThreadsSampled, ThreadsTruncated;
        }

        private readonly Func<IEnumerable<V>> views;
        private readonly BlockingCollection<Request> pending = new BlockingCollection<Request>(1);
        private readonly Queue<Shot> samples = new Queue<Shot>();
        private readonly List<string> errors = new List<string>();
        private readonly Dictionary<string, long> nextThreads = new Dictionary<string, long>();
        private readonly object gate = new object();
        private readonly Stopwatch clock = new Stopwatch();
        private readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 125 };
        private readonly Thread consumer;
        private readonly int parentPid;
        private readonly string workerPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker", "Lea.Script.Worker.exe"));
        private string previousTrace;
        private bool started, stopped, retired;
        private int droppedRequests, droppedSamples;

        internal WorkerObservation(Func<IEnumerable<V>> views)
        {
            this.views = views;
            using (var process = Process.GetCurrentProcess()) parentPid = process.Id;
            consumer = new Thread(Consume) { IsBackground = true, Name = "Lea worker observation" };
            timer.Tick += Tick;
        }

        internal static string ValidateReport(string path)
        {
            path = Path.GetFullPath(path);
            if (Path.GetFileName(path).IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException("Invalid observation report filename.", "path");
            if (File.Exists(path) || Directory.Exists(path)) throw new ArgumentException("The observation report must not already exist.", "path");
            if (!Directory.Exists(Path.GetDirectoryName(path))) throw new ArgumentException("The observation report directory must already exist.", "path");
            return path;
        }

        internal void Start()
        {
            previousTrace = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE");
            Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", "1");
            clock.Start();
            consumer.Start();
            started = true;
            timer.Start();
        }

        internal bool Stop()
        {
            if (stopped) return retired;
            stopped = true;
            try
            {
                timer.Stop(); timer.Dispose();
                pending.CompleteAdding();
                retired = !started || consumer.Join(5000);
                if (retired) pending.Dispose();
                return retired;
            }
            finally { Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", previousTrace); }
        }

        private Shot[] Samples() { lock (gate) return samples.ToArray(); }
        internal bool HasCounters { get { return Samples().Any(sample => sample.Error == null); } }
        internal bool HasThreads { get { return Samples().Any(sample => sample.Threads.Length > 0); } }

        internal void Save(string path, string kind, Exception failure)
        {
            var exception = failure == null ? "" : failure.ToString();
            var report = new { format = 2, kind, flowPassed = failure == null,
                exception = Limit(exception, 8192), exceptionTruncated = exception.Length > 8192,
                intervalMs = 125, threadIntervalMs = 1000, sampleCapacity = 512, threadCapacity = 16,
                workerPacingCapacity = 32, viewCapacity = 4, samplerRetired = retired,
                droppedRequests, droppedSamples, observerErrors = errors.ToArray(), elapsedMs = clock.ElapsedMilliseconds,
                samples = Samples(), worker = workerPath, parentPid, timingTrace = true, startupBudgetMs = 5000,
                coldCacheControlled = false, productionRuntimeModified = false };
            using (var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(output, new UTF8Encoding(false)))
                writer.Write(new JavaScriptSerializer { MaxJsonLength = 2 * 1024 * 1024, RecursionLimit = 16 }.Serialize(report));
        }

        private static string Limit(string value, int maximum) { return value.Length <= maximum ? value : value.Substring(0, maximum); }
        private static bool Expected(Exception error)
        {
            return error is Win32Exception || error is InvalidOperationException || error is ArgumentException ||
                error is NotSupportedException || error is EntryPointNotFoundException || error is DllNotFoundException ||
                error is TargetInvocationException;
        }

        private void Tick(object sender, EventArgs args)
        {
            try
            {
                foreach (var view in views().Take(4))
                {
                    var adapter = view.Xs;
                    if (adapter == null || adapter.GetType().FullName != "Lea.Script.Win.W") continue;
                    var at = adapter.GetType().GetProperty("At").GetValue(adapter, null);
                    var pid = (int)at.GetType().GetProperty("Pid").GetValue(at, null);
                    if (pid == 0) continue;
                    var field = at.GetType().GetField("worker", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (field == null) throw new InvalidOperationException("Worker observation endpoint contract changed.");
                    var endpoint = field.GetValue(at);
                    var text = endpoint == null ? "" : ((string)endpoint.GetType().GetProperty("E").GetValue(endpoint, null)).TrimEnd('\r', '\n');
                    var index = text.LastIndexOf('\n');
                    var phase = index < 0 ? text : text.Substring(index + 1);
                    if (!pending.TryAdd(new Request { Pid = pid, QueuedMs = clock.ElapsedMilliseconds, Phase = Limit(phase, 160) }))
                        Interlocked.Increment(ref droppedRequests);
                }
            }
            catch (Exception error)
            {
                if (!Expected(error)) throw;
                lock (gate) if (errors.Count < 32) errors.Add(Limit(error.GetType().Name + ": " + error.Message, 256));
            }
        }

        private void Consume()
        {
            foreach (var request in pending.GetConsumingEnumerable())
            {
                var shot = new Shot { Pid = request.Pid, QueuedMs = request.QueuedMs, Phase = request.Phase, ObservedMs = clock.ElapsedMilliseconds };
                var duration = Stopwatch.StartNew();
                try { Read(shot); }
                catch (Exception error) { if (!Expected(error)) throw; shot.Error = Limit(error.GetType().Name + ": " + error.Message, 256); }
                shot.SamplingMs = duration.ElapsedMilliseconds;
                lock (gate)
                {
                    if (samples.Count == 512) { samples.Dequeue(); droppedSamples++; }
                    samples.Enqueue(shot);
                }
            }
        }

        private void Read(Shot shot)
        {
            var counters = Stopwatch.StartNew();
            // Retain the identity-checked process object until every query finishes.
            using (var handle = OpenProcess(0x1000, false, shot.Pid))
            {
                if (handle.IsInvalid) throw new Win32Exception(Marshal.GetLastWin32Error());
                var path = new StringBuilder(4096);
                uint size = 4096;
                if (!QueryFullProcessImageName(handle, 0, path, ref size)) throw new Win32Exception(Marshal.GetLastWin32Error());
                var basic = new BasicInfo(); int returned;
                var status = NtQueryInformationProcess(handle, 0, ref basic, Marshal.SizeOf(typeof(BasicInfo)), out returned);
                if (status != 0) throw new InvalidOperationException("Process identity query failed: 0x" + status.ToString("X8"));
                if (basic.Pid.ToInt64() != shot.Pid || basic.ParentPid.ToInt64() != parentPid ||
                    !String.Equals(Path.GetFullPath(path.ToString()), workerPath, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Observed PID no longer identifies this test's worker.");
                IoCounters io;
                var memory = new MemoryCounters { Size = (uint)Marshal.SizeOf(typeof(MemoryCounters)) };
                if (!GetProcessIoCounters(handle, out io) || !GetProcessMemoryInfo(handle, ref memory, memory.Size))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                shot.PageFaults = memory.PageFaults; shot.WorkingSetBytes = memory.WorkingSet.ToUInt64(); shot.CommitBytes = memory.Commit.ToUInt64();
                shot.ReadOperations = io.ReadOperations; shot.ReadBytes = io.ReadBytes; shot.WriteOperations = io.WriteOperations; shot.WriteBytes = io.WriteBytes;
                long created, exited, kernel, user;
                if (!GetProcessTimes(handle, out created, out exited, out kernel, out user)) throw new Win32Exception(Marshal.GetLastWin32Error());
                shot.CpuMs = (kernel + user) / 10000;
                shot.StartedUtc = DateTime.FromFileTimeUtc(created).ToString("o");
                shot.CounterSamplingMs = counters.ElapsedMilliseconds;
                var identity = shot.Pid + ":" + created;
                long next;
                if (nextThreads.TryGetValue(identity, out next) && shot.ObservedMs < next) return;
                if (!nextThreads.ContainsKey(identity) && nextThreads.Count == 32)
                    nextThreads.Remove(nextThreads.OrderBy(pair => pair.Value).First().Key);
                nextThreads[identity] = shot.ObservedMs + 1000;
                shot.ThreadsSampled = true;
                var duration = Stopwatch.StartNew();
                try
                {
                    using (var process = Process.GetProcessById(shot.Pid))
                    {
                        var threads = process.Threads;
                        try { shot.ThreadsTruncated = threads.Count > 16; shot.Threads = threads.Cast<ProcessThread>().Take(16).Select(thread => ReadThread(thread, shot.Pid)).ToArray(); }
                        finally { foreach (ProcessThread thread in threads) thread.Dispose(); }
                    }
                }
                catch (Exception error) { if (!Expected(error)) throw; shot.ThreadError = Limit(error.GetType().Name + ": " + error.Message, 256); }
                shot.ThreadSamplingMs = duration.ElapsedMilliseconds;
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
                if (!GetThreadTimes(handle, out created, out exited, out kernel, out user)) throw new Win32Exception(Marshal.GetLastWin32Error());
                var state = thread.ThreadState;
                return new { id = thread.Id, state = state.ToString(), wait = state == System.Diagnostics.ThreadState.Wait ? thread.WaitReason.ToString() : null,
                    cpuMs = (kernel + user) / 10000, startedUtc = DateTime.FromFileTimeUtc(created).ToString("o") };
            }
        }

        [StructLayout(LayoutKind.Sequential)] private struct BasicInfo { internal IntPtr Reserved, Peb, Reserved2, Reserved3, Pid, ParentPid; }
        [StructLayout(LayoutKind.Sequential)] private struct IoCounters { internal ulong ReadOperations, WriteOperations, OtherOperations, ReadBytes, WriteBytes, OtherBytes; }
        [StructLayout(LayoutKind.Sequential)] private struct MemoryCounters
        {
            internal uint Size, PageFaults;
            internal UIntPtr PeakWorkingSet, WorkingSet, PeakPagedPool, PagedPool, PeakNonPagedPool, NonPagedPool, Commit, PeakCommit;
        }
        [DllImport("kernel32.dll", SetLastError = true)] private static extern SafeProcessHandle OpenProcess(uint access, bool inherit, int pid);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern SafeWaitHandle OpenThread(uint access, bool inherit, int tid);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern uint GetProcessIdOfThread(SafeWaitHandle thread);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetProcessTimes(SafeProcessHandle process, out long created, out long exited, out long kernel, out long user);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetThreadTimes(SafeWaitHandle thread, out long created, out long exited, out long kernel, out long user);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool QueryFullProcessImageName(SafeProcessHandle process, uint flags, StringBuilder path, ref uint size);
        [DllImport("ntdll.dll")] private static extern int NtQueryInformationProcess(SafeProcessHandle process, int kind, ref BasicInfo info, int size, out int returned);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetProcessIoCounters(SafeProcessHandle process, out IoCounters counters);
        [DllImport("psapi.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetProcessMemoryInfo(SafeProcessHandle process, ref MemoryCounters counters, uint size);
    }
}
