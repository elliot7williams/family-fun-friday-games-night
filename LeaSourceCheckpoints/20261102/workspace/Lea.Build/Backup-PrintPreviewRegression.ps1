param([switch]$Command)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$stem=Join-Path $workspace $(if($Command){'continuity-evidence/print-command-regression-source-20261028'}else{'continuity-evidence/print-preview-regression-source-20261027'})
if((Test-Path -LiteralPath ($stem+'.zip')) -or (Test-Path -LiteralPath ($stem+'.json'))){throw 'Preserve regression checkpoint'}
if((Get-PSDrive C).Free -lt 576MB){throw 'Checkpoint working reserve required'}
$prefix=if($Command){'print-command'}else{'print-preview'}
$hostPath=Join-Path $workspace ('continuity-evidence/temporal-startup-v1/host-'+$prefix+'-owned-v1/result.json')
$extractPath=Join-Path $workspace ('continuity-evidence/'+$prefix+'-package-extraction-v1/result.json')
$hostHash=if($Command){'82C0C3CFCEECC781622FE265AD90EBA540F7F2BCD230F29ADB988F5DFA4BFC1A'}else{'B721DF1664B22445A09DA73AD2B5F06DD449D8CFD58EA927A547E140AA7AA4DA'}
$extractHash=if($Command){'92E2D10BDC47A718AECEAAFA9E44471EECA210776A0B24BE01A863AF51383776'}else{'1A554B05D55DCAFF93C710D31C91265850DA8279B3EC2940FAFD86A09FA46522'}
foreach($binding in @(
    @($hostPath,$hostHash),
    @($extractPath,$extractHash)
)){
    if((Get-FileHash -LiteralPath $binding[0]).Hash -cne $binding[1]){throw 'Bound qualification receipt differs'}
    $proof=Get-Content -LiteralPath $binding[0] -Raw|ConvertFrom-Json
    if(!$proof.passed -or !$proof.inputsUnchanged -or $proof.ownerAttention){throw 'Qualified owners must retire safely'}
}
$references=@(
    @{name='print-preview-host-source-20261027.zip';sha256='3BE58454C829D6A3D8E17FE5B762206E47F030F0501C3206E0EDFA6DA3488524'},
    @{name='runtime-8033-matched-source-20261027.zip';sha256='84179CBDF6C3674E43C886FCF72B5C048056425B2D029E2CE3DBBE4CD719AEB9'}
)
if($Command){$references[0]=@{name='print-command-source-20261028.zip';sha256='7B56572A2318AE3B491320DADAF9EC3AB3B58D57EE90207BCB97B5E4314BCB0F'}}
foreach($reference in $references){if((Get-FileHash -LiteralPath (Join-Path $workspace ('continuity-evidence/'+$reference.name))).Hash -cne $reference.sha256){throw 'Prior matching source checkpoint differs'}}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($name in @('PROJECT_CONTEXT.md','LeaForge/PRINTING.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','LeaForge/CURRENT_HOST.md','LeaForge/STORAGE_CLEANUP.md','Lea.Firespider/PrintPreview.vb','Lea.Build/Prepare-PrintPreviewHostCampaign.ps1','Lea.Build/Test-PrintPreviewHostCampaign.generated.ps1','Lea.Build/Prepare-PrintPreviewDelivery.ps1','Lea.Build/Package-PrintPreviewDevelopment.generated.ps1','Lea.Build/Test-PrintPreviewExtraction.generated.ps1','Lea.Build/PrintPreviewDevelopmentNote.txt','Lea.Build/Backup-PrintPreviewRegression.ps1','Lea.Build/Compress-HistoricalEvidence.ps1','Lea.Build/OwnedTestProcess.cs','Lea.Build/Invoke-OwnedTestRun.ps1')){$null=$paths.Add((Join-Path $workspace $name))}
$directories=@(
    ('temporal-startup-v1/host-'+$prefix+'-owned-v1')
    ($prefix+'-host-campaign-preparation-v1')
    ($prefix+'-delivery-preparation-package-v1')
    ($prefix+'-delivery-preparation-extraction-v1')
    ($prefix+'-development-package-v1')
)
if(!$Command){$directories+=@('project-compression-20261027-otherstudies','project-compression-20261027-legacylarge','project-compression-20261027-historicaltext')}
if($Command){
    foreach($name in @('Lea.Firespider/PrintCommand.vb','Lea.Build/Test-PrintCommandHostCampaign.generated.ps1','Lea.Build/Package-PrintCommandDevelopment.generated.ps1','Lea.Build/Test-PrintCommandExtraction.generated.ps1','Lea.Build/PrintCommandDevelopmentNote.txt')){$null=$paths.Add((Join-Path $workspace $name))}
}
foreach($name in $directories){
    foreach($file in Get-ChildItem -LiteralPath (Join-Path $workspace ('continuity-evidence/'+$name)) -File -Recurse -Force){$null=$paths.Add($file.FullName)}
}
$extraction=Join-Path $workspace ('continuity-evidence/'+$prefix+'-package-extraction-v1')
foreach($file in Get-ChildItem -LiteralPath $extraction -File){$null=$paths.Add($file.FullName)}
foreach($name in @('LEA-TEST-BUILD.txt','lea-test-build-manifest.json')){$null=$paths.Add((Join-Path $extraction ('runtime/'+$name)))}
$members=@{}
foreach($path in $paths){
    $relative=[IO.Path]::GetRelativePath($workspace,$path).Replace('\','/')
    if([IO.Path]::IsPathRooted($relative) -or $relative.Split('/') -contains '..' -or ((Get-Item -LiteralPath $path).Attributes -band [IO.FileAttributes]::ReparsePoint)){throw 'Checkpoint source path invalid'}
    $members[$relative]=@{path=$path;sha256=(Get-FileHash -LiteralPath $path).Hash;bytes=(Get-Item -LiteralPath $path).Length}
}
$archive=[IO.Compression.ZipFile]::Open(($stem+'.zip'),'Create')
try{foreach($name in $members.Keys){$null=[IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive,$members[$name].path,$name,'Optimal')}}finally{$archive.Dispose()}
$archive=[IO.Compression.ZipFile]::OpenRead(($stem+'.zip'))
try{
    if($archive.Entries.Count -ne $members.Count){throw 'Checkpoint inventory differs'}
    foreach($entry in $archive.Entries){
        $spec=$members[$entry.FullName]
        if(!$spec -or $entry.Length -ne $spec.bytes){throw 'Checkpoint entry differs'}
        $s=$entry.Open();try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))}finally{$s.Dispose()}
        if($hash -cne $spec.sha256 -or (Get-FileHash -LiteralPath $spec.path).Hash -cne $spec.sha256){throw 'Checkpoint/original readback differs'}
    }
}finally{$archive.Dispose()}
$result=@{verified=$true;files=$members.Count;bytes=(Get-Item -LiteralPath ($stem+'.zip')).Length;sha256=(Get-FileHash -LiteralPath ($stem+'.zip')).Hash;sourceReferences=$references;members=$members;scope='Regression/delivery/runtime/controller supplement to preserved matching product source and prior suite checkpoint; local, not complete SDK or off-device backup'}
[IO.File]::WriteAllText(($stem+'.json'),($result|ConvertTo-Json -Depth 6))
$result|Select-Object verified,files,bytes,sha256|ConvertTo-Json
