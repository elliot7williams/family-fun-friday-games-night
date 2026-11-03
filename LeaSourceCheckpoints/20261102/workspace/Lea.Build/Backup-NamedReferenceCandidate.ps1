$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/named-references-v1'
$proof=Join-Path $root 'result.json'
if((Get-FileHash -LiteralPath $proof).Hash -cne 'D2B7756C2DE57DA73F6F32C7097CAE52F3FF0000D9F4DD4B05C6FF6D6205FF70'){throw 'Candidate receipt differs'}
$archive=Join-Path $root 'candidate-source.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve source archive'}
$members=[Collections.Generic.List[object]]::new()
foreach($line in Get-Content -LiteralPath (Join-Path $root 'compile-engine.rsp')){
    if($line -match '^"(.+\.cs)"$'){
        $path=[IO.Path]::GetFullPath($Matches[1])
        if(!$path.StartsWith((Join-Path $root 'source')+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Source escapes candidate'}
        $members.Add(@{path=$path;entry='source/'+[IO.Path]::GetFileName($path)})
    }
}
if($members.Count -ne 33){throw 'Compiled source inventory differs'}
foreach($name in @('NamedReferences.cs','NumericReferences.cs','entities.whatwg.json','entities.expected.tsv','result.json','compile-engine.rsp')){$members.Add(@{path=(Join-Path $root $name);entry=$name})}
foreach($name in @('Generate-HtmlNames.ps1','Prepare-NamedReferenceCandidate.ps1','Build-NamedReferenceCandidate.generated.ps1','Prepare-NamedReferenceRegression.ps1','Backup-NamedReferenceCandidate.ps1')){$members.Add(@{path=(Join-Path $PSScriptRoot $name);entry='recipes/'+$name})}
$zip=[IO.Compression.ZipFile]::Open($archive,[IO.Compression.ZipArchiveMode]::Create)
try{
    foreach($member in $members){
        $member.sha256=(Get-FileHash -LiteralPath $member.path).Hash
        $entry=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal)
        $input=[IO.File]::OpenRead($member.path);$output=$entry.Open()
        try{$input.CopyTo($output)}finally{$input.Dispose();$output.Dispose()}
    }
    $entry=$zip.CreateEntry('manifest.json');$writer=[IO.StreamWriter]::new($entry.Open(),[Text.UTF8Encoding]::new($false))
    try{$writer.Write((@{members=$members;scope='Exact compiled engine candidate source plus probes, standard input and local recipes; external references/toolchain not included; not a full portable browser build kit'}|ConvertTo-Json -Depth 5))}finally{$writer.Dispose()}
}finally{$zip.Dispose()}
$zip=[IO.Compression.ZipFile]::OpenRead($archive)
try{
    if($zip.Entries.Count -ne $members.Count+1){throw 'Archive inventory differs'}
    foreach($member in $members){
        $stream=$zip.GetEntry($member.entry).Open()
        try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($hash -cne $member.sha256 -or (Get-FileHash -LiteralPath $member.path).Hash -cne $member.sha256){throw 'Source archive member differs'}
    }
}finally{$zip.Dispose()}
$result=@{verified=$true;entries=$members.Count+1;compiledSources=33;sha256=(Get-FileHash -LiteralPath $archive).Hash;bytes=(Get-Item -LiteralPath $archive).Length;portableBuildKit=$false;offDeviceBackup=$false}
[IO.File]::WriteAllText((Join-Path $root 'source-backup.json'),($result|ConvertTo-Json))
$result|ConvertTo-Json
