$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$root=Join-Path $workspace 'dist/LeaExes'
foreach($version in @(218,219,220,221,222,224)){
    if([IO.DriveInfo]::new('C:\').AvailableFreeSpace -lt 512MB){throw 'Preserve build/storage reserve'}
    $choices=@(Get-ChildItem -LiteralPath $root -Directory | Where-Object Name -match ('^Lea-runtime-'+$version+'-ready-stage_2026-09-\d{2}_\d{2}-\d{2}-\d{2}$'))
    if($choices.Count -ne 1){throw 'Ambiguous old package'}
    $target=$choices[0].FullName
    if(@(Get-ChildItem -LiteralPath $target -Recurse -Force | Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}).Count){throw 'Reparse tree'}
    $ancestor=Get-Item -LiteralPath $target
    while($ancestor){if($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse ancestor'};$ancestor=$ancestor.Parent}
    $zip=Join-Path $root ($choices[0].Name.Replace('-ready-stage_','-ready_')+'.zip')
    if(Test-Path -LiteralPath $zip){throw 'Preserve existing archive; inspect before resuming'}
    [IO.Compression.ZipFile]::CreateFromDirectory($target,$zip,[IO.Compression.CompressionLevel]::Optimal,$false)
    & (Join-Path $PSScriptRoot 'Reclaim-OldPackage.ps1') -Version $version -Attempt 2 -Apply
    $receipt=Join-Path $workspace ('continuity-evidence/old-package-reclamation-20261026-'+$version+'-v2/result.json')
    $result=Get-Content -LiteralPath $receipt -Raw | ConvertFrom-Json
    if(!$result.verified -or !$result.removed -or $result.error){throw 'Stop on failed archive verification/removal'}
}
