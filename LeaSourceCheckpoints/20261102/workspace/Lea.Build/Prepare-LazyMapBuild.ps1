$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Build-GlobalInstall-v1.generated.ps1'
if((Get-FileHash $from).Hash -cne '69E3AE6A4C6D37D792B8B97731F5CC29DD7B151D0A3D26FACDA1B6701F4C6C78'){throw 'Scoped parent build template differs'}
$to=Join-Path $PSScriptRoot 'Build-LazyMap-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve lazy map builder'}
$original=[IO.File]::ReadAllText($from)
$pairs=@(
    @('global-install-build-20261102-v1','lazy-map-build-20261102-v1'),
    @('global-install-source-20261102-v1','lazy-map-source-20261102-v1'),
    @('8752B10BDD88F4AAFFFB016AFDC4EB0D49B4A56D054AA4738D461BAD415A46B5','4E8524E487B5F6E59408097EE7C5A7950C7BCADFF272B45BD7847B64334B5803'),
    @('$t.negativeControls -ne 6','$t.negativeControls -ne 16'),
    @('$t.budgetsChanged -or $t.globalDescriptorCallsRemoved -ne 1','$t.hostPolicyChanged -or $t.privateMapsDeferred -ne 2 -or !$t.constructorCaptured'),
    @("-Name 'compile-global-install'","-Name 'compile-lazy-map'")
)
$text=$original
foreach($pair in $pairs){if($pair.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique lazy map build boundary required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Lazy map builder inverse differs'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
