$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Build-TrustedBootstrapProfile-v3.generated.ps1'
$to=Join-Path $PSScriptRoot 'Build-TrustedBootstrapProfile-v4.generated.ps1'
if(Test-Path $to){throw 'Preserve resource profile builder'}
if((Get-FileHash $from).Hash -cne 'C4183B9B26B42B04B3E7E3B2E1521C75CDD2F798FD4F61FA33C221571CDD3CEC'){throw 'Detailed build template changed'}
$original=[IO.File]::ReadAllText($from)
$pairs=@(
    @('trusted-bootstrap-profile-build-20261102-v3','trusted-bootstrap-profile-build-20261102-v4'),
    @('trusted-bootstrap-profile-source-20261102-v3','trusted-bootstrap-profile-source-20261102-v4'),
    @('87E7699FF07EDD646DCEA936DFC61B87646C4EA19C6375AE68AD7565EAF9B406','8A80C461B25495B17862096469E0CC17AC061947380CB59019C47CB8136F60EC'),
    @('$t.pairShapeControls -ne 1','$t.pairShapeControls -ne 1 -or !$t.resourceCounters -or $t.telemetryNegativeControls -ne 3')
)
$text=$original
foreach($pair in $pairs){if($pair.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique resource build boundary required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Resource build inverse differs'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
