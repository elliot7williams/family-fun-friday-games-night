function Read-TrustedBootstrapProfile([string[]]$Lines,[switch]$Detailed,[switch]$Failed){
    $labels=@('enter','vm-begin','vm-end','prepare-begin','prepare-end','evaluate-begin','stage-declarations','stage-scope','stage-events','stage-interfaces','stage-elements','stage-document','evaluate-end','complete','end')
    $detailLabels=@('microtask','timers','frames','state','handler-begin','handler-end')
    if($Detailed){$labels=@($labels[0..7])+@($detailLabels|ForEach-Object {'stage-profile-'+$_})+@($labels[8..14])}
    if($Failed){
        $captured=@($Lines|Where-Object {$_.StartsWith('LEA_TRUSTED_BOOT ')})
        $prefixCount=$captured.Count-2
        if($prefixCount -lt 6 -or $prefixCount -gt $labels.Count-3){throw 'Invalid failed bootstrap prefix'}
        $labels=@($labels[0..($prefixCount-1)])+@('evaluate-end','end')
    }
    $rows=[Collections.Generic.List[object]]::new();$wallFrequency=0L;$cpuFrequency=0L
    foreach($line in $Lines){
        if(!$line.StartsWith('LEA_TRUSTED_BOOT')){continue}
        if($line -cnotmatch '^LEA_TRUSTED_BOOT wallFrequency=([0-9]+) cpuFrequency=([0-9]+) ([a-z-]+) wall=([0-9]+) cpu=([0-9]+) gc0=([0-9]+) gc1=([0-9]+) gc2=([0-9]+)$'){throw 'Malformed trusted profile marker'}
        $wf=[long]::Parse($Matches[1]);$cf=[long]::Parse($Matches[2]);$label=$Matches[3]
        $row=@{label=$label;wall=[long]::Parse($Matches[4]);cpu=[long]::Parse($Matches[5]);gc0=[long]::Parse($Matches[6]);gc1=[long]::Parse($Matches[7]);gc2=[long]::Parse($Matches[8])}
        if($rows.Count -ge $labels.Count -or $label -cne $labels[$rows.Count] -or $wf -le 0 -or $cf -ne [TimeSpan]::TicksPerSecond){throw 'Trusted profile sequence/frequency differs'}
        if($rows.Count){
            if($wf -ne $wallFrequency -or $cf -ne $cpuFrequency){throw 'Trusted profile frequencies changed'}
            $previous=$rows[$rows.Count-1];foreach($key in @('wall','cpu','gc0','gc1','gc2')){if($row[$key] -lt $previous[$key]){throw 'Nonmonotonic trusted profile counter'}}
        }else{$wallFrequency=$wf;$cpuFrequency=$cf}
        $rows.Add($row)
    }
    if($rows.Count -ne $labels.Count){throw 'Complete successful bootstrap trace required'}
    $byLabel=@{};foreach($row in $rows){$byLabel.Add($row.label,$row)}
    $intervals=[Collections.Generic.List[object]]::new()
    $phases=@(@{name='vm';begin='vm-begin';end='vm-end'},@{name='bridge';begin='vm-end';end='prepare-begin'},@{name='prepare';begin='prepare-begin';end='prepare-end'},@{name='evaluate';begin='evaluate-begin';end='evaluate-end'},@{name='scope-to-events';begin='stage-scope';end='stage-events'})
    if($Detailed){
        $boundaries=@('stage-scope')+@($detailLabels|ForEach-Object {'stage-profile-'+$_})+@('stage-events')
        for($i=0;$i -lt $boundaries.Count-1;$i++){$phases+=@{name=($boundaries[$i]+'-to-'+$boundaries[$i+1]);begin=$boundaries[$i];end=$boundaries[$i+1]}}
    }
    foreach($phase in $phases){
        if(!$byLabel.ContainsKey($phase.begin) -or !$byLabel.ContainsKey($phase.end)){continue}
        $a=$byLabel[$phase.begin];$b=$byLabel[$phase.end]
        $intervals.Add(@{phase=$phase.name;wallMs=1000.0*($b.wall-$a.wall)/$wallFrequency;processCpuMs=1000.0*($b.cpu-$a.cpu)/$cpuFrequency;gc0=$b.gc0-$a.gc0;gc1=$b.gc1-$a.gc1;gc2=$b.gc2-$a.gc2})
    }
    return @{verified=$true;bootstrapSucceeded=(!$Failed);markers=$rows.Count;intervals=$intervals;rows=$rows;scope='Single diagnostic process measurements; scope-to-events is nested in evaluate, do not sum all intervals. Wall/CPU gap is not causal attribution.'}
}
