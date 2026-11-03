$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$out='I:/LeaBuildStorage/lazy-map-audit-20261102-v1'
if(Test-Path $out){throw 'Preserve lazy map failure audit'}
$sourcePath='I:/LeaBuildStorage/lazy-map-source-20261102-v1/source.json'
if((Get-FileHash $sourcePath).Hash -cne '4E8524E487B5F6E59408097EE7C5A7950C7BCADFF272B45BD7847B64334B5803'){throw 'Lazy map source proof differs'}
$source=Get-Content $sourcePath -Raw|ConvertFrom-Json
if(!$source.verified -or !$source.inverseVerified -or $source.negativeControls -ne 16 -or $source.privateMapsDeferred -ne 2 -or !$source.constructorCaptured -or $source.hostPolicyChanged -or $source.files.Count -ne 2){throw 'Lazy map source boundary incomplete'}
foreach($file in $source.files){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-FileHash $file.original).Hash -cne $file.originalSha256){throw 'Lazy map source changed'}}
$nodePath='I:/LeaBuildStorage/lazy-map-source-20261102-v1/node-allocation-test.json'
$node=Get-Content $nodePath -Raw|ConvertFrom-Json
if(!$node.passed -or $node.checks -ne 312 -or $node.cases.Count -ne 4 -or $node.sourceProofSha256 -cne (Get-FileHash $sourcePath).Hash -or $node.testSha256 -cne (Get-FileHash (Join-Path $root 'Lea.Tests/LazyMapProjection.test.cjs')).Hash){throw 'Lazy map mock evidence differs'}
foreach($case in $node.cases){$expected=if($case.kind -ceq 'candidate'){1}else{3};if($case.checks -ne 78 -or $case.eagerMaps -ne $expected -or $case.finalMaps -ne 3){throw 'Counted mock map allocations differ'}}
$buildPath='I:/LeaBuildStorage/lazy-map-build-20261102-v1/result.json'
if((Get-FileHash $buildPath).Hash -cne 'F4194E982B87835E9EC08F553136E558C0C0652C48961D97E1FA3A794547007C'){throw 'Lazy map build receipt differs'}
$build=Get-Content $buildPath -Raw|ConvertFrom-Json
if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.runs.Count -ne 1 -or $build.sha256 -cne 'A17246FBF5F0EA68B931E7125ECD41189CC21ABA8D798396CB339C74F7728098'){throw 'Lazy map build incomplete'}
foreach($file in $build.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Lazy map build input changed'}}
foreach($name in @('8.cs','10.cs','2.cs')){
    if((Get-FileHash ('I:/LeaBuildStorage/lazy-map-build-20261102-v1/source/Lea.Script/'+$name)).Hash -cne (Get-FileHash ('I:/LeaBuildStorage/global-install-build-20261102-v1/source/Lea.Script/'+$name)).Hash){throw 'Host evaluation/policy/checkpoints must remain unchanged'}
}
$cases=@(@{version=1;hash='BD7F51B5473BBD51D647B6444B8916D204C92716053B80A927045EC95B33CF5B';fixture='javascript-view.html';failure='field identity value and CSS retained'},@{version=2;hash='068DD6A27069736060383DB42EEAD94EBA11C0A8141870C7824CEF69B5D41C45';fixture='javascript-attributes.html';failure='Script budget exceeded. phase=bootstrap; reason=TimeoutException; stage=document'})
$records=[Collections.Generic.List[object]]::new()
foreach($case in $cases){
    $folder=Join-Path $root ('continuity-evidence/lazy-map-trusted-v'+$case.version)
    $receipt=Join-Path $folder 'result.json'
    if((Get-FileHash $receipt).Hash -cne $case.hash){throw 'Lazy map campaign receipt differs'}
    $r=Get-Content $receipt -Raw|ConvertFrom-Json
    if($r.passed -or $r.completed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.before.Count -ne 13 -or $r.runs.Count -ne 2 -or $r.cases.Count -ne 1 -or $r.cases[0].passed -or $r.cases[0].checks -ne 0){throw 'Expected failed lazy map boundary differs'}
    foreach($file in $r.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Lazy map campaign input changed'}}
    foreach($file in $r.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Lazy map campaign bytes/ACL changed'}}
    if(@($r.inputs|Where-Object {([IO.Path]::GetFileName($_.path)) -ceq $case.fixture}).Count -ne 1){throw 'Lazy map fixture identity differs'}
    for($i=0;$i -lt 2;$i++){
        $run=$r.runs[$i];$budget=if($i -eq 0){120000}else{30000};$exit=if($i -eq 0){0}else{1}
        if(!$run.exitObserved -or !$run.outputDrained -or !$run.originalHandleCaptured -or !$run.ownedJob -or !$run.childRetirementIndependentlyAudited -or $run.activeAfterRetirement -ne 0 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.timedOut -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.exitCode -ne $exit){throw 'Lazy map process retirement incomplete'}
    }
    $raw=Get-Content $r.runs[1].stderr -Raw
    if(!$raw.Contains($case.failure) -or @((Get-Content $r.runs[1].stdout)|Where-Object {$_.StartsWith('PASS Lea lazy-map trusted ')}).Count -ne 0 -or @(Get-ChildItem $folder -File -Filter '*FirstUse*').Count){throw 'Expected failed suite/unexecuted probe boundary differs'}
    $records.Add(@{version=$case.version;fixture=$case.fixture;passed=$false;failure=$case.failure;killRequested=$r.runs[1].killRequested;firstUseExecuted=$false})
}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($case in $cases){
    $folder=Join-Path $root ('continuity-evidence/lazy-map-trusted-v'+$case.version);$target=Join-Path $out ('v'+$case.version);New-Item -ItemType Directory -Path $target|Out-Null
    foreach($file in Get-ChildItem $folder -File){Copy-Item -LiteralPath $file.FullName -Destination $target;if((Get-FileHash (Join-Path $target $file.Name)).Hash -cne (Get-FileHash $file.FullName).Hash){throw 'Lazy map evidence readback differs'}}
}
Copy-Item -LiteralPath $nodePath -Destination $out
if((Get-FileHash (Join-Path $out 'node-allocation-test.json')).Hash -cne (Get-FileHash $nodePath).Hash){throw 'Node allocation evidence readback differs'}
@{evidenceVerified=$true;candidateQualified=$false;mockChecks=312;mockEagerMapsBefore=3;mockEagerMapsAfter=1;hostPolicyUnchanged=$true;campaigns=$records;startupFixed=$false;nativeFirstUseExecuted=$false;workerBuilt=$false;browserAdopted=$false;scope='Mock allocation/equivalence proof only. V1 invalid-fixture failure excluded from qualification; corrected v2 failed at cold trusted bootstrap before class-list assertions. Preserve killRequested=true on v2. First-use probe was not reached; no retries or performance/reliability claim.'}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
