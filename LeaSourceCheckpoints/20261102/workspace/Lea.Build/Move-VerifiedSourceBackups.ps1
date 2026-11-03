$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$sourceRoot=Join-Path $workspace 'continuity-evidence'
$destination=[IO.Path]::GetFullPath('L:\Lea-source-backup-archive-20261027')
$names=@('native-selection-script-source-20261027.zip','print-styles-source-20261027.zip','print-styled-document-source-20261027.zip','print-live-capture-source-20261027.zip','print-document-source-20261027.zip','print-xps-source-20261027.zip','print-host-source-20261027.zip','print-preview-source-20261027.zip')
if($destination -cne 'L:\Lea-source-backup-archive-20261027' -or (Test-Path -LiteralPath $destination)){throw 'Preserve existing destination'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Firespider.Boot','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap browser/tests'}
function Assert-PlainPath([string]$Path){
    $walk=[IO.Path]::GetFullPath($Path)
    while($walk){
        if((Test-Path -LiteralPath $walk) -and ((Get-Item -LiteralPath $walk -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)){throw 'Linked archive path refused'}
        $walk=[IO.Path]::GetDirectoryName($walk)
    }
}
Assert-PlainPath $destination
$items=@(foreach($name in $names){
    if([IO.Path]::GetFileName($name) -cne $name){throw 'Archive leaf required'}
    $path=[IO.Path]::GetFullPath((Join-Path $sourceRoot $name))
    if(!$path.StartsWith($sourceRoot+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Source path escape'}
    Assert-PlainPath $path
    Get-Item -LiteralPath $path
})
$bytes=($items|Measure-Object Length -Sum).Sum
if([IO.DriveInfo]::new('L:\').AvailableFreeSpace -lt $bytes+32MB){throw 'L: archive reserve unavailable'}
$null=New-Item -ItemType Directory -Path $destination
Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $destination 'runner.ps1')
$result=[ordered]@{passed=$false;authorizedScope='User-approved older source ZIP relocation only';originalWorkingSourceChanged=$false;currentBuildsChanged=$false;records=[Collections.Generic.List[object]]::new();removedCopies=0;error=$null;freeBefore=[IO.DriveInfo]::new('C:\').AvailableFreeSpace}
function Save-Receipt {
    $result.freeAfter=[IO.DriveInfo]::new('C:\').AvailableFreeSpace
    $path=Join-Path $destination 'result.json'
    [IO.File]::WriteAllText($path,($result|ConvertTo-Json -Depth 9))
    $read=Get-Content -LiteralPath $path -Raw|ConvertFrom-Json
    if($read.records.Count -ne $result.records.Count -or $read.removedCopies -ne $result.removedCopies){throw 'Relocation receipt readback differs'}
}
function Read-ZipEntries([string]$Path){
    $zip=[IO.Compression.ZipFile]::OpenRead($Path)
    try{
        foreach($entry in $zip.Entries){
            $s=$entry.Open()
            try{@{name=$entry.FullName;bytes=$entry.Length;sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))}}finally{$s.Dispose()}
        }
    }finally{$zip.Dispose()}
}
try{
    foreach($item in $items){
        $to=Join-Path $destination $item.Name
        $s=[IO.File]::Open($item.FullName,'Open','Read','Read')
        try{
            $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));$s.Position=0
            $streams=@(foreach($stream in Get-Item -LiteralPath $item.FullName -Stream * -Force){
                if($stream.Stream -eq ':$DATA'){continue}
                $streamPath=$item.FullName+':'+$stream.Stream
                $data=[IO.File]::ReadAllBytes($streamPath)
                if($data.Length -gt 1MB){throw 'Unexpectedly large archive alternate stream'}
                @{name=$stream.Stream;bytes=$data.Length;base64=[Convert]::ToBase64String($data);sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($data))}
            })
            $target=[IO.File]::Open($to,'CreateNew','Write','None')
            try{$s.CopyTo($target);$target.Flush($true)}finally{$target.Dispose()}
            if((Get-FileHash -LiteralPath $to).Hash -cne $hash -or (Get-Item -LiteralPath $to).Length -ne $item.Length){throw 'Archive copy hash/length differs'}
            $entries=@(Read-ZipEntries $item.FullName)
            $copyEntries=@(Read-ZipEntries $to)
            if(($entries|ConvertTo-Json -Depth 4 -Compress) -cne ($copyEntries|ConvertTo-Json -Depth 4 -Compress)){throw 'ZIP entry readback differs'}
            $record=@{name=$item.Name;original=$item.FullName;destination=$to;sha256=$hash;bytes=$item.Length;written=$item.LastWriteTimeUtc.ToString('o');created=$item.CreationTimeUtc.ToString('o');attributes=[int]$item.Attributes;sddl=(Get-Acl -LiteralPath $item.FullName).Sddl;alternateStreams=$streams;entries=$entries;copyVerified=$true;sourceCopyRemoved=$false}
            $result.records.Add($record)
            Save-Receipt
        }finally{$s.Dispose()}
    }
    if($result.records.Count -ne 8){throw 'All eight copies must verify before removal'}
    # Only redundant archive leaves are removed, after every destination verified.
    foreach($record in $result.records){
        Assert-PlainPath $record.original
        Assert-PlainPath $record.destination
        if((Get-FileHash -LiteralPath $record.original).Hash -cne $record.sha256 -or (Get-FileHash -LiteralPath $record.destination).Hash -cne $record.sha256){throw 'Archive changed before removal'}
        Remove-Item -LiteralPath $record.original -Force
        if(Test-Path -LiteralPath $record.original){throw 'Archive source removal not confirmed'}
        $record.sourceCopyRemoved=$true;$result.removedCopies++
        Save-Receipt
    }
    $result.logicalBytesRelocated=$bytes;$result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{Save-Receipt}
[pscustomobject]$result|Select-Object passed,removedCopies,logicalBytesRelocated,freeBefore,freeAfter,error|ConvertTo-Json
if(!$result.passed){exit 1}
