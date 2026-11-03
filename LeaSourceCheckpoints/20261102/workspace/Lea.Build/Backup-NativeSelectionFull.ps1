$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/native-selection-full-source-20261027.zip'
if(Test-Path $archive){throw 'Preserve checkpoint'}
$root=Join-Path $workspace 'continuity-evidence/native-selection-full-v1'
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($relative in @('PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','LeaForge/NATIVE_SELECTION.md','Lea.Build/Test-NativeSelectionFull.ps1','Lea.Build/Audit-NativeSelectionFull.ps1','Lea.Build/Observe-NativeSelectionFull.ps1','Lea.Build/Backup-NativeSelectionFull.ps1')){
    $null=$paths.Add((Join-Path $workspace $relative))
}
foreach($file in Get-ChildItem $root -Recurse -File){$null=$paths.Add($file.FullName)}
$receipt=Get-Content (Join-Path $root 'result.json') -Raw|ConvertFrom-Json
foreach($input in $receipt.inputs){
    if([IO.Path]::GetExtension($input.path) -in @('.cs','.rsp')){$null=$paths.Add($input.path)}
}
$reference=Join-Path $workspace 'continuity-evidence/native-selection-source-20261027.zip'
$hash='3B3A74685DC11937AA63BED297D5E2843BF5DE7511436CB9CCC645BAE082FA99'
$leases=[Collections.Generic.List[IDisposable]]::new();$members=[Collections.Generic.List[object]]::new()
try{
    $s=[IO.File]::Open($reference,'Open','Read','Read');$leases.Add($s)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $hash){throw 'Prior source differs'}
    foreach($path in $paths){
        if(!$path.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Source outside workspace'}
        $s=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($s)
        $members.Add(@{entry=$path.Substring($workspace.Length+1).Replace('\','/');sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));stream=$s});$s.Position=0
    }
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{
        foreach($member in $members){$s=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$member.stream.CopyTo($s)}finally{$s.Dispose()}}
        $w=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$w.Write((@{scope='Experimental Sx full-native regression sources/binaries/fixtures/evidence and read-only audit. Prior exact view sources and dependencies retained by reference, not nested. Local checkpoint, not portable SDK or off-device backup.';reference=@{path=$reference;sha256=$hash};members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 6))}finally{$w.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{if($zip.Entries.Count -ne $members.Count+1){throw 'Inventory differs'};foreach($member in $members){$s=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $member.sha256){throw 'Readback differs'}}finally{$s.Dispose()}}}finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item $archive).Length;sha256=(Get-FileHash $archive).Hash}|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
