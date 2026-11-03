param([Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedBuildSha256)
$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Verify-RebuiltCoreScriptSuite.generated.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '9EDD803D7BE5D03C178B96EB0F91D2B83DC3BA2DB0BFED45BD532E3DC58A0816'){throw 'Suite verifier changed'}
$target=Join-Path $PSScriptRoot 'Verify-NumericScriptSuite.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve numeric suite verification'}
$text=[IO.File]::ReadAllText($base).Replace('rebuilt-core-script-suite-v1','numeric-references-script-suite-v1').Replace('A261868CAA8BE939B4124874FAAF98B6AD921DACB4E26F79855C4C221B4A2127',$ExpectedBuildSha256).Replace('6D89A8F5953F2E5A197C5D789D8D828BF8FC084836EB30F22EB00D228B0591F8','515121C456FEE03257695BB34BA7302CF11AF0FF868186A14FBB5E9CADF9DFC1')
$anchor='$expected=@{'
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Verifier pairing anchor differs'}
$text=$text.Replace($anchor,$anchor+"`r`n    'Lea.Script.Win.dll'='AD4A057F5E82C8A36FA8362E54D1F746AD6264B9DF8D5447491DE001E34A59EA'")
$t=$null;$e=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$t,[ref]$e)
if($e.Count){throw 'Numeric verifier syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
