$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$out=[IO.Path]::GetFullPath((Join-Path $root 'continuity-evidence/solid-build-archives-v1'))
$path=Join-Path $out 'scratch-cleanup.json'
if(Test-Path $path){throw 'Preserve cleanup evidence'}
$r=Get-Content (Join-Path $out 'result.json') -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.roundTripVerified -or $r.originalArchivesRemoved){throw 'Passing non-destructive experiment required'}
$pack=Join-Path $out 'older-builds.7z'
if((Get-FileHash $pack).Hash -cne $r.containerSha256){throw 'Verified container differs'}
$restore=[IO.Path]::GetFullPath((Join-Path $out 'restored'))
if(!$restore.StartsWith($out+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Scratch root escape'}
$walk=Get-Item -LiteralPath $restore
while($walk){if($walk.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Scratch ancestor link'};$walk=$walk.Parent}
$removed=[Collections.Generic.List[object]]::new()
foreach($input in $r.inputs){
    if($input.name -notin @('Lea-runtime-210-ready_2026-09-19_16-01-25.zip','Lea-runtime-211-ready_2026-09-19_20-45-48.zip')){throw 'Unexpected scratch name'}
    $scratch=[IO.Path]::GetFullPath((Join-Path $restore $input.name))
    $original=[IO.Path]::GetFullPath((Join-Path $root ('dist/LeaExes/'+$input.name)))
    if(!$scratch.StartsWith($restore+'\',[StringComparison]::OrdinalIgnoreCase) -or $original -cne $input.path){throw 'Scratch/original identity differs'}
    if((Get-Item -LiteralPath $scratch).Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Scratch file link'}
    $hold=[IO.File]::Open($original,'Open','Read','Read')
    try{
        if((Get-FileHash $original).Hash -cne $input.sha256 -or (Get-FileHash $scratch).Hash -cne $input.sha256){throw 'Original/scratch bytes differ'}
        Remove-Item -LiteralPath $scratch -Force
        if(Test-Path -LiteralPath $scratch){throw 'Scratch still present'}
        $removed.Add(@{path=$scratch;bytes=$input.length;originalPreserved=$original;sha256=$input.sha256})
    }finally{$hold.Dispose()}
}
$result=@{verified=$true;removedScratch=$removed;originalArchivesRemoved=$false;containerSha256=$r.containerSha256;freeAfter=(Get-PSDrive C).Free}
[IO.File]::WriteAllText($path,($result|ConvertTo-Json -Depth 5));$result|ConvertTo-Json -Depth 5
