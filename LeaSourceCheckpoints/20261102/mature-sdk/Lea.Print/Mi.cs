using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lea.Print
{
    public static class Mi
    {
        public static BitmapSource Cp(Pi image, bool grayscale)
        {
            if (image == null) throw new ArgumentNullException("image");
            byte[] pixels = image.Pixels;
            if (grayscale)
            {
                pixels = (byte[])pixels.Clone();
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    byte value = (byte)((299 * pixels[i + 2] + 587 * pixels[i + 1] + 114 * pixels[i] + 500) / 1000);
                    pixels[i] = pixels[i + 1] = pixels[i + 2] = value;
                }
            }
            var source = BitmapSource.Create(image.Width, image.Height, 96, 96, PixelFormats.Bgra32, null, pixels, image.Width * 4);
            source.Freeze(); return source;
        }
    }
}
