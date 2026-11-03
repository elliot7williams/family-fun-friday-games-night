function New-SdkPooledIo {
    param([Parameter(Mandatory)][string]$Text)
    $constructor=@'
            internal IoQueue()
            {
                var thread = new Thread(Consume) { IsBackground = true, Name = "Lea script I/O" };
                try { thread.Start(); }
                catch { actions.Dispose(); throw; }
            }

'@
    $enqueue=@'
                    })) throw new InvalidOperationException("A worker I/O operation is already queued.");
'@
    $scheduled=@'
                    })) throw new InvalidOperationException("A worker I/O operation is already queued.");
                    if (!scheduled)
                    {
                        scheduled = true;
                        try
                        {
                            if (!ThreadPool.QueueUserWorkItem(_ => Consume()))
                                throw new InvalidOperationException("Worker I/O scheduling failed.");
                        }
                        catch { scheduled = false; throw; }
                    }
'@
    $consume=@'
                try { foreach (var action in actions.GetConsumingEnumerable()) action(); }
                // Disposal cannot race the owner finishing CompleteAdding.
                finally { lock (gate) actions.Dispose(); }
'@
    $drain=@'
                while (true)
                {
                    Action action;
                    lock (gate)
                    {
                        if (!actions.TryTake(out action))
                        {
                            scheduled = false;
                            if (retired) actions.Dispose();
                            return;
                        }
                    }
                    action();
                }
'@
    $changes=@(
        @{old=$constructor;replacement=''},
        @{old='private bool retired;';replacement=('private bool retired;'+"`n"+'            private bool scheduled;')},
        @{old=$enqueue;replacement=$scheduled},
        @{old='actions.CompleteAdding();';replacement=('actions.CompleteAdding();'+"`n"+'                    if (!scheduled) actions.Dispose();')},
        @{old=$consume;replacement=$drain}
    )
    $constructor=$constructor.Replace("`r`n","`n").TrimEnd([char[]]"`n")+"`n`n"
    $changes[0].old=$constructor
    foreach($change in $changes){$change.old=$change.old.Replace("`r`n","`n");$change.replacement=$change.replacement.Replace("`r`n","`n")}
    function Project([string]$value){
        $normalized=$value.Replace("`r`n","`n")
        $offsets=[Collections.Generic.List[int]]::new()
        for($i=0;$i -lt $value.Length;$i++){$offsets.Add($i);if($value[$i] -eq "`r" -and $i+1 -lt $value.Length -and $value[$i+1] -eq "`n"){$i++}}
        $offsets.Add($value.Length)
        if($offsets.Count -ne $normalized.Length+1){throw 'Source offset map differs'}
        $edits=[Collections.Generic.List[object]]::new()
        foreach($change in $changes){
            $matches=[regex]::Matches($normalized,[regex]::Escape($change.old))
            if($matches.Count -ne 1 -or ($change.replacement -and $normalized.Contains($change.replacement))){throw 'Unique original request-I/O source shape required'}
            $start=$offsets[$matches[0].Index];$end=$offsets[$matches[0].Index+$change.old.Length]
            $lineEnd=$value.IndexOf("`n",$start);$nl=if($lineEnd -gt 0 -and $value[$lineEnd-1] -eq "`r"){"`r`n"}else{"`n"}
            $edits.Add(@{start=$start;old=$value.Substring($start,$end-$start);replacement=$change.replacement.Replace("`n",$nl)})
        }
        $ordered=@($edits|Sort-Object start -Descending);$limit=$value.Length
        $updated=$value
        foreach($edit in $ordered){if($edit.start+$edit.old.Length -gt $limit){throw 'Overlapping source edits refused'};$limit=$edit.start;$updated=$updated.Remove($edit.start,$edit.old.Length).Insert($edit.start,$edit.replacement)}
        $inverse=$updated
        for($i=$ordered.Count-1;$i -ge 0;$i--){$edit=$ordered[$i];if($inverse.Substring($edit.start,$edit.replacement.Length) -cne $edit.replacement){throw 'Source edit journal differs'};$inverse=$inverse.Remove($edit.start,$edit.replacement.Length).Insert($edit.start,$edit.old)}
        if($inverse -cne $value){throw 'Pooled I/O source inverse differs'}
        $completion='finally { if (timer) bridge.Timers.End(); if (render) bridge.Frames.End(); StopIo(); running = false; }'
        if([regex]::Matches($updated,[regex]::Escape($completion)).Count -ne 1){throw 'Original completion retirement contract changed'}
        return $updated
    }
    $candidate=Project $Text;$negative=0
    foreach($invalid in @($Text.Replace('internal IoQueue()','internal ChangedIoQueue()'),($Text+$Text),$candidate)){try{$null=Project $invalid}catch{$negative++}}
    if($negative -ne 3){throw 'Pooled I/O negative controls failed'}
    return @{parent=$candidate;inverseVerified=$true;negativeControls=3;edits=5;completionRetirementUnchanged=$true;budgetsChanged=$false;nativeGuardsChanged=$false;publicApiChanged=$false;runtimeQualified=$false;threadPoolSettingsChanged=$false}
}
