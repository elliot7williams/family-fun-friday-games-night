param([switch]$Apply,[switch]$LegacyHostChecks)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$allowed=[IO.Path]::GetFullPath((Join-Path $root 'dist'))
$out='I:\LeaBuildStorage\legacy-runtime-binaries-20261101-v1'
$directories=@(Get-ChildItem -LiteralPath $allowed -Directory -Force|Where-Object {$_.Name -match '^\.runtime(15[7-9]|16[0-9]|17[0-9]|18[0-4])-(work|check.*|stage.*|final-check.*)$'})
if($LegacyHostChecks){$out='I:\LeaBuildStorage\legacy-host-check-binaries-20261101-v1';$directories=@(Get-ChildItem -LiteralPath $allowed -Directory -Force|Where-Object {$_.Name -match '^\.m(4[7-9]|[5-9][0-9]|10[0-7])-host-check(-[0-9_\-]+)?$'})}
$files=@($directories|ForEach-Object {Get-ChildItem -LiteralPath $_.FullName -File -Force|Where-Object {$_.Extension -in '.dll','.exe','.pdb' -and ($_.Name.StartsWith('Lea.',[StringComparison]::Ordinal) -or $_.Name -ceq 'Firespider Internet 19.exe')}})
if($LegacyHostChecks){if($directories.Count -ne 12 -or $files.Count -ne 89 -or ($files|Measure-Object Length -Sum).Sum -ne 36021760){throw 'Inspected legacy host-check inventory differs'}}
elseif($directories.Count -ne 67 -or $files.Count -ne 559 -or ($files|Measure-Object Length -Sum).Sum -ne 209750016){throw 'Inspected legacy runtime inventory differs'}
if(!$Apply){$files|Select-Object FullName,Length;return}
if(Test-Path -LiteralPath $out){throw 'Preserve legacy binary archive evidence'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','SdkScriptLocal' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
function CheckPath([string]$path){
    $full=[IO.Path]::GetFullPath($path)
    if(!$full.StartsWith($allowed+'\',[StringComparison]::OrdinalIgnoreCase) -or [IO.Path]::GetDirectoryName([IO.Path]::GetDirectoryName($full)) -cne $allowed){throw 'Top-level legacy binary path escape'}
    $item=Get-Item -LiteralPath $full
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse legacy binary path refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    return $full
}
$processes=@(Get-CimInstance Win32_Process)
foreach($directory in $directories){if(@($processes|Where-Object {($_.ExecutablePath -and $_.ExecutablePath.StartsWith($directory.FullName+'\',[StringComparison]::OrdinalIgnoreCase)) -or ($_.CommandLine -and $_.CommandLine.Contains($directory.FullName))}).Count){throw 'Legacy stage in use'}}
New-Item -ItemType Directory -Path $out|Out-Null
$rows=[Collections.Generic.List[object]]::new();$deleted=[Collections.Generic.List[string]]::new();$holds=[Collections.Generic.Dictionary[string,IO.FileStream]]::new([StringComparer]::OrdinalIgnoreCase)
$result=@{verified=$false;files=$rows;deleted=$deleted;textSourcesRemoved=$false;unrelatedProjectsTouched=$false;failureEvidenceTouched=$false;freeBefore=(Get-PSDrive C).Free}
try{
    foreach($file in $files){
        $path=CheckPath $file.FullName
        if(@(Get-Item -LiteralPath $path -Stream *|Where-Object Stream -ne ':$DATA').Count){throw 'Alternate stream preservation required'}
        $stream=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($path,$stream)
        $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream));$stream.Position=0
        $rows.Add(@{path=$path;relative=[IO.Path]::GetRelativePath($allowed,$path).Replace('\','/');bytes=$stream.Length;sha256=$hash;sddl=(Get-Acl -LiteralPath $path).Sddl})
    }
    Add-Type -AssemblyName System.IO.Compression
    $zip=Join-Path $out 'legacy-runtime-binaries.zip';$archive=[IO.Compression.ZipFile]::Open($zip,'Create')
    try{foreach($row in $rows){$entry=$archive.CreateEntry($row.relative,[IO.Compression.CompressionLevel]::Optimal);$target=$entry.Open();try{$holds[$row.path].CopyTo($target)}finally{$target.Dispose()}}}finally{$archive.Dispose()}
    $archive=[IO.Compression.ZipFile]::OpenRead($zip)
    try{
        if($archive.Entries.Count -ne $rows.Count){throw 'Legacy archive inventory differs'}
        foreach($row in $rows){
            $entries=@($archive.Entries|Where-Object FullName -ceq $row.relative)
            if($entries.Count -ne 1 -or $entries[0].Length -ne $row.bytes){throw 'Legacy archive member differs'}
            $stream=$entries[0].Open();try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
            if($hash -cne $row.sha256 -or (Get-FileHash -LiteralPath $row.path).Hash -cne $row.sha256){throw 'Legacy archive readback differs'}
        }
    }finally{$archive.Dispose()}
    $result.archive=$zip;$result.archiveSha256=(Get-FileHash $zip).Hash;$result.verified=$true
    $result|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'verified-before-removal.json') -Encoding utf8
    foreach($stream in $holds.Values){$stream.Dispose()};$holds.Clear()
    foreach($row in $rows){
        $path=CheckPath $row.path
        if((Get-FileHash -LiteralPath $path).Hash -cne $row.sha256 -or (Get-Acl -LiteralPath $path).Sddl -cne $row.sddl){throw 'Legacy binary changed before removal'}
        Remove-Item -LiteralPath $path;$deleted.Add($path)
    }
    foreach($directory in $directories){
        $members=@($rows|Where-Object {[IO.Path]::GetDirectoryName($_.path) -ceq $directory.FullName})
        if($members.Count){@{archive=$zip;archiveSha256=$result.archiveSha256;manifest=(Join-Path $out 'result.json');files=$members;compiledCopiesRemoved=$true;sourceAndLogsKept=$true}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $directory.FullName 'compiled-archive.json') -Encoding utf8}
    }
}catch{$result.error=$_.Exception.ToString()}
finally{
    foreach($stream in $holds.Values){$stream.Dispose()}
    $result.removedAll=$deleted.Count -eq $rows.Count -and !$result.error
    $result.bytesRemoved=if($result.removedAll){($rows|Measure-Object bytes -Sum).Sum}else{($rows|Where-Object path -in $deleted|Measure-Object bytes -Sum).Sum}
    $result.freeAfter=(Get-PSDrive C).Free
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object verified,removedAll,bytesRemoved,freeBefore,freeAfter,error|ConvertTo-Json
}
