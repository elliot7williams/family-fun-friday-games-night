param([switch]$Adapter)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$directory=Join-Path $root 'continuity-evidence/sdk-script-worker-runtime-v2'
$receipt=Join-Path $directory 'result.json';$pin='E0BA2833A6FF54FF8216455CD630850A205F2E9C34E10D9CBC23C16962B1320E'
if($Adapter){$directory=Join-Path $root 'continuity-evidence/sdk-script-adapter-runtime-v4';$receipt=Join-Path $directory 'result.json';$pin='16ECB3BC687A29068BD397ECD8AC2B39564811C97A57C6EF93F3879BDBF458E9'}
if((Get-FileHash $receipt).Hash -cne $pin){throw 'Worker runtime receipt differs'}
$r=Get-Content $receipt -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.hostBindingConfiguration -or !$r.workerAclRestored -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.error -or $r.runs.Count -ne 2 -or $r.workerAclBefore.Count -ne 6){throw 'Focused worker gate incomplete'}
$budgets=@(120000,30000)
for($i=0;$i -lt 2;$i++){
    $run=$r.runs[$i]
    if(!$run.passed -or $run.exitCode -ne 0 -or !$run.exitObserved -or $run.timedOut -or $run.killRequested -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or $run.parentBudgetMs -ne $budgets[$i] -or $run.elapsedMs -gt $budgets[$i] -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Worker execution lifecycle differs'}
}
$lines=@(Get-Content $r.runs[1].stdout)
$checks=if($Adapter){26}else{23};$footer=if($Adapter){'PASS SDK ADAPTER 26'}else{'PASS SDK WORKER 23'}
if($lines[-1] -cne $footer -or @($lines|Where-Object {$_ -like 'PASS *'}).Count -ne ($checks+1)){throw 'Worker terminal coverage differs'}
if(!$Adapter -and @($lines|Where-Object {$_ -like 'WORKER_START ok;*'}).Count -ne 1){throw 'Worker start proof missing'}
foreach($file in $r.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Worker tested input changed'}}
foreach($file in $r.workerAclBefore){if((Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Worker current ACL differs'}}
foreach($pair in @(@('Lea.Script.dll','906E3CAAE4C160C7FDCF9D7E79B08076B9A12F46A76715AE02CAE9E72983764C'),@('ScriptWorker/Lea.Script.Worker.dll','F8BE188734FB0B747C131E396269E9698A02E257ED2DCF6DCB1DB7DECADFC81C'))){
    $path=Join-Path $directory $pair[0];$rows=@($r.inputs|Where-Object path -ceq $path)
    if($rows.Count -ne 1 -or $rows[0].sha256 -cne $pair[1]){throw 'Exact SDK parent/worker differs'}
}
if($Adapter -and (Get-FileHash (Join-Path $directory 'Lea.Script.Win.dll')).Hash -cne '1C3425A5CC1311AB61DE01FD49202A83CB7EA047B952BCAAF280C6ABC5025F2A'){throw 'SDK adapter identity differs'}
$out=Join-Path $directory 'worker-audit.json';if(Test-Path $out){throw 'Preserve focused runtime audit'}
@{verified=$true;receiptSha256=$pin;checks=$checks;focusedAdapterRuntimePassed=[bool]$Adapter;focusedWorkerRuntimePassed=$true;globalSecurityQualified=$false;fullScriptSuitePassed=$false;trustedParentColdStartupFixed=$false;completeSdk=$false;runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json|Set-Content $out -Encoding utf8
Get-FileHash $out
