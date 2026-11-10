using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Lea.Print
{
    // Only already-decoded pixels enter this boundary; never a URL or encoded input.
    public sealed class Pi
    {
        public const int MaxAxis = 2048, MaxPixels = 1048576, MaxBytes = 4 * 1024 * 1024;
        public int Width { get; private set; }
        public int Height { get; private set; }
        internal string Data { get; private set; }
        internal int Bytes { get; private set; }
        internal byte[] Pixels { get; private set; }
        private Pi() { }
        public static Pi Cp(Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException("bitmap");
            int width = bitmap.Width, height = bitmap.Height;
            if (width < 1 || height < 1 || width > MaxAxis || height > MaxAxis || (long)width * height > MaxPixels)
                throw new ArgumentException("Print image pixel limit");
            using (var clean = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            using (var stream = new BoundedStream())
            {
                using (var graphics = Graphics.FromImage(clean))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.DrawImageUnscaled(bitmap, 0, 0);
                }
                clean.Save(stream, ImageFormat.Png);
                var pixels = new byte[width * height * 4];
                var locked = clean.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try { for (int row = 0; row < height; row++) Marshal.Copy(IntPtr.Add(locked.Scan0, row * locked.Stride), pixels, row * width * 4, width * 4); }
                finally { clean.UnlockBits(locked); }
                return new Pi { Width = width, Height = height, Bytes = checked((int)stream.Length),
                    Pixels = pixels,
                    Data = "data:image/png;base64," + Convert.ToBase64String(stream.GetBuffer(), 0, checked((int)stream.Length)) };
            }
        }
        private sealed class BoundedStream : MemoryStream
        {
            public override void Write(byte[] buffer, int offset, int count)
            { if (count < 0 || Position > MaxBytes - count) throw new InvalidOperationException("Print PNG byte limit"); base.Write(buffer, offset, count); }
            public override void WriteByte(byte value)
            { if (Position >= MaxBytes) throw new InvalidOperationException("Print PNG byte limit"); base.WriteByte(value); }
            public override void SetLength(long value)
            { if (value < 0 || value > MaxBytes) throw new InvalidOperationException("Print PNG byte limit"); base.SetLength(value); }
        }
    }
}
