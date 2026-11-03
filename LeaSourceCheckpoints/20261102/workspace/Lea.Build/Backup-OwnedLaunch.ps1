$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/owned-launch-source-20261026.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve checkpoint'}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($relative in @('PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/CURRENT_HOST.md','LeaForge/FINISH_PLAN.md','LeaForge/STARTUP_IO.md','LeaForge/OWNED_TEST_LAUNCH.md','Lea.Build/OwnedTestProcess.cs','Lea.Build/Test-OwnedTestProcess.ps1','Lea.Build/Test-OwnedLeaHost.ps1','Lea.Build/Backup-OwnedLaunch.ps1')){$null=$paths.Add((Join-Path $workspace $relative))}
foreach($relative in @('owned-test-process-controls-v1','owned-test-process-controls-v2','owned-lea-host-v1')){
    foreach($file in Get-ChildItem -LiteralPath (Join-Path $workspace ('continuity-evidence/'+$relative)) -File -Recurse){$null=$paths.Add($file.FullName)}
}
$references=@(
    @{path=(Join-Path $workspace 'continuity-evidence/library-qualification-source-20261026.zip');sha256='6DD3C892E80C68AC51D0F3CF584B1E9C83BF354C84006EF6A1D64E801D19E84E'},
    @{path=(Join-Path $workspace 'dist/LeaExes/Firespider-Lea-library-development-20261026.zip');sha256='A87429089BCC81724C133355EADD689E91D8ABF00B547D9C154FE2CC33AD1D17'}
)
$leases=[Collections.Generic.List[IDisposable]]::new();$members=[Collections.Generic.List[object]]::new()
try{
    foreach($reference in $references){$s=[IO.File]::Open($reference.path,'Open','Read','Read');$leases.Add($s);if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $reference.sha256){throw 'Retained dependency differs'}}
    foreach($path in $paths){$s=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($s);$members.Add(@{entry=$path.Substring($workspace.Length+1).Replace('\','/');sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));stream=$s});$s.Position=0}
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{
        foreach($member in $members){$s=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$member.stream.CopyTo($s)}finally{$s.Dispose()}}
        $writer=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$writer.Write((@{scope='Standalone original-Job/process diagnostic launcher source, exact v1/v2 controls, real638 host subset evidence and current docs. Not yet full-suite integrated; product binaries unchanged. Prior complete matching source/runtime dependencies retained by verified references, not nested. Local checkpoint, not portable/off-device SDK.';references=$references;members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 6))}finally{$writer.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{if($zip.Entries.Count -ne $members.Count+1){throw 'Inventory differs'};foreach($member in $members){$s=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $member.sha256){throw 'Readback differs'}}finally{$s.Dispose()}}}finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item -LiteralPath $archive).Length;sha256=(Get-FileHash -LiteralPath $archive).Hash}|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
