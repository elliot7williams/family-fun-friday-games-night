param([ValidateSet(224,222,221,220,219,218,217,216,215,214,213,212,211,210,203,202,201,200,199,198,197,196,192,185,186)][int]$Version=217,[ValidateRange(1,9)][int]$Attempt=2,[switch]$Apply,[ValidateSet('Verify192Earlier','Verify192Second','Stage192','Stage196','Verify192','Verify196','Stage197','Stage198','Verify197','Verify198','Stage199','Verify199','Verify200','Verify200Earlier','Verify203','Verify202','Verify201','Verify210','VerifyM111','VerifyM111Own','Host185','Host185Second','Work186')][string]$HistoricalStage)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$allowed=[IO.Path]::GetFullPath((Join-Path $workspace 'dist/LeaExes'))
if($HistoricalStage -eq 'Work186'){$allowed=[IO.Path]::GetFullPath((Join-Path $workspace 'dist'))}
$choices=@(Get-ChildItem -LiteralPath $allowed -Directory | Where-Object Name -match ('^Lea-runtime-'+$Version+'-ready-stage_2026-09-\d{2}_\d{2}-\d{2}-\d{2}$'))
if($HistoricalStage){
    $stages=@{
        Verify192Earlier=@(192,'verify-runtime192-final','Lea-runtime-192-ready_2026-09-06_12-46-20.zip')
        Verify192Second=@(192,'verify-runtime192-final-2','Lea-runtime-192-ready_2026-09-06_17-10-44.zip')
        Stage192=@(192,'Lea-runtime-192-ready-stage','Lea-runtime-192-ready_2026-09-06_17-10-44.zip')
        Verify192=@(192,'verify-runtime192-final','Lea-runtime-192-ready_2026-09-06_17-10-44.zip')
        Stage196=@(196,'Lea-runtime-196-ready-stage','Lea-runtime-196-ready_2026-09-08_17-37-27.zip')
        Verify196=@(196,'verify-runtime196-final','Lea-runtime-196-ready_2026-09-08_17-37-27.zip')
        Stage197=@(197,'Lea-runtime-197-ready-stage','Lea-runtime-197-ready_2026-09-08_22-18-37.zip')
        Stage198=@(198,'Lea-runtime-198-ready-stage','Lea-runtime-198-ready_2026-09-08_23-20-10.zip')
        Verify197=@(197,'verify-runtime197-final_2026-09-08_22-19-25','Lea-runtime-197-ready_2026-09-08_22-18-37.zip')
        Verify198=@(198,'verify-runtime198-final_2026-09-08_23-20-10','Lea-runtime-198-ready_2026-09-08_23-20-10.zip')
        Stage199=@(199,'Lea-runtime-199-ready-stage','Lea-runtime-199-ready_2026-09-09_04-07-33.zip')
        Verify199=@(199,'verify-runtime199-final_2026-09-09_04-07-33','Lea-runtime-199-ready_2026-09-09_04-07-33.zip')
        Verify200=@(200,'verify-runtime200-final_2026-09-09_12-11-28','Lea-runtime-200-ready_2026-09-09_12-11-28.zip')
        Verify200Earlier=@(200,'verify-runtime200-final_2026-09-09_12-04-02','Lea-runtime-200-ready_2026-09-09_12-11-28.zip')
        Verify203=@(203,'verify-runtime203-final_2026-09-12_21-37-03','Lea-runtime-203-ready_2026-09-12_21-37-03.zip')
        Verify202=@(202,'verify-runtime202-final_2026-09-12_16-43-14','Lea-runtime-202-ready_2026-09-12_16-43-14.zip')
        Verify201=@(201,'verify-runtime201-final_2026-09-12_05-34-01','Lea-runtime-201-ready_2026-09-12_05-34-01.zip')
        Verify210=@(210,'verify-runtime210-final_2026-09-19_16-01-25','Lea-runtime-210-ready_2026-09-19_16-01-25.zip')
        VerifyM111=@(210,'verify-m111-host_2026-09-19_20-13-59','Lea-runtime-210-ready_2026-09-19_16-01-25.zip')
        VerifyM111Own=@(210,'verify-m111-host_2026-09-19_20-13-59','Archived-verify-m111-host_2026-09-19_20-13-59.zip')
        Host185=@(185,'Lea-runtime-185-host-stage','Archived-Lea-runtime-185-host-stage.zip')
        Host185Second=@(185,'Lea-runtime-185-host-stage2','Archived-Lea-runtime-185-host-stage2.zip')
        Work186=@(186,'.runtime186-work','Archived-runtime186-work.zip')
    }
    $stage=$stages[$HistoricalStage]
    if($Version -ne $stage[0]){throw 'Explicit historical target version differs'}
    $choices=@(Get-Item -LiteralPath (Join-Path $allowed $stage[1]))
}
if($choices.Count -ne 1){throw 'Old package identity ambiguous'}
$target=[IO.Path]::GetFullPath($choices[0].FullName)
$archive=Join-Path $allowed ($choices[0].Name.Replace('-ready-stage_','-ready_')+'.zip')
if($HistoricalStage){$archive=Join-Path $allowed $stage[2];if(!(Test-Path -LiteralPath $archive)){throw 'Explicit original archive missing'}}
if(!(Test-Path -LiteralPath $archive -PathType Leaf)){
    # Some historical packing runs stamped the ZIP after staging the directory.
    $archives=@(Get-ChildItem -LiteralPath $allowed -File | Where-Object Name -match ('^Lea-runtime-'+$Version+'-ready_2026-09-\d{2}_\d{2}-\d{2}-\d{2}\.zip$') | Sort-Object Name)
    if(!$archives.Count){throw 'Original archive missing'}
    $archive=$archives[0].FullName
}
$out=Join-Path $workspace ('continuity-evidence/old-package-reclamation-20261026-'+$Version+$(if($Attempt -gt 1){'-v'+$Attempt}else{''}))
if($HistoricalStage){$out+='-'+$HistoricalStage.ToLowerInvariant()}
if(Test-Path -LiteralPath $out){throw 'Preserve previous reclamation attempt'}
if(!(Test-Path -LiteralPath $archive -PathType Leaf)){throw 'Original archive missing'}
if(!$target.StartsWith($allowed+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Target escapes old packages'}
$ancestor=Get-Item -LiteralPath $target
while($ancestor){if($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse ancestor'};$ancestor=$ancestor.Parent}
$null=New-Item -ItemType Directory -Path $out
$records=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
$result=[ordered]@{verified=$false;removed=$false;target=$target;archive=$archive;error=$null}
Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
$result.runnerSha256=(Get-FileHash -LiteralPath $PSCommandPath).Hash
$sha=[Security.Cryptography.SHA256]::Create()
function HashStream([IO.Stream]$Stream){[BitConverter]::ToString($sha.ComputeHash($Stream)).Replace('-','')}
try{
    $archiveLease=[IO.File]::Open($archive,'Open','Read','Read');$leases.Add($archiveLease)
    $archiveHash=HashStream $archiveLease;$archiveLease.Position=0
    $zip=[IO.Compression.ZipArchive]::new($archiveLease,[IO.Compression.ZipArchiveMode]::Read,$true)
    $entries=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::OrdinalIgnoreCase)
    try{
        foreach($entry in $zip.Entries){
            $relative=$entry.FullName.Replace('/','\')
            if([IO.Path]::IsPathRooted($relative) -or $relative.Contains(':') -or @($relative.Split('\')|Where-Object {$_ -in @('.','..')}).Count){throw 'Unsafe archive entry'}
            if($relative.EndsWith('\')){continue}
            if(!$entries.TryAdd($relative,$entry)){throw 'Duplicate archive entry'}
        }
        $items=[Collections.Generic.List[object]]::new();$pending=[Collections.Generic.Stack[string]]::new();$pending.Push($target)
        $items.Add((Get-Item -LiteralPath $target))
        while($pending.Count){foreach($item in Get-ChildItem -LiteralPath $pending.Pop() -Force){
            if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse tree entry'}
            $items.Add($item);if($item.PSIsContainer){$pending.Push($item.FullName)}
        }}
        $matched=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
        foreach($item in $items){
            $relative=$item.FullName.Substring($target.Length).TrimStart('\')
            $extended='\\?\'+$item.FullName
            $record=[ordered]@{relative=$relative;directory=[bool]$item.PSIsContainer;attributes=[int]$item.Attributes;writtenUtc=$item.LastWriteTimeUtc.ToString('o');createdUtc=$item.CreationTimeUtc.ToString('o');sddl=(Get-Acl -LiteralPath $item.FullName).Sddl}
            if(!$item.PSIsContainer){
                $file=[IO.File]::Open($extended,'Open','Read','Read');$leases.Add($file)
                $record.length=$file.Length;$record.sha256=HashStream $file;$file.Position=0
                if($entries.ContainsKey($relative)){
                    $entry=$entries[$relative];$packed=$entry.Open()
                    try{if($entry.Length -ne $file.Length -or (HashStream $packed) -cne $record.sha256){throw ('Original ZIP differs: '+$relative)}}finally{$packed.Dispose()}
                    $null=$matched.Add($relative)
                }else{
                    $report=if($HistoricalStage -eq 'Verify192Earlier'){'runtime192-extracted-check.md'}elseif($HistoricalStage -eq 'Verify192Second'){'runtime192-package-check.md'}else{$null}
                    $knownReport=$report -and $relative -ceq $report -and $file.Length -le 32KB
                    if($file.Length -gt 1MB -or ([IO.Path]::GetFileName($relative) -notlike '._*' -and !$knownReport)){throw ('Unexpected nonarchived file: '+$relative)}
                    $bytes=[byte[]]::new([int]$file.Length);$file.ReadExactly($bytes);$file.Position=0
                    $record.extraBase64=[Convert]::ToBase64String($bytes)
                    if($knownReport){
                        $reportBackup=Join-Path $out $report
                        [IO.File]::WriteAllBytes($reportBackup,$bytes)
                        if((Get-FileHash $reportBackup).Hash -cne $record.sha256){throw 'Historical report backup differs'}
                        $result.retainedReport=$reportBackup
                    }
                }
            }
            try{$liveStreams=@(Get-Item -LiteralPath $item.FullName -Stream * -Force -ErrorAction Stop)}
            catch{$liveStreams=@(Get-Item -LiteralPath $extended -Stream * -Force -ErrorAction Stop)}
            $record.streams=@(foreach($stream in $liveStreams){
                if($stream.Stream -eq ':$DATA'){continue}
                if($stream.Stream -cnotmatch '^[A-Za-z0-9_.-]+$' -or $stream.Length -gt 1MB){throw 'Unexpected alternate stream'}
                $file=[IO.File]::Open($extended+':'+$stream.Stream,'Open','Read','Read');$leases.Add($file)
                $hash=HashStream $file;$file.Position=0;$bytes=[byte[]]::new([int]$file.Length);$file.ReadExactly($bytes)
                @{name=$stream.Stream;sha256=$hash;base64=[Convert]::ToBase64String($bytes);length=$bytes.Length}
            })
            $records.Add($record)
        }
        if($matched.Count -ne $entries.Count){throw 'Original archive has missing extracted files'}
    }finally{$zip.Dispose()}
    $metadata=Join-Path $out 'metadata.json'
    @{archive=$archive;archiveSha256=$archiveHash;target=$target;records=$records;scope='Original ZIP plus byte-exact AppleDouble extras/alternate streams and metadata; not full NTFS identity/SACL backup'}|ConvertTo-Json -Depth 9|Set-Content -LiteralPath $metadata
    $flush=[IO.File]::Open($metadata,'Open','ReadWrite','Read');try{$flush.Flush($true)}finally{$flush.Dispose()}
    $readback=Get-Content -LiteralPath $metadata -Raw|ConvertFrom-Json -DateKind String
    if($readback.records.Count -ne $records.Count){throw 'Metadata readback inventory differs'}
    foreach($record in $readback.records){
        if($record.extraBase64){$data=[Convert]::FromBase64String($record.extraBase64);if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($data)) -cne $record.sha256){throw 'Extra-file backup differs'}}
        foreach($stream in $record.streams){$data=[Convert]::FromBase64String($stream.base64);if($data.Length -ne $stream.length -or [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($data)) -cne $stream.sha256){throw 'Alternate-stream backup differs'}}
        $path=if($record.relative){Join-Path $target $record.relative}else{$target}
        $item=Get-Item -LiteralPath $path -Force
        if([int]$item.Attributes -ne $record.attributes -or $item.LastWriteTimeUtc.ToString('o') -cne $record.writtenUtc -or (Get-Acl -LiteralPath $path).Sddl -cne $record.sddl){throw 'Tree changed during verification'}
    }
    if(@(Get-ChildItem -LiteralPath $target -Recurse -Force).Count+1 -ne $records.Count){throw 'Final tree inventory differs'}
    if(@(Get-CimInstance Win32_Process|Where-Object {($_.ExecutablePath -and $_.ExecutablePath.StartsWith($target+'\',[StringComparison]::OrdinalIgnoreCase)) -or ($_.CommandLine -and $_.CommandLine.Contains($target))}).Count){throw 'Old package is in use'}
    $result.verified=$true;$result.archiveSha256=$archiveHash;$result.metadata=$metadata;$result.metadataSha256=(Get-FileHash -LiteralPath $metadata).Hash
    $result.files=@($records|Where-Object {!$_.directory}).Count
    if($Apply){
        $result.availableBefore=[IO.DriveInfo]::new('C:\').AvailableFreeSpace
        $result|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $out 'removal-intent.json')
        $intent=[IO.File]::Open((Join-Path $out 'removal-intent.json'),'Open','ReadWrite','Read');try{$intent.Flush($true)}finally{$intent.Dispose()}
        $metadataLease=[IO.File]::Open($metadata,'Open','Read','Read')
        try{
            for($index=$leases.Count-1;$index -ge 1;$index--){$leases[$index].Dispose();$leases.RemoveAt($index)}
            if([IO.Path]::GetFullPath($target) -cne $choices[0].FullName -or !$target.StartsWith($allowed+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Final absolute target differs'}
            if(@(Get-ChildItem -LiteralPath $target -Recurse -Force|Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}).Count){throw 'Final reparse entry'}
            Remove-Item -LiteralPath $target -Recurse -Force
            $result.removed=!(Test-Path -LiteralPath $target)
            if(!$result.removed -or (Get-FileHash -LiteralPath $archive).Hash -cne $archiveHash -or (Get-FileHash -LiteralPath $metadata).Hash -cne $result.metadataSha256){throw 'Retained backup/removal verification failed'}
        }finally{$metadataLease.Dispose()}
        $result.availableAfter=[IO.DriveInfo]::new('C:\').AvailableFreeSpace
    }
}catch{$result.error=$_.Exception.ToString()}
finally{
    foreach($lease in $leases){$lease.Dispose()};$sha.Dispose()
    $result|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $out 'result.json')
    $result|ConvertTo-Json -Depth 5
}
