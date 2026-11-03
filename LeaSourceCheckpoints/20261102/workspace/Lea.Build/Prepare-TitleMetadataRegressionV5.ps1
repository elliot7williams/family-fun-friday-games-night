$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-TitleMetadataRegressionV5.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve authored field contract run'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-TitleMetadataRegressionV4.generated.ps1'))
$text=$text.Replace('title-metadata-regression-v4','title-metadata-regression-v5')
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
