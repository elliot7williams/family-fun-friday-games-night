$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Test-AsyncCancellationSuiteV2.generated.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '82DE655EA2BDA72F1834C239571F03026B390428F599900EECF3BD699E607731'){throw 'Regression runner changed'}
$target=Join-Path $PSScriptRoot 'Test-NumericReferenceRegression.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve numeric regression'}
$text=[IO.File]::ReadAllText($base)
foreach($pair in @(
    @('rebuilt-core-regression-v4','numeric-references-regression-v1'),
    @('continuity-evidence/archived-native-build-v1/Lea.Engine/Lea.Engine.dll','continuity-evidence/numeric-references-v3/candidate/Lea.Engine.dll'),
    @('6D89A8F5953F2E5A197C5D789D8D828BF8FC084836EB30F22EB00D228B0591F8','515121C456FEE03257695BB34BA7302CF11AF0FF868186A14FBB5E9CADF9DFC1')
)){
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Regression substitution differs'}
    $text=$text.Replace($pair[0],$pair[1])
}
$t=$null;$e=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$t,[ref]$e)
if($e.Count){throw 'Numeric regression syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
