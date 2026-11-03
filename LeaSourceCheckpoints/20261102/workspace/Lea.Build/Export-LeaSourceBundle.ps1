$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$out=Join-Path $workspace 'continuity-evidence/source-bundle-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve source bundle'}
if((Get-PSDrive C).Free -lt 768MB){throw 'Source bundle reserve required'}
$leases=[Collections.Generic.List[IDisposable]]::new();$entries=[Collections.Generic.List[object]]::new();$names=@{}
$null=New-Item -ItemType Directory -Path $out
$archivePath=Join-Path $out 'Lea-source-and-build-evidence.zip'
$zip=[IO.Compression.ZipFile]::Open($archivePath,'Create')
function AddPinned([string]$Relative,[string]$Destination,[string]$Expected=''){
    if($names.ContainsKey($Destination)){throw 'Duplicate bundle destination'}
    $names[$Destination]=$true
    $path=[IO.Path]::GetFullPath((Join-Path $workspace $Relative))
    if(!$path.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Source outside workspace'}
    $walk=$path
    while($walk){if((Get-Item -LiteralPath $walk -Force).Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Linked source refused'};$walk=[IO.Path]::GetDirectoryName($walk)}
    $stream=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($stream)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream));$stream.Position=0
    if($Expected -and $Expected -cne $hash){throw "Source input changed: $Relative"}
    $entry=$zip.CreateEntry($Destination,[IO.Compression.CompressionLevel]::Optimal);$target=$entry.Open()
    try{$stream.CopyTo($target)}finally{$target.Dispose()}
    $entries.Add(@{path=$Destination;sourcePath=$Relative;sha256=$hash;length=$stream.Length})
}
function AddText([string]$Name,[string]$Text){
    $entry=$zip.CreateEntry($Name);$target=$entry.Open()
    try{$bytes=[Text.Encoding]::UTF8.GetBytes($Text);$target.Write($bytes)}finally{$target.Dispose()}
}
$result=@{passed=$false;error=$null;compilerExecuted=$false;runtimeExecuted=$false;portableBuildVerified=$false;releaseCertified=$false}
try{
    foreach($spec in @(
        @('tested-native-source-v1.zip','A5DAC85239EB06335323454696E7B8F9292859217934599989799E8752B8B158'),
        @('tested-parent-view-source-v1.zip','F6B770D3AB46AFC1FD6D9DF4D65CD272FA365D6494B9B647DD0DED77A5F12599'),
        @('tested-worker-source-v1.zip','25516AAAEDDEE8CF5FF058F343D8485F1AB8565C87FB546A7DC5F2A440863A79'),
        @('tested-interpreter-inputs-v1.zip','E8F3D3CDF7F25589A8E7FDBFEEC15732C46310444147648C7F8DF0EF84F58E1C'),
        @('tested-parser-source-v1.zip','326B4A7349F99ECB103E4F7371AC52F53BB562FD257FF03AC50202861279646A')
    )){AddPinned ('continuity-evidence/'+$spec[0]) ('source-archives/'+$spec[0]) $spec[1]}
    AddPinned 'continuity-evidence/engine-recovery-2026-10-14/prototype-inputs-20261015-180833-722/Firespider-Lea-146-script-host-source_2026-10-14_15-36-41.zip' 'source-archives/Firespider-host-source.zip' '1E6AA708C94F4D1E19B8EC832258F605E08310FEFAA646E01B12C894E27919F4'
    foreach($name in @('W.cs','AssemblyInfo.cs','Lea.Script.Win.csproj','packages.lock.json')){AddPinned ('continuity-evidence/script-adapter-source-build-v2/'+$name) ('adaptor-source/'+$name)}
    foreach($name in @('Lea.Script.Worker.runtimeconfig.json','Lea.Script.Worker.deps.json')){AddPinned ('continuity-evidence/worker-launcher-rebuild-v1/configuration-source/'+$name) ('worker-configuration-source/'+$name)}
    foreach($name in @(
        'Build-ArchivedNative.generated.ps1','Compare-ArchivedNative.ps1','IteratorDisposalProbe.cs','Test-IteratorDisposal.ps1',
        'Build-ArchivedParentView.ps1','Compare-ArchivedParentView.ps1','Build-PreservedScriptAdapter.ps1','Compare-PreservedScriptAdapter.ps1','AdapterAssemblyInfo.cs',
        'Build-PreservedFirespiderHost.ps1','Build-ArchivedWorker.ps1','Prepare-ArchivedNativeWorker.ps1','Build-ArchivedNativeWorkerV2.generated.ps1','Audit-ArchivedNativeWorker.ps1','Build-PreservedWorkerLauncher.ps1',
        'New-InterpreterBuildMap.ps1','Build-MappedInterpreter.ps1','Compile-MappedInterpreter.ps1','Audit-MappedInterpreterRebuild.ps1','Build-MappedNativeInterpreter.ps1',
        'Prepare-MappedParserBuild.ps1','Build-MappedParser.generated.ps1','Compile-MappedParser.ps1','Audit-MappedParserRebuild.ps1','Prepare-MappedNativeParser.ps1','Build-MappedNativeParser.generated.ps1',
        'Invoke-PublicCompiler.20261022.ps1','PublicCompiler.cs','Build-TestedRuntimePackage.ps1','Expand-VerifiedRuntime.ps1',
        'Invoke-LeaPreflight.ps1','Test-RuntimePrerequisites.ps1','Test-RuntimePrerequisites.Tests.ps1','PORTABLE_RUNTIME.md','README.md','Export-LeaSourceBundle.ps1'
    )){AddPinned ('Lea.Build/'+$name) ('build-scripts/'+$name)}
    foreach($spec in @(
        @('worker-archive-build-v1/result.json','15A9162F4B9E06F0D8455FC05D2250C62DC57127BACF56F9CAB6FC39FCADA75D'),
        @('worker-archive-native-v2/result.json','41E53BFC9FDE20D00CD2398832DD4F397A862C7D6E8A198E90D35D1FC7AF1404'),
        @('worker-archive-native-v1/result.json','A52E8CCEE757DAF7B5ED66DD4816A3BDC9BA962C238E67EA0E08479F365A5ED6'),
        @('worker-archive-native-v1/rebuild-verification.json','70EE1BEFACD114950C2DF9D8FEC39C98026BD9B24CF4C9485249DAEE9D77253B'),
        @('worker-launcher-rebuild-v1/result.json','B1DA2AE737893549D358A46E9C8A9DA06FBD59B1AFF4B9BB1ECD54D300C52557'),
        @('script-adapter-source-build-v2/result.json','7178C265B7BCAF50738DC8F610432F4C6B282DAB77442D689D06437CD3BD3E35'),
        @('script-adapter-source-build-v2/comparison.json','43731F9A2ECBB3C4185741A6DD14206DAC649755966F3E6091D517D4BCAE36D6'),
        @('firespider-source-rebuild-v1/result.json','9DDF92B7273943C6CB5481A8CCC8033F09053BE2C5968DC5E53EDA14E0A5F3DA'),
        @('interpreter-mapped-native-v1/result.json','CEB088149D4ED8C7103AFCD31C714E3245517E9C1DF8C5F1A486030EF0331F90'),
        @('parser-mapped-native-v1/result.json','81D8921C4B76A15A671DA06C547558D4E8123590E82603301467CCC7BE304999'),
        @('temporal-startup-v1/host-package-extracted-v1/result.json','0FBA544D311F22A84E046CFE99F97D381AB85D1FE936851CDDFB3833386823BD'),
        @('interpreter-ordered-map-v1/build-map.json','08C28F248AE2CC7BEC293E4EECEB455F0A530ADE1370B1298D59BA1EE1B9DC91'),
        @('tested-parser-source-v1/manifest.json','CA40B407F75DD28DA97CF4570CB4D52B6ACD66D01631D98FD576659E4D6054DF')
    )){AddPinned ('continuity-evidence/'+$spec[0]) ('evidence/'+$spec[0]) $spec[1]}
    foreach($name in @('PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','LeaForge/COMPATIBILITY.md')){AddPinned $name ('checkpoint/'+$name)}
    AddPinned 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1' 'build-support/Test-CompositeCandidate.v2.ps1' 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'
    AddPinned 'continuity-evidence/bootstrap-descriptor-batches-v1/ParentResourceIlAudit.cs' 'build-support/ParentResourceIlAudit.cs' 'B2438457F0CA2F9F54A9EB866335DF7EF23D50FB84F134F13B968C4FB6F557CC'
    AddPinned 'continuity-evidence/modern-worker-v1/InterpreterAssemblyAudit.cs' 'build-support/InterpreterAssemblyAudit.cs' '372098D022AF9E29B14CFF5AE57574F101B333AFFE4F94019720D315E443DAB9'
    $scope='Local source preservation bundle, not a portable build kit or release. Six source archives plus adaptor/configuration source, selected recipes/evidence/checkpoints. Scripts retain original paths and depend on external compiler/runtime/reference binaries and additional workspace evidence/test fixtures not fully included. Original source notices retained; license/redistribution review pending. Not a GitHub/off-device backup or clean-PC build verification. No claim that all archived sources were built together into the latest candidate.'
    $manifest=@{format=1;scope=$scope;files=$entries;remaining=@('Relocatable build orchestration and complete dependency closure','External compiler/reference/runtime provenance and license review','Full matched test harness and fixture packaging','Image/source toolchain follow-through','Clean-PC build/launch validation','Broad compatibility, startup reliability and security release gates')}
    $json=$manifest|ConvertTo-Json -Depth 7
    AddText 'manifest.json' $json;AddText 'README.txt' $scope
    [IO.File]::WriteAllText((Join-Path $out 'manifest.json'),$json,[Text.UTF8Encoding]::new($false))
    $zip.Dispose();$zip=$null
    $verify=[IO.Compression.ZipFile]::OpenRead($archivePath)
    try{
        if($verify.Entries.Count -ne $entries.Count+2){throw 'Bundle entry count differs'}
        foreach($entry in $entries){
            $member=$verify.GetEntry($entry.path)
            if(!$member -or $member.Length -ne $entry.length){throw 'Bundle entry missing/truncated'}
            $stream=$member.Open()
            try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
            if($hash -cne $entry.sha256){throw 'Bundle member hash differs'}
        }
    }finally{$verify.Dispose()}
    $result.archive=$archivePath;$result.sha256=(Get-FileHash -LiteralPath $archivePath).Hash
    $result.files=$entries.Count;$result.bytes=(Get-Item -LiteralPath $archivePath).Length;$result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    if($zip){$zip.Dispose()}
    foreach($lease in $leases){$lease.Dispose()}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 5))
}
[pscustomobject]$result|ConvertTo-Json -Depth 5
