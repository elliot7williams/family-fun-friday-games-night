$ErrorActionPreference='Stop'
$source=Join-Path $PSScriptRoot 'Test-PrintXpsV2.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintXpsV3.generated.ps1'
$out=Join-Path (Split-Path $PSScriptRoot -Parent) 'continuity-evidence/print-xps-preparation-v3'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve XPS V3 preparation'}
if((Get-FileHash $source).Hash -cne '605171764D1E9427BC58693818DD5EC4B4719366041110CFDB0B9EF43742D807'){throw 'Original differs'}
$original=[IO.File]::ReadAllText($source)
if([regex]::Matches($original,'print-xps-v2').Count -ne 1){throw 'Unique anchor required'}
$text=$original.Replace('print-xps-v2','print-xps-v3')
if($text.Replace('print-xps-v3','print-xps-v2') -cne $original){throw 'Inverse differs'}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Syntax differs'}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
@{inverseVerified=$true;syntaxVerified=$true;original=(Get-FileHash $source);generated=(Get-FileHash $target)}|ConvertTo-Json|Set-Content (Join-Path $out 'result.json')
Get-FileHash $target
