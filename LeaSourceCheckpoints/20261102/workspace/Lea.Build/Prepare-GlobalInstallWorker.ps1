$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Build-IsConnectedWorker.ps1'
if((Get-FileHash $from).Hash -cne 'CB92D89D7CA9D12F557DAECCBBE657FCA818407398BE2C007E4CD4CBBE37A9DE'){throw 'Worker template differs'}
$to=Join-Path $PSScriptRoot 'Build-GlobalInstallWorker-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve matching worker builder'}
$original=[IO.File]::ReadAllText($from)
$pairs=@(
    @('is-connected-worker-build-20261102-v1','global-install-worker-build-20261102-v1'),
    @('is-connected-source-20261102-v1','global-install-source-20261102-v1'),
    @('B4BEA8392042E88DE4D833AA158B35869811A95AC9D5D8CA5AF8D02B53FA359E','8752B10BDD88F4AAFFFB016AFDC4EB0D49B4A56D054AA4738D461BAD415A46B5'),
    @('$resource[0].originalSha256','$resource[0].sdkOriginalSha256'),
    @('!$f.inverseVerified -or $resource.Count -ne 1','!$f.inverseVerified -or $resource.Count -ne 1 -or !$f.candidateOnly -or $f.budgetsChanged -or $f.negativeControls -ne 6 -or $f.globalDescriptorCallsRemoved -ne 1'),
    @('Matching isConnected worker IL','Matching global-install/isConnected worker IL'),
    @("-Name 'build-connectivity-worker-il'","-Name 'build-global-install-worker-il'"),
    @("-Name 'build-connectivity-worker-native'","-Name 'build-global-install-worker-native'")
)
$text=$original
foreach($pair in $pairs){if($pair.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique worker projection boundary required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Worker builder inverse differs'}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Worker builder syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
