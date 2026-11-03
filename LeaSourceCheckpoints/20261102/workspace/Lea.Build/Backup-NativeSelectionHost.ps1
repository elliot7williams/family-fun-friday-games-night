$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/native-selection-host-source-20261027.zip'
if(Test-Path $archive){throw 'Preserve checkpoint'}
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-native-selection-owned-v1'
$receipt=Get-Content (Join-Path $root 'result.json') -Raw|ConvertFrom-Json
if(!$receipt.completed -or $receipt.ownerAttention){throw 'Campaign must finish safely before backup'}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($relative in @('PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','LeaForge/NATIVE_SELECTION.md','Lea.Build/Prepare-NativeSelectionHostCampaign.ps1','Lea.Build/Test-NativeSelectionHostCampaign.generated.ps1','Lea.Build/Test-PageColorCampaign.ps1','Lea.Build/Backup-NativeSelectionHost.ps1')){$null=$paths.Add((Join-Path $workspace $relative))}
foreach($folder in @($root,(Join-Path $workspace 'continuity-evidence/native-selection-host-preparation-v1'),(Join-Path $workspace 'continuity-evidence/page-color-host-v5/host-source'))){foreach($file in Get-ChildItem $folder -Recurse -File){$null=$paths.Add($file.FullName)}}
foreach($input in $receipt.inputs){if([IO.Path]::GetExtension($input.path) -in @('.cs','.ps1')){$null=$paths.Add($input.path)}}
$leases=[Collections.Generic.List[IDisposable]]::new();$members=[Collections.Generic.List[object]]::new()
try{
    foreach($path in $paths){
        if(!$path.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Path outside workspace'}
        $s=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($s)
        $members.Add(@{entry=[IO.Path]::GetRelativePath($workspace,$path).Replace('\','/');hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));stream=$s});$s.Position=0
    }
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{
        foreach($m in $members){$s=$zip.CreateEntry($m.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$m.stream.CopyTo($s)}finally{$s.Dispose()}}
        $w=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$w.Write((@{scope='Local host/view pairing checkpoint, matching staged host source and runtime; not portable SDK or off-device backup. Exact view source remains in separately retained native-selection-source-20261027.zip.';members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.hash}})}|ConvertTo-Json -Depth 5))}finally{$w.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{
        if($zip.Entries.Count -ne $members.Count+1){throw 'Inventory differs'}
        foreach($m in $members){$s=$zip.GetEntry($m.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $m.hash){throw 'Readback differs'}}finally{$s.Dispose()}}
    }finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item $archive).Length;sha256=(Get-FileHash $archive).Hash}|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
