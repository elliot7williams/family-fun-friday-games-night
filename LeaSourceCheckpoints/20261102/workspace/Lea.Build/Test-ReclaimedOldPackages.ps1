param([ValidatePattern('^[a-z0-9-]+\.json$')][string]$ReportName)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$reports=[Collections.Generic.List[object]]::new()
foreach($folder in Get-ChildItem -LiteralPath (Join-Path $workspace 'continuity-evidence') -Directory -Filter 'old-package-reclamation-20261026-*'){
    $receipt=Join-Path $folder.FullName 'result.json'
    if(!(Test-Path -LiteralPath $receipt)){continue}
    $result=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
    if(!$result.removed){continue}
    $archivePath=$result.archive;$recovered=$false
    if(!(Test-Path -LiteralPath $archivePath)){
        . (Join-Path $PSScriptRoot 'Get-ConsolidatedBuildArchive.ps1')
        $scratch=Join-Path $workspace ('continuity-evidence/solid-build-archives-v1/recovery-temp/'+[Guid]::NewGuid().ToString('N'))
        $archivePath=Get-ConsolidatedBuildArchive -OriginalPath $result.archive -Destination $scratch
        $recovered=$true
    }
    if(!$result.verified -or $result.error -or (Test-Path -LiteralPath $result.target) -or
        (Get-FileHash -LiteralPath $archivePath).Hash -cne $result.archiveSha256 -or
        (Get-FileHash -LiteralPath $result.metadata).Hash -cne $result.metadataSha256){throw 'Retained backup receipt differs'}
    $backup=Get-Content -LiteralPath $result.metadata -Raw|ConvertFrom-Json -DateKind String
    $files=@{};$extra=0;$streams=0;$packed=0
    foreach($record in $backup.records){
        if(!$record.directory){$files[$record.relative]=$record}
        if($record.PSObject.Properties.Name -contains 'extraBase64'){
            $bytes=[Convert]::FromBase64String($record.extraBase64)
            if($bytes.Length -ne $record.length -or [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)) -cne $record.sha256){throw 'Extra-file recovery data differs'}
            $extra++
        }
        foreach($stream in $record.streams){
            $bytes=[Convert]::FromBase64String($stream.base64)
            if($bytes.Length -ne $stream.length -or [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)) -cne $stream.sha256){throw 'Alternate-stream recovery data differs'}
            $streams++
        }
    }
    $zip=[IO.Compression.ZipFile]::OpenRead($archivePath)
    try{foreach($entry in $zip.Entries){
        $relative=$entry.FullName.Replace('/','\');if($relative.EndsWith('\')){continue}
        if(!$files.ContainsKey($relative)){throw 'Recovery manifest lacks archived file'}
        $record=$files[$relative];$input=$entry.Open()
        try{if($entry.Length -ne $record.length -or [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($input)) -cne $record.sha256){throw 'Archived recovery bytes differ'}}finally{$input.Dispose()}
        $packed++
    }}finally{$zip.Dispose()}
    if($packed+$extra -ne $files.Count){throw 'Recovery inventory incomplete'}
    if($recovered){
        $allowed=[IO.Path]::GetFullPath((Join-Path $workspace 'continuity-evidence/solid-build-archives-v1/recovery-temp'))
        if(![IO.Path]::GetFullPath($archivePath).StartsWith($allowed+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Recovery scratch escape'}
        Remove-Item -LiteralPath $archivePath -Force
    }
    $reports.Add(@{folder=$folder.Name;verified=$true;originalZipFiles=$packed;extraFiles=$extra;alternateStreams=$streams;sourceFilesRecoverable=$files.Count;metadataSha256=$result.metadataSha256;archiveSha256=$result.archiveSha256})
}
if($ReportName){
    $path=Join-Path $workspace ('continuity-evidence/'+$ReportName)
    if(Test-Path $path){throw 'Preserve recovery audit'}
    [IO.File]::WriteAllText($path,($reports|ConvertTo-Json -Depth 4))
}
$reports|ConvertTo-Json -Depth 4
