$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Verify-CurrentTemporalSuite.ps1'
$target=Join-Path $PSScriptRoot 'Verify-CurrentTemporalSuiteV2.generated.ps1'
if(Test-Path $target){throw 'Preserve V2 verifier'}
$text=[IO.File]::ReadAllText($base).Replace('current-lazy-suite-v1','current-lazy-suite-v2').Replace('8D446126AB56CF9656A9A2BEF9DC6FB4BDDAAB3D597765F7FB7DCC84000FD5CF','0D71CCA8558E48610A8B90C8A330D6D0EC1B7338E1174086E10B9AE5EEF395BA')
$old="(Join-Path `$root 'ReadEmbeddedEntryResource.cs'),"
if(!$text.Contains($old)){throw 'Helper anchor absent'}
$text=$text.Replace($old,"(Join-Path `$root 'ReadEmbeddedEntryResource.cs'),(Join-Path `$root 'EntryParentDigestAudit.cs'),")
$text=$text.Replace('[ParentResourceIlAudit]::Compare','[EntryParentDigestAudit]::Compare')
$t=$null;$e=$null;[void][Management.Automation.Language.Parser]::ParseInput($text,[ref]$t,[ref]$e);if($e.Count){throw 'V2 verifier syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
(Get-FileHash -LiteralPath $target).Hash
