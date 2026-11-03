$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Build-ArchivedNative.generated.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '18A2A19191B2A04EC5AAFBDE543585A002F74FB5460FDC9F883D38EAA47E2D53'){throw 'Builder changed'}
$target=Join-Path $PSScriptRoot 'Build-ArchivedNativeCSharp12.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve builder'}
$text=[IO.File]::ReadAllText($base).Replace('archived-native-build-v1','archived-native-build-v2')
$anchor='$text=[IO.File]::ReadAllText($recipe)'
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Recipe anchor changed'}
$text=$text.Replace($anchor,$anchor+".Replace('/langversion:latest','/langversion:12.0')")
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
