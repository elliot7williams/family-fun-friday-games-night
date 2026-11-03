$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$receipt=Join-Path $root 'continuity-evidence/sdk-full-script-owned-run-v8/result.json'
$out='I:\LeaBuildStorage\deterministic-full-script-timeout-20261102-v1'
if(Test-Path $out){throw 'Preserve timeout evidence'}
if((Get-FileHash $receipt).Hash -cne 'FC796BCEA5B95CF5B509CCE4661B8BBC356EC4C2C0FF93DE8DC6D0CC0E75ADD9'){throw 'Timeout receipt differs'}
$r=Get-Content $receipt -Raw|ConvertFrom-Json
if($r.passed -or !$r.completed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.before.Count -ne 65 -or $r.runs.Count -ne 1 -or $r.changedFiles.Count -ne 7 -or !$r.traceActivationVerified -or $r.traceEnvironmentRestored){throw 'Retained failure scope differs'}
$run=$r.runs[0]
if(!$run.timedOut -or !$run.killRequested -or $run.parentBudgetMs -ne 300000 -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000 -or !$run.exitObserved -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.retainedForOwner){throw 'Timeout/retirement boundary differs'}
$files=@(foreach($file in $r.before){
    $hash=(Get-FileHash $file.path).Hash;$sddl=(Get-Acl $file.path).Sddl
    if($hash -cne $file.sha256){throw 'Retained runtime bytes changed'}
    @{path=$file.path;sha256=$hash;originalSddl=$file.sddl;currentSddl=$sddl;aclChanged=($sddl -cne $file.sddl)}
})
if(@($files|Where-Object aclChanged).Count -ne 7){throw 'ACL difference coverage differs'}
$phases=[Collections.Generic.List[object]]::new();$pending=$null;$next=0;$previous=-1L
foreach($line in Get-Content $run.stderr){
    if($line -cnotmatch '^LEA_SUITE_PHASE ([0-9]+) ([0-9]+) (begin|end)$'){continue}
    $time=[long]$Matches[1];$index=[int]$Matches[2];$edge=$Matches[3]
    if($time -lt $previous -or $index -ne $next){throw 'Nonmonotonic phase sequence'};$previous=$time
    if($edge -ceq 'begin'){
        if($pending){throw 'Duplicate phase begin'};$pending=@{index=$index;beginMs=$time}
    }else{
        if(!$pending){throw 'Phase end without begin'}
        $phases.Add(@{index=$index;beginMs=$pending.beginMs;endMs=$time;durationMs=($time-$pending.beginMs)})
        $pending=$null;$next++
    }
}
if($phases.Count -ne 86 -or !$pending -or $pending.index -ne 86){throw 'Expected phase86 interrupted after86 completed phases'}
$program='I:\LeaBuildStorage\sdk-full-script-assembly-20261101-v8/compiled-source/source/Lea.Script.Tests/Program.cs'
$text=Get-Content $program -Raw
$sequence='SuitePhase("begin", 86);'+"`r`n"+'                Contains.Run(containsFixtureFull, "isolated");'
if(!$text.Contains($sequence) -and !$text.Contains($sequence.Replace("`r`n","`n"))){throw 'Phase86 source binding differs'}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($p in @($receipt,$run.stdout,$run.stderr)){
    $to=Join-Path $out ([IO.Path]::GetFileName($p));Copy-Item -LiteralPath $p -Destination $to
    if((Get-FileHash $p).Hash -cne (Get-FileHash $to).Hash){throw 'Evidence copy differs'}
}
@{verified=$true;scope='Exact retained deadline failure, runtime byte/ACL difference snapshot and phase-wall attribution; no causal claim or cleanup repair';sourceReceiptSha256=(Get-FileHash $receipt).Hash;runtimeFiles=$files;completedPhases=$phases;interruptedPhase=$pending;phase86='Contains.Run isolated';largestCompletedPhases=@($phases|Sort-Object durationMs -Descending|Select-Object -First 10);timingFailuresResolved=$false;aclRepaired=$false;candidateQualified=$false;traceRestorationProven=$false;runtimeByteChanges=0;aclChanges=7;ownerRetiredSeparately=$true}|ConvertTo-Json -Depth 8|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
