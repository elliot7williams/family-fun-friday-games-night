$ErrorActionPreference='Stop'
$helperBase=Join-Path $PSScriptRoot 'Initialize-RebuiltCoreScriptSuite.generated.ps1'
$builderBase=Join-Path $PSScriptRoot 'Build-RebuiltCoreScriptSuite.generated.ps1'
if((Get-FileHash -LiteralPath $helperBase).Hash -cne 'CEF09443682B4BBB1D9838341D9EA20E860B3BF98B6878CD159885ADF0AD5773' -or (Get-FileHash -LiteralPath $builderBase).Hash -cne 'AB70AF7FAD81A9A91100F7BC1DA3CAAC0F36D438C5317A6E55EAB62BB15F17DD'){throw 'Suite builders changed'}
$helper=Join-Path $PSScriptRoot 'Initialize-NumericScriptSuite.generated.ps1'
$builder=Join-Path $PSScriptRoot 'Build-NumericScriptSuite.generated.ps1'
if((Test-Path -LiteralPath $helper) -or (Test-Path -LiteralPath $builder)){throw 'Preserve numeric suite preparation'}
$text=[IO.File]::ReadAllText($helperBase)
foreach($pair in @(
    @('Initialize-RebuiltCoreScriptSuite','Initialize-NumericScriptSuite'),
    @('continuity-evidence/archived-native-build-v1/Lea.Engine/Lea.Engine.dll','continuity-evidence/numeric-references-v3/candidate/Lea.Engine.dll'),
    @('6D89A8F5953F2E5A197C5D789D8D828BF8FC084836EB30F22EB00D228B0591F8','515121C456FEE03257695BB34BA7302CF11AF0FF868186A14FBB5E9CADF9DFC1'),
    @('$replaced.Count -ne 5','$replaced.Count -ne 6')
)){
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Numeric helper anchor differs'}
    $text=$text.Replace($pair[0],$pair[1])
}
$anchor='    $replacement=@{'
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Replacement table differs'}
$text=$text.Replace($anchor,$anchor+"`r`n        'Lea.Script.Win.dll'=@('continuity-evidence/script-adapter-source-build-v2/Lea.Script.Win.dll','AD4A057F5E82C8A36FA8362E54D1F746AD6264B9DF8D5447491DE001E34A59EA')")
[IO.File]::WriteAllText($helper,$text,[Text.UTF8Encoding]::new($false))
$helperHash=(Get-FileHash -LiteralPath $helper).Hash
$text=[IO.File]::ReadAllText($builderBase).Replace('rebuilt-core-script-suite-v1','numeric-references-script-suite-v1').Replace('Initialize-RebuiltCoreScriptSuite','Initialize-NumericScriptSuite').Replace('CEF09443682B4BBB1D9838341D9EA20E860B3BF98B6878CD159885ADF0AD5773',$helperHash)
foreach($source in @([IO.File]::ReadAllText($helper),$text)){
    $t=$null;$e=$null;$null=[Management.Automation.Language.Parser]::ParseInput($source,[ref]$t,[ref]$e)
    if($e.Count){throw 'Numeric suite syntax invalid'}
}
[IO.File]::WriteAllText($builder,$text,[Text.UTF8Encoding]::new($false))
[pscustomobject]@{helper=$helperHash;builder=(Get-FileHash -LiteralPath $builder).Hash;assertionsRemoved=0}|ConvertTo-Json
