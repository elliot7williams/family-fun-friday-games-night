$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Build-GlobalInstall-v1.generated.ps1'
if((Get-FileHash $from).Hash -cne '69E3AE6A4C6D37D792B8B97731F5CC29DD7B151D0A3D26FACDA1B6701F4C6C78'){throw 'Scoped parent build template differs'}
$to=Join-Path $PSScriptRoot 'Build-BootstrapFunction-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve bootstrap function builder'}
$original=[IO.File]::ReadAllText($from)
$pairs=@(
    @('global-install-build-20261102-v1','bootstrap-function-build-20261102-v1'),
    @('global-install-source-20261102-v1','bootstrap-function-source-20261102-v1'),
    @('8752B10BDD88F4AAFFFB016AFDC4EB0D49B4A56D054AA4738D461BAD415A46B5','8BE6CCF6D976ED680FE2D5507D440FAB663B712B30B0EFE8A608F170B47BBA22'),
    @('!$t.inverseVerified -or $t.negativeControls -ne 6','!$t.bodyInverseVerified -or $t.negativeControls -ne 10'),
    @('$t.budgetsChanged -or $t.globalDescriptorCallsRemoved -ne 1','!$t.singleEvaluationRequired -or $t.hostPolicyChanged'),
    @("-Name 'compile-global-install'","-Name 'compile-bootstrap-function'")
)
$text=$original
foreach($pair in $pairs){if($pair.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique function build projection required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Bootstrap function builder inverse differs'}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Bootstrap function builder syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
