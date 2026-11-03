$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/trace-proof-source-20261026.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve checkpoint'}
$files=@('PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/STARTUP_IO.md','Lea.Build/Read-AllStartupTraceProof.ps1','Lea.Build/Test-AllStartupTraceProof.ps1','Lea.Build/Backup-TraceProof.ps1','continuity-evidence/all-startup-trace-proof-controls-v1/result.json')
$leases=[Collections.Generic.List[IDisposable]]::new();$members=[Collections.Generic.List[object]]::new()
try{
    foreach($file in $files){$s=[IO.File]::Open((Join-Path $workspace $file),'Open','Read','Read');$leases.Add($s);$members.Add(@{entry=$file;sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));stream=$s});$s.Position=0}
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{
        foreach($member in $members){$s=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$member.stream.CopyTo($s)}finally{$s.Dispose()}}
        $writer=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$writer.Write((@{scope='Source-only fail-closed trace reader, controls and current docs. Runner integration pending user restart; not release or off-device backup.';priorCheckpoint='library-and-trace-source-20261026.zip';priorSha256='A1515801B6A9696951BA07F874CAAD976E4A7896DAAD18F85D8138F6B8E38AC0';members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 5))}finally{$writer.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{if($zip.Entries.Count -ne $members.Count+1){throw 'Inventory differs'};foreach($member in $members){$s=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $member.sha256){throw 'Readback differs'}}finally{$s.Dispose()}}}finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item -LiteralPath $archive).Length;sha256=(Get-FileHash -LiteralPath $archive).Hash}|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
