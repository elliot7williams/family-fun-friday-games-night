$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/all-startup-trace-proof-controls-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve trace proof controls'}
. (Join-Path $PSScriptRoot 'Read-AllStartupTraceProof.ps1')
$begin='LEA_ALL_STARTUP_TRACE enabled';$end='LEA_ALL_STARTUP_TRACE restored=true'
$rows=[Collections.Generic.List[object]]::new()
foreach($case in @(
    @{name='normal';lines=@($begin,$end);expected=$true},
    @{name='blank-lines';lines=@('',$begin,'phase output',$end,'');expected=$true},
    @{name='empty';lines=@();expected=$false},
    @{name='missing-end';lines=@($begin);expected=$false},
    @{name='missing-begin';lines=@($end);expected=$false},
    @{name='duplicate-begin';lines=@($begin,$begin,$end);expected=$false},
    @{name='duplicate-end';lines=@($begin,$end,$end);expected=$false},
    @{name='reversed';lines=@($end,$begin);expected=$false},
    @{name='false-end';lines=@($begin,'LEA_ALL_STARTUP_TRACE restored=false');expected=$false},
    @{name='unknown-marker';lines=@($begin,'LEA_ALL_STARTUP_TRACE unexpected',$end);expected=$false},
    @{name='incomplete-stream';lines=@($begin,$end);key='StreamsComplete';expected=$false},
    @{name='unobserved-exit';lines=@($begin,$end);key='ExitObserved';expected=$false},
    @{name='timeout-with-markers';lines=@($begin,$end);key='TimedOut';expected=$false},
    @{name='kill-with-markers';lines=@($begin,$end);key='KillRequested';expected=$false}
)){
    $args=@{Lines=$case.lines;StreamsComplete=$true;ExitObserved=$true;TimedOut=$false;KillRequested=$false}
    if($case.key){$args[$case.key]=$case.key -in @('TimedOut','KillRequested')}
    $proof=Read-AllStartupTraceProof @args
    if($proof.traceEnvironmentRestored -ne $case.expected){throw ('Trace proof control failed: '+$case.name)}
    $rows.Add(@{name=$case.name;passed=$true})
}
$runRoot=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/current-native-script-run-v8'
$processPath=Join-Path $runRoot 'full-script-timed.result.json'
$stderrPath=Join-Path $runRoot 'full-script-timed.stderr.txt'
if((Get-FileHash -LiteralPath $processPath).Hash -cne '06FB33D39DD593146FEE529D9251D2E7245D4B1732CD1BF755532586DFC3681F' -or
   (Get-FileHash -LiteralPath $stderrPath).Hash -cne '101F733CF233EB59EED4ECFA8D17527965180A72288FB856CF6AD552C94B0EA7'){throw 'Retained forced-stop evidence changed'}
$run=Get-Content -LiteralPath $processPath -Raw|ConvertFrom-Json
if(!$run.timedOut -or !$run.killRequested -or !$run.exitObserved){throw 'Retained forced-stop evidence differs'}
$actual=Read-AllStartupTraceProof -Lines @(Get-Content -LiteralPath $stderrPath) -StreamsComplete ($run.stdoutComplete -and $run.stderrComplete) -ExitObserved $run.exitObserved -TimedOut $run.timedOut -KillRequested $run.killRequested
if(!$actual.traceActivationVerified -or $actual.traceEnvironmentRestored){throw 'Retained forced stop was not classified fail-closed'}
$rows.Add(@{name='retained-V8-forced-stop';passed=$true})
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText((Join-Path $out 'result.json'),(@{passed=$true;controls=$rows;nativeProcessesStarted=$false;runnerIntegrated=$false;retainedFailureProof=$actual;inputs=@(Get-FileHash $processPath,$stderrPath,$PSCommandPath,(Join-Path $PSScriptRoot 'Read-AllStartupTraceProof.ps1')|Select-Object Path,Hash)}|ConvertTo-Json -Depth 7))
@{passed=$true;controls=$rows.Count;runnerIntegrated=$false;nativeProcessesStarted=$false}|ConvertTo-Json
