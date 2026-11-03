$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/library-qualification-source-20261026.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve checkpoint'}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($relative in @('PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/CURRENT_HOST.md','LeaForge/FINISH_PLAN.md','LeaForge/STARTUP_IO.md','LeaForge/LIBRARY_NAVIGATION.md','Lea.Build/Backup-LibraryQualification.ps1','Lea.Build/Read-AllStartupTraceProof.ps1','Lea.Build/Test-TraceProofFinally.ps1','Lea.Build/Test-CurrentNativeScriptSuiteV2.generated.ps1','Lea.Build/Test-CurrentNativeHost.generated.ps1','Lea.Build/Package-AboutHostDevelopment.ps1','Lea.Build/Test-AboutDevelopmentPackage.ps1')){$null=$paths.Add((Join-Path $workspace $relative))}
foreach($relative in @('reboot-trace-integration-v1','trace-proof-finally-controls-v1','temporal-startup-v1/host-library-current-native-v1','library-host-development-package-v1','library-package-extraction-v1')){
    $folder=Join-Path $workspace ('continuity-evidence/'+$relative)
    foreach($file in Get-ChildItem -LiteralPath $folder -File -Recurse){
        $name=[IO.Path]::GetRelativePath($folder,$file.FullName)
        if(!$name.StartsWith('runtime\',[StringComparison]::OrdinalIgnoreCase)){$null=$paths.Add($file.FullName)}
    }
}
$references=@(
    @{path=(Join-Path $workspace 'continuity-evidence/library-and-trace-source-20261026.zip');sha256='A1515801B6A9696951BA07F874CAAD976E4A7896DAAD18F85D8138F6B8E38AC0'},
    @{path=(Join-Path $workspace 'continuity-evidence/trace-proof-source-20261026.zip');sha256='E1405A503F6C6184EC67D54C9FB9E73B86144B390F899D25D7CEB965F7BF98DF'},
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
        try{$writer.Write((@{scope='Thin local post-reboot trace-reporting fix, exact new638 host qualification, runtime package/extraction evidence and docs. Matching host/core sources and runtime ZIP retained separately by verified references; not nested or portable/off-device SDK. Full scripting and forced-stop descendant ownership remain open.';references=$references;members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 6))}finally{$writer.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{if($zip.Entries.Count -ne $members.Count+1){throw 'Inventory differs'};foreach($member in $members){$s=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $member.sha256){throw 'Readback differs'}}finally{$s.Dispose()}}}finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item -LiteralPath $archive).Length;sha256=(Get-FileHash -LiteralPath $archive).Hash}|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
