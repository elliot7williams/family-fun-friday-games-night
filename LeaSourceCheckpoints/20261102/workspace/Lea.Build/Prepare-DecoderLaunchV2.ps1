$ErrorActionPreference='Stop'
$targets=@('New-DecoderLaunchV2Source.ps1','Build-DecoderLaunchV2View.generated.ps1','Test-DecoderLaunchV2.generated.ps1')
foreach($name in $targets){if(Test-Path -LiteralPath (Join-Path $PSScriptRoot $name)){throw 'Preserve launch V2 preparation'}}
$helper=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'New-DecoderLaunchSource.ps1'))
if([regex]::Matches($helper,'guards').Count -ne 1){throw 'Parent stage name anchor differs'}
$helper=$helper.Replace('guards','token')
[IO.File]::WriteAllText((Join-Path $PSScriptRoot $targets[0]),$helper,[Text.UTF8Encoding]::new($false))
$builder=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Build-DecoderLaunchView.generated.ps1'))
$builder=$builder.Replace('New-DecoderLaunchSource.ps1','New-DecoderLaunchV2Source.ps1')
[IO.File]::WriteAllText((Join-Path $PSScriptRoot $targets[1]),$builder,[Text.UTF8Encoding]::new($false))
$runner=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-DecoderLaunchProof.generated.ps1'))
$runner=$runner.Replace('decoder-launch-proof-v1','decoder-launch-proof-v2').Replace('Build-DecoderLaunchView.generated','Build-DecoderLaunchV2View.generated').Replace('Decoder launch: checks=11; failures=0','Decoder launch: checks=12; failures=0')
[IO.File]::WriteAllText((Join-Path $PSScriptRoot $targets[2]),$runner,[Text.UTF8Encoding]::new($false))
