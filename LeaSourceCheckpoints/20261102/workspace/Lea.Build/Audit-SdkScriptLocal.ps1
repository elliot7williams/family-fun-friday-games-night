param([switch]$TimelineExport)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$directory=Join-Path $root 'continuity-evidence/sdk-script-local-timeline-v1'
$receipt=Join-Path $directory 'result.json';$pin='3091409C2C001831DB0A93DD3BC889377552C13CCF7EFB15B7B0F02E800175E6'
if($TimelineExport){$directory=Join-Path $root 'continuity-evidence/sdk-script-local-timeline-export-v1';$receipt=Join-Path $directory 'result.json';$pin='061087588C11B4DC1957F6CF389B005D9041AF5C93338322B6D991149BA17C5F'}
if((Get-FileHash $receipt).Hash -cne $pin){throw 'Local script receipt differs'}
$r=Get-Content $receipt -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.diagnosticOnly -or !$r.hostBindingConfiguration -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.error -or $r.runs.Count -ne 2){throw 'Diagnostic local script gate incomplete'}
$budgets=@(120000,30000)
for($i=0;$i -lt 2;$i++){
    $run=$r.runs[$i]
    if(!$run.passed -or $run.exitCode -ne 0 -or !$run.exitObserved -or $run.timedOut -or $run.killRequested -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or $run.parentBudgetMs -ne $budgets[$i] -or $run.elapsedMs -gt $budgets[$i] -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Local script lifecycle differs'}
}
$lines=@(Get-Content $r.runs[1].stdout)
if($lines[-1] -cne 'PASS SDK LOCAL SCRIPT 21' -or @($lines|Where-Object {$_ -like 'PASS *'}).Count -ne 22){throw 'Local script terminal coverage differs'}
$timeline=[Collections.Generic.List[object]]::new()
if($TimelineExport){
    $trace=@($lines|Where-Object {$_ -like 'BOOTSTRAP_TIMELINE *'})
    if($trace.Count -ne 1 -or $trace[0].Length -gt 400){throw 'Bounded exported timeline missing'}
    $stages=@('options','vm','bridge','prepared','declarations','scope','events','interfaces','elements','document','bridges-hidden','dispatch-ready')
    $parts=@($trace[0].Substring('BOOTSTRAP_TIMELINE '.Length).Split(';',[StringSplitOptions]::RemoveEmptyEntries))
    if($parts.Count -ne $stages.Count){throw 'Timeline stage coverage differs'}
    $previous=0L
    for($i=0;$i -lt $parts.Count;$i++){
        if($parts[$i] -notmatch '^([a-z-]+)=([0-9]+)$' -or $Matches[1] -cne $stages[$i]){throw 'Timeline stage order differs'}
        $value=[long]$Matches[2];if($value -lt $previous -or $value -gt 30000){throw 'Timeline not monotonic/bounded'}
        $timeline.Add(@{stage=$stages[$i];elapsedMs=$value;deltaMs=($value-$previous)});$previous=$value
    }
}
foreach($file in $r.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Local script input changed'}}
if(@(Get-ChildItem $directory -Recurse -File|Where-Object {$_.Name -like 'Lea.Script.Worker*'}).Count){throw 'Unexpected isolated worker in local diagnostic'}
$out=Join-Path $directory 'local-audit.json';if(Test-Path $out){throw 'Preserve local diagnostic audit'}
@{verified=$true;receiptSha256=$pin;checks=21;timeline=$timeline;diagnosticOnly=$true;startupCauseProven=$false;timestampsExported=[bool]$TimelineExport;unmodifiedCandidateQualified=$false;workerQualified=$false;completeSdk=$false;runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 5|Set-Content $out -Encoding utf8
Get-FileHash $out
