using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Lea.Engine;
using Lea.Win;

namespace Lea.Script
{
    public sealed class Jp : IDisposable
    {
        private static readonly string[] Names = { "Lea.Script.Worker.exe", "Lea.Script.Worker.exe.config",
            "Jint.dll", "Acornima.dll", "System.Memory.dll", "System.Buffers.dll",
            "System.Numerics.Vectors.dll", "System.Runtime.CompilerServices.Unsafe.dll" };
        private readonly E engine;
        private readonly P page;
        private readonly Ja bridge;
        private readonly int thread;
        private readonly string directory;
        private readonly string profile = "Lea.Script." + Guid.NewGuid().ToString("N");
        private readonly object gate = new object();
        private readonly List<FileStream> leases = new List<FileStream>();
        private Rp worker;
        private long generation;
        private long peak;
        private int sequence;
        private int tm = 2000;
        private bool disposed;
        private bool running;
        private volatile bool stopped;

        public Jp(E engine, P page) : this(engine, page,
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker")) { }

        public Jp(E engine, P page, string workerDirectory)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            if (page == null) throw new ArgumentNullException("page");
            engine.Q(page, "body");
            if (!Object.ReferenceEquals(engine.At, page)) throw new ArgumentException("The page must be active.", "page");
            this.engine = engine;
            this.page = page;
            directory = Path.GetFullPath(workerDirectory);
            if (Path.GetPathRoot(directory).StartsWith("\\\\", StringComparison.Ordinal))
                throw new ArgumentException("Worker files must be on a local ACL-capable volume.", "workerDirectory");
            bridge = new Ja(engine, page);
            thread = Thread.CurrentThread.ManagedThreadId;
            generation = engine.Jg;
        }

        public Jr At { get; private set; }
        public int Tm { get { return tm; } set { tm = Math.Max(25, Math.Min(5000, value)); } }
        public long Mm { get { return 128L * 1024 * 1024; } }
        public int Cp { get { return 2500; } }
        public int Pid { get { lock (gate) return worker == null ? 0 : worker.P.Id; } }
        public string Sid { get; private set; }
        public bool Ac { get; private set; }
        public bool Nd { get; private set; }
        public bool Ne { get; private set; }
        public int Np { get; private set; }
        public bool Fd { get; private set; }
        public int Mg { get; private set; }
        public long Mp { get { return Interlocked.Read(ref peak); } }
        public string Sd { get; private set; } = "";
        public int Td { get { CheckThread(); return disposed || stopped || !engine.Je || generation != engine.Jg || engine.At != page ? -1 : bridge.Timers.Td; } }
        public int Tn { get { CheckThread(); return bridge.Timers.N; } }

        public Jr Ex(string code) { return Run(code, false).GetAwaiter().GetResult(); }
        public Jr Tk() { return Run(null, false, timer: true).GetAwaiter().GetResult(); }
        public Task<Jr> TkA() { CheckContext(); return Run(null, true, timer: true); }

        public Task<Jr> ExA(string code)
        {
            CheckContext();
            return Run(code, true);
        }

        public Jr Ev(N target, string type)
        { Ja.EventType(type); return Run(null, false, target, type).GetAwaiter().GetResult(); }

        public Task<Jr> EvA(N target, string type)
        { CheckContext(); Ja.EventType(type); return Run(null, true, target, type); }

        private static void CheckContext()
        {
            var context = SynchronizationContext.Current;
            if (context == null || context.GetType() == typeof(SynchronizationContext))
                throw new InvalidOperationException("Use an owning UI synchronization context for asynchronous scripting.");
        }

        public void St()
        {
            stopped = true;
            lock (gate)
                if (worker != null && worker.J != null) { KeepPeak(worker.J.Peak); worker.J.Dispose(); }
        }

