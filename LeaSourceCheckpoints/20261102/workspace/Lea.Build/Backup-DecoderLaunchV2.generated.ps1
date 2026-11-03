$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$archive=Join-Path $workspace 'continuity-evidence/decoder-launch-source-v2.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve owner source archive'}
$members=[Collections.Generic.List[object]]::new()
foreach($folder in @('decoder-launch-proof-v2','decoder-launch-full-v2')){
    $root=Join-Path $workspace ('continuity-evidence/'+$folder)
    $receipt=Get-Content -LiteralPath (Join-Path $root 'result.json') -Raw|ConvertFrom-Json
    if(!$receipt.inputsUnchanged -or $receipt.ownerAttention){throw 'Owner evidence incomplete'}
    foreach($file in Get-ChildItem -LiteralPath $root -File -Recurse){
        $relative=[IO.Path]::GetRelativePath($root,$file.FullName)
        if($relative.StartsWith('runtime\') -or $relative.StartsWith('fixtures\')){continue}
        $members.Add(@{path=$file.FullName;entry=$folder+'/'+$relative.Replace('\','/')})
    }
}
foreach($name in @('New-DecoderOwnerSource.ps1','Prepare-DecoderOwner.ps1','Prepare-DecoderOwnerProof.ps1','Prepare-DecoderOwnerFull.ps1','DecoderOwner.fragment.cs','Build-DecoderOwnerView.generated.ps1','Test-DecoderOwner.generated.ps1','Test-DecoderOwnerProof.generated.ps1','Test-DecoderOwnerFull.generated.ps1','Prepare-DecoderRetirement.ps1','Test-DecoderRetirement.generated.ps1','Backup-DecoderOwner.ps1','New-DecoderLaunchSource.ps1','Prepare-DecoderLaunch.ps1','DecoderLaunch.fragment.cs','Build-DecoderLaunchView.generated.ps1','Test-DecoderLaunch.generated.ps1','Prepare-DecoderLaunchProof.ps1','Test-DecoderLaunchProof.generated.ps1','Prepare-DecoderLaunchFull.ps1','Test-DecoderLaunchFull.generated.ps1','Prepare-DecoderLaunchBackup.ps1','Backup-DecoderLaunch.generated.ps1','Prepare-DecoderLaunchV2.ps1','New-DecoderLaunchV2Source.ps1','Build-DecoderLaunchV2View.generated.ps1','Test-DecoderLaunchV2.generated.ps1','Prepare-DecoderLaunchV2Full.ps1','Test-DecoderLaunchV2Full.generated.ps1','Prepare-DecoderLaunchV2Backup.ps1','Backup-DecoderLaunchV2.generated.ps1','Audit-DecoderLaunchV2Outcome.ps1','Test-LeaSuiteFooter.ps1','Test-SuiteFooterProof.ps1','Prepare-DecoderLaunchV3Runner.ps1','Test-DecoderLaunchV3Full.generated.ps1')){
    $members.Add(@{path=(Join-Path $PSScriptRoot $name);entry='recipes/'+$name})
}
$members.Add(@{path=(Join-Path $workspace 'continuity-evidence/suite-footer-proof-v1/result.json');entry='footer-proof.json'})
$prior=Join-Path $workspace 'continuity-evidence/decoder-launch-source-v1.zip'
if((Get-FileHash -LiteralPath $prior).Hash -cne '976634E098F880748383D4DEDE6B9E714E65127828E2ACC601E6DB65DCD6BB54'){throw 'Prior source archive differs'}
$members.Add(@{path=$prior;entry='prior-source.zip'})
foreach($member in $members){$member.sha256=(Get-FileHash -LiteralPath $member.path).Hash}
$zip=[IO.Compression.ZipFile]::Open($archive,[IO.Compression.ZipArchiveMode]::Create)
try{
    foreach($member in $members){
        $entry=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal)
        $input=[IO.File]::OpenRead($member.path);$output=$entry.Open()
        try{$input.CopyTo($output)}finally{$input.Dispose();$output.Dispose()}
    }
    $writer=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open(),[Text.UTF8Encoding]::new($false))
    try{$writer.Write((@{members=$members;scope='Diagnostic sources, controls, full-run evidence and prior source archive; local only, not portable delivery or release certification'}|ConvertTo-Json -Depth 5))}finally{$writer.Dispose()}
}finally{$zip.Dispose()}
$zip=[IO.Compression.ZipFile]::OpenRead($archive)
try{
    if($zip.Entries.Count -ne $members.Count+1){throw 'Owner archive inventory differs'}
    foreach($member in $members){
        $stream=$zip.GetEntry($member.entry).Open()
        try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($hash -cne $member.sha256 -or (Get-FileHash -LiteralPath $member.path).Hash -cne $member.sha256){throw 'Owner archive bytes differ'}
    }
}finally{$zip.Dispose()}
$result=@{verified=$true;entries=$members.Count+1;bytes=(Get-Item -LiteralPath $archive).Length;sha256=(Get-FileHash -LiteralPath $archive).Hash;portableBuildKit=$false;offDeviceBackup=$false;releaseCertified=$false}
[IO.File]::WriteAllText((Join-Path $workspace 'continuity-evidence/decoder-launch-proof-v2/source-backup.json'),($result|ConvertTo-Json))
$result|ConvertTo-Json
