$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-PrintHost.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintDocument.generated.ps1'
$out=Join-Path $workspace 'continuity-evidence/print-document-preparation-v1'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve document preparation'}
if((Get-FileHash $source).Hash -cne 'CF42E7F5E95B9BDCAC015FB83D41C8558A96796DCE6A2DD48D566087CA0D779B'){throw 'Original runner differs'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-One([string]$Before,[string]$After){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne 1){throw ('Unique anchor required: '+$Before)}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After))
}
Replace-One 'print-host-v1' 'print-document-v1'
Replace-One 'Owned WinForms ElementHost preview window plus all prior print checks; no actual Firespider dispatch or printer adoption' 'Owned structured document renderer from actual native view snapshot plus prior print checks; CSS/images/full fidelity and Firespider adoption unfinished'
Replace-One "'Lea.Tests/PrintHostMain.cs','Lea.Print/Lea.Print.csproj'" "'Lea.Tests/PrintHostMain.cs','Lea.Print/Pm.cs','Lea.Tests/PrintDocumentMain.cs','Lea.Print/Lea.Print.csproj'"
Replace-One "('`"'+(Join-Path `$out 'Ph.cs')+'`"')" "('`"'+(Join-Path `$out 'Ph.cs')+'`"'),('`"'+(Join-Path `$out 'Pm.cs')+'`"')"
Replace-One '/main:PrintHostMain' '/main:PrintDocumentMain'
Replace-One "('`"'+(Join-Path `$out 'PrintHostMain.cs')+'`"')" "('`"'+(Join-Path `$out 'PrintHostMain.cs')+'`"'),('`"'+(Join-Path `$out 'PrintDocumentMain.cs')+'`"')"
Replace-One 'PASS Lea WinForms preview and foundation' 'PASS Lea structured document and foundation'
$before='    $sources=@('
$after=@'
    $documentFixture=Join-Path $workspace 'LeaForge/test-pages/print-document.html';Hold $documentFixture
    Copy-Item -LiteralPath $documentFixture -Destination (Join-Path $out 'print-document.html')
    Hold (Join-Path $out 'print-document.html')
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
