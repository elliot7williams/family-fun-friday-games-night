$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$out=Join-Path $root 'named-reference-qualification-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve qualification source backup'}
$members=[Collections.Generic.List[object]]::new()
$suite=Join-Path $root 'named-references-script-suite-v1'
foreach($file in Get-ChildItem -LiteralPath (Join-Path $suite 'source') -Recurse -File){
    $members.Add(@{path=$file.FullName;entry='tests/'+[IO.Path]::GetRelativePath((Join-Path $suite 'source'),$file.FullName).Replace('\','/')})
}
if($members.Count -ne 60){throw 'Matched test source inventory differs'}
foreach($name in @('build.ps1','WorkerEntryNativeHost.cs','relocated-fixture.json','relocated-entry-host.rsp','tests.rsp','result.json','binding-verification.json')){$members.Add(@{path=(Join-Path $suite $name);entry='suite/'+$name})}
foreach($pair in @(@('host-named-references-v1','host'),@('named-references-script-run-v1','full-script'))){
    foreach($name in @('runner.ps1','result.json')){$members.Add(@{path=(Join-Path $root ($pair[0]+'/'+$name));entry=$pair[1]+'/'+$name})}
}
foreach($name in @('Prepare-NamedReferenceHost.ps1','Prepare-NamedScriptSuite.ps1','Initialize-NamedScriptSuite.generated.ps1','Prepare-NamedScriptVerification.ps1','Verify-NamedScriptSuite.generated.ps1','Prepare-NamedScriptRun.ps1','Audit-NamedScriptResult.ps1','Backup-NamedQualification.ps1')){$members.Add(@{path=(Join-Path $PSScriptRoot $name);entry='recipes/'+$name})}
$members.Add(@{path=(Join-Path $root 'named-references-script-run-v1/verification.json');entry='full-script/verification.json'})
$members.Add(@{path=(Join-Path $workspace 'continuity-evidence/named-references-v1/candidate-source.zip');entry='candidate-engine-source.zip'})
foreach($member in $members){$member.sha256=(Get-FileHash -LiteralPath $member.path).Hash}
$null=New-Item -ItemType Directory -Path $out
$archive=Join-Path $out 'source-and-qualification.zip'
$zip=[IO.Compression.ZipFile]::Open($archive,[IO.Compression.ZipArchiveMode]::Create)
try{
    foreach($member in $members){
        $entry=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal)
        $input=[IO.File]::OpenRead($member.path);$output=$entry.Open()
        try{$input.CopyTo($output)}finally{$input.Dispose();$output.Dispose()}
    }
    $entry=$zip.CreateEntry('manifest.json');$writer=[IO.StreamWriter]::new($entry.Open(),[Text.UTF8Encoding]::new($false))
    try{$writer.Write((@{members=$members;scope='Exact matched tests, engine-source archive, local recipes and qualification receipts; not all browser sources, external dependencies, logs or runtime toolchain; local backup only'}|ConvertTo-Json -Depth 5))}finally{$writer.Dispose()}
}finally{$zip.Dispose()}
$zip=[IO.Compression.ZipFile]::OpenRead($archive)
try{
    if($zip.Entries.Count -ne $members.Count+1){throw 'Qualification archive inventory differs'}
    foreach($member in $members){
        $stream=$zip.GetEntry($member.entry).Open()
        try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($hash -cne $member.sha256 -or (Get-FileHash -LiteralPath $member.path).Hash -cne $member.sha256){throw 'Qualification archive bytes differ'}
    }
}finally{$zip.Dispose()}
$result=@{verified=$true;entries=$members.Count+1;bytes=(Get-Item -LiteralPath $archive).Length;sha256=(Get-FileHash -LiteralPath $archive).Hash;offDeviceBackup=$false;portableBuildKit=$false}
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json))
$result|ConvertTo-Json
