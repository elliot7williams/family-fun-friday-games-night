$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Test-NativeCounterCalibration.ps1'
$to=Join-Path $PSScriptRoot 'Test-FrameworkGuardCost.generated.ps1'
if(Test-Path -LiteralPath $to){throw 'Preserve guard runner'}
$text=[IO.File]::ReadAllText($from)
$changes=@(
    @('native-counter-calibration-v3','framework-guard-cost-v1'),
    @('NativeCounterCalibration.cs','FrameworkGuardCost.cs'),
    @('349A54BCE278CC0C73876F8489CC649C304B3E9D1A230966F65756D73AE76C1F',(Get-FileHash -LiteralPath (Join-Path $PSScriptRoot 'FrameworkGuardCost.cs')).Hash)
)
foreach($pair in $changes){
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Guard runner anchor differs'}
    $text=$text.Replace($pair[0],$pair[1])
}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($to,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $to
