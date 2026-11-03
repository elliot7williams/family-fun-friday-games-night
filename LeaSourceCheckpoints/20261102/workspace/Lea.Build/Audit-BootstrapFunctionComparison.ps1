$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$folder=Join-Path $root 'continuity-evidence/bootstrap-function-comparison-v1'
$out='I:/LeaBuildStorage/bootstrap-function-comparison-audit-20261102-v1'
if(Test-Path $out){throw 'Preserve rejected scope experiment audit'}
$receipt=Join-Path $folder 'result.json'
if((Get-FileHash $receipt).Hash -cne '92627174E37475AE86DCA8C96514766BF012076C0CE34A8A45C2CD6B4F315FD9'){throw 'Scope experiment receipt differs'}
$r=Get-Content $receipt -Raw|ConvertFrom-Json
if($r.passed -or !$r.comparisonCompleted -or !$r.baselineReused -or !$r.candidateOnly -or $r.runtimeQualified -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.before.Count -ne 20 -or $r.runs.Count -ne 5 -or $r.observations.Count -ne 4){throw 'Expected completed failed scope experiment differs'}
foreach($file in $r.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Scope experiment input changed'}}
foreach($file in $r.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Scope experiment bytes/ACL changed'}}
$order=@('baseline','candidate','candidate','baseline');$rows=[Collections.Generic.List[object]]::new()
for($i=0;$i -lt 5;$i++){
    $run=$r.runs[$i];$budget=if($i -eq 0){120000}else{30000};$exit=if($i -eq 2){1}else{0}
    if(!$run.exitObserved -or !$run.outputDrained -or !$run.originalHandleCaptured -or !$run.ownedJob -or !$run.childRetirementIndependentlyAudited -or $run.activeAfterRetirement -ne 0 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.timedOut -or $run.killRequested -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.exitCode -ne $exit -or $run.passed -ne ($i -ne 2)){throw 'Owned scope experiment result differs'}
    if($i -eq 0){continue}
    if($run.name -cne ('bootstrap-'+$i+'-'+$order[$i-1])){throw 'Scope experiment order differs'}
    $line=@(Get-Content $run.stdout|Where-Object {$_.StartsWith('LEA_BOOTSTRAP_COMPARISON ')})
    if($line.Count -ne 1){throw 'Unique raw scope comparison report required'}
    $report=$line[0].Substring('LEA_BOOTSTRAP_COMPARISON '.Length)|ConvertFrom-Json
    if($report.ok -ne ($i -ne 2)){throw 'Scope comparison outcome differs'}
    if($i -eq 2 -and ($report.status -cne 'limit' -or $report.message -cne 'Script budget exceeded. phase=bootstrap; reason=TimeoutException; stage=document')){throw 'Expected candidate cold timeout differs'}
    $rows.Add(@{iteration=$i;variant=$order[$i-1];report=$report;retirementMs=$run.retirementAndDrainElapsedMs})
}
$candidate=Join-Path $folder 'runtime';$baseline=Join-Path $root 'continuity-evidence/global-install-comparison-v1/runtime'
foreach($file in Get-ChildItem $candidate -File){
    $expected=if($file.Name -ceq 'Lea.Script.dll'){'F5BDB1FF4CAD596781E91D4FDAEED421E8B22C199EF97852BD96481D778E3B7B'}else{(Get-FileHash $file.FullName).Hash}
    if((Get-FileHash (Join-Path $baseline $file.Name)).Hash -cne $expected){throw 'Paired scope experiment differs beyond script DLL'}
}
if((Get-FileHash (Join-Path $candidate 'Lea.Script.dll')).Hash -cne '62B5656A41720E124A915E36E437C95337E06259985EC10923CA9E987AE7E17E'){throw 'Rejected scope DLL differs'}
$buildPath='I:/LeaBuildStorage/bootstrap-function-build-20261102-v1/result.json'
if((Get-FileHash $buildPath).Hash -cne '2597084C56DE45AA9DB11132091E02EE5F3C1D84890D11EE779F553CA2DD5D93'){throw 'Scope build receipt differs'}
$build=Get-Content $buildPath -Raw|ConvertFrom-Json
if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.runs.Count -ne 1){throw 'Scope build incomplete'}
foreach($file in $build.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Scope build source changed'}}
foreach($name in @('8.cs','10.cs','2.cs')){
    $old='I:/LeaBuildStorage/global-install-build-20261102-v1/source/Lea.Script/'+$name
    $new='I:/LeaBuildStorage/bootstrap-function-build-20261102-v1/source/Lea.Script/'+$name
    if((Get-FileHash $old).Hash -cne (Get-FileHash $new).Hash){throw 'Host evaluation/checkpoint/policy code must remain unchanged'}
}
$nodePath='I:/LeaBuildStorage/bootstrap-function-source-20261102-v1/node-equivalence-test.json'
$node=Get-Content $nodePath -Raw|ConvertFrom-Json
if(!$node.passed -or $node.checks -ne 268 -or $node.sourceProofSha256 -cne '8BE6CCF6D976ED680FE2D5507D440FAB663B712B30B0EFE8A608F170B47BBA22' -or $node.testSha256 -cne (Get-FileHash (Join-Path $root 'Lea.Tests/BootstrapFunctionProjection.test.cjs')).Hash){throw 'Mock bridge equivalence evidence differs'}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($file in @(Get-ChildItem $folder -File)+@(Get-Item $nodePath)){
    Copy-Item -LiteralPath $file.FullName -Destination $out
    if((Get-FileHash (Join-Path $out $file.Name)).Hash -cne (Get-FileHash $file.FullName).Hash){throw 'Rejected scope evidence readback differs'}
}
@{evidenceVerified=$true;experimentPassed=$false;candidateRejected=$true;records=$rows;nodeMockChecks=268;hostPolicyUnchanged=$true;performanceImprovementProven=$false;timeoutFixed=$false;workerBuilt=$false;browserAdopted=$false;scope='One fixed ABBA experiment: baselinePASS/candidateFAIL/candidatePASS/baselinePASS. Single strict IIFE does not meet the startup gate and is not adopted. Small sample is not a causal regression proof; all failures and original budgets retained.'}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
