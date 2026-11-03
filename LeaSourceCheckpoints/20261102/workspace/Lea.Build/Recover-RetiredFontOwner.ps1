$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'continuity-evidence/repro-sdk-font-relative-render-v2/owner-recovery.json'
if(Test-Path $out){throw 'Preserve recovery evidence'}
if($global:LeaCompositeProcessLeases.Count -ne 1){throw 'Exactly one original retained owner required'}
$lease=$global:LeaCompositeProcessLeases[0]
if($lease.record.name -cne 'compile-font-tests' -or $lease.record.pid -ne 16100 -or !$lease.record.retainedForOwner -or !$lease.record.exitObserved -or !$lease.record.outputDrained -or $lease.record.activeAfterRetirement -ne 0){throw 'Unexpected retained owner'}
$clock=[Diagnostics.Stopwatch]::StartNew()
$result=@{recovered=$false;originalPid=16100;originalFailurePreserved=$true;originalAttemptPassed=$false;childrenKilled=$false;deadlineMs=5000}
try{
    if(!$lease.process.WaitForExit(0) -or $lease.process.ActiveProcesses -ne 0){throw 'Original process/job still live'}
    foreach($reader in $lease.readers){
        if(!$reader.done -or $reader.disabled -or $reader.overflow){throw 'Original output not completely drained'}
        if($reader.file.CanWrite){$reader.file.Flush($true)}
        $reader.file.Dispose()
    }
    if($clock.ElapsedMilliseconds -ge 5000){throw 'Recovery deadline exceeded before owner disposal'}
    $lease.process.Dispose()
    foreach($stream in $global:LeaFontInputLeases){$stream.Dispose()}
    if($clock.ElapsedMilliseconds -ge 5000){throw 'Recovery deadline exceeded'}
    $global:LeaCompositeProcessLeases.Clear()
    Remove-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction Stop
    $result.recovered=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.elapsedMs=$clock.ElapsedMilliseconds
    $result|ConvertTo-Json|Set-Content -LiteralPath $out -Encoding utf8
    $result|ConvertTo-Json
}
