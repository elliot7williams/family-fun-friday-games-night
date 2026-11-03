$ErrorActionPreference='Stop'
$builder=Join-Path $PSScriptRoot 'Build-TitleMetadataView.generated.ps1'
$runner=Join-Path $PSScriptRoot 'Test-TitleMetadataVisuals.generated.ps1'
if((Test-Path -LiteralPath $builder) -or (Test-Path -LiteralPath $runner)){throw 'Preserve title metadata preparation'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Build-DecoderLaunchV2View.generated.ps1'))
$anchor='    $launchApplied=0'
$setup=@'
    $titleHelper=Join-Path $PSScriptRoot 'Remove-DocumentTitleSource.ps1';Hold $titleHelper (Get-FileHash -LiteralPath $titleHelper).Hash
    . $titleHelper
    $titleApplied=0
'@
$text=$text.Replace($anchor,$anchor+"`r`n"+$setup)
$anchor='                if($inverse -cne $original){throw ''Literal view inverse differs''}'
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Title view V overlay anchor differs'}
$text=$text.Replace($anchor,$anchor+"`r`n"+'                $updated=Remove-DocumentTitleSource $updated;$titleApplied++')
$text=$text.Replace('$launchApplied -ne 1','$launchApplied -ne 1 -or $titleApplied -ne 1').Replace('queueTraceInverseVerified=$true','queueTraceInverseVerified=$true;titleOverlayInverseVerified=$true')
[IO.File]::WriteAllText($builder,$text,[Text.UTF8Encoding]::new($false))
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-NestedFlowVisuals.generated.ps1'))
$text=$text.Replace('nested-flow-visuals-v1','title-metadata-visuals-v1').Replace('NestedFlowVisuals','TitleMetadataVisuals')
$text=$text.Replace('continuity-evidence/ordered-flow-regression-v3/fixtures/LeaForge/test-pages/nested-flow.html','LeaForge/test-pages/title-metadata.html').Replace('nested-flow.html','title-metadata.html')
$text=$text.Replace("Hold `$fixture '483F36D5FCE61EF0C0AC8C42B300E46111B04AD153A4BB1CDAA0D6237322719F'",'Hold $fixture (Get-FileHash -LiteralPath $fixture).Hash')
$start=$text.IndexOf("    `$view=Join-Path `$workspace ")
$end=$text.IndexOf('    $options=[Collections.Generic.List[string]]::new()',$start)
if($start -lt 0 -or $end -le $start){throw 'Title view staging span differs'}
$stage=@'
    $viewBuilder=Join-Path $PSScriptRoot 'Build-TitleMetadataView.generated.ps1';Hold $viewBuilder (Get-FileHash -LiteralPath $viewBuilder).Hash
    . $viewBuilder
    $result.viewBuild=Build-DecoderQueueTraceView $workspace $outRoot $result.candidate
    Hold $result.viewBuild.binary $result.viewBuild.sha256
'@
$text=$text.Remove($start,$end-$start).Insert($start,$stage+"`r`n")
$anchor="    `$result.before=@(Get-ChildItem -LiteralPath `$result.candidate"
$baseline=@'
    $baseline=Join-Path $outRoot 'baseline';$null=New-Item -ItemType Directory -Path $baseline
    foreach($name in @('Lea.Engine.dll','TitleMetadataVisuals.exe')){Copy-Item -LiteralPath (Join-Path $result.candidate $name) -Destination (Join-Path $baseline $name)}
    $oldView=Join-Path $workspace 'continuity-evidence/decoder-launch-proof-v2/runtime/Lea.Win.dll'
    Hold $oldView 'B2C1A05ABC9772857F69C9B6AE34C9E1CB69E51DFC35E8F03F60F70F4A523E6D'
    Copy-Item -LiteralPath $oldView -Destination (Join-Path $baseline 'Lea.Win.dll')
    $oldImages=Join-Path $baseline 'images';$null=New-Item -ItemType Directory -Path $oldImages
    $oldRun=Run 'baseline-title' (Join-Path $baseline 'TitleMetadataVisuals.exe') @(('"'+$page+'"'),('"'+$oldImages+'"')) 30
    $result.baselineRegressionReproduced=$oldRun.exitCode -eq 1 -and !$oldRun.timedOut -and !$oldRun.killRequested -and $oldRun.stdoutComplete -and $oldRun.stderrComplete -and ([IO.File]::ReadAllText($oldRun.stderr).Contains('synthetic page title absent'))
    if(!$result.baselineRegressionReproduced){throw 'Synthetic-title negative control not reproduced'}
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Title baseline stage anchor differs'}
$text=$text.Replace($anchor,$baseline+"`r`n"+$anchor)
$text=$text.Replace("@('nested-720','nested-320')","@('title-720','title-420')").Replace('(720|320) 900','(720|420) 500').Replace('Nested flow visuals: checks=36; failures=0','Title metadata visuals: checks=18; failures=0')
foreach($source in @([IO.File]::ReadAllText($builder),$text)){
    $tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($source,[ref]$tokens,[ref]$errors)
    if($errors.Count){throw ($errors|Out-String)}
}
[IO.File]::WriteAllText($runner,$text,[Text.UTF8Encoding]::new($false))
