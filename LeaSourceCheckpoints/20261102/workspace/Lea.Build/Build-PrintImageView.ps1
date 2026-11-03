$baselinePath=Join-Path $root 'continuity-evidence/print-live-capture-v1/result.json'
Hold $baselinePath '751F1EF75A7E184DFE245D1DEFE7732EBBB4A58C5E34AD284DFB716AAE57F123'
$baseline=Get-Content $baselinePath -Raw|ConvertFrom-Json
if(!$baseline.passed -or !$baseline.inputsUnchanged -or $baseline.ownerAttention){throw 'Mature view baseline incomplete'}
$recipe=Join-Path $root 'continuity-evidence/native-selection-view-v1/view.rsp'
Hold $recipe '788A7ACB5906115BE0FF86BA17152E4941C8CFA40E327644593D088876742F9C'
$viewSource=Join-Path $out 'view-source';$null=New-Item -ItemType Directory -Path $viewSource
$options=[Collections.Generic.List[string]]::new()
foreach($line in Get-Content $recipe){
    if($line.StartsWith('/out:')){$line='/out:"'+(Join-Path $out 'Lea.Win.dll')+'"'}
    elseif($line -match '^"(.+\.cs)"$'){
        $from=Join-Path $root ('continuity-evidence/print-live-capture-v1/view-source/'+[IO.Path]::GetFileName($Matches[1]))
        $pins=@($baseline.inputs|Where-Object path -ceq $from|Select-Object -ExpandProperty sha256 -Unique)
        if($pins.Count -ne 1){throw 'Mature source pin missing'}
        Hold $from $pins[0];$to=Join-Path $viewSource ([IO.Path]::GetFileName($from));Copy-Item $from $to
        if([IO.Path]::GetFileName($from) -eq 'Di.cs'){
            $projector=Join-Path $PSScriptRoot 'Project-DecoderRetirement.ps1';Hold $projector;. $projector
            $projected=Project-DecoderRetirement ([IO.File]::ReadAllText($from))
            [IO.File]::WriteAllText($to,$projected,[Text.UTF8Encoding]::new($false));Hold $to
        }
        if([IO.Path]::GetFileName($from) -eq 'Rp.cs'){
            $projector=Join-Path $PSScriptRoot 'Project-DecoderLauncherFailure.ps1';Hold $projector;. $projector
            $projected=Project-DecoderLauncherFailure ([IO.File]::ReadAllText($from))
            [IO.File]::WriteAllText($to,$projected,[Text.UTF8Encoding]::new($false));Hold $to
        }
        if($DocGestures -and [IO.Path]::GetFileName($from) -eq 'V.cs'){
            $original=[IO.File]::ReadAllText($from);$projected=$original
            $hooks=@(
                @('            PaintDocument(pe);',"            PaintDocument(pe);`r`n            PaintDocumentSelection(pe.Graphics);"),
                @('            base.OnMouseMove(e);',"            base.OnMouseMove(e);`r`n            if (MoveDocumentSelection(e)) return;"),
                @('            base.OnMouseClick(e);',"            base.OnMouseClick(e);`r`n            if (ConsumeDocumentSelectionClick(e)) return;"),
                @('            base.OnKeyDown(e);',"            base.OnKeyDown(e);`r`n            if (DocumentSelectionKey(e)) return;")
            )
            if($DocCopy){$hooks+=@(
                @('return Busy || editLock || !Ce("cp") ? String.Empty : ed.SelectedText;', 'return Busy || editLock || (edit == null && heldEdit == null) || !Ce("cp") ? String.Empty : ed.SelectedText;'),
                @("        public bool Ce(string code)`n        {", "        public bool Ce(string code)`n        {`n            if (DocumentCopyAvailable(code)) return true;"),
                @("        public int Ec(string code)`n        {", "        public int Ec(string code)`n        {`n            if (DocumentCopyAvailable(code)) return Sdc();")
            )}
            if($DocUia){$hooks+=@(
                @("        private sealed class Ut`n        {", "        private sealed class Ut`n        {`n            public bool DocumentModel;"),
                @('v.AutomationText()', 'v.DocumentSelectionSnapshot()',4),
                @('return SupportedTextSelection.None;', 'return SupportedTextSelection.Single;'),
                @('ITextRangeProvider[] ITextProvider.GetSelection() { throw new InvalidOperationException("The Lea document does not support text selection."); }', 'ITextRangeProvider[] ITextProvider.GetSelection() { return v.OnUi(delegate { return v.DocumentSelectionRanges(); }); }'),
                @('v.AutomationVisibleTextRanges(text)', 'v.DocumentVisibleRanges(text)'),
                @('v.AutomationTextRangeFromPoint(text, screenLocation)', 'v.DocumentRangeFromPoint(text, screenLocation)'),
                @('v.AutomationTextRectangles(t, s, e)', 'v.DocumentRangeRectangles(t, s, e)'),
                @('public void Select() { throw new InvalidOperationException("The Lea document does not support text selection."); }', 'public void Select() { v.OnUi(delegate { Check(); v.SelectDocumentRange(t, s, e); }); }'),
                @('if (!Object.ReferenceEquals(v.At, t.P)) throw new ElementNotAvailableException("The Lea text range belongs to an earlier page generation.");', 'v.CheckDocumentRange(t);'),
                @("                    start = position;`n                    end = Math.Min(t.X.Length, position + 1);", "                    start = DocumentBoundary(t.X, position);`n                    end = DocumentNextBoundary(t.X, start);"),
                @('if (unit == TextUnit.Line) return new List<int>(t.Ls);', 'if (unit == TextUnit.Line) return new List<int>(t.Ls); if (unit == TextUnit.Character) { var points = new List<int>(StringInfo.ParseCombiningCharacters(t.X)); if (points.Count == 0) points.Add(0); if (points[points.Count - 1] != t.X.Length) points.Add(t.X.Length); return points; }')
            )}
            if($ProviderThreadProbe){
                $hooks+=,@('ProviderOptions.ServerSideProvider | ProviderOptions.UseComThreading','ProviderOptions.ServerSideProvider',2)
                $result.providerThreadProbe=$true;$result.currentFullNativeQualified=$false;$result.legacyUiAUnresolved=$true
            }
            if($RangeLeaseProbe){
                foreach($hook in $hooks){if($hook[0] -ceq 'v.AutomationVisibleTextRanges(text)'){$hook[1]='LeaseVisible(v.DocumentVisibleRanges(text))'}}
                $hooks+=,@('public Ur(V owner) { v = owner; }', @'
private ITextRangeProvider[] visibleLease;
            private ITextRangeProvider[] LeaseVisible(ITextRangeProvider[] ranges)
            { visibleLease = ranges; return visibleLease; }
            public Ur(V owner) { v = owner; }
'@)
                $result.rangeLeaseProbe=$true;$result.currentFullNativeQualified=$false;$result.legacyUiAUnresolved=$true
            }
            foreach($hook in $hooks){$expected=if($hook.Count -gt 2){$hook[2]}else{1};if([regex]::Matches($projected,[regex]::Escape($hook[0])).Count -ne $expected){throw ('Selection hook anchor count differs: '+$hook[0])};$projected=$projected.Replace($hook[0],$hook[1])}
            $inverse=$projected;foreach($hook in $hooks){$inverse=$inverse.Replace($hook[1],$hook[0])}
            if($inverse -cne $original){throw 'Selection hook inverse differs'}
            [IO.File]::WriteAllText($to,$projected,[Text.UTF8Encoding]::new($false));$result.selectionHooksInverseVerified=$true
            Hold $to
        }elseif([IO.Path]::GetFileName($from) -notin @('Di.cs','Rp.cs')){
            Hold $to $pins[0]
        }
        $line='"'+$to+'"'
    }elseif($line -match '^/reference:"([^"]+)"$'){
        $from=$Matches[1]
        if([IO.Path]::GetFileName($from) -eq 'Lea.Engine.dll'){$from=$core;Hold $from 'A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791'}
        else{
            $pins=@($baseline.inputs|Where-Object path -ceq $from|Select-Object -ExpandProperty sha256 -Unique)
            if($pins.Count -ne 1){throw 'Mature reference pin missing'};Hold $from $pins[0]
        }
        $line='/reference:"'+$from+'"'
    }
    $options.Add($line)
}
$addition=Join-Path $root 'Lea.Win/PrintImages.cs';Hold $addition
if($DocUia){$options.Add('/define:DOCUMENT_UIA')}
$to=Join-Path $viewSource 'PrintImages.cs';Copy-Item $addition $to;Hold $to;$options.Add('"'+$to+'"')
if($DocSelection){
    $addition=Join-Path $root 'Lea.Win/DocumentSelection.cs';Hold $addition
    $to=Join-Path $viewSource 'DocumentSelection.cs';Copy-Item $addition $to;Hold $to;$options.Add('"'+$to+'"')
}
if($DocUia){
    $addition=Join-Path $root 'Lea.Win/DocumentSelectionAutomation.cs';Hold $addition
    $to=Join-Path $viewSource 'DocumentSelectionAutomation.cs';Copy-Item $addition $to;Hold $to;$options.Add('"'+$to+'"')
}
$response=Join-Path $out 'view.rsp';[IO.File]::WriteAllLines($response,$options);Hold $response
$wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261026.ps1';Hold $wrapper 'C3EBB3801FAFB80DA031AAB69FE4837EEB4E4EECD3907F9704F9D6186FA42F0A'
Hold (Join-Path $PSScriptRoot 'PublicCompiler.cs') 'D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB'
$built=Invoke-OwnedTestRun -Name 'build-cache-view' -Application (Get-Process -Id $PID).Path -Arguments @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$response+'"'),'-ExpectedResponseSha256',(Get-FileHash $response).Hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
if(!$built.passed){throw 'Mature cache view build failed'}
