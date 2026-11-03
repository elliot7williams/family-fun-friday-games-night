$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-PrintUnified.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintText.generated.ps1'
$out=Join-Path $workspace 'continuity-evidence/print-text-preparation-v1'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve text preparation'}
if((Get-FileHash $source).Hash -cne 'EB1C52E8534CBE76F1AE54BFCB86ADF46772E9C57632CDCEF3FD9EBA06B66CD1'){throw 'Original runner differs'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-One([string]$Before,[string]$After){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne 1){throw ('Unique anchor required: '+$Before)}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After))
}
Replace-One 'print-unified-view-v1' 'print-text-view-v1'
Replace-One 'Owned unified Framework snapshot/options/geometry/live view2BEB capture; no actual Firespider host, renderer, printer output or release qualification' 'Owned Framework native selected-text pagination/drawing and foundation; bitmap output only, no full-document renderer, host dispatch, actual printer or release qualification'
Replace-One "'Lea.Tests/PrintUnifiedMain.cs','Lea.Print/Lea.Print.csproj'" "'Lea.Tests/PrintUnifiedMain.cs','Lea.Print/Pt.cs','Lea.Tests/PrintTextMain.cs','Lea.Print/Lea.Print.csproj'"
Replace-One "('`"'+(Join-Path `$out 'Po.cs')+'`"')" "('`"'+(Join-Path `$out 'Po.cs')+'`"'),('`"'+(Join-Path `$out 'Pt.cs')+'`"')"
Replace-One "'PrintUnifiedChecks.exe'" "'PrintTextChecks.exe'"
Replace-One '/main:PrintUnifiedMain' '/main:PrintTextMain'
Replace-One "('`"'+(Join-Path `$out 'PrintUnifiedMain.cs')+'`"')" "('`"'+(Join-Path `$out 'PrintUnifiedMain.cs')+'`"'),('`"'+(Join-Path `$out 'PrintTextMain.cs')+'`"')"
Replace-One 'PASS Lea unified print foundation: 73 checks' 'PASS Lea native text print and foundation'
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
