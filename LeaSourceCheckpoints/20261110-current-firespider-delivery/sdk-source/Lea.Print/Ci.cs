using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Lea.Win;

namespace Lea.Print
{
    public static class Ci
    {
        public static ReadOnlyDictionary<int, Pi> Cp(V view, Pc capture)
        {
            if (view == null || capture == null) throw new ArgumentNullException(view == null ? "view" : "capture");
            if (view.IsDisposed || view.Disposing) throw new ObjectDisposedException("view");
            if (!view.IsHandleCreated || view.InvokeRequired) throw new InvalidOperationException("Image capture requires the view UI thread and an existing handle");
            if (view.Busy || view.Le != null || !Object.ReferenceEquals(capture.Page, view.At)) throw new InvalidOperationException("Print page changed or is busy");
            var images = new Dictionary<int, Pi>(); int bytes = 0;
            for (int index = 0; index < capture.Document.Nodes.Count; index++)
            {
                Pn node = capture.Document.Nodes[index];
                if (node.Kind != "img") continue;
                if (images.Count == 16) throw new InvalidOperationException("Print image count limit");
                Uri source;
                if (!Uri.TryCreate(node.CacheKey.Length == 0 ? node.Image : node.CacheKey, UriKind.Absolute, out source)) throw new InvalidOperationException("Print image has no permitted cache address");
                Pi image;
                using (var bitmap = view.Pix(capture.Page, source)) image = Pi.Cp(bitmap);
                if (image.Bytes > 8 * 1024 * 1024 - bytes) throw new InvalidOperationException("Print image aggregate limit");
                bytes += image.Bytes; images.Add(index, image);
            }
            if (view.Busy || view.Le != null || view.IsDisposed || !Object.ReferenceEquals(capture.Page, view.At)) throw new InvalidOperationException("Print page changed during image capture");
            return new ReadOnlyDictionary<int, Pi>(images);
        }
    }
}
