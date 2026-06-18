using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

class MakeClassicGameIcon
{
    static void Main(string[] args)
    {
        if (args.Length < 2) throw new Exception("Usage: MakeClassicGameIcon source.png out.ico");
        int[] sizes = new int[] {16, 24, 32, 48, 64, 128, 256};
        byte[][] images = new byte[sizes.Length][];
        using (Image source = Image.FromFile(args[0]))
        {
            for (int i = 0; i < sizes.Length; i++) images[i] = MakeDib(source, sizes[i]);
        }
        using (FileStream fs = new FileStream(args[1], FileMode.Create, FileAccess.Write))
        using (BinaryWriter bw = new BinaryWriter(fs))
        {
            bw.Write((ushort)0);
            bw.Write((ushort)1);
            bw.Write((ushort)sizes.Length);
            int offset = 6 + sizes.Length * 16;
            for (int i = 0; i < sizes.Length; i++)
            {
                int size = sizes[i];
                bw.Write((byte)(size >= 256 ? 0 : size));
                bw.Write((byte)(size >= 256 ? 0 : size));
                bw.Write((byte)0);
                bw.Write((byte)0);
                bw.Write((ushort)1);
                bw.Write((ushort)32);
                bw.Write((uint)images[i].Length);
                bw.Write((uint)offset);
                offset += images[i].Length;
            }
            foreach (byte[] image in images) bw.Write(image);
        }
    }

    static byte[] MakeDib(Image source, int size)
    {
        using (Bitmap bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb))
        using (Graphics g = Graphics.FromImage(bmp))
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            g.Clear(Color.Transparent);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.DrawImage(source, new Rectangle(0, 0, size, size));

            bw.Write((uint)40);
            bw.Write((int)size);
            bw.Write((int)(size * 2));
            bw.Write((ushort)1);
            bw.Write((ushort)32);
            bw.Write((uint)0);
            bw.Write((uint)(size * size * 4));
            bw.Write((int)0);
            bw.Write((int)0);
            bw.Write((uint)0);
            bw.Write((uint)0);

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, size, size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                int stride = data.Stride;
                byte[] row = new byte[Math.Abs(stride)];
                for (int y = size - 1; y >= 0; y--)
                {
                    IntPtr src = IntPtr.Add(data.Scan0, y * stride);
                    Marshal.Copy(src, row, 0, size * 4);
                    bw.Write(row, 0, size * 4);
                }
            }
            finally
            {
                bmp.UnlockBits(data);
            }

            int maskStride = ((size + 31) / 32) * 4;
            byte[] mask = new byte[maskStride * size];
            bw.Write(mask);
            return ms.ToArray();
        }
    }
}