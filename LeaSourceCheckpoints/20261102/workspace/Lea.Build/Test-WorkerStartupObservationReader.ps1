$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Read-WorkerStartupObservation.ps1')
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/startup-observation-reader-controls-v2'
if(Test-Path -LiteralPath $out){throw 'Preserve reader controls'}
function Record($observation){'WORKER_STARTUP_OBSERVATION '+($observation|ConvertTo-Json -Depth 8 -Compress)}
function Example {
    @{valid=$true;retired=$true;traceRestored=$true;overflow=$false;durationExceeded=$false;error=$null;samples=@(
        @{wallMs=20;samplingMs=0;counters=@{CpuTicks=100};trace="startup entry: wall=1; cpu=0 ms"},
        @{wallMs=30;samplingMs=0;counters=@{CpuTicks=200};trace="startup entry: wall=1; cpu=0 ms`nstartup ready: wall=8; cpu=1 ms"})}
}
$rows=[Collections.Generic.List[object]]::new()
$positive=Read-WorkerStartupObservation -Lines @(Record (Example)) -Target 'synthetic'
if(!$positive.valid -or $positive.stages.Count -ne 2 -or $positive.maximumCaptureGapMs -ne 10){throw 'Positive reader control failed'}
$rows.Add(@{name='positive';passed=$true})
$withBlank=Read-WorkerStartupObservation -Lines @('',(Record (Example)),'') -Target 'synthetic'
if(!$withBlank.valid -or $withBlank.stages.Count -ne 2){throw 'Blank line control failed'}
$rows.Add(@{name='blank-lines';passed=$true})
foreach($name in @('missing','duplicate','invalid','unretired','unrestored','overflow','duration','error','empty','wall-regression','cpu-regression','stage-change','untimed')){
    $o=Example
    switch($name){
        'invalid' {$o.valid=$false}
        'unretired' {$o.retired=$false}
        'unrestored' {$o.traceRestored=$false}
        'overflow' {$o.overflow=$true}
        'duration' {$o.durationExceeded=$true}
        'error' {$o.error='test'}
        'empty' {$o.samples=@()}
        'wall-regression' {$o.samples[1].wallMs=19}
        'cpu-regression' {$o.samples[1].counters.CpuTicks=99}
        'stage-change' {$o.samples[1].trace='startup entry: wall=2; cpu=0 ms'}
        'untimed' {foreach($s in $o.samples){$s.trace='startup entry'}}
    }
    $lines=@(Record $o)
    if($name -eq 'missing'){$lines=@('no observation')}
    if($name -eq 'duplicate'){$lines=@($lines[0],$lines[0])}
    $rejected=$false
    try{$null=Read-WorkerStartupObservation -Lines $lines -Target 'synthetic'}catch{$rejected=$_.Exception.Message -match '^(Expected exactly|Observation|Startup stage|No timed)'}
    if(!$rejected){throw ('Reader negative control failed: '+$name)}
    $rows.Add(@{name=$name;passed=$true})
}
$null=New-Item -ItemType Directory -Path $out
$result=@{passed=$true;controls=$rows;runtimeExecuted=$false;inputs=@(Get-FileHash $PSCommandPath,(Join-Path $PSScriptRoot 'Read-WorkerStartupObservation.ps1')|Select-Object Path,Hash)}
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 6))
@{passed=$true;controls=$rows.Count}|ConvertTo-Json
