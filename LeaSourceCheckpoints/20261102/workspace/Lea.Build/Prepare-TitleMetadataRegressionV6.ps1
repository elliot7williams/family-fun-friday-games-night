$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-TitleMetadataRegressionV6.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve renderer diagnostic attempt'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-TitleMetadataRegressionV5.generated.ps1'))
$text=$text.Replace('title-metadata-regression-v5','title-metadata-regression-v6')
$anchor='    $result.titleGeometryOverlayInverseVerified=$true'
$extra=@'
    $diagnosticHelper=Join-Path $PSScriptRoot 'New-RendererFailureDiagnostics.ps1';$null=Pin $diagnosticHelper;. $diagnosticHelper
    $updated=New-RendererFailureDiagnostics $updated
    $result.rendererFailureDiagnosticsInverseVerified=$true
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Renderer diagnostic stage differs'}
$text=$text.Replace($anchor,$anchor+"`r`n"+$extra)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
