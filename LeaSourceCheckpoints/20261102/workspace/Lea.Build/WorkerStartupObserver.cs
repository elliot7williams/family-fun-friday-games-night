using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.Script.Serialization;
using Lea.Engine;
using Lea.Script;
using Lea.Script.Win;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal sealed class WorkerStartupObserver : IDisposable
    {
        private readonly V view;
        private readonly Func<E, P, Jx> original, wrapper;
        private readonly ManualResetEvent stop = new ManualResetEvent(false);
        private readonly List<object> samples = new List<object>();
        private readonly string previousTrace;
        private Thread observer;
        private Stopwatch clock;
        private string error;
        private bool ended, overflow, durationExceeded;

        internal WorkerStartupObserver(V target)
        {
            view = target;
            original = view.Xf ?? throw new InvalidOperationException("Expected configured script factory.");
            previousTrace = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE");
            Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", "1");
            wrapper = (engine, page) =>
            {
                var provider = original(engine, page);
                if (observer == null)
                {
                    var adapter = provider as W;
                    if (adapter == null || adapter.At == null) throw new InvalidOperationException("Expected isolated adapter and provider.");
                    var isolated = adapter.At;
                    clock = Stopwatch.StartNew();
                    observer = new Thread(() => Observe(isolated)) { IsBackground = true, Name = "Lea bounded startup counters" };
                    observer.Start();
                }
                return provider;
            };
            view.Xf = wrapper;
        }

        private void Observe(object provider)
        {
            try
            {
                var field = provider.GetType().GetField("worker", BindingFlags.Instance | BindingFlags.NonPublic);
                if (field == null) throw new InvalidOperationException("Missing endpoint field.");
                while (!stop.WaitOne(25))
                {
                    if (clock.ElapsedMilliseconds > 6000) { durationExceeded = true; break; }
                    if (samples.Count >= 240) { overflow = true; break; }
                    var endpoint = field.GetValue(provider);
                    if (endpoint == null) continue;
                    var type = endpoint.GetType();
                    var handleField = type.GetField("processHandle", BindingFlags.Instance | BindingFlags.NonPublic);
                    var traceProperty = type.GetProperty("E", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (handleField == null || traceProperty == null) throw new InvalidOperationException("Missing retained endpoint data.");
                    var handle = handleField.GetValue(endpoint) as SafeHandle;
                    if (handle == null) continue;
                    bool held = false;
                    try
                    {
                        handle.DangerousAddRef(ref held);
                        var started = clock.ElapsedMilliseconds;
                        var counters = Lea.Build.ExternalProcessCounters.Read(handle.DangerousGetHandle());
                        var trace = (string)traceProperty.GetValue(endpoint, null);
                        samples.Add(new { wallMs = started, samplingMs = clock.ElapsedMilliseconds - started, counters, trace });
                    }
                    catch (ObjectDisposedException) { break; }
                    finally { if (held) handle.DangerousRelease(); }
                }
            }
            catch (Exception failure) { error = failure.ToString(); }
        }

        internal void Stop()
        {
            if (ended) return;
            ended = true;
            stop.Set();
            var retired = observer == null || observer.Join(2000);
            Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", previousTrace);
            var traceRestored = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE") == previousTrace;
            var valid = retired && observer != null && samples.Count > 0 && error == null && !overflow && !durationExceeded;
            Console.WriteLine("WORKER_STARTUP_OBSERVATION " + new JavaScriptSerializer().Serialize(new
            {
                valid, retired, overflow, durationExceeded, error, traceRestored, samples,
                scope = "Focused first native-removal startup, timed trace and external sampling add cost; no PID reopen, runtime changes or full-prefix equivalence."
            }));
            if (!retired) throw new InvalidOperationException("Startup observer did not retire; diagnostic invalid.");
        }

        public void Dispose()
        {
            try { Stop(); }
            finally
            {
                if (ReferenceEquals(view.Xf, wrapper)) view.Xf = original;
                Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", previousTrace);
                if (observer == null || !observer.IsAlive) stop.Dispose();
            }
        }
    }
}
