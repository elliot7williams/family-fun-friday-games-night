$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$stem=Join-Path $workspace 'continuity-evidence/print-preview-host-source-20261027'
if((Test-Path -LiteralPath ($stem+'.zip')) -or (Test-Path -LiteralPath ($stem+'.json'))){throw 'Preserve checkpoint'}
if((Get-PSDrive C).Free -lt 576MB){throw 'Checkpoint working reserve required'}
$proof=Join-Path $workspace 'continuity-evidence/print-preview-host-v2/result.json'
if((Get-FileHash -LiteralPath $proof).Hash -cne 'F046E375FE0277CDDB2433116BF293C8B577DDA91311D0C0B8EDF169C3BA014F'){throw 'Preview proof changed'}
$r=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.inputsUnchanged -or $r.ownerAttention){throw 'Preview test must finish safely'}
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($name in @('PROJECT_CONTEXT.md','LeaForge/PRINTING.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','LeaForge/CURRENT_HOST.md','LeaForge/test-pages/print-styled-document.html','LeaForge/test-pages/print-preview-image.html','Lea.Firespider/PrintPreview.vb','Lea.Build/New-PrintPreviewHostSource.ps1','Lea.Build/New-PrintPreviewHarness.ps1','Lea.Build/PrintPreviewHostProbe.fragment.cs','Lea.Build/Test-PrintPreviewHost.ps1','Lea.Build/Backup-PrintPreviewHost.ps1','Lea.Build/OwnedTestProcess.cs','Lea.Build/Invoke-OwnedTestRun.ps1')){$null=$paths.Add((Join-Path $workspace $name))}
foreach($name in @('print-preview-host-v1','print-preview-host-v2')){
    foreach($file in Get-ChildItem -LiteralPath (Join-Path $workspace ('continuity-evidence/'+$name)) -File){$null=$paths.Add($file.FullName)}
}
foreach($name in @('continuity-evidence/print-preview-host-v1/host-source','continuity-evidence/print-preview-host-v2/runtime','continuity-evidence/print-styles-v3/core-source','continuity-evidence/print-live-capture-v1/view-source','Lea.Print')){
    $root=Join-Path $workspace $name
    foreach($file in Get-ChildItem -LiteralPath $root -Recurse -Force -File){
        $parts=[IO.Path]::GetRelativePath($root,$file.FullName).Split('\')
        if($parts -contains 'obj' -or $parts -contains 'bin'){continue}
        $null=$paths.Add($file.FullName)
    }
}
$members=@{}
foreach($path in $paths){
    $relative=[IO.Path]::GetRelativePath($workspace,$path).Replace('\','/')
    if([IO.Path]::IsPathRooted($relative) -or $relative.Split('/') -contains '..' -or ((Get-Item -LiteralPath $path).Attributes -band [IO.FileAttributes]::ReparsePoint)){throw 'Checkpoint path invalid'}
    $members[$relative]=@{path=$path;sha256=(Get-FileHash -LiteralPath $path).Hash;bytes=(Get-Item -LiteralPath $path).Length}
}
$archive=[IO.Compression.ZipFile]::Open(($stem+'.zip'),'Create')
try{foreach($name in $members.Keys){$null=[IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive,$members[$name].path,$name,'Optimal')}}finally{$archive.Dispose()}
$archive=[IO.Compression.ZipFile]::OpenRead(($stem+'.zip'))
try{
    if($archive.Entries.Count -ne $members.Count){throw 'Checkpoint coverage differs'}
    foreach($entry in $archive.Entries){
        $expected=$members[$entry.FullName]
        if(!$expected -or $entry.Length -ne $expected.bytes){throw 'Checkpoint entry differs'}
        $s=$entry.Open();try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))}finally{$s.Dispose()}
        if($hash -cne $expected.sha256 -or (Get-FileHash -LiteralPath $expected.path).Hash -cne $expected.sha256){throw 'Checkpoint/original readback differs'}
    }
}finally{$archive.Dispose()}
$result=@{verified=$true;files=$members.Count;bytes=(Get-Item -LiteralPath ($stem+'.zip')).Length;sha256=(Get-FileHash -LiteralPath ($stem+'.zip')).Hash;members=$members;scope='Local experimental preview host/source/runtime/test/failed-first-probe checkpoint; external compiler/reference sources and prior worker archives remain separate, not complete SDK or release delivery'}
[IO.File]::WriteAllText(($stem+'.json'),($result|ConvertTo-Json -Depth 6))
$result|Select-Object verified,files,bytes,sha256|ConvertTo-Json
