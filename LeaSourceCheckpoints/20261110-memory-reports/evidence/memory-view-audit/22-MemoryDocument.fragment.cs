        public P Ht(string html)
        {
            CheckIdle();
            Cursor = Cursors.WaitCursor;
            try { return Show(LoadCore(() => e.Ht(html), true), true); }
            finally { if (!closed && loadOwner == null) Cursor = Cursors.Default; }
        }
