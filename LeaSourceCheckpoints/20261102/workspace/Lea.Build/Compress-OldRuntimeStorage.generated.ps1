param([ValidateRange(1,128)][int]$MaxFiles=64)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$root=[IO.Path]::GetFullPath((Join-Path $workspace 'continuity-evidence/modern-worker-v1'))
$out=Join-Path $workspace 'continuity-evidence/old-runtime-storage-20261026-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve storage operation evidence'}
if([IO.DriveInfo]::new('C:\').AvailableFreeSpace -lt 64MB){throw 'Insufficient cleanup journal reserve'}
$null=New-Item -ItemType Directory -Path $out
Add-Type -Path (Join-Path $PSScriptRoot 'CompressionAllocation.cs')
$records=[Collections.Generic.List[object]]::new()
$result=[ordered]@{completed=$false;deletedFiles=0;scope='NTFS storage compression of retained old modern-worker runtime binaries only; byte/ACL/mtime preservation checked, no deletion or engine adoption';records=$records;availableBefore=[IO.DriveInfo]::new('C:\').AvailableFreeSpace}
try{
    $files=@(Get-ChildItem -LiteralPath $root -Recurse -File -Force | Where-Object {
        $_.Extension -in @('.dll','.exe','.pdb') -and $_.Length -gt 1MB -and
        -not ($_.Attributes -band ([IO.FileAttributes]::Compressed -bor [IO.FileAttributes]::ReadOnly -bor [IO.FileAttributes]::ReparsePoint))
    }|Sort-Object Length -Descending|Select-Object -First $MaxFiles)
    if(!$files.Count){throw 'No eligible archived files'}
    $snapshots=@(foreach($file in $files){
        $path=[IO.Path]::GetFullPath($file.FullName)
        if(!$path.StartsWith($root+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Archive path escapes'}
        $ancestor=Get-Item -LiteralPath $path
        while($null -ne $ancestor){if($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse ancestor rejected'};$ancestor=if($ancestor.PSIsContainer){$ancestor.Parent}else{$ancestor.Directory}}
        [pscustomobject]@{path=$path;sha256=(Get-FileHash -LiteralPath $path).Hash;sddl=(Get-Acl -LiteralPath $path).Sddl;length=$file.Length;modifiedUtc=$file.LastWriteTimeUtc.ToString('o');allocated=[CompressionAllocation]::Bytes($path)}
    })
    $journal=Join-Path $out 'before.json'
    $snapshots|ConvertTo-Json -Depth 4|Set-Content -LiteralPath $journal
    $flush=[IO.File]::Open($journal,'Open','ReadWrite','Read');try{$flush.Flush($true)}finally{$flush.Dispose()}
    $compact=Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::System)) 'compact.exe'
    # Native compression preserves file contents and streams; small batches bound command length.
    for($index=0;$index -lt $snapshots.Count;$index+=8){
        $batch=@($snapshots|Select-Object -Skip $index -First 8)
        & $compact /C /Q /A @($batch.path) | Out-File -LiteralPath (Join-Path $out ('compact-'+$index+'.txt'))
        if($LASTEXITCODE -ne 0){throw 'Native storage compression failed'}
        foreach($before in $batch){
            $after=Get-Item -LiteralPath $before.path
            if((Get-FileHash -LiteralPath $before.path).Hash -cne $before.sha256 -or (Get-Acl -LiteralPath $before.path).Sddl -cne $before.sddl -or $after.Length -ne $before.length -or $after.LastWriteTimeUtc.ToString('o') -cne $before.modifiedUtc){throw 'Archived file preservation check failed'}
            $records.Add(@{path=$before.path;sha256=$before.sha256;preserved=$true;savedAllocatedBytes=$before.allocated-[CompressionAllocation]::Bytes($before.path)})
        }
        $records|ConvertTo-Json -Depth 4|Set-Content -LiteralPath (Join-Path $out 'verified.json')
    }
    $result.completed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.availableAfter=[IO.DriveInfo]::new('C:\').AvailableFreeSpace
    $result.savedAllocatedBytes=($records|Measure-Object savedAllocatedBytes -Sum).Sum
    $result|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $out 'result.json')
    $result|Select-Object completed,deletedFiles,availableBefore,availableAfter,savedAllocatedBytes,error|ConvertTo-Json
}
