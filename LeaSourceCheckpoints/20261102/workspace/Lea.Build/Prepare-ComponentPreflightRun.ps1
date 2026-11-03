$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-CurrentNativeScriptSuiteV2.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve component-preflight attempt'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-CurrentNativeScriptSuite.generated.ps1'))
$text=$text.Replace('current-native-script-run-v1','current-native-script-run-v2')
$anchor='    $result.candidate=$build.runtime'
$insert=@'
    $preflightHelper=Join-Path $workspace 'Lea.Build/Test-ComponentProbePrerequisites.ps1'
    $null=Pin $preflightHelper '21DF74BFC400ABDE2C5119C4CB8870E8D791521655568546AEA3CDB5DA3A26D5'
    . $preflightHelper
    $preflight=Get-LeaComponentProbePrerequisites
    $preflight|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $outRoot 'component-host-prerequisites.json')
    if(!$preflight.prerequisitesPresent){throw ($preflight.problems -join ' ')}
    $result.candidate=$build.runtime
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Component preflight anchor differs'}
$text=$text.Replace($anchor,$insert)
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
