$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$runRoot=Join-Path $root 'continuity-evidence/private-binding-full-owned-v5'
$out='I:\LeaBuildStorage\font-net48-full-failure-20261109-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve failure audit'}
$receipt=Join-Path $runRoot 'result.json'
$pin='F099EC003AEA14718CE72DDB646C58235F33A4290FDD5CF5C92EE87FD619AE85'
if((Get-FileHash -LiteralPath $receipt).Hash -cne $pin){throw 'Failure receipt differs'}
$r=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
if(!$r.completed -or $r.passed -or $r.behaviorPassed -or !$r.inputsUnchanged -or $r.changedFiles.Count -or $r.ownerAttention -or !$r.traceActivationVerified -or !$r.traceEnvironmentRestored -or $r.runs.Count -ne 1){throw 'Expected cleanly retired behavioral failure'}
$run=$r.runs[0]
if($run.passed -or $run.exitCode -ne 1 -or $run.timedOut -or $run.killRequested -or !$run.exitObserved -or !$run.outputDrained -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne 300000 -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Ownership/deadline outcome differs'}
foreach($input in $r.inputs){if((Get-FileHash -LiteralPath $input.path).Hash -cne $input.sha256){throw 'Failure input changed'}}
if($r.before.Count -ne 65){throw 'Runtime inventory differs'}
foreach($file in $r.before){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256 -or (Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Runtime bytes/ACL changed'}}
$stderr=@(Get-Content -LiteralPath $run.stderr)
$phases=@($stderr|Where-Object {$_ -cmatch '^LEA_SUITE_PHASE [0-9]+ [0-9]+ (begin|end)$'})
if($phases.Count -ne 17){throw 'Expected eight complete phases and phase8 begun'}
for($i=0;$i -lt 8;$i++){
    if($phases[2*$i] -cnotmatch ('^LEA_SUITE_PHASE [0-9]+ '+$i+' begin$') -or $phases[2*$i+1] -cnotmatch ('^LEA_SUITE_PHASE [0-9]+ '+$i+' end$')){throw 'Phase sequence differs'}
}
if($phases[16] -cnotmatch '^LEA_SUITE_PHASE [0-9]+ 8 begin$'){throw 'Failure phase differs'}
$failure='System.InvalidOperationException: FAIL computed script: limit; Script budget exceeded. phase=bootstrap; reason=TimeoutException; stage=events'
if(@($stderr|Where-Object {$_ -ceq $failure}).Count -ne 1 -or @($stderr|Where-Object {$_ -ceq 'LEA_ALL_STARTUP_TRACE restored=true'}).Count -ne 1){throw 'Failure identity/restoration marker differs'}
if(@(Get-Content -LiteralPath $run.stdout|Where-Object {$_ -cmatch '^PASS Lea\.Script: [0-9]+ checks; JavaScript, isolated worker, and typed DOM\.$'}).Count){throw 'Unexpected full-suite success footer'}
New-Item -ItemType Directory -Path $out|Out-Null
$copies=@(foreach($path in $receipt,$run.stdout,$run.stderr,(Join-Path $runRoot 'full-script-timed.result.json')){
    $to=Join-Path $out ([IO.Path]::GetFileName($path));Copy-Item -LiteralPath $path -Destination $to
    $hash=(Get-FileHash -LiteralPath $path).Hash
    if((Get-FileHash -LiteralPath $to).Hash -cne $hash){throw 'Evidence readback differs'}
    @{path=$to;sha256=$hash}
})
@{failureCaptureVerified=$true;passed=$false;fullSuitePassed=$false;receiptSha256=$pin;
    failure=$failure;completedPhases=8;startedPhase=8;elapsedMs=$run.elapsedMs;
    retirementMs=$run.retirementAndDrainElapsedMs;workersRetired=$true;
    runtimeBytesAndAclUnchanged=$true;traceEnvironmentRestored=$true;
    causeProven=$false;hostAdopted=$false;copies=$copies;
    runnerSha256=(Get-FileHash -LiteralPath $PSCommandPath).Hash}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'audit.json')|Format-List Hash
