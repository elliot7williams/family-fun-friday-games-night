param([switch]$RelativePair)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$directory=Join-Path $root 'continuity-evidence/temporal-startup-v1/initial-font-script-owned-run-v1'
$path=Join-Path $directory 'result.json'
$pin='2990322C7945B11249BFE198F8296ECFF3345CF35BBD6CB48E888BF6AC36F27B'
if($RelativePair){$directory=Join-Path $root 'continuity-evidence/temporal-startup-v1/relative-font-script-owned-run-v1';$path=Join-Path $directory 'result.json';$pin='7F1448D51CF16421602D7D02DDC58F066BDAF5DB9E22A8E8B3728C525B5BA205'}
if((Get-FileHash $path).Hash -cne $pin){throw 'Run receipt differs'}
$out=Join-Path $directory 'outcome-audit.json'
if(Test-Path $out){throw 'Preserve outcome audit'}
$r=Get-Content $path -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.completed -or !$r.behaviorPassed -or !$r.footerVerified -or !$r.traceActivationVerified -or !$r.traceEnvironmentRestored -or !$r.inputsUnchanged -or $r.changedFiles.Count -or $r.ownerAttention -or $r.error -or $r.runs.Count -ne 1 -or $r.limitsChanged -or $r.hostAdopted -or $r.currentFullNativeQualified -or !$r.legacyUiAUnresolved){throw 'Suite qualification boundary differs'}
$run=$r.runs[0]
if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.exitObserved -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or $run.elapsedMs -gt 300000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Execution or retirement gate failed'}
$lines=@(Get-Content $run.stdout)
$footer='PASS Lea.Script: 3412 checks; JavaScript, isolated worker, and typed DOM.'
if($lines[-1] -cne $footer -or @($lines|Where-Object {$_ -ceq $footer}).Count -ne 1 -or @($lines|Where-Object {$_.StartsWith('PASS ')}).Count -ne 3413){throw 'Terminal assertion footer/count differs'}
$phases=@(Get-Content $run.stderr|Where-Object {$_ -match '^LEA_SUITE_PHASE '})
if($phases.Count -ne 178){throw 'Phase coverage differs'}
for($i=0;$i -lt 89;$i++){
    if($phases[2*$i] -notmatch ('^LEA_SUITE_PHASE \d+ '+$i+' begin$') -or $phases[2*$i+1] -notmatch ('^LEA_SUITE_PHASE \d+ '+$i+' end$')){throw 'Phase sequence differs'}
}
foreach($inputFile in $r.inputFiles){if((Get-FileHash -LiteralPath $inputFile.path).Hash -cne $inputFile.sha256){throw 'Input differs'}}
if($r.before.Count -ne 65){throw 'Runtime coverage differs'}
foreach($file in $r.before){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256 -or (Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Runtime byte/ACL delta'}}
$audit=@{verified=$true;checks=3412;completedPhases=89;runReceiptSha256=$pin;elapsedMs=$run.elapsedMs;cleanupMs=$run.retirementAndDrainElapsedMs;runtimeFilesVerified=65;limitsChanged=$false;startupCauseFixed=$false;hostAdopted=$false;releaseQualified=$false;legacyUiAUnresolved=$true;runnerSha256=(Get-FileHash $PSCommandPath).Hash}
$audit|ConvertTo-Json -Depth 4|Set-Content -LiteralPath $out -Encoding utf8
$audit|ConvertTo-Json
