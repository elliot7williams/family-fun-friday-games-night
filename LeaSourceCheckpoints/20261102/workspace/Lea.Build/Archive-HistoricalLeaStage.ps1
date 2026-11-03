param([Parameter(Mandatory)][ValidateSet('Host185','Host185Second','Work186')][string]$Stage)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$stages=@{
    Host185=@(185,'dist/LeaExes','Lea-runtime-185-host-stage','Archived-Lea-runtime-185-host-stage.zip')
    Host185Second=@(185,'dist/LeaExes','Lea-runtime-185-host-stage2','Archived-Lea-runtime-185-host-stage2.zip')
    Work186=@(186,'dist','.runtime186-work','Archived-runtime186-work.zip')
}
$selected=$stages[$Stage]
$allowed=[IO.Path]::GetFullPath((Join-Path $root $selected[1]))
$target=[IO.Path]::GetFullPath((Join-Path $allowed $selected[2]))
$archive=Join-Path $allowed $selected[3]
if(!$target.StartsWith($allowed+'\',[StringComparison]::OrdinalIgnoreCase) -or !(Test-Path $target -PathType Container) -or (Test-Path $archive)){throw 'Fresh explicit historical backup required'}
$walk=Get-Item -LiteralPath $target
while($walk){if($walk.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse ancestor'};$walk=$walk.Parent}
$tree=@(Get-ChildItem -LiteralPath $target -Recurse -Force)
if(@($tree|Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}).Count){throw 'Reparse tree'}
$bytes=($tree|Where-Object {!$_.PSIsContainer}|Measure-Object Length -Sum).Sum
if($bytes -gt 150MB -or (Get-PSDrive C).Free -lt 32MB+2*$bytes){throw 'Bounded archive workspace required; not a build reserve waiver'}
if(@(Get-CimInstance Win32_Process|Where-Object {($_.ExecutablePath -and $_.ExecutablePath.StartsWith($target+'\',[StringComparison]::OrdinalIgnoreCase)) -or ($_.CommandLine -and $_.CommandLine.Contains($target))}).Count){throw 'Old stage in use'}
Add-Type -AssemblyName System.IO.Compression
[IO.Compression.ZipFile]::CreateFromDirectory($target,$archive,[IO.Compression.CompressionLevel]::Optimal,$false)
& (Join-Path $PSScriptRoot 'Reclaim-OldPackage.ps1') -Version $selected[0] -Attempt 6 -HistoricalStage $Stage -Apply
$receipt=Join-Path $root ('continuity-evidence/old-package-reclamation-20261026-'+$selected[0]+'-v6-'+$Stage.ToLowerInvariant()+'/result.json')
$result=Get-Content $receipt -Raw|ConvertFrom-Json
if(!$result.verified -or !$result.removed -or $result.error){throw 'Archive retained; stage removal not verified'}
