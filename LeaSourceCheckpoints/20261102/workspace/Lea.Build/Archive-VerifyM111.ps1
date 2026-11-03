$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$allowed=[IO.Path]::GetFullPath((Join-Path $root 'dist/LeaExes'))
$target=[IO.Path]::GetFullPath((Join-Path $allowed 'verify-m111-host_2026-09-19_20-13-59'))
$archive=Join-Path $allowed 'Archived-verify-m111-host_2026-09-19_20-13-59.zip'
if(!$target.StartsWith($allowed+'\',[StringComparison]::OrdinalIgnoreCase) -or !(Test-Path $target -PathType Container) -or (Test-Path $archive)){throw 'Fresh explicit historical backup required'}
$walk=Get-Item -LiteralPath $target
while($walk){if($walk.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse ancestor'};$walk=$walk.Parent}
$tree=@(Get-ChildItem -LiteralPath $target -Recurse -Force)
if(@($tree|Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}).Count){throw 'Reparse tree'}
$bytes=($tree|Where-Object {!$_.PSIsContainer}|Measure-Object Length -Sum).Sum
if($bytes -gt 100MB -or (Get-PSDrive C).Free -lt 32MB+2*$bytes){throw 'Bounded backup workspace required; not a build reserve waiver'}
if(@(Get-CimInstance Win32_Process|Where-Object {($_.ExecutablePath -and $_.ExecutablePath.StartsWith($target+'\',[StringComparison]::OrdinalIgnoreCase)) -or ($_.CommandLine -and $_.CommandLine.Contains($target))}).Count){throw 'Old stage in use'}
Add-Type -AssemblyName System.IO.Compression
[IO.Compression.ZipFile]::CreateFromDirectory($target,$archive,[IO.Compression.CompressionLevel]::Optimal,$false)
# The independent reclaimer must verify every file/stream and metadata before removal.
& (Join-Path $PSScriptRoot 'Reclaim-OldPackage.ps1') -Version 210 -Attempt 5 -HistoricalStage VerifyM111Own -Apply
$receipt=Join-Path $root 'continuity-evidence/old-package-reclamation-20261026-210-v5-verifym111own/result.json'
$result=Get-Content $receipt -Raw|ConvertFrom-Json
if(!$result.verified -or !$result.removed -or $result.error){throw 'Archive retained; stage removal not verified'}
