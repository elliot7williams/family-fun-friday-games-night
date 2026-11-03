$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/storage-and-overlap-source-20261026.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve source checkpoint'}
$paths=[Collections.Generic.List[string]]::new()
foreach($name in @('PROJECT_CONTEXT.md','Lea.Tests/Smoke.cs','Lea.Build/New-NestedOverlapSmokeSource.ps1','Lea.Build/Prepare-NestedOverlapRegression.ps1','Lea.Build/Test-NestedOverlapRegression.generated.ps1','Lea.Build/Reclaim-OldPackage.ps1','Lea.Build/Run-OldPackageCleanupBatch.ps1','Lea.Build/Test-ReclaimedOldPackages.ps1','Lea.Build/Compress-OldPackageStorage.ps1','Lea.Build/Prepare-OldRuntimeStorageCompression.ps1','Lea.Build/Compress-OldRuntimeStorage.generated.ps1','Lea.Build/Backup-StorageAndOverlap.ps1','LeaForge/STORAGE_CLEANUP.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','continuity-evidence/reclaimed-old-packages-20261026-v2.json')){$paths.Add((Join-Path $workspace $name))}
foreach($folder in Get-ChildItem -LiteralPath (Join-Path $workspace 'continuity-evidence') -Directory | Where-Object {$_.Name -like 'old-package-reclamation-20261026-*' -or $_.Name -like 'old-package-storage-20261026-*' -or $_.Name -like 'old-runtime-storage-20261026-*'}){
    foreach($file in Get-ChildItem -LiteralPath $folder.FullName -File){$paths.Add($file.FullName)}
}
$prior=Join-Path $workspace 'continuity-evidence/current-hit-geometry-source-v1.zip'
if((Get-FileHash -LiteralPath $prior).Hash -cne '0F81C395F5BBF11ECC97DFE013E1F1778546206CDCEE6834A4B6C57953E83815'){throw 'Matched engine/source archive differs'}
$paths.Add($prior)
$leases=[Collections.Generic.List[IDisposable]]::new();$members=[Collections.Generic.List[object]]::new()
try{
    foreach($path in $paths){
        $stream=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($stream)
        $members.Add(@{entry=$path.Substring($workspace.Length+1).Replace('\','/');sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream));stream=$stream});$stream.Position=0
    }
    $zip=[IO.Compression.ZipFile]::Open($archive,[IO.Compression.ZipArchiveMode]::Create)
    try{
        foreach($member in $members){$output=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$member.stream.CopyTo($output)}finally{$output.Dispose()}}
        $writer=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$writer.Write((@{scope='Local task source/cleanup checkpoint. Referenced original September ZIPs retained separately; not a portable/off-device full backup or release qualification.';members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 5))}finally{$writer.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{
        if($zip.Entries.Count -ne $members.Count+1){throw 'Checkpoint inventory differs'}
        foreach($member in $members){$input=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($input)) -cne $member.sha256){throw 'Checkpoint content differs'}}finally{$input.Dispose()}}
    }finally{$zip.Dispose()}
    @{sha256=(Get-FileHash -LiteralPath $archive).Hash;entries=$members.Count+1;verified=$true;archive=$archive}|ConvertTo-Json
}finally{foreach($lease in $leases){$lease.Dispose()}}
