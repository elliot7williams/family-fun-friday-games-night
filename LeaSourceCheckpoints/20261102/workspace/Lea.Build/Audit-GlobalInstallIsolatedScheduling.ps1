$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$folder=Join-Path $root 'continuity-evidence/global-install-isolated-scheduling-v2'
$out='I:/LeaBuildStorage/global-install-isolated-scheduling-audit-20261102-v2'
if(Test-Path $out){throw 'Preserve isolated scheduling audit'}
$receipt=Join-Path $folder 'result.json'
if((Get-FileHash $receipt).Hash -cne '170F5F1992E5BA32F406402FB23BC01A1B8E71DF34E9B7D44ACBAED277152FC0'){throw 'Isolated scheduling receipt differs'}
$r=Get-Content $receipt -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.completed -or !$r.candidateOnly -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.before.Count -ne 19 -or $r.runs.Count -ne 3 -or $r.cases.Count -ne 2 -or $r.browserAdopted -or !$r.isolatedWorkerTested){throw 'Isolated scheduling boundary incomplete'}
foreach($file in $r.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Isolated scheduling input changed'}}
foreach($file in $r.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Isolated scheduling runtime bytes/ACL changed'}}
$names=@('compile-connectivity-test','isolated-Timers','isolated-Frames')
function Owned($run,[int]$budget){
    if(!$run.passed -or $run.exitCode -ne 0 -or !$run.exitObserved -or !$run.outputDrained -or !$run.originalHandleCaptured -or !$run.ownedJob -or !$run.childRetirementIndependentlyAudited -or $run.activeAfterRetirement -ne 0 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.timedOut -or $run.killRequested -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000){throw 'Owned scheduling/build process incomplete'}
}
for($i=0;$i -lt 3;$i++){$budget=if($i -eq 0){120000}else{30000};if($r.runs[$i].name -cne $names[$i]){throw 'Scheduling process order differs'};Owned $r.runs[$i] $budget}
. (Join-Path $PSScriptRoot 'Read-AllStartupTraceProof.ps1')
$counts=@(59,27);$suites=@('Timers','Frames')
for($i=0;$i -lt 2;$i++){
    $case=$r.cases[$i];$run=$r.runs[$i+1]
    if(!$case.passed -or $case.name -cne $suites[$i] -or $case.checks -ne $counts[$i] -or !$case.traceActivationVerified -or !$case.traceEnvironmentRestored){throw 'Isolated assertion count/trace differs'}
    $expected='PASS Lea isolated scheduling '+$suites[$i]+': '+$counts[$i]+' checks.'
    if(@(Get-Content $run.stdout|Where-Object {$_ -ceq $expected}).Count -ne 1){throw 'Isolated footer missing/duplicated'}
    $trace=Read-AllStartupTraceProof -Lines @(Get-Content $run.stderr) -StreamsComplete $true -ExitObserved $true -TimedOut $false -KillRequested $false
    if(!$trace.traceActivationVerified -or !$trace.traceEnvironmentRestored){throw 'Raw startup trace not restored'}
}
if(@(Get-Content $r.runs[1].stdout|Where-Object {$_ -ceq 'LEA_SCHEDULING_GUARDS 21'}).Count -ne 1){throw 'Live timer sandbox observations differ'}
$workerPath='I:/LeaBuildStorage/global-install-worker-build-20261102-v1/result.json'
if((Get-FileHash $workerPath).Hash -cne '21EF3E9F03E594F562596DA7008A97A282CFE49D8CEC6C1D8DBFBDA2E73B112C'){throw 'Matching worker receipt differs'}
$worker=Get-Content $workerPath -Raw|ConvertFrom-Json
if(!$worker.passed -or !$worker.inputsUnchanged -or $worker.ownerAttention -or $worker.products.Count -ne 6 -or $worker.runs.Count -ne 2 -or $worker.resourceSha256 -cne '088847DC78DC1B70C5852A2C8934601110F82179A889AC9ADB1D1FF210A1B8EB'){throw 'Matching worker proof incomplete'}
foreach($run in $worker.runs){Owned $run 120000}
foreach($file in $worker.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Matching worker source/input changed'}}
foreach($file in $worker.products){
    $staged=Join-Path $folder ('runtime/ScriptWorker/'+[IO.Path]::GetFileName($file.path))
    if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-FileHash $staged).Hash -cne $file.sha256){throw 'Worker closure identity differs'}
}
$options=(Get-Content (Join-Path $folder 'runtime/ScriptWorker/Lea.Script.Worker.runtimeconfig.json') -Raw|ConvertFrom-Json).runtimeOptions
if($options.framework.version -cne '8.0.33' -or $options.rollForward -cne 'Disable'){throw 'Exact worker runtime policy differs'}
if((Get-FileHash (Join-Path $folder 'runtime/Lea.Script.dll')).Hash -cne 'F5BDB1FF4CAD596781E91D4FDAEED421E8B22C199EF97852BD96481D778E3B7B' -or (Get-FileHash 'I:/LeaBuildStorage/global-install-build-20261102-v1/source/Lea.Script/Dom.compact.js').Hash -cne $worker.resourceSha256){throw 'Matched parent/resource differs'}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($file in @(Get-ChildItem $folder -File)+@(Get-Item $workerPath)){
    $name=if($file.FullName -ceq $workerPath.Replace('/','\')){'worker-build-result.json'}else{$file.Name}
    if($file.FullName -like '*global-install-worker-build*'){$name='worker-build-result.json'}
    Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $out $name)
    if((Get-FileHash (Join-Path $out $name)).Hash -cne (Get-FileHash $file.FullName).Hash){throw 'Isolated scheduling evidence readback differs'}
}
@{evidenceVerified=$true;passed=$true;checks=86;timerGuardObservations=21;trustedResultsTransferred=$false;browserAdopted=$false;timeoutFixed=$false;scope=$r.scope}|ConvertTo-Json -Depth 4|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
