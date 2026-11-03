$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Test-CurrentTemporalSuiteFooterStrict.generated.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '72D9F33ECE6C09A481C2D845AA2F95487F7D483AC507B03DC484556E21BCFAFA'){throw 'Strict runner changed'}
$target=Join-Path $PSScriptRoot 'Test-RebuiltCoreScriptSuite.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve script run'}
$text=[IO.File]::ReadAllText($base)
foreach($pair in @(
    @('current-lazy-run-v3','rebuilt-core-script-run-v1'),
    @('current-lazy-suite-v2','rebuilt-core-script-suite-v1'),
    @('0D71CCA8558E48610A8B90C8A330D6D0EC1B7338E1174086E10B9AE5EEF395BA','A261868CAA8BE939B4124874FAAF98B6AD921DACB4E26F79855C4C221B4A2127'),
    @('8B25BF778552E8D2CC4A721A4DC8CD807D5A2E266107E61871E9A3972B69542B','8747463A10366CD90B6928272625CC954CFC666240CA846064F7293E0EDF0F0E')
)){
    if(!$text.Contains($pair[0])){throw 'Run anchor missing'}
    $text=$text.Replace($pair[0],$pair[1])
}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Invalid runner syntax'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
