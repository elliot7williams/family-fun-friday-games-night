$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Compress-OldRuntimeStorage.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve runtime storage cleanup recipe'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Compress-OldPackageStorage.ps1'))
$text=$text.Replace("'dist/LeaExes'","'continuity-evidence/modern-worker-v1'").Replace('old-package-storage-20261026-v1','old-runtime-storage-20261026-v1')
$text=$text.Replace("@('.cs','.vb','.dll','.exe','.pdb')","@('.dll','.exe','.pdb')").Replace('100KB','1MB')
$lines=$text -split "\r?\n"
$remove=@($lines|Where-Object {$_ -like '*Substring($root.Length+1)*' -or $_ -like '*$_.LastWriteTime -lt*'})
if($remove.Count -ne 2){throw 'Archived runtime filter anchors differ'}
$text=($lines|Where-Object {$_ -notin $remove}) -join "`r`n"
$text=$text.Replace('NTFS storage compression of old September extracted package files only; no working source/current candidate/ZIP edits','NTFS storage compression of retained old modern-worker runtime binaries only; byte/ACL/mtime preservation checked, no deletion or engine adoption')
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
