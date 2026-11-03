function Read-RetirementTrace([string[]]$Lines,[long]$Frequency){
    if($Frequency -le 0){throw 'Positive timestamp frequency required'}
    $allowed=@('reset-io','reset-endpoint','reset-leases','retire-job','retire-input','retire-output','retire-stderr','retire-permissions')
    $pending=@{};$spans=[Collections.Generic.List[object]]::new();$previous=-1L;$count=0
    foreach($line in $Lines){
        if(!$line.StartsWith('LEA_RETIREMENT_TRACE')){continue}
        if($line -cnotmatch '^LEA_RETIREMENT_TRACE ([a-z-]+) (begin|end) ([0-9]+)$'){throw 'Malformed retirement marker'}
        $label=$Matches[1];$phase=$Matches[2];$tick=[long]::Parse($Matches[3])
        if($label -cnotin $allowed -or $tick -lt $previous){throw 'Unknown or nonmonotonic retirement marker'}
        $previous=$tick;$count++
        if($phase -ceq 'begin'){
            if($pending.ContainsKey($label)){throw 'Overlapping retirement phase'}
            $pending.Add($label,$tick)
        }else{
            if(!$pending.ContainsKey($label)){throw 'Retirement end without begin'}
            $spans.Add(@{phase=$label;milliseconds=1000.0*($tick-$pending[$label])/$Frequency})
            $pending.Remove($label)
        }
    }
    if($pending.Count -or !$spans.Count){throw 'Incomplete retirement trace'}
    return @{verified=$true;markers=$count;spans=$spans;frequency=$Frequency;scope='Observed diagnostic call durations only; nested phases overlap, do not sum; stderr instrumentation can perturb timing'}
}
