$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/page-color-source-20261026.zip'
if(Test-Path $archive){throw 'Preserve source checkpoint'}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($relative in @('PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','LeaForge/COMPATIBILITY.md','LeaForge/PAGE_COLORS.md','Lea.Firespider/PageColor.vb','Lea.Build/New-PageColorSource.ps1','Lea.Build/New-PageColorHarness.ps1','Lea.Build/Test-PageColorHost.ps1','Lea.Build/Resume-PageColorObservation.ps1','Lea.Build/Audit-LegacyHostTools.ps1','Lea.Build/Backup-PageColorWork.ps1')){
    $null=$paths.Add((Join-Path $workspace $relative))
}
foreach($folder in Get-ChildItem (Join-Path $workspace 'continuity-evidence') -Directory|Where-Object{$_.Name -like 'page-color-host-v*' -or $_.Name -eq 'legacy-host-tools-audit-v1'}){
    foreach($file in Get-ChildItem $folder.FullName -Recurse -File){
        $relative=[IO.Path]::GetRelativePath($folder.FullName,$file.FullName)
        if($relative -match '(^|\\)obj\\'){continue}
        if($relative -match '^(runtime|host-bin)\\' -and $relative -notmatch '^(runtime|host-bin)\\(Firespider Internet 19\.exe|Firespider Internet 19\.pdb|Lea\.Firespider\.Boot\.exe)$'){continue}
        if($relative -match '^host-source\\Lea\\' -and $file.Extension -in @('.dll','.exe')){continue}
        $null=$paths.Add($file.FullName)
    }
}
$references=@(
    @{path=(Join-Path $workspace 'continuity-evidence/library-and-trace-source-20261026.zip');sha256='A1515801B6A9696951BA07F874CAAD976E4A7896DAAD18F85D8138F6B8E38AC0'},
    @{path=(Join-Path $workspace 'dist/LeaExes/Firespider-Lea-library-development-20261026.zip');sha256='A87429089BCC81724C133355EADD689E91D8ABF00B547D9C154FE2CC33AD1D17'},
    @{path=(Join-Path $workspace 'continuity-evidence/owned-full-suite-source-20261026.zip');sha256='4EE65BA9517F4D1DA0A1ACE438ECC7D97CCD68DA69B3D9389998790B71E6F74C'}
)
$leases=[Collections.Generic.List[IDisposable]]::new();$members=[Collections.Generic.List[object]]::new()
try{
    foreach($reference in $references){$s=[IO.File]::Open($reference.path,'Open','Read','Read');$leases.Add($s);if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $reference.sha256){throw 'Reference differs'}}
    foreach($path in $paths){$s=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($s);$members.Add(@{entry=$path.Substring($workspace.Length+1).Replace('\','/');sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));stream=$s});$s.Position=0}
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{
        foreach($member in $members){$s=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$member.stream.CopyTo($s)}finally{$s.Dispose()}}
        $writer=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$writer.Write((@{scope='Page-color work, source audit, matched diagnostic source/binaries and retained failures. Not release or complete portable SDK. Unchanged source/runtime/ownership dependencies separately pinned; no nested archives.';references=$references;members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 6))}finally{$writer.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{
        if($zip.Entries.Count -ne $members.Count+1){throw 'Inventory differs'}
        foreach($member in $members){$s=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $member.sha256){throw 'Readback differs'}}finally{$s.Dispose()}}
    }finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item $archive).Length;sha256=(Get-FileHash $archive).Hash}|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
