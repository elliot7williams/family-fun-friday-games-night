$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$candidate=Join-Path $root 'continuity-evidence/private-binding-full-suite-v3'
$runRoot=Join-Path $root 'continuity-evidence/private-binding-full-owned-v3'
$out='I:\LeaBuildStorage\private-binding-full-audit-20261103-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve existing full-suite audit'}
$holds=[Collections.Generic.List[IDisposable]]::new()
$copies=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$hash){
    $stream=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $hash){throw ('Audit identity differs: '+$path)}
}
try{
    $receipt=Join-Path $runRoot 'result.json';Pin $receipt 'FBF61B45F9D8F933B7C57BF44C67F78BA26484098BC5848A7B7754989EA44B55'
    $r=Get-Content $receipt -Raw|ConvertFrom-Json
    if(!$r.passed -or !$r.completed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.before.Count -ne 65 -or $r.runs.Count -ne 1 -or $r.checks -ne 3412 -or !$r.footerVerified -or !$r.phasesVerified -or !$r.traceActivationVerified -or !$r.traceEnvironmentRestored -or !$r.privateBindingPair){throw 'Full run boundary incomplete'}
    $run=$r.runs[0]
    if(!$run.passed -or !$run.ownedJob -or !$run.originalHandleCaptured -or !$run.exitObserved -or !$run.childRetirementIndependentlyAudited -or !$run.outputDrained -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.activeAfterRetirement -ne 0 -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or $run.unsafeToContinue -or $run.parentBudgetMs -ne 300000 -or $run.elapsedMs -gt 300000 -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Original ownership/deadline boundary differs'}
    foreach($input in $r.inputs){Pin $input.path $input.sha256}
    foreach($file in $r.before){Pin $file.path $file.sha256;if((Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Runtime ACL changed'}}
    $stagingPath=Join-Path $candidate 'staging.json';Pin $stagingPath '9EBB2B57ABFEDAB0275414887CF04115DA24F85D0AF28CDE2C91DEB6806CC846'
    $staging=Get-Content $stagingPath -Raw|ConvertFrom-Json
    $auditPath=Join-Path $candidate 'staging-audit.json';Pin $auditPath '2BDC07FCFC776C02A155F1673A19BB75B454A4462254562943539FD99A780546'
    $stagedAudit=Get-Content $auditPath -Raw|ConvertFrom-Json
    if(!$staging.verified -or $staging.files.Count -ne 247 -or !$stagedAudit.verified -or !$stagedAudit.embeddedFixtureReadWithoutLoadingHost -or $stagedAudit.embeddedFiles -ne 7){throw 'Stage/resource audit incomplete'}
    foreach($file in $staging.files){Pin (Join-Path $candidate $file.path) $file.sha256}
    if(@(Get-ChildItem (Join-Path $candidate 'runtime') -Recurse -File).Count -ne 65){throw 'Current runtime inventory differs'}
    Pin (Join-Path $candidate 'runtime/Lea.Script.dll') '406C5634638D75C66892BD95429760CB4A5C74C94277913D1DF467147E30E75F'
    Pin (Join-Path $candidate 'runtime/ScriptWorker/Lea.Script.Worker.dll') 'F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0'
    Pin (Join-Path $candidate 'fixtures/Lea.Script/Dom.js') 'A228494E1D20D2669DC2FED38B93B7549EF8849FA2C7997F45EE32678117239A'
    Pin (Join-Path $candidate 'fixtures/Lea.Script/Dom.compact.js') '196BCE962F2C2F375C77644D0AE19485820F599170D4D80013829A6ECD2886E3'
    $config=Get-Content (Join-Path $candidate 'runtime/ScriptWorker/Lea.Script.Worker.runtimeconfig.json') -Raw|ConvertFrom-Json
    if($config.runtimeOptions.framework.version -cne '8.0.33' -or $config.runtimeOptions.rollForward -cne 'Disable'){throw 'Worker runtime policy differs'}
    $footer=Join-Path $PSScriptRoot 'Read-LeaSuiteFooter.ps1';Pin $footer 'CE0005069067DDA4810317C0B9BFAF83339CD0DC929713458F76336FAC82D3B2';. $footer
    $proof=Read-LeaSuiteFooter -Lines @(Get-Content $run.stdout) -ExpectedChecks 3412
    $phases=@(Get-Content $run.stderr|Where-Object {$_ -cmatch '^LEA_SUITE_PHASE [0-9]+ [0-9]+ (begin|end)$'})
    if($phases.Count -ne 178){throw 'Full phase count differs'}
    for($i=0;$i -lt 89;$i++){if($phases[2*$i] -cnotmatch ('^LEA_SUITE_PHASE [0-9]+ '+$i+' begin$') -or $phases[2*$i+1] -cnotmatch ('^LEA_SUITE_PHASE [0-9]+ '+$i+' end$')){throw 'Phase pair sequence differs'}}
    New-Item -ItemType Directory -Path $out|Out-Null
    foreach($path in @($receipt,$stagingPath,$auditPath,$run.stdout,$run.stderr)){
        $to=Join-Path $out ([IO.Path]::GetFileName($path));Copy-Item -LiteralPath $path -Destination $to
        $hash=(Get-FileHash $path).Hash;if((Get-FileHash $to).Hash -cne $hash){throw 'Evidence copy differs'}
        $copies.Add(@{source=$path;copy=$to;sha256=$hash})
    }
    @{verified=$true;checks=$proof.Checks;phases=89;runtimeFiles=65;selectedFiles=247;runtimeBytesAndAclsUnchanged=$true;inputsUnchanged=$true;allOwnedProcessesRetired=$true;elapsedMs=$run.elapsedMs;deadlineMarginMs=(300000-$run.elapsedMs);performanceImprovementProven=$false;hostAdopted=$false;releaseQualified=$false;copies=$copies;runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'audit.json')
}finally{foreach($hold in $holds){$hold.Dispose()}}
