param([Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedBuildSha256,
    [Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedBindingSha256)
$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Test-RebuiltCoreScriptSuite.generated.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne 'ED09361580DC2D2143F9FCB4EF887C6884B9DC1AF14CDE2EC0D5E498F30BA37B'){throw 'Strict script runner changed'}
$target=Join-Path $PSScriptRoot 'Test-NumericScriptSuite.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve numeric script run'}
$text=[IO.File]::ReadAllText($base)
foreach($pair in @(
    @('rebuilt-core-script-run-v1','numeric-references-script-run-v1'),
    @('rebuilt-core-script-suite-v1','numeric-references-script-suite-v1'),
    @('A261868CAA8BE939B4124874FAAF98B6AD921DACB4E26F79855C4C221B4A2127',$ExpectedBuildSha256),
    @('8747463A10366CD90B6928272625CC954CFC666240CA846064F7293E0EDF0F0E',$ExpectedBindingSha256)
)){
    if(!$text.Contains($pair[0])){throw 'Numeric run anchor absent'}
    $text=$text.Replace($pair[0],$pair[1])
}
$t=$null;$e=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$t,[ref]$e)
if($e.Count){throw 'Numeric script runner syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
