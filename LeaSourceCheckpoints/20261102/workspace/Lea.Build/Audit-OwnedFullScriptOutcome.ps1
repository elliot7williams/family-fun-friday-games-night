$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$runRoot=Join-Path $root 'current-native-script-owned-run-v1'
$out=Join-Path $runRoot 'outcome-observation.json'
if(Test-Path -LiteralPath $out){throw 'Preserve read-only outcome observation'}
$paths=@{
    run=Join-Path $runRoot 'result.json'
    process=Join-Path $runRoot 'full-script-timed.result.json'
    build=Join-Path $root 'current-native-script-suite-v8/result.json'
    binding=Join-Path $root 'current-native-script-suite-v8/binding-verification.json'
    policy=Join-Path $PSScriptRoot 'Test-DecoderAclDelta.ps1'
    footer=Join-Path $PSScriptRoot 'Read-LeaSuiteFooter.ps1'
}
$expected=@{
    run='334A45678D717608E7B05C8722EA52C7928B51460658BBAE15740018DB54699D'
    process='E01640B4C0C3467A97431A9C74EA3E2EFAB54344AA9679F1589BF76356A4B248'
    build='FC4AD1E5E9249348B6A4A8D70609E3BB90ABD38E3458A52D5E52FDAB2F6543C1'
    binding='054089709A22C07E7D9C027AC8D03D26A5206D8A1C5B6BBD3B23634D326B689D'
    policy='15F46DBC7F4D4AD5B816DDFA1FE7213D02CAE1EDE79844AFBD904F281299E71E'
    footer='CE0005069067DDA4810317C0B9BFAF83339CD0DC929713458F76336FAC82D3B2'
}
$leases=[Collections.Generic.List[IDisposable]]::new();$pins=[Collections.Generic.List[object]]::new()
try{
    foreach($key in $paths.Keys){
        $s=[IO.File]::Open($paths[$key],'Open','Read','Read');$leases.Add($s)
        if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $expected[$key]){throw ('Evidence differs: '+$key)}
        $pins.Add(@{path=$paths[$key];sha256=$expected[$key]})
    }
    $r=Get-Content $paths.run -Raw|ConvertFrom-Json;$p=Get-Content $paths.process -Raw|ConvertFrom-Json
    $b=Get-Content $paths.build -Raw|ConvertFrom-Json;$binding=Get-Content $paths.binding -Raw|ConvertFrom-Json
    if(!$r.completed -or !$r.behaviorPassed -or !$r.footerVerified -or !$r.inputsUnchanged -or !$r.traceActivationVerified -or !$r.traceEnvironmentRestored -or $r.ownerAttention -or $r.passed -or $r.error){throw 'Expected behavior pass with retained raw audit failure'}
    if(!$p.passed -or !$p.exitObserved -or !$p.originalHandleCaptured -or $p.exitCode -ne 0 -or $p.timedOut -or $p.killRequested -or !$p.stdoutComplete -or !$p.stderrComplete -or !$p.outputDrained -or $p.captureOverflow -or !$p.childRetirementIndependentlyAudited -or $p.activeAfterRetirement -ne 0 -or $p.parentBudgetMs -ne 300000 -or $p.sharedRetirementDrainBudgetMs -ne 5000 -or $p.elapsedMs -gt 300000 -or $p.retirementAndDrainElapsedMs -gt 5000){throw 'Original process/Job/budget proof incomplete'}
    if(!$b.passed -or !$b.inputsUnchanged -or $b.ownerAttention -or !$binding.passed -or $b.sources.Count -ne 60 -or $b.files.Count -ne 65 -or $binding.buildReceiptSha256 -cne $expected.build){throw 'Build/binary binding incomplete'}
    foreach($file in @($b.files)+@($b.fixtureFiles)+@($r.inputFiles)){
        $s=[IO.File]::Open($file.path,'Open','Read','Read');$leases.Add($s)
        if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $file.sha256){throw 'Bound runtime/input bytes changed'}
    }
    . $paths.footer
    $footer=Read-LeaSuiteFooter -Lines @(Get-Content -LiteralPath $p.stdout)
    if($footer.Checks -ne 3412 -or $r.checks -ne $footer.Checks){throw 'Exact3412 terminal checks not verified'}
    $stderr=@(Get-Content -LiteralPath $p.stderr)
    $ends=@(foreach($line in $stderr){if($line -match '^LEA_SUITE_PHASE (\d+) (\d+) end$'){[int]$Matches[2]}})
    if(($ends -join ',') -cne ((0..88)-join ',')){throw 'All89 phase ends not verified'}
    if($r.changedFiles.Count -ne 1 -or $r.changedFiles[0].bytesChanged){throw 'Expected one metadata-only decoder delta'}
    . $paths.policy
    $change=$r.changedFiles[0];$image=Join-Path $r.candidate 'Lea.Image.exe'
    foreach($before in $r.before){
        $current=(Get-Acl -LiteralPath $before.path).Sddl
        if($before.path -ceq $image){if($current -cne $change.after){throw 'Recorded decoder ACL changed after test'}}
        elseif($current -cne $before.sddl){throw 'Unexpected current ACL delta'}
    }
    if(!(Test-DecoderAclDelta -Path $change.path -ExpectedDecoderPath $image -Before $change.before -After $change.after -ProfileName 'Lea.Image.Decoder')){throw 'Existing decoder policy did not classify exact delta'}
    foreach($case in @(
        @{name='script-path';path=(Join-Path $r.candidate 'ScriptWorker/Lea.Script.Worker.dll');profile='Lea.Image.Decoder';temporary=$false},
        @{name='other-profile';path=$image;profile='Lea.Script';temporary=$false},
        @{name='temporary-profile';path=$image;profile='Lea.Image.Decoder';temporary=$true}
    )){
        if(Test-DecoderAclDelta -Path $case.path -ExpectedDecoderPath $image -Before $change.before -After $change.after -ProfileName $case.profile -TemporaryProfile:$case.temporary){throw ('Decoder negative control accepted: '+$case.name)}
    }
    $result=@{observed=$true;rawAuditPassed=$false;executionPassed=$true;terminalChecks=$footer.Checks;completedPhases=89;exactBuildBindingVerified=$true;runtimeFiles=65;inputBytesHeld=$true;singleDecoderDeltaAccepted=$true;negativeControls=3;decoderPolicyChanged=$false;aclsModified=$false;profilesDeleted=$false;originalProcessAndJobRetired=$true;originalExecutionBudgetMs=300000;sharedRetirementBudgetMs=5000;elapsedMs=$p.elapsedMs;retirementMs=$p.retirementAndDrainElapsedMs;priorIntermittentFailuresResolved=$false;fullReleaseQualified=$false;inputs=$pins;scope='Read-only classification under existing decoder policy; raw result unchanged. One successful diagnostic campaign is not startup reliability, all-site/security qualification or cleanup/profile recovery proof.'}
    [IO.File]::WriteAllText($out,($result|ConvertTo-Json -Depth 6))
    $result|Select-Object executionPassed,terminalChecks,completedPhases,singleDecoderDeltaAccepted,rawAuditPassed,fullReleaseQualified|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
