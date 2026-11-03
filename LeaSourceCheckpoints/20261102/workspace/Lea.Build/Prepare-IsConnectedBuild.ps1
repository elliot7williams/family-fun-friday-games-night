$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Build-WorkerRetirementTrace.ps1'
if((Get-FileHash $from).Hash -cne 'F0646DAC7763F953C683FAAEE6B87C788B002CC853F05CB274D61CBBEE959F02'){throw 'Scoped script build template differs'}
$to=Join-Path $PSScriptRoot 'Build-IsConnected-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve connectivity builder'}
$original=[IO.File]::ReadAllText($from)
$pairs=@(
    @('worker-retirement-trace-build-20261102-v2','is-connected-script-build-20261102-v1'),
    @('worker-retirement-trace-source-20261102-v2','is-connected-source-20261102-v1'),
    @('336EB5EBDD9061182D7EC89B69A1C2C72BC2FC63AAA70374C568FD0A76614D3A','B4BEA8392042E88DE4D833AA158B35869811A95AC9D5D8CA5AF8D02B53FA359E'),
    @('diagnosticOnly=$true','diagnosticOnly=$false'),
    @("-Name 'compile-retirement-trace'","-Name 'compile-is-connected'")
)
$text=$original
foreach($pair in $pairs){if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique connectivity build projection required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Connectivity builder inverse differs'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
