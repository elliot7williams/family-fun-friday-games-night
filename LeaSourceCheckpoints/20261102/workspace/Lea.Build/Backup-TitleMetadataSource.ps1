$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$archive=Join-Path $workspace 'continuity-evidence/title-metadata-source-v1.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve title metadata source archive'}
$members=[Collections.Generic.List[object]]::new()
foreach($relative in @('title-metadata-visuals-v1','title-metadata-regression-v1','temporal-startup-v1/framework-deep-guard-cost-v1')){
    $root=Join-Path $workspace ('continuity-evidence/'+$relative)
    $r=Get-Content -LiteralPath (Join-Path $root 'result.json') -Raw|ConvertFrom-Json
    if(!$r.inputsUnchanged -or $r.ownerAttention){throw 'Unresolved title/control evidence'}
    foreach($file in Get-ChildItem -LiteralPath $root -File -Recurse){
        $name=[IO.Path]::GetRelativePath($root,$file.FullName)
        if($name.StartsWith('runtime\') -or $name.StartsWith('fixtures\') -or $name.StartsWith('baseline\')){continue}
        $members.Add(@{path=$file.FullName;entry=$relative+'/'+$name.Replace('\','/')})
    }
}
foreach($name in @('FrameworkDeepGuardCost.cs','Prepare-FrameworkDeepGuardCost.ps1','Test-FrameworkDeepGuardCost.generated.ps1','Remove-DocumentTitleSource.ps1','Prepare-TitleMetadataVisuals.ps1','Build-TitleMetadataView.generated.ps1','Test-TitleMetadataVisuals.generated.ps1','Prepare-TitleMetadataRegression.ps1','Test-TitleMetadataRegression.generated.ps1','Backup-TitleMetadataSource.ps1','Test-LeaSuiteFooter.ps1')){
    $members.Add(@{path=(Join-Path $PSScriptRoot $name);entry='recipes/'+$name})
}
$prior=Join-Path $workspace 'continuity-evidence/current-bootstrap-evidence-v1.zip'
if((Get-FileHash -LiteralPath $prior).Hash -cne '979DAC770EBDDE8160566841FD72F6D002FCF43178D3A74F0621A383A3D3E293'){throw 'Prior source/evidence differs'}
$members.Add(@{path=$prior;entry='prior-matched-source-evidence.zip'})
foreach($member in $members){$member.sha256=(Get-FileHash -LiteralPath $member.path).Hash}
$zip=[IO.Compression.ZipFile]::Open($archive,[IO.Compression.ZipArchiveMode]::Create)
try{
    foreach($member in $members){
        $input=[IO.File]::OpenRead($member.path);$output=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open()
        try{$input.CopyTo($output)}finally{$input.Dispose();$output.Dispose()}
    }
    $writer=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open(),[Text.UTF8Encoding]::new($false))
    try{$writer.Write((@{members=$members;scope='Exact current view sources, title probes/screenshots/review, full regression evidence, deep guard control and nested prior core/host/test sources; local-only, not portable delivery or release certification'}|ConvertTo-Json -Depth 5))}finally{$writer.Dispose()}
}finally{$zip.Dispose()}
$zip=[IO.Compression.ZipFile]::OpenRead($archive)
try{
    if($zip.Entries.Count -ne $members.Count+1){throw 'Title archive inventory differs'}
    foreach($member in $members){
        $stream=$zip.GetEntry($member.entry).Open()
        try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($hash -cne $member.sha256 -or (Get-FileHash -LiteralPath $member.path).Hash -cne $member.sha256){throw 'Title archive bytes differ'}
    }
}finally{$zip.Dispose()}
$result=@{verified=$true;entries=$members.Count+1;bytes=(Get-Item -LiteralPath $archive).Length;sha256=(Get-FileHash -LiteralPath $archive).Hash;offDeviceBackup=$false;portableBuildKit=$false;releaseCertified=$false}
[IO.File]::WriteAllText((Join-Path $workspace 'continuity-evidence/title-metadata-visuals-v1/source-backup.json'),($result|ConvertTo-Json))
$result|ConvertTo-Json
