$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Test-NormalLazyHost.generated.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '4D28604229726221DA8975D07FC7104CB1A6990F1593BEA5405104F03B4E0203'){throw 'Base runner changed'}
$target=Join-Path $PSScriptRoot 'Test-RebuiltCoreHost.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve host runner'}
$text=[IO.File]::ReadAllText($base)
foreach($pair in @(
    @('\temporal-startup-v1\host-normal-lazy-v1','\temporal-startup-v1\host-rebuilt-core-v1'),
    @('15-stage host compatibility subset only; engine fullsuite remains failed/open','15-stage host subset with rebuilt core; matched full scripting suite and release remain unverified')
)){
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Ambiguous host substitution'}
    $text=$text.Replace($pair[0],$pair[1])
}
$anchor='    Space;$null=Pin $PSCommandPath'
$proof=@'
    Space;$null=Pin $PSCommandPath
    $coreProof=$workspace+'\continuity-evidence\rebuilt-core-regression-v4\result.json'
    $null=Pin $coreProof 'B0CC2CAF715527EB68885F31860EAA7D3A01D7A66826000941BFA01D7FBAD631'
    $core=Get-Content -LiteralPath $coreProof -Raw|ConvertFrom-Json
    if(!$core.behaviorPassed -or !$core.fullSuiteFooter -or !$core.inputsUnchanged -or !$core.bytesUnchanged -or $core.ownerAttention){throw 'Core behavioral proof incomplete'}
    $null=Pin ($workspace+'\continuity-evidence\rebuilt-core-regression-v4\acl-observation.json') 'B0077F3EE89493360F733C40401025A8E31013A9B4876715FDF8F8144DBBA425'
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Proof anchor differs'}
$text=$text.Replace($anchor,$proof)
$anchor="    CopyExact (`$permissionRoot+'\retry-view-v1\Lea.Win.dll')"
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Runtime override anchor differs'}
$insert="    CopyExact (`$workspace+'\continuity-evidence\archived-native-build-v1\Lea.Engine\Lea.Engine.dll') (`$result.candidate+'\Lea.Engine.dll') '6D89A8F5953F2E5A197C5D789D8D828BF8FC084836EB30F22EB00D228B0591F8'"
$text=$text.Replace($anchor,$insert+"`r`n"+$anchor)
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Host runner syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
