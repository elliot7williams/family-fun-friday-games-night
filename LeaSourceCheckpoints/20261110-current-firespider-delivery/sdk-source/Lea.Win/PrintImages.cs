using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Lea.Engine;

namespace Lea.Win
{
    public sealed partial class V
    {
        // Explicit mature-view build addition. Caller owns the detached bitmap.
        public Bitmap Pix(P expected, Uri source)
        {
            if (closed || IsDisposed || Disposing) throw new ObjectDisposedException("V");
            if (!IsHandleCreated || InvokeRequired) throw new InvalidOperationException("Image capture requires the view UI thread and an existing handle");
            if (expected == null || source == null || !source.IsAbsoluteUri) throw new ArgumentException("Current page and absolute image source required");
            if (!Object.ReferenceEquals(expected, At) || Busy || Le != null || expected.D == null)
                throw new InvalidOperationException("Page is not ready for image capture");
            string key = source.AbsoluteUri;
            bool referenced = false;
            foreach (Uri active in PicSources(expected))
                if (String.Equals(active.AbsoluteUri, key, StringComparison.Ordinal)) { referenced = true; break; }
            Image image;
            if (!referenced || !pics.TryGetValue(key, out image) || image == null)
                throw new InvalidOperationException("Referenced image is not cached");
            int width = image.Width, height = image.Height;
            if (width < 1 || height < 1 || width > 2048 || height > 2048 || (long)width * height > 1048576)
                throw new InvalidOperationException("Print image pixel limit");
            var copy = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            try
            {
                using (var graphics = Graphics.FromImage(copy))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.DrawImageUnscaled(image, 0, 0);
                }
                if (!Object.ReferenceEquals(expected, At) || Busy || closed || IsDisposed || Le != null)
                    throw new InvalidOperationException("Page changed during image capture");
                return copy;
            }
            catch { copy.Dispose(); throw; }
        }
    }
}
