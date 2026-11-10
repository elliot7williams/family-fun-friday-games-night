using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Web.Script.Serialization;
using Lea.Diagnostics;

namespace Lea.Script.Tests
{
    internal static class SelectorObservation
    {
        private sealed class Endpoint
        {
            public string E { get { return "startup init\n" + new string('x', 200) + "\r\n"; } }
        }

        private sealed class Session
        {
            private readonly object gate = new object();
            private readonly Endpoint worker = new Endpoint();
            internal readonly ManualResetEvent Read = new ManualResetEvent(false);
            internal bool Protected;
            public int Pid
            {
                get
                {
                    Protected = Monitor.IsEntered(gate) && worker != null;
                    Read.Set();
                    using (var process = Process.GetCurrentProcess()) return process.Id;
                }
            }
        }

        internal static void Contract()
        {
            var directory = Path.Combine(Path.GetTempPath(), "lea-selector-observer-contract-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var previous = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE");
            var workerDirectory = Path.Combine(directory, "unused-worker");
            var expected = new InvalidOperationException("original selector observation failure");
            try
            {
                Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", "selector-contract-original");
                using (var entered = new ManualResetEvent(false))
                {
                    var probe = new WorkerObservation(() => { entered.Set(); return null; }, workerDirectory);
                    var retired = false;
                    try { probe.Start(); Check(entered.WaitOne(3000), "session polling runs without UI pumping"); }
                    finally { retired = probe.Stop(); }
                    Check(retired, "empty session sampler retires");
                    Check(probe.Stop(), "session stop remains idempotent");
                    Check(Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE") == "selector-contract-original", "empty session restores trace setting");
                    var path = Path.Combine(directory, "empty.json");
                    probe.Save(path, "selector-worker-session", expected);
                    var report = Read(path);
                    Check((int)report["viewCapacity"] == 0 && (int)report["sessionCapacity"] == 1 && (int)report["sampleCapacity"] == 512,
                        "session scope retains bounded sampler capacity without claiming view coverage");
                    Check(((IList)report["samples"]).Count == 0 && ((IList)report["observerErrors"]).Count == 0,
                        "null session is not an observation error or a fabricated sample");
                    Check(!(bool)report["flowPassed"] && ((string)report["exception"]).Contains(expected.Message) && (bool)report["samplerRetired"],
                        "failed session report preserves original failure and retirement");
                    Check((string)report["worker"] == Path.Combine(workerDirectory, "Lea.Script.Worker.exe"), "session sampler retains exact supplied worker path");
                }
                var session = new Session();
                try
                {
                    var probe = new WorkerObservation(() => session, workerDirectory);
                    var retired = false;
                    try { probe.Start(); Check(session.Read.WaitOne(3000), "direct session snapshot reads the supplied endpoint"); }
                    finally { retired = probe.Stop(); }
                    Check(retired, "identity-rejection sampler retires");
                    var path = Path.Combine(directory, "identity.json");
                    probe.Save(path, "selector-worker-session", null);
                    var rejected = false;
                    try { probe.Save(path, "selector-worker-session", null); }
                    catch (IOException) { rejected = true; }
                    Check(rejected, "session reports preserve create-new output semantics");
                    var report = Read(path);
                    var samples = (IList)report["samples"];
                    Check(samples.Count > 0 && samples.Count <= 512 &&
                        ((string)((Dictionary<string, object>)samples[0])["Error"]).Contains("Observed PID no longer identifies"),
                        "an unrelated process is rejected before counters or thread enumeration");
                    Check(((string)((Dictionary<string, object>)samples[0])["Phase"]).Length == 160,
                        "session phase uses the existing latest-line and text bound");
                    Check(session.Protected, "PID and phase snapshot holds the session lifecycle gate");
                    Check(!probe.HasCounters && !probe.HasThreads, "identity failure is not classified as successful sampling");
                }
                finally { session.Read.Dispose(); }
                using (var entered = new ManualResetEvent(false))
                {
                    var probe = new WorkerObservation(() => { entered.Set(); throw new InvalidOperationException(new string('e', 600)); }, workerDirectory);
                    var retired = false;
                    try { probe.Start(); Check(entered.WaitOne(3000), "expected callback failure is observed"); }
                    finally { retired = probe.Stop(); }
                    Check(retired, "failed callback sampler retires");
                    var path = Path.Combine(directory, "callback.json");
                    probe.Save(path, "selector-worker-session", expected);
                    var report = Read(path);
                    var errors = (IList)report["observerErrors"];
                    Check(errors.Count > 0 && errors.Count <= 32 && ((string)errors[0]).Length <= 256 &&
                        ((IList)report["samples"]).Count == 0, "callback errors retain existing text/count bounds without fabricated samples");
                    Check(Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE") == "selector-contract-original", "failed callback restores trace setting");
                }
            }
            finally
            {
                Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", previous);
                foreach (var name in new[] { "empty.json", "identity.json", "callback.json" })
                {
                    var path = Path.Combine(directory, name);
                    if (File.Exists(path)) File.Delete(path);
                }
                Directory.Delete(directory);
            }
        }

        private static Dictionary<string, object> Read(string path)
        { return new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(File.ReadAllText(path)); }

        private static void Check(bool value, string name)
        { Program.Check(value, "selector observation contract: " + name); }
    }
}
