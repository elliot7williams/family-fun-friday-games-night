using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.Script.Serialization;
using Lea.Build;
using Lea.Engine;
using Lea.Script;
#if LEA_THREAD_V2
using Snapshot = Lea.Build.OwnedThreadSnapshotV2;
#else
using Snapshot = Lea.Build.OwnedThreadSnapshot;
#endif

internal static class CombinedWorkerStartupProbe
{
    private static string runtime;
    private static readonly HashSet<string> libraries = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
        "Lea.Engine", "Lea.Script", "Lea.Win", "Jint", "Acornima", "System.Buffers", "System.Memory",
        "System.Numerics.Vectors", "System.Runtime.CompilerServices.Unsafe"
    };

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length != 2) return 2;
        runtime = Path.GetFullPath(args[0]);
        AppDomain.CurrentDomain.AssemblyResolve += Resolve;
        try { return Run(Path.GetFullPath(args[1])); }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        finally { AppDomain.CurrentDomain.AssemblyResolve -= Resolve; }
    }

    private static Assembly Resolve(object sender, ResolveEventArgs args)
    {
        var name = new AssemblyName(args.Name);
        if (!libraries.Contains(name.Name)) return null;
        var path = Path.Combine(runtime, name.Name + ".dll");
        if (AssemblyName.GetAssemblyName(path).FullName != name.FullName)
            throw new InvalidDataException("Diagnostic dependency identity differs.");
        return Assembly.LoadFrom(path);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int Run(string fixture)
    {
        var previous = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE");
        var engine = new E();
        var page = engine.Go(fixture);
        var script = new Jp(engine, page, Path.Combine(runtime, "ScriptWorker"));
        var stop = new ManualResetEvent(false);
        var samples = new List<object>();
        var watch = Stopwatch.StartNew();
        SafeHandle handle = null;
        bool held = false, exitedDuringCapture = false, overflow = false;
        string sampleError = null;
        var workerField = typeof(Jp).GetField("worker", BindingFlags.Instance | BindingFlags.NonPublic);
        Thread observer = null;
        try
        {
            if (workerField == null || !Snapshot.LayoutSupported() || script.Tm != 2000 || script.Cp != 2500 || script.Mm != 128L * 1024 * 1024)
                throw new InvalidOperationException("Diagnostic contract differs.");
            Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", "1");
            observer = new Thread(() => {
                try
                {
                    object endpoint = null;
                    PropertyInfo trace = null;
                    uint pid = 0;
                    long birth = 0;
                    while (!stop.WaitOne(100))
                    {
                        if (watch.ElapsedMilliseconds > 6000 || samples.Count >= 60) { overflow = true; break; }
                        if (handle == null)
                        {
                            endpoint = workerField.GetValue(script);
                            if (endpoint == null) continue;
                            var type = endpoint.GetType();
                            var field = type.GetField("processHandle", BindingFlags.Instance | BindingFlags.NonPublic);
                            trace = type.GetProperty("E");
                            if (field == null || trace == null) throw new InvalidOperationException("Original endpoint data missing.");
                            handle = (SafeHandle)field.GetValue(endpoint);
                            handle.DangerousAddRef(ref held);
                            var raw = handle.DangerousGetHandle();
                            pid = GetProcessId(raw);
                            long exit, kernel, user;
                            if (pid == 0 || !GetProcessTimes(raw, out birth, out exit, out kernel, out user))
                                throw new InvalidOperationException("Original worker identity unavailable.");
                        }
                        var original = handle.DangerousGetHandle();
                        if (WaitForSingleObject(original, 0) == 0) { exitedDuringCapture = true; break; }
                        var start = watch.ElapsedMilliseconds;
                        try
                        {
                            var counters = ExternalProcessCounters.Read(original);
                            var threads = Snapshot.Read(original, pid, birth);
                            samples.Add(new { wallMs = start, queryMs = watch.ElapsedMilliseconds - start, counters, threads, trace = (string)trace.GetValue(endpoint, null) });
                        }
                        catch
                        {
                            if (WaitForSingleObject(original, 0) != 0) throw;
                            exitedDuringCapture = true; break;
                        }
                    }
                }
                catch (Exception error) { sampleError = error.ToString(); }
            }) { IsBackground = true, Name = "Lea bounded combined-worker startup" };
            observer.Start();
            var result = script.Ex(engine.Q(page, "script").C[0].X);
            stop.Set();
            if (!observer.Join(2000)) throw new InvalidOperationException("Observer did not retire.");
            script.Dispose();
            var exited = held && WaitForSingleObject(handle.DangerousGetHandle(), 0) == 0;
            Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", previous);
            var restored = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE") == previous;
            var valid = samples.Count > 0 && sampleError == null && !overflow && exited && !script.Pc && restored;
            Console.WriteLine("COMBINED_WORKER_STARTUP " + new JavaScriptSerializer { MaxJsonLength = 2 * 1024 * 1024 }.Serialize(new {
                valid, scriptPassed = result.Ok, status = result.S, message = result.M, startup = script.Sd,
                mutations = result.N, title = page.T, exited, exitedDuringCapture, sampleError, overflow, traceRestored = restored,
                observerRetired = true, startupBudgetMs = 5000, executionBudgetMs = script.Tm, cpuRate = script.Cp, memoryBytes = script.Mm, samples,
                scope = "Single computed-script diagnostic; no full prefix equivalence, no VM instrumentation, no PID reopen. Sampling adds cost; I/O is logical and page faults are not exclusively hard faults."
            }));
            return valid ? 0 : 1;
        }
        finally
        {
            stop.Set();
            var retired = observer == null || observer.Join(2000);
            script.Dispose();
            Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", previous);
            if (!retired || script.Pc) { Console.Error.WriteLine("Retaining diagnostic owner for incomplete cleanup."); Thread.Sleep(Timeout.Infinite); }
            if (held) handle.DangerousRelease();
            stop.Dispose();
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)] private static extern uint GetProcessId(IntPtr process);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool GetProcessTimes(IntPtr process, out long created, out long exited, out long kernel, out long user);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern uint WaitForSingleObject(IntPtr process, uint milliseconds);
}
