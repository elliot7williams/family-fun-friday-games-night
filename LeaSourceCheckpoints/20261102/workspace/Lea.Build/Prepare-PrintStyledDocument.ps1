$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-PrintStylesV3.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintStyledDocument.generated.ps1'
$out=Join-Path $workspace 'continuity-evidence/print-styled-document-preparation-v1'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve styled preparation'}
if((Get-FileHash $source).Hash -cne 'A6D62F130E8562CFF175D9076261FBE26F7DD904D9EF0136AD531CF0AE7304AC'){throw 'Original runner differs'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-One([string]$Before,[string]$After){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne 1){throw ('Unique anchor required: '+$Before)}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After))
}
Replace-One 'print-styles-v3' 'print-styled-document-v1'
Replace-One 'Owned new core print-media style query plus prior print checks; style snapshot/renderer adoption and full regressions unfinished' 'Owned detached print styles and visibility/render/XPS integration plus prior checks; full CSS/resources/Firespider adoption unfinished'
Replace-One "'Lea.Print/Ps.cs','Lea.Print/Pc.cs'" "'Lea.Print/Ps.cs','Lea.Print/Py.cs','Lea.Print/Pz.cs','Lea.Print/Pc.cs'"
Replace-One "'Lea.Tests/PrintStylesMain.cs','Lea.Print/Lea.Print.csproj'" "'Lea.Tests/PrintStylesMain.cs','Lea.Tests/PrintStyledDocumentMain.cs','Lea.Print/Lea.Print.csproj'"
Replace-One "('`"'+(Join-Path `$out 'Ps.cs')+'`"')" "('`"'+(Join-Path `$out 'Ps.cs')+'`"'),('`"'+(Join-Path `$out 'Py.cs')+'`"')"
Replace-One "('`"'+(Join-Path `$out 'Pm.cs')+'`"')" "('`"'+(Join-Path `$out 'Pm.cs')+'`"'),('`"'+(Join-Path `$out 'Pz.cs')+'`"')"
Replace-One '/main:PrintStylesMain' '/main:PrintStyledDocumentMain'
Replace-One "('`"'+(Join-Path `$out 'PrintStylesMain.cs')+'`"')" "('`"'+(Join-Path `$out 'PrintStylesMain.cs')+'`"'),('`"'+(Join-Path `$out 'PrintStyledDocumentMain.cs')+'`"')"
Replace-One 'PASS Lea print styles and foundation' 'PASS Lea styled document and foundation'
$before=@'
    $coreBuilder=Join-Path $PSScriptRoot 'Build-PrintStylesCore.ps1';Hold $coreBuilder
    . $coreBuilder
'@
$after=@'
    $styleProofPath=Join-Path $workspace 'continuity-evidence/print-styles-v3/result.json';Hold $styleProofPath '445574DF56FB88273F314D07496DF63656C7BCAB4F448AF88463F29DA0750188'
    $styleProof=Get-Content $styleProofPath -Raw|ConvertFrom-Json
    if(!$styleProof.passed -or !$styleProof.inputsUnchanged -or $styleProof.ownerAttention){throw 'Style core proof incomplete'}
    $from=Join-Path $workspace 'continuity-evidence/print-styles-v3/Lea.Engine.dll';Hold $from 'A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791'
    $core=Join-Path $out 'Lea.Engine.dll';Copy-Item -LiteralPath $from -Destination $core
    Hold $core 'A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791'
'@
Replace-One $before.TrimEnd("`r","`n") $after.TrimEnd("`r","`n")
$before='    $sources=@('
$after=@'
    $styledFixture=Join-Path $workspace 'LeaForge/test-pages/print-styled-document.html';Hold $styledFixture
    Copy-Item -LiteralPath $styledFixture -Destination (Join-Path $out 'print-styled-document.html')
    Hold (Join-Path $out 'print-styled-document.html')
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
