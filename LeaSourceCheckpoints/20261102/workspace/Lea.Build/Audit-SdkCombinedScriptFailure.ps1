$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$candidate=Join-Path $root 'continuity-evidence/sdk-full-script-suite-v7'
$runRoot=Join-Path $root 'continuity-evidence/sdk-full-script-owned-run-v7'
$out='I:\LeaBuildStorage\sdk-combined-script-failure-audit-20261101-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve failure audit'}
$holds=[Collections.Generic.List[IDisposable]]::new();$pins=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$expected){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse audit input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($expected -and $hash -cne $expected){throw ('Audit input differs: '+$path)}
    $pins.Add(@{path=$path;sha256=$hash})
}
function Proof([string]$path,[string]$hash){Pin $path $hash;return Get-Content -LiteralPath $path -Raw|ConvertFrom-Json}
try{
    $r=Proof (Join-Path $runRoot 'result.json') 'CBECBD7932C23EF6FF187ED115A43D792528FE36B92675D4B1456302D2DBEB65'
    $p=Proof (Join-Path $runRoot 'full-script-timed.result.json') '7EF4C0E6EFA80FB464DB8677F56DA8F98844C257F57CC74A37C53E4CB5895E51'
    $staging=Proof (Join-Path $candidate 'staging.json') 'F693258746A0E08024A72011F9E5E6EE242766FAD468BE4849FF4BCC1C0BFBE8'
    $audit=Proof (Join-Path $candidate 'staging-audit.json') '0C5A7E6CE310807FBA80FF6FC74B5555FBF5AFE2389D46527D7C0A23A7476C14'
    if($r.passed -or !$r.completed -or $r.behaviorPassed -or !$r.inputsUnchanged -or !$r.traceActivationVerified -or !$r.traceEnvironmentRestored -or $r.changedFiles.Count -or $r.ownerAttention -or $r.validationError -or $r.runs.Count -ne 1){throw 'Expected cleanly retired behavior failure'}
    if($p.passed -or $p.exitCode -ne 1 -or !$p.exitObserved -or !$p.originalHandleCaptured -or !$p.ownedJob -or $p.timedOut -or $p.killRequested -or !$p.outputDrained -or !$p.stdoutComplete -or !$p.stderrComplete -or $p.captureOverflow -or !$p.childRetirementIndependentlyAudited -or $p.activeAfterRetirement -ne 0 -or $p.parentBudgetMs -ne 300000 -or $p.sharedRetirementDrainBudgetMs -ne 5000 -or $p.elapsedMs -gt 300000 -or $p.retirementAndDrainElapsedMs -gt 5000){throw 'Failure process ownership/budget proof incomplete'}
    if(!$staging.verified -or !$audit.verified -or !$audit.embeddedFixtureReadWithoutLoadingHost -or $audit.embeddedFiles -ne 7 -or $staging.files.Count -ne 247 -or $r.before.Count -ne 65 -or $staging.requiredChecks -ne 3412 -or $staging.requiredPhases -ne 89){throw 'Candidate coverage differs'}
    foreach($file in $staging.files){Pin (Join-Path $candidate $file.path) $file.sha256}
    foreach($file in $r.before){if((Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Current runtime ACL differs'}}
    $library=Proof 'I:\LeaBuildStorage\full-sdk-libraries-build-20261101-v1\result.json' '3AF0DA862CF1FA7C1E3E53E949B7EA0F37DE6593BEE8DDC5A02EFA8653522E6B'
    $native=Proof 'I:\LeaBuildStorage\full-sdk-native-build-20261101-v1\result.json' '20E2EE1F8A87BFC2B14CD581E48B59D1DE834E87EBA729DCB5A45CCF4E1A7893'
    $roles=0
    foreach($file in $library.libraryProducts|Where-Object {[IO.Path]::GetFileName($_.path) -cne 'Lea.Print.dll'}){Pin (Join-Path $staging.runtime ([IO.Path]::GetFileName($file.path))) $file.sha256;$roles++}
    foreach($file in $native.nativeProducts){Pin (Join-Path $staging.runtime ('ScriptWorker/'+[IO.Path]::GetFileName($file.path))) $file.sha256;$roles++}
    if($roles -ne 11){throw 'Combined eleven-role binding differs'}
    Pin $p.stderr;Pin $p.stdout
    $stderr=Get-Content -LiteralPath $p.stderr -Raw
    $ends=@(foreach($line in Get-Content -LiteralPath $p.stderr){if($line -cmatch '^LEA_SUITE_PHASE [0-9]+ ([0-9]+) end$'){[int]$Matches[1]}})
    if(($ends -join ',') -cne ((0..28)-join ',') -or $stderr -cnotmatch 'LEA_SUITE_PHASE [0-9]+ 29 begin' -or $stderr -cnotmatch 'FAIL isolated computed script: timeout; Worker startup deadline exceeded\. phase=hello;' -or $stderr -cnotmatch 'startup configured: wall=602; cpu=93 ms' -or $stderr -cmatch 'startup vm:'){throw 'Retained failure signature differs'}
    foreach($pin in $pins){if((Get-FileHash -LiteralPath $pin.path).Hash -cne $pin.sha256){throw 'Audit input changed'}}
    New-Item -ItemType Directory -Path $out|Out-Null
    @{verified=$true;executionPassed=$false;fullSuitePassed=$false;releaseQualified=$false;completedPhases=29;failedPhase=29;combinedRoles=11;selectedFiles=247;runtimeAclsUnchanged=$true;workerStartupBudgetMs=5000;originalSuiteBudgetMs=300000;originalCleanupBudgetMs=5000;elapsedMs=$p.elapsedMs;retirementMs=$p.retirementAndDrainElapsedMs;causeProven=$false;scope='Read-only audit of retained startup failure; configured is the final checkpoint, not exclusive constructor timing or cause proof.';inputs=$pins}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'result.json')
}finally{foreach($s in $holds){$s.Dispose()}}
