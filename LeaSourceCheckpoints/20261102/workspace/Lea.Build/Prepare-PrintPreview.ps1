$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-PrintXpsV3.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintPreview.generated.ps1'
$out=Join-Path $workspace 'continuity-evidence/print-preview-preparation-v1'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve preview preparation'}
if((Get-FileHash $source).Hash -cne '10A53494870D633EC87B89DC4FA01FE1CABDA6C52F91CC0416A0B83E3D7AB678'){throw 'Original runner differs'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-One([string]$Before,[string]$After){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne 1){throw ('Unique anchor required: '+$Before)}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After))
}
Replace-One 'print-xps-v3' 'print-preview-v1'
Replace-One 'Owned empty-stream XPS export/readback plus paginator/frame/foundation; no printer or host adoption' 'Owned WPF preview controls/window plus XPS/paginator/frame/foundation; no printer or Firespider adoption'
Replace-One "'Lea.Tests/PrintXpsMain.cs','Lea.Print/Lea.Print.csproj'" "'Lea.Tests/PrintXpsMain.cs','Lea.Print/Pv.cs','Lea.Tests/PrintPreviewMain.cs','Lea.Print/Lea.Print.csproj'"
Replace-One "('`"'+(Join-Path `$out 'Px.cs')+'`"')" "('`"'+(Join-Path `$out 'Px.cs')+'`"'),('`"'+(Join-Path `$out 'Pv.cs')+'`"')"
Replace-One '/main:PrintXpsMain' '/main:PrintPreviewMain'
Replace-One "('`"'+(Join-Path `$out 'PrintXpsMain.cs')+'`"')" "('`"'+(Join-Path `$out 'PrintXpsMain.cs')+'`"'),('`"'+(Join-Path `$out 'PrintPreviewMain.cs')+'`"')"
Replace-One 'PASS Lea XPS export and foundation' 'PASS Lea preview and foundation'
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
