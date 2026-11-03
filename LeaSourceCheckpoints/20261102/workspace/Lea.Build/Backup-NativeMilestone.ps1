$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/native-milestone-20261026.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve native milestone checkpoint'}
$paths=[Collections.Generic.List[string]]::new()
foreach($name in @('PROJECT_CONTEXT.md','Lea.Tests/Smoke.cs','Lea.Build/New-NestedOverlapSmokeSource.ps1','Lea.Build/Prepare-NestedOverlapRegression.ps1','Lea.Build/Test-NestedOverlapRegression.generated.ps1','Lea.Build/Prepare-NestedOverlapAudit.ps1','Lea.Build/Audit-NestedOverlapAcls.generated.ps1','Lea.Build/Audit-NestedOverlapOutcome.ps1','Lea.Build/Test-CurrentTitleVisuals.ps1','Lea.Build/Backup-NativeMilestone.ps1','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','LeaForge/TITLE_METADATA.md','LeaForge/HIT_GEOMETRY.md','LeaForge/CURRENT_HOST.md')){$paths.Add((Join-Path $workspace $name))}
foreach($relative in @('continuity-evidence/nested-overlap-regression-v1','continuity-evidence/current-title-visuals-v1')){
    $folder=Join-Path $workspace $relative
    foreach($file in Get-ChildItem -LiteralPath $folder -File){$paths.Add($file.FullName)}
    foreach($sub in @('runtime','images')){$child=Join-Path $folder $sub;if(Test-Path -LiteralPath $child){foreach($file in Get-ChildItem -LiteralPath $child -File){$paths.Add($file.FullName)}}}
}
foreach($file in Get-ChildItem -LiteralPath (Join-Path $workspace 'continuity-evidence/current-hit-geometry-v3/view-source') -File){$paths.Add($file.FullName)}
$references=@(
    @{path=(Join-Path $workspace 'continuity-evidence/current-hit-geometry-source-v1.zip');sha256='0F81C395F5BBF11ECC97DFE013E1F1778546206CDCEE6834A4B6C57953E83815'},
    @{path=(Join-Path $workspace 'continuity-evidence/storage-and-overlap-source-20261026.zip');sha256='17CEC711DF32A09E2FCA7912720079C5CD03E10FF56F53F575C9CC824F097C19'}
)
$members=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
try{
    foreach($reference in $references){$stream=[IO.File]::Open($reference.path,'Open','Read','Read');$leases.Add($stream);if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $reference.sha256){throw 'Referenced source archive differs'}}
    foreach($path in $paths){
        $stream=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($stream)
        $members.Add(@{entry=$path.Substring($workspace.Length+1).Replace('\','/');sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream));stream=$stream});$stream.Position=0
    }
    $zip=[IO.Compression.ZipFile]::Open($archive,[IO.Compression.ZipArchiveMode]::Create)
    try{
        foreach($member in $members){$output=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$member.stream.CopyTo($output)}finally{$output.Dispose()}}
        $writer=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$writer.Write((@{scope='Thin local source/evidence checkpoint. Previous complete source archives retained separately by reference, not copied again. Not a portable/off-device kit or release certification.';references=$references;members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 6))}finally{$writer.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{
        if($zip.Entries.Count -ne $members.Count+1){throw 'Milestone inventory differs'}
        foreach($member in $members){$input=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($input)) -cne $member.sha256){throw 'Milestone member differs'}}finally{$input.Dispose()}}
    }finally{$zip.Dispose()}
    @{verified=$true;sha256=(Get-FileHash -LiteralPath $archive).Hash;entries=$members.Count+1;bytes=(Get-Item -LiteralPath $archive).Length;scope='Thin local checkpoint; references retained separately'}|ConvertTo-Json
}finally{foreach($lease in $leases){$lease.Dispose()}}
