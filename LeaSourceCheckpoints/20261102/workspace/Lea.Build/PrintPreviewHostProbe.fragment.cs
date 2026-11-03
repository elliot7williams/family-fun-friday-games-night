        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindow", ExactSpelling = true)]
        private static extern IntPtr PreviewOwner(IntPtr window, uint command);
        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "IsWindowEnabled", ExactSpelling = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool PreviewEnabled(IntPtr window);

        private static void CheckPrintPreview(Form form, ApplicationSettingsBase settings, string root)
        {
            var tabs = Member<TabControl>(form, "k");
            var tab = tabs.SelectedTab;
            var view = (V)tab.Controls[0];
            Check(PreviewEnabled(form.Handle), "browser native window initially enabled");
            string fixture = Path.Combine(root, "test-pages", "print-styled-document.html");
            settings["t"] = "";
            Call(form, "e4", form, EventArgs.Empty);
            Check(!Application.OpenForms.Cast<Form>().Any(f => f is Lea.Print.Ph), "disabled preview hover has no dialog");
            for (int pass = 0; pass < 2; pass++)
            {
                view.Go(fixture); WaitLea(tabs);
                var original = view.At; var size = view.ClientSize;
                Lea.Print.Ph observed = null;
                Lea.Print.Pw capturedPlan = null;
                Exception failure = null;
                var watch = Stopwatch.StartNew();
                using (var timer = new System.Windows.Forms.Timer { Interval = 50 })
                {
                    int currentPass = pass;
                    timer.Tick += (sender, args) =>
                    {
                        var preview = Application.OpenForms.Cast<Form>().OfType<Lea.Print.Ph>().SingleOrDefault();
                        if (preview == null)
                        {
                            if (watch.ElapsedMilliseconds > 5000)
                            {
                                failure = new Exception("Expected Lea preview did not open"); timer.Stop();
                                foreach (var window in form.OwnedForms) window.Close();
                            }
                            return;
                        }
                        timer.Stop(); observed = preview;
                        try
                        {
                            Console.WriteLine("Preview window inventory: modal=" + preview.Modal + "; managedOwner=" + (preview.Owner == form) + "; managedEnabled=" + form.Enabled + "; nativeOwner=" + (PreviewOwner(preview.Handle, 4) == form.Handle) + "; nativeEnabled=" + PreviewEnabled(form.Handle));
                            Check(preview.Modal && PreviewOwner(preview.Handle, 4) == form.Handle && !PreviewEnabled(form.Handle), "preview owns native modal browser relationship");
                            Check(PreviewOwner(form.Handle, 4) != preview.Handle, "native preview ownership is directional");
                            preview.Vw.UpdateLayout();
                            var pages = (Lea.Print.Pp)typeof(Lea.Print.Pv).GetField("pages", Members).GetValue(preview.Vw);
                            capturedPlan = (Lea.Print.Pw)typeof(Lea.Print.Pp).GetField("plan", Members).GetValue(pages);
                            Check(capturedPlan.Styled && capturedPlan.Source.Contains("PRINT_VISIBLE") && !capturedPlan.Source.Contains("SECRET"), "browser preview uses styled print media and hidden-content policy");
                            var frame = (Lea.Print.Pf)typeof(Lea.Print.Pp).GetField("frame", Members).GetValue(pages);
                            Check(frame.Address == "", "local preview header omits filesystem path");
                            Check((bool)Call(form, "qpv") && Application.OpenForms.Cast<Form>().OfType<Lea.Print.Ph>().Count() == 1, "preview reentrant command cannot open a second dialog");
                            Check(preview.Vw.Count == pages.PageCount && preview.Vw.Count > 0, "browser preview has captured pages");
                            if (currentPass == 0)
                            {
                                int width = (int)preview.Vw.ActualWidth, height = (int)preview.Vw.ActualHeight;
                                Check(width >= 300 && height >= 200, "browser preview surface is arranged");
                                var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                                bitmap.Render(preview.Vw);
                                var pixels = new byte[width * height * 4]; bitmap.CopyPixels(pixels, width * 4, 0);
                                Check(pixels.Count(value => value != 0 && value != 255) > 500, "browser preview renders nonblank pixels");
                                var png = new System.Windows.Media.Imaging.PngBitmapEncoder();
                                png.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
                                using (var output = new FileStream(Path.Combine(Path.GetDirectoryName(root), "browser-preview.png"), FileMode.CreateNew)) png.Save(output);
                                preview.Close();
                            }
                            else
                            {
                                string source = capturedPlan.Source;
                                view.Go(Path.Combine(root, "Lea", "start.html"));
                                Check(!Object.ReferenceEquals(view.At, original) && capturedPlan.Source == source, "modal preview retains detached content across navigation");
                                object[] keys = { Message.Create(preview.Handle, 0x100, IntPtr.Zero, IntPtr.Zero), Keys.Escape };
                                Check((bool)typeof(Lea.Print.Ph).GetMethod("ProcessCmdKey", Members).Invoke(preview, keys), "preview Escape key is handled");
                            }
                        }
                        catch (Exception error) { failure = error; }
                        finally { if (!preview.IsDisposed && preview.Visible) preview.Close(); }
                    };
                    timer.Start();
                    try
                    {
                        if (pass == 0) Member<ToolStripItem>(form, "PP").PerformClick();
                        else { settings["t"] = "e"; Call(form, "e4", form, EventArgs.Empty); }
                    }
                    finally { timer.Stop(); }
                }
                if (failure != null) throw failure;
                Check(observed != null && observed.IsDisposed && PreviewEnabled(form.Handle), "preview closes and restores native enabled browser");
                Check(!(bool)form.GetType().GetField("qpb", Members).GetValue(form), "preview busy guard resets");
                bool disposed = false;
                try { capturedPlan.Dr(0); } catch (ObjectDisposedException) { disposed = true; }
                Check(disposed, "browser disposes preview plan after modal lifetime");
                Check(tabs.SelectedTab == tab && view.ClientSize == size && (pass != 0 || Object.ReferenceEquals(view.At, original)), "preview preserves selected tab and viewport");
                WaitLea(tabs);
            }
            view.Go(Path.Combine(root, "test-pages", "print-preview-image.html")); WaitLea(tabs);
            Check((bool)Call(form, "qpv") && Member<Label>(form, "su").Text == "Preview unavailable for this page", "unresolved image preview rejected without legacy fallback");
            Check(!Application.OpenForms.Cast<Form>().Any(f => f is Lea.Print.Ph), "image rejection leaves no preview dialog");
            var pending = typeof(V).GetField("activationPending", Members);
            int prior = (int)pending.GetValue(view); pending.SetValue(view, prior + 1);
            try { Check((bool)Call(form, "qpv") && !(bool)form.GetType().GetField("qpb", Members).GetValue(form), "busy view preview rejected and guard reset"); }
            finally { pending.SetValue(view, prior); }
            settings["t"] = "";
            Console.WriteLine("PASS Lea browser print preview: click/hover/modal/reentrancy/Escape/detached lifetime/privacy/image and busy guards");
        }

