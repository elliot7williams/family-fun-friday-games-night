param([ValidateSet(1,2)][int]$Version=2)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-PrintWpfProbe.generated.ps1'
$target=Join-Path $PSScriptRoot $(if($Version -eq 1){'Test-PrintWpfLayout.generated.ps1'}else{'Test-PrintWpfLayoutV2.generated.ps1'})
$out=Join-Path $workspace ('continuity-evidence/print-wpf-layout-preparation-v'+$Version)
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve WPF layout preparation'}
if((Get-FileHash $source).Hash -cne '37BF2F6DDEEB677078C016D7502D302B8B563CBFDA853B800A16B69092FD0FA0'){throw 'Original runner differs'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-One([string]$Before,[string]$After){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne 1){throw ('Unique anchor required: '+$Before)}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After))
}
Replace-One 'print-wpf-probe-v1' ('print-wpf-layout-v'+$Version)
Replace-One 'Owned WPF vector glyph fallback diagnostic only; no foundation rerun, pagination, actual printer, renderer adoption or host qualification' 'Owned bounded WPF selected-text pagination/vector drawing plus foundation73; no actual printer, full-document renderer or Firespider adoption'
Replace-One "'Lea.Tests/PrintWpfProbe.cs','Lea.Print/Lea.Print.csproj'" "'Lea.Tests/PrintWpfProbe.cs','Lea.Print/Pw.cs','Lea.Tests/PrintWpfLayoutMain.cs','Lea.Print/Lea.Print.csproj'"
Replace-One "@('PresentationCore.dll')" "@('PresentationCore.dll','PresentationFramework.dll','System.Xaml.dll')"
Replace-One "('`"'+(Join-Path `$out 'Pt.cs')+'`"')" "('`"'+(Join-Path `$out 'Pt.cs')+'`"'),('`"'+(Join-Path `$out 'Pw.cs')+'`"')"
Replace-One '/main:PrintWpfProbe' '/main:PrintWpfLayoutMain'
Replace-One "('`"'+(Join-Path `$out 'PrintWpfProbe.cs')+'`"')" "('`"'+(Join-Path `$out 'PrintWpfProbe.cs')+'`"'),('`"'+(Join-Path `$out 'PrintWpfLayoutMain.cs')+'`"')"
Replace-One 'PASS Lea WPF glyph diagnostic generated' 'PASS Lea paginated WPF text and foundation'
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
