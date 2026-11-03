$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/network-and-host-source-20261026.zip'
if(Test-Path $archive){throw 'Preserve source checkpoint'}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($relative in @('PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/CURRENT_HOST.md','LeaForge/FINISH_PLAN.md','LeaForge/STARTUP_IO.md','LeaForge/history/PROJECT_CONTEXT_20261026_before_host_qualification.md','Lea.Build/New-WorkerNetworkProbeSource.ps1','Lea.Build/Test-WorkerNetworkProbeSource.ps1','Lea.Build/Build-ArchivedWorker.ps1','Lea.Build/Build-ArchivedNativeWorkerV2.generated.ps1','Lea.Build/Test-ClassListPaintDiagnostic.ps1','Lea.Build/Test-CurrentNativeHost.generated.ps1','Lea.Build/Package-AboutHostDevelopment.ps1','Lea.Build/Test-AboutDevelopmentPackage.ps1','Lea.Build/Backup-NetworkAndHost.ps1')){$null=$paths.Add((Join-Path $workspace $relative))}
foreach($relative in @('worker-network-probe-source-controls-v1','worker-network-probe-il-v1','worker-network-probe-native-v1','network-probe-startup-v1','temporal-startup-v1/host-about-current-native-v1','about-host-development-package-v1','about-package-extraction-v1')){
    $folder=Join-Path $workspace ('continuity-evidence/'+$relative)
    $runtime=Join-Path $folder 'runtime'
    foreach($file in Get-ChildItem -LiteralPath $folder -File -Recurse | Where-Object {!$_.FullName.StartsWith($runtime+'\',[StringComparison]::OrdinalIgnoreCase)}){$null=$paths.Add($file.FullName)}
}
$null=$paths.Add((Join-Path $workspace 'continuity-evidence/network-probe-startup-v1/runtime/Lea.Script.Tests.exe'))
$references=@(
    @{path=(Join-Path $workspace 'continuity-evidence/worker-startup-source-20261026.zip');sha256='64A6C2A3E257E747044F6B660CE242E0C57C71B94B7C04D622539C6FDA2D3861'},
    @{path=(Join-Path $workspace 'continuity-evidence/about-and-cleanup-20261026.zip');sha256='67405EB185F26B790C6A272F0785B5495D916E15C5B9A0E45B5A317F72292F95'},
    @{path=(Join-Path $workspace 'dist/LeaExes/Firespider-Lea-development-20261026.zip');sha256='A3B64730267912591E099D0A85DF95D01A49149CE89AEF12579FBF4F77C7349B'}
)
$members=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
try{
    foreach($reference in $references){$s=[IO.File]::Open($reference.path,'Open','Read','Read');$leases.Add($s);if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $reference.sha256){throw 'Retained dependency differs'}}
    foreach($path in $paths){$s=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($s);$members.Add(@{entry=$path.Substring($workspace.Length+1).Replace('\','/');sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));stream=$s});$s.Position=0}
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{
        foreach($member in $members){$output=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$member.stream.CopyTo($output)}finally{$output.Dispose()}}
        $writer=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$writer.Write((@{scope='Thin local timing-worker source/binary, host/package evidence and current docs checkpoint. Existing matching host/core/test source and runtime dependencies retained separately by reference; not portable/off-device or release certification.';references=$references;members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 6))}finally{$writer.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{if($zip.Entries.Count -ne $members.Count+1){throw 'Checkpoint inventory differs'};foreach($member in $members){$s=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $member.sha256){throw 'Checkpoint bytes differ'}}finally{$s.Dispose()}}}finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item $archive).Length;sha256=(Get-FileHash $archive).Hash}|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
