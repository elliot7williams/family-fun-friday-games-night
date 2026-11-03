$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Audit-LiteralFixtureAcls.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve literal fixture audit'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Audit-NamedFixtureAcls.generated.ps1'))
foreach($pair in @(
    @('named-references-regression-v1','literal-text-regression-v1'),
    @('89E80A57BFBDF83EBCDCAE79F92984ECD8EE631FE0BBC83A11C0D32CF2A0304C','5955288E6E8507594FDB52D0A6F4AB7A0C0A80D91EDC4BB1C1C91B71D3566CE5'),
    @('71AF48EACCF6D373EBAFA17535A1B9284C937FAD4F2EB5824CC25F2CFE2DBDA6','E1DBA967FE00875E4134CEFCD303601A43173486556A58D18E6117927AF2F906')
)){
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Literal fixture audit anchor differs'}
    $text=$text.Replace($pair[0],$pair[1])
}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
