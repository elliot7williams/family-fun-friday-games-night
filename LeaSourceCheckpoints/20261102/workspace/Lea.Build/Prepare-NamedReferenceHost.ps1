$ErrorActionPreference='Stop'
$source=Join-Path $PSScriptRoot 'Test-NumericReferenceHost.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-NamedReferenceHost.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve named host evidence'}
$text=[IO.File]::ReadAllText($source)
foreach($pair in @(
    @('host-numeric-references-v1','host-named-references-v1'),
    @('numeric-references-regression-v1','named-references-regression-v1'),
    @('D4196CC2DB96AD7318704E6EC7BB30BC730D0E595DCB55B5205AA535D297A473','89E80A57BFBDF83EBCDCAE79F92984ECD8EE631FE0BBC83A11C0D32CF2A0304C'),
    @('5838A67A01DB46C16A60D6E3105CA4F748FDD9A62EBE5E38670036963D9EE19A','DFCD907F3B02328A605453DD2A82EED0682ECF26460ACB30509A1B2743E06353'),
    @('continuity-evidence\numeric-references-v3\candidate\Lea.Engine.dll','continuity-evidence\named-references-v1\candidate\Lea.Engine.dll'),
    @('515121C456FEE03257695BB34BA7302CF11AF0FF868186A14FBB5E9CADF9DFC1','9208520EE6A067FF38D39368B2E50E6B099E5120C977A071E8D3E09733A3AF17')
)){
    $expected=if($pair[0] -ceq 'numeric-references-regression-v1'){2}else{1}
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne $expected){throw 'Named host anchor differs'}
    $text=$text.Replace($pair[0],$pair[1])
}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
