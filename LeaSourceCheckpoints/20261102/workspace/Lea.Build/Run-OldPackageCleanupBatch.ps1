$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
foreach($version in @(211,210,203,202,201)){
    if([IO.DriveInfo]::new('C:\').AvailableFreeSpace -ge 1250MB){break}
    & (Join-Path $PSScriptRoot 'Reclaim-OldPackage.ps1') -Version $version -Attempt 2 -Apply
    $receipt=Join-Path $workspace ('continuity-evidence/old-package-reclamation-20261026-'+$version+'-v2/result.json')
    $result=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
    if(!$result.verified -or !$result.removed -or $result.error){throw 'Stop cleanup after an unverified package'}
}
