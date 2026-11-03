$ErrorActionPreference='Stop'
$folder=Join-Path (Split-Path $PSScriptRoot) 'continuity-evidence/connectivity-broader-dom-v1'
$out='I:\LeaBuildStorage\connectivity-broader-dom-failure-20261102-v1'
if(Test-Path $out){throw 'Preserve failure audit'}
$receipt=Join-Path $folder 'result.json'
if((Get-FileHash $receipt).Hash -cne '9CE53E15E1DACF8DF67402CF50F992CB55F02777732AE423ECC7B75C0B89AB0A'){throw 'Broader failure receipt differs'}
$r=Get-Content $receipt -Raw|ConvertFrom-Json
if($r.passed -or $r.completed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.before.Count -ne 19 -or $r.runs.Count -ne 2 -or $r.cases.Count -ne 1 -or $r.cases[0].name -cne 'Selectors'){throw 'Expected failed campaign boundary differs'}
$run=$r.runs[1]
if($run.exitCode -ne 1 -or !$run.exitObserved -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.unsafeToContinue -or $run.retainedForOwner -or $run.timedOut){throw 'Terminal failure ownership incomplete'}
$stderr=[IO.File]::ReadAllText($run.stderr)
if(!$stderr.Contains('phase=bootstrap; reason=TimeoutException; stage=document')){throw 'Expected bootstrap failure absent'}
foreach($file in $r.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Failed runtime bytes/ACL changed'}}
foreach($file in $r.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Failed campaign input changed'}}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($file in Get-ChildItem $folder -File){Copy-Item -LiteralPath $file.FullName -Destination $out;if((Get-FileHash (Join-Path $out $file.Name)).Hash -cne (Get-FileHash $file.FullName).Hash){throw 'Failure evidence copy differs'}}
@{evidenceVerified=$true;campaignPassed=$false;failedSuite='Selectors';failurePhase='trusted bootstrap';pendingSuites=@('Tree','Attributes','ClassLists');runtimeFiles=19;killRequested=$run.killRequested;traceRestoredMarkerObserved=$stderr.Contains('LEA_ALL_STARTUP_TRACE restored=true');traceRestorationGatePassed=$r.cases[0].traceEnvironmentRestored;scope='Failure preserved unchanged, no retries/budget changes. Parent exit/Job0/drain verified; trace gate remains false despite marker. No feature regression or root cause proven.'}|ConvertTo-Json|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
