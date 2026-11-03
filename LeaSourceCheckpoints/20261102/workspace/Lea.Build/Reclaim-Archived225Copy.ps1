param([switch]$Apply)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$allowed=[IO.Path]::GetFullPath((Join-Path $workspace 'dist/LeaExes'))
$expected=[IO.Path]::GetFullPath((Join-Path $allowed 'Lea-runtime-225-ready-stage_2026-09-23_20-50-24'))
$metadata=Join-Path $workspace 'continuity-evidence/archive-copy-reclamation-v1/225.metadata.json'
if((Get-FileHash $metadata).Hash -cne '2F6D75AD6F373199FDC3E3C9BCFD23CF6864D0CECAC88B5C0E088BD77A2FAFC0'){throw 'Metadata backup differs'}
$backup=Get-Content $metadata -Raw|ConvertFrom-Json -DateKind String
$target=(Resolve-Path -LiteralPath $backup.target).ProviderPath
if($target -cne $expected -or !$target.StartsWith($allowed+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Deletion target escapes exact archive scope'}
$archive=$backup.archive
if((Get-FileHash $archive).Hash -cne $backup.archiveSha256){throw 'ZIP backup differs'}
$ancestor=Get-Item -LiteralPath $target
while($null -ne $ancestor){if($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse ancestor'};$ancestor=$ancestor.Parent}
$items=@(Get-ChildItem -LiteralPath $target -Recurse -Force)
if(@($items|Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}).Count){throw 'Reparse entry'}
if($items.Count+1 -ne $backup.records.Count){throw 'Tree inventory changed'}
$names=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($item in $items){$null=$names.Add($item.FullName.Substring($target.Length+1))}
foreach($record in $backup.records){if($record.relative -and !$names.Contains($record.relative)){throw 'Tree entry changed'}}
if(@(Get-CimInstance Win32_Process|Where-Object {$_.ExecutablePath -and $_.ExecutablePath.StartsWith($target+'\',[StringComparison]::OrdinalIgnoreCase)}).Count){throw 'An archived executable is running'}
$leases=[Collections.Generic.List[IDisposable]]::new()
$sha=[Security.Cryptography.SHA256]::Create()
$result=[ordered]@{verified=$false;removed=$false;target=$target;archive=$archive;metadata=$metadata;error=$null}
$intent=Join-Path (Split-Path $metadata) '225.removal-intent.json'
if($Apply -and (Test-Path -LiteralPath $intent)){throw 'Preserve prior removal attempt'}
try{
    $flush=[IO.File]::Open($metadata,'Open','ReadWrite','Read');try{$flush.Flush($true)}finally{$flush.Dispose()}
    $leases.Add([IO.File]::Open($archive,'Open','Read','Read'))
    $leases.Add([IO.File]::Open($metadata,'Open','Read','Read'))
    foreach($record in $backup.records){
        $path=if($record.relative){Join-Path $target $record.relative}else{$target}
        $item=Get-Item -LiteralPath $path
        if([int]$item.Attributes -ne $record.attributes -or $item.LastWriteTimeUtc.ToString('o') -cne $record.writtenUtc -or
            (Get-Acl -LiteralPath $path).Sddl -cne $record.sddl){throw 'Metadata changed since backup'}
        if(!$record.directory){
            $file=[IO.File]::Open('\\?\'+$path,'Open','Read','Read');$leases.Add($file)
            if($file.Length -ne $record.length -or [BitConverter]::ToString($sha.ComputeHash($file)).Replace('-','') -cne $record.sha256){throw 'File content changed since backup'}
        }
        $streams=@(Get-Item -LiteralPath ('\\?\'+$path) -Stream * -ErrorAction Stop|Where-Object Stream -ne ':$DATA')
        if($streams.Count -ne @($record.streams).Count){throw 'Stream inventory changed'}
        foreach($stream in $record.streams){
            if($stream.name -cnotmatch '^[A-Za-z0-9_.-]+$'){throw 'Unsafe stream name'}
            $data=[Convert]::FromBase64String($stream.base64)
            $file=[IO.File]::Open('\\?\'+$path+':'+$stream.name,'Open','Read','Read');$leases.Add($file)
            if($file.Length -ne $stream.length -or [BitConverter]::ToString($sha.ComputeHash($file)).Replace('-','') -cne $stream.sha256 -or
                [BitConverter]::ToString($sha.ComputeHash($data)).Replace('-','') -cne $stream.sha256){throw 'Stream backup differs'}
        }
    }
    $comparison=& (Join-Path $PSScriptRoot 'Test-ArchiveCopy.ps1') -Archive $archive -Directory $target|ConvertFrom-Json
    if(!$comparison.contentMatches -or @($comparison.streamIssues|Where-Object {$_ -like 'Stream audit failed:*'}).Count){throw 'ZIP comparison failed'}
    $result.verified=$true
    if($Apply){
        $result.freeBefore=[IO.DriveInfo]::new('C:\').AvailableFreeSpace
        $result|ConvertTo-Json|Set-Content -LiteralPath $intent
        $flush=[IO.File]::Open($intent,'Open','ReadWrite','Read');try{$flush.Flush($true)}finally{$flush.Dispose()}
        # Release tree read locks only after verification; retain ZIP/sidecar locks.
        for($i=$leases.Count-1;$i -ge 2;$i--){$leases[$i].Dispose();$leases.RemoveAt($i)}
        if([IO.Path]::GetFullPath($target) -cne $expected){throw 'Final absolute target differs'}
        Remove-Item -LiteralPath $target -Recurse -Force
        $result.removed=!(Test-Path -LiteralPath $target)
        $result.freeAfter=[IO.DriveInfo]::new('C:\').AvailableFreeSpace
        if(!$result.removed -or (Get-FileHash $archive).Hash -cne $backup.archiveSha256){throw 'Removal/retained ZIP check failed'}
    }
}catch{$result.error=$_.Exception.ToString()}
finally{
    foreach($lease in $leases){$lease.Dispose()}
    $sha.Dispose()
    if($Apply){$result|ConvertTo-Json -Depth 4|Set-Content (Join-Path (Split-Path $metadata) '225.removal-result.json')}
}
[pscustomobject]$result|ConvertTo-Json -Depth 4
if(!$result.verified -or $result.error){throw 'Reclamation incomplete; preserve evidence'}
