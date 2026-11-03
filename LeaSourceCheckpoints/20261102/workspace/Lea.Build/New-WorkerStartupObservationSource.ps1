function New-WorkerStartupObservationSource {
    param([Parameter(Mandatory)][string]$Original)
    $anchors=@('                W.On(view, null, 5000);','                var page = await view.GoA(fixture);')
    $replacements=@(($anchors[0]+[Environment]::NewLine+'                using var observation = new WorkerStartupObserver(view);'),($anchors[1]+[Environment]::NewLine+'                observation.Stop();'))
    $updated=$Original
    if($Original.Contains('WorkerStartupObserver')){throw 'Startup observation already applied'}
    for($i=0;$i -lt $anchors.Count;$i++){
        if([regex]::Matches($updated,[regex]::Escape($anchors[$i])).Count -ne 1){throw 'Startup observation anchor differs'}
        $updated=$updated.Replace($anchors[$i],$replacements[$i])
    }
    $inverse=$updated
    for($i=$anchors.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($replacements[$i],$anchors[$i])}
    if($inverse -cne $Original){throw 'Startup observation inverse differs'}
    @{text=$updated;inverseVerified=$true;originalAssertionsPreserved=$true;scope='First native-removal startup observation only; original await, factory configuration, assertions and deadline retained'}
}
