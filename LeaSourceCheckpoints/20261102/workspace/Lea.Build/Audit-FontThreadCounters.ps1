$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$directory=Join-Path $root 'continuity-evidence/temporal-startup-v1/font-thread-counters-v1'
$path=Join-Path $directory 'result.json';$output=Join-Path $directory 'thread-audit.json'
if(Test-Path $output){throw 'Preserve thread audit'}
if((Get-FileHash $path).Hash -cne 'BDA2E33D337CB78BCF8A086C3F5A8485142B05B1678F5EDEC966292A718AA112'){throw 'Thread receipt differs'}
$result=Get-Content $path -Raw|ConvertFrom-Json
if(!$result.diagnosticCaptured -or !$result.prefixVerified -or !$result.computedPassed -or $result.fullSuitePassed -or !$result.inputsUnchanged -or $result.ownerAttention -or $result.changedFiles.Count -or $result.sampleErrors.Count -or $result.sampleOverflow){throw 'Thread diagnostic scope differs'}
if($result.runs.Count -ne 1){throw 'Expected one observed original process'}
$process=$result.runs[0];$birth=([datetime]$process.birthUtc).ToFileTimeUtc()
if($process.exitCode -ne 0 -or $process.timedOut -or $process.killRequested -or $process.activeAfterRetirement -ne 0 -or !$process.outputDrained){throw 'Thread diagnostic retirement incomplete'}
$samples=@($result.samples)
if($samples.Count -lt 3){throw 'Thread samples insufficient'}
foreach($sample in $samples){
    if(!$sample.snapshot.OriginalProcessHandleVerified -or $sample.snapshot.ProcessId -ne $process.pid -or $sample.snapshot.ProcessCreated -ne $birth -or !$sample.snapshot.Threads.Count){throw 'Thread process identity mismatch'}
}
$text=[IO.File]::ReadAllText((Join-Path $directory 'thread-prefix.stderr.txt'))
$markers=@([regex]::Matches($text,'(?m)^LEA_SUITE_PHASE \d+ 8 begin\r?$'))
if($markers.Count -ne 1){throw 'Computed marker missing/duplicated'}
$newline=$text.IndexOf("`n",$markers[0].Index)
if($newline -lt 0){throw 'Marker framing differs'}
$bytes=[Text.Encoding]::UTF8.GetByteCount($text.Substring(0,$newline+1))
$earliest=@($samples[0].snapshot.Threads|Sort-Object Created,Id)[0]
$window=@(foreach($sample in $samples|Where-Object stderrBytesObserved -ge $bytes){
    $threads=@($sample.snapshot.Threads|Where-Object {$_.Id -eq $earliest.Id -and $_.Created -eq $earliest.Created})
    if($threads.Count -ne 1){throw 'Earliest thread candidate disappeared or identity changed'}
    [pscustomobject]@{elapsedMs=$sample.elapsedMs;thread=$threads[0]}
})
if($window.Count -lt 2){throw 'Post-marker thread window insufficient'}
for($i=1;$i -lt $window.Count;$i++){
    if($window[$i].elapsedMs -le $window[$i-1].elapsedMs -or $window[$i].thread.CpuTicks -lt $window[$i-1].thread.CpuTicks -or $window[$i].thread.ContextSwitches -lt $window[$i-1].thread.ContextSwitches){throw 'Thread observation not monotonic'}
}
foreach($file in $result.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Thread input changed'}}
$first=$window[0];$last=$window[-1]
$audit=@{audited=$true;rawReceiptSha256=(Get-FileHash $path).Hash;runnerSha256=(Get-FileHash $PSCommandPath).Hash;samples=$samples.Count;markerByteOffset=$bytes;earliestThreadCandidateId=$earliest.Id;earliestThreadCreated=$earliest.Created;managedOwningThreadProven=$false;windowSamples=$window.Count;windowStartMs=$first.elapsedMs;windowEndMs=$last.elapsedMs;windowCpuMs=($last.thread.CpuTicks-$first.thread.CpuTicks)/10000;windowContextSwitches=$last.thread.ContextSwitches-$first.thread.ContextSwitches;stateCounts=@($window|Group-Object {$_.thread.State}|Select-Object Name,Count);waitingReasonCounts=@($window|Where-Object {$_.thread.State -eq 5}|Group-Object {$_.thread.WaitReason}|Select-Object Name,Count);maxQueryMs=($samples|Measure-Object queryMs -Maximum).Maximum;waitBetweenSamplesExcluded=$false;causeProven=$false;fullSuitePassed=$false;limitsChanged=$false}
[IO.File]::WriteAllText($output,($audit|ConvertTo-Json -Depth 6))
if(!(Get-Content $output -Raw|ConvertFrom-Json).audited){throw 'Thread audit readback differs'}
$audit|Select-Object audited,samples,windowSamples,windowCpuMs,windowContextSwitches,stateCounts,managedOwningThreadProven,causeProven|ConvertTo-Json -Depth 4
