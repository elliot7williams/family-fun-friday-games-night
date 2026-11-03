param([Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedBootstrapReceipt)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/project-compression-source-20261027.zip'
if(Test-Path $archive){throw 'Preserve checkpoint'}
$expected=@{trial='3B046F3DC04A7D043F45DAF6D2F67C4919EFF5DB93D05D98CA11E3FD7327AC43';modern='E825FFD56F04A55D76B740BBC8B838FED9E23F0DF4D854DFB1D02706CB74277A';bootstrap=$ExpectedBootstrapReceipt}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($relative in @('PROJECT_CONTEXT.md','LeaForge/STORAGE_CLEANUP.md','LeaForge/PROGRESS.md','Lea.Build/Compress-HistoricalEvidence.ps1','Lea.Build/Backup-ProjectCompression.ps1')){$null=$paths.Add((Join-Path $workspace $relative))}
foreach($scope in @('trial','modern','bootstrap')){
    $folder=Join-Path $workspace ('continuity-evidence/project-compression-20261027-'+$scope)
    $path=Join-Path $folder 'result.json'
    if((Get-FileHash $path).Hash -cne $expected[$scope]){throw 'Cleanup proof differs'}
    $proof=Get-Content $path -Raw|ConvertFrom-Json
    if(!$proof.passed -or $proof.deletedFiles -ne 0 -or $proof.before.Count -ne $proof.after.Count){throw 'Cleanup incomplete'}
    foreach($file in Get-ChildItem $folder -File){$null=$paths.Add($file.FullName)}
}
$leases=[Collections.Generic.List[IDisposable]]::new();$members=[Collections.Generic.List[object]]::new()
try{
    foreach($path in $paths){$s=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($s);$members.Add(@{entry=[IO.Path]::GetRelativePath($workspace,$path).Replace('\','/');sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));stream=$s});$s.Position=0}
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{
        foreach($m in $members){$s=$zip.CreateEntry($m.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$m.stream.CopyTo($s)}finally{$s.Dispose()}}
        $w=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$w.Write((@{scope='Project NTFS compression source and verification evidence only; no engine changes, deletion, runtime performance claim or complete SDK/off-device backup.';receipts=$expected;members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 5))}finally{$w.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{
        if($zip.Entries.Count -ne $members.Count+1){throw 'Inventory differs'}
        foreach($m in $members){$s=$zip.GetEntry($m.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $m.sha256){throw 'Readback differs'}}finally{$s.Dispose()}}
    }finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item $archive).Length;sha256=(Get-FileHash $archive).Hash}|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
