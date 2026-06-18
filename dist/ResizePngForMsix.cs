using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
public static class ResizePngForMsix
{
    public static void Main(string[] args)
    {
        if (args.Length < 4) throw new ArgumentException("Usage: input output width height");
        using (var source = Image.FromFile(args[0]))
        using (var bitmap = new Bitmap(Int32.Parse(args[2]), Int32.Parse(args[3]), PixelFormat.Format32bppArgb))
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.Transparent);
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.DrawImage(source, 0, 0, bitmap.Width, bitmap.Height);
            bitmap.Save(args[1], ImageFormat.Png);
        }
    }
}
