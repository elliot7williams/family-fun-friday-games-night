$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/native-selection-delivery-source-20261027.zip'
if(Test-Path $archive){throw 'Preserve checkpoint'}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($relative in @('PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','LeaForge/NATIVE_SELECTION.md','Lea.Build/Prepare-NativeSelectionDelivery.ps1','Lea.Build/Package-NativeSelectionDevelopment.generated.ps1','Lea.Build/Test-NativeSelectionExtraction.generated.ps1','Lea.Build/Backup-NativeSelectionDelivery.ps1')){$null=$paths.Add((Join-Path $workspace $relative))}
foreach($relative in @('native-selection-development-package-v1','native-selection-package-extraction-v1','native-selection-delivery-preparation-package-v1','native-selection-delivery-preparation-extraction-v1')){
    $folder=Join-Path $workspace ('continuity-evidence/'+$relative)
    foreach($file in Get-ChildItem $folder -Recurse -File){
        if([IO.Path]::GetRelativePath($folder,$file.FullName) -match '^runtime\\'){continue}
        $null=$paths.Add($file.FullName)
    }
}
$references=@(
    @{path=(Join-Path $workspace 'continuity-evidence/native-selection-host-source-20261027.zip');sha256='D4F8FB82153418B2584C27E6D6A50C9D99499119B28DE7A64C755DACD8D0FC80'},
    @{path=(Join-Path $workspace 'continuity-evidence/native-selection-source-20261027.zip');sha256='3B3A74685DC11937AA63BED297D5E2843BF5DE7511436CB9CCC645BAE082FA99'},
    @{path=(Join-Path $workspace 'dist/LeaExes/Firespider-Lea-native-selection-development-20261027.zip');sha256='0C8A9663F5E0211E8D18840A548510819457A3E3092114A0281D32B6F0903A6A'}
)
$leases=[Collections.Generic.List[IDisposable]]::new();$members=[Collections.Generic.List[object]]::new()
try{
    foreach($reference in $references){$s=[IO.File]::Open($reference.path,'Open','Read','Read');$leases.Add($s);if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $reference.sha256){throw 'Reference differs'}}
    foreach($path in $paths){
        if(!$path.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Path outside workspace'}
        $s=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($s)
        $members.Add(@{entry=[IO.Path]::GetRelativePath($workspace,$path).Replace('\','/');sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));stream=$s});$s.Position=0
    }
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{
        foreach($m in $members){$s=$zip.CreateEntry($m.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$m.stream.CopyTo($s)}finally{$s.Dispose()}}
        $w=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$w.Write((@{scope='Local thin delivery checkpoint; exact source and runtime separately pinned, no nested archives. Not portable SDK or off-device backup.';references=$references;members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 6))}finally{$w.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{
        if($zip.Entries.Count -ne $members.Count+1){throw 'Inventory differs'}
        foreach($m in $members){$s=$zip.GetEntry($m.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $m.sha256){throw 'Readback differs'}}finally{$s.Dispose()}}
    }finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item $archive).Length;sha256=(Get-FileHash $archive).Hash}|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
