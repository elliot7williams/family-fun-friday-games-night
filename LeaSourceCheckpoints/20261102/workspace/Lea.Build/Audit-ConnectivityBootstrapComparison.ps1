$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$folder=Join-Path $root 'continuity-evidence/connectivity-bootstrap-comparison-v1'
$out='I:\LeaBuildStorage\connectivity-bootstrap-comparison-audit-20261102-v1'
if(Test-Path $out){throw 'Preserve comparison evidence audit'}
$receipt=Join-Path $folder 'result.json'
if((Get-FileHash $receipt).Hash -cne '944DD563AF9ECC490139A06DC8E672E38CB884E6A8B47FD276CF69673EDAA9FB'){throw 'Comparison receipt differs'}
$r=Get-Content $receipt -Raw|ConvertFrom-Json
if($r.passed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.runs.Count -ne 5){throw 'Expected failed four-attempt comparison boundary differs'}
foreach($file in $r.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Comparative runtime bytes/ACL changed'}}
$order=@('baseline','candidate','candidate','baseline');$rows=[Collections.Generic.List[object]]::new()
for($i=0;$i -lt 4;$i++){
    $run=$r.runs[$i+1];$variant=$order[$i]
    if($run.name -cne ('bootstrap-'+($i+1)+'-'+$variant) -or !$run.exitObserved -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.parentBudgetMs -ne 30000 -or $run.sharedRetirementDrainBudgetMs -ne 5000){throw 'Owned comparative record incomplete'}
    $line=@(Get-Content $run.stdout|Where-Object {$_.StartsWith('LEA_BOOTSTRAP_COMPARISON ')})
    if($line.Count -ne 1){throw 'Unique raw comparison report required'}
    $report=$line[0].Substring('LEA_BOOTSTRAP_COMPARISON '.Length)|ConvertFrom-Json
    if($i -lt 3 -and (!$run.passed -or !$report.ok -or $run.exitCode -ne 0)){throw 'Expected initial success records differ'}
    if($i -eq 3 -and ($run.passed -or $report.ok -or $run.exitCode -ne 1 -or $report.status -cne 'limit' -or $report.message -cnotlike '*phase=bootstrap*TimeoutException*stage=document*')){throw 'Expected baseline bootstrap failure differs'}
    $rows.Add(@{iteration=($i+1);variant=$variant;report=$report;exitCode=$run.exitCode;killRequested=$run.killRequested;retirementMs=$run.retirementAndDrainElapsedMs})
}
$baseline=Join-Path $folder 'baseline';$candidate=Join-Path $folder 'runtime'
foreach($file in Get-ChildItem $candidate -File){
    $expected=if($file.Name -ceq 'Lea.Script.dll'){'D2685E2F9AC15A72373CEE7B2B5EA69A7DD46605E1C058CB32406A085D7F6518'}else{(Get-FileHash $file.FullName).Hash}
    if((Get-FileHash (Join-Path $baseline $file.Name)).Hash -cne $expected){throw 'Comparative closure differs beyond script DLL'}
}
if((Get-FileHash (Join-Path $candidate 'Lea.Script.dll')).Hash -cne 'FBD50D216D724A4BFCA0488C53CB1B648BC659AAE19F800AE7E129B207EBBD4C'){throw 'Candidate script differs'}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($file in Get-ChildItem $folder -File){Copy-Item -LiteralPath $file.FullName -Destination $out;if((Get-FileHash (Join-Path $out $file.Name)).Hash -cne (Get-FileHash $file.FullName).Hash){throw 'Comparison evidence readback differs'}}
@{evidenceVerified=$true;experimentPassed=$false;records=$rows;unchangedBaselineFailureObserved=$true;featureRegressionExcluded=$false;timeoutFixed=$false;scope='Four preplanned ABBA raw results inspected; wrapper stopped at last cleanup-request guard and remains failed. Original limits and exact paired file identities retained. Small comparison, not causal or reliability proof.'}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
