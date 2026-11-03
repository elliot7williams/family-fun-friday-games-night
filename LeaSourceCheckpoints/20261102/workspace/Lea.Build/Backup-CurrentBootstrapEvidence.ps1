$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$archive=Join-Path $workspace 'continuity-evidence/current-bootstrap-evidence-v1.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve current bootstrap archive'}
$members=[Collections.Generic.List[object]]::new()
foreach($relative in @('current-bootstrap-checkpoint-v1','current-bootstrap-checkpoint-v2','temporal-startup-v1/current-flow-bootstrap-cost-v1')){
    $root=Join-Path $workspace ('continuity-evidence/'+$relative)
    if($relative.EndsWith('cost-v1')){
        $r=Get-Content -LiteralPath (Join-Path $root 'result.json') -Raw|ConvertFrom-Json
        if(!$r.diagnosticCaptured -or !$r.inputsUnchanged -or !$r.candidateUnchanged -or $r.ownerAttention){throw 'Cost diagnostic incomplete'}
    }
    foreach($file in Get-ChildItem -LiteralPath $root -File -Recurse){
        $name=[IO.Path]::GetRelativePath($root,$file.FullName)
        if($name.StartsWith('runtime\')){continue}
        $members.Add(@{path=$file.FullName;entry=$relative+'/'+$name.Replace('\','/')})
    }
}
foreach($name in @('ReadBootstrapResource.cs','Audit-BootstrapCheckpointMeaning.ps1','Prepare-CurrentBootstrapCost.ps1','Test-CurrentBootstrapCost.generated.ps1','BootstrapCostObserver.cs','NativeProcessSample.cs','New-BootstrapCostProgram.ps1','Backup-CurrentBootstrapEvidence.ps1')){
    $members.Add(@{path=(Join-Path $PSScriptRoot $name);entry='recipes/'+$name})
}
$prior=Join-Path $workspace 'continuity-evidence/current-host-qualification-v1.zip'
if((Get-FileHash -LiteralPath $prior).Hash -cne '048CC1C0193611366077FC9D2030E1A0DD326F30BF27122DDE8076C5E5453989'){throw 'Prior matched source/failure archive differs'}
$members.Add(@{path=$prior;entry='prior-source-and-failures.zip'})
foreach($member in $members){$member.sha256=(Get-FileHash -LiteralPath $member.path).Hash}
$zip=[IO.Compression.ZipFile]::Open($archive,[IO.Compression.ZipArchiveMode]::Create)
try{
    foreach($member in $members){
        $input=[IO.File]::OpenRead($member.path);$output=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open()
        try{$input.CopyTo($output)}finally{$input.Dispose();$output.Dispose()}
    }
    $writer=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open(),[Text.UTF8Encoding]::new($false))
    try{$writer.Write((@{members=$members;scope='Exact checkpoint source/resource audit and diagnostic test/control sources, counters, recipes and prior source/failure archive; local-only, not a portable runtime/toolchain or release qualification'}|ConvertTo-Json -Depth 5))}finally{$writer.Dispose()}
}finally{$zip.Dispose()}
$zip=[IO.Compression.ZipFile]::OpenRead($archive)
try{
    if($zip.Entries.Count -ne $members.Count+1){throw 'Bootstrap archive inventory differs'}
    foreach($member in $members){
        $stream=$zip.GetEntry($member.entry).Open()
        try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($hash -cne $member.sha256 -or (Get-FileHash -LiteralPath $member.path).Hash -cne $member.sha256){throw 'Bootstrap archive member differs'}
    }
}finally{$zip.Dispose()}
$result=@{verified=$true;entries=$members.Count+1;bytes=(Get-Item -LiteralPath $archive).Length;sha256=(Get-FileHash -LiteralPath $archive).Hash;offDeviceBackup=$false;portableBuildKit=$false;releaseCertified=$false}
[IO.File]::WriteAllText((Join-Path $workspace 'continuity-evidence/current-bootstrap-checkpoint-v2/source-backup.json'),($result|ConvertTo-Json))
$result|ConvertTo-Json
