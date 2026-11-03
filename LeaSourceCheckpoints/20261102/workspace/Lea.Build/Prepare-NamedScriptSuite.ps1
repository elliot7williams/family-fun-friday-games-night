$ErrorActionPreference='Stop'
$helper=Join-Path $PSScriptRoot 'Initialize-NamedScriptSuite.generated.ps1'
$builder=Join-Path $PSScriptRoot 'Build-NamedScriptSuite.generated.ps1'
if((Test-Path -LiteralPath $helper) -or (Test-Path -LiteralPath $builder)){throw 'Preserve named script build'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Initialize-NumericScriptSuite.generated.ps1'))
foreach($pair in @(
    @('Initialize-NumericScriptSuite','Initialize-NamedScriptSuite'),
    @('continuity-evidence/numeric-references-v3/candidate/Lea.Engine.dll','continuity-evidence/named-references-v1/candidate/Lea.Engine.dll'),
    @('515121C456FEE03257695BB34BA7302CF11AF0FF868186A14FBB5E9CADF9DFC1','9208520EE6A067FF38D39368B2E50E6B099E5120C977A071E8D3E09733A3AF17')
)){
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Named script helper anchor differs'}
    $text=$text.Replace($pair[0],$pair[1])
}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($helper,$text,[Text.UTF8Encoding]::new($false))
$hash=(Get-FileHash -LiteralPath $helper).Hash
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Build-NumericScriptSuite.generated.ps1'))
$text=$text.Replace('numeric-references-script-suite-v1','named-references-script-suite-v1').Replace('Initialize-NumericScriptSuite','Initialize-NamedScriptSuite').Replace('C6848C0BD9344E11F7298749251980E18F4012BB040FEE03F135ABFD15E4061E',$hash)
$text=$text.Replace('Compile-only timing diagnostic, not rebound full-suite validation','Compile-only matched named-reference candidate suite; runtime validation separate')
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($builder,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $helper,$builder
