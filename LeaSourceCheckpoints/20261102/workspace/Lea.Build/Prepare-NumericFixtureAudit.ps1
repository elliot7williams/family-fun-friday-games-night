$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Audit-CoreFixtureAcls.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '00E2F26595940A84BB76BCE8336BEBF9548E1EE91D2C0665D31D1CC4C1189206'){throw 'Fixture auditor changed'}
$target=Join-Path $PSScriptRoot 'Audit-NumericFixtureAcls.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve numeric ACL audit'}
$text=[IO.File]::ReadAllText($base)
foreach($pair in @(
    @('rebuilt-core-regression-v4','numeric-references-regression-v1'),
    @('B0CC2CAF715527EB68885F31860EAA7D3A01D7A66826000941BFA01D7FBAD631','D4196CC2DB96AD7318704E6EC7BB30BC730D0E595DCB55B5205AA535D297A473'),
    @('094D88DA0630BF414ED91F3ABFF24750F00239EB9409D2599E300F547035BE08','A7ED08B0A3ED450D250C91019D81AFA8525579E2A47137D421C3F3C1A1159417')
)){
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Fixture audit substitution differs'}
    $text=$text.Replace($pair[0],$pair[1])
}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
