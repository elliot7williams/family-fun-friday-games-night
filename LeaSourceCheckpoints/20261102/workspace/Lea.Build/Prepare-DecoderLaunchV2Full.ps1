$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$proof=Join-Path $workspace 'continuity-evidence/decoder-launch-proof-v2/result.json'
$p=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
if(!$p.passed -or !$p.inputsUnchanged -or !$p.candidateUnchanged -or $p.ownerAttention){throw 'Launch V2 controls incomplete'}
$target=Join-Path $PSScriptRoot 'Test-DecoderLaunchV2Full.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve launch V2 full attempt'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-DecoderLaunchFull.generated.ps1'))
$text=$text.Replace('decoder-launch-full-v1','decoder-launch-full-v2').Replace('decoder-launch-proof-v1/result.json','decoder-launch-proof-v2/result.json').Replace('74FE78B4FC8BC9AEA16AF41596421CBD80B2B8C44AE61D99B549205D5A1EB3CB',(Get-FileHash -LiteralPath $proof).Hash)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
