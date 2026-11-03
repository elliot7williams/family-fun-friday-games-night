$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\combined-prefix-startup-audit-20261101-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve prefix audit'}
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$expected){
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($expected -and $hash -cne $expected){throw ('Prefix audit differs: '+$path)};$inputs.Add(@{path=$path;sha256=$hash})
}
function Proof([string]$path,[string]$hash){Pin $path $hash;return Get-Content -LiteralPath $path -Raw|ConvertFrom-Json}
function Process-Proof($run,[int]$budget){
    if(!$run.exitObserved -or !$run.originalHandleCaptured -or !$run.ownedJob -or $run.timedOut -or $run.killRequested -or !$run.outputDrained -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.captureOverflow -or !$run.childRetirementIndependentlyAudited -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Original process guards incomplete'}
    Pin $run.stdout;Pin $run.stderr
}
try{
    $controls=Proof 'I:\LeaBuildStorage\combined-startup-reader-controls-20261101-v2/result.json' 'C5B4F76913BA452ADD335202873303C4252F6776D850BDBF6D022DB838928859'
    $reader=Join-Path $PSScriptRoot 'Read-CombinedStartupObservation.ps1';Pin $reader $controls.readerSha256
    if(!$controls.passed -or $controls.positiveControls -ne 2 -or $controls.negativeControls -ne 8 -or $controls.interpreterExecuted){throw 'Reader controls differ'};. $reader
    $projection=Proof 'I:\LeaBuildStorage\combined-prefix-startup-source-20261101-v3/source.json' 'B04B2DEE1525FE6F3A8FF81A572FCA727F62CD3FD6F99262580040118DA2A040'
    if(!$projection.verified -or !$projection.originalAssertionPreserved -or !$projection.diagnosticOnly -or $projection.engineChanged -or $projection.originalPrefixPhases -ne 29 -or $projection.targetPhase -ne 29 -or $projection.files.Count -ne 3 -or $projection.testBudgetMs -ne 300000 -or $projection.startupBudgetMs -ne 5000 -or $projection.cleanupBudgetMs -ne 5000){throw 'Source scope differs'}
    foreach($file in $projection.files){Pin $file.path $file.sha256;Pin $file.origin $file.originalSha256;if(!$file.inverseVerified -or $file.negativeControls -ne 3){throw 'Source inverse controls incomplete'}}
    $program=@($projection.files|Where-Object {[IO.Path]::GetFileName($_.path) -ceq 'Program.cs'})[0]
    $old=[IO.File]::ReadAllText($program.origin);$new=[IO.File]::ReadAllText($program.path)
    $start='                SuitePhase("begin", 0);';$end='                SuitePhase("begin", 29);'
    $a=$old.IndexOf($start,[StringComparison]::Ordinal);$b=$old.IndexOf($end,[StringComparison]::Ordinal)
    $c=$new.IndexOf($start,[StringComparison]::Ordinal);$d=$new.IndexOf($end,[StringComparison]::Ordinal)
    if($a -lt 0 -or $b -le $a -or $c -lt 0 -or $d -le $c -or $old.Substring($a,$b-$a) -cne $new.Substring($c,$d-$c)){throw 'Original phases0..28 text changed'}
    $prefix=Proof 'I:\LeaBuildStorage\combined-prefix-startup-build-20261101-v1/result.json' '8F3A805C0EB0B21BAEC6892CA704E8A6EED8765BE8D498FF7EAB376BE40076F3'
    $campaign=Proof 'I:\LeaBuildStorage\combined-prefix-startup-campaign-20261101-v1/result.json' 'B769BE6096146565BBB90FC25FA3B53ED70E39CB986CCBEBD29EC6199FABE2FC'
    foreach($r in @($prefix,$campaign)){
        if(!$r.inputsUnchanged -or $r.changedFiles.Count -or $r.ownerAttention -or $r.before.Count -ne 65 -or $r.originalBefore.Count -ne 65 -or $r.fullSuitePassed -or $r.startupFailureResolved){throw 'Runtime integrity or qualification scope differs'}
        foreach($file in $r.inputs){Pin $file.path $file.sha256}
        foreach($file in @($r.before)+@($r.originalBefore)){if((Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Current runtime ACL differs'}}
        foreach($run in $r.runs){$budget=if($run.name.StartsWith('compile-')){120000}else{300000};Process-Proof $run $budget}
    }
    if(!$prefix.diagnosticCaptured -or !$prefix.prefixVerified -or !$prefix.computedPassed -or $prefix.runs.Count -ne 2 -or $prefix.compiledSources -ne 64 -or $prefix.stagedFiles -ne 247 -or !$prefix.entryFixtureBindingNotRequalified -or $campaign.diagnosticCaptured -or $campaign.runs.Count -ne 2 -or $campaign.observations.Count -ne 1 -or $campaign.error -cnotmatch 'Prefix stopped outside target capture'){throw 'Expected bounded campaign outcome differs'}
    $first=Read-CombinedStartupObservation -Lines @(Get-Content $prefix.runs[1].stdout)
    $repeat=Read-CombinedStartupObservation -Lines @(Get-Content $campaign.runs[0].stdout)
    foreach($pair in @(@($prefix.runs[1],$first),@($campaign.runs[0],$repeat))){
        if(!$pair[0].passed -or $pair[0].exitCode -ne 0 -or !$pair[1].scriptPassed -or $pair[1].mutations -ne 6 -or $pair[1].title -cne 'Lea 7'){throw 'Computed diagnostic success differs'}
        $lines=@(Get-Content $pair[0].stderr);$phases=@($lines|Where-Object {$_ -cmatch '^LEA_SUITE_PHASE '})
        if($phases.Count -ne 59){throw 'Observed full prefix count differs'}
        for($i=0;$i -lt 29;$i++){if($phases[2*$i] -cnotmatch ('^LEA_SUITE_PHASE [0-9]+ '+$i+' begin$') -or $phases[2*$i+1] -cnotmatch ('^LEA_SUITE_PHASE [0-9]+ '+$i+' end$')){throw 'Observed full prefix order differs'}}
        if($phases[-1] -cnotmatch '^LEA_SUITE_PHASE [0-9]+ 29 begin$' -or @($lines|Where-Object {$_ -ceq 'LEA_ALL_STARTUP_TRACE restored=true'}).Count -ne 1){throw 'Observed target/trace differs'}
    }
    $failed=$campaign.runs[1];$lines=@(Get-Content $failed.stderr);$phases=@($lines|Where-Object {$_ -cmatch '^LEA_SUITE_PHASE '})
    if($failed.passed -or $failed.exitCode -ne 1 -or $phases.Count -ne 17 -or $phases[-1] -cnotmatch '^LEA_SUITE_PHASE [0-9]+ 8 begin$' -or @($lines|Where-Object {$_ -ceq 'System.InvalidOperationException: FAIL computed script: limit; Script budget exceeded. phase=script; reason=TimeoutException'}).Count -ne 1 -or @($lines|Where-Object {$_ -ceq 'LEA_ALL_STARTUP_TRACE restored=true'}).Count -ne 1){throw 'Retained earlier trusted execution failure differs'}
    for($i=0;$i -lt 8;$i++){if($phases[2*$i] -cnotmatch ('^LEA_SUITE_PHASE [0-9]+ '+$i+' begin$') -or $phases[2*$i+1] -cnotmatch ('^LEA_SUITE_PHASE [0-9]+ '+$i+' end$')){throw 'Failed prefix sequence differs'}}
    foreach($input in $inputs){if((Get-FileHash $input.path).Hash -cne $input.sha256){throw 'Audit input changed'}}
    New-Item -ItemType Directory -Path $out|Out-Null
    @{verified=$true;fullSuitePassed=$false;startupFailureResolved=$false;causeProven=$false;originalPrefixTextUnchanged=$true;projectedSources=3;sourceNegativeControls=9;readerPositiveControls=2;readerNegativeControls=8;firstCapture=$first;repeatCapture=$repeat;earlierTrustedFailure=@{phase=8;boundary='script';reason='TimeoutException';elapsedMs=$failed.elapsedMs};inputs=$inputs;scope='Read-only audit. Two successful instrumented worker startups and an earlier trusted-page execution failure; original full-v7 phase29 failure remains unresolved. Unused embedded entry fixture retains original binding and is not requalified here.'}|ConvertTo-Json -Depth 9|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'result.json')
}finally{foreach($s in $holds){$s.Dispose()}}
