$ErrorActionPreference = 'Stop'
$workspace = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$target = Join-Path $workspace 'dist/LeaExes/Lea-runtime-225-ready-stage_2026-09-23_20-50-24'
$archive = Join-Path $workspace 'dist/LeaExes/Lea-runtime-225-ready_2026-09-23_20-50-24.zip'
$out = Join-Path $workspace 'continuity-evidence/archive-copy-reclamation-v1'
if (Test-Path -LiteralPath $out) { throw 'Preserve metadata backup evidence.' }
if ((Get-FileHash -LiteralPath $archive).Hash -cne '336CAA66EC15F1B280E61DF2B054B9BA4455EE8F65F7FED598F8A76AD02DA8D7') {
    throw 'Retained archive differs.'
}
$ancestor = Get-Item -LiteralPath $target
while ($null -ne $ancestor) {
    if ($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Reparse ancestor rejected.' }
    $ancestor = $ancestor.Parent
}
$comparison = & (Join-Path $PSScriptRoot 'Test-ArchiveCopy.ps1') -Archive $archive -Directory $target | ConvertFrom-Json
if (!$comparison.contentMatches -or $comparison.comparedFiles -ne 1399 -or
    @($comparison.streamIssues | Where-Object { $_ -like 'Stream audit failed:*' }).Count) {
    throw 'Archive/stream verification incomplete.'
}
$records = [Collections.Generic.List[object]]::new()
$pending = [Collections.Generic.Stack[string]]::new()
$pending.Push($target)
$sha = [Security.Cryptography.SHA256]::Create()
try {
    while ($pending.Count) {
        $directory = $pending.Pop()
        $items = @(Get-Item -LiteralPath $directory) + @(Get-ChildItem -LiteralPath $directory -Force)
        foreach ($item in $items) {
            if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Reparse entry rejected.' }
            if ($item.FullName -ne $directory -and $item.PSIsContainer) { $pending.Push($item.FullName); continue }
            $extended = '\\?\' + $item.FullName
            try { $streams = @(Get-Item -LiteralPath $item.FullName -Stream * -ErrorAction Stop) }
            catch { $streams = @(Get-Item -LiteralPath $extended -Stream * -ErrorAction Stop) }
            $savedStreams = @(foreach ($stream in $streams) {
                if ($stream.Stream -eq ':$DATA') { continue }
                if ($stream.Stream -cnotmatch '^[A-Za-z0-9_.-]+$' -or $stream.Length -gt 1MB) { throw 'Unexpected stream shape/size.' }
                $bytes = [IO.File]::ReadAllBytes($extended + ':' + $stream.Stream)
                if ($bytes.Length -ne $stream.Length) { throw 'Stream changed while reading.' }
                [pscustomobject]@{name=$stream.Stream;length=$bytes.Length;base64=[Convert]::ToBase64String($bytes);
                    sha256=[BitConverter]::ToString($sha.ComputeHash($bytes)).Replace('-', '')}
            })
            $records.Add([pscustomobject]@{
                relative=$item.FullName.Substring($target.Length).TrimStart('\');directory=[bool]$item.PSIsContainer
                length=$(if ($item.PSIsContainer) { 0 } else { $item.Length })
                sha256=$(if ($item.PSIsContainer) { $null } else { (Get-FileHash -LiteralPath $item.FullName).Hash })
                attributes=[int]$item.Attributes;createdUtc=$item.CreationTimeUtc.ToString('o')
                writtenUtc=$item.LastWriteTimeUtc.ToString('o');sddl=(Get-Acl -LiteralPath $item.FullName).Sddl
                streams=$savedStreams
            })
        }
    }
    $backup = [ordered]@{target=$target;archive=$archive;archiveSha256=$comparison.archiveSha256;comparison=$comparison
        records=$records;scope='Default file contents retained in ZIP; alternate-stream bytes and owner/group/DACL/times/attributes in this sidecar, not full NTFS identity/SACL backup.'
        deletedFiles=0}
    $null = New-Item -ItemType Directory -Path $out
    $path = Join-Path $out '225.metadata.json'
    $temporary = $path + '.tmp'
    [IO.File]::WriteAllText($temporary, ($backup | ConvertTo-Json -Depth 10), [Text.UTF8Encoding]::new($false))
    $flush = [IO.File]::Open($temporary, 'Open', 'ReadWrite', 'Read')
    try { $flush.Flush($true) } finally { $flush.Dispose() }
    $readback = Get-Content -LiteralPath $temporary -Raw | ConvertFrom-Json -DateKind String
    if ($readback.records.Count -ne $records.Count) { throw 'Backup readback differs.' }
    $streamCount = 0
    foreach ($record in $readback.records) {
        $original = if ($record.relative) { Join-Path $target $record.relative } else { $target }
        if ((Get-Acl -LiteralPath $original).Sddl -cne $record.sddl) { throw 'Permissions changed during backup.' }
        if (!$record.directory -and (Get-FileHash -LiteralPath $original).Hash -cne $record.sha256) { throw 'File changed during backup.' }
        foreach ($stream in $record.streams) {
            $bytes = [Convert]::FromBase64String($stream.base64)
            $live = [IO.File]::ReadAllBytes('\\?\' + $original + ':' + $stream.name)
            foreach ($data in @($bytes, $live)) {
                if ([BitConverter]::ToString($sha.ComputeHash([byte[]]$data)).Replace('-', '') -cne $stream.sha256) { throw 'Stream verification failed.' }
            }
            $streamCount++
        }
    }
    if ($streamCount -ne 4 -or @($readback.records | Where-Object { !$_.directory }).Count -ne 1399) { throw 'Unexpected metadata inventory.' }
    $again = & (Join-Path $PSScriptRoot 'Test-ArchiveCopy.ps1') -Archive $archive -Directory $target | ConvertFrom-Json
    if (!$again.contentMatches -or @($again.streamIssues | Where-Object { $_ -like 'Stream audit failed:*' }).Count) { throw 'Final archive comparison failed.' }
    [IO.File]::Move($temporary, $path)
    [pscustomobject]@{backup=$path;backupSha256=(Get-FileHash -LiteralPath $path).Hash;files=1399;streams=$streamCount;verified=$true;deletedFiles=0} | ConvertTo-Json
} finally { $sha.Dispose() }
