$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-CurrentHitGeometryV3.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve direct click controls'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-CurrentHitGeometryV2.generated.ps1'))
$text=$text.Replace('current-hit-geometry-v2','current-hit-geometry-v3')
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
