$ErrorActionPreference='Stop'
$source=Join-Path $PSScriptRoot 'Test-PrintStyledDocument.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintStyledDocumentV2.generated.ps1'
$out=Join-Path (Split-Path $PSScriptRoot -Parent) 'continuity-evidence/print-styled-document-preparation-v2'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve styled V2 preparation'}
if((Get-FileHash $source).Hash -cne '09C5C28932FE5C6FB025D16381B08DB29D7C557BA4D6E6114DD5B4518533A0B1'){throw 'Original differs'}
$original=[IO.File]::ReadAllText($source)
if([regex]::Matches($original,'print-styled-document-v1').Count -ne 1){throw 'Unique anchor required'}
$text=$original.Replace('print-styled-document-v1','print-styled-document-v2')
if($text.Replace('print-styled-document-v2','print-styled-document-v1') -cne $original){throw 'Inverse differs'}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Syntax differs'}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
@{inverseVerified=$true;syntaxVerified=$true;original=(Get-FileHash $source);generated=(Get-FileHash $target)}|ConvertTo-Json|Set-Content (Join-Path $out 'result.json')
Get-FileHash $target
