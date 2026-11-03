$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/named-references-script-run-v1'
$receipt=Join-Path $root 'result.json'
$out=Join-Path $root 'verification.json'
if(Test-Path -LiteralPath $out){throw 'Preserve script result verification'}
if((Get-FileHash -LiteralPath $receipt).Hash -cne 'D37C971EE714141BCC203DA2315D8ABA9F979CA95367A856F46C5CCFAA129C7F'){throw 'Script receipt differs'}
$r=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
if(!$r.completed -or $r.passed -or !$r.behaviorPassed -or $r.checks -ne 3320 -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.runs.Count -ne 1 -or $r.changedFiles.Count -ne 1){throw 'Unexpected script result'}
$run=$r.runs[0]
if(!$run.passed -or !$run.exitObserved -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.elapsedMs -gt 300000){throw 'Script lifecycle differs'}
$policy=Join-Path $PSScriptRoot 'Test-DecoderAclDelta.ps1'
if((Get-FileHash -LiteralPath $policy).Hash -cne '15F46DBC7F4D4AD5B816DDFA1FE7213D02CAE1EDE79844AFBD904F281299E71E'){throw 'Permission classifier differs'}
. $policy
$change=$r.changedFiles[0]
$decoder=Join-Path $r.candidate 'Lea.Image.exe'
if($change.bytesChanged -or $change.path -cne $decoder -or (Get-FileHash -LiteralPath $decoder).Hash -cne 'FE744BE1D11E99B36A35F4E6CF12C45229AB7FF89134434E61438DE8DED3E535' -or (Get-Acl -LiteralPath $decoder).Sddl -cne $change.after){throw 'Decoder delta differs'}
$accepted=Test-DecoderAclDelta -Path $decoder -ExpectedDecoderPath $decoder -Before $change.before -After $change.after -ProfileName 'Lea.Image.Decoder'
if(!$accepted){throw 'Decoder delta not classified'}
foreach($file in $r.before){
    if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'Runtime bytes differ'}
    $expected=if($file.path -ceq $decoder){$change.after}else{$file.sddl}
    if((Get-Acl -LiteralPath $file.path).Sddl -cne $expected){throw 'Unclassified runtime permission change'}
}
foreach($file in $r.inputFiles){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'Protected input differs'}}
$phases=@(Get-Content -LiteralPath $run.stderr|Where-Object {$_ -cmatch '^LEA_SUITE_PHASE \d+ \d+ (begin|end)$'})
if($phases.Count -ne 178){throw 'Phase coverage differs'}
$previous=-1L
for($i=0;$i -lt $phases.Count;$i++){
    if($phases[$i] -cnotmatch '^LEA_SUITE_PHASE (\d+) (\d+) (begin|end)$'){throw 'Malformed phase'}
    $expectedEdge=if($i%2 -eq 0){'begin'}else{'end'}
    if([long]$Matches[1] -lt $previous -or [int]$Matches[2] -ne [Math]::Floor($i/2) -or $Matches[3] -cne $expectedEdge){throw 'Phase order differs'}
    $previous=[long]$Matches[1]
}
$result=@{verified=$true;fullBehaviorPassed=$true;checks=3320;stages=89;phaseLines=178;elapsedMs=$run.elapsedMs;deadlineHeadroomMs=300000-$run.elapsedMs;rawAuditPassed=$false;decoderDeltaClassified=$accepted;filesystemAclModified=$false;productionPolicyBroadened=$false;profileDeletionProven=$false;coldReliabilityProven=$false;releaseCertified=$false;receiptSha256=(Get-FileHash -LiteralPath $receipt).Hash;scope='Read-only classification of one exact decoder grant and verification of this completed full scripting run only'}
[IO.File]::WriteAllText($out,($result|ConvertTo-Json))
$result|ConvertTo-Json
(Get-FileHash -LiteralPath $out).Hash
