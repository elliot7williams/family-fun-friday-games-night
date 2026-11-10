using System;
using Lea.Win;

namespace Lea.Print
{
    // UI-thread bridge. NativeSelection is field text, not a document selection API.
    public sealed class Pc
    {
        public Ps Document { get; private set; }
        public string NativeSelection { get; private set; }
        public int StyleWidth { get; private set; }
        public int StyleHeight { get; private set; }
        internal Lea.Engine.P Page { get; private set; }
        private Pc() { }

        public static Pc Cp(V view)
        { return Capture(view, null); }

        public static Pc Cs(V view, Pg geometry)
        {
            if (geometry == null) throw new ArgumentNullException("geometry");
            return Capture(view, geometry);
        }
        public static Pc Cf(V view, Pg geometry)
        {
            if (geometry == null) throw new ArgumentNullException("geometry");
            return Capture(view, geometry, true);
        }
        private static Pc Capture(V view, Pg geometry, bool friendly = false)
        {
            if (view == null) throw new ArgumentNullException("view");
            if (view.IsDisposed || view.Disposing) throw new ObjectDisposedException("view");
            if (!view.IsHandleCreated || view.InvokeRequired)
                throw new InvalidOperationException("Print capture requires the view UI thread and an existing handle");
            if (view.Busy || view.Le != null || view.At == null || view.At.D == null)
                throw new InvalidOperationException("Page is not ready for print capture");
            var page = view.At;
            string selection = view.Sx();
            int width = geometry == null ? 0 : checked((int)Math.Floor(geometry.LayoutWidth));
            int height = geometry == null ? 0 : checked((int)Math.Floor(geometry.LayoutHeight));
            Ps document = geometry == null ? Ps.Cp(page) : friendly
                ? Ps.Cf(page, view.Pst(width, height)) : Ps.Cs(page, view.Pst(width, height));
            if (!Object.ReferenceEquals(page, view.At) || view.Busy || view.IsDisposed || view.Le != null)
                throw new InvalidOperationException("Page changed during print capture");
            return new Pc { Document = document, NativeSelection = selection, StyleWidth = width, StyleHeight = height, Page = page };
        }
    }
}
