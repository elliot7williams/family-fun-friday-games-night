function New-SdkSessionIoReuse {
    param([Parameter(Mandatory)][string]$Text)
    $nl=if($Text.Contains("`r`n")){"`r`n"}else{"`n"}
    $stop='if (worker != null && worker.J != null) { KeepPeak(worker.J.Peak); worker.J.Dispose(); }'
    $changes=@(
        @{old='var started = false;';replacement=('var started = false;'+$nl+'            var keepIo = false;')},
        @{old='if (status != "ok") Reset();';replacement='if (status != "ok") Reset(); else keepIo = true;'},
        @{old='finally { if (timer) bridge.Timers.End(); if (render) bridge.Frames.End(); StopIo(); running = false; }';replacement='finally { if (timer) bridge.Timers.End(); if (render) bridge.Frames.End(); if (!keepIo || stopped || worker == null) StopIo(); running = false; }'},
        @{old=$stop;replacement=($stop+$nl+'            var pending = Volatile.Read(ref io);'+$nl+'            if (pending != null) pending.Stop();')}
    )
    function Project([string]$value){
        foreach($change in $changes){
            if([regex]::Matches($value,[regex]::Escape($change.old)).Count -ne 1 -or $value.Contains($change.replacement)){throw 'Unique original session I/O source shape required'}
        }
        $updated=$value
        foreach($change in $changes){$updated=$updated.Replace($change.old,$change.replacement)}
        $inverse=$updated
        for($i=$changes.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($changes[$i].replacement,$changes[$i].old)}
        if($inverse -cne $value){throw 'Session I/O source inverse differs'}
        return $updated
    }
    $candidate=Project $Text;$negative=0
    foreach($invalid in @($Text.Replace($changes[0].old,''),($Text+$Text),$candidate)){try{$null=Project $invalid}catch{$negative++}}
    if($negative -ne 3){throw 'Session I/O negative controls failed'}
    return @{parent=$candidate;inverseVerified=$true;negativeControls=3;edits=4;budgetsChanged=$false;nativeGuardsChanged=$false;publicApiChanged=$false;runtimeQualified=$false}
}
