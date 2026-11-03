$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\combined-worker-startup-audit-20261101-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve startup audit'}
$pins=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new()
function Pin([string]$path,[string]$expected){
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($expected -and $hash -cne $expected){throw ('Startup audit input differs: '+$path)};$pins.Add(@{path=$path;sha256=$hash})
}
function Normalize([string]$text){return (($text.Replace("`r`n","`n").Split("`n")|Where-Object {$_.Trim().Length}) -join "`n")}
function Valid-Observation($report){
    if(!$report.valid -or !$report.traceRestored -or !$report.exited -or !$report.observerRetired -or $report.sampleError -or $report.overflow -or $report.startupBudgetMs -ne 5000 -or $report.executionBudgetMs -ne 2000 -or $report.cpuRate -ne 2500 -or $report.memoryBytes -ne 134217728 -or $report.samples.Count -lt 1 -or $report.samples.Count -gt 60){return $false}
    $pidSeen=0L;$birth=0L;$lastWall=-1L;$cpu=-1L;$faults=-1L;$reads=-1L;$writes=-1L
    foreach($sample in $report.samples){
        $c=$sample.counters;$t=$sample.threads
        if(!$c -or !$t -or !$t.OriginalProcessHandleVerified -or $t.ProcessId -le 0 -or $t.ProcessCreated -le 0 -or !$t.Threads.Count -or $t.Threads.Count -gt 4096 -or $sample.wallMs -le $lastWall -or $sample.queryMs -lt 0 -or $c.CpuTicks -lt $cpu -or $c.PageFaults -lt $faults -or $c.ReadBytes -lt $reads -or $c.WriteBytes -lt $writes -or $c.AvailablePhysicalBytes -gt $c.TotalPhysicalBytes){return $false}
        if($pidSeen -and ($t.ProcessId -ne $pidSeen -or $t.ProcessCreated -ne $birth)){return $false}
        $ids=[Collections.Generic.HashSet[long]]::new()
        foreach($thread in $t.Threads){if($thread.Id -le 0 -or !$ids.Add($thread.Id) -or $thread.Created -le 0 -or $thread.CpuTicks -lt 0 -or $thread.State -gt 9){return $false}}
        $pidSeen=$t.ProcessId;$birth=$t.ProcessCreated;$lastWall=$sample.wallMs;$cpu=$c.CpuTicks;$faults=$c.PageFaults;$reads=$c.ReadBytes;$writes=$c.WriteBytes
    }
    return $true
}
try{
    $path='I:\LeaBuildStorage\combined-worker-startup-20261101-v2\result.json';Pin $path 'BCE712793B3CCC1EC7EA1FC0F8569491F0D00689B59632C9FA2383F894925D30'
    $r=Get-Content $path -Raw|ConvertFrom-Json
    if(!$r.diagnosticCaptured -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.snapshotControls -ne 13 -or $r.runs.Count -ne 4 -or $r.before.Count -ne 65){throw 'Valid retired diagnostic required'}
    foreach($file in $r.inputs){Pin $file.path $file.sha256}
    foreach($file in $r.before){if((Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Current runtime ACL differs'}}
    foreach($run in $r.runs){$budget=if($run.name.StartsWith('compile-')){120000}else{30000};if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.exitObserved -or !$run.outputDrained -or !$run.childRetirementIndependentlyAudited -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Original process guards differ'};Pin $run.stdout;Pin $run.stderr}
    $old=Join-Path $PSScriptRoot 'OwnedThreadSnapshot.cs';Pin $old '43F229376D069D3BF7CA7BC9CFDA5F0F36AD9BABC80DB125ADF800A40E552D52'
    $new=Join-Path $PSScriptRoot 'OwnedThreadSnapshotV2.cs';Pin $new
    $original=Normalize ([IO.File]::ReadAllText($old));$candidate=Normalize ([IO.File]::ReadAllText($new))
    function Inverse([string]$text){
        $insert='                    // Bound target allocation without rejecting a large unrelated process.'+"`n"+'                    if (row.Threads > 4096) throw new InvalidDataException("Target thread count bound");'+"`n"
        if([regex]::Matches($text,[regex]::Escape($insert)).Count -ne 1 -or [regex]::Matches($text,'public static class OwnedThreadSnapshotV2').Count -ne 1){return $false}
        $text=$text.Replace($insert,'').Replace('public static class OwnedThreadSnapshotV2','public static class OwnedThreadSnapshot')
        $anchor='(row.Next != 0 && row.Next % 8 != 0) || 256L'
        if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){return $false}
        return $text.Replace($anchor,'(row.Next != 0 && row.Next % 8 != 0) || row.Threads > 4096 || 256L') -ceq $original
    }
    if(!(Inverse $candidate)){throw 'Thread sampler common-source inverse differs'}
    foreach($bad in @($candidate.Replace('4096','4095'),($candidate+$candidate),($candidate+"`n// unrelated change"))){if(Inverse $bad){throw 'Sampler inverse negative accepted'}}
    $report=$r.observation
    if(!(Valid-Observation $report) -or !$report.scriptPassed -or $report.mutations -ne 6 -or $report.title -cne 'Lea 7'){throw 'Computed diagnostic or sample report differs'}
    foreach($kind in 1..5){
        $bad=$report|ConvertTo-Json -Depth 15|ConvertFrom-Json
        switch($kind){1{$bad.samples=@()}2{$bad.valid=$false}3{$bad.samples[0].threads.OriginalProcessHandleVerified=$false}4{$bad.samples[-1].threads.ProcessCreated++}5{$bad.samples[-1].wallMs=-1}}
        if(Valid-Observation $bad){throw 'Malformed sample report accepted'}
    }
    $first=$report.samples[0];$last=$report.samples[-1]
    $stages=@{};$states=@{}
    foreach($sample in $report.samples){
        foreach($match in [regex]::Matches($sample.trace,'startup ([a-z-]+): wall=([0-9]+); cpu=([0-9]+) ms')){$name=$match.Groups[1].Value;$stage=@{wallMs=[long]$match.Groups[2].Value;cpuMs=[long]$match.Groups[3].Value};if($stages.ContainsKey($name) -and ($stages[$name].wallMs -ne $stage.wallMs -or $stages[$name].cpuMs -ne $stage.cpuMs)){throw 'Observed worker checkpoint changed'};$stages[$name]=$stage}
        $earliest=$sample.threads.Threads|Sort-Object Created,Id|Select-Object -First 1
        $key=[string]$earliest.State;if(!$states.ContainsKey($key)){$states[$key]=0};$states[$key]++
    }
    if(!$stages.ContainsKey('configured') -or !$stages.ContainsKey('vm')){throw 'Expected configured and VM checkpoints not captured'}
    foreach($pin in $pins){if((Get-FileHash -LiteralPath $pin.path).Hash -cne $pin.sha256){throw 'Audit input changed'}}
    New-Item -ItemType Directory -Path $out|Out-Null
    @{verified=$true;samplerInverseVerified=$true;samplerNegativeControls=3;reportNegativeControls=5;nativeControls=13;samples=$report.samples.Count;scriptPassed=$true;fullSuitePassed=$false;startupFailureResolved=$false;causeProven=$false;sampleSpanMs=($last.wallMs-$first.wallMs);sampleCpuMs=(($last.counters.CpuTicks-$first.counters.CpuTicks)/10000.0);pageFaultDelta=($last.counters.PageFaults-$first.counters.PageFaults);logicalReadBytesDelta=($last.counters.ReadBytes-$first.counters.ReadBytes);earliestThreadStateCounts=$states;workerStages=$stages;inputs=$pins;scope='Read-only diagnostic audit. Sampling adds cost; earliest thread is not proven managed VM thread, I/O is logical, faults are not exclusively hard faults. Focused success does not resolve full-suite failure.'}|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'result.json')
}finally{foreach($s in $holds){$s.Dispose()}}
