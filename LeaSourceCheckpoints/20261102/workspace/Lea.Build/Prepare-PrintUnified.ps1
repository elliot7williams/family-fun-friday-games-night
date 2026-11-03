$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-PrintCapture.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintUnified.generated.ps1'
$out=Join-Path $workspace 'continuity-evidence/print-unified-preparation-v1'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve unified preparation'}
if((Get-FileHash $source).Hash -cne '529E4155B71FF281929D42AD2C05CD40844156F99F291F2D8239259E8828579D'){throw 'Original runner differs'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-One([string]$Before,[string]$After){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne 1){throw ('Unique anchor required: '+$Before)}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After))
}
Replace-One 'print-capture-view-v1' 'print-unified-view-v1'
Replace-One 'Owned Framework print capture bridge and actual view2BEB probe; no actual Firespider host, renderer, printer output or release qualification' 'Owned unified Framework snapshot/options/geometry/live view2BEB capture; no actual Firespider host, renderer, printer output or release qualification'
Replace-One "@('Lea.Print/Ps.cs','Lea.Print/Pc.cs','Lea.Tests/PrintCaptureProbe.cs','Lea.Print/Lea.Print.csproj')" "@('Lea.Print/Ps.cs','Lea.Print/Pc.cs','Lea.Print/Po.cs','Lea.Tests/PrintCaptureProbe.cs','Lea.Tests/PrintSnapshot.cs','Lea.Tests/PrintGeometry.cs','Lea.Tests/PrintUnifiedMain.cs','Lea.Print/Lea.Print.csproj')"
Replace-One "('`"'+(Join-Path `$out 'Pc.cs')+'`"')" "('`"'+(Join-Path `$out 'Pc.cs')+'`"'),('`"'+(Join-Path `$out 'Po.cs')+'`"')"
Replace-One "'PrintCaptureProbe.exe'" "'PrintUnifiedChecks.exe'"
Replace-One "('/reference:`"'+`$library+'`"'),('`"'+(Join-Path `$out 'PrintCaptureProbe.cs')+'`"')" "('/reference:`"'+`$library+'`"'),'/main:PrintUnifiedMain',('`"'+(Join-Path `$out 'PrintCaptureProbe.cs')+'`"'),('`"'+(Join-Path `$out 'PrintSnapshot.cs')+'`"'),('`"'+(Join-Path `$out 'PrintGeometry.cs')+'`"'),('`"'+(Join-Path `$out 'PrintUnifiedMain.cs')+'`"')"
Replace-One 'PASS Lea print capture: 13 checks' 'PASS Lea unified print foundation: 73 checks'
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
