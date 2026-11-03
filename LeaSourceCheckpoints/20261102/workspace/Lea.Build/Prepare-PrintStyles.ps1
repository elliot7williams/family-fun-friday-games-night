$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-PrintDocumentV2.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintStyles.generated.ps1'
$out=Join-Path $workspace 'continuity-evidence/print-styles-preparation-v1'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve styles preparation'}
if((Get-FileHash $source).Hash -cne '34CE35EA910179C15AB85455731637D2BF8BAD1E1C00F9B2790704DD1BFE3D0A'){throw 'Original runner differs'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-One([string]$Before,[string]$After){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne 1){throw ('Unique anchor required: '+$Before)}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After))
}
Replace-One 'print-document-v2' 'print-styles-v1'
Replace-One 'Owned structured document renderer from actual native view snapshot plus prior print checks; CSS/images/full fidelity and Firespider adoption unfinished' 'Owned new core print-media style query plus prior print checks; style snapshot/renderer adoption and full regressions unfinished'
Replace-One "'Lea.Tests/PrintDocumentMain.cs','Lea.Print/Lea.Print.csproj'" "'Lea.Tests/PrintDocumentMain.cs','Lea.Tests/PrintStylesMain.cs','Lea.Print/Lea.Print.csproj'"
Replace-One '/main:PrintDocumentMain' '/main:PrintStylesMain'
Replace-One "('`"'+(Join-Path `$out 'PrintDocumentMain.cs')+'`"')" "('`"'+(Join-Path `$out 'PrintDocumentMain.cs')+'`"'),('`"'+(Join-Path `$out 'PrintStylesMain.cs')+'`"')"
Replace-One 'PASS Lea structured document and foundation' 'PASS Lea print styles and foundation'
$before=@'
    $core=Join-Path $workspace 'continuity-evidence/native-selection-view-v1/Lea.Engine.dll'
    Hold $core '52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC'
    Copy-Item -LiteralPath $core -Destination (Join-Path $out 'Lea.Engine.dll')
    $core=Join-Path $out 'Lea.Engine.dll';Hold $core '52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC'
'@
$after=@'
    $coreBuilder=Join-Path $PSScriptRoot 'Build-PrintStylesCore.ps1';Hold $coreBuilder
    . $coreBuilder
'@
Replace-One $before.TrimEnd("`r","`n") $after.TrimEnd("`r","`n")
$before='    $sources=@('
$after=@'
    $styleFixture=Join-Path $workspace 'LeaForge/test-pages/print-styles.html';Hold $styleFixture
    Copy-Item -LiteralPath $styleFixture -Destination (Join-Path $out 'print-styles.html')
    Hold (Join-Path $out 'print-styles.html')
    $sources=@(
'@
Replace-One $before $after.TrimEnd("`r","`n")
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