        private async Task<Jr> Run(string code, bool asynchronous, N target = null, string type = null, bool timer = false)
        {
            CheckThread();
            if (disposed) throw new ObjectDisposedException("Jp");
            if (running) throw new InvalidOperationException("A Lea worker script is already running.");
            bridge.Begin();
            if (generation != engine.Jg) { Reset(); generation = engine.Jg; }
            if (!engine.Je) { Reset(); return Result("off", "Scripting is disabled."); }
            if (!Object.ReferenceEquals(engine.At, page)) { Reset(); return Result("stale", "The bound page is no longer active."); }
            if (timer && stopped) { Reset(); return Result("cancel", "Worker execution stopped."); }
            if (!timer && type == null && (code == null || code.Length > 16 * 1024)) return Result("code", "Code must be at most 16 KiB.");
            if (type != null && target != null && !engine.Jn(page, target)) return Result("node", "Event target is not connected to the bound page.");
            if (sequence == Int32.MaxValue) { Reset(); sequence = 0; }
            var token = timer ? bridge.Timers.Take() : 0;
            if (timer && token == 0) return Result("ok", "");
            running = true;
            stopped = false;
            var started = false;
            try
            {
                if (worker == null) await Start(asynchronous);
                started = true;
                CheckState();
                var clock = Stopwatch.StartNew();
                var id = ++sequence;
                var endpoint = worker;
                var path = type == null ? null : bridge.Path(target);
                await Io(() =>
                {
                    if (timer) Wire.Write(endpoint.I, new { k = "timer", i = id, t = token });
                    else if (type == null) Wire.Write(endpoint.I, new { k = "exec", i = id, c = code });
                    else Wire.Write(endpoint.I, new { k = "event", i = id, t = type, p = path });
                    return true;
                }, clock, tm, asynchronous);
                var request = 0;
                while (true)
                {
                    var frame = await Io(() => Wire.Read(endpoint.O), clock, tm, asynchronous);
                    CheckThread();
                    CheckState();
                    if (clock.ElapsedMilliseconds >= tm) throw new TimeoutException();
                    var kind = Wire.Step(frame, id, ref request);
                    if (kind == "end")
                    {
                        var status = Wire.Text(frame, "s", 8);
                        var message = Wire.Text(frame, "m", 512);
                        if (status != "ok") Reset();
                        return Result(status, message, (bool)frame["d"]);
                    }
                    var value = bridge.Call((object[])frame["b"]);
                    if (clock.ElapsedMilliseconds >= tm) throw new TimeoutException();
                    await Io(() => { Wire.Write(endpoint.I, new { k = "reply", i = id, r = request, v = value }); return true; }, clock, tm, asynchronous);
                }
            }
            catch (Exception error) when (error is IOException || error is InvalidDataException || error is Win32Exception ||
                error is UnauthorizedAccessException || error is NotSupportedException ||
                error is TimeoutException || error is Ja.LimitException || error is StateException ||
                error is ArgumentException || error is InvalidOperationException || error is System.Security.SecurityException)
            {
                var state = error as StateException;
                var status = state == null ? stopped ? "cancel" :
                    error is TimeoutException ? "timeout" : error is Ja.LimitException ? "limit" :
                    !started ? "setup" : error is InvalidDataException ? "protocol" : "worker" : state.Status;
                var details = worker == null ? "" : worker.E;
                var message = status == "timeout" ? "Worker " + (started ? "execution" : "startup") + " deadline exceeded." :
                    status == "cancel" ? "Worker execution stopped." : error.Message +
                    (details.Length == 0 ? "" : " " + details);
                // A later lifecycle attempt can replace Sd before the host inspects this failure.
                if (!started && Sd.Length != 0) message += " " + Sd;
                Reset();
                return Result(status, message);
            }
            finally { if (timer) bridge.Timers.End(); running = false; }
        }

