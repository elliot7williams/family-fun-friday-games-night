$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$folder='I:\LeaBuildStorage\current-resource-bootstrap-run-20261109-v1'
$out='I:\LeaBuildStorage\current-resource-bootstrap-audit-20261109-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve resource audit'}
$receipt=Join-Path $folder 'result.json'
if((Get-FileHash -LiteralPath $receipt).Hash -cne '4BDFDE9812B653099F478C5B9B37B0D68DDBA95B8A5DF0B40EF0CE6F2FF5AF07'){throw 'Current resource receipt differs'}
$r=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.resourceCounters -or !$r.detailedStages -or !$r.diagnosticOnly -or $r.runtimeQualified -or $r.hostAdopted -or !$r.measurementCaptured -or !$r.report.ok -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.error -or !$r.historicalMetadataRecovered -or $r.metadataReplacementExplicit -or $r.limitsChanged -or $r.before.Count -ne 10 -or $r.runs.Count -ne 3){throw 'Resource diagnostic scope differs'}
foreach($input in $r.inputs){if((Get-FileHash -LiteralPath $input.path).Hash -cne $input.sha256){throw 'Resource input changed'}}
foreach($file in $r.before){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256 -or (Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Resource runtime byte/ACL drift'}}
for($i=0;$i -lt 3;$i++){
    $run=$r.runs[$i];$budget=if($i -lt 2){120000}else{30000}
    if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.ownedJob -or !$run.originalHandleCaptured -or !$run.childRetirementIndependentlyAudited -or !$run.exitObserved -or !$run.outputDrained -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.captureOverflow -or $run.error){throw 'Owned resource run differs'}
}
$labels=@('enter','vm-begin','vm-end','prepare-begin','prepare-end','evaluate-begin','detail-script-entry','stage-declarations','detail-block-entry','stage-scope','detail-scope-ready','detail-timers-begin','detail-timers-end','detail-events-ready','stage-events','detail-interfaces-ready','stage-interfaces','stage-elements','stage-document','detail-bootstrap-done','evaluate-end','complete','end')
$lines=@(Get-Content -LiteralPath $r.runs[2].stderr)
$profileLines=@($lines|Where-Object {$_.StartsWith('LEA_TRUSTED_BOOT ')})
if($profileLines.Count -ne $labels.Count){throw 'Full profile required'}
$frequency=0L;$previous=$null;$profileRows=@()
foreach($line in $profileLines){
    if($line -cnotmatch '^LEA_TRUSTED_BOOT wallFrequency=([0-9]+) cpuFrequency=10000000 ([a-z-]+) wall=([0-9]+) cpu=([0-9]+) gc0=([0-9]+) gc1=([0-9]+) gc2=([0-9]+)$'){throw 'Malformed profile'}
    $f=[long]$Matches[1];$row=@{label=$Matches[2];wall=[long]$Matches[3];cpu=[long]$Matches[4];gc0=[long]$Matches[5];gc1=[long]$Matches[6];gc2=[long]$Matches[7]}
    if($f -le 0 -or ($frequency -and $f -ne $frequency) -or $row.label -cne $labels[$profileRows.Count]){throw 'Profile order/frequency differs'}
    if($previous){foreach($key in @('wall','cpu','gc0','gc1','gc2')){if($row[$key] -lt $previous[$key]){throw 'Profile counter moved backwards'}}}
    $frequency=$f;$previous=$row;$profileRows+=$row
}
. (Join-Path $PSScriptRoot 'Read-TrustedBootstrapProfile.ps1')
. (Join-Path $PSScriptRoot 'Read-BootstrapResources.ps1')
$original=Read-TrustedBootstrapProfile -Lines @($profileLines|Where-Object {$_ -cnotmatch ' detail-'})
$resources=Read-BootstrapResources -Lines $lines -Labels $labels -WallFrequency $frequency
for($i=0;$i -lt $labels.Count;$i++){if($resources.rows[$i].wall -lt $profileRows[$i].wall){throw 'Resource sample precedes paired profile marker'}}
$test=Join-Path $root 'Lea.Tests/BootstrapResources.test.ps1'
$parserOutput=& $test
if($parserOutput -cne 'PASS resource parser: 9 cases'){throw 'Resource parser controls failed'}
New-Item -ItemType Directory -Path $out|Out-Null
$resources|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $out 'resources.json') -Encoding utf8
$copies=@(foreach($file in Get-ChildItem -LiteralPath $folder -File){$to=Join-Path $out $file.Name;Copy-Item -LiteralPath $file.FullName -Destination $to;$h=(Get-FileHash -LiteralPath $file.FullName).Hash;if((Get-FileHash -LiteralPath $to).Hash -cne $h){throw 'Evidence readback differs'};@{path=$to;sha256=$h}})
@{verified=$true;diagnosticOnly=$true;pairedMarkers=23;resourceThreadId=$resources.threadId;parserControls=9;inputsAndRuntimeStable=$true;allThreeOwnedRunsRetired=$true;historicalMetadataRecovered=$true;startupFixed=$false;causeProven=$false;speedImprovementProven=$false;hostAdopted=$false;originalProfile=$original;resourcesSHA256=(Get-FileHash -LiteralPath (Join-Path $out 'resources.json')).Hash;parserSHA256=(Get-FileHash -LiteralPath (Join-Path $PSScriptRoot 'Read-BootstrapResources.ps1')).Hash;testSHA256=(Get-FileHash -LiteralPath $test).Hash;scriptSHA256=(Get-FileHash -LiteralPath $PSCommandPath).Hash;copies=$copies;scope='One instrumented successful probe on current diagnostic sources. Sequential, non-atomic snapshots; aggregate/adjacent intervals overlap. Not uninstrumented startup or sandbox qualification.'}|ConvertTo-Json -Depth 9|Set-Content -LiteralPath (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'audit.json')|Format-List Hash
$resources.intervals|Where-Object kind -CEQ 'aggregate'|ForEach-Object {[pscustomobject]$_}|Format-List
