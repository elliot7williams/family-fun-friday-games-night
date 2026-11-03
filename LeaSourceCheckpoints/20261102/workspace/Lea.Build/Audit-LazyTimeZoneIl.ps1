$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$out=Join-Path $root 'il-audit.json'
if(Test-Path $out){throw 'Preserve IL audit'}
$buildPath=Join-Path $root 'build-v1/result.json'
if((Get-FileHash $buildPath).Hash -cne '7D82A029AA5977798CF8B323920D606E21DFE4D803243914378EC1CAD8761D29'){throw 'Build differs'}
$build=Get-Content $buildPath -Raw|ConvertFrom-Json
if(!$build.passed -or !$build.inputsUnchanged){throw 'Build incomplete'}
foreach($row in $build.runs){if((Get-FileHash $row.binary).Hash -cne $row.sha256){throw 'Binary changed'}}
$helper=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/ParentResourceIlAudit.cs'
$refs=@(Get-ChildItem (Join-Path $PSHOME 'ref') -File -Filter '*.dll'|ForEach-Object FullName)
Add-Type -Path @($helper,(Join-Path $root 'TemporalIlDelta.cs')) -ReferencedAssemblies $refs -CompilerOptions '/nowarn:1701'
$a=@($build.runs|Where-Object name -ceq 'control');$b=@($build.runs|Where-Object name -ceq 'lazy')
if($a.Count -ne 1 -or $b.Count -ne 1){throw 'Pair incomplete'}
$result=[TemporalIlDelta]::Compare($a[0].binary,$b[0].binary)
$result|ConvertTo-Json -Depth 6|Set-Content $out
$result|ConvertTo-Json -Depth 6
if(!$result.passed){throw 'Scoped IL audit failed; retain raw differences'}