        private async Task Start(bool asynchronous)
        {
            Sd = "";
            Ac = Nd = Ne = Fd = false;
            Np = 0;
            Mg = 0;
            for (var folder = new DirectoryInfo(directory); folder != null; folder = folder.Parent)
                if ((folder.Attributes & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException("Worker runtime directory must not use reparse points.");
            var files = new List<string>();
            foreach (var name in Names)
            {
                var path = Path.Combine(directory, name);
                var file = new FileInfo(path);
                if (!file.Exists || file.Length < 1 || file.Length > 16 * 1024 * 1024 ||
                    (file.Attributes & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException("Worker runtime file is missing or invalid: " + name);
                leases.Add(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
                files.Add(path);
            }
            var clock = Stopwatch.StartNew();
            var phase = "launch";
            long launched = 0, probed = 0;
            Rp endpoint = null;
            try
            {
                // Keep leases and session ownership until native launch returns, even if Stop arrives meanwhile.
                if (asynchronous)
                    endpoint = await Task.Factory.StartNew(() => Launch(files),
                        CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                else endpoint = Launch(files);
                launched = clock.ElapsedMilliseconds;
                CheckState();
                Ac = endpoint.C && endpoint.N && endpoint.R && endpoint.L && endpoint.S && endpoint.M;
                Sid = endpoint.Sid;
                Ne = endpoint.B;
                var canary = Path.Combine(Path.GetTempPath(), "lea-script-canary-" + Guid.NewGuid().ToString("N") + ".txt");
                var listener = new TcpListener(IPAddress.Loopback, 0);
                phase = "probe";
                try
                {
                    File.WriteAllText(canary, Guid.NewGuid().ToString("N"));
                    listener.Start(1);
                    var port = ((IPEndPoint)listener.LocalEndpoint).Port;
                    using (var control = new TcpClient())
                    {
                        await Io(() => { control.Connect(IPAddress.Loopback, port); return true; }, clock, 5000, asynchronous);
                        using (await Io(() => listener.AcceptTcpClient(), clock, 5000, asynchronous)) { }
                    }
                    probed = clock.ElapsedMilliseconds;
                    phase = "hello";
                    await Io(() => { Wire.Write(endpoint.I, new { k = "init", p = port, f = canary }); return true; }, clock, 5000, asynchronous);
                    var hello = await Io(() => Wire.Read(endpoint.O), clock, 5000, asynchronous);
                    Wire.Fields(hello, "k", "g", "n", "f");
                    if (Wire.Text(hello, "k", 8) != "hello") throw new InvalidDataException("Missing worker handshake.");
                    Mg = Wire.Number(hello, "g");
                    Np = Wire.Number(hello, "n");
                    Nd = Ne && (Np == 1 || Np == 2) && !listener.Pending();
                    Fd = Wire.Number(hello, "f") == 1;
                    if (!Ac || Mg != 15 || !Nd || !Fd) throw new InvalidDataException("Worker isolation checks failed: token=" +
                        Ac + ", mitigations=" + Mg + ", network=" + Nd + ", file=" + Fd + ".");
                    phase = "ready";
                }
                finally { listener.Stop(); File.Delete(canary); }
            }
            finally
            {
                long cpu = 0;
                if (endpoint != null)
                    try { cpu = (long)endpoint.P.TotalProcessorTime.TotalMilliseconds; }
                    catch (InvalidOperationException) { }
                    catch (Win32Exception) { }
                var report = "phase=" + phase + "; launch=" + launched + "; probe=" +
                    Math.Max(0, probed - launched) + "; wall=" + clock.ElapsedMilliseconds + "; cpu=" + cpu + " ms";
                if (endpoint != null && endpoint.E.Length != 0) report += "; " + endpoint.E;
                Sd = report.Length <= 512 ? report : report.Substring(0, 512);
            }
        }

        private Rp Launch(List<string> files)
        {
            var endpoint = Rp.Open(files[0], "--pipe", directory, Mm, Cp, profile, files, true);
            lock (gate)
            {
                worker = endpoint;
                // Stop must retire a just-created child even while the owner context is not pumping.
                if (stopped) { KeepPeak(endpoint.J.Peak); endpoint.J.Dispose(); }
            }
            return endpoint;
        }

        private async Task<T> Io<T>(Func<T> action, Stopwatch clock, int limit, bool asynchronous)
        {
            var remaining = limit - (int)clock.ElapsedMilliseconds;
            if (remaining < 1) throw new TimeoutException();
            var task = Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            var observed = task.ContinueWith(failed => { var error = failed.Exception; }, CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            if (!asynchronous)
            {
                if (Task.WaitAny(new Task[] { task }, remaining) < 0) throw new TimeoutException();
                return task.GetAwaiter().GetResult();
            }
            using (var budget = new CancellationTokenSource())
            {
                if (await Task.WhenAny(task, Task.Delay(remaining, budget.Token)) != task) throw new TimeoutException();
                budget.Cancel();
                return await task;
            }
        }

        private void CheckState()
        {
            if (stopped) throw new StateException("cancel");
            if (!engine.Je || generation != engine.Jg) throw new StateException("off");
            if (!Object.ReferenceEquals(engine.At, page)) throw new StateException("stale");
        }

        private void KeepPeak(long value)
        {
            long prior;
            do { prior = Interlocked.Read(ref peak); if (value <= prior) return; }
            while (Interlocked.CompareExchange(ref peak, value, prior) != prior);
        }

        private void Reset()
        {
            Rp endpoint;
            lock (gate) { endpoint = worker; worker = null; }
            if (endpoint != null) { KeepPeak(endpoint.J.Peak); endpoint.Dispose(); }
            foreach (var lease in leases) lease.Dispose();
            leases.Clear();
            bridge.Reset();
        }

        private Jr Result(string status, string message, bool prevented = false)
        { return At = new Jr(status, bridge.Changes, message.Length <= 512 ? message : message.Substring(0, 512), prevented); }
        private void CheckThread()
        {
            if (Thread.CurrentThread.ManagedThreadId != thread)
                throw new InvalidOperationException("Use the Lea worker session on its owning thread.");
        }

        public void Dispose()
        {
            CheckThread();
            if (running) throw new InvalidOperationException("Stop and await the running script before disposing its session.");
            disposed = true;
            Reset();
        }

        private sealed class StateException : Exception
        { internal readonly string Status; internal StateException(string status) : base("Worker session state changed.") { Status = status; } }
    }
}
