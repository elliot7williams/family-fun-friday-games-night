function Read-WorkerStartupObservation {
    param([Parameter(Mandatory)][AllowEmptyString()][string[]]$Lines,[Parameter(Mandatory)][string]$Target)
    $prefix='WORKER_STARTUP_OBSERVATION '
    $records=@($Lines|Where-Object {$_.StartsWith($prefix,[StringComparison]::Ordinal)})
    if($records.Count -ne 1){throw 'Expected exactly one startup observation'}
    $observation=$records[0].Substring($prefix.Length)|ConvertFrom-Json
    if(!$observation.valid -or !$observation.retired -or !$observation.traceRestored -or $observation.error -or $observation.overflow -or $observation.durationExceeded){throw 'Observation incomplete'}
    $samples=@($observation.samples)
    if($samples.Count -lt 1 -or $samples.Count -gt 240){throw 'Observation sample count differs'}
    $stages=[Collections.Generic.List[object]]::new()
    $known=@{};$lastWall=-1L;$lastCpu=-1L;$maxGap=0L
    foreach($sample in $samples){
        if($null -eq $sample.counters -or $null -eq $sample.counters.CpuTicks -or $sample.wallMs -lt $lastWall -or $sample.counters.CpuTicks -lt $lastCpu -or $sample.samplingMs -lt 0){throw 'Observation counters are invalid or nonmonotonic'}
        if($lastWall -ge 0){$maxGap=[Math]::Max($maxGap,([long]$sample.wallMs-$lastWall))}
        $lastWall=[long]$sample.wallMs;$lastCpu=[long]$sample.counters.CpuTicks
        foreach($line in ($sample.trace -split '\r?\n')){
            if($line -match '^startup ([a-z-]+): wall=(\d+); cpu=(\d+) ms$'){
                $name=$Matches[1];$wall=[long]$Matches[2];$cpu=[long]$Matches[3]
                if($known.ContainsKey($name)){
                    if($known[$name].workerWallMs -ne $wall -or $known[$name].workerCpuMs -ne $cpu){throw 'Startup stage changed across captures'}
                }else{
                    $stage=@{stage=$name;workerWallMs=$wall;workerCpuMs=$cpu;firstCapturedWallMs=[long]$sample.wallMs}
                    $known[$name]=$stage;$stages.Add($stage)
                }
            }
        }
    }
    if(!$stages.Count){throw 'No timed startup stages captured'}
    $previousWall=-1L;$previousCpu=-1L
    foreach($stage in $stages){
        if($stage.workerWallMs -lt $previousWall -or $stage.workerCpuMs -lt $previousCpu){throw 'Worker stage clock is nonmonotonic'}
        $previousWall=$stage.workerWallMs;$previousCpu=$stage.workerCpuMs
    }
    @{
        valid=$true;target=$Target;samples=$samples.Count;traceRestored=$true;retired=$true
        stages=$stages;maximumCaptureGapMs=$maxGap
        firstCounters=$samples[0].counters;lastCounters=$samples[-1].counters
        scope='Read-only bounded first-startup observation. Capture and worker clocks differ; instrumentation adds cost. Logical I/O is not disk I/O, total faults are not hard faults. A successful diagnostic startup does not resolve an intermittent failure or prove a speedup.'
    }
}
