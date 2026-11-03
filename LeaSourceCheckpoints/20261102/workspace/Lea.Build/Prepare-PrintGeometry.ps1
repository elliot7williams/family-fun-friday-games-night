$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-PrintSnapshotFramework.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintGeometry.generated.ps1'
$out=Join-Path $workspace 'continuity-evidence/print-geometry-preparation-v1'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve geometry preparation'}
if((Get-FileHash $source).Hash -cne 'C7E76DDAA782D417B3E926F968BE19CED33349C7663FCBB150A77AA4C593FB33'){throw 'Original runner differs'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-One([string]$Before,[string]$After){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne 1){throw ('Unique anchor required: '+$Before)}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After))
}
Replace-One 'print-snapshot-framework-v1' 'print-geometry-framework-v1'
Replace-One 'Owned Framework library/test build and pure managed snapshot EXE; no view/host integration, printer output, visual or release qualification' 'Owned Framework snapshot plus options/geometry tests; no renderer, host options, pagination, printer or release qualification'
Replace-One "@('Lea.Print/Ps.cs','Lea.Tests/PrintSnapshot.cs','Lea.Tests/PrintSnapshotMain.cs','Lea.Print/Lea.Print.csproj')" "@('Lea.Print/Ps.cs','Lea.Print/Po.cs','Lea.Tests/PrintSnapshot.cs','Lea.Tests/PrintGeometry.cs','Lea.Tests/PrintGeometryMain.cs','Lea.Print/Lea.Print.csproj')"
Replace-One "('`"'+(Join-Path `$out 'Ps.cs')+'`"')" "('`"'+(Join-Path `$out 'Ps.cs')+'`"'),('`"'+(Join-Path `$out 'Po.cs')+'`"')"
Replace-One "'PrintSnapshotChecks.exe'" "'PrintGeometryChecks.exe'"
Replace-One "('`"'+(Join-Path `$out 'PrintSnapshotMain.cs')+'`"')" "('`"'+(Join-Path `$out 'PrintGeometry.cs')+'`"'),('`"'+(Join-Path `$out 'PrintGeometryMain.cs')+'`"')"
Replace-One 'PASS Lea Framework print snapshot: 40 checks' 'PASS Lea print snapshot and geometry: 60 checks'
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
