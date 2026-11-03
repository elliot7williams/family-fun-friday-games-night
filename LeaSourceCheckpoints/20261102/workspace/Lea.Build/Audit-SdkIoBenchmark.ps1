$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'continuity-evidence/sdk-script-io-benchmark-audit-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve benchmark audit'}
$cases=@(
    @('baseline-v1','retire','A0F71B58E9DEE613417F84B96AB8EA1792987B476B9F4BF67FCBD7256C760A77'),
    @('candidate-v1','retain','08FF79210A09DC309DAACF9ADAE62775161F71518C1A0ACD3A97A9EA3D04A1DB'),
    @('candidate-v2','retain','68AFAF2D6477F625AAFCB981D07698BBB485D56A5DC7460F1342EE77FC4468F3'),
    @('baseline-v2','retire','C0E5A167A5F02B2757B361CDC70948DC04C9F74657BA6D8A8EDE9E539D26F6D2')
)
function ParseSamples([string[]]$lines,[string]$mode){
    if($lines.Count -ne 35 -or $lines[0] -cnotmatch ('^BENCHMARK '+$mode+' ([1-9][0-9]*) 32$')){throw 'Benchmark header/inventory differs'}
    $frequency=[long]$Matches[1]
    if($lines[1] -cnotmatch '^FIRST ([1-9][0-9]*)$'){throw 'First execution timing absent'}
    $first=[long]$Matches[1];$ticks=[Collections.Generic.List[long]]::new()
    for($i=0;$i -lt 32;$i++){
        if($lines[$i+2] -cnotmatch ('^SAMPLE '+$i+' ([1-9][0-9]*)$')){throw 'Benchmark sample order/value differs'}
        $ticks.Add([long]$Matches[1])
    }
    if($lines[-1] -cne 'PASS SDK IO BENCHMARK 37'){throw 'Benchmark completion differs'}
    return @{frequency=$frequency;firstTicks=$first;samples=@($ticks);firstMs=$first*1000.0/$frequency;sampleMs=@($ticks|ForEach-Object {$_*1000.0/$frequency})}
}
function Stats([double[]]$values){
    $sorted=@($values|Sort-Object);$n=$sorted.Count
    if(!$n){throw 'Empty benchmark population'}
    $middle=[int][Math]::Floor($n/2)
    $median=if($n%2){$sorted[$middle]}else{($sorted[$middle-1]+$sorted[$middle])/2.0}
    return @{count=$n;totalMs=($values|Measure-Object -Sum).Sum;meanMs=($values|Measure-Object -Average).Average;medianMs=$median;p95NearestRankMs=$sorted[[int][Math]::Ceiling(0.95*$n)-1]}
}
$rows=[Collections.Generic.List[object]]::new();$lastFinish=[DateTimeOffset]::MinValue;$sourcePin=$null;$negative=0
foreach($case in $cases){
    $directory=Join-Path $root ('continuity-evidence/sdk-script-io-benchmark-'+$case[0]);$receipt=Join-Path $directory 'result.json'
    if((Get-FileHash $receipt).Hash -cne $case[2]){throw 'Benchmark receipt differs'}
    $run=Get-Content $receipt -Raw|ConvertFrom-Json
    if(!$run.passed -or !$run.benchmarkOnly -or !$run.inputsUnchanged -or !$run.workerAclRestored -or $run.ownerAttention -or $run.warmupCalls -ne 0 -or $run.measuredRequests -ne 32 -or $run.runs.Count -ne 2){throw 'Benchmark lifecycle incomplete'}
    foreach($process in $run.runs){
        if(!$process.passed -or $process.exitCode -ne 0 -or $process.timedOut -or $process.killRequested -or $process.activeAfterRetirement -ne 0 -or !$process.outputDrained -or !$process.childRetirementIndependentlyAudited -or $process.retirementAndDrainElapsedMs -gt 5000){throw 'Benchmark process gate failed'}
    }
    if([DateTimeOffset]::Parse($run.runs[0].birthUtc) -lt $lastFinish){throw 'Benchmark ABBA processes overlapped'}
    $lastFinish=[DateTimeOffset]::Parse($run.runs[1].finishedUtc)
    foreach($file in $run.inputs){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw ('Benchmark input changed: '+$file.path)}}
    foreach($file in $run.workerAclBefore){if((Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Benchmark worker ACL differs'}}
    $source=Join-Path $root 'Lea.Tests/SdkSessionIoBenchmarkMain.cs'
    $pins=@($run.inputs|Where-Object path -ceq $source|Select-Object -ExpandProperty sha256 -Unique)
    if($pins.Count -ne 1 -or ($sourcePin -and $sourcePin -cne $pins[0])){throw 'Benchmark workload differs'};$sourcePin=$pins[0]
    $parent=if($case[1] -ceq 'retain'){'0425BFF9D9289C5F50DCA565BB257A394DA3D529F96202FA6D6D7324905707B7'}else{'906E3CAAE4C160C7FDCF9D7E79B08076B9A12F46A76715AE02CAE9E72983764C'}
    foreach($pair in @(@('Lea.Script.dll',$parent),@('Lea.Engine.dll','5B8A501DA627467A6C261A355917DE5F73DF8F00758B49C1D240CF7D596E8A4A'),@('Lea.Win.dll','A622A1C266AD7CA27F437832E390E594F7E419CAFED700C9F466F0517970CE0D'),@('ScriptWorker/Lea.Script.Worker.dll','F8BE188734FB0B747C131E396269E9698A02E257ED2DCF6DCB1DB7DECADFC81C'))){if((Get-FileHash (Join-Path $directory $pair[0])).Hash -cne $pair[1]){throw 'Benchmark component identity differs'}}
    $process=$run.runs[1]
    if($process.parentBudgetMs -ne 30000 -or $run.runs[0].parentBudgetMs -ne 120000 -or (Get-Item $process.stdout).Length -ne $process.stdoutBytes -or (Get-Item $process.stderr).Length -ne 0){throw 'Benchmark budget/output differs'}
    $lines=@(Get-Content -LiteralPath $process.stdout);$parsed=ParseSamples $lines $case[1]
    if(!$negative){
        $badHeader=$lines.Clone();$badHeader[0]='BENCHMARK invalid 1 32'
        $badOrder=$lines.Clone();$badOrder[3]='SAMPLE 0 1'
        foreach($invalid in @($badHeader,$badOrder,@($lines|Select-Object -Skip 1))){try{$null=ParseSamples $invalid $case[1]}catch{$negative++}}
        if($negative -ne 3){throw 'Benchmark parser negative controls failed'}
    }
    $rows.Add(@{case=$case[0];policy=$case[1];receipt=$receipt;receiptSha256=$case[2];stdoutSha256=(Get-FileHash $process.stdout).Hash;firstMs=$parsed.firstMs;frequency=$parsed.frequency;sampleTicks=$parsed.samples;sampleMs=$parsed.sampleMs;statistics=(Stats $parsed.sampleMs)})
}
$baseline=Stats @($rows|Where-Object policy -ceq 'retire'|ForEach-Object {$_.sampleMs})
$candidate=Stats @($rows|Where-Object policy -ceq 'retain'|ForEach-Object {$_.sampleMs})
if($baseline.count -ne 64 -or $candidate.count -ne 64){throw 'All planned samples required'}
New-Item -ItemType Directory -Path $out|Out-Null
@{verified=$true;order='ABBA';cases=$rows;baseline=$baseline;candidate=$candidate;meanReductionPercent=100*(1-$candidate.meanMs/$baseline.meanMs);warmupCalls=0;parserNegativeControls=3;workloadSha256=$sourcePin;browserSpeedupProven=$false;startupFailureResolved=$false;releaseQualified=$false;scope='Four process cases on one machine,32 retained-realm DOM requests each; first execution separately measured; OS caches not flushed; no statistical or broad browser-speed claim';runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 8|Set-Content (Join-Path $out 'comparison.json') -Encoding utf8
Get-FileHash (Join-Path $out 'comparison.json')
