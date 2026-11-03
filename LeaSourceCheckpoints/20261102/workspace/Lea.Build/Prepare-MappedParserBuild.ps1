$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Build-MappedInterpreter.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne 'CDAD2518C668A552B9886837A5381090BA08F314642E6D8ACF07F65C5CB026D0'){throw 'Build owner changed'}
$target=Join-Path $PSScriptRoot 'Build-MappedParser.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve parser build owner'}
$text=[IO.File]::ReadAllText($base)
foreach($pair in @(
    @('interpreter-mapped-build-v1','parser-mapped-build-v1'),
    @('tested-interpreter-inputs-v1.zip','tested-parser-source-v1.zip'),
    @('E8F3D3CDF7F25589A8E7FDBFEEC15732C46310444147648C7F8DF0EF84F58E1C','326B4A7349F99ECB103E4F7371AC52F53BB562FD257FF03AC50202861279646A'),
    @('interpreter-ordered-map-v1/build-map.json','tested-parser-source-v1/manifest.json'),
    @('08C28F248AE2CC7BEC293E4EECEB455F0A530ADE1370B1298D59BA1EE1B9DC91','CA40B407F75DD28DA97CF4570CB4D52B6ACD66D01631D98FD576659E4D6054DF'),
    @('Compile-MappedInterpreter.ps1','Compile-MappedParser.ps1'),
    @('compile-archived-interpreter','compile-archived-parser')
)){
    if(!$text.Contains($pair[0])){throw 'Parser build anchor missing'}
    $text=$text.Replace($pair[0],$pair[1])
}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
