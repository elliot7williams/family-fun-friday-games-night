param([ValidateSet(1,2)][int]$Version=2)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$path=Join-Path $root 'continuity-evidence/trusted-working-set-policy-v1/result.json'
$out="I:/LeaBuildStorage/working-set-policy-audit-20261102-v$Version"
if(Test-Path $out){throw 'Preserve working-set audit'}
if((Get-FileHash $path).Hash -cne 'B753902B772FA3E106C59A7190BC935360030162AD38B9CCD16E1E333F987BF3'){throw 'Policy receipt differs'}
$n=Get-Content $path -Raw|ConvertFrom-Json
if($n.passed -or !$n.diagnosticOnly -or $n.runtimeQualified -or !$n.measurementCaptured -or !$n.inputsUnchanged -or $n.ownerAttention -or $n.changedFiles.Count -or $n.before.Count -ne 12 -or $n.runs.Count -ne 2 -or $n.report.ok -or $n.report.status -cne 'limit' -or !$n.report.message.Contains('phase=bootstrap; reason=TimeoutException; stage=elements')){throw 'Typed diagnostic failure boundary differs'}
foreach($input in $n.inputs){if((Get-FileHash $input.path).Hash -cne $input.sha256){throw 'Policy input changed'}}
foreach($file in $n.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Policy runtime byte/ACL drift'}}
foreach($run in $n.runs){if(!$run.ownedJob -or !$run.exitObserved -or !$run.originalHandleCaptured -or !$run.childRetirementIndependentlyAudited -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or $run.timedOut -or $run.killRequested -or $run.unsafeToContinue -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Policy retirement differs'}}
if($n.runs[0].exitCode -ne 0 -or $n.runs[1].exitCode -ne 1){throw 'Policy process exits differ'}
$priorPath=Join-Path $root 'continuity-evidence/trusted-bootstrap-profile-v4/result.json'
if((Get-FileHash $priorPath).Hash -cne '2B084ABE6DD48B6C3B7D5AE7CA5EA4EEAFAD5C09CAAADD888E837D04854A59FA'){throw 'Prior runtime proof changed'}
$prior=Get-Content $priorPath -Raw|ConvertFrom-Json
foreach($old in $prior.before){$same=@($n.before|Where-Object path -CEQ $old.path);if($same.Count -ne 1 -or $same[0].sha256 -cne $old.sha256 -or $same[0].sddl -cne $old.sddl){throw 'Old shared runtime file altered'}}
if($Version -eq 2){
    $extra=@($n.before|Where-Object {$_.path -cnotin @($prior.before.path)})
    if($extra.Count -ne 2){throw 'Exactly two new driver files required'}
    foreach($file in $extra){if([IO.Path]::GetFileName($file.path) -cnotin @('Lea.WorkingSetPolicy.Tests.exe','Lea.WorkingSetPolicy.Tests.exe.config')){throw 'Unexpected new runtime file'}}
    if(($extra|ForEach-Object {(Get-Item $_.path).Length}|Measure-Object -Sum).Sum -gt 256KB){throw 'Driver staging bound exceeded'}
    $runtime=Split-Path $n.before[0].path
    if(@(Get-ChildItem $runtime -Recurse -File).Count -ne 12){throw 'Current shared runtime inventory changed'}
}
. (Join-Path $PSScriptRoot 'Read-WorkingSetPolicy.ps1')
. (Join-Path $PSScriptRoot 'Read-TrustedBootstrapProfile.ps1')
. (Join-Path $PSScriptRoot 'Read-BootstrapResources.ps1')
$policy=Read-WorkingSetPolicy $n.report.policyBefore $n.report.policyAfter $n.runs[1].pid
if(!$policy.unchanged -or $policy.hardMaximumObserved -or $policy.before.flags -ne 10 -or $policy.before.minimumBytes -ne 204800 -or $policy.before.maximumBytes -ne 1413120){throw 'Observed policy values differ'}
$lines=Get-Content $n.runs[1].stderr
$legacy=Read-TrustedBootstrapProfile $lines -Detailed -Failed
$resources=Read-BootstrapResources $lines @($legacy.rows|ForEach-Object label) 10000000
$test=Join-Path $root 'Lea.Tests/WorkingSetPolicy.test.ps1'
$tests=& pwsh -NoProfile -File $test
if($LASTEXITCODE -or $tests -cne 'PASS working-set policy parser: 9 cases'){throw 'Policy parser controls failed'}
New-Item -ItemType Directory -Path $out|Out-Null
$policy|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'policy.json') -Encoding utf8
$resources|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'resources.json') -Encoding utf8
$copies=[Collections.Generic.List[object]]::new();$i=0
foreach($from in @($path,$n.runs[0].stdout,$n.runs[0].stderr,$n.runs[1].stdout,$n.runs[1].stderr)){
    $to=Join-Path $out ((++$i).ToString('D2')+'-'+[IO.Path]::GetFileName($from));Copy-Item -LiteralPath $from -Destination $to
    $hash=(Get-FileHash $from).Hash;if((Get-FileHash $to).Hash -cne $hash){throw 'Evidence readback mismatch'};$copies.Add(@{original=$from;copy=$to;sha256=$hash})
}
@{verified=$true;startupFixed=$false;diagnosticOnly=$true;policyUnchanged=$true;hardMaximumObserved=$false;parserControls=9;runtimeFiles=12;priorTenFilesUnchanged=$true;twoNewDriverFilesOnly=$true;ownedRunsRetired=2;causeEstablished=$false;copies=$copies}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
