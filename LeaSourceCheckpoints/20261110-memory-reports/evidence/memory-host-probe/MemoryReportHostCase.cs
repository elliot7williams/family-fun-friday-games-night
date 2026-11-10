        private static void CheckAccessibilityReport(Form form, ApplicationSettingsBase settings, string root)
        {
            var tabs = Member<TabControl>(form, "k");
            var original = tabs.SelectedTab;
            var view = (V)original.Controls[0];
            var reportFiles = Directory.GetFiles(Path.GetTempPath(), "Firespider Accessibility *.html").OrderBy(path => path).ToArray();
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
            Check(rendered.T == "Accessibility Inspector" && rendered.At.X.Contains("0 potential issue(s)"), "report summary missing");
            Check(rendered.H.Contains("&lt;unsafe&gt;") && !rendered.H.Contains("<unsafe>"), "report title was not escaped");
            Check(!rendered.U.IsFile && rendered.At.Og.Op && !rendered.At.Og.Lc, "report is not opaque nonlocal memory");
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
            CheckAccessibilityStates(form, settings, root, fixture);
            Check(reportFiles.SequenceEqual(Directory.GetFiles(Path.GetTempPath(), "Firespider Accessibility *.html").OrderBy(path => path)), "report command wrote an HTML file");
        }

        private static void CheckAccessibilityStates(Form form, ApplicationSettingsBase settings, string root, string fixture)
        {
            var tabs = Member<TabControl>(form, "k");
            var original = tabs.SelectedTab;
            var count = tabs.TabCount;
            var view = (V)original.Controls[0];
            using (var site = new LoopbackSite())
            {
                site.Release.Reset();
                try
                {
                    Call(form, "q1", site.Url + "accessibility-loading");
                    Wait(() => site.Hits > 0 && view.Busy, "accessibility loading gate");
                    Menu(form, "Accessibility Inspector").PerformClick();
                    Check(tabs.TabCount == count && Member<Label>(form, "su").Text.Contains("ready Lea page"), "loading report was not guarded");
                }
                finally { site.Release.Set(); }
                WaitLea(tabs);
            }
            Call(form, "q1", new Uri(fixture).AbsoluteUri);
            WaitLea(tabs);
            Check((bool)Call(form, "qleaSuspend", original), "report source could not pause");
            var placeholder = original.Controls[0];
            Menu(form, "Accessibility Inspector").PerformClick();
            Check(tabs.TabCount == count && original.Controls[0] == placeholder && placeholder is Panel, "paused report resumed or replaced tab");
            view = (V)Call(form, "qleaResume", original, true);
            WaitLea(tabs);
            tabs.TabPages.Remove(original);
            Check(tabs.TabCount == 0, "no-selection fixture has unexpected tabs");
            Menu(form, "Accessibility Inspector").PerformClick();
            Check(tabs.TabCount == 0, "no-selection report created a tab");
            tabs.TabPages.Add(original); tabs.SelectedTab = original;

            var oldPrivate = view.Pv;
            Set(form, "pv", true);
            try
            {
                var secret = "PRIVATE_REPORT_" + Guid.NewGuid().ToString("N");
                var privateFixture = Path.Combine(Path.GetTempPath(), "lea-private-source-" + Guid.NewGuid().ToString("N") + ".html");
                File.WriteAllText(privateFixture, "<title>" + secret + "</title><main><h1>Private test</h1></main>");
                var sourceUrl = new Uri(privateFixture).AbsoluteUri;
                Call(form, "q1", sourceUrl);
                WaitLea(tabs);
                Menu(form, "Accessibility Inspector").PerformClick();
                WaitLea(tabs);
                var report = tabs.SelectedTab;
                var privateView = (V)report.Controls[0];
                Check(privateView.Pv && !privateView.U.IsFile && privateView.At.Og.Op && !privateView.At.Og.Lc, "private report lost memory/nonlocal/private state");
                Check(privateView.H.Contains(secret), "private report omitted source title");
                var reportUrl = privateView.U.AbsoluteUri;
                Call(form, "qrecoverysnapshot");
                Call(form, "qleaSaveSession");
                foreach (SettingsProperty property in settings.Properties)
                {
                    var values = settings[property.Name] as StringCollection;
                    Check(values == null || !values.Cast<string>().Any(value => value != null && (value.Contains(secret) || value.Contains(sourceUrl) || value.Contains(reportUrl))), "private report leaked to settings " + property.Name);
                }
                var session = Environment.GetEnvironmentVariable("LEA_FIRESPIDER_SESSION");
                Check(!File.Exists(session) || (!File.ReadAllText(session).Contains(sourceUrl) && !File.ReadAllText(session).Contains(reportUrl)), "private report leaked to session");
                tabs.TabPages.Remove(report); report.Dispose();
                tabs.SelectedTab = original;
                File.WriteAllText(Path.Combine(Path.GetDirectoryName(root), "private-report-retention.json"),
                    "{\"privateView\":true,\"reportUrlIsFile\":false,\"privateSourceOrReportInSession\":false,\"globalPrivacyQualified\":false}");
            }
            finally { Set(form, "pv", false); view.Pv = oldPrivate; }
            Check(!Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(module => module.ModuleName.Equals("mshtml.dll", StringComparison.OrdinalIgnoreCase)), "state probes loaded legacy engine");
            Console.WriteLine("PASS Lea accessibility states: loading/paused/no-selection guards; private memory report/session checks");
        }
