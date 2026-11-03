$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'continuity-evidence/solid-build-archives-v1'
if(Test-Path $out){throw 'Preserve previous experiment'}
$source=Join-Path $root 'dist/LeaExes'
$names=@('Lea-runtime-210-ready_2026-09-19_16-01-25.zip','Lea-runtime-211-ready_2026-09-19_20-45-48.zip')
$total=($names|ForEach-Object {(Get-Item (Join-Path $source $_)).Length}|Measure-Object -Sum).Sum
if((Get-PSDrive C).Free -lt 64MB+2*$total){throw 'Bounded experiment workspace required; not a build reserve waiver'}
if((Get-CimInstance Win32_OperatingSystem).FreePhysicalMemory*1KB -lt 1GB){throw 'Compression memory headroom required'}
$tar=Join-Path $env:WINDIR 'System32/tar.exe'
if((Get-FileHash $tar).Hash -cne '2CA7DB442FEAFA8E6D7A88C7DDDF3113128F85701A4EB1182C71CFBF3D479A56'){throw 'Tested archive tool differs'}
$null=New-Item -ItemType Directory -Path $out
$restored=Join-Path $out 'restored';$null=New-Item -ItemType Directory -Path $restored
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new()
$result=[ordered]@{passed=$false;originalArchivesRemoved=$false;inputs=$inputs;toolSha256=(Get-FileHash $tar).Hash}
try{
    Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
    foreach($name in $names){
        $path=Join-Path $source $name;$s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
        $inputs.Add(@{name=$name;path=$path;length=$s.Length;sha256=(Get-FileHash $path).Hash;written=(Get-Item $path).LastWriteTimeUtc.ToString('o');sddl=(Get-Acl $path).Sddl})
    }
    $pack=Join-Path $out 'older-builds.7z'
    $clock=[Diagnostics.Stopwatch]::StartNew()
    & $tar -cf $pack --format=7zip --options=7zip:compression=lzma2,7zip:compression-level=9 -C $source @names
    $result.createMs=$clock.ElapsedMilliseconds
    if($LASTEXITCODE -ne 0){throw 'Archive creation failed'}
    $entries=@(& $tar -tf $pack)
    if($LASTEXITCODE -ne 0 -or ($entries|Sort-Object|ConvertTo-Json -Compress) -cne ($names|Sort-Object|ConvertTo-Json -Compress)){throw 'Archive inventory differs'}
    & $tar -xkf $pack -C $restored
    if($LASTEXITCODE -ne 0){throw 'Restoration failed'}
    foreach($input in $inputs){
        $file=Join-Path $restored $input.name
        if((Get-Item $file).Length -ne $input.length -or (Get-FileHash $file).Hash -cne $input.sha256){throw 'Restored original ZIP differs'}
        if((Get-FileHash $input.path).Hash -cne $input.sha256 -or (Get-Acl $input.path).Sddl -cne $input.sddl -or (Get-Item $input.path).LastWriteTimeUtc.ToString('o') -cne $input.written){throw 'Original archive changed'}
    }
    $result.roundTripVerified=$true;$result.sourceBytes=$total;$result.containerBytes=(Get-Item $pack).Length
    $result.containerSha256=(Get-FileHash $pack).Hash
    $result.potentialLogicalReduction=$total-$result.containerBytes
    $result.scope='Archive-tool experiment only. Original ZIP bytes restored/hash-verified; no originals removed, no current source/browser changes or engine speed claim.'
    $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    foreach($hold in $holds){$hold.Dispose()}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 5))
    $result|ConvertTo-Json -Depth 5
}
