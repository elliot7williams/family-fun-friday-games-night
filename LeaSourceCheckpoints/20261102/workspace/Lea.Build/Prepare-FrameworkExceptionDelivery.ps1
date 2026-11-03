$ErrorActionPreference='Stop'
$source=Join-Path $PSScriptRoot 'Test-NativeCounterCalibration.ps1'
$target=Join-Path $PSScriptRoot 'Test-FrameworkExceptionDelivery.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve delivery runner'}
$text=[IO.File]::ReadAllText($source)
$changes=@(
    @('native-counter-calibration-v3','framework-exception-delivery-v1'),
    @('NativeCounterCalibration.cs','FrameworkExceptionDelivery.cs'),
    @('349A54BCE278CC0C73876F8489CC649C304B3E9D1A230966F65756D73AE76C1F',(Get-FileHash -LiteralPath (Join-Path $PSScriptRoot 'FrameworkExceptionDelivery.cs')).Hash)
)
foreach($pair in $changes){
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Delivery runner anchor differs'}
    $text=$text.Replace($pair[0],$pair[1])
}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
