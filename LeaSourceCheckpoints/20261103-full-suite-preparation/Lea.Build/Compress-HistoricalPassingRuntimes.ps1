param([ValidateSet('Plan','Apply')][string]$Mode='Plan',[string]$PlanSha256,[ValidateSet('Baseline','Diagnostic')][string]$Batch='Baseline')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$base='I:/LeaBuildStorage/historical-runtime-compression-20261102-v1'
if($Batch -ceq 'Diagnostic'){$base='I:/LeaBuildStorage/historical-diagnostic-compression-20261102-v1'}
$out=$base+'-'+$Mode.ToLowerInvariant()
if(Test-Path $out){throw 'Preserve compression records'}
if((Get-Volume -DriveLetter C).FileSystem -cne 'NTFS'){throw 'NTFS required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No active browser/test processes'}
New-Item -ItemType Directory -Path $out|Out-Null
$env:TEMP=Join-Path $out 'tmp';$env:TMP=$env:TEMP
New-Item -ItemType Directory -Path $env:TEMP|Out-Null
Add-Type -Path (Join-Path $PSScriptRoot 'NtfsStorageProbe.cs')
$specs=@(
    @{runtime='continuity-evidence/deterministic-full-replay-v1/runtime';receipt='continuity-evidence/deterministic-full-replay-owned-v1/result.json';hash='C9A3C41F5A76A33C7D792B3F7FBC34ED3D7E55BAE64A8844B49C812FE4D3A4CE';count=65},
    @{runtime='continuity-evidence/is-connected-isolated-v1/runtime';receipt='continuity-evidence/is-connected-isolated-v1/result.json';hash='429BD1EF2F574E657F056C9F8A34D245E34122EBCD98CD95F0FC035294F465AF';count=16}
)
if($Batch -ceq 'Diagnostic'){$specs=@(@{runtime='continuity-evidence/contains-recovery-full-prefix-v1/runtime';receipt='continuity-evidence/contains-recovery-full-prefix-owned-v1/result.json';hash='1D0BCC947674AED3C0CA66295365CAD009FD474F8F0CB9736AF4D5F21229F7A7';count=65})}
function CheckPath([string]$path,[string]$allowed){
    $resolved=[IO.Path]::GetFullPath($path);$prefix=[IO.Path]::GetFullPath($allowed)+[IO.Path]::DirectorySeparatorChar
    if(!$resolved.StartsWith($prefix,[StringComparison]::OrdinalIgnoreCase)){throw 'Historical runtime path escape'}
    $item=Get-Item -LiteralPath $resolved
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse path refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
}
$rows=[Collections.Generic.List[object]]::new()
foreach($spec in $specs){
    $runtime=Join-Path $root $spec.runtime;$receipt=Join-Path $root $spec.receipt
    if((Get-FileHash $receipt).Hash -cne $spec.hash){throw 'Historical passing receipt changed'}
    $r=Get-Content $receipt -Raw|ConvertFrom-Json
    if(!$r.passed -or $r.ownerAttention -or $r.changedFiles.Count -or $r.before.Count -ne $spec.count -or @(Get-ChildItem $runtime -Recurse -File).Count -ne $spec.count){throw 'Historical passing boundary differs'}
    foreach($old in $r.before){
        CheckPath $old.path $runtime
        if((Get-FileHash $old.path).Hash -cne $old.sha256 -or (Get-Acl $old.path).Sddl -cne $old.sddl){throw 'Historical file data/ACL changed'}
        $state=[Lea.Build.NtfsStorageProbe]::Read($old.path)
        $flags=[IO.FileAttributes]$state.Attributes
        $eligible=$state.Links -eq 1 -and $state.LogicalBytes -ge 64KB -and $state.LogicalBytes -le 16MB -and ([IO.Path]::GetExtension($old.path) -cin @('.dll','.exe')) -and !($flags -band ([IO.FileAttributes]::Compressed -bor [IO.FileAttributes]::ReadOnly -bor [IO.FileAttributes]::Encrypted -bor [IO.FileAttributes]::SparseFile))
        $rows.Add(@{path=$old.path;runtime=$runtime;sha256=$old.sha256;sddl=$old.sddl;before=$state;eligible=[bool]$eligible;lastWriteUtc=(Get-Item $old.path).LastWriteTimeUtc})
    }
}
if($Mode -ceq 'Plan'){
    @{verified=$true;files=$rows;eligibleFiles=@($rows|Where-Object eligible).Count;eligibleLogicalBytes=($rows|Where-Object eligible|ForEach-Object {$_.before.LogicalBytes}|Measure-Object -Sum).Sum;noFilesChanged=$true;scriptSha256=(Get-FileHash $PSCommandPath).Hash;probeSha256=(Get-FileHash (Join-Path $PSScriptRoot 'NtfsStorageProbe.cs')).Hash}|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'plan.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'plan.json')
    exit 0
}
$planPath=$base+'-plan/plan.json'
if(!$PlanSha256 -or (Get-FileHash $planPath).Hash -cne $PlanSha256){throw 'Pinned read-only plan required'}
$plan=Get-Content $planPath -Raw|ConvertFrom-Json
if(!$plan.verified -or !$plan.noFilesChanged -or $plan.files.Count -ne $rows.Count){throw 'Compression plan incomplete'}
if($plan.scriptSha256 -and ($plan.scriptSha256 -cne (Get-FileHash $PSCommandPath).Hash -or $plan.probeSha256 -cne (Get-FileHash (Join-Path $PSScriptRoot 'NtfsStorageProbe.cs')).Hash)){throw 'Planned compression source changed'}
foreach($row in $rows){$previous=@($plan.files|Where-Object path -CEQ $row.path);if($previous.Count -ne 1 -or $previous[0].sha256 -cne $row.sha256 -or $previous[0].sddl -cne $row.sddl -or $previous[0].eligible -ne $row.eligible -or $previous[0].before.FileId -ne $row.before.FileId -or $previous[0].before.Volume -ne $row.before.Volume -or $previous[0].before.Attributes -ne $row.before.Attributes -or $previous[0].before.Links -ne $row.before.Links){throw 'Planned file identity changed'}}
$operations=[Collections.Generic.List[object]]::new()
$result=@{verified=$false;allLogicalDataAndAclsUnchanged=$false;noFilesDeleted=$true;noDirectoriesMarked=$true;operatingSystemFilesTouched=$false;currentPrivateBindingRuntimeTouched=$false;freeBytesBefore=(Get-PSDrive C).Free;operations=$operations}
try{
    $compact=Join-Path $env:SystemRoot 'System32/compact.exe'
    foreach($row in $rows|Where-Object eligible){
        if((Get-PSDrive C).Free -lt (600MB+[long]$row.before.LogicalBytes+1MB)){throw 'Original reserve plus file-size scratch allowance required'}
        CheckPath $row.path $row.runtime
        $current=[Lea.Build.NtfsStorageProbe]::Read($row.path)
        if($current.FileId -ne $row.before.FileId -or $current.Volume -ne $row.before.Volume -or $current.Links -ne 1 -or (Get-FileHash $row.path).Hash -cne $row.sha256 -or (Get-Acl $row.path).Sddl -cne $row.sddl){throw 'Compression file changed before operation'}
        $operation=@{path=$row.path;sha256=$row.sha256;sddl=$row.sddl;before=$current;verified=$false};$operations.Add($operation)
        $operations|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'operations.json') -Encoding utf8
        $messages=@(& $compact /C /F /A /Q $row.path 2>&1);$code=$LASTEXITCODE
        $messages|Set-Content (Join-Path $out ($operations.Count.ToString('D2')+'-compact.txt')) -Encoding utf8
        $after=[Lea.Build.NtfsStorageProbe]::Read($row.path);$operation.after=$after;$operation.exitCode=$code
        if($code -ne 0 -or $after.FileId -ne $current.FileId -or $after.Volume -ne $current.Volume -or $after.Links -ne 1 -or $after.LogicalBytes -ne $current.LogicalBytes -or !($after.Attributes -band [int][IO.FileAttributes]::Compressed) -or ($after.Attributes -band [int][IO.FileAttributes]::ReparsePoint) -or (Get-FileHash $row.path).Hash -cne $row.sha256 -or (Get-Acl $row.path).Sddl -cne $row.sddl){throw 'Compression outcome/data/ACL verification failed'}
        $operation.verified=$true
        $operations|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'operations.json') -Encoding utf8
    }
    foreach($row in $rows){if((Get-FileHash $row.path).Hash -cne $row.sha256 -or (Get-Acl $row.path).Sddl -cne $row.sddl){throw 'Historical post-compression data/ACL mismatch'}}
    $result.allLogicalDataAndAclsUnchanged=$true;$result.verified=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.freeBytesAfter=(Get-PSDrive C).Free
    $result.storageCounterBytesSaved=($operations|Where-Object verified|ForEach-Object {[decimal]$_.before.StorageBytes-[decimal]$_.after.StorageBytes}|Measure-Object -Sum).Sum
    $result.scope='Plain NTFS compression of explicitly planned historical passing runtime files only. No /EXE, recursive directory marking, deletion or byte/ACL changes. Older timing observations are not new performance claims; free-space delta may include external changes.'
    $result|ConvertTo-Json -Depth 8|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object verified,storageCounterBytesSaved,allLogicalDataAndAclsUnchanged,freeBytesBefore,freeBytesAfter,error|ConvertTo-Json
}
