using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;

class MakeGameIcons
{
    static void Main(string[] args)
    {
        if (args.Length < 2) throw new Exception("Usage: MakeGameIcons source.png outputDir");
        string source = args[0];
        string outputDir = args[1];
        Directory.CreateDirectory(outputDir);
        int[] sizes = new int[] {16, 32, 48, 64, 128, 192, 256, 512};
        List<byte[]> pngs = new List<byte[]>();
        using (Image img = Image.FromFile(source))
        {
            foreach (int size in sizes)
            {
                using (Bitmap bmp = new Bitmap(size, size))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.DrawImage(img, new Rectangle(0, 0, size, size));
                    string pngPath = Path.Combine(outputDir, "games-night-icon-" + size + ".png");
                    bmp.Save(pngPath, ImageFormat.Png);
                    if (size == 16 || size == 32 || size == 48 || size == 256)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            bmp.Save(ms, ImageFormat.Png);
                            pngs.Add(ms.ToArray());
                        }
                    }
                }
            }
        }
        WriteIco(Path.Combine(outputDir, "favicon.ico"), pngs, new int[] {16, 32, 48, 256});
    }

    static void WriteIco(string path, List<byte[]> images, int[] sizes)
    {
        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
        using (BinaryWriter bw = new BinaryWriter(fs))
        {
            bw.Write((ushort)0);
            bw.Write((ushort)1);
            bw.Write((ushort)images.Count);
            int offset = 6 + images.Count * 16;
            for (int i = 0; i < images.Count; i++)
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
}