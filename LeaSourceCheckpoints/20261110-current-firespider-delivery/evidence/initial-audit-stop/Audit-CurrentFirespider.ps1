param([Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$SmokeSha256,[switch]$Workflows)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$buildRoot='I:\LeaBuildStorage\current-firespider-build-20261109-v1'
$runtimeRoot='I:\LeaBuildStorage\current-firespider-runtime-20261110-v1'
$sourceRoot='I:\LeaBuildStorage\current-firespider-smoke-source-20261110-v1'
$smoke=Join-Path $root 'continuity-evidence/current-source-host-smokes-v1'
$out='I:\LeaBuildStorage\current-firespider-audit-20261110-v1'
if($Workflows){$sourceRoot='I:\LeaBuildStorage\current-firespider-workflow-source-20261110-v1';$smoke=Join-Path $root 'continuity-evidence/current-source-host-workflows-v1';$out='I:\LeaBuildStorage\current-firespider-workflow-audit-20261110-v1'}
if(Test-Path -LiteralPath $out){throw 'Preserve current host audit'}
function Read-Proof([string]$Path,[string]$Hash){if((Get-FileHash -LiteralPath $Path).Hash -cne $Hash){throw ('Proof differs: '+$Path)};return Get-Content -LiteralPath $Path -Raw|ConvertFrom-Json}
$b=Read-Proof (Join-Path $buildRoot 'result.json') 'CD72727F31282AABB42DABBEE2F55BCA69C63A9352A3528C8EB5657A03A2B408'
$m=Read-Proof (Join-Path $runtimeRoot 'runtime-manifest.json') '2836AC6BA77C2DB096C5365EED4D7B6E48D56A82A0A4DFDBEBFD2CCE3DD6E1AB'
$s=Read-Proof (Join-Path $sourceRoot 'source.json') $(if($Workflows){'3A3644B1376964A0850695E904F9C9ADE978812ACE9132DA2AA02B07FCAA4B34'}else{'94D96DA622E3776B5F442573FEEF7663331DB41D95CB63F55999A18C67232EF7'})
$r=Read-Proof (Join-Path $smoke 'result.json') $SmokeSha256
if(!$b.passed -or !$b.inputsUnchanged -or $b.ownerAttention -or $b.products.Count -ne 22 -or $b.runs.Count -ne 1 -or !$b.hostSourceUnchanged -or $b.liveProjectChanged -or $b.installedBrowserChanged -or $b.browserExecuted -or $b.hostSha256 -cne '9513A0BD7C620589F4AB332CE19F155686C01104B1E8D99A5DECAAF1814F2586'){throw 'Build scope differs'}
if(!$m.verified -or !$m.currentSourceSdk -or $m.files.Count -ne 400 -or $m.buildOutputsBound -ne 22 -or $m.hostSha256 -cne $b.hostSha256 -or !$s.verified -or !$s.inverseVerified -or $s.assertionsChanged -or $s.manifestSha256 -cne (Get-FileHash -LiteralPath (Join-Path $runtimeRoot 'runtime-manifest.json')).Hash){throw 'Runtime/projection scope differs'}
$caseCount=if($Workflows){15}else{3}
if(!$r.passed -or !$r.completed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.byteChanges.Count -or $r.aclChanges.Count -or $r.before.Count -ne 400 -or $r.runs.Count -ne $caseCount -or !$r.decoderPolicyAccepted){throw 'Smoke campaign scope differs'}
if($Workflows -and (!$s.workflows -or $s.requiredCases -ne 15)){throw 'Original workflow scope differs'}
foreach($state in @($b,$m,$r)){foreach($file in $state.inputs){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'Input drift'}}}
foreach($file in $b.products){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'Build product drift'}}
foreach($file in $m.files){if((Get-FileHash -LiteralPath (Join-Path $runtimeRoot ('runtime/'+$file.path))).Hash -cne $file.sha256){throw 'Composed runtime drift'}}
foreach($file in $r.before){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256 -or (Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Smoke runtime byte/ACL drift'}}
$plan=Get-Content -LiteralPath (Join-Path $buildRoot 'candidate-manifest.json') -Raw|ConvertFrom-Json
if(!$plan.verified -or !$plan.currentSourceSdk -or !$plan.hostSourceUnchanged -or $plan.files.Count -ne 105 -or @($plan.files|Where-Object kind -CEQ 'preserved-host-source').Count -ne 87){throw 'Source plan differs'}
foreach($file in $plan.files){if((Get-FileHash -LiteralPath (Join-Path $buildRoot ('source/'+$file.path))).Hash -cne $file.sha256){throw 'Host staged source drift'}}
$original=[IO.File]::ReadAllText((Join-Path $PSScriptRoot $(if($Workflows){'Test-PrivateBindingHostWorkflows-v1.generated.ps1'}else{'Test-PrivateBindingHostSmokes-v1.generated.ps1'})))
$projected=[IO.File]::ReadAllText($s.source)
if((Get-FileHash -LiteralPath $s.source).Hash -cne $s.sha256){throw 'Smoke source drift'}
$helperPath="'"+$PSScriptRoot.Replace("'","''")+"'";$inverse=$projected.Replace($helperPath,'$PSScriptRoot')
for($i=$s.replacements.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($s.replacements[$i][1],$s.replacements[$i][0])}
if($inverse -cne $original){throw 'Original smoke assertions changed'}
$runs=@($b.runs)+@($r.runs)
$budgets=@{'compile-current-firespider'=120000;'settings-path'=30000}
if($Workflows){foreach($name in @('clean','lea','navigation','second','split','edit','style-start','script')){$budgets.Add('host-'+$name,90000)};foreach($name in @('clean','restored','locked','oversized','lazy-background','lazy-all')){$budgets.Add('exe-'+$name,100000)}}else{$budgets.Add('navigation',90000);$budgets.Add('exe-clean',100000)}
$seen=[Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
for($i=0;$i -lt $runs.Count;$i++){
    $run=$runs[$i];if(!$budgets.ContainsKey($run.name) -or !$seen.Add($run.name)){throw 'Unexpected/duplicate host case'};$budget=$budgets[$run.name]
    if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.originalHandleCaptured -or !$run.ownedJob -or !$run.childRetirementIndependentlyAudited -or !$run.exitObserved -or !$run.outputDrained -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.captureOverflow -or $run.error){throw 'Owned host run/deadline differs'}
}
if(!$Workflows -and (($r.runs.name -join ',') -cne 'settings-path,navigation,exe-clean' -or @(Get-Content -LiteralPath $r.runs[1].stdout|Where-Object {$_.StartsWith('PASS Lea boot: navigation;')}).Count -ne 1)){throw 'Original smoke cases/footer differ'}
if($Workflows){
    foreach($run in $r.runs|Where-Object {$_.name.StartsWith('host-')}){if(!$run.exactFooterVerified -or @(Get-Content -LiteralPath $run.stdout|Where-Object {$_.StartsWith('PASS Lea boot: '+$run.name.Substring(5)+';')}).Count -ne 1){throw 'Original host workflow footer differs'}}
    foreach($name in @('clean','restored','locked','oversized','lazy-background','lazy-all')){$case=Get-Content -LiteralPath (Join-Path $smoke ($name+'.json')) -Raw|ConvertFrom-Json;if(!$case.Passed -or !$case.Responsive -or !$case.Lea -or $case.LegacyEngine -or $case.ForcedCleanup -or $case.CleanupErrors.Count -or $case.Scenario -cne $name){throw 'Actual EXE workflow readiness/cleanup differs'}}
}
$clean=Get-Content -LiteralPath (Join-Path $smoke 'clean.json') -Raw|ConvertFrom-Json
if(!$clean.Passed -or !$clean.Responsive -or !$clean.Lea -or $clean.LegacyEngine -or $clean.ForcedCleanup -or $clean.CleanupErrors.Count -or $clean.Tabs -ne 1 -or $clean.Scenario -cne 'clean'){throw 'Actual EXE readiness/cleanup differs'}
if($clean.SourceConfigSHA256 -cne (Get-FileHash -LiteralPath (Join-Path $r.runtime 'Firespider Internet 19.exe.config')).Hash){throw 'Actual EXE config binding differs'}
New-Item -ItemType Directory -Path $out|Out-Null
$copies=@(foreach($spec in @(@{folder=$buildRoot;prefix='build'},@{folder=$runtimeRoot;prefix='runtime'},@{folder=$sourceRoot;prefix='source'},@{folder=$smoke;prefix='smoke'})){foreach($file in Get-ChildItem -LiteralPath $spec.folder -File|Where-Object Extension -in @('.json','.txt','.ps1')){$to=Join-Path $out ($spec.prefix+'-'+$file.Name);Copy-Item -LiteralPath $file.FullName -Destination $to;$hash=(Get-FileHash -LiteralPath $file.FullName).Hash;if((Get-FileHash -LiteralPath $to).Hash -cne $hash){throw 'Raw host evidence copy differs'};@{path=$to;sha256=$hash}}})
@{verified=$true;hostSha256=$b.hostSha256;smokeSha256=$SmokeSha256;sourceMembers=87;buildOutputs=22;runtimeMembers=400;ownedRuns=($caseCount+1);smokeCases=$caseCount;actualExeResponsive=$true;actualExeLea=$true;legacyEngineLoaded=$false;forcedCleanup=$false;runtimeBytesAndAclStable=$true;assertionsChanged=$false;liveProjectChanged=$false;installedBrowserChanged=$false;hostAdopted=$false;all15WorkflowsQualified=[bool]$Workflows;printQualified=$false;startupReliabilityFixed=$false;fullReleaseQualified=$false;copies=$copies;scriptSha256=(Get-FileHash -LiteralPath $PSCommandPath).Hash;scope=$(if($Workflows){'Current source-built SDK/Firespider 19 original15workflows, not print/installed-profile/all-sites/reliability/release qualification.'}else{'Current source-built SDK in a fresh preserved-source Firespider 19 build; settings/navigation/actual clean EXE only, not installed-profile/all-sites/reliability/release qualification.'})}|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'audit.json')|Format-List Hash
