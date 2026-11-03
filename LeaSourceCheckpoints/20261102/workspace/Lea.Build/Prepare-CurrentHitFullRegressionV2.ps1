$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-CurrentHitFullRegressionV2.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve corrected candidate pin attempt'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-CurrentHitFullRegression.generated.ps1'))
$text=$text.Replace('current-hit-full-regression-v1','current-hit-full-regression-v2')
$old="'74956DA3FBBF28BC1DF281691FE7F8215C74CA67608AA61F6A6540E3BB0714AAC'"
if([regex]::Matches($text,[regex]::Escape($old)).Count -ne 1){throw 'Malformed pin anchor differs'}
$text=$text.Replace($old,'$hit.viewBuild.sha256')
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
