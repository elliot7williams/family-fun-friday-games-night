$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'continuity-evidence/solid-archive-tool-v1'
if(Test-Path $out){throw 'Preserve previous experiment'}
if((Get-PSDrive C).Free -lt 64MB){throw 'Backup experiment workspace required; not an engine build waiver'}
$input=Join-Path $out 'input';$restore=Join-Path $out 'restored'
$null=New-Item -ItemType Directory -Path $input,$restore
$tar=Join-Path $env:WINDIR 'System32/tar.exe'
$result=[ordered]@{passed=$false;originalArchivesRemoved=$false;tarSha256=(Get-FileHash $tar).Hash}
try{
    Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
    $bytes=[byte[]]::new(2MB);[Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
    foreach($name in @('first.bin','second.bin')){[IO.File]::WriteAllBytes((Join-Path $input $name),$bytes)}
    $archive=Join-Path $out 'probe.7z'
    & $tar -cf $archive --format=7zip --options=7zip:compression=lzma2,7zip:compression-level=6 -C $input first.bin second.bin
    if($LASTEXITCODE -ne 0){throw 'Bundled tar cannot create requested archive'}
    $entries=@(& $tar -tf $archive)
    if($LASTEXITCODE -ne 0 -or ($entries|Sort-Object|ConvertTo-Json -Compress) -cne (@('first.bin','second.bin')|ConvertTo-Json -Compress)){throw 'Archive inventory differs'}
    & $tar -xkf $archive -C $restore
    if($LASTEXITCODE -ne 0){throw 'Archive restoration failed'}
    foreach($name in $entries){
        if((Get-FileHash (Join-Path $input $name)).Hash -cne (Get-FileHash (Join-Path $restore $name)).Hash){throw 'Restored bytes differ'}
    }
    $result.roundTripVerified=$true
    $result.inputBytes=4MB;$result.archiveBytes=(Get-Item $archive).Length
    $result.duplicateReductionObserved=$result.archiveBytes -lt 3MB
    $result.passed=$result.duplicateReductionObserved
}catch{$result.error=$_.Exception.ToString()}
finally{
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 4))
    $result|ConvertTo-Json -Depth 4
}
