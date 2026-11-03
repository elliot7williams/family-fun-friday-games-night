$ErrorActionPreference='Stop'
$builder=Join-Path $PSScriptRoot 'Build-CurrentHitGeometry.generated.ps1'
$runner=Join-Path $PSScriptRoot 'Test-CurrentHitGeometry.generated.ps1'
if((Test-Path -LiteralPath $builder) -or (Test-Path -LiteralPath $runner)){throw 'Preserve hit geometry candidate recipes'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Build-TitleMetadataView.generated.ps1'))
$anchor='                $updated=Remove-DocumentTitleSource $updated;$titleApplied++'
$replacement=@'
                $updated=Remove-DocumentTitleSource $updated;$titleApplied++
                $hitHelper=Join-Path $PSScriptRoot 'New-CurrentHitGeometrySource.ps1';Hold $hitHelper (Get-FileHash -LiteralPath $hitHelper).Hash
                . $hitHelper
                $updated=New-CurrentHitGeometrySource $updated
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'View builder anchor differs'}
$text=$text.Replace($anchor,$replacement)
[IO.File]::WriteAllText($builder,$text,[Text.UTF8Encoding]::new($false))
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-FocusClickProbe.ps1'))
$text=$text.Replace('focus-click-probe-v2','current-hit-geometry-v1')
$anchor='    $fixtureRows=@($prior.inputs|Where-Object'
$insert=@'
    function Hold([string]$Path,[string]$Expected){$null=Pin $Path $Expected}
    $viewBuilder=Join-Path $PSScriptRoot 'Build-CurrentHitGeometry.generated.ps1';$null=Pin $viewBuilder
    . $viewBuilder
    $result.viewBuild=Build-DecoderQueueTraceView $workspace $outRoot $outRoot
    $fixtureRows=@($prior.inputs|Where-Object
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Probe builder anchor differs'}
$text=$text.Replace($anchor,$insert)
$anchor='    $run=Run ''focus-probe'' $binary @((''"''+(Join-Path $outRoot ''keyboard-focus.html'')+''"'')) 60'
$replacement='    $run=Run ''focus-probe'' $binary @((''"''+(Join-Path $outRoot ''keyboard-focus.html'')+''"''),''require-current'') 60'
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Probe activation anchor differs'}
$text=$text.Replace($anchor,$replacement)
[IO.File]::WriteAllText($runner,$text,[Text.UTF8Encoding]::new($false))
