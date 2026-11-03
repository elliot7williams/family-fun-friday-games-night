param(
    [Parameter(Mandatory=$true)][string]$Archive,
    [Parameter(Mandatory=$true)][string]$Directory
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../dist'))
$archivePath = (Resolve-Path -LiteralPath $Archive).ProviderPath
$directoryPath = (Resolve-Path -LiteralPath $Directory).ProviderPath.TrimEnd('\')
foreach ($path in @($archivePath, $directoryPath)) {
    if (!$path.StartsWith($root + '\', [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Only archived dist copies may be compared.'
    }
    $item = Get-Item -LiteralPath $path
    while ($null -ne $item -and $item.FullName.Length -ge $root.Length) {
        if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Reparse paths rejected.' }
        $item = if ($item -is [IO.DirectoryInfo]) { $item.Parent } else { $item.Directory }
    }
}
if (!(Test-Path -LiteralPath $directoryPath -PathType Container)) { throw 'Directory required.' }
$differences = [Collections.Generic.List[string]]::new()
$streamIssues = [Collections.Generic.List[string]]::new()
$seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$sha = [Security.Cryptography.SHA256]::Create()
$archiveStream = [IO.File]::Open($archivePath, 'Open', 'Read', 'Read')
$zip = $null
$files = 0
$bytes = 0L
try {
    $archiveHash = [BitConverter]::ToString($sha.ComputeHash($archiveStream)).Replace('-', '')
    $archiveStream.Position = 0
    $zip = [IO.Compression.ZipArchive]::new($archiveStream, [IO.Compression.ZipArchiveMode]::Read, $true)
    foreach ($entry in $zip.Entries) {
        $relative = $entry.FullName.Replace('/', '\')
        if ([IO.Path]::IsPathRooted($relative) -or $relative.Contains(':') -or
            @($relative.Split('\') | Where-Object { $_ -eq '..' -or $_ -eq '.' }).Count) {
            throw 'Unsafe ZIP entry rejected.'
        }
        if ($relative.EndsWith('\')) { continue }
        if (!$seen.Add($relative)) { throw 'Duplicate ZIP entry rejected.' }
        $target = [IO.Path]::GetFullPath((Join-Path $directoryPath $relative))
        if (!$target.StartsWith($directoryPath + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'ZIP path escapes.' }
        $item = Get-Item -LiteralPath $target -ErrorAction SilentlyContinue
        if ($null -eq $item -or $item.PSIsContainer) { $differences.Add('Missing: ' + $relative); continue }
        $ancestor = $item
        while ($ancestor.FullName.Length -ge $directoryPath.Length) {
            if ($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Extracted reparse entry rejected.' }
            $ancestor = if ($ancestor -is [IO.DirectoryInfo]) { $ancestor.Parent } else { $ancestor.Directory }
        }
        if ($item.Length -ne $entry.Length) { $differences.Add('Length: ' + $relative); continue }
        $packed = $entry.Open()
        $local = $null
        try {
            $local = [IO.File]::Open($target, 'Open', 'Read', 'Read')
            $expected = [BitConverter]::ToString($sha.ComputeHash($packed))
            $actual = [BitConverter]::ToString($sha.ComputeHash($local))
            if ($expected -cne $actual) { $differences.Add('Content: ' + $relative) }
            $files++
            $bytes += $item.Length
        } finally {
            if ($null -ne $local) { $local.Dispose() }
            $packed.Dispose()
        }
    }
    # Reject links before recursive traversal; never follow them into another tree.
    $pending = [Collections.Generic.Stack[string]]::new()
    $pending.Push($directoryPath)
    try {
        try { $rootStreams = @(Get-Item -LiteralPath $directoryPath -Stream * -ErrorAction Stop) }
        catch { $rootStreams = @(Get-Item -LiteralPath ('\\?\' + $directoryPath) -Stream * -ErrorAction Stop) }
        foreach ($stream in $rootStreams) {
            if ($stream.Stream -ne ':$DATA') { $streamIssues.Add('Unarchived stream: .:' + $stream.Stream) }
        }
    } catch { $streamIssues.Add('Stream audit failed: .') }
    while ($pending.Count) {
        foreach ($item in Get-ChildItem -LiteralPath $pending.Pop() -Force) {
            if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Extracted reparse entry rejected.' }
            if ($item.PSIsContainer) { $pending.Push($item.FullName) }
            $relative = $item.FullName.Substring($directoryPath.Length + 1)
            if (!$item.PSIsContainer -and !$seen.Contains($relative)) { $differences.Add('Extra: ' + $relative) }
            try {
                try { $streams = @(Get-Item -LiteralPath $item.FullName -Stream * -ErrorAction Stop) }
                catch { $streams = @(Get-Item -LiteralPath ('\\?\' + $item.FullName) -Stream * -ErrorAction Stop) }
                foreach ($stream in $streams) {
                    if ($stream.Stream -ne ':$DATA') {
                        $streamIssues.Add('Unarchived stream: ' + $relative + ':' + $stream.Stream)
                    }
                }
            } catch { $streamIssues.Add('Stream audit failed: ' + $relative) }
        }
    }
    [pscustomobject]@{
        archive=$archivePath; archiveSha256=$archiveHash; directory=$directoryPath
        contentMatches=($differences.Count -eq 0); comparedFiles=$files; comparedLogicalBytes=$bytes
        differences=$differences.ToArray(); filesystemModified=$false; deletionAuthorized=$false
        streamIssues=$streamIssues.ToArray(); fileStreamsPreserved=($streamIssues.Count -eq 0)
        limitation='Content comparison only; not an ACL, stream, metadata, or concurrent directory snapshot backup. Recheck before any deletion.'
    } | ConvertTo-Json -Depth 4
} finally {
    if ($null -ne $zip) { $zip.Dispose() }
    $archiveStream.Dispose()
    $sha.Dispose()
}
