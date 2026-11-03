param([switch]$Package,[switch]$PrintSdk)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$directory=Join-Path $root 'continuity-evidence/sdk-portable-source-v1'
$path=Join-Path $directory 'result.json'
$pin='57E993FAE281B565D502BBEE77A95C98A71B5A0816C5DC5D66250F3EE9C32E36'
if($Package){$directory=Join-Path $root 'continuity-evidence/sdk-package-source-v1';$path=Join-Path $directory 'result.json';$pin='6ECCE54E3CCF99B4A6CA3841E54392296206391478E5E12AAB4BE32E675C15F3'}
if($PrintSdk){if(!$Package){throw 'Print source audit requires package mode'};$directory=Join-Path $root 'continuity-evidence/sdk-print-package-source-v1';$path=Join-Path $directory 'result.json';$pin='649AD2862712AF78954243DBBE56E80D004D31E09ADD440757A205619D69F362'}
if((Get-FileHash $path).Hash -cne $pin){throw 'Aggregate SDK receipt differs'}
$r=Get-Content $path -Raw|ConvertFrom-Json
$archivePin=if($PrintSdk){'2B035A42A34A375393D5E4F4E2B27119363C81DF25239C3BF87412B4980C3133'}else{'8E16F555B7D94D5ED65A02E5CD57EFBE30764EC8B092F404088A0A4C87F194EA'}
if($Package -and (!$r.packageExtractionVerified -or $r.packageSha256 -cne $archivePin)){throw 'Package extraction identity missing'}
$sources=if($PrintSdk){76}else{53};$excluded=if($PrintSdk){10}else{8}
if($PrintSdk -and !$r.printIncluded){throw 'Print source mode missing'}
if(!$r.passed -or !$r.aggregateSourceBuildPassed -or $r.prebuiltFilesExcluded -ne $excluded -or $r.freshSourceFiles -ne $sources -or $r.sourceEmbeddingChecks -ne 14 -or !$r.workerAclRestored -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.error -or $r.runs.Count -ne 4){throw 'Aggregate SDK gate incomplete'}
$budgets=@(120000,30000,120000,30000)
for($i=0;$i -lt 4;$i++){
    $run=$r.runs[$i]
    if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.exitObserved -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne $budgets[$i] -or $run.elapsedMs -gt $budgets[$i] -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Aggregate SDK run lifecycle differs'}
}
foreach($case in @(@(1,'PASS LEA SDK EMBEDDING 14'),@(3,'PASS SDK VIEW IMAGES 42'))){$lines=@(Get-Content $r.runs[$case[0]].stdout);if($lines[-1] -cne $case[1] -or @($lines|Where-Object {$_ -ceq $case[1]}).Count -ne 1){throw 'Aggregate SDK terminal footer differs'}}
if($r.sourceBuiltProducts.Count -ne $(if($PrintSdk){5}else{4})){throw 'Aggregate SDK product coverage differs'}
foreach($file in $r.sourceBuiltProducts){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Aggregate SDK product changed'}}
foreach($file in $r.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Aggregate SDK input changed'}}
if((Get-Acl $r.workerPath).Sddl -cne $r.workerAclBefore){throw 'Aggregate SDK worker ACL differs'}
$out=Join-Path $directory 'source-audit.json';if(Test-Path $out){throw 'Preserve aggregate SDK audit'}
@{verified=$true;sourceBuildReceiptSha256=$pin;sourceFiles=$sources;prebuiltFilesExcluded=$excluded;products=$r.sourceBuiltProducts;embeddingChecks=14;imageChecks=42;completeSdk=$false;globalSecurityQualified=$false;runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 5|Set-Content $out -Encoding utf8
Get-FileHash $out
