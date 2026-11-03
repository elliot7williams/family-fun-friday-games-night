$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Expand-VerifiedRuntime.ps1')
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$out=Join-Path $workspace 'continuity-evidence/portable-runtime-rejection-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve rejection evidence'}
$null=New-Item -ItemType Directory -Path $out
$rows=[Collections.Generic.List[object]]::new()
foreach($case in @('archive-hash','manifest-hash','traversal','duplicate','missing-manifest','inventory')){
    $archive=Join-Path $out ($case+'.zip')
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    $json='{"format":1,"files":[]}'
    try{
        if($case -ne 'missing-manifest'){
            $entry=$zip.CreateEntry('manifest.json');$target=$entry.Open()
            try{$bytes=[Text.Encoding]::UTF8.GetBytes($json);$target.Write($bytes)}finally{$target.Dispose()}
        }
        if($case -eq 'traversal'){$null=$zip.CreateEntry('../escape.txt')}
        if($case -eq 'duplicate'){$null=$zip.CreateEntry('MANIFEST.JSON')}
    }finally{$zip.Dispose()}
    $archiveHash=(Get-FileHash -LiteralPath $archive).Hash
    $manifestHash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($json)))
    if($case -eq 'archive-hash'){$archiveHash='0'*64}
    if($case -eq 'manifest-hash'){$manifestHash='0'*64}
    $destination=Join-Path $out ($case+'-extracted')
    $errorText=$null
    try{$null=Expand-VerifiedLeaRuntime -Archive $archive -ExpectedArchiveSha256 $archiveHash -ExpectedManifestSha256 $manifestHash -Destination $destination}catch{$errorText=$_.Exception.Message}
    $expected=switch($case){
        'archive-hash'{'Archive identity differs'}
        'manifest-hash'{'Manifest identity differs'}
        'traversal'{'Unsafe or duplicate archive path'}
        'duplicate'{'Unsafe or duplicate archive path'}
        'missing-manifest'{'Invalid manifest entry'}
        'inventory'{'Unexpected package inventory'}
    }
    $rows.Add(@{case=$case;passed=$errorText -ceq $expected -and !(Test-Path -LiteralPath $destination);error=$errorText})
}
$result=@{passed=@($rows|Where-Object {!$_.passed}).Count -eq 0;checks=$rows;runtimeExecuted=$false}
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 5))
[pscustomobject]$result|ConvertTo-Json -Depth 5
