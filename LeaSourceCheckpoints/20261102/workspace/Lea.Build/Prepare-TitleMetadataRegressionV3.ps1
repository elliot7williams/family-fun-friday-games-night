$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-TitleMetadataRegressionV3.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve title geometry contract run'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-TitleMetadataRegressionV2.generated.ps1'))
$text=$text.Replace('title-metadata-regression-v2','title-metadata-regression-v3')
$anchor='    $result.titleCountOverlayInverseVerified=$true'
$extra=@'
    $geometryHelper=Join-Path $PSScriptRoot 'New-TitleGeometrySmokeSource.ps1';$null=Pin $geometryHelper;. $geometryHelper
    $updated=New-TitleGeometrySmokeSource $updated
    $result.titleGeometryOverlayInverseVerified=$true
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Title geometry overlay stage differs'}
$text=$text.Replace($anchor,$anchor+"`r`n"+$extra)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
