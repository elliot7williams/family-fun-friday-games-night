$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/embedding-modern-worker-source-20261026.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve embedding checkpoint'}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($name in @('PROJECT_CONTEXT.md','LeaForge/FINISH_PLAN.md','LeaForge/history/FINISH_PLAN_20261026_before_consolidation.md','LeaForge/PROGRESS.md','LeaForge/INTEGRATION.md','Lea.Build/Test-EmbedModernWorker.ps1','Lea.Build/Backup-EmbedModernWorker.ps1')){$null=$paths.Add((Join-Path $workspace $name))}
foreach($file in Get-ChildItem -LiteralPath (Join-Path $workspace 'Lea.Embed.Sample') -File){$null=$paths.Add($file.FullName)}
foreach($relative in @('embedding-modern-worker-v1','embedding-modern-worker-v2')){
    $folder=Join-Path $workspace ('continuity-evidence/'+$relative)
    foreach($file in Get-ChildItem -LiteralPath $folder -File){$null=$paths.Add($file.FullName)}
    $source=Join-Path $folder 'source';if(Test-Path -LiteralPath $source){foreach($file in Get-ChildItem -LiteralPath $source -File){$null=$paths.Add($file.FullName)}}
}
$runtime=Join-Path $workspace 'continuity-evidence/embedding-modern-worker-v2/runtime'
foreach($name in @('Lea.Embed.Sample.exe','Lea.Embed.Sample.exe.config','ScriptWorker/Lea.Script.Worker.deps.json','ScriptWorker/Lea.Script.Worker.runtimeconfig.json')){$null=$paths.Add((Join-Path $runtime $name))}
$pdb=Join-Path $runtime 'Lea.Embed.Sample.pdb';if(Test-Path -LiteralPath $pdb){$null=$paths.Add($pdb)}
$prior=Join-Path $workspace 'continuity-evidence/current-native-host-checkpoint-20261026.zip'
$priorHash='52B691AB2E3CED8614E9D7BBBD3DA9D2E75F82E0BCCDCB1C5D4B59A271030B43'
if((Get-FileHash -LiteralPath $prior).Hash -cne $priorHash){throw 'Current source reference differs'}
$members=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
try{
    $leases.Add([IO.File]::Open($prior,'Open','Read','Read'))
    foreach($path in $paths){$stream=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($stream);$members.Add(@{entry=$path.Substring($workspace.Length+1).Replace('\','/');sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream));stream=$stream});$stream.Position=0}
    $zip=[IO.Compression.ZipFile]::Open($archive,[IO.Compression.ZipArchiveMode]::Create)
    try{
        foreach($member in $members){$output=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$member.stream.CopyTo($output)}finally{$output.Dispose()}}
        $writer=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$writer.Write((@{scope='Thin local current/tested embedding source, binary and evidence checkpoint. Runtime dependencies and prior sources remain separately retained; not portable/off-device or script/release qualification.';references=@(@{path=$prior;sha256=$priorHash});members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 6))}finally{$writer.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{if($zip.Entries.Count -ne $members.Count+1){throw 'Embedding checkpoint inventory differs'};foreach($member in $members){$input=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($input)) -cne $member.sha256){throw 'Embedding checkpoint member differs'}}finally{$input.Dispose()}}}finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item -LiteralPath $archive).Length;sha256=(Get-FileHash -LiteralPath $archive).Hash}|ConvertTo-Json
}finally{foreach($lease in $leases){$lease.Dispose()}}
