$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/rebuilt-core-script-run-v1'
$path=Join-Path $root 'result.json';$out=Join-Path $root 'outcome-verification.json'
if(Test-Path -LiteralPath $out){throw 'Preserve outcome verification'}
if((Get-FileHash -LiteralPath $path).Hash -cne '4B7CED0181E8AD91C0BFF2D8A6A03A42690D68D1342BFD660D4CDA00E113A290'){throw 'Receipt changed'}
$r=Get-Content -LiteralPath $path -Raw|ConvertFrom-Json
if($r.passed -or !$r.completed -or !$r.behaviorPassed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.error -or $r.runs.Count -ne 1){throw 'Unexpected outcome'}
$run=$r.runs[0]
if(!$run.passed -or $run.elapsedMs -gt 300000 -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.exitObserved -or !$run.stdoutComplete -or !$run.stderrComplete){throw 'Parent not clean'}
$footer=Join-Path $PSScriptRoot 'Read-LeaSuiteFooter.ps1'
if((Get-FileHash -LiteralPath $footer).Hash -cne 'CE0005069067DDA4810317C0B9BFAF83339CD0DC929713458F76336FAC82D3B2'){throw 'Footer helper changed'}
. $footer
$completion=Read-LeaSuiteFooter -Lines @(Get-Content -LiteralPath $run.stdout)
if($completion.Checks -ne 3320){throw 'Check count changed'}
$phases=@(Get-Content -LiteralPath $run.stderr|Where-Object{$_ -cmatch '^LEA_SUITE_PHASE \d+ \d+ (begin|end)$'})
if($phases.Count -ne 178){throw 'Phase count differs'}
for($i=0;$i -lt 89;$i++){
    if($phases[$i*2] -cnotmatch ('^LEA_SUITE_PHASE (\d+) '+$i+' begin$')){throw 'Missing ordered begin'}
    $begin=[long]$Matches[1]
    if($phases[$i*2+1] -cnotmatch ('^LEA_SUITE_PHASE (\d+) '+$i+' end$') -or [long]$Matches[1] -lt $begin){throw 'Missing or invalid ordered end'}
}
foreach($file in @($r.before)+@($r.inputFiles)){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'Input/candidate changed'}}
$changes=@($r.before|Where-Object{(Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl})
if($changes.Count -ne 1 -or $r.changedFiles.Count -ne 1 -or $r.changedFiles[0].bytesChanged){throw 'Unexpected runtime delta'}
$policy=Join-Path $PSScriptRoot 'Test-DecoderAclDelta.ps1'
if((Get-FileHash -LiteralPath $policy).Hash -cne '15F46DBC7F4D4AD5B816DDFA1FE7213D02CAE1EDE79844AFBD904F281299E71E'){throw 'Policy changed'}
. $policy
if(!(Test-DecoderAclDelta -Path $changes[0].path -ExpectedDecoderPath (Join-Path $r.candidate 'Lea.Image.exe') -Before $changes[0].sddl -After (Get-Acl -LiteralPath $changes[0].path).Sddl -ProfileName 'Lea.Image.Decoder')){throw 'Unclassified decoder grant'}
$result=@{behaviorVerified=$true;checks=3320;orderedStages=89;elapsedMs=$run.elapsedMs;onlyDeltaIsFixedDecoderGrant=$true;rawAuditPassed=$false;filesystemAclModified=$false;profileDeletionProven=$false;releaseCertified=$false}
[IO.File]::WriteAllText($out,($result|ConvertTo-Json))
$result|ConvertTo-Json
(Get-FileHash -LiteralPath $out).Hash
