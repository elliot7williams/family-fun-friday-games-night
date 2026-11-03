$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-NestedOverlapRegression.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve overlap regression recipe'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-CurrentHitFullRegressionV2.generated.ps1'))
$text=$text.Replace('current-hit-full-regression-v2','nested-overlap-regression-v1')
$anchor='    $updated=New-FocusClickDiagnostics $updated'
$addition=@'
    $updated=New-FocusClickDiagnostics $updated
    $overlapHelper=Join-Path $PSScriptRoot 'New-NestedOverlapSmokeSource.ps1';$null=Pin $overlapHelper;. $overlapHelper
    $updated=New-NestedOverlapSmokeSource $updated
    $result.nestedOverlapSetupInverseVerified=$true
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Overlap runner anchor differs'}
$text=$text.Replace($anchor,$addition)
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
