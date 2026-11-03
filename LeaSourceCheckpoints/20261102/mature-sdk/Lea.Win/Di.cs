using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lea.Win
{
    // Di owns Lea's single reusable, killable native image decoder process.
    internal static class Di
    {
        private const int Magic = 0x3141454c;
        private const int Hello = 0x3241454c;
        private const int RequiredGuards = 3;
        private const long MinMemory = 64L * 1024 * 1024;
        private const long MaxMemory = 512L * 1024 * 1024;
        private const int MinCpu = 100;
        private const int MaxCpu = 10000;
        private static readonly SemaphoreSlim one = new SemaphoreSlim(1, 1);
        private static readonly DecodeQueue decodeQueue = new DecodeQueue(one);
        private static readonly object state = new object();
        private static long workerMemory = 192L * 1024 * 1024;
        private static int workerCpu = 2500;
        private static Rp worker;
        private static BinaryWriter input;
        private static BinaryReader output;
        private static string workerKey = "";
        private static int shuttingDown;

        static Di()
        {
            AppDomain.CurrentDomain.ProcessExit += OnShutdown;
            AppDomain.CurrentDomain.DomainUnload += OnShutdown;
        }

        private static void OnShutdown(object sender, EventArgs args)
        {
            if (!Shutdown())
                try { Console.Error.WriteLine("LEA_DECODER_SHUTDOWN pending"); }
                catch (IOException) { }
        }

        internal static bool Shutdown()
        {
            Interlocked.Exchange(ref shuttingDown, 1);
            if (!Monitor.TryEnter(state, 1000)) return false;
            try
            {
                Drop();
                return true;
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException ||
                error is InvalidOperationException || error is System.ComponentModel.Win32Exception ||
                error is System.Security.SecurityException)
            { return false; }
            finally { Monitor.Exit(state); }
        }

        public static long L
        {
            get { return Interlocked.Read(ref workerMemory); }
            set { Interlocked.Exchange(ref workerMemory, Math.Max(MinMemory, Math.Min(MaxMemory, value))); }
        }

        public static int C
        {
            get { return Interlocked.CompareExchange(ref workerCpu, 0, 0); }
            set { Interlocked.Exchange(ref workerCpu, Math.Max(MinCpu, Math.Min(MaxCpu, value))); }
        }

        public static string P(string configured)
        {
            if (!String.IsNullOrWhiteSpace(configured))
                try { return Path.GetFullPath(configured); }
                catch (Exception) { return configured; }
            var name = "Lea.Image.exe";
            var app = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name);
            if (File.Exists(app)) return app;
            var assembly = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", name);
            if (File.Exists(assembly)) return assembly;
            var development = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "",
                "..", "..", "..", "Lea.Image", "bin", "Release", name));
            return File.Exists(development) ? development : app;
        }

        private sealed class DecoderOwner
        {
            internal readonly long Id;
            internal readonly string Args;
            internal readonly CancellationToken Token;
            internal readonly Stopwatch Clock = Stopwatch.StartNew();
            internal string Phase = "entry";
            internal string Trace = "";
            internal DecoderOwner(string args, CancellationToken token)
            {
                Id = Interlocked.Increment(ref ownerSequence);
                Args = args == "--image-fake" ? "fake" : args == "--pipe --delay=6000" ? "delayed" : "other";
                Token = token;
            }
        }
        private static long ownerSequence;
        private static DecoderOwner activeOwner;
        private static void MarkOwner(string phase)
        {
            var owner = Interlocked.CompareExchange(ref activeOwner, null, null);
            if (owner != null)
            {
                Interlocked.Exchange(ref owner.Phase, phase);
                var entry = phase + "=" + owner.Clock.ElapsedMilliseconds + ";";
                string previous;
                do
                {
                    previous = Interlocked.CompareExchange(ref owner.Trace, null, null);
                    if (previous.Length + entry.Length > 180) break;
                } while (Interlocked.CompareExchange(ref owner.Trace, previous + entry, previous) != previous);
            }
        }
        private static string OwnerSnapshot()
        {
            var owner = Interlocked.CompareExchange(ref activeOwner, null, null);
            return owner == null ? "; owner=none" : "; owner=" + owner.Id +
                ":" + owner.Args + ":" + Interlocked.CompareExchange(ref owner.Phase, null, null) +
                ":age=" + owner.Clock.ElapsedMilliseconds + ":s=" + Interlocked.CompareExchange(ref owner.Trace, null, null) + ":cancel=" + (owner.Token.IsCancellationRequested ? "1" : "0");
        }

        public static async Task<Image> Rd(byte[] data, string path, string args, CancellationToken token,
            long reserve, long limit, Func<long> hostMemory,
            Action<int, bool, bool, bool, bool, bool, bool, bool, bool, bool, int, int> started,
            Action<long, long, long, long, bool> admission,
            Action<long, long, long, long> measured, Action killed, Action<string> diagnostics)
        {
            var entered = false;
            var canceledAtEntry = token.IsCancellationRequested;
            var clock = Stopwatch.StartNew();
            string queuePhase = "wait";
            long admittedAt = -1, dispatchAt = -1, threadAt = -1;
            Action<string> observe = diagnostics == null ? null : new Action<string>(phase =>
            {
                var elapsed = clock.ElapsedMilliseconds;
                if (phase == "admit") Interlocked.Exchange(ref admittedAt, elapsed);
                else if (phase == "dispatch") Interlocked.Exchange(ref dispatchAt, elapsed);
                else if (phase == "thread") Interlocked.Exchange(ref threadAt, elapsed);
                Interlocked.Exchange(ref queuePhase, phase);
            });
            Action<string> report = diagnostics == null ? null : new Action<string>(text =>
            {
                var detail = "; queue=" + Interlocked.CompareExchange(ref queuePhase, null, null) +
                    "; admitMs=" + Interlocked.Read(ref admittedAt) + "; dispatchMs=" + Interlocked.Read(ref dispatchAt) +
                    "; threadMs=" + Interlocked.Read(ref threadAt);
                detail += OwnerSnapshot();
                if (detail.Length > 448) detail = detail.Substring(0, 448);
                var message = (text.Length > 512 - detail.Length ? text.Substring(0, 512 - detail.Length) : text) + detail;
                diagnostics(message.Length <= 512 ? message : message.Substring(0, 512));
            });
            try
            {
                return await decodeQueue.RunObserved(() =>
                {
                    entered = true;
                    var owner = new DecoderOwner(args, token);
                    Interlocked.Exchange(ref activeOwner, owner);
                    try
                    {
                        return Read(data, path, args, token, reserve, limit, hostMemory,
                            started, admission, measured, killed, report);
                    }
                    finally { Interlocked.CompareExchange(ref activeOwner, null, owner); }
                }, token, observe).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                if (!entered && !canceledAtEntry && report != null)
                    try { report("phase=queue; result=cancel; worker=0; wall=" + clock.ElapsedMilliseconds +
                        "; ready=0; kind=OperationCanceledException; steps=queue=0;"); }
                    catch (Exception) { }
                throw;
            }
        }

        internal sealed class DecodeQueue
        {
            private readonly SemaphoreSlim gate;

            internal DecodeQueue() : this(new SemaphoreSlim(1, 1)) { }
            internal DecodeQueue(SemaphoreSlim gate)
            { this.gate = gate ?? throw new ArgumentNullException(nameof(gate)); }

            internal Task<T> Run<T>(Func<T> decode, CancellationToken token)
            { return RunObserved(decode, token, null); }

            private static void Observe(Action<string> observer, string phase)
            {
                if (observer != null) try { observer(phase); } catch (Exception) { }
            }

            internal async Task<T> RunObserved<T>(Func<T> decode, CancellationToken token, Action<string> observer)
            {
                Observe(observer, "wait");
                await gate.WaitAsync(token).ConfigureAwait(false);
                Task<T> work;
                try
                {
                    Observe(observer, "admit");
                    token.ThrowIfCancellationRequested();
                    Observe(observer, "dispatch");
                    work = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            Observe(observer, "thread");
                            token.ThrowIfCancellationRequested();
                            return decode();
                        }
                        finally { gate.Release(); }
                    }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                }
                catch { gate.Release(); throw; }
                return await work.ConfigureAwait(false);
            }
        }

        private static Image Read(byte[] data, string path, string args, CancellationToken token,
            long reserve, long limit, Func<long> hostMemory,
            Action<int, bool, bool, bool, bool, bool, bool, bool, bool, bool, int, int> started,
            Action<long, long, long, long, bool> admission,
            Action<long, long, long, long> measured, Action killed, Action<string> diagnostics)
        {
            Rp current = null;
            var processId = 0;
            var phase = "queue";
            var result = "ok";
            var kind = "";
            var clock = Stopwatch.StartNew();
            var steps = diagnostics == null ? null : new StringBuilder();
            Action<string> mark = value =>
            {
                phase = value;
                MarkOwner(value);
                if (steps != null) steps.Append(value).Append('=').Append(clock.ElapsedMilliseconds).Append(';');
            };
            try
            {
                mark("queue");
                token.ThrowIfCancellationRequested();
                mark("launch");
                current = Open(path, args);
                var process = current.P;
                var job = current.J;
                processId = process.Id;
                var reader = output;
                var writer = input;
                using (token.Register(() =>
                {
                    MarkOwner("stop" );
                    if (Stop(current) && killed != null) killed();
                    MarkOwner("stopped" );
                }))
                {
                    token.ThrowIfCancellationRequested();
                    if (!current.Ready)
                    {
                        mark("hello");
                        var hello = reader.ReadInt32();
                        if (hello != Hello)
                            throw new InvalidDataException("Image worker hardening handshake is invalid.");
                        mark("guards");
                        var guards = reader.ReadInt32();
                        if ((guards & RequiredGuards) != RequiredGuards)
                            throw new InvalidDataException("Image worker hardening handshake is invalid.");
                        current.G = guards;
                        current.Ready = true;
                    }
                    token.ThrowIfCancellationRequested();
                    mark("host");
                    var host = Host(hostMemory);
                    token.ThrowIfCancellationRequested();
                    mark("admission");
                    var memory = Private(process);
                    var peak = job == null ? 0 : job.Peak;
                    var combined = Total(host, Math.Max(memory, peak), reserve);
                    var admitted = host > 0 && memory > 0 && peak > 0 && reserve > 0 && limit > 0 && combined <= limit;
                    if (admission != null) admission(host, memory, peak, combined, admitted);
                    if (!admitted)
                        throw new InvalidDataException("Image decode exceeds Lea's combined host/helper memory limit.");
                    token.ThrowIfCancellationRequested();
                    mark("start");
                    if (started != null) started(processId, job != null, current.R, current.L,
                        current.S, current.A, current.M, current.C, current.N, current.F,
                        job == null ? 0 : job.C, current.G);
                    token.ThrowIfCancellationRequested();
                    mark("request");
                    writer.Write(data.Length);
                    writer.Write(data);
                    writer.Flush();
                    mark("response");
                    var magic = reader.ReadInt32();
                    var status = reader.ReadByte();
                    if (magic != Magic) throw new InvalidDataException("Image worker protocol marker is invalid.");
                    if (status == 0)
                    {
                        var length = reader.ReadInt32();
                        if (length < 0 || length > 1024) throw new InvalidDataException("Image worker error is invalid.");
                        throw new InvalidDataException(Encoding.UTF8.GetString(Bytes(reader, length)));
                    }
                    if (status != 1) throw new InvalidDataException("Image worker response kind is invalid.");
                    var width = reader.ReadInt32();
                    var height = reader.ReadInt32();
                    var lengthPixels = reader.ReadInt32();
                    var pixels = (long)width * height;
                    if (width < 1 || height < 1 || width > Pi.Axis || height > Pi.Axis || pixels > Pi.Pixels ||
                        lengthPixels != pixels * 4)
                        throw new InvalidDataException("Image worker dimensions exceed Lea's protocol limit.");
                    var bytes = Bytes(reader, lengthPixels);
                    token.ThrowIfCancellationRequested();
                    mark("bitmap");
                    var image = Bitmap(width, height, bytes);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        host = Host(hostMemory);
                        memory = Private(process);
                        peak = job == null ? 0 : job.Peak;
                        combined = Total(host, Math.Max(memory, peak), 0);
                        if (measured != null) measured(host, memory, peak, combined);
                        token.ThrowIfCancellationRequested();
                        mark("done");
                        return image;
                    }
                    catch
                    {
                        image.Dispose();
                        throw;
                    }
                }
            }
            catch (Exception error)
            {
                result = token.IsCancellationRequested ? "cancel" : "error";
                kind = error.GetType().Name;
                if (kind.Length > 80) kind = kind.Substring(0, 80);
                if (current != null) Stop(current);
                token.ThrowIfCancellationRequested();
                throw;
            }
            finally
            {
                if (diagnostics != null)
                {
                    // Diagnostic consumers cannot change decoder results or gate cleanup.
                    try
                    {
                        var text = "phase=" + phase + "; result=" + result + "; worker=" + processId +
                            "; wall=" + clock.ElapsedMilliseconds + "; ready=" + (current != null && current.Ready ? "1" : "0") +
                            "; kind=" + kind + "; steps=" + steps;
                        diagnostics(text.Length <= 512 ? text : text.Substring(0, 512));
                    }
                    catch (Exception) { }
                }
            }
        }

        private static Rp Open(string path, string args)
        {
            MarkOwner("prep" );
            var full = Rp.Prepare(P(path));
            MarkOwner("prepared" );
            var limit = L;
            var cpu = C;
            var key = full + "\0" + (args ?? "") + "\0" + limit + "\0" + cpu;
            MarkOwner("lock" );
            lock (state)
            {
                if (Interlocked.CompareExchange(ref shuttingDown, 0, 0) != 0) throw new InvalidOperationException("Image decoder is shutting down.");
                if (worker != null)
                {
                    try { if (worker.P != null && !worker.P.HasExited && workerKey == key && worker.J != null) return worker; }
                    catch (InvalidOperationException) { }
                    MarkOwner("drop" );
                    Drop();
                    MarkOwner("dropped" );
                }
                if (!File.Exists(full)) throw new FileNotFoundException("Lea image worker was not found.", full);
                Rp created = null;
                BinaryWriter writer = null;
                BinaryReader reader = null;
                try
                {
                    var observer = Rp.StageObserver;
                    try { Rp.StageObserver = MarkOwner; created = Rp.Open(full, args ?? "--pipe", Path.GetDirectoryName(full), limit, cpu); }
                    finally { Rp.StageObserver = observer; }
                    writer = new BinaryWriter(created.I, Encoding.UTF8);
                    reader = new BinaryReader(created.O, Encoding.UTF8);
                    worker = created;
                    input = writer;
                    output = reader;
                    workerKey = key;
                    return worker;
                }
                catch (Exception error)
                {
                    if (writer != null) try { writer.Dispose(); } catch (IOException) { }
                    if (reader != null) try { reader.Dispose(); } catch (IOException) { }
                    RetireOpenFailure(created, error);
                    throw;
                }
            }
        }

        private static void RetireFailedOpen(Rp created)
        {
            if (created == null) return;
            // Publish ownership before cleanup, which may fail or remain pending.
            worker = created;
            workerKey = "";
            Drop();
        }

        private static void RetireOpenFailure(Rp created, Exception cause)
        {
            var failed = cause as Rp.SetupFailure;
            try { RetireFailedOpen(created ?? (failed == null ? null : failed.Endpoint)); }
            catch (Exception cleanup) when (cleanup is IOException || cleanup is UnauthorizedAccessException ||
                cleanup is InvalidOperationException || cleanup is System.ComponentModel.Win32Exception ||
                cleanup is System.Security.SecurityException)
            { throw new Rp.SetupFailure(new AggregateException(cause, cleanup), worker); }
        }

        private static long Private(Process process)
        {
            try { process.Refresh(); return process.PrivateMemorySize64; }
            catch (InvalidOperationException) { return 0; }
            catch (System.ComponentModel.Win32Exception) { return 0; }
        }

        private static long Host(Func<long> source)
        {
            if (source == null) return 0;
            try { return Math.Max(0, source()); }
            catch (Exception) { return 0; }
        }

        private static long Total(long first, long second, long third)
        {
            if (first < 0 || second < 0 || third < 0) return Int64.MaxValue;
            if (first > Int64.MaxValue - second) return Int64.MaxValue;
            var total = first + second;
            return total > Int64.MaxValue - third ? Int64.MaxValue : total + third;
        }

        private static bool Stop(Rp expected)
        {
            MarkOwner("lock" );
            lock (state)
            {
                if (!Object.ReferenceEquals(worker, expected)) return false;
                var running = false;
                try
                {
                    running = worker.P != null && !worker.P.HasExited;
                    if (running) worker.P.Kill();
                }
                catch (InvalidOperationException) { }
                catch (System.ComponentModel.Win32Exception) { }
                Drop();
                return running;
            }
        }

        private static void Drop()
        {
            if (input != null) try { input.Dispose(); } catch (IOException) { }
            if (output != null) try { output.Dispose(); } catch (IOException) { }
            if (worker != null) { worker.Dispose(); if (worker.P != null || worker.CleanupPending) throw new IOException("Image-worker retirement remains pending."); }
            input = null;
            output = null;
            worker = null;
            workerKey = "";
        }

        private static byte[] Bytes(BinaryReader reader, int length)
        {
            var bytes = new byte[length];
            var at = 0;
            while (at < bytes.Length)
            {
                var count = reader.Read(bytes, at, bytes.Length - at);
                if (count == 0) throw new EndOfStreamException("Image worker response ended early.");
                at += count;
            }
            return bytes;
        }

        private static Image Bitmap(int width, int height, byte[] bytes)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            try
            {
                var locked = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                try
                {
                    var row = checked(width * 4);
                    for (var y = 0; y < height; y++)
                    {
                        var at = locked.Stride >= 0 ? IntPtr.Add(locked.Scan0, y * locked.Stride) :
                            IntPtr.Add(locked.Scan0, (height - 1 - y) * -locked.Stride);
                        Marshal.Copy(bytes, y * row, at, row);
                    }
                }
                finally { bitmap.UnlockBits(locked); }
                return bitmap;
            }
            catch
            {
                bitmap.Dispose();
                throw;
            }
        }
    }
}
