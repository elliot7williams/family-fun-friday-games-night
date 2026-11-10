        private static void CheckAccessibilityReport(Form form, ApplicationSettingsBase settings, string root)
        {
            var tabs = Member<TabControl>(form, "k");
            var original = tabs.SelectedTab;
            var view = (V)original.Controls[0];
            var fixture = Path.Combine(Path.GetTempPath(), "lea-access-source-" + Guid.NewGuid().ToString("N") + ".html");
            File.WriteAllText(fixture, "<!doctype html><title>&lt;unsafe&gt;</title><main><h1>Report test</h1><img alt='Named'><a href='#x'>Named link</a><input aria-label='Name'></main>");
            Call(form, "q1", new Uri(fixture).AbsoluteUri);
            Wait(() => !view.Busy && view.At != null && view.T == "<unsafe>", "accessibility fixture load");
            var count = tabs.TabCount;
            settings["t"] = "";
            Call(form, "q871", form, EventArgs.Empty);
            Check(tabs.TabCount == count, "disabled hover created a report");
            Menu(form, "Accessibility Inspector").PerformClick();
            WaitLea(tabs);
            Check(tabs.TabCount == count + 1 && tabs.SelectedTab.Controls[0] is V, "report click did not create Lea tab");
            var report = tabs.SelectedTab;
            var rendered = (V)report.Controls[0];
            Check(rendered.T == "Accessibility Inspector" && rendered.Q.Contains("0 potential issue(s)"), "report summary missing");
            Check(rendered.H.Contains("&lt;unsafe&gt;") && !rendered.H.Contains("<unsafe>"), "report title was not escaped");
            Check(File.Exists(rendered.U.LocalPath), "report file workflow changed");
            using (var image = new Bitmap(form.ClientSize.Width, form.ClientSize.Height))
            {
                form.DrawToBitmap(image, form.ClientRectangle);
                image.Save(Path.Combine(Path.GetDirectoryName(root), "accessibility-report.png"));
            }
            tabs.TabPages.Remove(report); report.Dispose();
            tabs.SelectedTab = original;
            settings["t"] = "e";
            Call(form, "q871", form, EventArgs.Empty);
            WaitLea(tabs);
            Check(tabs.TabCount == count + 1 && tabs.SelectedTab.Controls[0] is V, "enabled hover did not create Lea report");
            report = tabs.SelectedTab;
            tabs.TabPages.Remove(report); report.Dispose();
            settings["t"] = "";
            using (var empty = new TabPage("Empty test"))
            {
                tabs.TabPages.Add(empty); tabs.SelectedTab = empty;
                var before = tabs.TabCount;
                Menu(form, "Accessibility Inspector").PerformClick();
                Check(tabs.TabCount == before, "empty tab created a report");
                tabs.TabPages.Remove(empty);
            }
            tabs.SelectedTab = original;
            Check(!Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(module => module.ModuleName.Equals("mshtml.dll", StringComparison.OrdinalIgnoreCase)), "report loaded old engine");
            Console.WriteLine("PASS Lea accessibility host: click/hover/escaping/empty-tab/no-legacy");
        }
