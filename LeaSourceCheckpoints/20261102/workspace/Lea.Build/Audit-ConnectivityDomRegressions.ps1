$ErrorActionPreference='Stop'
$folder=Join-Path (Split-Path $PSScriptRoot) 'continuity-evidence/connectivity-dom-regressions-v1'
$out='I:\LeaBuildStorage\connectivity-dom-regression-audit-20261102-v1'
if(Test-Path $out){throw 'Preserve DOM regression audit'}
$receipt=Join-Path $folder 'result.json'
if((Get-FileHash $receipt).Hash -cne '04080131811DD89DC7A3DAAFFC6931DEB4BCD2A8D6A73B0E9EC232D309708DFB'){throw 'DOM regression receipt differs'}
$r=Get-Content $receipt -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.completed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.before.Count -ne 25 -or $r.runs.Count -ne 4 -or $r.cases.Count -ne 4){throw 'DOM regression evidence incomplete'}
$names=@('contains-isolated','subtree-isolated','contains-native','subtree-native');$counts=@(22,21,10,4)
for($i=0;$i -lt 4;$i++){
    $run=$r.runs[$i];$case=$r.cases[$i]
    if($run.name -cne $names[$i] -or $case.name -cne $names[$i] -or !$case.passed -or $case.checks -ne $counts[$i] -or !$case.traceActivationVerified -or !$case.traceEnvironmentRestored){throw 'Original case footer/trace differs'}
    if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.originalHandleCaptured -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.retainedForOwner -or $run.parentBudgetMs -ne 30000 -or $run.sharedRetirementDrainBudgetMs -ne 5000){throw 'DOM owned process boundary differs'}
}
foreach($file in $r.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'DOM regression input changed'}}
foreach($file in $r.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'DOM runtime bytes/ACL differ'}}
foreach($name in @('contains-desktop.png','contains-narrow.png')){if(!(Test-Path (Join-Path $folder $name))){throw 'Native render evidence missing'}}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($file in Get-ChildItem $folder -File){Copy-Item -LiteralPath $file.FullName -Destination $out;if((Get-FileHash (Join-Path $out $file.Name)).Hash -cne (Get-FileHash $file.FullName).Hash){throw 'DOM evidence readback differs'}}
@{evidenceVerified=$true;focusedRegressionsPassed=$true;checks=57;cases=4;stagedFiles=25;nativeCaptures=2;scope='Unchanged original containment and subtree isolated/native cases only. No full DOM/trusted bootstrap/full suite/browser adoption or release proof.'}|ConvertTo-Json|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
