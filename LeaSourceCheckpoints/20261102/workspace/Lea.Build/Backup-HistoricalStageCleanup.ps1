$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$audit='continuity-evidence/reclaimed-old-packages-20261028-v2.json'
if((Get-FileHash (Join-Path $root $audit)).Hash -cne '5A36B0F8527426BF142565CFB49E482035C8029ABE5F34D7A8BF4D93C3CC033B'){throw 'Recovery audit differs'}
$relative=@($audit,'PROJECT_CONTEXT.md','LeaForge/STORAGE_CLEANUP.md','LeaForge/PRINT_FRIENDLY.md',
    'Lea.Build/Reclaim-OldPackage.ps1','Lea.Build/Archive-HistoricalLeaStage.ps1',
    'Lea.Build/Test-ReclaimedOldPackages.ps1','Lea.Build/Backup-HistoricalStageCleanup.ps1')
foreach($name in @('185-v6-host185','185-v6-host185second','186-v6-work186')){
    $folder=Join-Path $root ('continuity-evidence/old-package-reclamation-20261026-'+$name)
    $r=Get-Content (Join-Path $folder 'result.json') -Raw|ConvertFrom-Json
    if(!$r.verified -or !$r.removed -or $r.error -or (Test-Path $r.target) -or
        (Get-FileHash $r.archive).Hash -cne $r.archiveSha256 -or
        (Get-FileHash $r.metadata).Hash -cne $r.metadataSha256){throw 'Retained recovery state differs'}
    $relative+=@(Get-ChildItem $folder -File|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})
}
$total=($relative|ForEach-Object {(Get-Item (Join-Path $root $_)).Length}|Measure-Object -Sum).Sum
if($total -gt 8MB -or (Get-PSDrive C).Free -lt 32MB+2*$total){throw 'Bounded backup workspace required; not a build waiver'}
$path=Join-Path $root 'continuity-evidence/historical-stage-cleanup-source-20261028.zip'
if(Test-Path $path){throw 'Preserve existing checkpoint'}
Add-Type -AssemblyName System.IO.Compression
$zip=[IO.Compression.ZipFile]::Open($path,'Create')
try{foreach($name in $relative){$null=[IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,(Join-Path $root $name),$name.Replace('\','/'),'Optimal')}}finally{$zip.Dispose()}
$zip=[IO.Compression.ZipFile]::OpenRead($path)
try{
    if($zip.Entries.Count -ne $relative.Count){throw 'Checkpoint inventory differs'}
    foreach($entry in $zip.Entries){
        $stream=$entry.Open()
        try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($hash -cne (Get-FileHash (Join-Path $root $entry.FullName)).Hash){throw 'Checkpoint entry differs'}
    }
}finally{$zip.Dispose()}
[pscustomobject]@{entries=$relative.Count;bytes=(Get-Item $path).Length;sha256=(Get-FileHash $path).Hash;buildRun=$false}|ConvertTo-Json
