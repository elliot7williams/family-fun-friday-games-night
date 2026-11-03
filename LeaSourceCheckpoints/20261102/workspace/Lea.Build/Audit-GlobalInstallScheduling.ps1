$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$folder=Join-Path $root 'continuity-evidence/global-install-scheduling-v2'
$out='I:/LeaBuildStorage/global-install-scheduling-audit-20261102-v2'
if(Test-Path $out){throw 'Preserve scheduling audit'}
$receipt=Join-Path $folder 'result.json'
if((Get-FileHash $receipt).Hash -cne 'F680DDEE0DE0E7EC0DA98378B1983815302E7C6FFF8925B092629BA2C859BDF8'){throw 'Scheduling receipt differs'}
$r=Get-Content $receipt -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.completed -or !$r.candidateOnly -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.before.Count -ne 13 -or $r.runs.Count -ne 3 -or $r.cases.Count -ne 2 -or $r.browserAdopted -or $r.isolatedWorkerTested){throw 'Trusted scheduling qualification boundary incomplete'}
foreach($file in $r.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Scheduling input changed'}}
foreach($file in $r.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Scheduling runtime bytes/ACL changed'}}
$names=@('compile-connectivity-test','trusted-Timers','trusted-Frames')
for($i=0;$i -lt 3;$i++){
    $run=$r.runs[$i];$budget=if($i -eq 0){120000}else{30000}
    if($run.name -cne $names[$i] -or !$run.passed -or $run.exitCode -ne 0 -or !$run.exitObserved -or !$run.outputDrained -or !$run.originalHandleCaptured -or !$run.ownedJob -or !$run.childRetirementIndependentlyAudited -or $run.activeAfterRetirement -ne 0 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.timedOut -or $run.killRequested -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000){throw 'Owned scheduling process incomplete'}
}
$counts=@(59,26);$suites=@('Timers','Frames')
for($i=0;$i -lt 2;$i++){
    if(!$r.cases[$i].passed -or $r.cases[$i].name -cne $suites[$i] -or $r.cases[$i].checks -ne $counts[$i]){throw 'Scheduling assertion count differs'}
    $expected='PASS Lea scheduling '+$suites[$i]+': '+$counts[$i]+' checks.'
    if(@(Get-Content $r.runs[$i+1].stdout|Where-Object {$_ -ceq $expected}).Count -ne 1){throw 'Scheduling footer missing/duplicated'}
}
if((Get-FileHash (Join-Path $folder 'runtime/Lea.Script.dll')).Hash -cne 'F5BDB1FF4CAD596781E91D4FDAEED421E8B22C199EF97852BD96481D778E3B7B'){throw 'Scheduling candidate differs'}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($file in Get-ChildItem $folder -File){Copy-Item -LiteralPath $file.FullName -Destination $out;if((Get-FileHash (Join-Path $out $file.Name)).Hash -cne (Get-FileHash $file.FullName).Hash){throw 'Scheduling evidence readback differs'}}
@{evidenceVerified=$true;passed=$true;checks=85;trustedTimersChecks=59;trustedFramesChecks=26;workerQualified=$false;browserAdopted=$false;timeoutFixed=$false;scope=$r.scope}|ConvertTo-Json -Depth 4|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
