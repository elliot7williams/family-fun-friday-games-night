param(
    [Parameter(Mandatory=$true)][string]$RunDirectory,
    [Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedRunSha256
)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..')).TrimEnd('\')
$directory=[IO.Path]::GetFullPath($RunDirectory).TrimEnd('\')
if(!$directory.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Run outside workspace'}
$out=Join-Path $directory 'phase-summary.json'
if(Test-Path -LiteralPath $out){throw 'Preserve phase summary'}
$receipt=Join-Path $directory 'result.json'
if((Get-FileHash -LiteralPath $receipt).Hash -cne $ExpectedRunSha256){throw 'Run receipt changed'}
$run=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
if($run.runs.Count -ne 1){throw 'Expected one suite attempt'}
$parent=$run.runs[0]
if(!$parent.exitObserved -or !$parent.stdoutComplete -or !$parent.stderrComplete){throw 'Parent/output not terminal'}
$stageProof=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/suite-phase-source-v1/result.json'
if((Get-FileHash -LiteralPath $stageProof).Hash -cne '22948AC8AD6303C1D1FEAEFFE15CB30FBB03871619445D45F6606E176628C8C3'){throw 'Stage map changed'}
$map=(Get-Content -LiteralPath $stageProof -Raw|ConvertFrom-Json).stages
foreach($path in @($parent.stdout,$parent.stderr)){
    $full=[IO.Path]::GetFullPath($path)
    if(!$full.StartsWith($directory+'\',[StringComparison]::OrdinalIgnoreCase) -or (Get-Item -LiteralPath $full).Length -gt 4MB){throw 'Unsafe or oversized log'}
}
$markers=@(Get-Content -LiteralPath $parent.stderr|Where-Object {$_.StartsWith('LEA_SUITE_PHASE ')})
if($markers.Count -gt 178){throw 'Phase marker overflow'}
$phases=[Collections.Generic.List[object]]::new();$pending=$null;$last=-1L;$next=0
foreach($line in $markers){
    if($line -cnotmatch '^LEA_SUITE_PHASE (\d+) (\d+) (begin|end)$'){throw 'Malformed phase marker'}
    $ms=[long]$Matches[1];$index=[int]$Matches[2];$edge=$Matches[3]
    if($ms -lt $last -or $index -ge $map.Count){throw 'Invalid phase ordering'}
    $last=$ms
    if($edge -ceq 'begin'){
        if($pending -or $index -ne $next){throw 'Overlapping/missing phase'}
        $pending=@{index=$index;label=$map[$index];beginMs=$ms;endMs=$null;durationMs=$null}
    }else{
        if(!$pending -or $pending.index -ne $index){throw 'Unpaired phase end'}
        $pending.endMs=$ms;$pending.durationMs=$ms-$pending.beginMs
        $phases.Add($pending);$pending=$null;$next++
    }
}
if($pending){$phases.Add($pending)}
$summary=@{runReceiptSha256=$ExpectedRunSha256;parentExit=$parent.exitCode;parentElapsedMs=$parent.elapsedMs;parentTimedOut=$parent.timedOut;parentKilled=$parent.killRequested;observedStages=$phases.Count;completedStages=$next;unfinishedStage=$pending;phases=$phases;longestCompleted=@($phases|Where-Object {$null -ne $_.durationMs}|Sort-Object durationMs -Descending|Select-Object -First 10);passAssertions=@(Get-Content -LiteralPath $parent.stdout|Where-Object {$_.StartsWith('PASS ')}).Count;scope='Observed instrumented wall intervals only; no exclusive CPU/cause attribution or gate override'}
[IO.File]::WriteAllText($out,($summary|ConvertTo-Json -Depth 7),[Text.UTF8Encoding]::new($false))
$summary|Select-Object parentExit,parentElapsedMs,passAssertions,observedStages,completedStages,unfinishedStage,longestCompleted|ConvertTo-Json -Depth 5
(Get-FileHash -LiteralPath $out).Hash
