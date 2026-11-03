$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Build-WorkerRetirementTrace.ps1'
if((Get-FileHash $from).Hash -cne 'F0646DAC7763F953C683FAAEE6B87C788B002CC853F05CB274D61CBBEE959F02'){throw 'Scoped build template differs'}
$to=Join-Path $PSScriptRoot 'Build-GlobalInstall-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve global installation builder'}
$original=[IO.File]::ReadAllText($from)
$pairs=@(
    @('worker-retirement-trace-build-20261102-v2','global-install-build-20261102-v1'),
    @('worker-retirement-trace-source-20261102-v2','global-install-source-20261102-v1'),
    @('336EB5EBDD9061182D7EC89B69A1C2C72BC2FC63AAA70374C568FD0A76614D3A','8752B10BDD88F4AAFFFB016AFDC4EB0D49B4A56D054AA4738D461BAD415A46B5'),
    @('diagnosticOnly=$true','diagnosticOnly=$false;candidateOnly=$true'),
    @('$t.files.Count -ne 2','$t.files.Count -ne 2 -or !$t.candidateOnly -or $t.budgetsChanged -or $t.globalDescriptorCallsRemoved -ne 1'),
    @('$overlay[0].originalSha256','$overlay[0].sdkOriginalSha256'),
    @("-Name 'compile-retirement-trace'","-Name 'compile-global-install'")
)
$text=$original
foreach($pair in $pairs){if($pair.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique build projection required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Global installation builder inverse differs'}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Builder syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
