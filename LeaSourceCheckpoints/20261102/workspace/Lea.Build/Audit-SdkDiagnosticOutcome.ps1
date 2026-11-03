$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$directory=Join-Path $root 'continuity-evidence/sdk-full-script-owned-run-v4'
$out=Join-Path $directory 'diagnostic-outcome.json'
if(Test-Path -LiteralPath $out){throw 'Preserve diagnostic outcome audit'}
$receipt=Join-Path $directory 'result.json'
if((Get-FileHash $receipt).Hash -cne '8FB97484B25C460994C5190A7B28BCAA09C8A9327968E5587F6B3406222C024C'){throw 'Diagnostic run receipt differs'}
$run=Get-Content $receipt -Raw|ConvertFrom-Json
if(!$run.diagnosticOnly -or $run.passed -or !$run.inputsUnchanged -or $run.ownerAttention -or $run.runs.Count -ne 1){throw 'Unexpected diagnostic outcome'}
$process=$run.runs[0]
if(!$process.timedOut -or !$process.killRequested -or !$process.originalHandleCaptured -or !$process.exitObserved -or !$process.childRetirementIndependentlyAudited -or $process.activeAfterRetirement -ne 0 -or !$process.outputDrained -or $process.retirementAndDrainElapsedMs -gt 5000){throw 'Deadline/retirement evidence incomplete'}
foreach($stream in @('stdout','stderr')){if((Get-Item -LiteralPath $process.$stream).Length -ne $process.($stream+'Bytes')){throw 'Diagnostic stream byte count differs'}}
function Phases([string[]]$lines){
    $rows=[Collections.Generic.List[object]]::new();$active=-1;$last=-1L;$begin=0L
    foreach($line in $lines){
        if(!$line.StartsWith('LEA_SUITE_PHASE ',[StringComparison]::Ordinal)){continue}
        if($line -cnotmatch '^LEA_SUITE_PHASE ([0-9]+) ([0-9]+) (begin|end)$'){throw 'Malformed phase marker'}
        $ms=[long]$Matches[1];$id=[int]$Matches[2];$edge=$Matches[3]
        if($ms -lt $last){throw 'Phase clock moved backwards'};$last=$ms
        if($edge -ceq 'begin'){if($active -ne -1 -or $id -ne $rows.Count){throw 'Phase begin sequence differs'};$active=$id;$begin=$ms}
        else{if($active -ne $id){throw 'Phase end sequence differs'};$rows.Add(@{phase=$id;elapsedMs=$ms-$begin});$active=-1}
    }
    return @{completed=$rows;active=$active}
}
$negative=0
foreach($control in @(@('LEA_SUITE_PHASE 0 1 begin'),@('LEA_SUITE_PHASE 10 0 begin','LEA_SUITE_PHASE 9 0 end'),@('LEA_SUITE_PHASE 0 0 end'))){try{$null=Phases $control}catch{$negative++}}
if($negative -ne 3){throw 'Phase parser negative controls failed'}
$current=Phases @(Get-Content -LiteralPath $process.stderr)
if($current.completed.Count -ne 86 -or $current.active -ne 86){throw 'Unexpected diagnostic completion boundary'}
$baselinePath=Join-Path $root 'continuity-evidence/temporal-startup-v1/relative-font-script-owned-run-v1/result.json'
if((Get-FileHash $baselinePath).Hash -cne '7F1448D51CF16421602D7D02DDC58F066BDAF5DB9E22A8E8B3728C525B5BA205'){throw 'Baseline receipt differs'}
$baseline=Get-Content $baselinePath -Raw|ConvertFrom-Json
if(!$baseline.passed -or $baseline.runs.Count -ne 1){throw 'Baseline outcome incomplete'}
$previous=Phases @(Get-Content -LiteralPath $baseline.runs[0].stderr)
if($previous.completed.Count -ne 89 -or $previous.active -ne -1){throw 'Baseline phases incomplete'}
$differences=@(foreach($row in $current.completed){$old=$previous.completed[$row.phase];@{phase=$row.phase;baselineMs=$old.elapsedMs;diagnosticMs=$row.elapsedMs;deltaMs=$row.elapsedMs-$old.elapsedMs}})
$acl=[Collections.Generic.List[object]]::new()
foreach($file in $run.before){
    if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'Runtime bytes changed'}
    $sddl=(Get-Acl -LiteralPath $file.path).Sddl
    if($sddl -cne $file.sddl){$acl.Add(@{path=$file.path;before=$file.sddl;after=$sddl;bytesUnchanged=$true})}
}
if($acl.Count -ne 7 -or @($acl|Where-Object {$_.path -notin $run.changedFiles}).Count){throw 'ACL delta boundary differs'}
@{verified=$true;runPassed=$false;diagnosticOnly=$true;runReceiptSha256=(Get-FileHash $receipt).Hash;deadlinePreserved=$true;ownedJobRetired=$true;runtimeBytesUnchanged=$true;aclRestored=$false;aclDeltas=$acl;candidateQuarantined=$true;phaseParserNegativeControls=3;completedPhases=86;activePhase=86;phaseDifferences=$differences;slowestPositiveDeltas=@($differences|Sort-Object deltaMs -Descending|Select-Object -First 12);observedPhase50Completed=$true;observedPhase57Completed=$true;originalIntermittentFailureResolved=$false;comparisonScope='Observed wall-time differences across distinct candidates/runs; not exclusive CPU cost, root cause or permission repair';streams=@(@{path=$process.stdout;sha256=(Get-FileHash $process.stdout).Hash},@{path=$process.stderr;sha256=(Get-FileHash $process.stderr).Hash});runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 8|Set-Content $out -Encoding utf8
Get-FileHash $out
