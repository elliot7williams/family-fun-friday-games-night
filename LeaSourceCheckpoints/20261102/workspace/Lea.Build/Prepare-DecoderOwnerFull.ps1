$ErrorActionPreference='Stop'
$workspace=(Get-Location).Path
$proof=Join-Path $workspace 'continuity-evidence/decoder-owner-proof-v1/result.json'
$p=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
if(!$p.passed -or !$p.inputsUnchanged -or !$p.candidateUnchanged -or $p.ownerAttention){throw 'Owner controls incomplete'}
$target=Join-Path $PSScriptRoot 'Test-DecoderOwnerFull.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve full owner diagnostic attempt'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-DecoderQueueFull.generated.ps1'))
$text=$text.Replace('decoder-queue-full-v1','decoder-owner-full-v1').Replace('decoder-queue-trace-v1/result.json','decoder-owner-proof-v1/result.json')
$text=$text.Replace('6AF56EF9D858E49A60FA050CED05A2430443C4AA8588B371CAB57BDA3E50BC0D',(Get-FileHash -LiteralPath $proof).Hash)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
