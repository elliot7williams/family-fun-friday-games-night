function New-AboutHarnessSource {
    param([string]$Original)
    $helper=@'
        private static Form AboutMain(Assembly assembly)
        {
            var project = assembly.GetType("Firespider_Internet.My.MyProject", true);
            var forms = project.GetProperty("Forms", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null, null);
            return (Form)forms.GetType().GetProperty("j").GetValue(forms, null);
        }

        private static void CheckAboutNavigation(Form form, ApplicationSettingsBase settings, string page)
        {
            var tabs = Member<TabControl>(form, "k");
            var allowed = new List<bool>();
            settings["bs"] = Collection("firespiderinternet.weebly.com");
            ControlEventHandler added = (sender, args) =>
            {
                var tab = args.Control as TabPage;
                if (tab == null) return;
                var view = tab.Controls.OfType<V>().Single();
                var policy = view.Ng;
                Check(policy != null, "About tab retains navigation policy");
                view.Ng = address =>
                {
                    var decision = policy(address);
                    if (address.Host == "firespiderinternet.weebly.com")
                    {
                        allowed.Add(decision);
                        return false; // Never permit external I/O in this regression fixture.
                    }
                    return decision;
                };
            };
            tabs.ControlAdded += added;
            try
            {
                using (var about = (Form)Activator.CreateInstance(form.GetType().Assembly.GetType("Firespider_Internet.f2", true)))
                {
                    settings["t"] = "";
                    var count = tabs.TabCount;
                    Call(about, "l9", about, null);
                    Call(about, "m1", about, null);
                    Check(tabs.TabCount == count && allowed.Count == 0, "About hover disabled without side effects");
                    foreach (var method in new[] { "l4", "l8", "l9", "m1" })
                    {
                        settings["t"] = method == "l9" || method == "m1" ? "e" : "";
                        var before = tabs.TabCount;
                        var attempts = allowed.Count;
                        Call(about, method, about, null);
                        var selected = (V)tabs.SelectedTab.Controls[0];
                        var pending = Member<IDictionary>(form, "qleaPending");
                        Wait(() => !selected.Busy && !pending.Contains(selected), "About navigation completes: " + method);
                        Check(tabs.TabCount == before + 1 && tabs.SelectedTab.Controls[0] is V, "About creates Lea tab: " + method);
                        Check(allowed.Count == attempts + 1 && !allowed[attempts], "About routes through blocked-site policy: " + method);
                        Check(selected.Le != null && selected.Le.K == "blocked", "About target is rejected before remote load: " + method);
                    }
                }
            }
            finally { tabs.ControlAdded -= added; }
            Console.WriteLine("PASS About navigation: four click/hover paths, Lea tabs, original blocked-site decisions, disabled hover and no external I/O.");
        }

'@
    $text=$Original.Replace("`r`n","`n");$helper=$helper.Replace("`r`n","`n")
    $edits=@(
        @('"cache", "script" }.Contains(mode)','"cache", "script", "about" }.Contains(mode)'),
        @('var form = (Form)Activator.CreateInstance(type);','var form = mode == "about" ? AboutMain(assembly) : (Form)Activator.CreateInstance(type);'),
        @('                if (mode == "style-start") CheckStyleStartup(form, settings, log);',('                if (mode == "about") CheckAboutNavigation(form, settings, page);'+"`n"+'                if (mode == "style-start") CheckStyleStartup(form, settings, log);')),
        @('        private static object Call(Form form, string name, params object[] args)',($helper+'        private static object Call(Form form, string name, params object[] args)'))
    )
    $updated=$text
    foreach($pair in $edits){if([regex]::Matches($updated,[regex]::Escape($pair[0])).Count -ne 1){throw ('About harness anchor differs: '+$pair[0])};$updated=$updated.Replace($pair[0],$pair[1])}
    $inverse=$updated;for($index=$edits.Count-1;$index -ge 0;$index--){$inverse=$inverse.Replace($edits[$index][1],$edits[$index][0])}
    if($inverse -cne $text){throw 'About harness inverse differs'}
    return $updated
}
