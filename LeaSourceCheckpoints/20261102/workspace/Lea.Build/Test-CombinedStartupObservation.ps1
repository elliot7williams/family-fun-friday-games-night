param([ValidateRange(1,9)][int]$Version=2)
$ErrorActionPreference='Stop'
$out='I:\LeaBuildStorage\combined-startup-reader-controls-20261101-v'+$Version
if(Test-Path -LiteralPath $out){throw 'Preserve reader controls'}
$reader=Join-Path $PSScriptRoot 'Read-CombinedStartupObservation.ps1'
. $reader
$prior='I:\LeaBuildStorage\combined-worker-startup-20261101-v2/result.json'
if((Get-FileHash $prior).Hash -cne 'BCE712793B3CCC1EC7EA1FC0F8569491F0D00689B59632C9FA2383F894925D30'){throw 'Known capture differs'}
$report=(Get-Content $prior -Raw|ConvertFrom-Json).observation
function Line($value){return 'COMBINED_WORKER_STARTUP '+($value|ConvertTo-Json -Depth 16 -Compress)}
$good=Line $report
$parsed=Read-CombinedStartupObservation -Lines @($good)
if(!$parsed.verified -or $parsed.samples -ne 19 -or !$parsed.scriptPassed -or $parsed.mutations -ne 6 -or $parsed.title -cne 'Lea 7'){throw 'Known capture not accepted'}
$mixed=Read-CombinedStartupObservation -Lines @('', 'PASS unrelated assertion', $good, '')
if(!$mixed.verified -or $mixed.samples -ne 19){throw 'Ordinary blank log lines not accepted'}
$rejected=0
foreach($kind in 1..8){
    $bad=$report|ConvertTo-Json -Depth 16|ConvertFrom-Json
    switch($kind){1{$lines=@()}2{$lines=@($good,$good)}3{$bad.valid=$false}4{$bad.samples=@()}5{$bad.samples[-1].threads.ProcessCreated++}6{$bad.samples[0].threads.OriginalProcessHandleVerified=$false}7{$bad.samples[-1].wallMs=-1}8{$bad.startupBudgetMs=5001}}
    if($kind -gt 2){$lines=@(Line $bad)}
    $denied=$false;try{$null=Read-CombinedStartupObservation -Lines $lines}catch{$denied=$true}
    if(!$denied){throw ('Reader negative accepted: '+$kind)};$rejected++
}
New-Item -ItemType Directory -Path $out|Out-Null
@{passed=$true;positiveControls=2;negativeControls=$rejected;interpreterExecuted=$false;readerSha256=(Get-FileHash $reader).Hash;captureSha256=(Get-FileHash $prior).Hash;runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json|Set-Content (Join-Path $out 'result.json') -Encoding utf8
Get-FileHash (Join-Path $out 'result.json')
