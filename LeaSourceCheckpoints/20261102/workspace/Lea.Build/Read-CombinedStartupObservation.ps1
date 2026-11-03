function Read-CombinedStartupObservation {
    param([Parameter(Mandatory)][AllowEmptyCollection()][AllowEmptyString()][string[]]$Lines)
    $prefix='COMBINED_WORKER_STARTUP '
    $records=@($Lines|Where-Object {$_.StartsWith($prefix,[StringComparison]::Ordinal)})
    if($records.Count -ne 1){throw 'Exactly one combined startup report required'}
    $report=$records[0].Substring($prefix.Length)|ConvertFrom-Json
    if(!$report.valid -or !$report.exited -or !$report.observerRetired -or !$report.traceRestored -or $report.sampleError -or $report.overflow -or $report.startupBudgetMs -ne 5000 -or $report.executionBudgetMs -ne 2000 -or $report.cpuRate -ne 2500 -or $report.memoryBytes -ne 134217728 -or $report.samples.Count -lt 1 -or $report.samples.Count -gt 60){throw 'Bounded original startup contract incomplete'}
    $lastWall=-1L;$cpu=-1L;$faults=-1L;$reads=-1L;$writes=-1L;$birth=0L;$pidSeen=0L
    $stages=@{};$states=@{}
    foreach($sample in $report.samples){
        $c=$sample.counters;$t=$sample.threads
        if(!$c -or !$t -or !$t.OriginalProcessHandleVerified -or $t.ProcessId -le 0 -or $t.ProcessCreated -le 0 -or !$t.Threads.Count -or $t.Threads.Count -gt 4096 -or $sample.wallMs -le $lastWall -or $sample.queryMs -lt 0 -or $c.CpuTicks -lt $cpu -or $c.PageFaults -lt $faults -or $c.ReadBytes -lt $reads -or $c.WriteBytes -lt $writes -or $c.TotalPhysicalBytes -le 0 -or $c.AvailablePhysicalBytes -gt $c.TotalPhysicalBytes){throw 'Invalid/nonmonotonic original-handle counters'}
        if($pidSeen -and ($t.ProcessId -ne $pidSeen -or $t.ProcessCreated -ne $birth)){throw 'Worker identity changed between samples'}
        $ids=[Collections.Generic.HashSet[long]]::new()
        foreach($thread in $t.Threads){if($thread.Id -le 0 -or !$ids.Add($thread.Id) -or $thread.Created -le 0 -or $thread.CpuTicks -lt 0 -or $thread.State -gt 9){throw 'Thread record invalid'}}
        foreach($match in [regex]::Matches($sample.trace,'startup ([a-z-]+): wall=([0-9]+); cpu=([0-9]+) ms')){
            $name=$match.Groups[1].Value;$stage=@{wallMs=[long]$match.Groups[2].Value;cpuMs=[long]$match.Groups[3].Value}
            if($stages.ContainsKey($name) -and ($stages[$name].wallMs -ne $stage.wallMs -or $stages[$name].cpuMs -ne $stage.cpuMs)){throw 'Worker checkpoint changed between samples'}
            $stages[$name]=$stage
        }
        $earliest=$t.Threads|Sort-Object Created,Id|Select-Object -First 1
        $key=[string]$earliest.State;if(!$states.ContainsKey($key)){$states[$key]=0};$states[$key]++
        $lastWall=$sample.wallMs;$cpu=$c.CpuTicks;$faults=$c.PageFaults;$reads=$c.ReadBytes;$writes=$c.WriteBytes;$birth=$t.ProcessCreated;$pidSeen=$t.ProcessId
    }
    if(!$stages.Count){throw 'No timed worker checkpoint captured'}
    $first=$report.samples[0];$last=$report.samples[-1]
    @{
        verified=$true;scriptPassed=$report.scriptPassed;status=$report.status;samples=$report.samples.Count
        mutations=$report.mutations;title=$report.title;sampleSpanMs=($last.wallMs-$first.wallMs)
        sampleCpuMs=(($last.counters.CpuTicks-$first.counters.CpuTicks)/10000.0)
        pageFaultDelta=($last.counters.PageFaults-$first.counters.PageFaults)
        logicalReadBytesDelta=($last.counters.ReadBytes-$first.counters.ReadBytes)
        earliestThreadStateCounts=$states;workerStages=$stages;causeProven=$false;startupFailureResolved=$false
        scope='Instrumentation adds cost; earliest thread is not proven managed VM thread, I/O is logical and faults are not exclusively hard faults. Diagnostic success is not full-suite or reliability qualification.'
    }
}
