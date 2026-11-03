$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$oldCore=Join-Path $workspace 'continuity-evidence/print-style-core-source-v1/E.cs'
$coreOut=Join-Path $workspace 'continuity-evidence/print-style-core-source-v2'
$runner=Join-Path $PSScriptRoot 'Test-PrintStyles.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintStylesV2.generated.ps1'
$out=Join-Path $workspace 'continuity-evidence/print-styles-preparation-v2'
if((Test-Path $coreOut) -or (Test-Path $out) -or (Test-Path $target)){throw 'Preserve V2 evidence'}
if((Get-FileHash $oldCore).Hash -cne 'CE230E646EB9A47743E90186C83D91FBD45B3C5A3429EBDAB0D20B0BBBDB7540' -or (Get-FileHash $runner).Hash -cne '2F6BA122FB46A1DB1F6188D3BB0A33AD629F3967BD6D4C0D89219C21228DC1A4'){throw 'Original source differs'}
$original=[IO.File]::ReadAllText($oldCore)
$before='ReadOnlyDictionary<N, S> Ps(P page, int width, int height)'
$after='ReadOnlyDictionary<N, S> Pst(P page, int width, int height)'
if([regex]::Matches($original,[regex]::Escape($before)).Count -ne 1){throw 'Alias anchor differs'}
$text=$original.Replace($before,$after)
if($text.Replace($after,$before) -cne $original){throw 'Core inverse differs'}
$fragment=Join-Path $PSScriptRoot 'PrintStyles.fragment.cs'
if(!$text.Contains([IO.File]::ReadAllText($fragment).Replace("`r",''))){throw 'Current fragment not matched'}
$null=New-Item -ItemType Directory -Path $coreOut
[IO.File]::WriteAllText((Join-Path $coreOut 'E.cs'),$text,[Text.UTF8Encoding]::new($false))
@{inverseVerified=$true;source=(Get-FileHash $oldCore);fragment=(Get-FileHash $fragment);candidate=(Get-FileHash (Join-Path $coreOut 'E.cs'));scope='Alias correction only; inherited V1 projection receipt required'}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $coreOut 'result.json')
$original=[IO.File]::ReadAllText($runner)
if([regex]::Matches($original,'print-styles-v1').Count -ne 1){throw 'Runner anchor differs'}
$text=$original.Replace('print-styles-v1','print-styles-v2')
if($text.Replace('print-styles-v2','print-styles-v1') -cne $original){throw 'Runner inverse differs'}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Runner syntax differs'}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
@{inverseVerified=$true;syntaxVerified=$true;original=(Get-FileHash $runner);generated=(Get-FileHash $target)}|ConvertTo-Json|Set-Content (Join-Path $out 'result.json')
Get-FileHash (Join-Path $coreOut 'E.cs'),$target
