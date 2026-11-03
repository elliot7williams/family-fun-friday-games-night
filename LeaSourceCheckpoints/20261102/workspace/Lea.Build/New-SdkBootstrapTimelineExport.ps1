function New-SdkBootstrapTimelineExport([string]$Parent){
    $method='        private void Start()'
    $property="        internal string BootstrapTimeline { get; private set; }`n`n"+$method
    $begin='            var markers = new Bm();'
    $newBegin=$begin+"`n            try`n            {"
    $ends=@([regex]::Matches($Parent,'            startupStage = null;\r?\n        }'))
    if($ends.Count -ne 1 -or $Parent.Contains('internal string BootstrapTimeline')){throw 'Unique unexported Start boundary required'}
    $end=$ends[0].Value
    $newEnd="            startupStage = null;`n            }`n            finally { BootstrapTimeline = markers.Report; }`n        }"
    foreach($anchor in @($method,$begin)){if([regex]::Matches($Parent,[regex]::Escape($anchor)).Count -ne 1){throw 'Unique timeline export anchor required'}}
    $updated=$Parent.Replace($method,$property).Replace($begin,$newBegin).Replace($end,$newEnd)
    if($updated.Replace($property,$method).Replace($newBegin,$begin).Replace($newEnd,$end) -cne $Parent){throw 'Timeline export inverse differs'}
    return @{parent=$updated;inverseVerified=$true;diagnosticOnly=$true;budgetMs=1000}
}
