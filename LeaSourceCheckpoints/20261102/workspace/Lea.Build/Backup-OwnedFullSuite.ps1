$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/owned-full-suite-source-20261026.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve checkpoint'}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($relative in @('PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','LeaForge/OWNED_TEST_LAUNCH.md','LeaForge/STARTUP_IO.md','LeaForge/CURRENT_HOST.md','Lea.Build/OwnedTestProcess.cs','Lea.Build/Invoke-OwnedTestRun.ps1','Lea.Build/Test-OwnedRunAdapter.ps1','Lea.Build/Test-OwnedFullScriptSuite.ps1','Lea.Build/Test-OwnedFullSuiteContracts.ps1','Lea.Build/Audit-OwnedFullScriptOutcome.ps1','Lea.Build/Build-CurrentNativeScriptSuite.generated.ps1','Lea.Build/Verify-CurrentNativeScriptSuite.generated.ps1','Lea.Build/Backup-OwnedFullSuite.ps1')){$null=$paths.Add((Join-Path $workspace $relative))}
foreach($relative in @('owned-run-adapter-controls-v1','owned-run-adapter-controls-v2','owned-full-suite-contracts-v1','temporal-startup-v1/current-native-script-owned-run-v1','temporal-startup-v1/current-native-script-suite-v8')){
    foreach($file in Get-ChildItem -LiteralPath (Join-Path $workspace ('continuity-evidence/'+$relative)) -File -Recurse){
        if($relative -like '*suite-v8' -and $file.FullName.Contains('\runtime\') -and $file.Name -notlike 'Lea.Script.Tests*'){continue}
        $null=$paths.Add($file.FullName)
    }
}
$references=@(
    @{path=(Join-Path $workspace 'continuity-evidence/owned-launch-source-20261026.zip');sha256='4EF808D53C3A87A84CF079A5E1530D40613C20567C5E88DF7DFEE22F02476B79'},
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
        try{$writer.Write((@{scope='Owned full scripting source, exact suite sources and test binary, retained adapter failure and controls, run and read-only audit. Product unchanged; prior matching dependencies retained by verified references. Local thin checkpoint, not portable or off-device.';references=$references;members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 6))}finally{$writer.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{if($zip.Entries.Count -ne $members.Count+1){throw 'Inventory differs'};foreach($member in $members){$s=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $member.sha256){throw 'Readback differs'}}finally{$s.Dispose()}}}finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item -LiteralPath $archive).Length;sha256=(Get-FileHash -LiteralPath $archive).Hash}|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
