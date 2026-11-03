$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/print-snapshot-source-20261027.zip'
if(Test-Path $archive){throw 'Preserve checkpoint'}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($relative in @('PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','LeaForge/PRINTING.md','Lea.Print/Ps.cs','Lea.Tests/PrintSnapshot.cs','Lea.Build/Test-PrintSnapshot.ps1','Lea.Build/Backup-PrintSnapshot.ps1','continuity-evidence/native-selection-view-v1/Lea.Engine.dll')){$null=$paths.Add((Join-Path $workspace $relative))}
foreach($v in 1..5){foreach($file in Get-ChildItem (Join-Path $workspace ('continuity-evidence/print-snapshot-v'+$v)) -File){$null=$paths.Add($file.FullName)}}
$leases=[Collections.Generic.List[IDisposable]]::new();$members=[Collections.Generic.List[object]]::new()
try{
    foreach($path in $paths){
        $s=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($s)
        $members.Add(@{entry=[IO.Path]::GetRelativePath($workspace,$path).Replace('\','/');sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));stream=$s});$s.Position=0
    }
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{
        foreach($m in $members){$s=$zip.CreateEntry($m.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$m.stream.CopyTo($s)}finally{$s.Dispose()}}
        $w=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$w.Write((@{scope='Local pure-managed print snapshot source/core/tests and retained failures. Compiler references hash-recorded separately, not portable SDK/off-device backup or host/printing qualification.';members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 5))}finally{$w.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{
        if($zip.Entries.Count -ne $members.Count+1){throw 'Inventory differs'}
        foreach($m in $members){$s=$zip.GetEntry($m.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $m.sha256){throw 'Readback differs'}}finally{$s.Dispose()}}
    }finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item $archive).Length;sha256=(Get-FileHash $archive).Hash}|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
