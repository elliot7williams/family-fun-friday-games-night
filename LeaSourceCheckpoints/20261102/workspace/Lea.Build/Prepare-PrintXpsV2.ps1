$ErrorActionPreference='Stop'
$source=Join-Path $PSScriptRoot 'Test-PrintXps.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintXpsV2.generated.ps1'
$out=Join-Path (Split-Path $PSScriptRoot -Parent) 'continuity-evidence/print-xps-preparation-v2'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve XPS V2 preparation'}
if((Get-FileHash $source).Hash -cne 'E6D8A33FF256EE6B6A8B1E54A523F8BE034545D397C2F8636E47DA133D5C7DFA'){throw 'Original differs'}
$original=[IO.File]::ReadAllText($source)
if([regex]::Matches($original,'print-xps-v1').Count -ne 1){throw 'Unique anchor required'}
$text=$original.Replace('print-xps-v1','print-xps-v2')
if($text.Replace('print-xps-v2','print-xps-v1') -cne $original){throw 'Inverse differs'}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Syntax differs'}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
@{inverseVerified=$true;syntaxVerified=$true;original=(Get-FileHash $source);generated=(Get-FileHash $target)}|ConvertTo-Json|Set-Content (Join-Path $out 'result.json')
Get-FileHash $target
