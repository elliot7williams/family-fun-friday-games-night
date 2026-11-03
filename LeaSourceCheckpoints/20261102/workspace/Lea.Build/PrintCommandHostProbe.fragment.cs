        private delegate bool PrintWindowCallback(IntPtr window, IntPtr parameter);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool EnumWindows(PrintWindowCallback callback, IntPtr parameter);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetAncestor(IntPtr window, uint flags);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr window);
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet=System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern int GetClassName(IntPtr window, System.Text.StringBuilder text, int capacity);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);

        private static void CheckPrintCommand(Form form, ApplicationSettingsBase settings, string root)
        {
            CheckPrintPreview(form, settings, root);
            var tabs = Member<TabControl>(form, "k");
            var tab = tabs.SelectedTab;
            var view = (V)tab.Controls[0];
            var busy = form.GetType().GetField("qpb", Members);
            var printer = form.GetType().GetField("qps", Members);
            // Deterministic supported fixture; no system default-printer mutation.
            var initial = new System.Drawing.Printing.PrinterSettings { PrinterName = "Microsoft XPS Document Writer" };
            Check(initial.IsValid, "print test has installed virtual printer"); printer.SetValue(form, initial);
            settings["t"] = ""; Call(form, "e2", form, EventArgs.Empty);
            Check(!(bool)busy.GetValue(form), "disabled print hover leaves no active selection");
            for (int pass = 0; pass < 3; pass++)
            {
                view.Go(Path.Combine(root, "test-pages", "print-styled-document.html")); WaitLea(tabs);
                var page = view.At; var viewport = view.ClientSize;
                bool seen = false, posted = false; Exception failure = null;
                var changedTab = new TabPage("Print guard fixture"); changedTab.Controls.Add(new Panel());
                int currentPass = pass;
                using (var timer = new System.Windows.Forms.Timer { Interval = 100 })
                {
                    timer.Tick += (sender, args) =>
                    {
                        if (seen) return;
                        EnumWindows((window, unused) =>
                        {
                            var name = new System.Text.StringBuilder(64); GetClassName(window, name, name.Capacity);
                            if (window == form.Handle || !IsWindowVisible(window) || GetAncestor(window, 3) != form.Handle || name.ToString() != "#32770") return true;
                            seen = true;
                            try
                            {
                                Check(!PreviewEnabled(form.Handle) && (bool)busy.GetValue(form), "print native modal ownership and busy state");
                                Check((bool)Call(form, "qpr") && (bool)Call(form, "qpv"), "print and preview share reentry guard");
                                if (currentPass == 2) { tabs.TabPages.Add(changedTab); tabs.SelectedTab = changedTab; }
                            }
                            catch (Exception error) { failure = error; }
                            finally { posted = PostMessage(window, 0x111, new IntPtr(currentPass == 2 && failure == null ? 1 : 2), IntPtr.Zero); }
                            return false;
                        }, IntPtr.Zero);
                    };
                    timer.Start();
                    try
                    {
                        if (pass == 1) { settings["t"] = "e"; Call(form, "e2", form, EventArgs.Empty); }
                        else Member<ToolStripItem>(form, "P").PerformClick();
                    }
                    finally { timer.Stop(); tabs.SelectedTab = tab; tabs.TabPages.Remove(changedTab); changedTab.Dispose(); }
                }
                if (failure != null) throw failure;
                Check(seen && posted && PreviewEnabled(form.Handle), "print selection returns and restores browser");
                Check(!(bool)busy.GetValue(form), "print busy guard resets");
                string expected = pass == 2 ? "Print unavailable for this page" : "Printing canceled";
                Check(Member<Label>(form, "su").Text == expected, "print cancel or changed-tab acceptance handled without fallback");
                Check(Object.ReferenceEquals(printer.GetValue(form), initial), "cancel/rejected selection does not replace remembered settings");
                Check(tabs.SelectedTab == tab && Object.ReferenceEquals(view.At, page) && view.ClientSize == viewport, "print preserves page and viewport");
            }
            var pending = typeof(V).GetField("activationPending", Members); int previous = (int)pending.GetValue(view);
            pending.SetValue(view, previous + 1);
            try { Check((bool)Call(form, "qpr") && Member<Label>(form, "su").Text == "Print unavailable for this page" && !(bool)busy.GetValue(form), "busy page print fails before dialog"); }
            finally { pending.SetValue(view, previous); }
            var unknown = new TabPage("Unknown print fixture"); unknown.Controls.Add(new Panel()); tabs.TabPages.Add(unknown);
            try { tabs.SelectedTab = unknown; Check((bool)Call(form, "qpr") && Member<Label>(form, "su").Text == "Printing unavailable for this tab", "unknown tab print never casts to WebBrowser"); }
            finally { tabs.SelectedTab = tab; tabs.TabPages.Remove(unknown); unknown.Dispose(); }
            settings["t"] = "";
            Console.WriteLine("PASS Lea browser print command: click/hover/cancel/native-owner/reentry/changed-tab/busy/unknown guards; no queue submission");
        }
