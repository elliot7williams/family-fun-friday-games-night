$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$output=Join-Path $workspace 'continuity-evidence/tested-interpreter-inputs-v1'
if((Test-Path -LiteralPath $output) -or (Test-Path -LiteralPath ($output+'.zip'))){throw 'Preserve export'}
if((Get-PSDrive C).Free -lt 512MB){throw 'Working reserve required'}
$receipt=Join-Path $root 'build-v1/result.json'
if((Get-FileHash -LiteralPath $receipt).Hash -cne '7D82A029AA5977798CF8B323920D606E21DFE4D803243914378EC1CAD8761D29'){throw 'Interpreter proof changed'}
$build=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
if(!$build.passed -or !$build.inputsUnchanged -or $build.error -or $build.runs.Count -ne 2){throw 'Incomplete source build'}
$nativeProof=Join-Path $root 'native-pair-v2/literal-input-verification.json'
if((Get-FileHash -LiteralPath $nativeProof).Hash -cne '98A773D0A4B40B2D116F8BAEA1B4CF1F20D5D9C6607C1FEB297EE4805C9428C8'){throw 'Native proof changed'}
$native=Get-Content -LiteralPath $nativeProof -Raw|ConvertFrom-Json
if(!$native.passed){throw 'Native validation incomplete'}
$lazy=Join-Path $root 'native-pair-v2/lazy/Jint.dll'
if((Get-FileHash -LiteralPath $lazy).Hash -cne '67F3E7D75EDADCBC0110FD6BB7AC26E56784D3650A83C2AE0AA23DD67EB03E6D'){throw 'Tested interpreter changed'}
$null=New-Item -ItemType Directory -Path $output
$records=[Collections.Generic.List[object]]::new();$seen=@{}
function CopyInput([string]$Path,[string]$Hash,[string]$Role){
    $full=[IO.Path]::GetFullPath($Path)
    if(!$full.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Source export outside workspace'}
    if($seen.ContainsKey($full)){if($seen[$full] -cne $Hash){throw 'Conflicting source hash'};return}
    if((Get-FileHash -LiteralPath $full).Hash -cne $Hash){throw "Changed build input: $full"}
    $seen[$full]=$Hash
    $relative='files/'+$records.Count.ToString('D4')+'/'+[IO.Path]::GetFileName($full)
    $target=Join-Path $output $relative
    $null=New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($target)) -Force
    Copy-Item -LiteralPath $full -Destination $target
    if((Get-FileHash -LiteralPath $target).Hash -cne $Hash){throw 'Copy mismatch'}
    $records.Add(@{originalPath=$full;archivePath=$relative;sha256=$Hash;role=$Role})
}
foreach($input in $build.inputs){
    $full=[IO.Path]::GetFullPath($input.path)
    if($full.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase) -and [IO.Path]::GetExtension($full) -cin @('.cs','.txt','.snk','.props','.csproj','.ps1')){
        CopyInput $full $input.sha256 'Pinned control/lazy build input; not an ordered compilation source list'
    }
}
if(@($records|Where-Object{[IO.Path]::GetExtension($_.originalPath) -ceq '.cs'}).Count -ne 714){throw 'Expected714 distinct recorded C# inputs'}
foreach($name in @('LICENSE.txt','CREDITS.txt','LikelySubtags.txt')){if(@($records|Where-Object{[IO.Path]::GetFileName($_.originalPath) -ceq $name}).Count -ne 1){throw 'Upstream notice/resource missing'}}
CopyInput $receipt '7D82A029AA5977798CF8B323920D606E21DFE4D803243914378EC1CAD8761D29' 'IL build receipt'
CopyInput $nativeProof '98A773D0A4B40B2D116F8BAEA1B4CF1F20D5D9C6607C1FEB297EE4805C9428C8' 'Supplemental native verification; original native receipt failure retained separately'
$raw=Join-Path $root 'native-pair-v2/result.json'
CopyInput $raw '20835577F4B1470E96F63C91553F2F9D1C5F12F20EEA10EBD16167F53779DB92' 'Original raw native receipt, still failed'
$sourceProof=Join-Path $workspace 'continuity-evidence/modern-worker-v1/interpreter-source-v1/result.json'
CopyInput $sourceProof 'D013E5CCEDED3C5CC52C94EBE87E3E9CF2A4629CD4DCE750A6B234BCF06F3E28' 'Upstream source provenance'
foreach($name in @('build-v1/TemporalStartupCompiler.cs','native-pair-v2/build.ps1','native-pair-v2/lazy/native.rsp')){
    $path=Join-Path $root $name
    if(Test-Path -LiteralPath $path){CopyInput $path (Get-FileHash -LiteralPath $path).Hash 'Retained recipe captured now; historical source pin not asserted'}
}
$scope='Private preservation of pinned interpreter control/lazy build inputs, including upstream notices, generated sources and overlays. The 714 C# input inventory is not the ordered 698-file lazy compilation list (plus generated assembly metadata). Original path mapping retained. References/toolchain/binaries, parser sources and complete portable rebuild driver are not included. No independent rebuild, license clearance, remote backup or release claim.'
[IO.File]::WriteAllText((Join-Path $output 'README.txt'),$scope)
[IO.File]::WriteAllText((Join-Path $output 'manifest.json'),(@{scope=$scope;files=$records;originalInputs=$build.inputs;testedNativeSha256='67F3E7D75EDADCBC0110FD6BB7AC26E56784D3650A83C2AE0AA23DD67EB03E6D'}|ConvertTo-Json -Depth 7))
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
[pscustomobject]@{verified=$true;files=$files.Count;copiedInputs=$records.Count;archive=$archive;sha256=(Get-FileHash -LiteralPath $archive).Hash}|ConvertTo-Json
