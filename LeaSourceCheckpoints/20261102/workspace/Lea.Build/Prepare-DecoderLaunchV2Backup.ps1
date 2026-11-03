$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Backup-DecoderLaunchV2.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve launch V2 backup recipe'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Backup-DecoderLaunch.generated.ps1'))
$text=$text.Replace('decoder-launch-proof-v1','decoder-launch-proof-v2').Replace('decoder-launch-full-v1','decoder-launch-full-v2').Replace('decoder-launch-source-v1.zip','decoder-launch-source-v2.zip')
$text=$text.Replace('nested-flow-qualification-v1.zip','decoder-launch-source-v1.zip').Replace('023645FD52440AA8479B0FDB5C85921041371B66157D10EBF78AA103CB3D4695','976634E098F880748383D4DEDE6B9E714E65127828E2ACC601E6DB65DCD6BB54')
$text=$text.Replace("'Backup-DecoderLaunch.generated.ps1'","'Backup-DecoderLaunch.generated.ps1','Prepare-DecoderLaunchV2.ps1','New-DecoderLaunchV2Source.ps1','Build-DecoderLaunchV2View.generated.ps1','Test-DecoderLaunchV2.generated.ps1','Prepare-DecoderLaunchV2Full.ps1','Test-DecoderLaunchV2Full.generated.ps1','Prepare-DecoderLaunchV2Backup.ps1','Backup-DecoderLaunchV2.generated.ps1','Audit-DecoderLaunchV2Outcome.ps1','Test-LeaSuiteFooter.ps1','Test-SuiteFooterProof.ps1','Prepare-DecoderLaunchV3Runner.ps1','Test-DecoderLaunchV3Full.generated.ps1'")
$anchor='$prior=Join-Path $workspace'
$extra=@'
$members.Add(@{path=(Join-Path $workspace 'continuity-evidence/suite-footer-proof-v1/result.json');entry='footer-proof.json'})
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Footer proof archive anchor differs'}
$text=$text.Replace($anchor,$extra+"`r`n"+$anchor)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
