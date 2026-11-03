$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$folder=Join-Path $root 'continuity-evidence/global-install-comparison-v1'
$out='I:\LeaBuildStorage\global-install-comparison-audit-20261102-v1'
if(Test-Path $out){throw 'Preserve global installation comparison audit'}
$receipt=Join-Path $folder 'result.json'
if((Get-FileHash $receipt).Hash -cne 'EFCA1D194071A89041D8A6B5283A753C32F05C6A161B7E03A1D4C6D1F27693D8'){throw 'Comparison receipt differs'}
$r=Get-Content $receipt -Raw|ConvertFrom-Json
if($r.passed -or !$r.comparisonCompleted -or !$r.candidateOnly -or $r.runtimeQualified -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.runs.Count -ne 5 -or $r.before.Count -ne 20 -or $r.observations.Count -ne 4){throw 'Expected completed failed comparison differs'}
foreach($file in $r.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Comparison input changed'}}
foreach($file in $r.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Comparison bytes/ACL changed'}}
$order=@('baseline','candidate','candidate','baseline');$rows=[Collections.Generic.List[object]]::new()
for($i=0;$i -lt 5;$i++){
    $run=$r.runs[$i]
    $budget=if($i -eq 0){120000}else{30000}
    if(!$run.exitObserved -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.timedOut -or $run.killRequested -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000){throw 'Owned comparison record incomplete'}
    if($i -eq 0){if(!$run.passed -or $run.exitCode -ne 0){throw 'Compiler did not pass'};continue}
    $variant=$order[$i-1]
    if($run.name -cne ('bootstrap-'+$i+'-'+$variant)){throw 'Comparison order differs'}
    $line=@(Get-Content $run.stdout|Where-Object {$_.StartsWith('LEA_BOOTSTRAP_COMPARISON ')})
    if($line.Count -ne 1){throw 'Unique raw report required'}
    $report=$line[0].Substring('LEA_BOOTSTRAP_COMPARISON '.Length)|ConvertFrom-Json
    if($i -lt 4 -and (!$run.passed -or !$report.ok -or $run.exitCode -ne 0)){throw 'Expected initial successful observations differ'}
    if($i -eq 4 -and ($run.passed -or $report.ok -or $run.exitCode -ne 1 -or $report.status -cne 'limit' -or $report.message -cne 'Script budget exceeded. phase=bootstrap; reason=TimeoutException; stage=events')){throw 'Expected final baseline timeout differs'}
    $rows.Add(@{iteration=$i;variant=$variant;report=$report;retirementMs=$run.retirementAndDrainElapsedMs})
}
$candidate=Join-Path $folder 'runtime';$baseline=Join-Path $folder 'baseline'
foreach($file in Get-ChildItem $candidate -File){
    $expected=if($file.Name -ceq 'Lea.Script.dll'){'FBD50D216D724A4BFCA0488C53CB1B648BC659AAE19F800AE7E129B207EBBD4C'}else{(Get-FileHash $file.FullName).Hash}
    if((Get-FileHash (Join-Path $baseline $file.Name)).Hash -cne $expected){throw 'Comparison differs beyond script DLL'}
}
if((Get-FileHash (Join-Path $candidate 'Lea.Script.dll')).Hash -cne 'F5BDB1FF4CAD596781E91D4FDAEED421E8B22C199EF97852BD96481D778E3B7B'){throw 'Candidate script changed'}
$prefixPath='I:\LeaBuildStorage\global-install-source-20261102-v1\node-prefix-test.json'
$prefix=Get-Content $prefixPath -Raw|ConvertFrom-Json
if(!$prefix.passed -or $prefix.checks -ne 276 -or $prefix.testSha256 -cne (Get-FileHash (Join-Path $root 'Lea.Tests/GlobalInstallProjection.test.cjs')).Hash){throw 'Prefix behavior evidence differs'}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($file in @((Get-ChildItem $folder -File)) + @(Get-Item $prefixPath)){
    Copy-Item -LiteralPath $file.FullName -Destination $out
    if((Get-FileHash (Join-Path $out $file.Name)).Hash -cne (Get-FileHash $file.FullName).Hash){throw 'Comparison readback differs'}
}
@{evidenceVerified=$true;experimentPassed=$false;candidateStartupObservationsPassed=2;prefixChecks=276;records=$rows;performanceImprovementProven=$false;timeoutFixed=$false;browserAdopted=$false;scope='Fixed four-process comparison. Candidate passed both common containment scripts; baseline passed once and timed out once. Timing variation, not a speed/reliability or timer/frame/worker/host qualification.'}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
