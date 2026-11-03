$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'continuity-evidence/decoder-grant-ownership-v1'
if(Test-Path $out){throw 'Preserve ownership observation'}
$sources=@(
    @{name='mature';path='continuity-evidence/print-live-capture-v1/view-source/Rp.cs';hash='2E419AAA91822EFB77C838F8DA268B107B1D9D1033F3CBBF762369816DB3F693'},
    @{name='root';path='Lea.Win/Rp.cs';hash='DF50F83C8BF1B0B2B91BFA24EDC05712691555E7035CEA61AB80BE14EA81B7CA'}
)
$findings=@()
foreach($source in $sources){
    $path=Join-Path $root $source.path
    if((Get-FileHash $path).Hash -cne $source.hash){throw 'Launcher source changed; inspect again'}
    $text=[IO.File]::ReadAllText($path)
    foreach($anchor in @(
        'private const string ContainerName = "Lea.Image.Decoder";',
        'return Open(path, args, directory, memory, cpu, ContainerName, new[] { path }, false);',
        'foreach (var file in files) fileAcl &= GrantFile(file, appSid);',
        'if (temporaryProfile) worker.release = () => Release(profile, files, identity);',
        'return revoked && DeleteAppContainerProfile(profile) == 0;'
    )){
        if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Expected exact source ownership contract differs'}
    }
    $findings+=@{source=$source.path;sha256=$source.hash;fixedDecoderProfile=$true;grantsFileAccess=$true;decoderSchedulesRevocation=$false;temporaryProfilesScheduleRevocation=$true;retainedSetupFailureEndpoint=$text.Contains('internal sealed class SetupFailure : InvalidOperationException')}
}
$run=Join-Path $root 'continuity-evidence/temporal-startup-v1/document-selection-script-owned-run-v1/result.json'
if((Get-FileHash $run).Hash -cne '3AE0FB5B8C8AC6AE96980905D7FD0D6C840F2B5CC725DEE12921D3144DE81676'){throw 'Observed campaign changed'}
$r=Get-Content $run -Raw|ConvertFrom-Json
if($r.passed -or !$r.behaviorPassed -or $r.changedFiles.Count -ne 1 -or $r.changedFiles[0].bytesChanged){throw 'Recorded ownership symptom differs'}
$result=@{observed=$true;sourceContracts=$findings;campaignReceiptSha256=(Get-FileHash $run).Hash;runtimeExecuted=$false;productionModified=$false;aclsModified=$false;rootLauncherAdopted=$false;securityQualified=$false;conclusion='Fixed decoder path explicitly opts out of temporary profile cleanup. Do not revoke shared fixed-SID grants independently across concurrent workers. Mature launcher lacks the root launcher retained setup-failure endpoint; temporary ownership migration needs failure-retirement coverage first.'}
$null=New-Item -ItemType Directory -Path $out
Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 5))
$result|ConvertTo-Json -Depth 5
