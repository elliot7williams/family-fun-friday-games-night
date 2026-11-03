$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/current-native-host-checkpoint-20261026.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve host checkpoint'}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($name in @('PROJECT_CONTEXT.md','LeaForge/CURRENT_HOST.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','Lea.Build/Prepare-CurrentNativeHost.ps1','Lea.Build/Test-CurrentNativeHost.generated.ps1','Lea.Build/Prepare-CurrentNativeScriptSuite.ps1','Lea.Build/Initialize-CurrentNativeScriptSuite.generated.ps1','Lea.Build/Build-CurrentNativeScriptSuite.generated.ps1','Lea.Build/Prepare-CurrentNativeScriptVerification.ps1','Lea.Build/Verify-CurrentNativeScriptSuite.generated.ps1','Lea.Build/Prepare-CurrentNativeScriptRun.ps1','Lea.Build/Test-CurrentNativeScriptSuite.generated.ps1','Lea.Build/Test-ComponentProbePrerequisites.ps1','Lea.Build/Test-ComponentProbePrerequisites.Tests.ps1','Lea.Build/Prepare-ComponentPreflightRun.ps1','Lea.Build/Test-CurrentNativeScriptSuiteV2.generated.ps1','Lea.Build/Backup-CurrentNativeHost.ps1')){$null=$paths.Add((Join-Path $workspace $name))}
foreach($relative in @('host-current-native-v1','current-native-script-suite-v1','current-native-script-run-v1')){
    $folder=Join-Path $workspace ('continuity-evidence/temporal-startup-v1/'+$relative)
    foreach($file in Get-ChildItem -LiteralPath $folder -Recurse -File){
        $inside=$file.FullName.Substring($folder.Length+1)
        if($inside.StartsWith('runtime\',[StringComparison]::OrdinalIgnoreCase) -and $file.Extension -in @('.dll','.exe','.pdb')){continue}
        $null=$paths.Add($file.FullName)
    }
}
$buildRoot=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/current-native-script-suite-v1'
$b=Get-Content -LiteralPath (Join-Path $buildRoot 'result.json') -Raw|ConvertFrom-Json
if(!$b.passed -or !$b.inputsUnchanged -or $b.ownerAttention){throw 'Matched build checkpoint incomplete'}
foreach($name in @('Lea.Script.Tests.exe','Lea.Script.Tests.pdb','WorkerEntryProbe/WorkerEntryNativeHost.exe')){$null=$paths.Add((Join-Path $b.runtime $name))}
foreach($file in $b.fixtureFiles){$null=$paths.Add($file.path)}
foreach($file in Get-ChildItem -LiteralPath (Join-Path $workspace 'continuity-evidence/component-host-prerequisites-v1') -File){$null=$paths.Add($file.FullName)}
$references=@(
    @{path=(Join-Path $workspace 'continuity-evidence/native-milestone-20261026.zip');sha256='4191B17DFAE36345AC92E4EF3022CF6EE13BC80A38B40561B1BED4893AB5A63A'},
    @{path=(Join-Path $workspace 'continuity-evidence/current-host-qualification-v1.zip');sha256='048CC1C0193611366077FC9D2030E1A0DD326F30BF27122DDE8076C5E5453989'}
)
$members=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
try{
    foreach($reference in $references){$stream=[IO.File]::Open($reference.path,'Open','Read','Read');$leases.Add($stream);if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $reference.sha256){throw 'Prior source reference differs'}}
    foreach($path in $paths){$stream=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($stream);$members.Add(@{entry=$path.Substring($workspace.Length+1).Replace('\','/');sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream));stream=$stream});$stream.Position=0}
    $zip=[IO.Compression.ZipFile]::Open($archive,[IO.Compression.ZipArchiveMode]::Create)
    try{
        foreach($member in $members){$output=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$member.stream.CopyTo($output)}finally{$output.Dispose()}}
        $writer=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$writer.Write((@{scope='Thin local host/script source and evidence checkpoint. Unchanged runtime dependencies remain in the retained candidates and referenced archives. Not portable/off-device or release certified.';references=$references;members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 6))}finally{$writer.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{if($zip.Entries.Count -ne $members.Count+1){throw 'Checkpoint inventory differs'};foreach($member in $members){$input=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($input)) -cne $member.sha256){throw 'Checkpoint member differs'}}finally{$input.Dispose()}}}finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item -LiteralPath $archive).Length;sha256=(Get-FileHash -LiteralPath $archive).Hash}|ConvertTo-Json
}finally{foreach($lease in $leases){$lease.Dispose()}}
