$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-PrintFrame.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintPaginator.generated.ps1'
$out=Join-Path $workspace 'continuity-evidence/print-paginator-preparation-v1'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve paginator preparation'}
if((Get-FileHash $source).Hash -cne 'D094716B236DDF26385BE0F302B1A2CCC3C8FC335AB8BFECB3D73BA6ACBBC20B'){throw 'Original runner differs'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-One([string]$Before,[string]$After){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne 1){throw ('Unique anchor required: '+$Before)}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After))
}
Replace-One 'print-frame-v1' 'print-paginator-v1'
Replace-One 'Owned WPF selected-text frame/header/footer and full prior foundation/layout/guards; no actual printer, full-document renderer or Firespider adoption' 'Owned fixed WPF paginator plus frame and prior foundation; no XPS export, printer or host adoption'
Replace-One "'Lea.Tests/PrintFrameMain.cs','Lea.Print/Lea.Print.csproj'" "'Lea.Tests/PrintFrameMain.cs','Lea.Print/Pp.cs','Lea.Tests/PrintPaginatorMain.cs','Lea.Print/Lea.Print.csproj'"
Replace-One "('`"'+(Join-Path `$out 'Pf.cs')+'`"')" "('`"'+(Join-Path `$out 'Pf.cs')+'`"'),('`"'+(Join-Path `$out 'Pp.cs')+'`"')"
Replace-One '/main:PrintFrameMain' '/main:PrintPaginatorMain'
Replace-One "('`"'+(Join-Path `$out 'PrintFrameMain.cs')+'`"')" "('`"'+(Join-Path `$out 'PrintFrameMain.cs')+'`"'),('`"'+(Join-Path `$out 'PrintPaginatorMain.cs')+'`"')"
Replace-One 'PASS Lea print frame and foundation' 'PASS Lea paginator and foundation'
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
