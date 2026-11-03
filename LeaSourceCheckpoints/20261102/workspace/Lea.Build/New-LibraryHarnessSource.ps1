function New-LibraryHarnessSource {
    param([Parameter(Mandatory)][string]$Original)
    $about=New-AboutHarnessSource $Original
    $begin='        private static void CheckAboutNavigation(Form form, ApplicationSettingsBase settings, string page)'
    $end='        private static object Call(Form form, string name, params object[] args)'
    if([regex]::Matches($about,[regex]::Escape($begin)).Count -ne 1 -or [regex]::Matches($about,[regex]::Escape($end)).Count -ne 1){throw 'Library harness method anchors differ'}
    $offset=$about.IndexOf($begin,[StringComparison]::Ordinal)
    $tail=$about.IndexOf($end,$offset,[StringComparison]::Ordinal)
    $old=$about.Substring($offset,$tail-$offset)
    $helper=@'
        private static void CheckLibraryNavigation(Form form, ApplicationSettingsBase settings, string page)
        {
            var tabs = Member<TabControl>(form, "k");
            var view = (V)tabs.SelectedTab.Controls[0];
            var tab = tabs.SelectedTab;
            var count = tabs.TabCount;
            var policy = view.Ng;
            Check(policy != null, "Library current tab retains navigation policy");
            settings["bs"] = Collection("lea-library.invalid");
            var blocked = new List<bool>();
            var local = 0;
            view.Ng = address =>
            {
                var decision = policy(address);
                if (address.Host == "lea-library.invalid")
                {
                    blocked.Add(decision);
                    return false; // The regression must never make external requests.
                }
                if (address.Scheme == Uri.UriSchemeFile) local++;
                return decision;
            };
            try
            {
                foreach (var kind in new[] { "history", "bookmarks" })
                using (var library = (Form)Activator.CreateInstance(form.GetType().Assembly.GetType(
                    "Firespider_Internet." + (kind == "history" ? "f6" : "F7"), true)))
                {
                    var list = Member<ListBox>(library, kind == "history" ? "l0" : "x1");
                    var method = kind == "history" ? "o4" : "p1";
                    list.DataSource = null;
                    list.Items.Clear();
                    list.Items.Add("https://lea-library.invalid/" + kind);
                    list.Items.Add(page);
                    list.SelectedIndex = -1;
                    var attempts = blocked.Count;
                    var localBefore = local;
                    Call(library, method, library, EventArgs.Empty);
                    Check(blocked.Count == attempts && local == localBefore && !view.Busy,
                        "Library empty selection does not navigate: " + kind);
                    list.SelectedIndex = 0;
                    Call(library, method, library, EventArgs.Empty);
                    var pending = Member<IDictionary>(form, "qleaPending");
                    Wait(() => !view.Busy && !pending.Contains(view), "Library blocked navigation completes: " + kind);
                    Check(blocked.Count == attempts + 1 && !blocked[attempts],
                        "Library routes through blocked-site policy: " + kind);
                    Check(view.Le != null && view.Le.K == "blocked", "Library blocked target rejected: " + kind);
                    Check(tabs.TabCount == count && tabs.SelectedTab == tab && ReferenceEquals(tab.Controls[0], view),
                        "Library reuses selected Lea tab: " + kind);
                    list.SelectedIndex = 1;
                    Call(library, method, library, EventArgs.Empty);
                    Wait(() => !view.Busy && !pending.Contains(view), "Library local navigation completes: " + kind);
                    Check(local == localBefore + 1 && view.Le == null && view.At != null &&
                        (string)Call(form, "qleaUrl", tab) == page, "Library local target loads: " + kind);
                    Check(tabs.TabCount == count && tabs.SelectedTab == tab && ReferenceEquals(tab.Controls[0], view),
                        "Library local load retains selected tab: " + kind);
                }
            }
            finally { view.Ng = policy; }
            Console.WriteLine("PASS Library navigation: history and bookmarks, empty selection, original blocked-site decisions, local loads and current-tab reuse; no external I/O.");
        }

'@
    $helper=$helper.Replace("`r`n","`n")
    $text=$about.Replace($old,$helper)
    $call='CheckAboutNavigation(form, settings, page)'
    if([regex]::Matches($text,'"about"').Count -ne 3 -or [regex]::Matches($text,[regex]::Escape($call)).Count -ne 1){throw 'Library harness dispatch anchors differ'}
    $text=$text.Replace('"about"','"library"').Replace($call,'CheckLibraryNavigation(form, settings, page)')
    $inverse=$text.Replace('"library"','"about"').Replace('CheckLibraryNavigation(form, settings, page)',$call).Replace($helper,$old)
    if($inverse -cne $about){throw 'Library harness inverse differs'}
    @{text=$text;inverseVerified=$true;scope='Default main form, real list handlers, blocked and file navigation, selected-tab identity and no external I/O'}
}
