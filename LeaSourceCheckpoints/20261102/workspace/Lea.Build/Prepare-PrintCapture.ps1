$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-PrintSnapshotFramework.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintCapture.generated.ps1'
$out=Join-Path $workspace 'continuity-evidence/print-capture-preparation-v1'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve capture preparation'}
if((Get-FileHash $source).Hash -cne 'C7E76DDAA782D417B3E926F968BE19CED33349C7663FCBB150A77AA4C593FB33'){throw 'Framework runner differs'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-One([string]$Before,[string]$After){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne 1){throw ('Unique anchor required: '+$Before)}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After))
}
Replace-One 'print-snapshot-framework-v1' 'print-capture-view-v1'
Replace-One 'Owned Framework library/test build and pure managed snapshot EXE; no view/host integration, printer output, visual or release qualification' 'Owned Framework print capture bridge and actual view2BEB probe; no actual Firespider host, renderer, printer output or release qualification'
Replace-One "@('mscorlib.dll','System.dll','System.Core.dll')" "@('mscorlib.dll','System.dll','System.Core.dll','System.Drawing.dll','System.Windows.Forms.dll','Accessibility.dll','UIAutomationClient.dll','UIAutomationTypes.dll','WindowsBase.dll')"
Replace-One "    `$references.Add('/reference:`"'+`$core+'`"')" @'
    $references.Add('/reference:"'+$core+'"')
    $view=Join-Path $workspace 'continuity-evidence/native-selection-view-v1/Lea.Win.dll'
    Hold $view '2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC'
    Copy-Item -LiteralPath $view -Destination (Join-Path $out 'Lea.Win.dll')
    $view=Join-Path $out 'Lea.Win.dll';Hold $view '2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC'
    $references.Add('/reference:"'+$view+'"')
    $fixture=Join-Path $workspace 'LeaForge/test-pages/native-selection.html';Hold $fixture
    Copy-Item -LiteralPath $fixture -Destination (Join-Path $out 'native-selection.html')
    $fixture=Join-Path $out 'native-selection.html';Hold $fixture
'@
Replace-One "@('Lea.Print/Ps.cs','Lea.Tests/PrintSnapshot.cs','Lea.Tests/PrintSnapshotMain.cs','Lea.Print/Lea.Print.csproj')" "@('Lea.Print/Ps.cs','Lea.Print/Pc.cs','Lea.Tests/PrintCaptureProbe.cs','Lea.Print/Lea.Print.csproj')"
Replace-One "('`"'+(Join-Path `$out 'Ps.cs')+'`"')" "('`"'+(Join-Path `$out 'Ps.cs')+'`"'),('`"'+(Join-Path `$out 'Pc.cs')+'`"')"
Replace-One "'PrintSnapshotChecks.exe'" "'PrintCaptureProbe.exe'"
Replace-One "('`"'+(Join-Path `$out 'PrintSnapshot.cs')+'`"'),('`"'+(Join-Path `$out 'PrintSnapshotMain.cs')+'`"')" "('`"'+(Join-Path `$out 'PrintCaptureProbe.cs')+'`"')"
Replace-One "-Name 'framework-snapshot' -Application `$exe -Arguments @()" "-Name 'view-print-capture' -Application `$exe -Arguments @(('`"'+`$fixture+'`"'))"
Replace-One 'PASS Lea Framework print snapshot: 40 checks' 'PASS Lea print capture: 13 checks'
$inverse=$text
for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
if($inverse -cne $original){throw 'Projection inverse differs'}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
$receipt=@{inverseVerified=$true;syntaxVerified=$true;original=(Get-FileHash $source);generated=(Get-FileHash $target);edits=$edits}
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($receipt|ConvertTo-Json -Depth 6))
$receipt|Select-Object inverseVerified,syntaxVerified,generated|ConvertTo-Json
