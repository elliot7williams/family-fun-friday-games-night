$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/page-color-delivery-source-20261026.zip'
if(Test-Path $archive){throw 'Preserve checkpoint'}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($relative in @('PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','LeaForge/PAGE_COLORS.md','LeaForge/CURRENT_HOST.md','Lea.Build/Package-PageColorDevelopment.ps1','Lea.Build/Test-PageColorExtraction.ps1','Lea.Build/Backup-PageColorDelivery.ps1')){
    $null=$paths.Add((Join-Path $workspace $relative))
}
foreach($relative in @('page-color-development-package-v1','page-color-package-extraction-v1')){
    $folder=Join-Path $workspace ('continuity-evidence/'+$relative)
    foreach($file in Get-ChildItem $folder -Recurse -File){
        $name=[IO.Path]::GetRelativePath($folder,$file.FullName)
        if($name -match '^runtime\\'){continue}
        $null=$paths.Add($file.FullName)
    }
}
$references=@(
    @{path=(Join-Path $workspace 'continuity-evidence/page-color-qualification-source-20261026.zip');sha256='DC5D1464FC77FC9A67A287DC230C5340F685E95B616A900D4A05E194A4C4602B'},
    @{path=(Join-Path $workspace 'dist/LeaExes/Firespider-Lea-page-color-development-20261026.zip');sha256='BF7C6C6602FC0F65E38952873D89211AFB7138629A64E06C9B4971FF01D70DCF'}
)
$leases=[Collections.Generic.List[IDisposable]]::new();$members=[Collections.Generic.List[object]]::new()
try{
    foreach($reference in $references){$s=[IO.File]::Open($reference.path,'Open','Read','Read');$leases.Add($s);if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $reference.sha256){throw 'Reference differs'}}
    foreach($path in $paths){$s=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($s);$members.Add(@{entry=$path.Substring($workspace.Length+1).Replace('\','/');sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));stream=$s});$s.Position=0}
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{
        foreach($member in $members){$s=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$member.stream.CopyTo($s)}finally{$s.Dispose()}}
        $w=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$w.Write((@{scope='Development package/extraction source and evidence. Prior exact product source and runtime separately hash-pinned, no nested archives. Local thin checkpoint, not complete portable SDK or off-device backup.';references=$references;members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 6))}finally{$w.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{
        if($zip.Entries.Count -ne $members.Count+1){throw 'Inventory differs'}
        foreach($member in $members){$s=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $member.sha256){throw 'Readback differs'}}finally{$s.Dispose()}}
    }finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item $archive).Length;sha256=(Get-FileHash $archive).Hash}|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
