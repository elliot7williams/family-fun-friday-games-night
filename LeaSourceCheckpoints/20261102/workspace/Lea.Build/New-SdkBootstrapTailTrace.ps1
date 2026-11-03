function New-SdkBootstrapTailTrace([string]$Bootstrap,[string]$Markers){
    $before="delete globalThis.__lea;delete globalThis.__jobs;(function dispatch("
    $after="delete globalThis.__lea;delete globalThis.__jobs;checkpoint('bridges-hidden');const leaDispatchResult=(function dispatch("
    $tail='return canceled})}'
    $replacement="return canceled});checkpoint('dispatch-ready');leaDispatchResult}"
    $oldStages='"elements", "document"'
    $newStages='"elements", "document", "bridges-hidden", "dispatch-ready"'
    foreach($pair in @(@($Bootstrap,$before),@($Bootstrap,$tail),@($Markers,$oldStages))){
        if([regex]::Matches($pair[0],[regex]::Escape($pair[1])).Count -ne 1){throw 'Unique original bootstrap tail shape required'}
    }
    if($Bootstrap.Contains('leaDispatchResult') -or $Bootstrap.Contains("checkpoint('bridges-hidden')") -or $Bootstrap.Contains("checkpoint('dispatch-ready')") -or $Markers.Contains('"dispatch-ready"')){throw 'Already traced bootstrap refused'}
    $script=$Bootstrap.Replace($before,$after).Replace($tail,$replacement)
    $stages=$Markers.Replace($oldStages,$newStages)
    if($script.Replace($after,$before).Replace($replacement,$tail) -cne $Bootstrap -or $stages.Replace($newStages,$oldStages) -cne $Markers){throw 'Tail trace inverse differs'}
    return @{bootstrap=$script;markers=$stages;inverseVerified=$true;addedCheckpoints=2;diagnosticOnly=$true;budgetMs=1000}
}
