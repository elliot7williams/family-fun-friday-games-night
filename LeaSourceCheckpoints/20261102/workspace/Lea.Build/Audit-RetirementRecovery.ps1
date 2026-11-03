$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Read-RetirementTrace.ps1')
$folder=Join-Path (Split-Path $PSScriptRoot) 'continuity-evidence/retirement-recovery-v1'
$out='I:\LeaBuildStorage\retirement-recovery-audit-20261102-v1'
if(Test-Path $out){throw 'Preserve audit'}
$receipt=Join-Path $folder 'result.json'
if((Get-FileHash $receipt).Hash -cne '02F722813D61C94F10B09EB1A7A81833086191DE221BAC4A15C4A95A8A2F5E40'){throw 'Recovery receipt differs'}
$r=Get-Content $receipt -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or !$r.footerVerified -or !$r.recoveryAssertionVerified -or !$r.traceActivationVerified -or !$r.traceEnvironmentRestored -or $r.before.Count -ne 23 -or $r.runs.Count -ne 1){throw 'Recovery boundary incomplete'}
$run=$r.runs[0]
if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne 30000 -or $run.sharedRetirementDrainBudgetMs -ne 5000){throw 'Owned run incomplete'}
foreach($file in $r.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Runtime bytes/ACL differ'}}
$trace=Read-RetirementTrace -Lines @(Get-Content $run.stderr) -Frequency ([Diagnostics.Stopwatch]::Frequency)
if(@($trace.spans.phase|Select-Object -Unique).Count -ne 8){throw 'All retirement phase types required'}
$negative=0
$cases=[Collections.Generic.List[object]]::new()
$cases.Add(@('LEA_RETIREMENT_TRACE retire-job begin 2'))
$cases.Add(@('LEA_RETIREMENT_TRACE retire-job end 2'))
$cases.Add(@('LEA_RETIREMENT_TRACE retire-job begin 2','LEA_RETIREMENT_TRACE retire-job end 1'))
$cases.Add(@('LEA_RETIREMENT_TRACE unknown begin 1','LEA_RETIREMENT_TRACE unknown end 2'))
$cases.Add(@('LEA_RETIREMENT_TRACE invalid'))
foreach($case in $cases){$rejected=$false;try{$null=Read-RetirementTrace -Lines $case -Frequency 1000}catch{$rejected=$true};if(!$rejected){throw 'Invalid trace accepted'};$negative++}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($file in Get-ChildItem $folder -File){Copy-Item -LiteralPath $file.FullName -Destination $out;if((Get-FileHash (Join-Path $out $file.Name)).Hash -cne (Get-FileHash $file.FullName).Hash){throw 'Evidence copy differs'}}
@{passed=$true;diagnosticOnly=$true;negativeControls=$negative;trace=$trace;timeoutCauseProven=$false;fullPrefixExecuted=$false;scope='One diagnostic isolated Contains recovery pass, not reliability or release qualification; retirement spans nested, do not sum'}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
