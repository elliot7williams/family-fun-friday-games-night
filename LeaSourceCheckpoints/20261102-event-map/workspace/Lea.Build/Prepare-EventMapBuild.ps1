$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Build-LazyMap-v1.generated.ps1'
$to=Join-Path $PSScriptRoot 'Build-EventMap-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve event map builder'}
$original=[IO.File]::ReadAllText($from)
$pairs=@(
    @('lazy-map-build-20261102-v1','event-map-build-20261102-v1'),
    @('lazy-map-source-20261102-v1','event-map-source-20261102-v1'),
    @('4E8524E487B5F6E59408097EE7C5A7950C7BCADFF272B45BD7847B64334B5803','FE6451DEA2C77AF80A4D43020D5F6CEE8B7EFE47C24C32AB61ABF070F4CFC0F3'),
    @('$t.negativeControls -ne 16','$t.negativeControls -ne 12'),
    @('$t.hostPolicyChanged -or $t.privateMapsDeferred -ne 2 -or !$t.constructorCaptured','$t.hostPolicyChanged'),
    @("-Name 'compile-lazy-map'","-Name 'compile-event-map'")
)
$text=$original
foreach($pair in $pairs){if($pair.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique event map build boundary required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Event map builder inverse differs'}
foreach($critical in @('600MB','-Seconds 120','OwnedTestProcess.cs','Invoke-OwnedTestRun.ps1')){if([regex]::Matches($text,[regex]::Escape($critical)).Count -ne [regex]::Matches($original,[regex]::Escape($critical)).Count){throw 'Build safety operation changed'}}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
