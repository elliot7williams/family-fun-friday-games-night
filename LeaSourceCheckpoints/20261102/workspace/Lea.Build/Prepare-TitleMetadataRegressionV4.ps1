$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-TitleMetadataRegressionV4.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve corrected title geometry contract run'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-TitleMetadataRegressionV3.generated.ps1'))
$text=$text.Replace('title-metadata-regression-v3','title-metadata-regression-v4')
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
