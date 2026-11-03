$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-PrintPreview.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintHost.generated.ps1'
$out=Join-Path $workspace 'continuity-evidence/print-host-preparation-v1'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve host preparation'}
if((Get-FileHash $source).Hash -cne 'D5ADC8665C309890BF5B1906168E7156638E89D30D7B490484402A8C8D2F9D46'){throw 'Original runner differs'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-One([string]$Before,[string]$After){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne 1){throw ('Unique anchor required: '+$Before)}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After))
}
Replace-One 'print-preview-v1' 'print-host-v1'
Replace-One 'Owned WPF preview controls/window plus XPS/paginator/frame/foundation; no printer or Firespider adoption' 'Owned WinForms ElementHost preview window plus all prior print checks; no actual Firespider dispatch or printer adoption'
Replace-One "'System.Xml.Linq.dll'" "'System.Xml.Linq.dll','WindowsFormsIntegration.dll'"
Replace-One "'Lea.Tests/PrintPreviewMain.cs','Lea.Print/Lea.Print.csproj'" "'Lea.Tests/PrintPreviewMain.cs','Lea.Print/Ph.cs','Lea.Tests/PrintHostMain.cs','Lea.Print/Lea.Print.csproj'"
Replace-One "('`"'+(Join-Path `$out 'Pv.cs')+'`"')" "('`"'+(Join-Path `$out 'Pv.cs')+'`"'),('`"'+(Join-Path `$out 'Ph.cs')+'`"')"
Replace-One '/main:PrintPreviewMain' '/main:PrintHostMain'
Replace-One "('`"'+(Join-Path `$out 'PrintPreviewMain.cs')+'`"')" "('`"'+(Join-Path `$out 'PrintPreviewMain.cs')+'`"'),('`"'+(Join-Path `$out 'PrintHostMain.cs')+'`"')"
Replace-One 'PASS Lea preview and foundation' 'PASS Lea WinForms preview and foundation'
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
