$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-PrintPaginator.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintXps.generated.ps1'
$out=Join-Path $workspace 'continuity-evidence/print-xps-preparation-v1'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve XPS preparation'}
if((Get-FileHash $source).Hash -cne 'E53C97A11F66806023B6AC451EC9A2C75D7F8168CDA76E372763A9D347ABC99F'){throw 'Original runner differs'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-One([string]$Before,[string]$After){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne 1){throw ('Unique anchor required: '+$Before)}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After))
}
Replace-One 'print-paginator-v1' 'print-xps-v1'
Replace-One 'Owned fixed WPF paginator plus frame and prior foundation; no XPS export, printer or host adoption' 'Owned empty-stream XPS export/readback plus paginator/frame/foundation; no printer or host adoption'
Replace-One "'PresentationCore.dll','PresentationFramework.dll','System.Xaml.dll'" "'PresentationCore.dll','PresentationFramework.dll','System.Xaml.dll','ReachFramework.dll','System.Printing.dll','System.Xml.dll','System.Xml.Linq.dll'"
Replace-One "'Lea.Tests/PrintPaginatorMain.cs','Lea.Print/Lea.Print.csproj'" "'Lea.Tests/PrintPaginatorMain.cs','Lea.Print/Px.cs','Lea.Tests/PrintXpsMain.cs','Lea.Print/Lea.Print.csproj'"
Replace-One "('`"'+(Join-Path `$out 'Pp.cs')+'`"')" "('`"'+(Join-Path `$out 'Pp.cs')+'`"'),('`"'+(Join-Path `$out 'Px.cs')+'`"')"
Replace-One '/main:PrintPaginatorMain' '/main:PrintXpsMain'
Replace-One "('`"'+(Join-Path `$out 'PrintPaginatorMain.cs')+'`"')" "('`"'+(Join-Path `$out 'PrintPaginatorMain.cs')+'`"'),('`"'+(Join-Path `$out 'PrintXpsMain.cs')+'`"')"
Replace-One 'PASS Lea paginator and foundation' 'PASS Lea XPS export and foundation'
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
