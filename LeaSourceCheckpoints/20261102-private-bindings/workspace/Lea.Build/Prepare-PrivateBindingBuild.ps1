$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Build-GlobalInstall-v1.generated.ps1'
$to=Join-Path $PSScriptRoot 'Build-PrivateBindings-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve private binding builder'}
if((Get-FileHash $from).Hash -cne '69E3AE6A4C6D37D792B8B97731F5CC29DD7B151D0A3D26FACDA1B6701F4C6C78'){throw 'Scoped parent build template differs'}
$original=[IO.File]::ReadAllText($from)
$pairs=@(
    @('global-install-build-20261102-v1','private-binding-build-20261102-v1'),
    @('global-install-source-20261102-v1','private-binding-source-20261102-v1'),
    @('8752B10BDD88F4AAFFFB016AFDC4EB0D49B4A56D054AA4738D461BAD415A46B5','9CD78B418ED5CE1216123B7093CD0CB3593EF0D7812414FF4D35D718CC266BA2'),
    @('$t.inverseVerified','$t.structuralAstVerified'),
    @('$t.negativeControls -ne 6','$t.negativeControls -ne 4'),
    @('$t.budgetsChanged -or $t.globalDescriptorCallsRemoved -ne 1','$t.hostPolicyChanged -or !$t.deterministic -or $t.originalBytes -ne 27635 -or $t.minifiedBytes -ne 21342'),
    @("-Name 'compile-global-install'","-Name 'compile-private-bindings'")
)
$text=$original
foreach($pair in $pairs){if($pair.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique private binding build boundary required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Private binding builder inverse differs'}
foreach($critical in @('600MB','-Seconds 120','OwnedTestProcess.cs','Invoke-OwnedTestRun.ps1')){if([regex]::Matches($text,[regex]::Escape($critical)).Count -ne [regex]::Matches($original,[regex]::Escape($critical)).Count){throw 'Build safety operation changed'}}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
