$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$stem=Join-Path $root 'continuity-evidence/print-package-probe-source-20261028'
if((Test-Path ($stem+'.zip')) -or (Test-Path ($stem+'.json'))){throw 'Preserve source checkpoint'}
if((Get-PSDrive C).Free -lt 512MB){throw 'Source checkpoint reserve required'}
$proofPath=Join-Path $root 'continuity-evidence/print-package-cancel-v4/result.json'
if((Get-FileHash $proofPath).Hash -cne '71E2BA9C404CD22E0BA6CC0470C7A5A6D9A5A6C3DC2BF9743D7FEA6D7E8A0473'){throw 'Original target proof differs'}
$proof=Get-Content $proofPath -Raw|ConvertFrom-Json
if(!$proof.passed -or !$proof.nativeCanceled -or !$proof.inputsUnchanged -or $proof.ownerAttention){throw 'Retire original owners before checkpoint'}
$files=@()
foreach($name in @('PROJECT_CONTEXT.md','LeaForge/PRINT_PACKAGE_TARGET.md','LeaForge/PRINTING.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','Lea.Tests/PrintPackageAbi.cpp','Lea.Tests/PrintPackageLease.cs','Lea.Build/Test-PrintPackageCancel.ps1','Lea.Build/Read-OwnedJobMembers.cs','Lea.Build/OwnedTestProcess.cs','Lea.Build/Invoke-OwnedTestRun.ps1','Lea.Build/Backup-PrintPackageProbe.ps1')){$files+=Get-Item (Join-Path $root $name)}
foreach($version in 1..4){$files+=Get-ChildItem (Join-Path $root ('continuity-evidence/print-package-cancel-v'+$version)) -File}
$files=@($files|Sort-Object FullName -Unique)
$members=[Collections.Generic.List[object]]::new()
$archive=[IO.Compression.ZipFile]::Open(($stem+'.zip'),'Create')
try{
    foreach($file in $files){
        $name=[IO.Path]::GetRelativePath($root,$file.FullName).Replace('\','/')
        if([IO.Path]::IsPathFullyQualified($name) -or $name.Split('/') -contains '..' -or $file.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Checkpoint source escape'}
        $hash=(Get-FileHash $file.FullName).Hash
        $null=[IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive,$file.FullName,$name,'Optimal')
        $members.Add(@{entry=$name;path=$file.FullName;sha256=$hash;bytes=$file.Length})
    }
}finally{$archive.Dispose()}
$archive=[IO.Compression.ZipFile]::OpenRead(($stem+'.zip'))
try{
    if($archive.Entries.Count -ne $members.Count){throw 'Checkpoint inventory differs'}
    foreach($member in $members){
        $entry=$archive.GetEntry($member.entry);$stream=$entry.Open()
        try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($entry.Length -ne $member.bytes -or $hash -cne $member.sha256 -or (Get-FileHash $member.path).Hash -cne $hash){throw 'Checkpoint/original differs'}
    }
}finally{$archive.Dispose()}
$result=@{verified=$true;files=$members.Count;bytes=(Get-Item ($stem+'.zip')).Length;sha256=(Get-FileHash ($stem+'.zip')).Hash;members=$members;scope='Experimental supported-target ABI/cancel source and retained failure/pass receipts; private compiler binary kit excluded, manifest and original source hashes retained; local supplement, not adopted engine/portable SDK/off-device backup'}
[IO.File]::WriteAllText(($stem+'.json'),($result|ConvertTo-Json -Depth 6))
$result|Select-Object verified,files,bytes,sha256|ConvertTo-Json -Compress
