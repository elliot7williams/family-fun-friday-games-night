$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$out=Join-Path $workspace 'continuity-evidence/portable-runtime-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve package evidence'}
if((Get-PSDrive C).Free -lt 768MB){throw 'Insufficient package reserve'}
$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$Path,[string]$Hash){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $Hash){throw "Changed input: $Path"}
    $stream.Position=0;return $stream
}
$result=@{passed=$false;runtimeExecuted=$false;releaseCertified=$false;error=$null}
$null=New-Item -ItemType Directory -Path $out
try{
    $proof=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-adapter-rebuilt-v1/result.json'
    $null=Hold $proof 'CD0676855551890BBAB723BAD4A37FC887138811E38174CD87A2F5E183C2A26C'
    $tested=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
    if(!$tested.passed -or !$tested.completed -or $tested.ownerAttention -or !$tested.originalInputsUnchanged -or !$tested.candidateBytesUnchanged -or $tested.runs.Count -ne 15){throw 'Incomplete runtime proof'}
    $files=@($tested.before|Sort-Object path)
    if($files.Count -ne 398 -or (Get-ChildItem -LiteralPath $tested.candidate -Recurse -File).Count -ne $files.Count){throw 'Unexpected runtime inventory'}
    $manifest=[Collections.Generic.List[object]]::new()
    $zipPath=Join-Path $out 'Lea-Firespider-development-runtime.zip'
    $zip=[IO.Compression.ZipFile]::Open($zipPath,'Create')
    try{
        foreach($file in $files){
            if(!$file.path.StartsWith($tested.candidate+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'File outside candidate'}
            $relative=$file.path.Substring($tested.candidate.Length+1).Replace('\','/')
            $walk=$file.path
            while($walk){if((Get-Item -LiteralPath $walk -Force).Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Linked input refused'};$walk=[IO.Path]::GetDirectoryName($walk)}
            $stream=Hold $file.path $file.sha256
            $entry=$zip.CreateEntry('runtime/'+$relative,[IO.Compression.CompressionLevel]::Optimal)
            $target=$entry.Open();try{$stream.CopyTo($target)}finally{$target.Dispose()}
            $manifest.Add(@{path='runtime/'+$relative;sha256=$file.sha256;length=$stream.Length})
        }
        $document=@{format=1;scope='Development runtime and test fixtures; not a complete source/toolchain kit or certified release';testedReceiptSha256='CD0676855551890BBAB723BAD4A37FC887138811E38174CD87A2F5E183C2A26C';files=@($manifest)}
        $json=$document|ConvertTo-Json -Depth 5
        [IO.File]::WriteAllText((Join-Path $out 'manifest.json'),$json,[Text.UTF8Encoding]::new($false))
        $entry=$zip.CreateEntry('manifest.json');$target=$entry.Open()
        try{$bytes=[Text.Encoding]::UTF8.GetBytes($json);$target.Write($bytes)}finally{$target.Dispose()}
    }finally{$zip.Dispose()}
    $result.archive=$zipPath;$result.archiveSha256=(Get-FileHash -LiteralPath $zipPath).Hash
    $result.manifestSha256=(Get-FileHash -LiteralPath (Join-Path $out 'manifest.json')).Hash
    $result.fileCount=$manifest.Count;$result.uncompressedBytes=($manifest|Measure-Object length -Sum).Sum
    $result.compressedBytes=(Get-Item -LiteralPath $zipPath).Length;$result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    foreach($lease in $leases){$lease.Dispose()}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 6))
}
[pscustomobject]$result|ConvertTo-Json -Depth 6
