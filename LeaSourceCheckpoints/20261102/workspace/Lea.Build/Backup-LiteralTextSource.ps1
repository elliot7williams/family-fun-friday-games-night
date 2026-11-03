$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/named-reference-visuals-v4'
$proof=Join-Path $root 'result.json'
if((Get-FileHash -LiteralPath $proof).Hash -cne '168AA864BF848B97C75DAEDC72E1F595D231CB5121732F556840D64693C7DD14'){throw 'Literal view receipt differs'}
$archive=Join-Path $root 'literal-text-source.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve literal source backup'}
$members=[Collections.Generic.List[object]]::new()
foreach($file in Get-ChildItem -LiteralPath (Join-Path $root 'view-source') -File){$members.Add(@{path=$file.FullName;entry='view-source/'+$file.Name})}
if($members.Count -ne 6){throw 'Compiled view source inventory differs'}
foreach($name in @('NamedVisuals.cs','named-references.html','runner.ps1','view.rsp','compile.rsp','result.json','visual-review.json')){$members.Add(@{path=(Join-Path $root $name);entry=$name})}
foreach($name in @('Build-LiteralTextView.ps1','Backup-LiteralTextSource.ps1','Prepare-LiteralTextRegression.ps1')){$members.Add(@{path=(Join-Path $PSScriptRoot $name);entry='recipes/'+$name})}
foreach($file in Get-ChildItem -LiteralPath (Join-Path $root 'images') -Filter '*.png'){$members.Add(@{path=$file.FullName;entry='images/'+$file.Name})}
$members.Add(@{path=(Join-Path $workspace 'continuity-evidence/named-references-v1/candidate-source.zip');entry='named-core-source.zip'})
foreach($member in $members){$member.sha256=(Get-FileHash -LiteralPath $member.path).Hash}
$zip=[IO.Compression.ZipFile]::Open($archive,[IO.Compression.ZipArchiveMode]::Create)
try{
    foreach($member in $members){
        $entry=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal)
        $input=[IO.File]::OpenRead($member.path);$output=$entry.Open()
        try{$input.CopyTo($output)}finally{$input.Dispose();$output.Dispose()}
    }
    $entry=$zip.CreateEntry('manifest.json');$writer=[IO.StreamWriter]::new($entry.Open(),[Text.UTF8Encoding]::new($false))
    try{$writer.Write((@{members=$members;scope='Exact compiled view source, nested named-core source archive, visual probe, screenshots, review and local recipes; external dependencies/toolchain and complete browser source not included; not a release bundle'}|ConvertTo-Json -Depth 5))}finally{$writer.Dispose()}
}finally{$zip.Dispose()}
$zip=[IO.Compression.ZipFile]::OpenRead($archive)
try{
    if($zip.Entries.Count -ne $members.Count+1){throw 'Literal source archive inventory differs'}
    foreach($member in $members){
        $stream=$zip.GetEntry($member.entry).Open()
        try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($hash -cne $member.sha256 -or (Get-FileHash -LiteralPath $member.path).Hash -cne $member.sha256){throw 'Literal source archive bytes differ'}
    }
}finally{$zip.Dispose()}
$result=@{verified=$true;entries=$members.Count+1;bytes=(Get-Item -LiteralPath $archive).Length;sha256=(Get-FileHash -LiteralPath $archive).Hash;portableBuildKit=$false;offDeviceBackup=$false}
[IO.File]::WriteAllText((Join-Path $root 'source-backup.json'),($result|ConvertTo-Json))
$result|ConvertTo-Json
