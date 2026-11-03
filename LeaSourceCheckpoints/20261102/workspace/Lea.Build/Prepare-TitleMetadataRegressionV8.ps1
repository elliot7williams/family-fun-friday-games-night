$ErrorActionPreference = 'Stop'
$target = Join-Path $PSScriptRoot 'Test-TitleMetadataRegressionV8.generated.ps1'
if (Test-Path -LiteralPath $target) { throw 'Preserve focus diagnostic attempt' }
$text = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-TitleMetadataRegressionV7.generated.ps1'))
$text = $text.Replace('title-metadata-regression-v7', 'title-metadata-regression-v8')
$anchor = '    $updated=New-RendererFailureDiagnostics $updated'
$addition = @'
    $updated=New-RendererFailureDiagnostics $updated
    $focusHelper=Join-Path $PSScriptRoot 'New-FocusClickDiagnostics.ps1';$null=Pin $focusHelper;. $focusHelper
    $updated=New-FocusClickDiagnostics $updated
    $result.focusClickDiagnosticsInverseVerified=$true
'@
if ([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1) { throw 'Runner diagnostic anchor differs' }
$text = $text.Replace($anchor, $addition)
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
