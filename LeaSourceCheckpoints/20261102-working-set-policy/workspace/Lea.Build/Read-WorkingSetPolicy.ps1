function Read-WorkingSetPolicy($Before,$After,[uint]$ProcessId){
    if(!$ProcessId){throw 'Expected owned process identity required'}
    foreach($sample in @($Before,$After)){
        foreach($name in @('pid','minimumBytes','maximumBytes','flags','hardMinimum','hardMaximum')){if(!$sample.PSObject.Properties[$name]){throw 'Incomplete working-set sample'}}
        if($sample.pid -ne $ProcessId -or $sample.minimumBytes -le 0 -or $sample.maximumBytes -lt $sample.minimumBytes -or $sample.flags -lt 0 -or $sample.flags -gt [uint]::MaxValue){throw 'Invalid policy identity or bounds'}
        if($sample.hardMinimum -isnot [bool] -or $sample.hardMaximum -isnot [bool]){throw 'Boolean policy flags required'}
        $flags=[uint]$sample.flags
        if(($flags -band 3) -eq 3 -or ($flags -band 12) -eq 12){throw 'Contradictory policy flags'}
        if($sample.hardMinimum -ne (($flags -band 1) -ne 0) -or $sample.hardMaximum -ne (($flags -band 4) -ne 0)){throw 'Decoded hard-limit flags differ'}
    }
    return @{verified=$true;processId=$ProcessId;before=$Before;after=$After;unchanged=($Before.minimumBytes -eq $After.minimumBytes -and $Before.maximumBytes -eq $After.maximumBytes -and $Before.flags -eq $After.flags);hardMaximumObserved=($Before.hardMaximum -or $After.hardMaximum);scope='Two read-only samples of this owned process only; not outer job policy, continuous observation or proof of timeout causality.'}
}
