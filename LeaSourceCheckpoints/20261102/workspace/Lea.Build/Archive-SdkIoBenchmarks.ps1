param([switch]$LegacyFullSuites,[switch]$SupersededWorkerBaselines,[switch]$RejectedSessionIoPasses,[switch]$SupersededComponentProbes,[switch]$SupersededSdkPasses)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$allowed=[IO.Path]::GetFullPath((Join-Path $root 'continuity-evidence'))
$out='I:\LeaBuildStorage\sdk-io-benchmark-archives-20261101-v1'
if(@($LegacyFullSuites,$SupersededWorkerBaselines,$RejectedSessionIoPasses,$SupersededComponentProbes,$SupersededSdkPasses|Where-Object {$_}).Count -gt 1){throw 'Separate archive selections required'}
if(!$LegacyFullSuites -and !$SupersededWorkerBaselines -and !$RejectedSessionIoPasses -and !$SupersededComponentProbes -and !$SupersededSdkPasses -and (Test-Path -LiteralPath $out)){throw 'Preserve benchmark archives'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','SdkScriptLocal','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$cases=@(
    @('baseline-v1','A0F71B58E9DEE613417F84B96AB8EA1792987B476B9F4BF67FCBD7256C760A77'),
    @('candidate-v1','08FF79210A09DC309DAACF9ADAE62775161F71518C1A0ACD3A97A9EA3D04A1DB'),
    @('candidate-v2','68AFAF2D6477F625AAFCB981D07698BBB485D56A5DC7460F1342EE77FC4468F3'),
    @('baseline-v2','C0E5A167A5F02B2757B361CDC70948DC04C9F74657BA6D8A8EDE9E539D26F6D2')
)
if($LegacyFullSuites){
    $allowed=[IO.Path]::GetFullPath((Join-Path $root 'continuity-evidence/temporal-startup-v1'))
    $out='I:\LeaBuildStorage\legacy-full-suite-runtimes-20261101-v3'
    if(Test-Path -LiteralPath $out){throw 'Preserve legacy suite archives'}
    $cases=@(@('initial-font-script-suite-v1','2990322C7945B11249BFE198F8296ECFF3345CF35BBD6CB48E888BF6AC36F27B','initial-font-script-owned-run-v1'),@('shutdown-script-suite-v1','5F4C37A3C7858E1C8EB95E57147A82FCED3D19D8AAE9E5C97EA1C0C29F39C5E0','shutdown-script-owned-run-v1'))
}
if($SupersededWorkerBaselines){
    $out='I:\LeaBuildStorage\superseded-worker-baselines-20261101-v1'
    if(Test-Path -LiteralPath $out){throw 'Preserve worker baseline archives'}
    $cases=@(@('sdk-script-worker-runtime-v2','E0BA2833A6FF54FF8216455CD630850A205F2E9C34E10D9CBC23C16962B1320E'),@('sdk-script-worker-runtime-v3','B5F55DA81F0F67EBE163933C153BC107E5A7D29E0CCC7551409E0127C9FDD30E'))
}
if($RejectedSessionIoPasses){
    $out='I:\LeaBuildStorage\rejected-session-io-passes-20261101-v1'
    if(Test-Path -LiteralPath $out){throw 'Preserve rejected experiment archives'}
    $cases=@(@('sdk-script-session-io-adapter-v1','C7B86C94CE91B5C15285F3099CC0AD58C07C21724BD60366F10D48024B0ECD91'),@('sdk-script-session-io-lifecycle-v2','8D5F034EF2EBEC7D14257270689C571B28309F6266B2A13A1610DEB4EF9DF617'),@('sdk-script-session-io-runtime-v1','DEE05B8BD8552ADA0B5D9AF27B551CF4B2853BAAA3D5D221FB126D77A1994184'))
}
if($SupersededComponentProbes){
    $out='I:\LeaBuildStorage\superseded-component-probes-20261101-v2'
    if(Test-Path -LiteralPath $out){throw 'Preserve component archives'}
    $cases=,@('sdk-component-probes-runtime-v2','D66767E5C517FD9F1D99E20FAEF8211B16662A9CCCE6AB3AFE07C437970E6B20')
}
if($SupersededSdkPasses){
    $out='I:\LeaBuildStorage\superseded-sdk-passes-20261101-v1'
    if(Test-Path -LiteralPath $out){throw 'Preserve SDK pass archives'}
    $cases=@(
        @('sdk-script-pooled-io-adapter-v1','6E0BF5E7C6AEDC708928376FBC65661386C9A02149AF942DC1DD2C07506F98D3'),
        @('sdk-script-pooled-io-runtime-v2','FF0A0DF9BDC982C16D0AC226E57FA47800BE578D5380F77838283361F63AA6D8'),
        @('sdk-script-adapter-runtime-v4','16ECB3BC687A29068BD397ECD8AC2B39564811C97A57C6EF93F3879BDBF458E9'),
        @('sdk-script-worker-runtime-v4','3C8FBCDBE7E87C035F77B41B1DB2C22FD694B5E3D57598B106B70C87CE42D519')
    )
}
New-Item -ItemType Directory -Path $out|Out-Null
$records=[Collections.Generic.List[object]]::new();$deleted=[Collections.Generic.List[object]]::new()
$result=@{verified=$false;records=$records;deleted=$deleted;sourceDeleted=$false;failureEvidenceTouched=$false;freeBefore=(Get-PSDrive C).Free}
try{
    Add-Type -AssemblyName System.IO.Compression
    foreach($case in $cases){
        $target=[IO.Path]::GetFullPath((Join-Path $allowed $(if($LegacyFullSuites){$case[0]+'/runtime'}elseif($SupersededWorkerBaselines -or $RejectedSessionIoPasses -or $SupersededComponentProbes -or $SupersededSdkPasses){$case[0]}else{'sdk-script-io-benchmark-'+$case[0]})))
        if(!$target.StartsWith($allowed+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Archive target escape'}
        $item=Get-Item -LiteralPath $target
        while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse archive ancestor refused'};$item=$item.Parent}
        $tree=@(Get-ChildItem -LiteralPath $target -Recurse -Force)
        if(@($tree|Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}).Count){throw 'Reparse archive member refused'}
        $receipt=if($LegacyFullSuites){Join-Path $allowed ($case[2]+'/result.json')}else{Join-Path $target 'result.json'}
        if((Get-FileHash $receipt).Hash -cne $case[1]){throw 'Completed benchmark receipt differs'}
        $run=Get-Content $receipt -Raw|ConvertFrom-Json
        if(!$run.passed -or !$run.inputsUnchanged -or $run.ownerAttention -or @($run.runs|Where-Object {!$_.passed -or $_.activeAfterRetirement -ne 0 -or !$_.outputDrained}).Count){throw 'Passing retired test required'}
        if($LegacyFullSuites){
            if($run.candidate -cne $target -or $run.before.Count -ne 65 -or $run.changedFiles.Count -or !$run.footerVerified -or !$run.traceEnvironmentRestored){throw 'Legacy passing suite closure incomplete'}
            foreach($file in $run.before){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256 -or (Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Legacy qualified runtime differs'}}
        }elseif($SupersededComponentProbes){
            if($run.before.Count -ne 27 -or $run.changedFiles.Count -or $run.reports.Count -ne 5 -or $run.workerEntryPointExecuted -or $run.nativeIsolationTest){throw 'Superseded component closure incomplete'}
            foreach($file in $run.before){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256 -or (Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Qualified component file differs'}}
        }elseif(!$run.workerAclRestored -or (!$SupersededWorkerBaselines -and !$RejectedSessionIoPasses -and !$SupersededSdkPasses -and !$run.benchmarkOnly)){throw 'Passing runtime closure incomplete'}
        $rows=@(foreach($file in $tree|Where-Object {!$_.PSIsContainer}){@{relative=[IO.Path]::GetRelativePath($target,$file.FullName).Replace('\','/');path=$file.FullName;bytes=$file.Length;sha256=(Get-FileHash -LiteralPath $file.FullName).Hash}})
        $bound=if($LegacyFullSuites){75MB}elseif($SupersededComponentProbes){60MB}else{20MB}
        if(($rows|Measure-Object bytes -Sum).Sum -gt $bound){throw 'Bounded runtime archive scope exceeded'}
        if($LegacyFullSuites -and ($rows.Count -ne 65 -or @($rows|Where-Object {[IO.Path]::GetExtension($_.relative) -notin '.dll','.exe','.pdb','.config','.json' -and $_.relative -cne 'ComponentProbes/wire/oracle.sha256'}).Count)){throw 'Unexpected non-runtime legacy member'}
        $holds=[Collections.Generic.List[IDisposable]]::new()
        try{
            foreach($row in $rows){$holds.Add([IO.File]::Open($row.path,'Open','Read','Read'))}
            $zip=Join-Path $out ($case[0]+'.zip')
            [IO.Compression.ZipFile]::CreateFromDirectory($target,$zip,[IO.Compression.CompressionLevel]::Optimal,$false)
            $archive=[IO.Compression.ZipFile]::OpenRead($zip)
            try{
                if($archive.Entries.Count -ne $rows.Count){throw 'ZIP inventory differs'}
                foreach($row in $rows){
                    $matches=@($archive.Entries|Where-Object FullName -ceq $row.relative)
                    if($matches.Count -ne 1 -or $matches[0].Length -ne $row.bytes){throw 'ZIP member differs'}
                    $stream=$matches[0].Open();try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
                    if($hash -cne $row.sha256 -or (Get-FileHash $row.path).Hash -cne $row.sha256){throw 'ZIP/current source hash differs'}
                }
            }finally{$archive.Dispose()}
            $removable=if($LegacyFullSuites){@($rows|Where-Object {$_.relative -cne 'ComponentProbes/wire/oracle.sha256'})}else{@($rows|Where-Object {$_.relative -notlike '*/*' -and [IO.Path]::GetExtension($_.relative) -ceq '.dll' -or $_.relative.StartsWith('ScriptWorker/')})}
            $expected=if($LegacyFullSuites){64}else{16}
            if($SupersededComponentProbes){$removable=@($rows|Where-Object {$_.path -in $run.before.path -and [IO.Path]::GetExtension($_.path) -in '.dll','.exe','.json'});$expected=26}
            if($removable.Count -ne $expected){throw 'Inspected runtime member count differs'}
            if($SupersededWorkerBaselines -or $RejectedSessionIoPasses -or $SupersededSdkPasses){
                foreach($row in $removable){
                    $pins=@($run.inputs|Where-Object path -ceq $row.path|Select-Object -ExpandProperty sha256 -Unique)
                    if($pins.Count -ne 1 -or $pins[0] -cne $row.sha256){throw 'Qualified baseline runtime bytes differ'}
                }
                foreach($file in $run.workerAclBefore){if((Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Qualified baseline ACL differs'}}
            }
            $record=@{case=$case[0];target=$target;archive=$zip;archiveSha256=(Get-FileHash $zip).Hash;files=$rows;verified=$true;runtimeFiles=$removable;runtimeFilesRemoved=$false}
            $records.Add($record)
            $record|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out ($case[0]+'-verified.json')) -Encoding utf8
        }finally{foreach($hold in $holds){$hold.Dispose()}}
        foreach($row in $removable){
            $path=[IO.Path]::GetFullPath($row.path)
            if(!$path.StartsWith($target+'\',[StringComparison]::OrdinalIgnoreCase) -or (Get-FileHash -LiteralPath $path).Hash -cne $row.sha256){throw 'Deletion target changed or escaped'}
            Remove-Item -LiteralPath $path
            $deleted.Add(@{path=$path;archive=$zip;relative=$row.relative;sha256=$row.sha256;bytes=$row.bytes})
        }
        $record.runtimeFilesRemoved=$true
        $record|ConvertTo-Json -Depth 6|Set-Content (Join-Path $target 'runtime-archive.json') -Encoding utf8
    }
    $result.verified=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.freeAfter=(Get-PSDrive C).Free;$result.bytesRemoved=($deleted|Measure-Object bytes -Sum).Sum
    $result|ConvertTo-Json -Depth 9|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object verified,bytesRemoved,freeBefore,freeAfter,error|ConvertTo-Json
}
