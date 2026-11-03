$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/worker-startup-source-20261026.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve startup source checkpoint'}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($name in @('PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/CURRENT_HOST.md','LeaForge/FINISH_PLAN.md','LeaForge/STARTUP_IO.md','Lea.Build/WorkerStartupIl.cs','Lea.Build/ReadFailureIL.cs','Lea.Build/Audit-WorkerStartupBoundary.ps1','Lea.Build/WorkerStartupObserver.cs','Lea.Build/ExternalProcessCounters.cs','Lea.Build/Test-ClassListPaintDiagnostic.ps1','Lea.Build/Restore-WorkerReferencePack.ps1','Lea.Build/Build-ArchivedWorker.ps1','Lea.Build/Build-ArchivedNativeWorkerV2.generated.ps1','Lea.Build/Backup-WorkerStartupObservation.ps1')){$null=$paths.Add((Join-Path $workspace $name))}
foreach($name in @('classlist-paint-startup-v1','classlist-paint-startup-v2','classlist-paint-startup-v3','worker-startup-boundary-v1','worker-reference-pack-8032-v1','worker-local-reference-build-v1','worker-local-reference-native-v1')){
    foreach($file in Get-ChildItem -LiteralPath (Join-Path $workspace ('continuity-evidence/'+$name)) -File | Where-Object Extension -ne '.nupkg'){$null=$paths.Add($file.FullName)}
}
$source=Join-Path $workspace 'continuity-evidence/worker-local-reference-build-v1/archive'
foreach($file in Get-ChildItem -LiteralPath $source -File -Recurse){$null=$paths.Add($file.FullName)}
$null=$paths.Add((Join-Path $workspace 'continuity-evidence/classlist-paint-startup-v3/runtime/Lea.Script.Tests.exe'))
$references=@(
    @{path=(Join-Path $workspace 'continuity-evidence/classlist-paint-source-20261026.zip');sha256='0C06A88F0E397E7E1BFEE0C00A430ABBE2D027392EA091ADEAD8F62D4933B09F'},
    @{path=(Join-Path $workspace 'continuity-evidence/worker-reference-pack-8032-v1/microsoft.netcore.app.ref.8.0.32.nupkg');sha256='6088B3EBFE5AF44511EAF8D592E029E1EF870DA01B1FCB80AA790A6536BC14DB'}
)
$leases=[Collections.Generic.List[IDisposable]]::new();$members=[Collections.Generic.List[object]]::new()
try{
    foreach($reference in $references){$s=[IO.File]::Open($reference.path,'Open','Read','Read');$leases.Add($s);if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $reference.sha256){throw 'Retained reference differs'}}
    foreach($path in $paths){$s=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($s);$members.Add(@{entry=$path.Substring($workspace.Length+1).Replace('\','/');sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));stream=$s});$s.Position=0}
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{
        foreach($member in $members){$output=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$member.stream.CopyTo($output)}finally{$output.Dispose()}}
        $writer=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$writer.Write((@{scope='Thin local startup diagnostic and exact managed/native worker source rebuild checkpoint; unchanged test sources/dependencies/reference package retained separately. Not portable/off-device/release certification.';references=$references;members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 6))}finally{$writer.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{if($zip.Entries.Count -ne $members.Count+1){throw 'Checkpoint inventory differs'};foreach($member in $members){$s=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $member.sha256){throw 'Checkpoint member differs'}}finally{$s.Dispose()}}}finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item $archive).Length;sha256=(Get-FileHash $archive).Hash}|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
