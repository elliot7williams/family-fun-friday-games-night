$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-PrintWpfLayoutV2.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintFrame.generated.ps1'
$out=Join-Path $workspace 'continuity-evidence/print-frame-preparation-v1'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve frame preparation'}
if((Get-FileHash $source).Hash -cne '4F69DEE677EEA7131CFFF6308F2B11DC54E28B39C5C56911534A82ECDFE5ECC1'){throw 'Original runner differs'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-One([string]$Before,[string]$After){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne 1){throw ('Unique anchor required: '+$Before)}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After))
}
Replace-One 'print-wpf-layout-v2' 'print-frame-v1'
Replace-One 'Owned bounded WPF selected-text pagination/vector drawing plus foundation73; no actual printer, full-document renderer or Firespider adoption' 'Owned WPF selected-text frame/header/footer and full prior foundation/layout/guards; no actual printer, full-document renderer or Firespider adoption'
Replace-One "'Lea.Tests/PrintWpfLayoutMain.cs','Lea.Print/Lea.Print.csproj'" "'Lea.Tests/PrintWpfLayoutMain.cs','Lea.Print/Pf.cs','Lea.Tests/PrintFrameMain.cs','Lea.Print/Lea.Print.csproj'"
Replace-One "('`"'+(Join-Path `$out 'Pw.cs')+'`"')" "('`"'+(Join-Path `$out 'Pw.cs')+'`"'),('`"'+(Join-Path `$out 'Pf.cs')+'`"')"
Replace-One '/main:PrintWpfLayoutMain' '/main:PrintFrameMain'
Replace-One "('`"'+(Join-Path `$out 'PrintWpfLayoutMain.cs')+'`"')" "('`"'+(Join-Path `$out 'PrintWpfLayoutMain.cs')+'`"'),('`"'+(Join-Path `$out 'PrintFrameMain.cs')+'`"')"
Replace-One 'PASS Lea paginated WPF text and foundation' 'PASS Lea print frame and foundation'
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
