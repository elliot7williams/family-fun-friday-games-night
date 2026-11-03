$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Test-ConnectivityBootstrapComparison-v1.generated.ps1'
if((Get-FileHash $from).Hash -cne '1DDF939CC6D4E68CDA537C51A2DCB0FE298D8D2E9430FA6D263CC1A55D4D3AE1'){throw 'Comparison template differs'}
$to=Join-Path $PSScriptRoot 'Test-GlobalInstallComparison-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve installation comparison'}
$text=[IO.File]::ReadAllText($from)
$pairs=@(
    @('connectivity-bootstrap-comparison-v1','global-install-comparison-v1'),
    @('is-connected-script-build-20261102-v1/result.json','global-install-build-20261102-v1/result.json'),
    @('2596ED5B2B5E5210324DBE403C1526E029E4F99240080C4434DCD028445D46F9','FA61813EBDA435DFF4DC1138E450FE2C385AC3156534B507DB9216AA9878B21F'),
    @('FBD50D216D724A4BFCA0488C53CB1B648BC659AAE19F800AE7E129B207EBBD4C','F5BDB1FF4CAD596781E91D4FDAEED421E8B22C199EF97852BD96481D778E3B7B'),
    @('I:\LeaBuildStorage\sdk-reproducibility-20261101-v1\a\bin\Release\Lea.Script.dll','I:\LeaBuildStorage\is-connected-script-build-20261102-v1\source\bin\Release\Lea.Script.dll'),
    @('D2685E2F9AC15A72373CEE7B2B5EA69A7DD46605E1C058CB32406A085D7F6518','FBD50D216D724A4BFCA0488C53CB1B648BC659AAE19F800AE7E129B207EBBD4C')
)
foreach($pair in $pairs){$expected=if($pair[0].StartsWith('D2685')){2}else{1};if($pair.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne $expected){throw 'Comparison projection boundary differs'};$text=$text.Replace($pair[0],$pair[1])}
$anchor="    if(`$dlls.Count -ne 8){throw 'Eight-file trusted closure required'}"
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Comparison reserve insertion differs'}
$reserve=@'
    $stageBytes=2L*[long](($dlls|Measure-Object Length -Sum).Sum)+4MB
    if((Get-PSDrive C).Free -lt (600MB+$stageBytes)){throw 'Comparison requires reserve plus BOTH staged closures'}
'@
$text=$text.Replace($anchor,$anchor+[Environment]::NewLine+$reserve)
$text=$text.Replace('$result.comparisonCompleted=$false;','$result.candidateOnly=$true;$result.runtimeQualified=$false;$result.comparisonCompleted=$false;')
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Comparison syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
