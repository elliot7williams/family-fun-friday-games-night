function Read-BootstrapResources([string[]]$Lines,[string[]]$Labels,[long]$WallFrequency){
    if($WallFrequency -le 0 -or $Labels.Count -lt 2){throw 'Frequency and expected labels required'}
    $rows=[Collections.Generic.List[object]]::new()
    $pattern='^LEA_BOOT_RESOURCE ([a-z-]+) wall=([0-9]+) thread=([0-9]+) process=([0-9]+) faults=([0-9]+) read=([0-9]+) write=([0-9]+) other=([0-9]+) cycles=([0-9]+) working=([0-9]+) commit=([0-9]+) tid=([0-9]+)$'
    foreach($line in $Lines){
        if(!$line.StartsWith('LEA_BOOT_RESOURCE')){continue}
        if($line -cnotmatch $pattern){throw 'Malformed resource marker'}
        $row=@{label=$Matches[1]}
        $keys=@('wall','thread','process','faults','read','write','other','cycles','working','commit','tid')
        for($i=0;$i -lt $keys.Count;$i++){$row[$keys[$i]]=[ulong]::Parse($Matches[$i+2])}
        if($rows.Count -ge $Labels.Count -or $row.label -cne $Labels[$rows.Count] -or $row.tid -eq 0 -or $row.process -lt $row.thread){throw 'Resource sequence or CPU identity invalid'}
        if($rows.Count){
            $previous=$rows[$rows.Count-1]
            if($row.tid -ne $previous.tid){throw 'Bootstrap thread changed'}
            foreach($key in @('wall','thread','process','faults','read','write','other','cycles')){if($row[$key] -lt $previous[$key]){throw 'Nonmonotonic resource counter'}}
        }
        $rows.Add($row)
    }
    if($rows.Count -ne $Labels.Count){throw 'Resource trace incomplete'}
    $intervals=[Collections.Generic.List[object]]::new()
    $pairs=[Collections.Generic.List[object]]::new()
    for($i=1;$i -lt $rows.Count;$i++){$pairs.Add(@{a=$rows[$i-1];b=$rows[$i];kind='adjacent'})}
    foreach($phase in @('prepare','evaluate')){
        $a=@($rows|Where-Object label -CEQ ($phase+'-begin'));$b=@($rows|Where-Object label -CEQ ($phase+'-end'))
        if($a.Count -eq 1 -and $b.Count -eq 1){$pairs.Add(@{a=$a[0];b=$b[0];kind='aggregate'})}
    }
    foreach($pair in $pairs){
        $a=$pair.a;$b=$pair.b
        $intervals.Add(@{begin=$a.label;end=$b.label;kind=$pair.kind;wallMs=1000.0*($b.wall-$a.wall)/$WallFrequency;threadCpuMs=($b.thread-$a.thread)/10000.0;processCpuMs=($b.process-$a.process)/10000.0;pageFaults=$b.faults-$a.faults;readBytes=$b.read-$a.read;writeBytes=$b.write-$a.write;otherBytes=$b.other-$a.other;threadCycles=$b.cycles-$a.cycles;workingDelta=[decimal]$b.working-[decimal]$a.working;commitDelta=[decimal]$b.commit-[decimal]$a.commit})
    }
    return @{verified=$true;markers=$rows.Count;threadId=$rows[0].tid;intervals=$intervals;rows=$rows;scope='Sequential observer samples. Page faults include soft/hard; process I/O is not proof of paging/disk waits. Cycles are not converted to time. Aggregates overlap adjacent intervals.'}
}
