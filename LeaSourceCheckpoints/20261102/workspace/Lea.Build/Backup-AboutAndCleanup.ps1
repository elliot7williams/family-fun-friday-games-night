$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/about-and-cleanup-20261026.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve previous checkpoint'}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($relative in @('PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/CURRENT_HOST.md','LeaForge/STORAGE_CLEANUP.md','Lea.Build/Backup-AboutAndCleanup.ps1','Lea.Build/Reclaim-OldPackage.ps1','Lea.Build/Archive-OldSeptemberPackages.ps1','Lea.Build/Test-ReclaimedOldPackages.ps1','Lea.Build/Test-AboutNavigation.ps1','Lea.Build/New-AboutNavigationSource.ps1','Lea.Build/New-AboutHarnessSource.ps1','continuity-evidence/reclaimed-old-packages-20261026-v3.json')){$null=$paths.Add((Join-Path $workspace $relative))}
foreach($version in @(1,2)){
    $folder=Join-Path $workspace ('continuity-evidence/about-navigation-v'+$version)
    foreach($file in Get-ChildItem -LiteralPath $folder -File){$null=$paths.Add($file.FullName)}
}
$source=Join-Path $workspace 'continuity-evidence/about-navigation-v2/host-source'
foreach($file in Get-ChildItem -LiteralPath $source -Recurse -File | Where-Object {$_.FullName -notmatch '\\(obj|bin)\\'}){$null=$paths.Add($file.FullName)}
$null=$paths.Add((Join-Path $workspace 'continuity-evidence/about-navigation-v2/runtime/Firespider Internet 19.exe'))
$null=$paths.Add((Join-Path $workspace 'Lea.Build/Repair-DotNet8032Host.ps1'))
$repair=Join-Path $workspace 'continuity-evidence/dotnet-host-repair-20261026-v1'
foreach($file in Get-ChildItem -LiteralPath $repair -File | Where-Object Extension -ne '.exe'){$null=$paths.Add($file.FullName)}
foreach($version in @(213,218,219,220,221,222,224)){
    $folder=Join-Path $workspace ('continuity-evidence/old-package-reclamation-20261026-'+$version+'-v2')
    foreach($name in @('result.json','runner.ps1')){$null=$paths.Add((Join-Path $folder $name))}
}
$members=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
try{
    foreach($path in $paths){$input=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($input);$members.Add(@{entry=$path.Substring($workspace.Length+1).Replace('\','/');sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($input));stream=$input});$input.Position=0}
    $zip=[IO.Compression.ZipFile]::Open($archive,[IO.Compression.ZipArchiveMode]::Create)
    try{
        foreach($member in $members){$output=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$member.stream.CopyTo($output)}finally{$output.Dispose()}}
        $writer=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$writer.Write((@{scope='Thin local About fix source/binary/evidence and cleanup receipts. Prior runtime dependencies and package ZIPs/metadata remain separately retained, not portable/off-device.';members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 5))}finally{$writer.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{if($zip.Entries.Count -ne $members.Count+1){throw 'Checkpoint inventory differs'};foreach($member in $members){$input=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($input)) -cne $member.sha256){throw 'Checkpoint bytes differ'}}finally{$input.Dispose()}}}finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item -LiteralPath $archive).Length;sha256=(Get-FileHash -LiteralPath $archive).Hash}|ConvertTo-Json
}finally{foreach($lease in $leases){$lease.Dispose()}}
