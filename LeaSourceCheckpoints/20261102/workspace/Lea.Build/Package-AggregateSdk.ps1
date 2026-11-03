param([switch]$PrintSdk)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$destination='I:\LeaBuildStorage\sdk-aggregate-package-20261031-v1'
if($PrintSdk){$destination='I:\LeaBuildStorage\sdk-print-aggregate-package-20261031-v1'}
if(Test-Path -LiteralPath $destination){throw 'Preserve existing SDK package'}
$rows=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new()
function AddFile([string]$path,[string]$relative,[string]$pin){
    if([IO.Path]::IsPathRooted($relative) -or $relative.Contains(':') -or $relative.Contains('\') -or $relative.Split('/') -contains '..'){throw 'Unsafe package path'}
    if(@($rows|Where-Object name -ceq $relative).Count){throw 'Duplicate package entry'}
    $ancestor=Get-Item -LiteralPath $path
    while($ancestor){if($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse path refused'};$ancestor=if($ancestor -is [IO.FileInfo]){$ancestor.Directory}else{$ancestor.Parent}}
    $stream=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($stream)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($hash -cne $pin){throw ('Package input differs: '+$relative)}
    $rows.Add(@{name=$relative;path=$path;sha256=$hash;bytes=$stream.Length;stream=$stream})
}
function Proof([string]$relative,[string]$pin,[string]$name){
    $path=Join-Path $root $relative;AddFile $path ('evidence/'+$name) $pin
    return Get-Content -LiteralPath $path -Raw|ConvertFrom-Json
}
try{
    if($PrintSdk){
        $build=Proof 'continuity-evidence/sdk-print-aggregate-source-v1/result.json' '47C4617143CFF63510728C88587E278C423F9E3C7AFA92629BA5D29FFD48DDA6' 'source-build.json'
        if(!$build.aggregateSourceBuildPassed -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.sourceBuiltProducts.Count -ne 5 -or $build.runs.Count -ne 1 -or !$build.runs[0].passed -or $build.runs[0].timedOut -or $build.runs[0].killRequested -or $build.runs[0].exitCode -ne 0){throw 'Print source compile incomplete'}
        $runtime=Proof 'continuity-evidence/sdk-print-aggregate-runtime-v1/result.json' 'D6D0E65685041CFB517712C92824F3AB5E50E22E4ED6DB5086E882B058D0B676' 'runtime.json'
        if(!$runtime.passed -or !$runtime.aggregateProducts -or !$runtime.inputsUnchanged -or $runtime.ownerAttention){throw 'Print runtime incomplete'}
    }else{
        $build=Proof 'continuity-evidence/sdk-portable-source-v1/result.json' '57E993FAE281B565D502BBEE77A95C98A71B5A0816C5DC5D66250F3EE9C32E36' 'source-build.json'
        if(!$build.passed -or !$build.aggregateSourceBuildPassed -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.sourceBuiltProducts.Count -ne 4){throw 'Aggregate build incomplete'}
    }
    $audits=@(
        @('continuity-evidence/sdk-aggregate-core-view-regression-20261031-v1/result.json','F6C8ADB6D27726EF18D7988C8C456799289CDF22B14C21444ACF8956CE9F9B40','font-audit.json'),
        @('continuity-evidence/sdk-aggregate-decoder-audit-v1/result.json','BF09BC5E7FD4E145D862BA9030FBABD6F8386E0EC28FA1485C954D76777C4245','decoder-audit.json')
    )
    if($PrintSdk){$audits=@(
        @('continuity-evidence/sdk-print-aggregate-core-view-regression-v1/result.json','83677E5B754B9100081A82281BD44F479B316F3B7CC11D1D533F9A1086C5C27F','font-audit.json'),
        @('continuity-evidence/sdk-print-aggregate-decoder-audit-v1/result.json','9473FA3B160D46F2883E3CD9256AF1A989F2B1BB99890A2A7EC9F98A36431262','decoder-audit.json'),
        @('continuity-evidence/sdk-print-aggregate-runtime-v1/runtime-audit.json','93868A8836E302732AAA8484DA69B430BBF65E040868604A600B24DF8B26EE33','print-audit.json')
    )}
    foreach($spec in $audits){$audit=Proof $spec[0] $spec[1] $spec[2];if(!$audit.verified){throw 'Prerequisite audit incomplete'}}
    $manifestPath='I:\LeaBuildStorage\portable-sdk-20261031-v1\bundle-manifest.json'
    $manifestPin='18A18FBEEBBE8DC0587E12467B2332A84CCC7A4929F373022DF506D92BD0ADB6'
    if($PrintSdk){
        $manifestPath='I:\LeaBuildStorage\portable-print-sdk-20261031-v1\bundle-manifest.json'
        $pins=@($build.inputs|Where-Object path -ceq $manifestPath|Select-Object -ExpandProperty sha256 -Unique)
        if($pins.Count -ne 1){throw 'Print aggregate inventory pin missing'};$manifestPin=$pins[0]
    }
    if((Get-FileHash $manifestPath).Hash -cne $manifestPin){throw 'Source inventory differs'}
    $inventory=Get-Content $manifestPath -Raw|ConvertFrom-Json
    $bundle=Join-Path $root 'continuity-evidence/sdk-portable-source-v1/bundle'
    if($PrintSdk){$bundle=Join-Path $root 'continuity-evidence/sdk-print-aggregate-source-v1/bundle'}
    $sources=@($inventory.files|Where-Object {$_.path -notmatch '(^|/)bin/'})
    $sourceCount=if($PrintSdk){76}else{53};$runtimeCount=if($PrintSdk){10}else{8}
    if($sources.Count -ne $sourceCount){throw 'Source coverage differs'}
    foreach($file in $sources){
        $path=Join-Path $bundle $file.path
        $pins=@($build.inputs|Where-Object path -ceq $path|Select-Object -ExpandProperty sha256 -Unique)
        if($pins.Count -ne 1 -or $pins[0] -cne $file.sha256){throw 'Source was not in tested aggregate build'}
        AddFile $path $file.path $file.sha256
    }
    foreach($product in $build.sourceBuiltProducts){
        $name=[IO.Path]::GetFileName($product.path)
        AddFile $product.path ('Sample/bin/Release/'+$name) $product.sha256
        if($name -ne 'Lea.EmbeddingSample.exe'){AddFile (Join-Path $bundle ('bin/Release/'+$name)) ('bin/Release/'+$name) $product.sha256}
    }
    $html=Join-Path $bundle 'Sample/embedding.html'
    AddFile (Join-Path $bundle 'Sample/bin/Release/embedding.html') 'Sample/bin/Release/embedding.html' (Get-FileHash $html).Hash
    New-Item -ItemType Directory -Path $destination | Out-Null
    $status=Join-Path $destination 'PACKAGE_STATUS.md'
    @'
# Lea SDK Development Candidate

This package contains 53 source/project/document files and eight runtime files
from the tested aggregate source build, plus evidence and an integrity manifest.
The source, core/view/image worker and sample are included. See README.md for
Visual Studio/MSBuild prerequisites and SDK.Build.proj for the build entrypoint.

The included binaries passed the source-build embedding/image checks, font
models/rendering and decoder retirement/shutdown checks. Rebuilding changes
binary identities and requires its own validation. Evidence receipts contain
historical local paths; those paths are not build dependencies.

This is NOT the complete SDK or a finished browser. JavaScript integration,
printing, complete dependency notices/licensing review, clean-machine testing,
legacy accessibility and startup reliability remain open. Remote scripting
remains disabled. No global security or universal website compatibility claim.

ZIP compression reduces storage/download size, not rendering or execution time.
Extract before building/running. SHA256 verifies integrity, not drive endurance.
'@ | Set-Content -LiteralPath $status -Encoding utf8
    if($PrintSdk){
        @'
# Lea SDK With Printing: Development Candidate

Contains 76 source/project/document files, ten runtime files, and evidence.
Core/view/image/print/sample build through SDK.Build.proj with relative paths.
Use modern C#12 MSBuild and the .NET Framework4.8 targeting pack. Printing uses
Windows WPF/Framework assemblies; these are prerequisites, not bundled runtimes.

The source-build receipt preserves a storage failure AFTER successful compilation.
A separate runtime campaign passed embedding, print capture/export, ranges and
image printing on those exact products. Font/render and decoder lifecycle audits
also passed. Rebuilding creates new identities requiring separate qualification.
Receipts contain historical paths, not portable build dependencies.

PACKAGE_STATUS.md supersedes the historical README's statement that printing is
missing: printing is included here, but full printing/physical output is not
qualified. JavaScript, complete notices/licensing review, host delivery,
clean-machine testing, legacy accessibility and startup reliability remain open.
Remote scripting stays disabled. This is not a complete SDK or finished browser.

Extract before running/building. ZIP compression saves storage/download bytes,
not execution time. Hash readback verifies integrity, not drive endurance.
'@ | Set-Content -LiteralPath $status -Encoding utf8
    }
    AddFile $status 'PACKAGE_STATUS.md' (Get-FileHash $status).Hash
    $manifest=Join-Path $destination 'package-manifest.json'
    @{completeSdk=$false;releaseQualified=$false;files=@($rows|ForEach-Object {@{path=$_.name;sha256=$_.sha256;bytes=$_.bytes}})}|ConvertTo-Json -Depth 5|Set-Content -LiteralPath $manifest -Encoding utf8
    AddFile $manifest 'package-manifest.json' (Get-FileHash $manifest).Hash
    $archive=Join-Path $destination 'Lea-SDK-development.zip'
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{foreach($row in $rows){$entry=$zip.CreateEntry($row.name,[IO.Compression.CompressionLevel]::Optimal);$output=$entry.Open();try{$row.stream.Position=0;$row.stream.CopyTo($output)}finally{$output.Dispose()}}}finally{$zip.Dispose()}
    $extract=Join-Path $destination 'verified-extraction'
    [IO.Compression.ZipFile]::ExtractToDirectory($archive,$extract)
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{
        if($zip.Entries.Count -ne $rows.Count){throw 'Archive coverage differs'}
        foreach($row in $rows){
            $entry=$zip.GetEntry($row.name);$inputStream=$entry.Open()
            try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($inputStream))}finally{$inputStream.Dispose()}
            if($entry.Length -ne $row.bytes -or $hash -cne $row.sha256 -or (Get-FileHash (Join-Path $extract $row.name)).Hash -cne $row.sha256){throw 'Archive/extraction readback differs'}
        }
    }finally{$zip.Dispose()}
    $result=@{verified=$true;archive=$archive;sha256=(Get-FileHash $archive).Hash;files=$rows.Count;sourceFiles=$sourceCount;runtimeFiles=$runtimeCount;bytes=(Get-Item $archive).Length;extraction=$extract;extractedRuntimeExecuted=$false;completeSdk=$false;githubUploaded=$false;runnerSha256=(Get-FileHash $PSCommandPath).Hash}
    $result|ConvertTo-Json -Depth 4|Set-Content -LiteralPath (Join-Path $destination 'result.json') -Encoding utf8
    $result|ConvertTo-Json
}finally{foreach($hold in $holds){$hold.Dispose()}}
