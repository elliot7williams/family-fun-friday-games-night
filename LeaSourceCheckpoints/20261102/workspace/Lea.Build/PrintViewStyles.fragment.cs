        public System.Collections.ObjectModel.ReadOnlyDictionary<N, S> Pst(int width, int height)
        {
            if (closed || IsDisposed || Disposing) throw new ObjectDisposedException("V");
            if (!IsHandleCreated || InvokeRequired) throw new InvalidOperationException("Print styles require the view UI thread and an existing handle");
            if (Busy || Le != null || At == null || At.D == null) throw new InvalidOperationException("Page is not ready for print styles");
            var page = At;
            var styles = e.Pst(page, width, height);
            if (!Object.ReferenceEquals(page, At) || Busy || closed || IsDisposed || Le != null) throw new InvalidOperationException("Page changed during print styles");
            return styles;
        }

