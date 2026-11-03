$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$base=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1'
$ilDirectory=Join-Path $base 'paired-worker-il-v2'
$nativeDirectory=Join-Path $base 'paired-worker-native-v1'
$output=Join-Path $workspace 'continuity-evidence/tested-worker-source-v1'
if((Test-Path -LiteralPath $output) -or (Test-Path -LiteralPath ($output+'.zip'))){throw 'Preserve export'}
if((Get-PSDrive C).Free -lt 512MB){throw 'Working reserve required'}
$ilReceipt=Join-Path $ilDirectory 'result.json';$nativeReceipt=Join-Path $nativeDirectory 'result.json'
if((Get-FileHash -LiteralPath $ilReceipt).Hash -cne '9AA40DCE98261B0018CD507E597D65FF4D341888DCCB316F546FAEE31F796EF5' -or (Get-FileHash -LiteralPath $nativeReceipt).Hash -cne '896375B3D0BA4DD7C26EEB9B933C5548201B15AC5669F363039EBD7EA8C3AFFF'){throw 'Build proofs changed'}
$il=Get-Content -LiteralPath $ilReceipt -Raw|ConvertFrom-Json
$native=Get-Content -LiteralPath $nativeReceipt -Raw|ConvertFrom-Json
if(!$il.passed -or !$il.inputsUnchanged -or $il.ownerAttention -or !$native.passed -or !$native.inputsUnchanged -or $native.ownerAttention){throw 'Worker build incomplete'}
foreach($pair in @(@((Join-Path $ilDirectory 'Lea.Script.Worker.dll'),'83FE295571FAFF00D4864A13C37D270B998B339EA27A61A950A3572A494AC2DB'),@((Join-Path $nativeDirectory 'Lea.Script.Worker.dll'),'60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'))){if((Get-FileHash -LiteralPath $pair[0]).Hash -cne $pair[1]){throw 'Worker binary changed'}}
$null=New-Item -ItemType Directory -Path $output
$copied=[Collections.Generic.List[object]]::new()
function CopyPinned([string]$Path,[string]$Expected,[string]$Relative){
    if((Get-FileHash -LiteralPath $Path).Hash -cne $Expected){throw "Changed source input: $Path"}
    $target=Join-Path $output $Relative
    $null=New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($target)) -Force
    Copy-Item -LiteralPath $Path -Destination $target
    if((Get-FileHash -LiteralPath $target).Hash -cne $Expected){throw 'Export copy mismatch'}
    $copied.Add(@{archivePath=$Relative;sha256=$Expected;originalPath=$Path})
}
$known=@{};foreach($input in $il.inputs){$known[$input.path]=$input.sha256}
$recipe=Join-Path $ilDirectory 'worker.rsp'
$references=[Collections.Generic.List[object]]::new();$sourceCount=0;$resources=0
foreach($line in Get-Content -LiteralPath $recipe){
    if($line -match '^"(.+\.cs)"$'){
        $path=$Matches[1];if(!$known.ContainsKey($path)){throw 'Unpinned worker source'}
        CopyPinned $path $known[$path] ('source/'+[IO.Path]::GetFileName($path));$sourceCount++
    }elseif($line -match '^/resource:"([^"]+)"'){
        $path=$Matches[1];if(!$known.ContainsKey($path)){throw 'Unpinned worker resource'}
        CopyPinned $path $known[$path] 'resource/Dom.compact.js';$resources++
    }elseif($line -match '^/reference:"([^"]+)"$'){
        $path=$Matches[1];if(!$known.ContainsKey($path)){throw 'Unpinned compiler reference'}
        $references.Add(@{path=$path;sha256=$known[$path];currentlyAvailable=(Test-Path -LiteralPath $path);included=$false})
    }
}
if($sourceCount -ne 17 -or $resources -ne 1){throw 'Worker source coverage differs'}
foreach($pair in @(@($ilReceipt,'proofs/il-build.json'),@($nativeReceipt,'proofs/native-build.json'))){CopyPinned $pair[0] (Get-FileHash -LiteralPath $pair[0]).Hash $pair[1]}
CopyPinned $recipe (Get-FileHash -LiteralPath $recipe).Hash 'recipes/worker.rsp'
$nativeRecipe=Join-Path $nativeDirectory 'native.rsp'
$nativePin=@($native.inputs|Where-Object path -ceq $nativeRecipe)
if($nativePin.Count -ne 1){throw 'Native recipe proof missing'}
CopyPinned $nativeRecipe $nativePin[0].sha256 'recipes/native.rsp'
foreach($spec in @(@($il,'Build-PairedWorkerV2.ps1','recipes/Build-PairedWorkerV2.ps1'),@($native,'Build-PairedNative.ps1','recipes/Build-PairedNative.ps1'))){
    $pin=@($spec[0].inputs|Where-Object{[IO.Path]::GetFileName($_.path) -ceq $spec[1]})
    if($pin.Count -ne 1){throw 'Original build script proof missing'}
    CopyPinned $pin[0].path $pin[0].sha256 $spec[2]
}
$scope='Exact tested worker 60FF source chain: 17 C# files, compact DOM resource, original IL/ReadyToRun recipes and receipts. Copyright headers retained. Reference/compiler/runtime binaries, parser/interpreter source, apphost generation and complete portable project are not included. Third-party redistribution/license review and independent rebuild remain required; private preservation only, not release or remote backup.'
[IO.File]::WriteAllText((Join-Path $output 'README.txt'),$scope)
[IO.File]::WriteAllText((Join-Path $output 'manifest.json'),(@{scope=$scope;files=$copied;references=$references;ilSha256=$il.binarySha256;nativeSha256=$native.binarySha256}|ConvertTo-Json -Depth 7))
$files=@(Get-ChildItem -LiteralPath $output -Recurse -File);$archive=$output+'.zip'
[IO.Compression.ZipFile]::CreateFromDirectory($output,$archive,[IO.Compression.CompressionLevel]::Optimal,$false)
$zip=[IO.Compression.ZipFile]::OpenRead($archive)
try{
    if($zip.Entries.Count -ne $files.Count){throw 'Archive count differs'}
    foreach($file in $files){
        $entry=$zip.GetEntry([IO.Path]::GetRelativePath($output,$file.FullName).Replace('\','/'))
        if(!$entry -or $entry.Length -ne $file.Length){throw 'Archive entry missing/truncated'}
        $stream=$entry.Open()
        try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($hash -cne (Get-FileHash -LiteralPath $file.FullName).Hash){throw 'Archive content differs'}
    }
}finally{$zip.Dispose()}
[pscustomobject]@{verified=$true;sources=$sourceCount;resources=$resources;files=$files.Count;references=$references.Count;missingReferences=@($references|Where-Object{!$_.currentlyAvailable}).Count;archive=$archive;sha256=(Get-FileHash -LiteralPath $archive).Hash}|ConvertTo-Json
