$ErrorActionPreference='Stop'
$baseHelper=Join-Path $PSScriptRoot 'Initialize-CurrentTemporalSuiteV2.generated.ps1'
$baseBuilder=Join-Path $PSScriptRoot 'Build-CurrentTemporalSuiteV2.generated.ps1'
if((Get-FileHash -LiteralPath $baseHelper).Hash -cne '8CAABC601214CD8DD8907A15C892137D4181FA3C111563FF5FB40A263CDA5A5B' -or (Get-FileHash -LiteralPath $baseBuilder).Hash -cne 'FDD3958BF5BB04B6BE7EA183BE3D53210D70AC2F78D65DAE4DEC1C93299D4C6F'){throw 'Preserve original suite builders'}
$helper=Join-Path $PSScriptRoot 'Initialize-RebuiltCoreScriptSuite.generated.ps1'
$builder=Join-Path $PSScriptRoot 'Build-RebuiltCoreScriptSuite.generated.ps1'
if((Test-Path -LiteralPath $helper) -or (Test-Path -LiteralPath $builder)){throw 'Preserve generated files'}
$text=[IO.File]::ReadAllText($baseHelper).Replace('Initialize-CurrentTemporalSuiteV2','Initialize-RebuiltCoreScriptSuite')
$anchor='    $replacement=@{'
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Replacement anchor changed'}
$core=@'
        'Lea.Engine.dll'=@('continuity-evidence/archived-native-build-v1/Lea.Engine/Lea.Engine.dll','6D89A8F5953F2E5A197C5D789D8D828BF8FC084836EB30F22EB00D228B0591F8')
'@
$text=$text.Replace($anchor,$anchor+"`r`n"+$core).Replace('$replaced.Count -ne 4','$replaced.Count -ne 5')
[IO.File]::WriteAllText($helper,$text,[Text.UTF8Encoding]::new($false))
$hash=(Get-FileHash -LiteralPath $helper).Hash
$text=[IO.File]::ReadAllText($baseBuilder).Replace('current-lazy-suite-v2','rebuilt-core-script-suite-v1').Replace('Initialize-CurrentTemporalSuiteV2.generated.ps1','Initialize-RebuiltCoreScriptSuite.generated.ps1').Replace('Initialize-CurrentTemporalSuiteV2 $build','Initialize-RebuiltCoreScriptSuite $build').Replace('8CAABC601214CD8DD8907A15C892137D4181FA3C111563FF5FB40A263CDA5A5B',$hash)
foreach($source in @([IO.File]::ReadAllText($helper),$text)){
    $tokens=$null;$errors=$null
    $null=[Management.Automation.Language.Parser]::ParseInput($source,[ref]$tokens,[ref]$errors)
    if($errors.Count){throw 'Generated syntax invalid'}
}
[IO.File]::WriteAllText($builder,$text,[Text.UTF8Encoding]::new($false))
[pscustomobject]@{helper=$hash;builder=(Get-FileHash -LiteralPath $builder).Hash;core='6D89A8F5953F2E5A197C5D789D8D828BF8FC084836EB30F22EB00D228B0591F8';assertionsRemoved=0}|ConvertTo-Json
