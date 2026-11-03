$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/classlist-paint-source-20261026.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve previous checkpoint'}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($relative in @('PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/CURRENT_HOST.md','LeaForge/FINISH_PLAN.md','LeaForge/CLASSLIST_PAINT.md','Lea.Script.Tests/View.cs','Lea.Script.Tests/ClassLists.cs','Lea.Build/Backup-ClassListPaint.ps1','Lea.Build/Test-ClassListPaintDiagnostic.ps1','Lea.Build/New-ClassListPaintContractSource.ps1','Lea.Build/Build-CurrentNativeScriptSuite.generated.ps1','Lea.Build/Verify-CurrentNativeScriptSuite.generated.ps1','Lea.Build/Test-CurrentNativeScriptSuiteV2.generated.ps1')){$null=$paths.Add((Join-Path $workspace $relative))}
foreach($relative in @('classlist-paint-diagnostic-v1','classlist-paint-contract-v1','classlist-paint-contract-v2','classlist-paint-contract-v3','temporal-startup-v1/current-native-script-suite-v2','temporal-startup-v1/current-native-script-run-v3')){
    foreach($file in Get-ChildItem -LiteralPath (Join-Path $workspace ('continuity-evidence/'+$relative)) -File){$null=$paths.Add($file.FullName)}
}
$suite=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/current-native-script-suite-v2'
foreach($file in Get-ChildItem -LiteralPath (Join-Path $suite 'source') -File -Recurse){$null=$paths.Add($file.FullName)}
foreach($relative in @('Lea.Script.Tests.exe','Lea.Script.Tests.pdb','WorkerEntryProbe/WorkerEntryNativeHost.exe','WorkerEntryProbe/manifest.json')){$null=$paths.Add((Join-Path (Join-Path $suite 'runtime') $relative))}
$prior=Join-Path $workspace 'continuity-evidence/current-native-host-checkpoint-20261026.zip'
$priorHash='52B691AB2E3CED8614E9D7BBBD3DA9D2E75F82E0BCCDCB1C5D4B59A271030B43'
if((Get-FileHash $prior).Hash -cne $priorHash){throw 'Prior source reference differs'}
$members=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
try{
    foreach($path in $paths){$s=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($s);$members.Add(@{entry=$path.Substring($workspace.Length+1).Replace('\','/');sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));stream=$s});$s.Position=0}
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{
        foreach($member in $members){$s=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$member.stream.CopyTo($s)}finally{$s.Dispose()}}
        $writer=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$writer.Write((@{scope='Thin local exact test source/binary/evidence checkpoint, not release/off-device backup; unchanged engine/worker dependencies retained separately.';references=@(@{path=$prior;sha256=$priorHash});members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 5))}finally{$writer.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{if($zip.Entries.Count -ne $members.Count+1){throw 'Checkpoint inventory differs'};foreach($member in $members){$s=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $member.sha256){throw 'Checkpoint member differs'}}finally{$s.Dispose()}}}finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item $archive).Length;sha256=(Get-FileHash $archive).Hash}|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
