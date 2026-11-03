$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$out='I:/LeaBuildStorage/global-install-feature-audit-20261102-v1'
if(Test-Path $out){throw 'Preserve matched feature audit'}
if((Get-FileHash (Join-Path $root 'Lea.Tests/IsConnectedMain.cs')).Hash -cne '8AE71E18B5CDB42D43D61FFD912FA638FD018EB21DB7A85DDE6850806B9162F4' -or (Get-FileHash (Join-Path $root 'Lea.Tests/IsConnectedCases.js')).Hash -cne 'AE7B3FD5C013D83D2BFD3740890FD67E13BB8E46CA2C87DFC67B4011388623F6'){throw 'Feature assertion source differs'}
$cases=@(
    @{mode='trusted';hash='FC427EDDF4B292FB1D7D24A23059FC87CB57035CD6599F53F9201FF4B18EB023';files=10},
    @{mode='isolated';hash='75E76ACE29D3C84835E8BFECB2EC5DDF9889C5154690C7836F0AEF583A08B1AD';files=16}
)
$records=[Collections.Generic.List[object]]::new()
foreach($case in $cases){
    $folder=Join-Path $root ('continuity-evidence/global-install-feature-'+$case.mode+'-v1')
    $receipt=Join-Path $folder 'result.json'
    if((Get-FileHash $receipt).Hash -cne $case.hash){throw 'Feature receipt differs'}
    $r=Get-Content $receipt -Raw|ConvertFrom-Json
    if(!$r.passed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.before.Count -ne $case.files -or $r.runs.Count -ne 2 -or $r.browserAdopted -or $r.isolatedWorkerTested -ne ($case.mode -ceq 'isolated')){throw 'Matched feature boundary incomplete'}
    foreach($file in $r.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Feature input changed'}}
    foreach($file in $r.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Feature bytes/ACL changed'}}
    for($i=0;$i -lt 2;$i++){
        $run=$r.runs[$i];$budget=if($i -eq 0){120000}else{30000}
        if(!$run.passed -or $run.exitCode -ne 0 -or !$run.exitObserved -or !$run.outputDrained -or !$run.originalHandleCaptured -or !$run.ownedJob -or !$run.childRetirementIndependentlyAudited -or $run.activeAfterRetirement -ne 0 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.timedOut -or $run.killRequested -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000){throw 'Owned feature run incomplete'}
    }
    $footer='PASS Lea isConnected '+$case.mode+': nonmutating read, descriptors, brands, read-only, detached reads/write guard, revoked permission, cycle limit and recovery.'
    if(@(Get-Content $r.runs[1].stdout|Where-Object {$_ -ceq $footer}).Count -ne 1){throw 'Feature semantic footer missing/duplicated'}
    if((Get-FileHash (Join-Path $folder 'runtime/Lea.Script.dll')).Hash -cne 'F5BDB1FF4CAD596781E91D4FDAEED421E8B22C199EF97852BD96481D778E3B7B'){throw 'Feature parent differs'}
    if($case.mode -ceq 'isolated'){
        $workerPath='I:/LeaBuildStorage/global-install-worker-build-20261102-v1/result.json'
        if((Get-FileHash $workerPath).Hash -cne '21EF3E9F03E594F562596DA7008A97A282CFE49D8CEC6C1D8DBFBDA2E73B112C'){throw 'Feature matching worker differs'}
        $worker=Get-Content $workerPath -Raw|ConvertFrom-Json
        if(!$worker.passed -or $worker.products.Count -ne 6){throw 'Feature worker closure incomplete'}
        foreach($product in $worker.products){if((Get-FileHash (Join-Path $folder ('runtime/ScriptWorker/'+[IO.Path]::GetFileName($product.path)))).Hash -cne $product.sha256){throw 'Feature worker identity differs'}}
        $options=(Get-Content (Join-Path $folder 'runtime/ScriptWorker/Lea.Script.Worker.runtimeconfig.json') -Raw|ConvertFrom-Json).runtimeOptions
        if($options.framework.version -cne '8.0.33' -or $options.rollForward -cne 'Disable'){throw 'Feature worker runtime policy differs'}
    }
    $records.Add(@{mode=$case.mode;receiptSha256=$case.hash;files=$case.files;passed=$true})
}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($case in $cases){
    $folder=Join-Path $root ('continuity-evidence/global-install-feature-'+$case.mode+'-v1')
    $target=Join-Path $out $case.mode;New-Item -ItemType Directory -Path $target|Out-Null
    foreach($file in Get-ChildItem $folder -File){Copy-Item -LiteralPath $file.FullName -Destination $target;if((Get-FileHash (Join-Path $target $file.Name)).Hash -cne (Get-FileHash $file.FullName).Hash){throw 'Feature evidence readback differs'}}
}
@{evidenceVerified=$true;passed=$true;campaigns=$records;browserAdopted=$false;startupReliabilityProven=$false;scope='Exact global-install/isConnected parent and worker feature behavior only. Original assertions/deadlines and sandbox flags retained; broader DOM/native-view/host/full-suite qualification pending.'}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
