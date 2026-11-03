param([ValidateSet(1,2,3)][int]$Version=2)
$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Build-WorkerRetirementTrace.ps1'
if((Get-FileHash $from).Hash -cne 'F0646DAC7763F953C683FAAEE6B87C788B002CC853F05CB274D61CBBEE959F02'){throw 'Scoped build template differs'}
$to=Join-Path $PSScriptRoot "Build-TrustedBootstrapProfile-v$Version.generated.ps1"
if(Test-Path $to){throw 'Preserve buffered profile build'}
$original=[IO.File]::ReadAllText($from)
$pairs=@(
    @('worker-retirement-trace-build-20261102-v2','trusted-bootstrap-profile-build-20261102-v1'),
    @('worker-retirement-trace-source-20261102-v2','trusted-bootstrap-profile-source-20261102-v1'),
    @('336EB5EBDD9061182D7EC89B69A1C2C72BC2FC63AAA70374C568FD0A76614D3A','6DB59C04E568E47765CC2DC19F7C4A629E4A851722AAAEC7A02BBAC2D3C8B97F'),
    @("-Name 'compile-retirement-trace'","-Name 'compile-trusted-bootstrap-profile'")
)
$text=$original
foreach($pair in $pairs){if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique profile build projection required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Profile build projection inverse differs'}
if($Version -ge 2){
    $text=$text.Replace('trusted-bootstrap-profile-build-20261102-v1','trusted-bootstrap-profile-build-20261102-v2').Replace('trusted-bootstrap-profile-source-20261102-v1','trusted-bootstrap-profile-source-20261102-v2').Replace('6DB59C04E568E47765CC2DC19F7C4A629E4A851722AAAEC7A02BBAC2D3C8B97F','D1255F73A1101815927CF1E909D605163D1C743C30AC0C7FFA66255A11AA3CBE').Replace('$t.files.Count -ne 2','$t.files.Count -ne 2 -or !$t.criticalOperationsVerified -or $t.pairShapeControls -ne 1')
}
if($Version -eq 3){
    $text=$text.Replace('trusted-bootstrap-profile-build-20261102-v2','trusted-bootstrap-profile-build-20261102-v3').Replace('trusted-bootstrap-profile-source-20261102-v2','trusted-bootstrap-profile-source-20261102-v3').Replace('D1255F73A1101815927CF1E909D605163D1C743C30AC0C7FFA66255A11AA3CBE','87E7699FF07EDD646DCEA936DFC61B87646C4EA19C6375AE68AD7565EAF9B406').Replace('$t.files.Count -ne 2','$t.files.Count -ne 3 -or !$t.resourceMarkerOrderVerified -or $t.markerNegativeControls -ne 3 -or !$t.diagnosticOnly -or $t.budgetsChanged')
}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
