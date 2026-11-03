param([Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedBuildSha256)
$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Verify-NamedScriptSuite.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve named script verification'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Verify-NumericScriptSuite.generated.ps1'))
foreach($pair in @(
    @('numeric-references-script-suite-v1','named-references-script-suite-v1'),
    @('8992F34CA977FA5DF8F65501E8495342F53D5538EC524682FC826B6B4C192071',$ExpectedBuildSha256),
    @('515121C456FEE03257695BB34BA7302CF11AF0FF868186A14FBB5E9CADF9DFC1','9208520EE6A067FF38D39368B2E50E6B099E5120C977A071E8D3E09733A3AF17')
)){
    $expected=if($pair[0] -ceq 'numeric-references-script-suite-v1'){3}else{1}
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne $expected){throw 'Named script verification anchor differs'}
    $text=$text.Replace($pair[0],$pair[1])
}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
