        // Read a permitted native-field selection without changing focus or clipboard.
        public string Sx()
        {
            return OnUi(delegate
            {
                return Busy || editLock || !Ce("cp") ? String.Empty : ed.SelectedText;
            }) ?? String.Empty;
        }

