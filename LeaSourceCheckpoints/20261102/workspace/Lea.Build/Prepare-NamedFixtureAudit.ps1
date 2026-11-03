$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Audit-NamedFixtureAcls.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve named ACL observation'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Audit-NumericFixtureAcls.generated.ps1'))
foreach($pair in @(
    @('numeric-references-regression-v1','named-references-regression-v1'),
    @('D4196CC2DB96AD7318704E6EC7BB30BC730D0E595DCB55B5205AA535D297A473','89E80A57BFBDF83EBCDCAE79F92984ECD8EE631FE0BBC83A11C0D32CF2A0304C'),
    @('A7ED08B0A3ED450D250C91019D81AFA8525579E2A47137D421C3F3C1A1159417','71AF48EACCF6D373EBAFA17535A1B9284C937FAD4F2EB5824CC25F2CFE2DBDA6')
)){
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Named ACL audit anchor differs'}
    $text=$text.Replace($pair[0],$pair[1])
}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
