$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-TitleMetadataRegressionV7.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve corrected renderer diagnostic attempt'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-TitleMetadataRegressionV6.generated.ps1'))
$text=$text.Replace('title-metadata-regression-v6','title-metadata-regression-v7')
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
