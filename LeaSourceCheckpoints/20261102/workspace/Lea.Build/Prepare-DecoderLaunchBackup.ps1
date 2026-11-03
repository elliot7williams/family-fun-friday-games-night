$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Backup-DecoderLaunch.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve launch backup recipe'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Backup-DecoderOwner.ps1'))
$text=$text.Replace('decoder-owner-proof-v1','decoder-launch-proof-v1').Replace('decoder-owner-full-v1','decoder-launch-full-v1').Replace('decoder-owner-source-v1.zip','decoder-launch-source-v1.zip')
$text=$text.Replace('decoder-queue-trace-source-v1.zip','nested-flow-qualification-v1.zip').Replace('3CEB637EBE8212886263C2C47CDBF25FD9A1280BAC0AB1109FC4842BB52F4349','023645FD52440AA8479B0FDB5C85921041371B66157D10EBF78AA103CB3D4695')
$text=$text.Replace("'Backup-DecoderOwner.ps1'","'Backup-DecoderOwner.ps1','New-DecoderLaunchSource.ps1','Prepare-DecoderLaunch.ps1','DecoderLaunch.fragment.cs','Build-DecoderLaunchView.generated.ps1','Test-DecoderLaunch.generated.ps1','Prepare-DecoderLaunchProof.ps1','Test-DecoderLaunchProof.generated.ps1','Prepare-DecoderLaunchFull.ps1','Test-DecoderLaunchFull.generated.ps1','Prepare-DecoderLaunchBackup.ps1','Backup-DecoderLaunch.generated.ps1'")
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
