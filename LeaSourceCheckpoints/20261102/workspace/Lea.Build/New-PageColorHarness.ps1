function New-PageColorHarness {
    param([Parameter(Mandatory)][string]$Original)
    $about=New-AboutHarnessSource $Original
    $begin='        private static void CheckAboutNavigation(Form form, ApplicationSettingsBase settings, string page)'
    $end='        private static object Call(Form form, string name, params object[] args)'
    if([regex]::Matches($about,[regex]::Escape($begin)).Count -ne 1 -or [regex]::Matches($about,[regex]::Escape($end)).Count -ne 1){throw 'Theme harness anchors differ'}
    $offset=$about.IndexOf($begin,[StringComparison]::Ordinal);$tail=$about.IndexOf($end,$offset,[StringComparison]::Ordinal)
    $old=$about.Substring($offset,$tail-$offset)
    $helper=@'
        private static void CheckAboutNavigation(Form form, ApplicationSettingsBase settings, string page)
        {
            var tabs = Member<TabControl>(form, "k");
            var view = (V)tabs.SelectedTab.Controls[0];
            var tab = tabs.SelectedTab;
            var count = tabs.TabCount;
            var toolbar = Member<Control>(form, "q");
            var policy = view.Ng;
            view.Ng = address => address.IsFile && policy(address);
            var folder = Path.Combine(Path.GetTempPath(), "lea-page-colors-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            try
            {
                var cases = new[] {
                    new[] { "<meta name='theme-color' content='#125678'>", "", "#125678" },
                    new[] { "<meta name='theme-color' content='not-a-color'><meta name='theme-color' content='#245689'>", "", "#245689" },
                    new[] { "<meta name='theme-color' media='(unknown-feature)' content='#ff0000'><meta name='theme-color' content='#35679a'>", "", "#35679a" },
                    new[] { "<meta property='MSAPPLICATION-TILECOLOR' content='#4678ab'>", "", "#4678ab" },
                    new[] { "", "bgcolor='#5789bc'", "#5789bc" },
                    new[] { "<style>body{background-color:#689acd}</style>", "", "#689acd" }
                };
                for (var index = 0; index < cases.Length; index++)
                {
                    var path = Path.Combine(folder, index + ".html");
                    File.WriteAllText(path, "<!doctype html><html><head>" + cases[index][0] +
                        "</head><body " + cases[index][1] + "><p>Lea page color " + index + "</p></body></html>");
                    settings["chromeMode"] = "site";
                    settings["i"] = Color.FromArgb(0, 204, 255);
                    Call(form, "q1", new Uri(path).AbsoluteUri);
                    var pending = Member<IDictionary>(form, "qleaPending");
                    Wait(() => !view.Busy && !pending.Contains(view), "Theme page completion " + index);
                    Check(view.Le == null && view.At != null, "Theme local page loaded " + index);
                    var expected = ColorTranslator.FromHtml(cases[index][2]).ToArgb();
                    var actual = (Color)Call(form, "qsitechromecolor", new object[] { null });
                    Console.WriteLine("THEME_OBSERVATION index=" + index + " actual=" + actual + " url=" + view.U + " direct=" + Call(form, "qleapagecolor", view) + " html=" + view.H);
                    Console.WriteLine("THEME_STATE busy=" + view.Busy + " disposed=" + view.IsDisposed + " error=" + view.Le + " root=" + (view.At.D == null ? "null" : view.At.D.K) + " parse=" + Call(form, "qparsechromecolor", "#125678"));
                    var nodes = new Queue<Lea.Engine.N>();
                    if (view.At.D != null) nodes.Enqueue(view.At.D);
                    for (var inspected = 0; nodes.Count != 0 && inspected < 32; inspected++)
                    {
                        var node = nodes.Dequeue();
                        Console.WriteLine("THEME_NODE kind=" + node.K + " name=" + node.Av("name") + " content=" + node.Av("content") + " media=" + node.Av("media"));
                        foreach (var child in node.C.Take(32)) nodes.Enqueue(child);
                    }
                    Check(actual.ToArgb() == expected, "Lea parsed document page color " + index);
                    Check(toolbar.BackColor.ToArgb() == expected, "Lea completion updates selected toolbar " + index);
                    Check(tabs.TabCount == count && tabs.SelectedTab == tab && ReferenceEquals(tab.Controls[0], view),
                        "Theme lookup retains selected tab " + index);
                }
                Call(form, "qusecurrentpagecolor");
                Check((string)settings["chromeMode"] == "custom" &&
                    ((Color)settings["i"]).ToArgb() == ColorTranslator.FromHtml("#689acd").ToArgb(),
                    "Use-current-page-color command applies Lea color");
                var custom = Color.FromArgb(31, 82, 47);
                settings["i"] = custom;
                Call(form, "qapplybrowserappearance");
                Call(form, "qleaDone", view, EventArgs.Empty);
                Check(toolbar.BackColor.ToArgb() == custom.ToArgb(), "Completion preserves custom toolbar mode");
                Check(((Color)Call(form, "qleapagecolor", new object[] { null })).IsEmpty, "Missing Lea host has no color");
                var root = view.At.D;
                var children = root.C.ToArray();
                var expectedFallback = ColorTranslator.FromHtml("#689acd").ToArgb();
                try
                {
                    root.C.Clear();
                    root.C.Add(root);
                    Check(((Color)Call(form, "qleapagecolor", view)).ToArgb() == expectedFallback,
                        "Cyclic document terminates with computed background");
                    root.C.Clear();
                    for (var index = 0; index < 65535; index++) root.C.Add(root);
                    Check(((Color)Call(form, "qleapagecolor", view)).ToArgb() == expectedFallback,
                        "Exact 65536 queued-entry budget remains usable");
                    root.C.Add(root);
                    Check(((Color)Call(form, "qleapagecolor", view)).IsEmpty,
                        "Oversized document fails closed before traversal");
                }
                finally
                {
                    root.C.Clear();
                    foreach (var child in children) root.C.Add(child);
                }
                Check(root.C.SequenceEqual(children), "Boundary fixture restores original document children");
                settings["chromeMode"] = "site";
                Call(form, "qapplybrowserappearance");
                TabPage background = null;
                try
                {
                    Call(form, "qln", new Uri(Path.Combine(folder, "0.html")).AbsoluteUri);
                    background = tabs.SelectedTab;
                    var other = (V)background.Controls[0];
                    Check(background != tab && other != view, "Background completion uses a distinct Lea tab");
                    tabs.SelectedTab = tab;
                    var pending = Member<IDictionary>(form, "qleaPending");
                    Wait(() => !other.Busy && !pending.Contains(other), "Background theme page completes");
                    Check(other.Le == null && other.At != null && other.U.AbsoluteUri == new Uri(Path.Combine(folder, "0.html")).AbsoluteUri,
                        "Background theme fixture loaded");
                    Check(toolbar.BackColor.ToArgb() == expectedFallback && tabs.SelectedTab == tab,
                        "Background completion cannot recolor the selected page toolbar");
                }
                finally { if (background != null && background != tab) Call(form, "qleaClose", background); }
                Check(tabs.TabCount == count && tabs.SelectedTab == tab && ReferenceEquals(tab.Controls[0], view),
                    "Background fixture restores original tab collection");
                Console.WriteLine("PASS Lea page color boundaries: null host, cycle, exact and over-limit traversal, restored tree, background completion and original tabs.");
            }
            finally
            {
                view.Ng = policy;
                foreach (var file in Directory.GetFiles(folder)) File.Delete(file);
                Directory.Delete(folder);
            }
            Console.WriteLine("PASS Lea page colors: six parsed-document fixtures, automatic selected completion, current-page command, custom mode, same tab and no external I/O.");
        }

'@
    $updated=$about.Replace($old,$helper)
    if($updated.Replace($helper,$old) -cne $about){throw 'Theme harness inverse differs'}
    @{text=$updated;inverseVerified=$true}
}
