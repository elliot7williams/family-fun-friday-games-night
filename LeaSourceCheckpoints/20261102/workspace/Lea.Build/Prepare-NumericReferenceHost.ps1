$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Test-RebuiltAdapterHost.generated.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '586FE306934D59A8EF1A3F2851BC925B4F14749B03720DC301BAD5F9D455510A'){throw 'Host runner changed'}
$target=Join-Path $PSScriptRoot 'Test-NumericReferenceHost.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve numeric host run'}
$text=[IO.File]::ReadAllText($base)
foreach($pair in @(
    @('host-adapter-rebuilt-v1','host-numeric-references-v1'),
    @('rebuilt-core-regression-v4','numeric-references-regression-v1'),
    @('B0CC2CAF715527EB68885F31860EAA7D3A01D7A66826000941BFA01D7FBAD631','D4196CC2DB96AD7318704E6EC7BB30BC730D0E595DCB55B5205AA535D297A473'),
    @('B0077F3EE89493360F733C40401025A8E31013A9B4876715FDF8F8144DBBA425','5838A67A01DB46C16A60D6E3105CA4F748FDD9A62EBE5E38670036963D9EE19A'),
    @('continuity-evidence\archived-native-build-v1\Lea.Engine\Lea.Engine.dll','continuity-evidence\numeric-references-v3\candidate\Lea.Engine.dll'),
    @('6D89A8F5953F2E5A197C5D789D8D828BF8FC084836EB30F22EB00D228B0591F8','515121C456FEE03257695BB34BA7302CF11AF0FF868186A14FBB5E9CADF9DFC1')
)){
    $expected=if($pair[0] -ceq 'rebuilt-core-regression-v4'){2}else{1}
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne $expected){throw ('Host anchor differs: '+$pair[0])}
    $text=$text.Replace($pair[0],$pair[1])
}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Numeric host runner syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
