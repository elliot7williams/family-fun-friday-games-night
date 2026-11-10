param([Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$PreparationSha256,[Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$RunSha256)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$prep='I:\LeaBuildStorage\sdk-source-full-preparation-20261109-v1'
$candidate=Join-Path $root 'continuity-evidence/sdk-source-full-suite-v1'
$runRoot=Join-Path $root 'continuity-evidence/sdk-source-full-owned-v1'
$out='I:\LeaBuildStorage\sdk-source-full-outcome-20261109-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve full-suite outcome'}
function Read-Proof([string]$Path,[string]$Hash){if((Get-FileHash -LiteralPath $Path).Hash -cne $Hash){throw ('Proof differs: '+$Path)};return Get-Content -LiteralPath $Path -Raw|ConvertFrom-Json}
$p=Read-Proof (Join-Path $prep 'result.json') $PreparationSha256
$r=Read-Proof (Join-Path $runRoot 'result.json') $RunSha256
if(!$p.passed -or !$p.inputsUnchanged -or $p.ownerAttention -or $p.error -or $p.runs.Count -ne 2 -or $p.compiledSources -ne 60 -or $p.references -ne 193 -or $p.members.Count -ne 305 -or !$p.dispatcherInverseVerified -or $p.dispatcherNegativeControls -ne 3 -or $p.assertionsChanged -or $p.limitsChanged -or $p.engineExecuted){throw 'Preparation scope differs'}
if(!$r.currentSdkSourceCandidate -or !$r.completed -or $r.ownerAttention -or $r.runs.Count -ne 1 -or $r.before.Count -ne 65 -or $r.requiredChecks -ne 3412 -or $r.requiredPhases -ne 89 -or $r.limitsChanged -or $r.hostAdopted){throw 'Full run scope differs'}
foreach($state in @($p,$r)){foreach($input in $state.inputs){if((Get-FileHash -LiteralPath $input.path).Hash -cne $input.sha256){throw ('Input drift: '+$input.path)}}}
$runs=@($p.runs)+@($r.runs)
for($i=0;$i -lt $runs.Count;$i++){
    $run=$runs[$i];$budget=if($i -lt 2){120000}else{300000}
    if(!$run.originalHandleCaptured -or !$run.ownedJob -or !$run.childRetirementIndependentlyAudited -or !$run.exitObserved -or !$run.outputDrained -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.captureOverflow -or $run.error){throw 'Ownership/deadline scope differs'}
    if($i -lt 2 -and (!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested)){throw 'Compilation did not pass'}
}
$original=Read-Proof 'I:\LeaBuildStorage\private-binding-full-assembly-20261103-v5-assets\assembly.json' 'F647679D939FD32355585B61561761BAFAB7B6239D83601A0FFE23B0BCF21DEA'
$changes=@('runtime/Lea.Engine.dll','runtime/Lea.Win.dll','runtime/WorkerEntryProbe/manifest.json','runtime/WorkerEntryProbe/WorkerEntryNativeHost.exe','compiled-source/source/Lea.Script.Tests/ModernComponents.cs')
$difference=[Collections.Generic.List[string]]::new()
foreach($file in $p.members){
    $old=@($original.files|Where-Object path -CEQ $file.path)
    if($old.Count -ne 1){throw 'Original source/member scope differs'}
    if($file.sha256 -cne $old[0].sha256){if($file.path -cnotin $changes){throw 'Unapproved test/product change'};$difference.Add($file.path)}
    if((Get-FileHash -LiteralPath (Join-Path $prep ('assembly/'+$file.path))).Hash -cne $file.sha256){throw 'Prepared member drift'}
}
if($difference.Count -ne 5){throw 'Expected only core/view and three entry-binding identity changes'}
$dispatcher=Join-Path $prep 'assembly/compiled-source/source/Lea.Script.Tests/ModernComponents.cs'
$oldDispatcher='I:\LeaBuildStorage\private-binding-full-assembly-20261103-v5-assets\compiled-source\source\Lea.Script.Tests\ModernComponents.cs'
$oldManifest='1729514FB578A585FF3812EFA20BCCB3EF6FE8BEBAB3782B62A9B0D217B653FB'
$newManifest=(Get-FileHash -LiteralPath (Join-Path $prep 'assembly/runtime/WorkerEntryProbe/manifest.json')).Hash
if([IO.File]::ReadAllText($dispatcher).Replace($newManifest,$oldManifest) -cne [IO.File]::ReadAllText($oldDispatcher)){throw 'Test source assertions changed'}
$s=Read-Proof (Join-Path $candidate 'staging.json') $p.stagingSha256
$a=Read-Proof (Join-Path $candidate 'staging-audit.json') $p.auditSha256
if(!$s.verified -or !$a.verified -or !$s.currentSdkSourceCandidate -or !$a.currentSdkSourceCandidate -or $s.files.Count -ne 247 -or !$a.embeddedFixtureReadWithoutLoadingHost -or $a.embeddedFiles -ne 7 -or $a.stagingReceiptSha256 -cne $p.stagingSha256 -or $s.assertionsChanged -or $s.limitsChanged){throw 'Staging scope differs'}
foreach($file in $s.files){if((Get-FileHash -LiteralPath (Join-Path $candidate $file.path)).Hash -cne $file.sha256){throw 'Staged byte drift'}}
$deltas=@(foreach($file in $r.before){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256 -or (Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){$file.path}})
if((($deltas|Sort-Object) -join "`n") -cne (($r.changedFiles|Sort-Object) -join "`n")){throw 'Recorded runtime deltas differ'}
$stderr=@(Get-Content -LiteralPath $r.runs[0].stderr);$stdout=@(Get-Content -LiteralPath $r.runs[0].stdout)
$phaseLines=@($stderr|Where-Object {$_ -cmatch '^LEA_SUITE_PHASE [0-9]+ [0-9]+ (begin|end)$'})
$ends=@(foreach($line in $phaseLines){if($line -cmatch '^LEA_SUITE_PHASE [0-9]+ ([0-9]+) end$'){[int]$Matches[1]}})
for($i=0;$i -lt $ends.Count;$i++){if($ends[$i] -ne $i){throw 'Completed phase prefix differs'}}
$footer=@($stdout|Where-Object {$_ -cmatch '^PASS Lea\.Script: 3412 checks; JavaScript, isolated worker, and typed DOM\.$'})
if($r.passed){
    if(!$r.behaviorPassed -or !$r.footerVerified -or !$r.phasesVerified -or !$r.traceActivationVerified -or !$r.traceEnvironmentRestored -or !$r.inputsUnchanged -or $deltas.Count -or $r.error -or $r.validationError -or $footer.Count -ne 1 -or $phaseLines.Count -ne 178 -or $ends.Count -ne 89 -or !$r.runs[0].passed -or $r.runs[0].exitCode -ne 0 -or $r.runs[0].timedOut -or $r.runs[0].killRequested){throw 'Full success scope incomplete'}
    for($i=0;$i -lt 89;$i++){if($phaseLines[2*$i] -cnotmatch ('^LEA_SUITE_PHASE [0-9]+ '+$i+' begin$') -or $phaseLines[2*$i+1] -cnotmatch ('^LEA_SUITE_PHASE [0-9]+ '+$i+' end$')){throw 'Full phase pairs differ'}}
}
New-Item -ItemType Directory -Path $out|Out-Null
$copies=@(foreach($path in @((Join-Path $prep 'result.json'),(Join-Path $candidate 'staging.json'),(Join-Path $candidate 'staging-audit.json'),(Join-Path $runRoot 'result.json'),$r.runs[0].stdout,$r.runs[0].stderr,(Join-Path $runRoot 'full-script-timed.result.json'))){$prefix=if($path.StartsWith($prep,[StringComparison]::OrdinalIgnoreCase)){'preparation-'}elseif($path.StartsWith($candidate,[StringComparison]::OrdinalIgnoreCase)){'staging-'}else{'run-'};$to=Join-Path $out ($prefix+[IO.Path]::GetFileName($path));Copy-Item -LiteralPath $path -Destination $to;$hash=(Get-FileHash -LiteralPath $path).Hash;if((Get-FileHash -LiteralPath $to).Hash -cne $hash){throw 'Outcome copy differs'};@{path=$to;sha256=$hash}})
@{observationVerified=$true;fullSuitePassed=[bool]$r.passed;completedPhases=$ends.Count;phaseLines=$phaseLines;failureLines=@($stderr|Where-Object {$_ -match 'FAIL |Exception:|stage='});runtimeBytesAndAclUnchanged=($deltas.Count -eq 0);runtimeDeltas=$deltas;traceEnvironmentRestored=[bool]$r.traceEnvironmentRestored;quarantineRequired=($deltas.Count -ne 0 -or !$r.traceEnvironmentRestored);preparationSha256=$PreparationSha256;runSha256=$RunSha256;ownedRuns=3;assertionsChanged=$false;limitsChanged=$false;sourceIdentityChanges=$difference;copies=$copies;causeProven=$false;startupReliabilityFixed=$false;hostAdopted=$false;releaseQualified=$false;scriptSha256=(Get-FileHash -LiteralPath $PSCommandPath).Hash;scope='Exact new source-built core/view runtime in unchanged full suite; one observed campaign does not resolve historical startup reliability.'}|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'audit.json')|Format-List Hash
