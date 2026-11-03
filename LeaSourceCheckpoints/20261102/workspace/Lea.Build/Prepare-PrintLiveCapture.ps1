$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-PrintStyledDocumentV2.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintLiveCapture.generated.ps1'
$out=Join-Path $workspace 'continuity-evidence/print-live-capture-preparation-v1'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve live capture preparation'}
if((Get-FileHash $source).Hash -cne '82B5DF77A1A8AFB81112B4790617A291602234446B6F3B8B2DC83F99ADB92914'){throw 'Original runner differs'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-One([string]$Before,[string]$After){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne 1){throw ('Unique anchor required: '+$Before)}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After))
}
Replace-One 'print-styled-document-v2' 'print-live-capture-v1'
Replace-One 'Owned detached print styles and visibility/render/XPS integration plus prior checks; full CSS/resources/Firespider adoption unfinished' 'Owned live view print styles/capture bridge plus prior checks on rebuilt view; full CSS/resources/Firespider adoption unfinished'
Replace-One "'Lea.Tests/PrintStyledDocumentMain.cs','Lea.Print/Lea.Print.csproj'" "'Lea.Tests/PrintStyledDocumentMain.cs','Lea.Tests/PrintLiveCaptureMain.cs','Lea.Print/Lea.Print.csproj'"
Replace-One '/main:PrintStyledDocumentMain' '/main:PrintLiveCaptureMain'
Replace-One "('`"'+(Join-Path `$out 'PrintStyledDocumentMain.cs')+'`"')" "('`"'+(Join-Path `$out 'PrintStyledDocumentMain.cs')+'`"'),('`"'+(Join-Path `$out 'PrintLiveCaptureMain.cs')+'`"')"
Replace-One 'PASS Lea styled document and foundation' 'PASS Lea live styled capture and foundation'
$before=@'
    $view=Join-Path $workspace 'continuity-evidence/native-selection-view-v1/Lea.Win.dll'
    Hold $view '2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC'
    Copy-Item -LiteralPath $view -Destination (Join-Path $out 'Lea.Win.dll')
    $view=Join-Path $out 'Lea.Win.dll';Hold $view '2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC'
'@
$after=@'
    $viewBuilder=Join-Path $PSScriptRoot 'Build-PrintViewStyles.ps1';Hold $viewBuilder
    . $viewBuilder
'@
Replace-One $before.TrimEnd("`r","`n") $after.TrimEnd("`r","`n")
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
