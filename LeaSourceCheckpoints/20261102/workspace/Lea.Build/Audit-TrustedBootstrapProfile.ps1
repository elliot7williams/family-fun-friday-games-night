$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$folder=Join-Path $root 'continuity-evidence/trusted-bootstrap-profile-v2'
$out='I:\LeaBuildStorage\trusted-bootstrap-profile-audit-20261102-v2'
if(Test-Path $out){throw 'Preserve bootstrap profile audit'}
$receipt=Join-Path $folder 'result.json'
if((Get-FileHash $receipt).Hash -cne '47FB59FC847310FB40562C2E4A758C3F80D996E11AA44E71259AD372F1FCAC35'){throw 'Profile receipt differs'}
$r=Get-Content $receipt -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.diagnosticOnly -or !$r.measurementCaptured -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.before.Count -ne 10 -or $r.runs.Count -ne 2){throw 'Successful diagnostic boundary incomplete'}
foreach($run in $r.runs){if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.sharedRetirementDrainBudgetMs -ne 5000){throw 'Owned profile run incomplete'}}
if($r.runs[0].parentBudgetMs -ne 120000 -or $r.runs[1].parentBudgetMs -ne 30000){throw 'Original profile test budgets differ'}
foreach($file in $r.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Profile input changed'}}
foreach($file in $r.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Profile runtime bytes/ACL changed'}}
. (Join-Path $PSScriptRoot 'Read-TrustedBootstrapProfile.ps1')
$lines=@(Get-Content $r.runs[1].stderr|Where-Object {$_.StartsWith('LEA_TRUSTED_BOOT ')})
$profile=Read-TrustedBootstrapProfile $lines
$bad=[Collections.Generic.List[object]]::new()
$bad.Add(@($lines|Select-Object -SkipLast 1));$bad.Add(@($lines)+@($lines[0]))
$copy=[string[]]$lines.Clone();$copy[1]=[regex]::Replace($copy[1],'wallFrequency=[0-9]+','wallFrequency=999');$bad.Add($copy)
$copy=[string[]]$lines.Clone();$copy[1]=[regex]::Replace($copy[1],' wall=[0-9]+',' wall=0');$bad.Add($copy)
$copy=[string[]]$lines.Clone();$copy[-1]=$copy[-1].Replace(' end wall=',' unexpected wall=');$bad.Add($copy)
$copy=[string[]]$lines.Clone();$copy[-1]=[regex]::Replace($copy[-1],'gc0=[0-9]+','gc0=-1');$bad.Add($copy)
$negative=0
foreach($case in $bad){$rejected=$false;try{$null=Read-TrustedBootstrapProfile $case}catch{$rejected=$true};if(!$rejected){throw 'Invalid bootstrap trace accepted'};$negative++}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($file in Get-ChildItem $folder -File){Copy-Item -LiteralPath $file.FullName -Destination $out;if((Get-FileHash (Join-Path $out $file.Name)).Hash -cne (Get-FileHash $file.FullName).Hash){throw 'Profile evidence readback differs'}}
@{evidenceVerified=$true;diagnosticOnly=$true;profile=$profile;negativeControls=$negative;timeoutFixed=$false;scope='Corrected buffered cold profile only; invalid v1 excluded. One observation, not a root-cause, performance improvement, feature or release qualification.'}|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
