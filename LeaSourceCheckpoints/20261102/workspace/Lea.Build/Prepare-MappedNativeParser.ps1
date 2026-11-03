$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Build-MappedNativeInterpreter.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '612E3C72B6CE1CDE7F9A70E69E6E51F230815349085F708B28DD49507C4C67A4'){throw 'Native owner changed'}
$target=Join-Path $PSScriptRoot 'Build-MappedNativeParser.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve native parser rebuild'}
$text=[IO.File]::ReadAllText($base)
foreach($pair in @(
    @('interpreter-mapped-native-v1','parser-mapped-native-v1'),
    @('interpreter-mapped-build-v1','parser-mapped-build-v1'),
    @('86199E378206B2398B934DF76DB4D1F90905FB39DD22CE32B6D8316874598DE7','9327D3AA2C7E1F658F02E3987BB8E99CF11055C1FD71C84045CE43F52D81AA50'),
    @('13B3EF3BA07B3DE50572D07D1B485A1ACDEBE307BE935C6E6F5D5771C492E32F','C3374BB12569649776AE440B81AF167E276858971CD9A6C34A30A6BB12698B7B'),
    @('67F3E7D75EDADCBC0110FD6BB7AC26E56784D3650A83C2AE0AA23DD67EB03E6D','CE23BFEFCDD287E0DE38CD818DF643287CD4C6CDCAACB20C46C379631D5C8821'),
    @("Join-Path `$root 'native-pair-v2/lazy/Jint.dll'","Join-Path `$workspace 'continuity-evidence/modern-worker-v1/parser-r2r-v1/variant/Acornima.dll'"),
    @("Join-Path `$root 'native-pair-v2/lazy/native.rsp'","Join-Path `$workspace 'continuity-evidence/modern-worker-v1/parser-r2r-v1/variant/compile.rsp'"),
    @('Jint.dll','Acornima.dll')
)){
    if(!$text.Contains($pair[0])){throw 'Parser native anchor missing'}
    $text=$text.Replace($pair[0],$pair[1])
}
$anchor="    `$lines=[IO.File]::ReadAllLines(`$recipe);"
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Recipe verification anchor differs'}
$text=$text.Replace($anchor,"    Hold `$recipe 'A14DF1D1C583A3C3BE3D24D4B8A6FC048F6FFBF5CBCAD129C10D197F30FF06EA'`r`n"+$anchor)
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Native parser owner syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
