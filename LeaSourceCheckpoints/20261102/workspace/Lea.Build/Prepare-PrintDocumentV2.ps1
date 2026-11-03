$ErrorActionPreference='Stop'
$source=Join-Path $PSScriptRoot 'Test-PrintDocument.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintDocumentV2.generated.ps1'
$out=Join-Path (Split-Path $PSScriptRoot -Parent) 'continuity-evidence/print-document-preparation-v2'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve document V2 preparation'}
if((Get-FileHash $source).Hash -cne '6D0A5AAC3A9AE8E18B264EAEDE270A3DAD6CDCA4483DE9583EA80F854A3C0DDB'){throw 'Original differs'}
$original=[IO.File]::ReadAllText($source)
if([regex]::Matches($original,'print-document-v1').Count -ne 1){throw 'Unique anchor required'}
$text=$original.Replace('print-document-v1','print-document-v2')
if($text.Replace('print-document-v2','print-document-v1') -cne $original){throw 'Inverse differs'}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Syntax differs'}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
@{inverseVerified=$true;syntaxVerified=$true;original=(Get-FileHash $source);generated=(Get-FileHash $target)}|ConvertTo-Json|Set-Content (Join-Path $out 'result.json')
Get-FileHash $target
