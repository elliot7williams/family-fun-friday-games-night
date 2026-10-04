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
        private static readonly object state = new object();
        private static long workerMemory = 192L * 1024 * 1024;
        private static int workerCpu = 2500;
        private static Rp worker;
        private static BinaryWriter input;
        private static BinaryReader output;
        private static string workerKey = "";

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

        public static Task<Image> Rd(byte[] data, string path, string args, CancellationToken token,
            long reserve, long limit, Func<long> hostMemory,
            Action<int, bool, bool, bool, bool, bool, bool, bool, bool, bool, int, int> started,
            Action<long, long, long, long, bool> admission,
            Action<long, long, long, long> measured, Action killed, Action<string> diagnostics)
        {
            return Task.Factory.StartNew(() => Read(data, path, args, token, reserve, limit, hostMemory,
                started, admission, measured, killed, diagnostics),
                token, TaskCreationOptions.None, TaskScheduler.Default);
        }

        private static Image Read(byte[] data, string path, string args, CancellationToken token,
            long reserve, long limit, Func<long> hostMemory,
            Action<int, bool, bool, bool, bool, bool, bool, bool, bool, bool, int, int> started,
            Action<long, long, long, long, bool> admission,
            Action<long, long, long, long> measured, Action killed, Action<string> diagnostics)
        {
            Rp current = null;
            var held = false;
            var processId = 0;
            var phase = "queue";
            var result = "ok";
            var kind = "";
            var clock = Stopwatch.StartNew();
            var steps = diagnostics == null ? null : new StringBuilder();
            Action<string> mark = value =>
            {
                phase = value;
                if (steps != null) steps.Append(value).Append('=').Append(clock.ElapsedMilliseconds).Append(';');
            };
            try
            {
                mark("queue");
                one.Wait(token);
                held = true;
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
                    if (Stop(current) && killed != null) killed();
                }))
                {
                    token.ThrowIfCancellationRequested();
                    if (!current.Ready)
                    {
                        mark("hello");
                        var hello = reader.ReadInt32();
                        var guards = reader.ReadInt32();
                        if (hello != Hello || (guards & RequiredGuards) != RequiredGuards)
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
                if (held) one.Release();
            }
        }

        private static Rp Open(string path, string args)
        {
            var full = Rp.Prepare(P(path));
            var limit = L;
            var cpu = C;
            var key = full + "\0" + (args ?? "") + "\0" + limit + "\0" + cpu;
            lock (state)
            {
                if (worker != null)
                {
                    try { if (!worker.P.HasExited && workerKey == key && worker.J != null) return worker; }
                    catch (InvalidOperationException) { }
                    Drop();
                }
                if (!File.Exists(full)) throw new FileNotFoundException("Lea image worker was not found.", full);
                Rp created = null;
                BinaryWriter writer = null;
                BinaryReader reader = null;
                try
                {
                    created = Rp.Open(full, args ?? "--pipe", Path.GetDirectoryName(full), limit, cpu);
                    writer = new BinaryWriter(created.I, Encoding.UTF8);
                    reader = new BinaryReader(created.O, Encoding.UTF8);
                    worker = created;
                    input = writer;
                    output = reader;
                    workerKey = key;
                    return worker;
                }
                catch
                {
                    if (writer != null) try { writer.Dispose(); } catch (IOException) { }
                    if (reader != null) try { reader.Dispose(); } catch (IOException) { }
                    if (created != null) created.Dispose();
                    throw;
                }
            }
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
            lock (state)
            {
                if (!Object.ReferenceEquals(worker, expected)) return false;
                var running = false;
                try
                {
                    running = !worker.P.HasExited;
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
            if (worker != null) worker.Dispose();
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
