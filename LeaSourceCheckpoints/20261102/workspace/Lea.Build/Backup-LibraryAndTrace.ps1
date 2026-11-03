$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/library-and-trace-source-20261026.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve existing checkpoint'}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($relative in @('PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/CURRENT_HOST.md','LeaForge/FINISH_PLAN.md','LeaForge/STARTUP_IO.md','LeaForge/LIBRARY_NAVIGATION.md','Lea.Build/Backup-LibraryAndTrace.ps1','Lea.Build/New-LibraryNavigationSource.ps1','Lea.Build/New-LibraryHarnessSource.ps1','Lea.Build/Test-LibraryNavigationSource.ps1','Lea.Build/New-AboutHarnessSource.ps1','Lea.Build/New-AboutNavigationSource.ps1','Lea.Build/Test-AboutNavigation.ps1','Lea.Build/Test-CurrentNativeHost.generated.ps1','Lea.Build/Invoke-WorkerStartupTrace.ps1','Lea.Build/Test-WorkerStartupTrace.ps1','Lea.Build/New-AllStartupTraceSource.ps1','Lea.Build/Test-AllStartupTraceSource.ps1','Lea.Build/Build-CurrentNativeScriptSuite.generated.ps1','Lea.Build/Verify-CurrentNativeScriptSuite.generated.ps1','Lea.Build/Test-CurrentNativeScriptSuiteV2.generated.ps1','Lea.Build/Test-FullSuiteOutcomeControls.ps1')){$null=$paths.Add((Join-Path $workspace $relative))}
foreach($relative in @('library-navigation-source-controls-v1','library-navigation-v1','all-startup-trace-controls-v1','all-startup-trace-controls-v2','all-startup-trace-source-controls-v1','full-suite-outcome-controls-v2','temporal-startup-v1/current-native-script-suite-v6','temporal-startup-v1/current-native-script-suite-v7','temporal-startup-v1/current-native-script-run-v7','temporal-startup-v1/current-native-script-run-v8','temporal-startup-v1/host-library-current-native-v1')){
    $folder=Join-Path $workspace ('continuity-evidence/'+$relative)
    if(!(Test-Path -LiteralPath $folder)){
        if($relative -eq 'temporal-startup-v1/host-library-current-native-v1'){continue}
        throw ('Required evidence missing: '+$relative)
    }
    foreach($file in Get-ChildItem -LiteralPath $folder -File -Recurse){
        $name=[IO.Path]::GetRelativePath($folder,$file.FullName)
        if($name -match '^(runtime|host-bin)\\' -or $name -match '^host-source\\obj\\'){continue}
        $null=$paths.Add($file.FullName)
    }
    foreach($leaf in @('runtime/Lea.Script.Tests.exe','runtime/Firespider Internet 19.exe','runtime/Lea.Firespider.Boot.exe')){
        $path=Join-Path $folder $leaf
        if(Test-Path -LiteralPath $path){$null=$paths.Add($path)}
    }
}
$references=@(
    @{path=(Join-Path $workspace 'continuity-evidence/full-tree-startup-source-20261026.zip');sha256='B23E56B12FD4040659F0A19E831DB28B62879053122D337A436EE79FC85FD394'},
    @{path=(Join-Path $workspace 'continuity-evidence/about-and-cleanup-20261026.zip');sha256='67405EB185F26B790C6A272F0785B5495D916E15C5B9A0E45B5A317F72292F95'},
    @{path=(Join-Path $workspace 'dist/LeaExes/Firespider-Lea-development-20261026.zip');sha256='A3B64730267912591E099D0A85DF95D01A49149CE89AEF12579FBF4F77C7349B'}
)
$leases=[Collections.Generic.List[IDisposable]]::new();$members=[Collections.Generic.List[object]]::new()
try{
    foreach($reference in $references){$s=[IO.File]::Open($reference.path,'Open','Read','Read');$leases.Add($s);if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $reference.sha256){throw 'Retained dependency differs'}}
    foreach($path in $paths){
        $s=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($s)
        $members.Add(@{entry=$path.Substring($workspace.Length+1).Replace('\','/');sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));stream=$s});$s.Position=0
    }
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{
        foreach($member in $members){$s=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$member.stream.CopyTo($s)}finally{$s.Dispose()}}
        $writer=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$writer.Write((@{scope='Thin local matching history/bookmark host sources/binary, diagnostic full-suite sources/binaries, controls, evidence and docs. Unchanged product dependencies retained by verified references, not nested. Not portable/off-device SDK or release certification.';references=$references;members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 6))}finally{$writer.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{if($zip.Entries.Count -ne $members.Count+1){throw 'Inventory differs'};foreach($member in $members){$s=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $member.sha256){throw 'Readback differs'}}finally{$s.Dispose()}}}finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item -LiteralPath $archive).Length;sha256=(Get-FileHash -LiteralPath $archive).Hash}|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
