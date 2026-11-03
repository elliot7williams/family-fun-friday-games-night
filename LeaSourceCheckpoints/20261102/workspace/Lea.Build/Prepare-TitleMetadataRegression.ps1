$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-TitleMetadataRegression.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve title full regression'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-DecoderLaunchV3Full.generated.ps1'))
$text=$text.Replace('decoder-launch-full-v3','title-metadata-regression-v1').Replace('decoder-launch-proof-v2/result.json','title-metadata-visuals-v1/result.json').Replace('EEB2A55F52143D5F4A8170339CCA18308A6D5ABB15E33680012F2EC623D6EC8E','04CB8E02EAC3FDEF82D2006C7E6C504C3A8913467DB4926EDFA5701A6D2103D4')
$anchor='-or !$trace.compatibilityProofPassed'
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Title proof requirement anchor differs'}
$text=$text.Replace($anchor,'-or !$trace.viewBuild.titleOverlayInverseVerified -or !$trace.baselineRegressionReproduced')
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
