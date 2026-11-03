param([Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedBuildSha256,
    [Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedBindingSha256)
$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-NamedScriptSuite.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve named full-suite attempt'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-NumericScriptSuite.generated.ps1'))
foreach($pair in @(
    @('numeric-references-script-run-v1','named-references-script-run-v1'),
    @('numeric-references-script-suite-v1','named-references-script-suite-v1'),
    @('8992F34CA977FA5DF8F65501E8495342F53D5538EC524682FC826B6B4C192071',$ExpectedBuildSha256),
    @('8AE4FEA4E26719D5563859451B5B6A5E63CA639B6B1BB7299134F8E5F7275D38',$ExpectedBindingSha256)
)){
    $expected=if($pair[0] -ceq 'numeric-references-script-suite-v1'){2}else{1}
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne $expected){throw 'Named full-suite anchor differs'}
    $text=$text.Replace($pair[0],$pair[1])
}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
