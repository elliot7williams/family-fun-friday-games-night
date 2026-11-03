$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Verify-CurrentTemporalSuiteV2.generated.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '1E3FB18498B703457BEC14622E0BD6E04A6334353858CC9987E519E5F96C6ADF'){throw 'Verifier changed'}
$target=Join-Path $PSScriptRoot 'Verify-RebuiltCoreScriptSuite.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve verifier'}
$text=[IO.File]::ReadAllText($base).Replace('current-lazy-suite-v2','rebuilt-core-script-suite-v1').Replace('0D71CCA8558E48610A8B90C8A330D6D0EC1B7338E1174086E10B9AE5EEF395BA','A261868CAA8BE939B4124874FAAF98B6AD921DACB4E26F79855C4C221B4A2127')
$anchor='$expected=@{'
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Pairing anchor changed'}
$text=$text.Replace($anchor,$anchor+"`r`n    'Lea.Engine.dll'='6D89A8F5953F2E5A197C5D789D8D828BF8FC084836EB30F22EB00D228B0591F8'")
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
