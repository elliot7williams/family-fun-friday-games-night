$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/page-color-qualification-source-20261026.zip'
if(Test-Path $archive){throw 'Preserve checkpoint'}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($relative in @('PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','LeaForge/PAGE_COLORS.md','LeaForge/COMPATIBILITY.md','LeaForge/CURRENT_HOST.md','Lea.Build/Test-PageColorHost.ps1','Lea.Build/New-PageColorHarness.ps1','Lea.Build/Test-PageColorCampaign.ps1','Lea.Build/Test-PageColorCampaignContracts.ps1','Lea.Build/Backup-PageColorQualification.ps1')){
    $null=$paths.Add((Join-Path $workspace $relative))
}
foreach($relative in @('page-color-host-v6','page-color-host-v7','page-color-campaign-contracts-preliminary','page-color-campaign-contracts-v1','temporal-startup-v1/host-page-color-owned-v1')){
    $folder=Join-Path $workspace ('continuity-evidence/'+$relative)
    foreach($file in Get-ChildItem $folder -Recurse -File){
        $name=[IO.Path]::GetRelativePath($folder,$file.FullName)
        if($name -match '^host-source\\' -and $name -notin @('host-source\BrowserFeatures10.vb','host-source\FirespiderLea.vb')){continue}
        if($name -match '^runtime\\' -and $name -ne 'runtime\Lea.Firespider.Boot.exe'){continue}
        $null=$paths.Add($file.FullName)
    }
}
$references=@(
    @{path=(Join-Path $workspace 'continuity-evidence/page-color-source-20261026.zip');sha256='D4815CEA7C040DA98C2BEB6C86970AAB2F58170A2ED3AC074A909B1B386210E6'},
    @{path=(Join-Path $workspace 'dist/LeaExes/Firespider-Lea-library-development-20261026.zip');sha256='A87429089BCC81724C133355EADD689E91D8ABF00B547D9C154FE2CC33AD1D17'}
)
$leases=[Collections.Generic.List[IDisposable]]::new();$members=[Collections.Generic.List[object]]::new()
try{
    foreach($reference in $references){$s=[IO.File]::Open($reference.path,'Open','Read','Read');$leases.Add($s);if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $reference.sha256){throw 'Dependency differs'}}
    foreach($path in $paths){$s=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($s);$members.Add(@{entry=$path.Substring($workspace.Length+1).Replace('\','/');sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));stream=$s});$s.Position=0}
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{
        foreach($member in $members){$s=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$member.stream.CopyTo($s)}finally{$s.Dispose()}}
        $writer=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$writer.Write((@{scope='Page-color boundary and own-host qualification source/evidence, retained fixture failure and controls. Prior exact FBBD product source/binary and unchanged dependencies separately verified, not nested. Local checkpoint, not portable SDK or off-device backup.';references=$references;members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 6))}finally{$writer.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{
        if($zip.Entries.Count -ne $members.Count+1){throw 'Inventory differs'}
        foreach($member in $members){$s=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $member.sha256){throw 'Readback differs'}}finally{$s.Dispose()}}
    }finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item $archive).Length;sha256=(Get-FileHash $archive).Hash}|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
