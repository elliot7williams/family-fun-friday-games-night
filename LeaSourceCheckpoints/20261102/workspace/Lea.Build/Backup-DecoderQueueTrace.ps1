$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/decoder-queue-trace-v1'
$archive=Join-Path $workspace 'continuity-evidence/decoder-queue-trace-source-v1.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve decoder trace source backup'}
if((Get-FileHash -LiteralPath (Join-Path $root 'result.json')).Hash -cne '6AF56EF9D858E49A60FA050CED05A2430443C4AA8588B371CAB57BDA3E50BC0D'){throw 'Queue proof differs'}
$members=[Collections.Generic.List[object]]::new()
foreach($file in Get-ChildItem -LiteralPath (Join-Path $root 'view-source') -File){$members.Add(@{path=$file.FullName;entry='view-source/'+$file.Name})}
if($members.Count -ne 6){throw 'Trace compiled source inventory differs'}
foreach($name in @('DecoderQueueTraceProof.cs','QueueProof.cs','view.rsp','compile.rsp','runner.ps1','result.json','queue-trace-proof.stdout.txt','queue-trace-proof.stderr.txt','queue-trace-proof.result.json')){$members.Add(@{path=(Join-Path $root $name);entry='controls/'+$name})}
foreach($name in @('DecoderQueueTrace.fragment.cs','New-DecoderQueueTraceSource.ps1','Prepare-DecoderQueueTrace.ps1','Build-DecoderQueueTraceView.generated.ps1','Prepare-DecoderQueueNative.ps1','Test-DecoderQueueNative.generated.ps1','Prepare-DecoderQueueFull.ps1','Test-DecoderQueueFull.generated.ps1','Prepare-DecoderNativeAudit.ps1','Audit-DecoderNativeAcls.generated.ps1','Backup-DecoderQueueTrace.ps1')){$members.Add(@{path=(Join-Path $PSScriptRoot $name);entry='recipes/'+$name})}
foreach($name in @('result.json','runner.ps1','native-pictures.stdout.txt','native-pictures.stderr.txt','native-pictures.result.json','acl-observation.json')){$members.Add(@{path=(Join-Path $workspace ('continuity-evidence/decoder-queue-native-v1/'+$name));entry='native/'+$name})}
foreach($name in @('result.json','runner.ps1','full-engine-traced.stdout.txt','full-engine-traced.stderr.txt','full-engine-traced.result.json')){$members.Add(@{path=(Join-Path $workspace ('continuity-evidence/decoder-queue-full-v1/'+$name));entry='full-failure/'+$name})}
foreach($name in @('result.json','full-core.stderr.txt')){$members.Add(@{path=(Join-Path $workspace ('continuity-evidence/ordered-flow-regression-v2/'+$name));entry='original-failure/'+$name})}
$members.Add(@{path=(Join-Path $workspace 'continuity-evidence/ordered-flow-v4/candidate-source.zip');entry='core-source.zip'})
foreach($member in $members){$member.sha256=(Get-FileHash -LiteralPath $member.path).Hash}
$zip=[IO.Compression.ZipFile]::Open($archive,[IO.Compression.ZipArchiveMode]::Create)
try{
    foreach($member in $members){
        $entry=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal)
        $input=[IO.File]::OpenRead($member.path);$output=$entry.Open()
        try{$input.CopyTo($output)}finally{$input.Dispose();$output.Dispose()}
    }
    $entry=$zip.CreateEntry('manifest.json');$writer=[IO.StreamWriter]::new($entry.Open(),[Text.UTF8Encoding]::new($false))
    try{$writer.Write((@{members=$members;scope='Exact diagnostic view source, controls, recipes, native subset and retained full failures, nested matching core source; external compiler/references/runtime and complete browser source not included; local private backup, not release qualification'}|ConvertTo-Json -Depth 5))}finally{$writer.Dispose()}
}finally{$zip.Dispose()}
$zip=[IO.Compression.ZipFile]::OpenRead($archive)
try{
    if($zip.Entries.Count -ne $members.Count+1){throw 'Trace archive inventory differs'}
    foreach($member in $members){
        $stream=$zip.GetEntry($member.entry).Open()
        try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($hash -cne $member.sha256 -or (Get-FileHash -LiteralPath $member.path).Hash -cne $member.sha256){throw 'Trace archive bytes differ'}
    }
}finally{$zip.Dispose()}
$result=@{verified=$true;entries=$members.Count+1;bytes=(Get-Item -LiteralPath $archive).Length;sha256=(Get-FileHash -LiteralPath $archive).Hash;portableBuildKit=$false;offDeviceBackup=$false;releaseCertified=$false}
[IO.File]::WriteAllText((Join-Path $root 'source-backup.json'),($result|ConvertTo-Json))
$result|ConvertTo-Json
