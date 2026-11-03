$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/native-selection-script-source-20261027.zip'
if(Test-Path $archive){throw 'Preserve source checkpoint'}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($relative in @(
    'PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','LeaForge/NATIVE_SELECTION.md',
    'Lea.Build/Prepare-NativeSelectionScriptSuite.ps1','Lea.Build/Initialize-NativeSelectionScriptSuite.generated.ps1',
    'Lea.Build/Build-NativeSelectionScriptSuite.generated.ps1','Lea.Build/Verify-NativeSelectionScriptSuite.generated.ps1',
    'Lea.Build/Test-OwnedNativeSelectionScriptSuite.generated.ps1','Lea.Build/Test-NativeSelectionScriptProjection.ps1',
    'Lea.Build/Audit-NativeSelectionScriptOutcome.ps1','Lea.Build/Audit-OwnedNativeSelectionScriptOutcome.generated.ps1',
    'Lea.Build/Backup-NativeSelectionScript.ps1'
)){$null=$paths.Add((Join-Path $workspace $relative))}
foreach($relative in @(
    'native-selection-script-preparation-v1','native-selection-script-preparation-v2',
    'native-selection-script-projection-controls-v1','temporal-startup-v1/current-native-script-suite-v9',
    'temporal-startup-v1/native-selection-script-owned-run-v1'
)){
    foreach($file in Get-ChildItem (Join-Path $workspace ('continuity-evidence/'+$relative)) -Recurse -File){$null=$paths.Add($file.FullName)}
}
$build=Get-Content (Join-Path $workspace 'continuity-evidence/temporal-startup-v1/current-native-script-suite-v9/result.json') -Raw|ConvertFrom-Json
foreach($file in $build.fixtureFiles){$null=$paths.Add($file.path)}
$reference=Join-Path $workspace 'continuity-evidence/native-selection-full-source-20261027.zip'
$referenceHash='244029DA5825D1F0A6720BA1F7499986D2BB94FE88AB192BD99CD8A5CD0ADCAC'
$leases=[Collections.Generic.List[IDisposable]]::new();$members=[Collections.Generic.List[object]]::new()
try{
    $s=[IO.File]::Open($reference,'Open','Read','Read');$leases.Add($s)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $referenceHash){throw 'Prior source checkpoint differs'}
    foreach($path in $paths){$s=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($s);$members.Add(@{entry=$path.Substring($workspace.Length+1).Replace('\','/');sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));stream=$s});$s.Position=0}
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{
        foreach($member in $members){$s=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$member.stream.CopyTo($s)}finally{$s.Dispose()}}
        $w=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$w.Write((@{scope='Matched Sx scripting suite9 exact source/runtime, projections, owned compiler/run proofs and read-only outcome. Prior view source and unchanged dependencies retained by reference, not nested. Local checkpoint, not portable SDK or off-device backup.';reference=@{path=$reference;sha256=$referenceHash};members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 6))}finally{$w.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{if($zip.Entries.Count -ne $members.Count+1){throw 'Inventory differs'};foreach($member in $members){$s=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $member.sha256){throw 'Readback differs'}}finally{$s.Dispose()}}}finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item $archive).Length;sha256=(Get-FileHash $archive).Hash}|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
