$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/ordered-flow-regression-v3'
$archive=Join-Path $workspace 'continuity-evidence/ordered-flow-regression-v3-source.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve regression source evidence'}
$r=Get-Content -LiteralPath (Join-Path $root 'result.json') -Raw|ConvertFrom-Json
if(!$r.inputsUnchanged -or !$r.candidateBytesUnchanged -or $r.ownerAttention){throw 'Regression evidence incomplete'}
$members=[Collections.Generic.List[object]]::new()
foreach($name in @('result.json','runner.ps1','full-engine-traced.stdout.txt','full-engine-traced.stderr.txt','full-engine-traced.result.json')){
    $members.Add(@{path=(Join-Path $root $name);entry='regression/'+$name})
}
foreach($pair in @(
    @('continuity-evidence/ordered-flow-v6/candidate-source.zip','core-source.zip','8B34DBB53FA1C4122A8CE7A448D3BA583D57241B1BC200A0E2628825F404F115'),
    @('continuity-evidence/decoder-owner-source-v1.zip','view-source-and-evidence.zip','A253F1718C907EB63ED376D38190CCB612628F70D57A475DF75DB8B8B1D716EA')
)){
    $path=Join-Path $workspace $pair[0]
    if((Get-FileHash -LiteralPath $path).Hash -cne $pair[2]){throw 'Matching source archive differs'}
    $members.Add(@{path=$path;entry=$pair[1]})
}
foreach($name in @('Prepare-OrderedFlowV5Regression.ps1','Test-OrderedFlowV5Regression.generated.ps1','Backup-OrderedFlowRegressionV3.ps1')){
    $members.Add(@{path=(Join-Path $PSScriptRoot $name);entry='recipes/'+$name})
}
foreach($member in $members){$member.sha256=(Get-FileHash -LiteralPath $member.path).Hash}
$zip=[IO.Compression.ZipFile]::Open($archive,[IO.Compression.ZipArchiveMode]::Create)
try{
    foreach($member in $members){
        $input=[IO.File]::OpenRead($member.path);$output=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open()
        try{$input.CopyTo($output)}finally{$input.Dispose();$output.Dispose()}
    }
    $writer=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open(),[Text.UTF8Encoding]::new($false))
    try{$writer.Write((@{members=$members;scope='Exact matched candidate sources and retained full regression evidence; local-only, no portable toolchain or release certification'}|ConvertTo-Json -Depth 5))}finally{$writer.Dispose()}
}finally{$zip.Dispose()}
$zip=[IO.Compression.ZipFile]::OpenRead($archive)
try{
    if($zip.Entries.Count -ne $members.Count+1){throw 'Regression archive inventory differs'}
    foreach($member in $members){
        $stream=$zip.GetEntry($member.entry).Open()
        try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($hash -cne $member.sha256 -or (Get-FileHash -LiteralPath $member.path).Hash -cne $member.sha256){throw 'Regression archive member differs'}
    }
}finally{$zip.Dispose()}
$result=@{verified=$true;entries=$members.Count+1;bytes=(Get-Item -LiteralPath $archive).Length;sha256=(Get-FileHash -LiteralPath $archive).Hash;offDeviceBackup=$false;portableBuildKit=$false;releaseCertified=$false}
[IO.File]::WriteAllText((Join-Path $root 'source-backup.json'),($result|ConvertTo-Json))
$result|ConvertTo-Json
