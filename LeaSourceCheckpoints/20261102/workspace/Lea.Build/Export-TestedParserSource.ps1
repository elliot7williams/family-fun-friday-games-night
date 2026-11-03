$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$modern=Join-Path $workspace 'continuity-evidence/modern-worker-v1'
$output=Join-Path $workspace 'continuity-evidence/tested-parser-source-v1'
if((Test-Path -LiteralPath $output) -or (Test-Path -LiteralPath ($output+'.zip'))){throw 'Preserve parser export'}
if((Get-PSDrive C).Free -lt 512MB){throw 'Working reserve required'}
$proofs=@(
    @('parser-source-v1/result.json','7B38DA222065CF56B72C5D6F033734528953A8AB00492A9CADAD81F26069904C'),
    @('parser-generation-check-v1/result.json','F8479476818B98EDFF2BB64702E575CB46BAF02C5787CA810EEAD6306A60CCC3'),
    @('parser-variant-v1/result.json','2CB04DC08D2CE053AD37A24F4AC7BA7B97EF4A1ACF008E052962EBB7D18E41DD'),
    @('parser-r2r-v1/result.json','29A5D11C910074586164BAE609433ACE8980B239632123F4EB82F0F2EADC9FDC')
)
foreach($pair in $proofs){if((Get-FileHash -LiteralPath (Join-Path $modern $pair[0])).Hash -cne $pair[1]){throw 'Parser proof changed'}}
$source=Get-Content -LiteralPath (Join-Path $modern $proofs[0][0]) -Raw|ConvertFrom-Json
$generation=Get-Content -LiteralPath (Join-Path $modern $proofs[1][0]) -Raw|ConvertFrom-Json
$variant=Get-Content -LiteralPath (Join-Path $modern $proofs[2][0]) -Raw|ConvertFrom-Json
$native=Get-Content -LiteralPath (Join-Path $modern $proofs[3][0]) -Raw|ConvertFrom-Json
if(!$source.passed -or !$generation.passed -or !$variant.passed -or !$variant.inputsUnchanged -or !$native.passed -or !$native.inputsUnchanged -or $variant.changes.Count -ne 9){throw 'Parser provenance incomplete'}
$il=Join-Path $modern 'parser-variant-v1/variant/Acornima.dll'
$binary=Join-Path $modern 'parser-r2r-v1/variant/Acornima.dll'
if((Get-FileHash -LiteralPath $il).Hash -cne 'C3374BB12569649776AE440B81AF167E276858971CD9A6C34A30A6BB12698B7B' -or (Get-FileHash -LiteralPath $binary).Hash -cne 'CE23BFEFCDD287E0DE38CD818DF643287CD4C6CDCAACB20C46C379631D5C8821'){throw 'Parser binary identity changed'}
$known=@{};foreach($input in $variant.inputs){$known[[IO.Path]::GetFullPath($input.path)]=$input.sha256}
$null=New-Item -ItemType Directory -Path $output
$files=[Collections.Generic.List[object]]::new();$seen=@{}
function CopyPinned([string]$Path,[string]$Hash,[string]$Role){
    $path=[IO.Path]::GetFullPath($Path)
    if(!$path.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Source outside workspace'}
    if($seen.ContainsKey($path)){if($seen[$path].sha256 -cne $Hash){throw 'Conflicting source pin'};return $seen[$path]}
    if((Get-FileHash -LiteralPath $path).Hash -cne $Hash){throw 'Parser source/input changed'}
    $relative='files/'+$files.Count.ToString('D4')+'/'+[IO.Path]::GetFileName($path)
    $target=Join-Path $output $relative
    $null=New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($target)) -Force
    Copy-Item -LiteralPath $path -Destination $target
    if((Get-FileHash -LiteralPath $target).Hash -cne $Hash){throw 'Copy differs'}
    $record=@{originalPath=$path;archivePath=$relative;sha256=$Hash;role=$Role}
    $files.Add($record);$seen[$path]=$record
    return $record
}
$sources=[Collections.Generic.List[object]]::new();$overlays=0
$sourceRoot=Join-Path $modern 'parser-source-v1/source'
foreach($file in $source.files|Where-Object{$_.relative.StartsWith('src/Acornima/',[StringComparison]::Ordinal) -and $_.relative.EndsWith('.cs',[StringComparison]::Ordinal)}|Sort-Object relative){
    $path=[IO.Path]::GetFullPath((Join-Path $sourceRoot $file.relative))
    $change=@($variant.changes|Where-Object Source -ceq $path)
    if($change.Count -gt 1){throw 'Ambiguous overlay'}
    if($change.Count -eq 1){
        if(!$change[0].InverseVerified){throw 'Overlay inverse unverified'}
        $null=CopyPinned $path $file.sha256 'Original pre-overlay source'
        $path=[IO.Path]::GetFullPath($change[0].Output);$overlays++
    }
    if(!$known.ContainsKey($path)){throw 'Selected source lacks compile pin'}
    $sources.Add((CopyPinned $path $known[$path] 'Ordered effective runtime source'))
}
if($sources.Count -ne 198 -or $overlays -ne 9){throw 'Runtime source coverage differs'}
foreach($file in $generation.generated|Sort-Object name){$sources.Add((CopyPinned (Join-Path $modern ('parser-generation-check-v1/generated/'+$file.name)) $file.sha256 'Ordered generated source'))}
if($sources.Count -ne 210 -or @($sources.archivePath|Sort-Object -Unique).Count -ne 210){throw 'Source order/count differs'}
$resources=@(foreach($file in $source.files|Where-Object{$_.relative.StartsWith('src/Acornima/Properties/',[StringComparison]::Ordinal) -and $_.relative.EndsWith('.resx',[StringComparison]::Ordinal)}|Sort-Object relative){CopyPinned (Join-Path $sourceRoot $file.relative) $file.sha256 'Embedded resource input'})
if($resources.Count -ne 3){throw 'Resource coverage differs'}
$extra=@{}
foreach($name in @('src/Karambolo.Public.snk','src/Acornima/Acornima.csproj','LICENSE')){
    $file=@($source.files|Where-Object relative -ceq $name);if($file.Count -ne 1){throw 'Missing license/project/key pin'}
    $extra[$name]=CopyPinned (Join-Path $sourceRoot $name) $file[0].sha256 'Upstream license/project/public signing key'
}
$compilerPath=Join-Path $modern 'ParserVariantCompiler.cs'
$compiler=CopyPinned $compilerPath $known[$compilerPath] 'Exact variant compiler helper'
foreach($pair in $proofs){$null=CopyPinned (Join-Path $modern $pair[0]) $pair[1] 'Original provenance receipt'}
foreach($name in @('Build-ParserVariant.ps1','ParserOptimizationPolicy.cs')){$path=Join-Path $modern $name;$null=CopyPinned $path $known[$path] 'Pinned variant build/policy source'}
$references=@($known.Keys|Where-Object{$_ -like '*\Microsoft.NETCore.App.Ref\8.0.32\ref\net8.0\*.dll'}|Sort-Object {[IO.Path]::GetFileName($_)}|ForEach-Object{@{path=$_;sha256=$known[$_];included=$false}})
$scope='Private exact production parser source preservation with ordered 210-file compilation map, nine optimization overlays, twelve generated units, three resources and upstream license/public signing material. External references/compiler/ReadyToRun toolchain not bundled. No independent archive rebuild, license clearance, portable kit or release claim.'
[IO.File]::WriteAllText((Join-Path $output 'README.txt'),$scope)
[IO.File]::WriteAllText((Join-Path $output 'manifest.json'),(@{scope=$scope;files=$files;orderedSources=$sources;resources=$resources;compiler=$compiler;extra=$extra;references=$references;expectedIlSha256='C3374BB12569649776AE440B81AF167E276858971CD9A6C34A30A6BB12698B7B';expectedNativeSha256='CE23BFEFCDD287E0DE38CD818DF643287CD4C6CDCAACB20C46C379631D5C8821'}|ConvertTo-Json -Depth 8))
$all=@(Get-ChildItem -LiteralPath $output -Recurse -File);$archive=$output+'.zip'
[IO.Compression.ZipFile]::CreateFromDirectory($output,$archive,[IO.Compression.CompressionLevel]::Optimal,$false)
$zip=[IO.Compression.ZipFile]::OpenRead($archive)
try{
    if($zip.Entries.Count -ne $all.Count){throw 'Archive count differs'}
    foreach($file in $all){
        $entry=$zip.GetEntry([IO.Path]::GetRelativePath($output,$file.FullName).Replace('\','/'))
        if(!$entry -or $entry.Length -ne $file.Length){throw 'Archive entry missing/truncated'}
        $stream=$entry.Open()
        try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($hash -cne (Get-FileHash -LiteralPath $file.FullName).Hash){throw 'Archive content differs'}
    }
}finally{$zip.Dispose()}
[pscustomobject]@{verified=$true;orderedSources=$sources.Count;overlays=$overlays;resources=$resources.Count;files=$all.Count;references=$references.Count;sha256=(Get-FileHash -LiteralPath $archive).Hash}|ConvertTo-Json
