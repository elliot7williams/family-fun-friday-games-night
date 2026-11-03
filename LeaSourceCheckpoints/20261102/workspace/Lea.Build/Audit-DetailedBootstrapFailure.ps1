$ErrorActionPreference='Stop'
$folder=Join-Path (Split-Path $PSScriptRoot) 'continuity-evidence/trusted-bootstrap-profile-v3'
$out='I:\LeaBuildStorage\trusted-bootstrap-profile-audit-20261102-v3'
if(Test-Path $out){throw 'Preserve detailed failure audit'}
$receipt=Join-Path $folder 'result.json'
if((Get-FileHash $receipt).Hash -cne '278AE2103EA153B378D5DF2D7AD97BC6FE30BB82F6B6C45AB26C02C60CAAB54C'){throw 'Failure receipt differs'}
$r=Get-Content $receipt -Raw|ConvertFrom-Json
if($r.passed -or !$r.diagnosticOnly -or !$r.measurementCaptured -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.before.Count -ne 10 -or $r.runs.Count -ne 2 -or $r.report.ok -or $r.report.status -cne 'limit' -or $r.report.message -cne 'Script budget exceeded. phase=bootstrap; reason=TimeoutException; stage=elements'){throw 'Expected failed diagnostic boundary differs'}
foreach($run in $r.runs){if(!$run.exitObserved -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.timedOut -or $run.unsafeToContinue){throw 'Failure process retirement incomplete'}}
if(!$r.runs[0].passed -or $r.runs[0].parentBudgetMs -ne 120000 -or $r.runs[1].exitCode -ne 1 -or $r.runs[1].parentBudgetMs -ne 30000){throw 'Original budgets/outcomes differ'}
foreach($file in $r.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Failure input changed'}}
foreach($file in $r.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Failure runtime bytes/ACL changed'}}
. (Join-Path $PSScriptRoot 'Read-TrustedBootstrapProfile.ps1')
$lines=@(Get-Content $r.runs[1].stderr)
$profile=Read-TrustedBootstrapProfile $lines -Detailed -Failed
$bad=[Collections.Generic.List[object]]::new()
$bad.Add(@($lines|Select-Object -SkipLast 1));$bad.Add(@($lines)+@($lines[0]))
$copy=[string[]]$lines.Clone();$copy[1]=[regex]::Replace($copy[1],'wallFrequency=[0-9]+','wallFrequency=999');$bad.Add($copy)
$copy=[string[]]$lines.Clone();$copy[1]=[regex]::Replace($copy[1],' wall=[0-9]+',' wall=0');$bad.Add($copy)
$copy=[string[]]$lines.Clone();$copy[-1]=$copy[-1].Replace(' end wall=',' complete wall=');$bad.Add($copy)
$copy=[string[]]$lines.Clone();$copy[8]=$copy[8].Replace('stage-profile-microtask','stage-profile-timers');$bad.Add($copy)
$negative=0
foreach($case in $bad){$rejected=$false;try{$null=Read-TrustedBootstrapProfile $case -Detailed -Failed}catch{$rejected=$true};if(!$rejected){throw 'Invalid partial trace accepted'};$negative++}
$rejected=$false;try{$null=Read-TrustedBootstrapProfile $lines -Detailed}catch{$rejected=$true};if(!$rejected){throw 'Failed trace accepted as success'};$negative++
$old=Get-Content (Join-Path (Split-Path $folder) 'trusted-bootstrap-profile-v2/cold-trusted-profile.stderr.txt')
$oldProfile=Read-TrustedBootstrapProfile $old
if(!$oldProfile.bootstrapSucceeded -or $oldProfile.markers -ne 15){throw 'Existing successful trace parser regressed'}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($file in Get-ChildItem $folder -File){Copy-Item -LiteralPath $file.FullName -Destination $out;if((Get-FileHash (Join-Path $out $file.Name)).Hash -cne (Get-FileHash $file.FullName).Hash){throw 'Failure evidence readback differs'}}
@{evidenceVerified=$true;experimentPassed=$false;diagnosticOnly=$true;profile=$profile;negativeControls=$negative;oldSuccessfulParserVerified=$true;killRequested=$r.runs[1].killRequested;timeoutFixed=$false;scope='One failed diagnostic observation; no product adoption or cause attribution.'}|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
