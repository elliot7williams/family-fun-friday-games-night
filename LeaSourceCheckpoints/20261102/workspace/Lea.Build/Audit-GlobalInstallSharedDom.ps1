$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$folder='I:/LeaBuildStorage/global-install-shared-dom-20261102-v1'
$out='I:/LeaBuildStorage/global-install-shared-dom-audit-20261102-v1'
if(Test-Path $out){throw 'Preserve shared DOM audit'}
$receipt=Join-Path $folder 'result.json'
if((Get-FileHash $receipt).Hash -cne 'DC438755F25368344B992F131C27DFE892DF74FB72EE2C39EE1AAC4E08C41106'){throw 'Shared DOM receipt differs'}
$r=Get-Content $receipt -Raw|ConvertFrom-Json
if($r.passed -or $r.completed -or !$r.runtimeReused -or $r.engineBinariesChanged -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.before.Count -ne 21 -or $r.cases.Count -ne 4 -or $r.runs.Count -ne 5 -or $r.driverFilesAdded.Count -ne 2 -or $r.error -cnotlike '*Original DOM suite failed: ClassLists*'){throw 'Expected partial DOM failure boundary differs'}
foreach($file in $r.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Shared DOM input changed'}}
foreach($file in $r.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Shared DOM bytes/ACL changed'}}
if(@(Get-ChildItem $r.runtime -Recurse -File).Count -ne 21){throw 'Shared DOM inventory drift'}
$prior=Get-Content (Join-Path $root 'continuity-evidence/global-install-isolated-scheduling-v2/result.json') -Raw|ConvertFrom-Json
if($prior.before.Count -ne 19){throw 'Original shared runtime inventory differs'}
foreach($file in $prior.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Previously qualified runtime member changed'}}
$added=@($r.before|Where-Object {$_.path -cnotin $prior.before.path}|Select-Object -ExpandProperty path)
if($added.Count -ne 2 -or @($added|Where-Object {$_ -cnotin $r.driverFilesAdded}).Count){throw 'Only fresh driver/config additions permitted'}
. (Join-Path $PSScriptRoot 'Read-AllStartupTraceProof.ps1')
$names=@('Selectors','Tree','Attributes','ClassLists');$counts=@(48,48,66,0)
for($i=0;$i -lt 5;$i++){
    $run=$r.runs[$i];$budget=if($i -eq 0){120000}else{30000};$exit=if($i -eq 4){1}else{0}
    if(!$run.exitObserved -or !$run.outputDrained -or !$run.originalHandleCaptured -or !$run.ownedJob -or !$run.childRetirementIndependentlyAudited -or $run.activeAfterRetirement -ne 0 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.timedOut -or $run.killRequested -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.exitCode -ne $exit -or $run.passed -ne ($i -ne 4)){throw 'Owned DOM result differs'}
    if($i -eq 0){continue}
    $case=$r.cases[$i-1];$name=$names[$i-1]
    if($run.name -cne ('original-'+$name) -or $case.name -cne $name -or $case.checks -ne $counts[$i-1] -or $case.passed -ne ($i -ne 4)){throw 'Original DOM assertion outcome differs'}
    $trace=Read-AllStartupTraceProof -Lines @(Get-Content $run.stderr) -StreamsComplete $true -ExitObserved $true -TimedOut $false -KillRequested $false
    if(!$trace.traceActivationVerified -or !$trace.traceEnvironmentRestored -or !$case.traceActivationVerified -or !$case.traceEnvironmentRestored){throw 'DOM startup trace not restored'}
    $footers=@(Get-Content $run.stdout|Where-Object {$_.StartsWith('PASS Lea DOM regression ')})
    if($i -lt 4){if($footers.Count -ne 1 -or $footers[0] -cne ('PASS Lea DOM regression '+$name+': '+$counts[$i-1]+' checks.')){throw 'Successful original DOM footer differs'}}
    elseif($footers.Count -ne 0 -or !(Get-Content $run.stderr -Raw).Contains('Script budget exceeded. phase=bootstrap; reason=TimeoutException; stage=events')){throw 'Expected class-list bootstrap failure differs'}
}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($file in Get-ChildItem $folder -File){Copy-Item -LiteralPath $file.FullName -Destination $out;if((Get-FileHash (Join-Path $out $file.Name)).Hash -cne (Get-FileHash $file.FullName).Hash){throw 'Shared DOM evidence readback differs'}}
@{evidenceVerified=$true;campaignPassed=$false;successfulChecks=162;successfulSuites=@('Selectors','Tree','Attributes');classListChecks=0;failure='Trusted bootstrap TimeoutException at events before first class-list assertion';originalRuntimeFilesUnchanged=19;newDriverFiles=2;timeoutFixed=$false;browserAdopted=$false;scope='Three original suites passed both modes; fourth stopped at trusted bootstrap. Clean owned retirement and stable bytes/ACL do not make the campaign pass. No changed engine binaries or retries.'}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
