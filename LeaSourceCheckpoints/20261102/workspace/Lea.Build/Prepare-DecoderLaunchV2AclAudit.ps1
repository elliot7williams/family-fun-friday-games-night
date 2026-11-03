$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$proof=Join-Path $workspace 'continuity-evidence/decoder-launch-full-v2/result.json'
$p=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
if(!$p.fullBehaviorPassed -or !$p.inputsUnchanged -or !$p.candidateBytesUnchanged -or $p.ownerAttention){throw 'Full behavior evidence incomplete'}
$target=Join-Path $PSScriptRoot 'Audit-DecoderLaunchV2Acls.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve V2 grant observation'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Audit-DecoderNativeAcls.generated.ps1'))
$text=$text.Replace('decoder-queue-native-v1','decoder-launch-full-v2').Replace('F760B078269B86A6053C4A29EF29194A70FE2129434E1644B68E606966FC6130',(Get-FileHash -LiteralPath $proof).Hash).Replace('nativeBehaviorPassed','fullBehaviorPassed')
$text=$text.Replace('13000CE7C894A3719ECC1682041793591929DD595EFCE202D9BFBDB3B3CD8933','7728A2E5905FB3A5E22AD912FD24AB76F84C7A5B9F30FE98EA4B7FE9C54B80F4')
$text=$text.Replace('$observations.Count -ne 2','$observations.Count -ne 3').Replace('exactly two','exactly three').Replace('two exact private fixture grants','three exact private fixture grants')
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
