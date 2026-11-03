$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$archive=Join-Path $workspace 'continuity-evidence/current-host-qualification-v1.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve current qualification archive'}
$members=[Collections.Generic.List[object]]::new()
$suite=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/current-flow-script-suite-v1'
$build=Get-Content -LiteralPath (Join-Path $suite 'result.json') -Raw|ConvertFrom-Json
if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.sources.Count -ne 60){throw 'Suite build evidence incomplete'}
foreach($relative in @('temporal-startup-v1/host-current-flow-v1','split-host-diagnostic-v1','split-host-diagnostic-v2','temporal-startup-v1/current-flow-script-suite-v1','temporal-startup-v1/current-flow-script-run-v1')){
    $root=Join-Path $workspace ('continuity-evidence/'+$relative)
    $receipt=Get-Content -LiteralPath (Join-Path $root 'result.json') -Raw|ConvertFrom-Json
    if($receipt.ownerAttention){throw 'Unresolved qualification owner'}
    foreach($file in Get-ChildItem -LiteralPath $root -File -Recurse){
        $name=[IO.Path]::GetRelativePath($root,$file.FullName)
        if($name.StartsWith('runtime\') -or $name.StartsWith('references\')){continue}
        $members.Add(@{path=$file.FullName;entry=$relative+'/'+$name.Replace('\','/')})
    }
}
$hostSource=Join-Path $workspace 'continuity-evidence/firespider-source-rebuild-v1/source'
$hostFiles=@(Get-ChildItem -LiteralPath $hostSource -File -Recurse)
if($hostFiles.Count -ne 116){throw 'Retained Firespider source inventory differs'}
foreach($file in $hostFiles){$members.Add(@{path=$file.FullName;entry='firespider-source/'+[IO.Path]::GetRelativePath($hostSource,$file.FullName).Replace('\','/')})}
foreach($pair in @(
    @('continuity-evidence/firespider-source-rebuild-v1/result.json','firespider-source-build.json'),
    @('continuity-evidence/small-engine-v1/harness-build-result.json','original-harness-build.json'),
    @('continuity-evidence/small-engine-v1/source/Lea.Build/WorkerObservation.cs','harness/WorkerObservation.cs'),
    @('Lea.Firespider/Boot.cs','root-harness/Boot.cs')
)){$members.Add(@{path=(Join-Path $workspace $pair[0]);entry=$pair[1]})}
foreach($name in @('Prepare-CurrentFlowHost.ps1','Test-CurrentFlowHost.generated.ps1','Test-SplitHostDiagnostic.ps1','Prepare-SplitHostDiagnosticV2.ps1','Test-SplitHostDiagnosticV2.generated.ps1','Prepare-CurrentFlowScript.ps1','Initialize-CurrentFlowScriptSuite.generated.ps1','Build-CurrentFlowScriptSuite.generated.ps1','Prepare-CurrentFlowScriptVerification.ps1','Verify-CurrentFlowScriptSuite.generated.ps1','Prepare-CurrentFlowScriptRun.ps1','Test-CurrentFlowScriptSuite.generated.ps1','Backup-CurrentHostQualification.ps1')){
    $members.Add(@{path=(Join-Path $PSScriptRoot $name);entry='recipes/'+$name})
}
$prior=Join-Path $workspace 'continuity-evidence/decoder-launch-source-v2.zip'
if((Get-FileHash -LiteralPath $prior).Hash -cne '5A1F5B2679C4DFD855DE736002DF895D0379DB1AC88A9341348465C1B9BD2409'){throw 'Prior matched source archive differs'}
$members.Add(@{path=$prior;entry='matched-engine-view-source.zip'})
foreach($member in $members){$member.sha256=(Get-FileHash -LiteralPath $member.path).Hash}
$zip=[IO.Compression.ZipFile]::Open($archive,[IO.Compression.ZipArchiveMode]::Create)
try{
    foreach($member in $members){
        $input=[IO.File]::OpenRead($member.path);$output=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open()
        try{$input.CopyTo($output)}finally{$input.Dispose();$output.Dispose()}
    }
    $writer=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open(),[Text.UTF8Encoding]::new($false))
    try{$writer.Write((@{members=$members;scope='Matched test sources, retained host source, diagnostic overlays, receipts/logs and nested core/view archive; root harness is not the mature compiled overlay; external references/runtime toolchain and complete project portability not included; local-only, not release certification'}|ConvertTo-Json -Depth 5))}finally{$writer.Dispose()}
}finally{$zip.Dispose()}
$zip=[IO.Compression.ZipFile]::OpenRead($archive)
try{
    if($zip.Entries.Count -ne $members.Count+1){throw 'Qualification archive inventory differs'}
    foreach($member in $members){
        $stream=$zip.GetEntry($member.entry).Open()
        try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($hash -cne $member.sha256 -or (Get-FileHash -LiteralPath $member.path).Hash -cne $member.sha256){throw 'Qualification archive member differs'}
    }
}finally{$zip.Dispose()}
$result=@{verified=$true;entries=$members.Count+1;bytes=(Get-Item -LiteralPath $archive).Length;sha256=(Get-FileHash -LiteralPath $archive).Hash;offDeviceBackup=$false;portableBuildKit=$false;releaseCertified=$false}
[IO.File]::WriteAllText((Join-Path $suite 'source-backup.json'),($result|ConvertTo-Json))
$result|ConvertTo-Json
