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
        private static readonly SemaphoreSlim one = new SemaphoreSlim(1, 1);
        private static readonly object state = new object();
        private static Process worker;
        private static BinaryWriter input;
        private static BinaryReader output;
        private static string workerKey = "";

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
            Action<int> started, Action killed)
        {
            return Task.Factory.StartNew(() => Read(data, path, args, token, started, killed),
                CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        private static Image Read(byte[] data, string path, string args, CancellationToken token,
            Action<int> started, Action killed)
        {
            one.Wait(token);
            Process current = null;
            try
            {
                token.ThrowIfCancellationRequested();
                current = Open(path, args);
                if (started != null) started(current.Id);
                using (token.Register(() =>
                {
                    if (Stop(current) && killed != null) killed();
                }))
                {
                    input.Write(data.Length);
                    input.Write(data);
                    input.Flush();
                    var magic = output.ReadInt32();
                    var status = output.ReadByte();
                    if (magic != Magic) throw new InvalidDataException("Image worker protocol marker is invalid.");
                    if (status == 0)
                    {
                        var length = output.ReadInt32();
                        if (length < 0 || length > 1024) throw new InvalidDataException("Image worker error is invalid.");
                        throw new InvalidDataException(Encoding.UTF8.GetString(Bytes(output, length)));
                    }
                    if (status != 1) throw new InvalidDataException("Image worker response kind is invalid.");
                    var width = output.ReadInt32();
                    var height = output.ReadInt32();
                    var lengthPixels = output.ReadInt32();
                    var pixels = (long)width * height;
                    if (width < 1 || height < 1 || width > Pi.Axis || height > Pi.Axis || pixels > Pi.Pixels ||
                        lengthPixels != pixels * 4)
                        throw new InvalidDataException("Image worker dimensions exceed Lea's protocol limit.");
                    var bytes = Bytes(output, lengthPixels);
                    token.ThrowIfCancellationRequested();
                    return Bitmap(width, height, bytes);
                }
            }
            catch
            {
                if (current != null) Stop(current);
                throw;
            }
            finally { one.Release(); }
        }

        private static Process Open(string path, string args)
        {
            var full = P(path);
            var key = full + "\0" + (args ?? "");
            lock (state)
            {
                if (worker != null)
                {
                    try { if (!worker.HasExited && workerKey == key) return worker; }
                    catch (InvalidOperationException) { }
                    Drop();
                }
                if (!File.Exists(full)) throw new FileNotFoundException("Lea image worker was not found.", full);
                var start = new ProcessStartInfo(full, args ?? "--pipe")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetDirectoryName(full)
                };
                worker = Process.Start(start);
                if (worker == null) throw new InvalidOperationException("Lea image worker did not start.");
                input = new BinaryWriter(worker.StandardInput.BaseStream, Encoding.UTF8);
                output = new BinaryReader(worker.StandardOutput.BaseStream, Encoding.UTF8);
                workerKey = key;
                return worker;
            }
        }

        private static bool Stop(Process expected)
        {
            lock (state)
            {
                if (!Object.ReferenceEquals(worker, expected)) return false;
                var running = false;
                try
                {
                    running = !worker.HasExited;
                    if (running) worker.Kill();
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
