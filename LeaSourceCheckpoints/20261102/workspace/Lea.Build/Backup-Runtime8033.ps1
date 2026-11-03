param([ValidateSet('Suite','Host')][string]$Scope='Suite')
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$stem=if($Scope -eq 'Suite'){'runtime-8033-matched-source-20261027'}else{'runtime-8033-host-source-20261027'}
$zipPath=Join-Path $workspace ('continuity-evidence/'+$stem+'.zip')
$receipt=Join-Path $workspace ('continuity-evidence/'+$stem+'.json')
if((Test-Path -LiteralPath $zipPath) -or (Test-Path -LiteralPath $receipt)){throw 'Preserve source checkpoint'}
if([IO.DriveInfo]::new('C:\').AvailableFreeSpace -lt 576MB){throw 'Checkpoint working reserve required'}
$files=@(Get-ChildItem -LiteralPath $PSScriptRoot -File|Where-Object Name -Like '*8033*')
$files+=@(Get-Item -LiteralPath (Join-Path $workspace 'PROJECT_CONTEXT.md'),(Join-Path $workspace 'Lea.Build/Compress-HistoricalEvidence.ps1'))
foreach($name in @('RUNTIME_8033_PLAN.md','PROGRESS.md','FINISH_PLAN.md','PRINTING.md','STORAGE_CLEANUP.md')){$files+=Get-Item -LiteralPath (Join-Path $workspace ('LeaForge/'+$name))}
$directories=@('runtime-8033-configs-v1','runtime-8033-prerequisite-preparation-v1','component-host-prerequisites-8033-v1','runtime-8033-suite-preparation-v1','runtime-8033-staging-controls-v1','runtime-8033-verification-preparation-v1','runtime-8033-outcome-preparation-v1','temporal-startup-v1/runtime-8033-script-owned-run-v1','print-styles-v3/core-source','print-live-capture-v1/view-source')
if($Scope -eq 'Suite'){$directories+='temporal-startup-v1/current-native-script-suite-v11'}
else{
    $directories+=@('runtime-8033-host-preparation-v1','temporal-startup-v1/host-runtime8033-owned-v1','runtime-8033-delivery-preparation-package-v1','runtime-8033-delivery-preparation-extraction-v1','runtime-8033-development-package-v1','page-color-host-v5/host-source')
    $files+=@(Get-ChildItem -LiteralPath (Join-Path $workspace 'continuity-evidence/runtime-8033-package-extraction-v1') -File)
    $files+=@(Get-Item -LiteralPath (Join-Path $workspace 'Lea.Build/Move-VerifiedSourceBackups.ps1'),(Join-Path $workspace 'LeaForge/CURRENT_HOST.md'),(Join-Path $workspace 'continuity-evidence/source-backup-relocation-20261027.json'))
}
foreach($name in $directories){
    $files+=@(Get-ChildItem -LiteralPath (Join-Path $workspace ('continuity-evidence/'+$name)) -File -Recurse -Force)
}
$files+=@(Get-ChildItem -LiteralPath (Join-Path $workspace 'Lea.Print') -File -Recurse -Force)
$specs=@{}
foreach($file in $files){
    $relative=[IO.Path]::GetRelativePath($workspace,$file.FullName).Replace('\','/')
    if([IO.Path]::IsPathRooted($relative) -or $relative.Split('/') -contains '..' -or $specs.ContainsKey($relative) -or ($file.Attributes -band [IO.FileAttributes]::ReparsePoint)){throw 'Checkpoint source path invalid'}
    $specs[$relative]=@{path=$file.FullName;sha256=(Get-FileHash -LiteralPath $file.FullName).Hash;length=$file.Length}
}
$archive=[IO.Compression.ZipFile]::Open($zipPath,'Create')
try{foreach($relative in $specs.Keys){$null=[IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive,$specs[$relative].path,$relative,'Optimal')}}finally{$archive.Dispose()}
$archive=[IO.Compression.ZipFile]::OpenRead($zipPath)
try{
    if($archive.Entries.Count -ne $specs.Count){throw 'Checkpoint inventory differs'}
    foreach($entry in $archive.Entries){
        $spec=$specs[$entry.FullName]
        if(!$spec -or $entry.Length -ne $spec.length){throw 'Checkpoint entry differs'}
        $stream=$entry.Open()
        try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($hash -cne $spec.sha256 -or (Get-FileHash -LiteralPath $spec.path).Hash -cne $spec.sha256){throw 'Checkpoint readback/original differs'}
    }
}finally{$archive.Dispose()}
$result=@{passed=$true;files=$specs.Count;archive=$zipPath;sha256=(Get-FileHash -LiteralPath $zipPath).Hash;bytes=(Get-Item -LiteralPath $zipPath).Length;originalsRetained=$true;sourceFiles=$specs;checkpointKind=$Scope;scope='Matched suite or host source/runtime/recipes/receipts plus projected core/view and print source; retained external worker/reference and prior suite source archives remain separate, not complete portable SDK or off-device backup'}
[IO.File]::WriteAllText($receipt,($result|ConvertTo-Json -Depth 6))
$result|Select-Object passed,files,sha256,bytes,originalsRetained|ConvertTo-Json
