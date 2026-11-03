$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/native-selection-source-20261027.zip'
if(Test-Path $archive){throw 'Preserve source checkpoint'}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($relative in @('PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','LeaForge/NATIVE_SELECTION.md','Lea.Build/NativeSelection.fragment.cs','Lea.Build/New-NativeSelectionSource.ps1','Lea.Build/Test-NativeSelectionSource.ps1','Lea.Build/Test-NativeSelection.ps1','Lea.Build/Backup-NativeSelection.ps1','Lea.Tests/NativeSelectionProbe.cs','LeaForge/test-pages/native-selection.html')){
    $null=$paths.Add((Join-Path $workspace $relative))
}
foreach($relative in @('native-selection-source-v1','native-selection-view-v1','native-selection-view-v2')){
    foreach($file in Get-ChildItem (Join-Path $workspace ('continuity-evidence/'+$relative)) -Recurse -File){$null=$paths.Add($file.FullName)}
}
$reference=Join-Path $workspace 'continuity-evidence/page-color-delivery-source-20261026.zip'
$referenceHash='0F767FDC8978B978054866F1B67C76F2C654F5D5275A2282D4CEBEDF3C4FB42B'
$leases=[Collections.Generic.List[IDisposable]]::new();$members=[Collections.Generic.List[object]]::new()
try{
    $s=[IO.File]::Open($reference,'Open','Read','Read');$leases.Add($s)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $referenceHash){throw 'Retained dependency differs'}
    foreach($path in $paths){$s=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($s);$members.Add(@{entry=$path.Substring($workspace.Length+1).Replace('\','/');sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));stream=$s});$s.Position=0}
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{
        foreach($member in $members){$s=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$member.stream.CopyTo($s)}finally{$s.Dispose()}}
        $w=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$w.Write((@{scope='Experimental Sx view candidate, exact source and build/probe evidence. No host or default adoption. Prior delivery/unchanged dependencies separately retained; local thin checkpoint, not portable SDK or off-device backup.';reference=@{path=$reference;sha256=$referenceHash};members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 6))}finally{$w.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{if($zip.Entries.Count -ne $members.Count+1){throw 'Inventory differs'};foreach($member in $members){$s=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $member.sha256){throw 'Readback differs'}}finally{$s.Dispose()}}}finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item $archive).Length;sha256=(Get-FileHash $archive).Hash}|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
