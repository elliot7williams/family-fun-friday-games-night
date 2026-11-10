$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\memory-report-host-probe-20261110-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve memory report probe'}
$case=Join-Path $root 'Lea.Tests/AccessibilityHostCase.cs'
$receipt=Join-Path $root 'continuity-evidence/accessibility-host-ui-v3/result.json'
if((Get-FileHash -LiteralPath $receipt).Hash -cne 'B1C72E3C3AD9587511057D2ED6F667310629B133B0AC5FBC40E61633EDA1BC81'){throw 'Retained privacy reproduction differs'}
$r=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
$pin=@($r.inputs|Where-Object path -CEQ $case)
if($pin.Count -ne 1 -or (Get-FileHash -LiteralPath $case).Hash -cne $pin[0].sha256){throw 'Retained case source differs'}
$original=[IO.File]::ReadAllText($case);$text=$original
$old='            Check(File.Exists(rendered.U.LocalPath), "report file workflow changed");'
$new='            Check(!rendered.U.IsFile && rendered.At.Og.Op && !rendered.At.Og.Lc, "report is not opaque nonlocal memory");'
if([regex]::Matches($text,[regex]::Escape($old)).Count -ne 1){throw 'File assertion boundary differs'}
$text=$text.Replace($old,$new)
$marker='            var fixture = Path.Combine'
$text=$text.Replace($marker,'            var reportFiles = Directory.GetFiles(Path.GetTempPath(), "Firespider Accessibility *.html").OrderBy(path => path).ToArray();'+[Environment]::NewLine+$marker)
$call='            CheckAccessibilityStates(form, settings, root, fixture);'
$text=$text.Replace($call,$call+[Environment]::NewLine+'            Check(reportFiles.SequenceEqual(Directory.GetFiles(Path.GetTempPath(), "Firespider Accessibility *.html").OrderBy(path => path)), "report command wrote an HTML file");')
$start=$text.IndexOf('            var oldPrivate = view.Pv;',[StringComparison]::Ordinal)
$end=$text.IndexOf('            Check(!Process.GetCurrentProcess().Modules',$start,[StringComparison]::Ordinal)
if($start -lt 0 -or $end -le $start){throw 'Private diagnostic boundary differs'}
$replacement=@'
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
'@
$text=$text.Substring(0,$start)+$replacement+[Environment]::NewLine+$text.Substring($end)
$text=$text.Replace('PASS Lea accessibility states: loading/paused/no-selection guards; private-retention issue reproduced, NOT fixed','PASS Lea accessibility states: loading/paused/no-selection guards; private memory report/session checks')
New-Item -ItemType Directory -Path $out|Out-Null
$target=Join-Path $out 'MemoryReportHostCase.cs'
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
if([IO.File]::ReadAllText($target) -cne $text){throw 'Probe readback differs'}
@{source=$target;sha256=(Get-FileHash -LiteralPath $target).Hash;baselineCaseSha256=$pin[0].sha256;baselinePrivacyReceiptSha256=(Get-FileHash -LiteralPath $receipt).Hash;settingsNavigationChecksRetained=$true;expectedBehaviorChanged='Report is memory-backed, not retained HTML';executed=$false}|ConvertTo-Json -Depth 4|Set-Content -LiteralPath (Join-Path $out 'source.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'source.json')
