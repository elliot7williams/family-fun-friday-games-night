$ErrorActionPreference='Stop'
$folder='I:\LeaBuildStorage\current-bootstrap-fold-abba-20261109-v1'
$out='I:\LeaBuildStorage\current-bootstrap-fold-audit-20261109-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve comparison audit'}
$receipt=Join-Path $folder 'result.json'
if((Get-FileHash -LiteralPath $receipt).Hash -cne '01984D30DCC0F2FE1824CA175372C040BA4BE12EC159659902D087A895DB0007'){throw 'Fixed comparison receipt differs'}
$r=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.measurementCaptured -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.runs.Count -ne 6 -or $r.before.Count -ne 20 -or !$r.foldingInverseVerified -or $r.foldingNegativeControls -ne 3 -or $r.historicalMetadataRecovered -or !$r.metadataReplacementExplicit -or $r.limitsChanged -or $r.speedImprovementProven){throw 'Expected bounded folding comparison'}
for($i=0;$i -lt 6;$i++){
    $run=$r.runs[$i];$budget=if($i -lt 2){120000}else{30000}
    if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.exitObserved -or !$run.outputDrained -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Original process ownership/budgets differ'}
}
foreach($input in $r.inputs){if((Get-FileHash -LiteralPath $input.path).Hash -cne $input.sha256){throw 'Comparison input changed'}}
foreach($file in $r.before){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256 -or (Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Compared runtime bytes/ACL changed'}}
$original=[IO.File]::ReadAllText('I:\LeaBuildStorage\current-bootstrap-profile-source-20261109-v1\8.cs')
$candidate=[IO.File]::ReadAllText((Join-Path $folder '8.fold.cs'))
if([regex]::Matches($original,'FoldConstants = false,').Count -ne 1 -or [regex]::Matches($candidate,'FoldConstants = true,').Count -ne 1 -or $candidate.Replace('FoldConstants = true,','FoldConstants = false,') -cne $original){throw 'One-option host inverse differs'}
. (Join-Path $PSScriptRoot 'Read-TrustedBootstrapProfile.ps1')
$order=@('baseline-1','folded-2','folded-3','baseline-4')
if($r.observations.Count -ne 4){throw 'Incomplete fixed ABBA order'}
$samples=@(for($i=0;$i -lt 4;$i++){
    $observation=$r.observations[$i];$run=$r.runs[$i+2]
    if($observation.label -cne $order[$i] -or $run.name -cne $order[$i] -or !$observation.report.ok){throw 'Fixed ABBA labels/outcome differ'}
    $lines=@(Get-Content -LiteralPath $run.stderr|Where-Object {$_.StartsWith('LEA_TRUSTED_BOOT ')})
    $parsed=Read-TrustedBootstrapProfile $lines
    $reports=@(Get-Content -LiteralPath $run.stdout|Where-Object {$_.StartsWith('LEA_BOOTSTRAP_COMPARISON ')})
    if($reports.Count -ne 1){throw 'Expected one raw report'}
    $report=$reports[0].Substring('LEA_BOOTSTRAP_COMPARISON '.Length)|ConvertFrom-Json
    if(!$report.ok -or $report.elapsedMs -ne $observation.report.elapsedMs -or $report.status -cne 'ok'){throw 'Raw report differs'}
    $evaluation=@($parsed.intervals|Where-Object phase -CEQ 'evaluate')[0]
    $preparation=@($parsed.intervals|Where-Object phase -CEQ 'prepare')[0]
    @{label=$order[$i];group=$(if($order[$i].StartsWith('baseline')){'baseline'}else{'folded'});totalMs=$report.elapsedMs;prepareMs=$preparation.wallMs;evaluateMs=$evaluation.wallMs;evaluateProcessCpuMs=$evaluation.processCpuMs;markers=$parsed.markers}
})
$summary=@(foreach($group in 'baseline','folded'){
    $selected=@($samples|Where-Object group -CEQ $group)
    if($selected.Count -ne 2){throw 'Matched group inventory differs'}
    @{group=$group;samples=2;meanTotalMs=($selected|Measure-Object totalMs -Average).Average;meanEvaluateMs=($selected|Measure-Object evaluateMs -Average).Average;meanPrepareMs=($selected|Measure-Object prepareMs -Average).Average}
})
New-Item -ItemType Directory -Path $out|Out-Null
$copies=@(foreach($file in Get-ChildItem -LiteralPath $folder -File){
    $to=Join-Path $out $file.Name;Copy-Item -LiteralPath $file.FullName -Destination $to
    $hash=(Get-FileHash -LiteralPath $file.FullName).Hash
    if((Get-FileHash -LiteralPath $to).Hash -cne $hash){throw 'Audit copy differs'}
    @{path=$to;sha256=$hash}
})
@{evidenceVerified=$true;diagnosticOnly=$true;oneOptionInverseVerified=$true;samples=$samples;summary=$summary;copies=$copies;
    adoptionRecommended=$false;speedImprovementProven=$false;timeoutFixed=$false;productionRuntimeChanged=$false;
    scope='One fixed ABBA campaign, two fresh processes per arm, instrumented and explicit diagnostic metadata. No general speed/reliability or full-suite qualification.';
    runnerSha256=(Get-FileHash -LiteralPath $PSCommandPath).Hash}|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'audit.json')|Format-List Hash
