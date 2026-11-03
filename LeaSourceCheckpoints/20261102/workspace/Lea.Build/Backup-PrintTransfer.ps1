param([switch]$Submission,[switch]$Render,[switch]$Safety,[switch]$Fault)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
if ((Get-PSDrive C).Free -lt 512MB) { throw 'Storage reserve required' }
$paths = @('Lea.Tests/PrintPackageTransfer.cpp','Lea.Tests/PrintPackageLease.cs',
    'Lea.Build/Test-PrintPackageTransfer.ps1','Lea.Build/Test-PrintPackageStream.ps1',
    'Lea.Build/Backup-PrintTransfer.ps1','Lea.Build/OwnedTestProcess.cs',
    'Lea.Build/Invoke-OwnedTestRun.ps1','PROJECT_CONTEXT.md',
    'LeaForge/PRINT_PACKAGE_TARGET.md','LeaForge/PROGRESS.md')
$paths += @(Get-ChildItem (Join-Path $root 'continuity-evidence/print-package-transfer-v1') -File | ForEach-Object { [IO.Path]::GetRelativePath($root, $_.FullName) })
$paths += 'continuity-evidence/print-ticket-project-v1/bin/prepared-ticket.xps'
if ($Submission) {
    $paths += @('Lea.Build/Test-PrintPackageCancel.ps1','Lea.Build/Audit-PrintTransferOutput.ps1')
    $paths += @(Get-ChildItem (Join-Path $root 'continuity-evidence/print-package-cancel-v5') -File | ForEach-Object { [IO.Path]::GetRelativePath($root, $_.FullName) })
}
if ($Render) {
    $paths += @('Lea.Tests/RenderPrintTransfer.cs','Lea.Tests/RenderPrintTransfer.ps1',
        'Lea.Build/Test-PrintTransferRender.ps1','Lea.Build/Restore-PrintTestDefault.ps1','Lea.Build/Audit-RestoredDefault.ps1')
    foreach ($version in 1..4) {
        $paths += @(Get-ChildItem (Join-Path $root ('continuity-evidence/print-transfer-render-v'+$version)) -File | ForEach-Object { [IO.Path]::GetRelativePath($root, $_.FullName) })
    }
}
if ($Safety) {
    $paths += @('Lea.Tests/PrintDefaultSafetyMain.cs','Lea.Build/Test-PrintDefaultSafety.ps1')
    foreach ($folder in @('print-default-safety-v1','print-default-net48-v1','print-default-net48-v2','print-default-net48-v3','print-default-net48-v4')) {
        $paths += @(Get-ChildItem (Join-Path $root ('continuity-evidence/'+$folder)) -File | ForEach-Object { [IO.Path]::GetRelativePath($root, $_.FullName) })
    }
}
$name = if ($Fault) { 'print-fault-source-20261028.zip' } elseif ($Safety) { 'print-safety-source-20261028.zip' } elseif ($Render) { 'print-render-source-20261028.zip' } elseif ($Submission) { 'print-submission-source-20261028.zip' } else { 'print-transfer-source-20261028.zip' }
$destination = Join-Path $root ('continuity-evidence/' + $name)
if (Test-Path $destination) { throw 'Preserve existing archive' }
Add-Type -AssemblyName System.IO.Compression
$archive = [IO.Compression.ZipFile]::Open($destination, 'Create')
try {
    foreach ($relative in $paths) {
        $source = Join-Path $root $relative
        $null = [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $source, $relative.Replace('\','/'), 'Optimal')
    }
} finally { $archive.Dispose() }
$archive = [IO.Compression.ZipFile]::OpenRead($destination)
try {
    if ($archive.Entries.Count -ne $paths.Count) { throw 'Archive inventory differs' }
    foreach ($entry in $archive.Entries) {
        $stream = $entry.Open()
        try { $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) } finally { $stream.Dispose() }
        if ($hash -cne (Get-FileHash (Join-Path $root $entry.FullName)).Hash) { throw "Archive entry differs: $($entry.FullName)" }
    }
} finally { $archive.Dispose() }
[pscustomobject]@{ path = $destination; files = $paths.Count; sha256 = (Get-FileHash $destination).Hash; bytes = (Get-Item $destination).Length }
