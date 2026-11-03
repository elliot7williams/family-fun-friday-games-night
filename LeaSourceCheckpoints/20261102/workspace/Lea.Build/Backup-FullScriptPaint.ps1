$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/full-tree-startup-source-20261026.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve existing checkpoint'}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($relative in @('PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/CURRENT_HOST.md','LeaForge/FINISH_PLAN.md','LeaForge/STARTUP_IO.md','LeaForge/CLASSLIST_PAINT.md','Lea.Script.Tests/View.cs','Lea.Build/Backup-FullScriptPaint.ps1','Lea.Build/New-WorkerStartupObservationSource.ps1','Lea.Build/Test-WorkerStartupObservationSource.ps1','Lea.Build/WorkerStartupObserver.cs','Lea.Build/ExternalProcessCounters.cs','Lea.Build/Read-WorkerStartupObservation.ps1','Lea.Build/Test-WorkerStartupObservationReader.ps1','Lea.Build/New-ClassListPaintContractSource.ps1','Lea.Build/Test-ClassListPaintDiagnostic.ps1','Lea.Build/Build-CurrentNativeScriptSuite.generated.ps1','Lea.Build/Verify-CurrentNativeScriptSuite.generated.ps1','Lea.Build/Test-CurrentNativeScriptSuiteV2.generated.ps1','Lea.Build/Test-FullSuiteOutcomeControls.ps1')){
    $null=$paths.Add((Join-Path $workspace $relative))
}
foreach($relative in @('full-tree-observer-source-controls-v1','startup-observation-reader-controls-v1','startup-observation-reader-controls-v2','temporal-startup-v1/current-native-script-suite-v5','temporal-startup-v1/current-native-script-run-v6')){
    $folder=Join-Path $workspace ('continuity-evidence/'+$relative)
    $runtime=Join-Path $folder 'runtime'
    foreach($file in Get-ChildItem -LiteralPath $folder -File -Recurse){
        if(!$file.FullName.StartsWith($runtime+'\',[StringComparison]::OrdinalIgnoreCase)){$null=$paths.Add($file.FullName)}
    }
    $test=Join-Path $runtime 'Lea.Script.Tests.exe'
    if(Test-Path -LiteralPath $test){$null=$paths.Add($test)}
}
$leases=[Collections.Generic.List[IDisposable]]::new()
$members=[Collections.Generic.List[object]]::new()
try{
    foreach($path in $paths){
        $s=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($s)
        $members.Add(@{entry=$path.Substring($workspace.Length+1).Replace('\','/');sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));stream=$s});$s.Position=0
    }
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{
        foreach($member in $members){$s=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$member.stream.CopyTo($s)}finally{$s.Dispose()}}
        $writer=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$writer.Write((@{scope='Thin local exact tree-observed full-suite sources, binary and evidence. Product runtime unchanged. Prior full-script-paint-source-20261026.zip CC0821D6797F843E54A62E034812ACC0F7D7AA20E941F941094B489E83DA9AC9 and network-and-host-source-20261026.zip retained separately. Not portable SDK or off-device backup.';members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 5))}finally{$writer.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{
        if($zip.Entries.Count -ne $members.Count+1){throw 'Inventory differs'}
        foreach($member in $members){$s=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $member.sha256){throw 'Readback differs'}}finally{$s.Dispose()}}
    }finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item -LiteralPath $archive).Length;sha256=(Get-FileHash -LiteralPath $archive).Hash}|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
