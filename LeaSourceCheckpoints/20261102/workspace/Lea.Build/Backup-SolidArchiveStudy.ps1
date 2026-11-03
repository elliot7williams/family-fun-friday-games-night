param([ValidateRange(1,99)][int]$Version=1)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$relative=@('Lea.Build/Test-SolidArchiveTool.ps1','Lea.Build/Test-SolidBuildArchives.ps1',
    'Lea.Build/Clear-SolidArchiveScratch.ps1','Lea.Build/Backup-SolidArchiveStudy.ps1',
    'PROJECT_CONTEXT.md','LeaForge/STORAGE_CLEANUP.md',
    'continuity-evidence/solid-archive-tool-v1/result.json','continuity-evidence/solid-archive-tool-v1/runner.ps1',
    'continuity-evidence/solid-build-archives-v1/result.json','continuity-evidence/solid-build-archives-v1/runner.ps1',
    'continuity-evidence/solid-build-archives-v1/scratch-cleanup.json')
if($Version -ge 2){$relative+=@('Lea.Build/Consolidate-TwoOldBuildZips.ps1','Lea.Build/Get-ConsolidatedBuildArchive.ps1',
    'Lea.Build/Test-ReclaimedOldPackages.ps1','continuity-evidence/solid-build-archives-v1/consolidation.json',
    'continuity-evidence/solid-build-archives-v1/originals.json','continuity-evidence/solid-build-archives-v1/removal-intent.json',
    'continuity-evidence/reclaimed-old-packages-20261028-v3.json')}
if($Version -ge 3){$relative+=@('continuity-evidence/solid-build-archives-v2/result.json',
    'continuity-evidence/solid-build-archives-v2/runner.ps1','continuity-evidence/solid-build-archives-v2/consolidation.json',
    'continuity-evidence/solid-build-archives-v2/originals.json','continuity-evidence/solid-build-archives-v2/removal-intent.json',
    'continuity-evidence/reclaimed-old-packages-20261028-v4.json')}
$total=($relative|ForEach-Object {(Get-Item (Join-Path $root $_)).Length}|Measure-Object -Sum).Sum
if($total -gt 1MB -or (Get-PSDrive C).Free -lt 32MB+2*$total){throw 'Bounded checkpoint workspace required; not a build waiver'}
$name=if($Version -eq 1){'solid-archive-study-source-20261028.zip'}else{'solid-archive-study-source-20261028-v'+$Version+'.zip'}
$path=Join-Path $root ('continuity-evidence/'+$name)
if(Test-Path $path){throw 'Preserve previous checkpoint'}
Add-Type -AssemblyName System.IO.Compression
$zip=[IO.Compression.ZipFile]::Open($path,'Create')
try{foreach($name in $relative){$null=[IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,(Join-Path $root $name),$name.Replace('\','/'),'Optimal')}}finally{$zip.Dispose()}
$zip=[IO.Compression.ZipFile]::OpenRead($path)
try{
    if($zip.Entries.Count -ne $relative.Count){throw 'Checkpoint inventory differs'}
    foreach($entry in $zip.Entries){
        $stream=$entry.Open()
        try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($hash -cne (Get-FileHash (Join-Path $root $entry.FullName)).Hash){throw 'Checkpoint bytes differ'}
    }
}finally{$zip.Dispose()}
[pscustomobject]@{entries=$relative.Count;bytes=(Get-Item $path).Length;sha256=(Get-FileHash $path).Hash;scope='Source/evidence supplement; recovery container retained separately'}|ConvertTo-Json
