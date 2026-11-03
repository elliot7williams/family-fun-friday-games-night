$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\deterministic-phase-cost-comparison-20261102-v1'
if(Test-Path $out){throw 'Preserve phase comparison'}
$old=Join-Path $root 'continuity-evidence/temporal-startup-v1/relative-font-script-owned-run-v1/result.json'
$new=Join-Path $root 'continuity-evidence/sdk-full-script-owned-run-v8/result.json'
$programs=@(
    (Join-Path $root 'continuity-evidence/temporal-startup-v1/relative-font-script-suite-v1/source/Lea.Script.Tests/Program.cs'),
    'I:\LeaBuildStorage\sdk-full-script-assembly-20261101-v8/compiled-source/source/Lea.Script.Tests/Program.cs'
)
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$expected){
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($expected -and $hash -cne $expected){throw 'Phase comparison input differs'}
    $inputs.Add(@{path=$path;sha256=$hash})
}
function ReadPhases([string[]]$lines,[int]$completed,[int]$pendingIndex){
    $rows=[Collections.Generic.List[object]]::new();$pending=$null;$next=0;$previous=-1L
    foreach($line in $lines){
        if(!$line.StartsWith('LEA_SUITE_PHASE ')){continue}
        if($line -cnotmatch '^LEA_SUITE_PHASE ([0-9]+) ([0-9]+) (begin|end)$'){throw 'Malformed phase marker'}
        $time=[long]$Matches[1];$index=[int]$Matches[2];$edge=$Matches[3]
        if($time -lt $previous -or $index -ne $next -or $index -gt 88){throw 'Phase sequence differs'};$previous=$time
        if($edge -ceq 'begin'){if($pending){throw 'Duplicate begin'};$pending=@{index=$index;beginMs=$time}}
        else{if(!$pending){throw 'End without begin'};$rows.Add(@{index=$index;beginMs=$pending.beginMs;endMs=$time;durationMs=($time-$pending.beginMs)});$pending=$null;$next++}
    }
    if($rows.Count -ne $completed -or ($pendingIndex -eq -1 -and $pending) -or ($pendingIndex -ne -1 -and (!$pending -or $pending.index -ne $pendingIndex))){throw 'Phase coverage differs'}
    return @{phases=$rows.ToArray();pending=$pending}
}
try{
    Pin $PSCommandPath ''
    Pin $old '7F1448D51CF16421602D7D02DDC58F066BDAF5DB9E22A8E8B3728C525B5BA205'
    Pin $new 'FC796BCEA5B95CF5B509CCE4661B8BBC356EC4C2C0FF93DE8DC6D0CC0E75ADD9'
    $a=Get-Content $old -Raw|ConvertFrom-Json;$b=Get-Content $new -Raw|ConvertFrom-Json
    if(!$a.passed -or !$a.footerVerified -or $a.runs.Count -ne 1 -or $b.passed -or $b.runs.Count -ne 1 -or !$b.runs[0].timedOut){throw 'Historical pass/current timeout boundary differs'}
    foreach($run in @($a.runs[0],$b.runs[0])){if(!$run.exitObserved -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne 300000){throw 'Retained process boundary differs'};Pin $run.stderr '';Pin $run.stdout ''}
    foreach($p in $programs){Pin $p 'E4C8C28FE50A3B8AA76344D618BE4CCCE6E9B998DB5E2688DA102A462949EF0A'}
    $source=Get-Content $programs[1] -Raw
    $matches=[regex]::Matches($source,'(?ms)^\s*SuitePhase\("begin", (?<index>[0-9]+)\);\r?\n(?<body>.*?)^\s*SuitePhase\("end", \k<index>\);')
    if($matches.Count -ne 89){throw 'Expected exact source marker regions'}
    $labels=@{};foreach($m in $matches){$index=[int]$m.Groups['index'].Value;if($labels.ContainsKey($index)){throw 'Duplicate source phase'};$labels[$index]=$m.Groups['body'].Value.Trim()}
    $prior=ReadPhases @(Get-Content $a.runs[0].stderr) 89 -1
    $current=ReadPhases @(Get-Content $b.runs[0].stderr) 86 86
    $rows=@(for($i=0;$i -lt 86;$i++){
        if(!$labels.ContainsKey($i)){throw 'Source phase absent'}
        @{index=$i;statement=$labels[$i];priorMs=$prior.phases[$i].durationMs;currentMs=$current.phases[$i].durationMs;deltaMs=($current.phases[$i].durationMs-$prior.phases[$i].durationMs)}
    })
    $negative=0
    foreach($bad in @(
        @('LEA_SUITE_PHASE 0 0 begin','LEA_SUITE_PHASE 1 0 begin'),
        @('LEA_SUITE_PHASE 2 0 begin','LEA_SUITE_PHASE 1 0 end'),
        @('LEA_SUITE_PHASE 0 1 begin'),
        @('LEA_SUITE_PHASE malformed'),
        @('LEA_SUITE_PHASE 0 0 begin')
    )){$denied=$false;try{$null=ReadPhases $bad 1 -1}catch{$denied=$true};if(!$denied){throw 'Malformed trace accepted'};$negative++}
    New-Item -ItemType Directory -Path $out|Out-Null
    @{verified=$true;inputs=$inputs;negativeControls=$negative;programBytesIdentical=$true;comparedCompletedPhases=86;interruptedPhase=$current.pending;phaseCosts=$rows;largestSlowdowns=@($rows|Sort-Object deltaMs -Descending|Select-Object -First 12);largestSpeedups=@($rows|Sort-Object deltaMs|Select-Object -First 12);priorTailMs=($prior.phases[88].endMs-$prior.phases[85].endMs);scope='Single historical pass versus current timeout, identical Program marker regions but different products/paths/runtime conditions. Wall time includes tests, I/O, scheduling and engine work. Not controlled benchmark, causality, whole-source equivalence or release qualification';candidateQualified=$false}|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'result.json')
}finally{foreach($s in $holds){$s.Dispose()}}
