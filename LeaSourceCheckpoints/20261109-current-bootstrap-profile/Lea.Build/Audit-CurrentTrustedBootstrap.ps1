$ErrorActionPreference='Stop'
$out='I:\LeaBuildStorage\current-bootstrap-profile-audit-20261109-v1'
$folder='I:\LeaBuildStorage\current-bootstrap-profile-run-20261109-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve profile audit'}
$receipt=Join-Path $folder 'result.json'
if((Get-FileHash -LiteralPath $receipt).Hash -cne 'E926C855BBAA2528E7FD113A2D1A337C616A1F0173C44B06B74C94EEA22D3B0F'){throw 'Current diagnostic receipt differs'}
$r=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.diagnosticOnly -or !$r.measurementCaptured -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.before.Count -ne 10 -or $r.runs.Count -ne 3 -or $r.historicalMetadataRecovered -or !$r.metadataReplacementExplicit -or $r.limitsChanged){throw 'Diagnostic scope differs'}
for($i=0;$i -lt 3;$i++){
    $run=$r.runs[$i];$budget=if($i -lt 2){120000}else{30000}
    if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.exitObserved -or !$run.outputDrained -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Original ownership/deadline boundary differs'}
}
foreach($input in $r.inputs){if((Get-FileHash -LiteralPath $input.path).Hash -cne $input.sha256){throw 'Profile input changed'}}
foreach($file in $r.before){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256 -or (Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Profile runtime bytes/ACL changed'}}
. (Join-Path $PSScriptRoot 'Read-TrustedBootstrapProfile.ps1')
$lines=@(Get-Content -LiteralPath $r.runs[2].stderr|Where-Object {$_.StartsWith('LEA_TRUSTED_BOOT ')})
$profile=Read-TrustedBootstrapProfile $lines
$bad=[Collections.Generic.List[object]]::new()
$bad.Add(@($lines|Select-Object -SkipLast 1));$bad.Add(@($lines)+@($lines[0]))
$copy=[string[]]$lines.Clone();$copy[1]=[regex]::Replace($copy[1],'wallFrequency=[0-9]+','wallFrequency=999');$bad.Add($copy)
$copy=[string[]]$lines.Clone();$copy[1]=[regex]::Replace($copy[1],' wall=[0-9]+',' wall=0');$bad.Add($copy)
$copy=[string[]]$lines.Clone();$copy[-1]=$copy[-1].Replace(' end wall=',' unexpected wall=');$bad.Add($copy)
$copy=[string[]]$lines.Clone();$copy[-1]=[regex]::Replace($copy[-1],'gc0=[0-9]+','gc0=-1');$bad.Add($copy)
$negative=0
foreach($case in $bad){$rejected=$false;try{$null=Read-TrustedBootstrapProfile $case}catch{$rejected=$true};if(!$rejected){throw 'Malformed diagnostic trace accepted'};$negative++}
$events=@($profile.rows|Where-Object label -CEQ 'stage-events')[0]
$interfaces=@($profile.rows|Where-Object label -CEQ 'stage-interfaces')[0]
$frequency=[long]([regex]::Match($lines[0],'wallFrequency=([0-9]+)').Groups[1].Value)
$profile.intervals.Add(@{phase='events-to-interfaces';wallMs=1000.0*($interfaces.wall-$events.wall)/$frequency;processCpuMs=1000.0*($interfaces.cpu-$events.cpu)/[TimeSpan]::TicksPerSecond;gc0=$interfaces.gc0-$events.gc0;gc1=$interfaces.gc1-$events.gc1;gc2=$interfaces.gc2-$events.gc2})
New-Item -ItemType Directory -Path $out|Out-Null
$copies=@(foreach($file in Get-ChildItem -LiteralPath $folder -File){
    $to=Join-Path $out $file.Name;Copy-Item -LiteralPath $file.FullName -Destination $to
    $hash=(Get-FileHash -LiteralPath $file.FullName).Hash
    if((Get-FileHash -LiteralPath $to).Hash -cne $hash){throw 'Audit evidence readback differs'}
    @{path=$to;sha256=$hash}
})
@{evidenceVerified=$true;diagnosticOnly=$true;profile=$profile;negativeControls=$negative;copies=$copies;
    diagnosticParentSha256=$r.diagnosticParentSha256;historicalMetadataRecovered=$false;
    productionRuntimeChanged=$false;timeoutFixed=$false;causeProven=$false;speedImprovementProven=$false;
    runnerSha256=(Get-FileHash -LiteralPath $PSCommandPath).Hash;
    scope='One fresh diagnostic process; explicit replacement metadata, original Js/Vo operations and policies. Buffered observation perturbs timing. Nested intervals must not be summed.'}|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'audit.json')|Format-List Hash
