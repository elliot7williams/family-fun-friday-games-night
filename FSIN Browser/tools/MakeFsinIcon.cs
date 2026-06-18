using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

class MakeFsinIcon
{
    static void Main(string[] args)
    {
        string outDir = args[0];
        Directory.CreateDirectory(outDir);
        int[] sizes = new int[] {16, 32, 48, 64, 128, 192, 256, 512};
        foreach (int size in sizes)
        {
            using (Bitmap bmp = DrawIcon(size)) bmp.Save(Path.Combine(outDir, "fsin-icon-" + size + ".png"), ImageFormat.Png);
        }
        WriteClassicIco(Path.Combine(outDir, "fsin.ico"), sizes);
    }

    static Bitmap DrawIcon(int size)
    {
        Bitmap bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.Transparent);
            using (LinearGradientBrush bg = new LinearGradientBrush(new Rectangle(0,0,size,size), Color.FromArgb(5,18,30), Color.FromArgb(26,92,128), 135f))
            {
                g.FillEllipse(bg, 0, 0, size - 1, size - 1);
            }
            using (Pen ring = new Pen(Color.FromArgb(125,228,255), Math.Max(2, size / 15f)))
            {
                g.DrawEllipse(ring, size * .12f, size * .12f, size * .76f, size * .76f);
            }
            using (Font font = new Font(FontFamily.GenericSansSerif, size * .31f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                g.DrawString("FS", font, brush, new RectangleF(0, size * .04f, size, size * .58f), format);
                using (Font small = new Font(FontFamily.GenericSansSerif, size * .16f, FontStyle.Bold, GraphicsUnit.Pixel))
                {
                    g.DrawString("IN", small, brush, new RectangleF(0, size * .52f, size, size * .28f), format);
                }
            }
        }
        return bmp;
    }

    static void WriteClassicIco(string path, int[] sizes)
    {
        byte[][] images = new byte[sizes.Length][];
        for (int i = 0; i < sizes.Length; i++) using (Bitmap bmp = DrawIcon(sizes[i])) images[i] = MakeDib(bmp, sizes[i]);
        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
        using (BinaryWriter bw = new BinaryWriter(fs))
        {
            bw.Write((ushort)0); bw.Write((ushort)1); bw.Write((ushort)sizes.Length);
            int offset = 6 + sizes.Length * 16;
            for (int i = 0; i < sizes.Length; i++)
            {
                int size = sizes[i];
                bw.Write((byte)(size >= 256 ? 0 : size)); bw.Write((byte)(size >= 256 ? 0 : size)); bw.Write((byte)0); bw.Write((byte)0);
                bw.Write((ushort)1); bw.Write((ushort)32); bw.Write((uint)images[i].Length); bw.Write((uint)offset);
                offset += images[i].Length;
            }
            foreach (byte[] image in images) bw.Write(image);
        }
    }

    static byte[] MakeDib(Bitmap bmp, int size)
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write((uint)40); bw.Write((int)size); bw.Write((int)(size * 2)); bw.Write((ushort)1); bw.Write((ushort)32);
            bw.Write((uint)0); bw.Write((uint)(size * size * 4)); bw.Write((int)0); bw.Write((int)0); bw.Write((uint)0); bw.Write((uint)0);
            BitmapData data = bmp.LockBits(new Rectangle(0,0,size,size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                byte[] row = new byte[size * 4];
                for (int y = size - 1; y >= 0; y--)
                {
                    Marshal.Copy(IntPtr.Add(data.Scan0, y * data.Stride), row, 0, row.Length);
                    bw.Write(row);
                }
            }
            finally { bmp.UnlockBits(data); }
            bw.Write(new byte[((size + 31) / 32) * 4 * size]);
            return ms.ToArray();
        }
    }
}