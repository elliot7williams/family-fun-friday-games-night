$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/ordered-flow-visuals-v2'
$proof=Join-Path $root 'result.json'
if((Get-FileHash -LiteralPath $proof).Hash -cne '420AC4000877524E5EA285BFFA79AF2F0D0896CEA46A2D9B80E35E431BA4D4D4'){throw 'Ordered visual receipt differs'}
$archive=Join-Path $workspace 'continuity-evidence/ordered-flow-visuals-v2-backup.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve visual backup'}
$core=Join-Path $workspace 'continuity-evidence/ordered-flow-v4/candidate-source.zip'
if((Get-FileHash -LiteralPath $core).Hash -cne '676F599FA5DFFB0453E48618FCCA3E7CA5FFFEA38D3C10B7F9B77DE1EE2D925C'){throw 'Ordered source archive differs'}
foreach($pair in @(@($core,'engine-and-view-source.zip'),@($PSCommandPath,'backup.ps1'))){
    $to=Join-Path $root $pair[1]
    if(Test-Path -LiteralPath $to){throw 'Preserve existing source member'}
    Copy-Item -LiteralPath $pair[0] -Destination $to
    if((Get-FileHash -LiteralPath $pair[0]).Hash -cne (Get-FileHash -LiteralPath $to).Hash){throw 'Source member copy differs'}
}
$members=@(Get-ChildItem -LiteralPath $root -Recurse -File|ForEach-Object {@{path=$_.FullName;entry=[IO.Path]::GetRelativePath($root,$_.FullName).Replace('\','/');sha256=(Get-FileHash -LiteralPath $_.FullName).Hash}})
$zip=[IO.Compression.ZipFile]::Open($archive,[IO.Compression.ZipArchiveMode]::Create)
try{
    foreach($member in $members){
        $entry=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal)
        $input=[IO.File]::OpenRead($member.path);$output=$entry.Open()
        try{$input.CopyTo($output)}finally{$input.Dispose();$output.Dispose()}
    }
    $entry=$zip.CreateEntry('manifest.json');$writer=[IO.StreamWriter]::new($entry.Open(),[Text.UTF8Encoding]::new($false))
    try{$writer.Write((@{members=$members;scope='Native visual probe, six PNGs, review, exact test binaries/logs and nested engine/view source archive; not all browser sources or external compiler/runtime toolchain; local private backup, not a release'}|ConvertTo-Json -Depth 5))}finally{$writer.Dispose()}
}finally{$zip.Dispose()}
$zip=[IO.Compression.ZipFile]::OpenRead($archive)
try{
    if($zip.Entries.Count -ne $members.Count+1){throw 'Visual backup inventory differs'}
    foreach($member in $members){
        $stream=$zip.GetEntry($member.entry).Open()
        try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($hash -cne $member.sha256 -or (Get-FileHash -LiteralPath $member.path).Hash -cne $member.sha256){throw 'Visual backup bytes differ'}
    }
}finally{$zip.Dispose()}
$result=@{verified=$true;entries=$members.Count+1;bytes=(Get-Item -LiteralPath $archive).Length;sha256=(Get-FileHash -LiteralPath $archive).Hash;portableBuildKit=$false;offDeviceBackup=$false;releaseCertified=$false}
[IO.File]::WriteAllText((Join-Path $root 'backup-result.json'),($result|ConvertTo-Json))
$result|ConvertTo-Json
