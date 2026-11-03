$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Build-GlobalInstallWorker-v1.generated.ps1'
$to=Join-Path $PSScriptRoot 'Build-PrivateBindingWorker-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve matching private binding worker builder'}
if((Get-FileHash $from).Hash -cne 'CDE5588179A1649ECED1B804CF75534A3707C83FA4E1217C57BEE490087F5469'){throw 'Matching worker template changed'}
$original=[IO.File]::ReadAllText($from)
$pairs=@(
    @('global-install-worker-build-20261102-v1','private-binding-worker-build-20261102-v1'),
    @('global-install-source-20261102-v1','private-binding-source-20261102-v1'),
    @('8752B10BDD88F4AAFFFB016AFDC4EB0D49B4A56D054AA4738D461BAD415A46B5','9CD78B418ED5CE1216123B7093CD0CB3593EF0D7812414FF4D35D718CC266BA2'),
    @('$f.inverseVerified','$f.structuralAstVerified'),
    @('$f.budgetsChanged -or $f.negativeControls -ne 6 -or $f.globalDescriptorCallsRemoved -ne 1','$f.hostPolicyChanged -or !$f.deterministic -or $f.negativeControls -ne 4 -or $f.minifiedBytes -ne 21342'),
    @('Matching global-install/isConnected worker IL','Matching private-binding/isConnected worker IL'),
    @("-Name 'build-global-install-worker-il'","-Name 'build-private-binding-worker-il'"),
    @("-Name 'build-global-install-worker-native'","-Name 'build-private-binding-worker-native'")
)
$text=$original
foreach($pair in $pairs){if($pair.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique private worker projection boundary required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Private worker builder inverse differs'}
foreach($critical in @('-Seconds 120','600MB','8.0.33','Disable','Only matching DOM resource substitution required','Six-file worker closure required')){if([regex]::Matches($text,[regex]::Escape($critical)).Count -ne [regex]::Matches($original,[regex]::Escape($critical)).Count){throw 'Worker build safety operation changed'}}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Worker builder syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
