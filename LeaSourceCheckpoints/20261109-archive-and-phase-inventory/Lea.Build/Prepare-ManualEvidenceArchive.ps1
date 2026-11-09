param(
    [string]$Source = 'C:\Users\Elliot\Documents\Family Fun Friday Games Night copy\continuity-evidence\temporal-startup-v1',
    [string]$Destination = 'I:\LeaBuildStorage\temporal-startup-manual-cleanup-20261109-v1'
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression
$root = (Get-Item -LiteralPath $Source).FullName.TrimEnd('\')
if (Test-Path -LiteralPath $Destination) { throw 'Destination already exists; preserve it.' }
$items = @(Get-Item -LiteralPath $root) + @(Get-ChildItem -LiteralPath $root -Recurse -Force)
if (@($items | Where-Object { $_.Attributes -band [IO.FileAttributes]::ReparsePoint }).Count) { throw 'Reparse point found.' }
$active = @(Get-CimInstance Win32_Process | Where-Object { $_.ExecutablePath -and $_.ExecutablePath.StartsWith($root+'\',[StringComparison]::OrdinalIgnoreCase) })
if ($active.Count) { throw 'An executable is running from the source.' }
$files = @($items | Where-Object { !$_.PSIsContainer })
$bytes = ($files | Measure-Object Length -Sum).Sum
if ((Get-PSDrive I).Free -lt ($bytes + 512MB)) { throw 'Insufficient archive space.' }
New-Item -ItemType Directory -Path $Destination | Out-Null
$records = @()
foreach ($item in $items) {
    $relative = if ($item.FullName -eq $root) { '' } else { $item.FullName.Substring($root.Length+1) }
    if (!$item.PSIsContainer) {
        $streams = @(Get-Item -LiteralPath $item.FullName -Stream * | Where-Object { $_.Stream -ne ':$DATA' })
        if ($streams.Count) { throw "Alternate streams require separate preservation: $relative" }
    }
    $records += [pscustomobject]@{ Path=$relative; Directory=[bool]$item.PSIsContainer; Length=if($item.PSIsContainer){0}else{$item.Length}; SHA256=if($item.PSIsContainer){$null}else{(Get-FileHash -LiteralPath $item.FullName -Algorithm SHA256).Hash}; Sddl=(Get-Acl -LiteralPath $item.FullName).Sddl; Attributes=[int]$item.Attributes; CreationUtc=$item.CreationTimeUtc.ToString('o'); LastWriteUtc=$item.LastWriteTimeUtc.ToString('o') }
}
$zipPath = Join-Path $Destination 'temporal-startup-v1.zip'
$stream = [IO.File]::Open($zipPath,[IO.FileMode]::CreateNew)
$zip = [IO.Compression.ZipArchive]::new($stream,[IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($record in $records) {
        if (!$record.Path) { continue }
        $name = $record.Path.Replace('\','/')
        if ($record.Directory) { $null=$zip.CreateEntry($name+'/'); continue }
        $entry=$zip.CreateEntry($name,[IO.Compression.CompressionLevel]::Optimal)
        $input=[IO.File]::OpenRead((Join-Path $root $record.Path)); $output=$entry.Open()
        try { $input.CopyTo($output) } finally { $output.Dispose(); $input.Dispose() }
    }
} finally { $zip.Dispose(); $stream.Dispose() }
$stream=[IO.File]::OpenRead($zipPath); $zip=[IO.Compression.ZipArchive]::new($stream,[IO.Compression.ZipArchiveMode]::Read)
try {
    if ($zip.Entries.Count -ne ($records.Count-1)) { throw 'Archive entry count mismatch.' }
    foreach ($record in $records) {
        if ($record.Directory) { continue }
        $entry=$zip.GetEntry($record.Path.Replace('\','/'))
        if (!$entry -or $entry.Length -ne $record.Length) { throw 'Archive length mismatch.' }
        $input=$entry.Open(); $sha=[Security.Cryptography.SHA256]::Create()
        try { $hash=[BitConverter]::ToString($sha.ComputeHash($input)).Replace('-','') } finally { $input.Dispose(); $sha.Dispose() }
        if ($hash -ne $record.SHA256) { throw 'Archive content mismatch.' }
    }
} finally { $zip.Dispose(); $stream.Dispose() }
$after=@(Get-Item -LiteralPath $root)+@(Get-ChildItem -LiteralPath $root -Recurse -Force)
if ($after.Count -ne $records.Count) { throw 'Source inventory changed.' }
foreach ($record in $records) {
    $path=if($record.Path){Join-Path $root $record.Path}else{$root}
    $item=Get-Item -LiteralPath $path -Force
    if ((Get-Acl -LiteralPath $path).Sddl -ne $record.Sddl -or [int]$item.Attributes -ne $record.Attributes -or $item.LastWriteTimeUtc.ToString('o') -ne $record.LastWriteUtc) { throw 'Source metadata changed.' }
    if (!$record.Directory -and (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $record.SHA256) { throw 'Source bytes changed.' }
}
$result=[ordered]@{ Verified=$true; Source=$root; OriginalUntouched=$true; Files=$files.Count; LogicalBytes=$bytes; Archive=$zipPath; ArchiveSHA256=(Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash; Records=$records; RestoreNote='ZIP does not carry NTFS ACLs. Restore to the original NTFS paths and reapply recorded SDDL, attributes and timestamps before historical replay. This is not a qualified native test stage. No files were deleted.' }
$result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $Destination 'verified-manifest.json') -Encoding UTF8
[pscustomobject]$result | Select-Object Verified,Source,OriginalUntouched,Files,LogicalBytes,Archive,ArchiveSHA256 | Format-List
