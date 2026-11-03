param([switch]$Apply,[switch]$NextBatch,[switch]$LateSeptember)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
if($LateSeptember){if($NextBatch){throw 'Choose one explicit batch'};$NextBatch=$true}
$store=Join-Path $root $(if($NextBatch){'continuity-evidence/solid-build-archives-v2'}else{'continuity-evidence/solid-build-archives-v1'})
if($LateSeptember){$store=Join-Path $root 'continuity-evidence/solid-build-archives-v3'}
if($NextBatch){
    if(Test-Path $store){throw 'Preserve previous batch attempt'}
    $names=@('Lea-runtime-212-ready_2026-09-20_01-51-52.zip','Lea-runtime-213-ready_2026-09-20_06-23-03.zip',
        'Lea-runtime-214-ready_2026-09-20_07-00-46.zip','Lea-runtime-215-ready_2026-09-20_14-51-15.zip',
        'Lea-runtime-216-ready_2026-09-20_19-26-05.zip','Lea-runtime-217-ready_2026-09-22_16-26-29.zip')
    $source=Join-Path $root 'dist/LeaExes'
    if($LateSeptember){$names=@('Lea-runtime-218-ready_2026-09-22_20-59-28.zip','Lea-runtime-219-ready_2026-09-22_22-06-32.zip','Lea-runtime-220-ready_2026-09-23_04-11-39.zip','Lea-runtime-221-ready_2026-09-23_08-09-47.zip','Lea-runtime-222-ready_2026-09-23_09-09-06.zip','Lea-runtime-224-ready_2026-09-23_16-45-23.zip')}
    $files=@($names|ForEach-Object {Get-Item -LiteralPath (Join-Path $source $_)})
    $total=($files|Measure-Object Length -Sum).Sum;$largest=($files|Measure-Object Length -Maximum).Maximum
    if((Get-PSDrive C).Free -lt 64MB+$total+$largest){throw 'Worst-case archive/restore workspace reserve required; not a build waiver'}
    if((Get-CimInstance Win32_OperatingSystem).FreePhysicalMemory*1KB -lt 1GB){throw 'Compression memory headroom required'}
    $auditPath=Join-Path $root 'continuity-evidence/reclaimed-old-packages-20261028-v3.json'
    $auditHash='424AABAB41DD5BF17B925E02CA3A2E94FFD27F019DFCBDEBCCB3503B53822AAA'
    if($LateSeptember){$auditPath=Join-Path $root 'continuity-evidence/reclaimed-old-packages-20261028-v4.json';$auditHash='5ED964D05EE5ACBB0BBC06183F7AA2F2F09B786CB66060D2F7F7B57530256F07'}
    if((Get-FileHash $auditPath).Hash -cne $auditHash){throw 'Current recovery audit differs'}
    $audit=Get-Content $auditPath -Raw|ConvertFrom-Json
    $tar=Join-Path $env:WINDIR 'System32/tar.exe'
    if((Get-FileHash $tar).Hash -cne '2CA7DB442FEAFA8E6D7A88C7DDDF3113128F85701A4EB1182C71CFBF3D479A56'){throw 'Tested archive tool differs'}
    $creationHolds=[Collections.Generic.List[IDisposable]]::new();$newInputs=[Collections.Generic.List[object]]::new()
    $null=New-Item -ItemType Directory -Path $store
    Copy-Item $PSCommandPath (Join-Path $store 'runner.ps1')
    try{
        foreach($file in $files){
            if($file.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Source archive link'}
            $hold=[IO.File]::Open($file.FullName,'Open','Read','Read');$creationHolds.Add($hold)
            $hash=(Get-FileHash $file.FullName).Hash
            $version=[regex]::Match($file.Name,'^Lea-runtime-(\d+)-').Groups[1].Value
            $pins=@($audit|Where-Object folder -Match ('^old-package-reclamation-20261026-'+$version+'(?:-|$)')|Select-Object -ExpandProperty archiveSha256 -Unique)
            if($pins.Count -ne 1 -or $pins[0] -cne $hash){throw 'Source ZIP differs from its version-specific recovery audit'}
            $newInputs.Add(@{name=$file.Name;path=$file.FullName;length=$file.Length;sha256=$hash})
        }
        $packPath=Join-Path $store 'older-builds.7z';$clock=[Diagnostics.Stopwatch]::StartNew()
        & $tar -cf $packPath --format=7zip --options=7zip:compression=lzma2,7zip:compression-level=9 -C $source @names
        if($LASTEXITCODE -ne 0){throw 'Archive creation failed; originals retained'}
        $entries=@(& $tar -tf $packPath)
        if($LASTEXITCODE -ne 0 -or ($entries|Sort-Object|ConvertTo-Json -Compress) -cne ($names|Sort-Object|ConvertTo-Json -Compress)){throw 'Container inventory differs; originals retained'}
        $creation=@{creationPassed=$true;roundTripVerified=$false;inputs=$newInputs;containerSha256=(Get-FileHash $packPath).Hash;
            createMs=$clock.ElapsedMilliseconds;sourceBytes=$total;containerBytes=(Get-Item $packPath).Length;
            scope='Container creation/inventory only; full restore verification and removal are in consolidation.json'}
        [IO.File]::WriteAllText((Join-Path $store 'result.json'),($creation|ConvertTo-Json -Depth 5))
    }finally{foreach($hold in $creationHolds){$hold.Dispose()}}
}
$receiptPath=Join-Path $store 'consolidation.json';$manifestPath=Join-Path $store 'originals.json'
if((Test-Path $receiptPath) -or (Test-Path $manifestPath)){throw 'Preserve existing consolidation state'}
$proofPath=Join-Path $store 'result.json'
if(!$NextBatch -and (Get-FileHash $proofPath).Hash -cne 'FE5A04D4127C48938436D6390D19BDF7B6415F0E6FE032DC8029C06BECBDEB1C'){throw 'Producer proof differs'}
$proof=Get-Content $proofPath -Raw|ConvertFrom-Json
$pack=Join-Path $store 'older-builds.7z'
if((!$NextBatch -and (!$proof.passed -or !$proof.roundTripVerified)) -or ($NextBatch -and !$proof.creationPassed) -or (Get-FileHash $pack).Hash -cne $proof.containerSha256){throw 'Verified container required'}
$holds=[Collections.Generic.List[IDisposable]]::new();$records=[Collections.Generic.List[object]]::new()
$result=[ordered]@{verified=$false;removed=@();error=$null;containerSha256=$proof.containerSha256;metadataScope='Default bytes, alternate streams, attributes, creation/write times, owner/group/DACL; not full NTFS identity/SACL recreation'}
function Save-State {
    $json=$result|ConvertTo-Json -Depth 8
    [IO.File]::WriteAllText($receiptPath,$json)
    $s=[IO.File]::Open($receiptPath,'Open','ReadWrite','Read');try{$s.Flush($true)}finally{$s.Dispose()}
}
try{
    $freeBefore=(Get-PSDrive C).Free
    foreach($input in $proof.inputs){
        $path=[IO.Path]::GetFullPath($input.path)
        $allowedNames=if($NextBatch){$names}else{@('Lea-runtime-210-ready_2026-09-19_16-01-25.zip','Lea-runtime-211-ready_2026-09-19_20-45-48.zip')}
        if($path -cne (Join-Path $root ('dist/LeaExes/'+$input.name)) -or $input.name -notin $allowedNames){throw 'Explicit old ZIP required'}
        $item=Get-Item -LiteralPath $path
        $walk=$item
        while($walk){if($walk.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Original archive link'};$walk=if($walk -is [IO.FileInfo]){$walk.Directory}else{$walk.Parent}}
        $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
        if((Get-FileHash $path).Hash -cne $input.sha256 -or $s.Length -ne $input.length){throw 'Original ZIP differs'}
        $streams=@(foreach($stream in Get-Item -LiteralPath $path -Stream * -Force){
            if($stream.Stream -eq ':$DATA'){continue}
            if($stream.Stream -cnotmatch '^[A-Za-z0-9_.-]+$' -or $stream.Length -gt 1MB){throw 'Unsupported alternate stream'}
            $streamHold=[IO.File]::Open($path+':'+$stream.Stream,'Open','Read','Read');$holds.Add($streamHold)
            $data=[byte[]]::new([int]$streamHold.Length);$streamHold.ReadExactly($data)
            @{name=$stream.Stream;length=$data.Length;sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($data));base64=[Convert]::ToBase64String($data)}
        })
        $records.Add(@{path=$path;name=$input.name;length=$s.Length;sha256=$input.sha256;attributes=[int]$item.Attributes;created=$item.CreationTimeUtc.ToString('o');written=$item.LastWriteTimeUtc.ToString('o');sddl=(Get-Acl $path).Sddl;streams=$streams})
    }
    [IO.File]::WriteAllText($manifestPath,(@{originals=$records;containerSha256=$proof.containerSha256}|ConvertTo-Json -Depth 8))
    $s=[IO.File]::Open($manifestPath,'Open','ReadWrite','Read');try{$s.Flush($true)}finally{$s.Dispose()}
    $result.manifestSha256=(Get-FileHash $manifestPath).Hash
    $back=Get-Content $manifestPath -Raw|ConvertFrom-Json -DateKind String
    if($back.originals.Count -ne $proof.inputs.Count){throw 'Manifest inventory differs'}
    foreach($entry in $back.originals){foreach($stream in $entry.streams){
        $data=[Convert]::FromBase64String($stream.base64)
        if($data.Length -ne $stream.length -or [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($data)) -cne $stream.sha256){throw 'Stream backup readback differs'}
    }}
    $result.verified=$true;Save-State
    . (Join-Path $PSScriptRoot 'Get-ConsolidatedBuildArchive.ps1')
    $negative=0
    foreach($case in @(
        @((Join-Path $root 'outside.zip'),(Join-Path $store 'recovery-temp/negative')),
        @((Join-Path $root 'dist/LeaExes/unknown.zip'),(Join-Path $store 'recovery-temp/negative')),
        @($records[0].path,$store),
        @($records[0].path,(Join-Path $store 'recovery-temp/../escape'))
    )){
        $denied=$false;try{$null=Get-ConsolidatedBuildArchive -OriginalPath $case[0] -Destination $case[1] -ValidateOnly}catch{$denied=$true}
        if(!$denied){throw 'Recovery path negative control accepted'};$negative++
    }
    $result.recoveryNegativeControls=$negative
    foreach($entry in $records){
        $scratch=Join-Path $store ('recovery-temp/'+[Guid]::NewGuid().ToString('N'))
        $restored=Get-ConsolidatedBuildArchive -OriginalPath $entry.path -Destination $scratch -ValidateOnly
        if((Get-FileHash $restored).Hash -cne $entry.sha256){throw 'Independent recovery path differs'}
        # Remove only the newly created, hash-verified recovery copy, never its parent recursively.
        Remove-Item -LiteralPath $restored -Force
    }
    $result.recoveryPathVerified=$true;Save-State
    if($Apply){
        $intent=Join-Path $store 'removal-intent.json'
        [IO.File]::WriteAllText($intent,($result|ConvertTo-Json -Depth 8))
        $s=[IO.File]::Open($intent,'Open','ReadWrite','Read');try{$s.Flush($true)}finally{$s.Dispose()}
        for($i=$holds.Count-1;$i -ge 0;$i--){$holds[$i].Dispose()};$holds.Clear()
        foreach($entry in $records){
            $path=[IO.Path]::GetFullPath($entry.path)
            if($path -cne (Join-Path $root ('dist/LeaExes/'+$entry.name)) -or (Get-Item $path).Attributes -band [IO.FileAttributes]::ReparsePoint -or
                (Get-FileHash $path).Hash -cne $entry.sha256 -or (Get-Acl $path).Sddl -cne $entry.sddl -or
                (Get-Item $path).LastWriteTimeUtc.ToString('o') -cne $entry.written -or
                (Get-Item $path).CreationTimeUtc.ToString('o') -cne $entry.created -or [int](Get-Item $path).Attributes -ne $entry.attributes -or
                (Get-FileHash $manifestPath).Hash -cne $result.manifestSha256 -or (Get-FileHash $pack).Hash -cne $proof.containerSha256){throw 'Final source/backup verification differs'}
            $streams=@(Get-Item -LiteralPath $path -Stream * -Force|Where-Object Stream -ne ':$DATA')
            if($streams.Count -ne @($entry.streams).Count){throw 'Final alternate-stream inventory differs'}
            foreach($stream in $entry.streams){
                $data=[IO.File]::ReadAllBytes($path+':'+$stream.name)
                if($data.Length -ne $stream.length -or [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($data)) -cne $stream.sha256){throw 'Final alternate-stream bytes differ'}
            }
            Remove-Item -LiteralPath $path -Force
            if(Test-Path -LiteralPath $path){throw 'Original copy still present'}
            $result.removed+=@($path);Save-State
        }
    }
    $result.freeBefore=$freeBefore;$result.freeAfter=(Get-PSDrive C).Free
}catch{$result.error=$_.Exception.ToString()}
finally{
    foreach($hold in $holds){$hold.Dispose()}
    Save-State
    $result|ConvertTo-Json -Depth 5
}
