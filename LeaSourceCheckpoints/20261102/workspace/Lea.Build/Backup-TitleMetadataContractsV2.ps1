$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/title-metadata-regression-v2'
$archive=Join-Path $workspace 'continuity-evidence/title-metadata-contracts-v2.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve title contract archive'}
$r=Get-Content -LiteralPath (Join-Path $root 'result.json') -Raw|ConvertFrom-Json
if(!$r.inputsUnchanged -or $r.ownerAttention){throw 'Unresolved title contract run'}
$members=[Collections.Generic.List[object]]::new()
foreach($file in Get-ChildItem -LiteralPath $root -File){$members.Add(@{path=$file.FullName;entry='regression/'+$file.Name})}
foreach($name in @('New-TitleMetadataSmokeSource.ps1','Prepare-TitleMetadataRegressionV2.ps1','Test-TitleMetadataRegressionV2.generated.ps1','Backup-TitleMetadataContractsV2.ps1')){
    $members.Add(@{path=(Join-Path $PSScriptRoot $name);entry='recipes/'+$name})
}
$prior=Join-Path $workspace 'continuity-evidence/title-metadata-source-v1.zip'
if((Get-FileHash -LiteralPath $prior).Hash -cne '9FA3619F328E4B26E5CBFCF929D3136CF7D63C444B2FB51A65E2EBA7623791F1'){throw 'Matched title source archive differs'}
$members.Add(@{path=$prior;entry='matched-view-core-source.zip'})
foreach($member in $members){$member.sha256=(Get-FileHash -LiteralPath $member.path).Hash}
$zip=[IO.Compression.ZipFile]::Open($archive,[IO.Compression.ZipArchiveMode]::Create)
try{
    foreach($member in $members){
        $input=[IO.File]::OpenRead($member.path);$output=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open()
        try{$input.CopyTo($output)}finally{$input.Dispose();$output.Dispose()}
    }
    $writer=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open(),[Text.UTF8Encoding]::new($false))
    try{$writer.Write((@{members=$members;scope='Exact revised test source and matched title view/core source archive, compile/runtime evidence and local recipes; external references/toolchain not included, local-only and not release certification'}|ConvertTo-Json -Depth 5))}finally{$writer.Dispose()}
}finally{$zip.Dispose()}
$zip=[IO.Compression.ZipFile]::OpenRead($archive)
try{
    if($zip.Entries.Count -ne $members.Count+1){throw 'Title contract archive inventory differs'}
    foreach($member in $members){
        $stream=$zip.GetEntry($member.entry).Open()
        try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($hash -cne $member.sha256 -or (Get-FileHash -LiteralPath $member.path).Hash -cne $member.sha256){throw 'Title contract archive bytes differ'}
    }
}finally{$zip.Dispose()}
$result=@{verified=$true;entries=$members.Count+1;bytes=(Get-Item -LiteralPath $archive).Length;sha256=(Get-FileHash -LiteralPath $archive).Hash;offDeviceBackup=$false;portableBuildKit=$false;releaseCertified=$false}
[IO.File]::WriteAllText((Join-Path $root 'source-backup.json'),($result|ConvertTo-Json))
$result|ConvertTo-Json
