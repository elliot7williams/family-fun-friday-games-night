using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Lea.Image
{
    internal static class Program
    {
        private const int Magic = 0x3141454c;
        private const int InputLimit = 4 * 1024 * 1024;
        private const int AxisLimit = 8192;
        private const long PixelLimit = 8L * 1024 * 1024;

        private static int Main(string[] args)
        {
            if (args.Length == 0 || args[0] != "--pipe") return 2;
            var delay = 0;
            foreach (var arg in args)
                if (arg.StartsWith("--delay=", StringComparison.Ordinal) &&
                    (!Int32.TryParse(arg.Substring(8), out delay) || delay < 0 || delay > 60000)) return 2;
            try
            {
                using (var input = Console.OpenStandardInput())
                using (var output = Console.OpenStandardOutput())
                using (var reader = new BinaryReader(input, Encoding.UTF8))
                using (var writer = new BinaryWriter(output, Encoding.UTF8))
                {
                    while (true)
                    {
                        int length;
                        try { length = reader.ReadInt32(); }
                        catch (EndOfStreamException) { return 0; }
                        if (length == 0) return 0;
                        if (length < 1 || length > InputLimit) return 3;
                        var data = Read(reader, length);
                        if (delay > 0) Thread.Sleep(delay);
                        try { Decode(data, writer); }
                        catch (Exception error)
                        {
                            Failure(writer, error.Message);
                            return 1;
                        }
                        writer.Flush();
                    }
                }
            }
            catch (Exception error)
            {
                try { Console.Error.WriteLine(error.GetType().Name + ": " + error.Message); }
                catch { }
                return 4;
            }
        }

        private static void Decode(byte[] data, BinaryWriter writer)
        {
            using (var stream = new MemoryStream(data, false))
            using (var source = System.Drawing.Image.FromStream(stream, false, true))
            {
                var width = source.Width;
                var height = source.Height;
                var pixels = (long)width * height;
                if (width < 1 || height < 1 || width > AxisLimit || height > AxisLimit || pixels > PixelLimit)
                    throw new InvalidDataException("Decoded dimensions exceed Lea's image limit.");
                using (var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.CompositingMode = CompositingMode.SourceCopy;
                        graphics.DrawImage(source, new Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel);
                    }
                    writer.Write(Magic);
                    writer.Write((byte)1);
                    writer.Write(width);
                    writer.Write(height);
                    writer.Write(checked((int)(pixels * 4)));
                    var area = new Rectangle(0, 0, width, height);
                    var locked = bitmap.LockBits(area, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    try
                    {
                        var row = new byte[checked(width * 4)];
                        for (var y = 0; y < height; y++)
                        {
                            var at = locked.Stride >= 0 ? IntPtr.Add(locked.Scan0, y * locked.Stride) :
                                IntPtr.Add(locked.Scan0, (height - 1 - y) * -locked.Stride);
                            Marshal.Copy(at, row, 0, row.Length);
                            writer.Write(row);
                        }
                    }
                    finally { bitmap.UnlockBits(locked); }
                }
            }
        }

        private static byte[] Read(BinaryReader reader, int length)
        {
            var data = new byte[length];
            var at = 0;
            while (at < data.Length)
            {
                var count = reader.Read(data, at, data.Length - at);
                if (count == 0) throw new EndOfStreamException("Image request ended early.");
                at += count;
            }
            return data;
        }

        private static void Failure(BinaryWriter writer, string message)
        {
            var data = Encoding.UTF8.GetBytes(message ?? "Image decode failed.");
            if (data.Length > 1024) Array.Resize(ref data, 1024);
            writer.Write(Magic);
            writer.Write((byte)0);
            writer.Write(data.Length);
            writer.Write(data);
            writer.Flush();
        }
    }
}
