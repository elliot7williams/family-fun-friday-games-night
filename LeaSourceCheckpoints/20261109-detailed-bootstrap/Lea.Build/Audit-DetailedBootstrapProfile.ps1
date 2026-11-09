$ErrorActionPreference='Stop'
$folder='I:\LeaBuildStorage\detailed-bootstrap-profile-run-20261109-v1'
$out='I:\LeaBuildStorage\detailed-bootstrap-profile-audit-20261109-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve previous audit'}
$receipt=Join-Path $folder 'result.json'
if((Get-FileHash -LiteralPath $receipt).Hash -cne '6365D0C5C5E0C526B29BA7A3A4EF1C5455C2471F55E09B8815D7B4ED59B67C6D'){throw 'Diagnostic receipt differs'}
$r=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
if($r.passed -or !$r.detailedStages -or !$r.diagnosticOnly -or !$r.measurementCaptured -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.error -or !$r.historicalMetadataRecovered -or $r.metadataReplacementExplicit -or $r.limitsChanged -or $r.runs.Count -ne 3 -or $r.before.Count -ne 10){throw 'Failure scope differs'}
if($r.report.ok -or $r.report.status -cne 'limit' -or $r.report.message -cne 'Script budget exceeded. phase=bootstrap; reason=TimeoutException; stage=elements'){throw 'Unexpected failure'}
for($i=0;$i -lt 3;$i++){
    $run=$r.runs[$i];$budget=if($i -lt 2){120000}else{30000};$exit=if($i -lt 2){0}else{1}
    if($run.exitCode -ne $exit -or $run.timedOut -or !$run.originalHandleCaptured -or !$run.ownedJob -or !$run.exitObserved -or !$run.outputDrained -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.captureOverflow -or $run.error){throw 'Ownership/deadline boundary differs'}
    if($i -lt 2 -and (!$run.passed -or $run.killRequested)){throw 'Compile failed or needed cleanup'}
    if($i -eq 2 -and ($run.passed -or !$run.killRequested)){throw 'Preserve recorded failed-probe cleanup request'}
}
foreach($input in $r.inputs){if((Get-FileHash -LiteralPath $input.path).Hash -cne $input.sha256){throw 'Diagnostic input changed'}}
foreach($file in $r.before){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256 -or (Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Diagnostic runtime bytes/ACL changed'}}
$expected=@('enter','vm-begin','vm-end','prepare-begin','prepare-end','evaluate-begin','detail-script-entry','stage-declarations','detail-block-entry','stage-scope','detail-scope-ready','detail-timers-begin','detail-timers-end','detail-events-ready','stage-events','detail-interfaces-ready','stage-interfaces','stage-elements','evaluate-end','end')
function Read-DetailTrace([string[]]$Lines){
    if($Lines.Count -ne $expected.Count){throw 'Trace count differs'}
    $rows=@();$frequency=0L
    foreach($line in $Lines){
        if($line -cnotmatch '^LEA_TRUSTED_BOOT wallFrequency=([0-9]+) cpuFrequency=10000000 ([a-z-]+) wall=([0-9]+) cpu=([0-9]+) gc0=([0-9]+) gc1=([0-9]+) gc2=([0-9]+)$'){throw 'Malformed marker'}
        $f=[long]$Matches[1];$row=@{label=$Matches[2];wall=[long]$Matches[3];cpu=[long]$Matches[4];gc0=[long]$Matches[5];gc1=[long]$Matches[6];gc2=[long]$Matches[7]}
        if($f -le 0 -or $row.label -cne $expected[$rows.Count] -or ($rows.Count -and $f -ne $frequency)){throw 'Trace sequence/frequency differs'}
        if($rows.Count){foreach($key in @('wall','cpu','gc0','gc1','gc2')){if($row[$key] -lt $rows[-1][$key]){throw 'Counter moved backwards'}}}
        $frequency=$f;$rows+=$row
    }
    return @{rows=$rows;frequency=$frequency}
}
$lines=@(Get-Content -LiteralPath $r.runs[2].stderr|Where-Object {$_.StartsWith('LEA_TRUSTED_BOOT ')})
$parsed=Read-DetailTrace $lines
$bad=[Collections.Generic.List[object]]::new();$bad.Add(@($lines|Select-Object -SkipLast 1));$bad.Add(@($lines)+@($lines[0]))
$copy=[string[]]$lines.Clone();$copy[8]=$copy[8].Replace('detail-block-entry','detail-wrong');$bad.Add($copy)
$copy=[string[]]$lines.Clone();$copy[8]=[regex]::Replace($copy[8],'wallFrequency=[0-9]+','wallFrequency=999');$bad.Add($copy)
$copy=[string[]]$lines.Clone();$copy[8]=[regex]::Replace($copy[8],' wall=[0-9]+',' wall=0');$bad.Add($copy)
$copy=[string[]]$lines.Clone();$copy[-1]=[regex]::Replace($copy[-1],'gc0=[0-9]+','gc0=-1');$bad.Add($copy)
foreach($case in $bad){$rejected=$false;try{$null=Read-DetailTrace $case}catch{$rejected=$true};if(!$rejected){throw 'Malformed trace accepted'}}
. (Join-Path $PSScriptRoot 'Read-TrustedBootstrapProfile.ps1')
$original=Read-TrustedBootstrapProfile -Lines @($lines|Where-Object {$_ -cnotmatch ' detail-'}) -Failed
$by=@{};foreach($row in $parsed.rows){$by[$row.label]=$row}
$intervals=@(foreach($pair in @(@('evaluate-begin','detail-script-entry'),@('stage-declarations','detail-block-entry'),@('detail-scope-ready','detail-timers-begin'),@('detail-timers-begin','detail-timers-end'),@('detail-timers-end','detail-events-ready'),@('stage-events','detail-interfaces-ready'))){$a=$by[$pair[0]];$b=$by[$pair[1]];@{begin=$pair[0];end=$pair[1];wallMs=1000.0*($b.wall-$a.wall)/$parsed.frequency;processCpuMs=1000.0*($b.cpu-$a.cpu)/10000000}})
New-Item -ItemType Directory -Path $out|Out-Null
$copies=@(foreach($file in Get-ChildItem -LiteralPath $folder -File){$to=Join-Path $out $file.Name;Copy-Item -LiteralPath $file.FullName -Destination $to;$hash=(Get-FileHash -LiteralPath $file.FullName).Hash;if((Get-FileHash -LiteralPath $to).Hash -cne $hash){throw 'Evidence copy differs'};@{path=$to;sha256=$hash}})
@{evidenceVerified=$true;startupPassed=$false;timeoutFixed=$false;causeProven=$false;speedImprovementProven=$false;historicalMetadataRecovered=$true;failedProbeCleanupRequested=$true;negativeControls=$bad.Count;markers=$parsed.rows.Count;originalProfile=$original;intervals=$intervals;copies=$copies;scriptSHA256=(Get-FileHash -LiteralPath $PSCommandPath).Hash;scope='One instrumented diagnostic failure. Includes observer costs and setup; intervals do not prove causality. Original runtime binaries unchanged.'}|ConvertTo-Json -Depth 9|Set-Content -LiteralPath (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'audit.json')|Format-List Hash
$intervals|Format-Table begin,end,wallMs,processCpuMs
