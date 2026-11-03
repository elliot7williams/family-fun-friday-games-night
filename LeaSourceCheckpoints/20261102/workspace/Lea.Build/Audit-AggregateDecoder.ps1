param([switch]$PrintSdk)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'continuity-evidence/sdk-aggregate-decoder-audit-v1'
if($PrintSdk){$out=Join-Path $root 'continuity-evidence/sdk-print-aggregate-decoder-audit-v1'}
if(Test-Path $out){throw 'Preserve decoder audit'}
$buildPath=Join-Path $root 'continuity-evidence/sdk-portable-source-v1/result.json'
$buildPin='57E993FAE281B565D502BBEE77A95C98A71B5A0816C5DC5D66250F3EE9C32E36'
if($PrintSdk){$buildPath=Join-Path $root 'continuity-evidence/sdk-print-aggregate-runtime-v1/result.json';$buildPin='D6D0E65685041CFB517712C92824F3AB5E50E22E4ED6DB5086E882B058D0B676'}
if((Get-FileHash $buildPath).Hash -cne $buildPin){throw 'Aggregate receipt differs'}
$build=Get-Content $buildPath -Raw|ConvertFrom-Json
if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention){throw 'Aggregate proof incomplete'}
if($PrintSdk){if(!$build.aggregateProducts -or $build.runs.Count -ne 4){throw 'Print aggregate proof incomplete'}}elseif(!$build.aggregateSourceBuildPassed -or $build.sourceBuiltProducts.Count -ne 4){throw 'Aggregate source build incomplete'}
$products=@{
    'Lea.Engine.dll'='DC71E2B9DA704135CA022E5BBC2F1C57BA8046621C41E40AB98C7DC354BD6213'
    'Lea.Win.dll'='6F907862F57C7640A8B0E7BE2C22DFEDD18B3966D472958FCFCEDA53A00F0F52'
    'Lea.Image.exe'='A4E44422DE7A0F07A3DFDE0C6827F97C6B818A35D94D8ABD54F4CBF1692E7E42'
}
$cases=@(
    @('sdk-aggregate-decoder-retirement-v1','057790C6A830481B0154FC668A50D4A6CBC078929D00196F85CDF87B54E5F92B','Decoder retirement passed: 81'),
    @('sdk-aggregate-decoder-shutdown-race-v1','80B92A594BCF92333DC58D2B5F50882A0561EE0C6F145E3D49F372CA4D128A77','Decoder shutdown race passed: 14')
)
if($PrintSdk){
    $products=@{'Lea.Engine.dll'='5B8A501DA627467A6C261A355917DE5F73DF8F00758B49C1D240CF7D596E8A4A';'Lea.Win.dll'='A622A1C266AD7CA27F437832E390E594F7E419CAFED700C9F466F0517970CE0D';'Lea.Image.exe'='B5E1A90210B53FC3F957D0D8DF0E3EDBCA9D7AB7B825992A82C26648038BC8E7'}
    $cases=@(
        @('sdk-print-aggregate-decoder-retirement-v1','77234D023B8554545534A3FB5C7C6A30604184BC38829349E32428A87F6D85A9','Decoder retirement passed: 81'),
        @('sdk-print-aggregate-decoder-shutdown-race-v1','7696D49DFB87F8495F3183A1348EFB4ECD4884C2719E86B373095209170DB4CC','Decoder shutdown race passed: 14')
    )
}
foreach($case in $cases){
    $directory=Join-Path $root ('continuity-evidence/'+$case[0]);$receipt=Join-Path $directory 'result.json'
    if((Get-FileHash $receipt).Hash -cne $case[1]){throw 'Decoder receipt differs'}
    $r=Get-Content $receipt -Raw|ConvertFrom-Json
    if($PrintSdk -and !$r.printSdk){throw 'Print SDK identity mode missing'}
    if(!$r.passed -or !$r.aggregateSdkPair -or !$r.sdkPair -or !$r.realWorkerCampaign -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.error -or $r.runs.Count -ne 2){throw 'Decoder gates incomplete'}
    if(!$r.decoderAclRestored -or $r.decoderAclBefore -cne $r.decoderAclAfter -or (Get-Acl (Join-Path $directory 'Lea.Image.exe')).Sddl -cne $r.decoderAclBefore){throw 'Worker ACL not restored'}
    if($r.runs[0].parentBudgetMs -ne 120000 -or $r.runs[1].parentBudgetMs -ne 30000){throw 'Original deadlines differ'}
    foreach($run in $r.runs){
        if(!$run.passed -or $run.exitCode -ne 0 -or !$run.exitObserved -or $run.timedOut -or $run.killRequested -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or $run.retirementAndDrainElapsedMs -gt 5000 -or $run.elapsedMs -gt $run.parentBudgetMs){throw 'Execution or retirement failed'}
    }
    $lines=@(Get-Content $r.runs[1].stdout)
    if($lines[-1] -cne $case[2] -or @($lines|Where-Object {$_ -ceq $case[2]}).Count -ne 1){throw 'Terminal test footer differs'}
    foreach($name in $products.Keys){
        $copy=Join-Path $directory $name;$pin=$products[$name]
        $rows=@($r.inputs|Where-Object path -ceq $copy)
        $original=Join-Path $root ('continuity-evidence/sdk-portable-source-v1/bundle/Sample/bin/Release/'+$name)
        if($PrintSdk){$original=Join-Path $root ('continuity-evidence/sdk-print-aggregate-source-v1/bundle/Sample/bin/Release/'+$name)}
        if($rows.Count -ne 1 -or $rows[0].sha256 -cne $pin -or (Get-FileHash $copy).Hash -cne $pin -or (Get-FileHash $original).Hash -cne $pin){throw 'Tested aggregate product differs'}
    }
    foreach($inputFile in $r.inputs){if((Get-FileHash $inputFile.path).Hash -cne $inputFile.sha256){throw 'Input changed after test'}}
}
New-Item -ItemType Directory -Path $out | Out-Null
@{verified=$true;products=$products;cases=$cases;retirementChecks=81;shutdownRaceChecks=14;globalSecurityQualified=$false;completeSdk=$false;runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'result.json') -Encoding utf8
Get-FileHash (Join-Path $out 'result.json')
