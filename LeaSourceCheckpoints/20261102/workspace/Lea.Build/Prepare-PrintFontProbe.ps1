$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-PrintText.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintFontProbe.generated.ps1'
$out=Join-Path $workspace 'continuity-evidence/print-font-preparation-v1'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve font preparation'}
if((Get-FileHash $source).Hash -cne '1E4D3B811CA13C2611961A84F412B2B69432B206E7EDA5337698EB6A7F4A81C5'){throw 'Original runner differs'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-One([string]$Before,[string]$After){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne 1){throw ('Unique anchor required: '+$Before)}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After))
}
Replace-One 'print-text-view-v1' 'print-font-probe-v1'
Replace-One 'Owned Framework native selected-text pagination/drawing and foundation; bitmap output only, no full-document renderer, host dispatch, actual printer or release qualification' 'Owned font diagnostic only: compare native GDI+ and GDI font fallback. Does not run foundation suite, qualify renderer or change production engine'
Replace-One "'Lea.Tests/PrintTextMain.cs','Lea.Print/Lea.Print.csproj'" "'Lea.Tests/PrintTextMain.cs','Lea.Tests/PrintFontProbe.cs','Lea.Print/Lea.Print.csproj'"
Replace-One '/main:PrintTextMain' '/main:PrintFontProbe'
Replace-One "('`"'+(Join-Path `$out 'PrintTextMain.cs')+'`"')" "('`"'+(Join-Path `$out 'PrintTextMain.cs')+'`"'),('`"'+(Join-Path `$out 'PrintFontProbe.cs')+'`"')"
Replace-One 'PASS Lea native text print and foundation' 'PASS Lea print font diagnostic generated'
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
