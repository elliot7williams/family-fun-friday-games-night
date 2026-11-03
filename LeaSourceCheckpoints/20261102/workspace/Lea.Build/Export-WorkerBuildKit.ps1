$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$out=Join-Path $workspace 'continuity-evidence/worker-build-kit-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve worker kit'}
if((Get-PSDrive C).Free -lt 768MB){throw 'Kit reserve required'}
$null=New-Item -ItemType Directory -Path $out
$leases=[Collections.Generic.List[IDisposable]]::new();$files=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Hash){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $Hash){throw 'Worker kit input differs'}
}
$kit=Join-Path $out 'kit';$null=New-Item -ItemType Directory -Path $kit
function CopyPinned([string]$Path,[string]$Hash,[string]$Relative){
    Hold $Path $Hash
    $target=Join-Path $kit $Relative
    if(Test-Path -LiteralPath $target){throw 'Duplicate kit file'}
    $null=New-Item -ItemType Directory -Path (Split-Path $target) -Force
    Copy-Item -LiteralPath $Path -Destination $target
    if((Get-FileHash -LiteralPath $target).Hash -cne $Hash){throw 'Kit copy differs'}
    $files.Add(@{path=$Relative;sha256=$Hash})
}
$result=@{passed=$false;error=$null;compilerExecuted=$false;runtimeExecuted=$false;cleanPcValidated=$false}
try{
    $archive=Join-Path $workspace 'continuity-evidence/tested-worker-source-v1.zip'
    Hold $archive '25516AAAEDDEE8CF5FF058F343D8485F1AB8565C87FB546A7DC5F2A440863A79'
    $source=Join-Path $out 'source-archive';[IO.Compression.ZipFile]::ExtractToDirectory($archive,$source)
    $old=Get-Content -LiteralPath (Join-Path $source 'manifest.json') -Raw|ConvertFrom-Json
    $mapping=@{};foreach($entry in $old.files){$mapping[$entry.originalPath]=$entry}
    $references=@{};foreach($entry in $old.references){$references[$entry.path]=$entry.sha256}
    $sources=[Collections.Generic.List[string]]::new();$refs=[Collections.Generic.List[string]]::new();$options=[Collections.Generic.List[string]]::new();$resources=0;$outputs=0
    foreach($line in Get-Content -LiteralPath (Join-Path $source 'recipes/worker.rsp')){
        if($line -match '^"(.+\.cs)"$'){
            $entry=$mapping[$Matches[1]];if(!$entry){throw 'Source mapping absent'}
            CopyPinned (Join-Path $source $entry.archivePath) $entry.sha256 $entry.archivePath
            $sources.Add($entry.archivePath)
        }elseif($line -match '^/reference:"([^"]+)"$'){
            $path=$Matches[1];$hash=$references[$path];if(!$hash){throw 'Reference mapping absent'}
            $relative='references/'+$hash+'/'+[IO.Path]::GetFileName($path)
            CopyPinned $path $hash $relative;$refs.Add($relative)
        }elseif($line -match '^/resource:"([^"]+)",Lea.Script.Dom.js$'){
            $entry=$mapping[$Matches[1]];if(!$entry){throw 'Resource mapping absent'}
            CopyPinned (Join-Path $source $entry.archivePath) $entry.sha256 'resource/Dom.compact.js';$resources++
        }elseif($line -match '^/out:'){$outputs++}
        elseif($line -in @('/nologo','/nostdlib+','/target:exe','/platform:anycpu','/langversion:12.0','/optimize+','/deterministic+','/warn:4','/warnaserror+','/define:TRACE;RELEASE;NET8_0')){$options.Add($line)}
        else{throw 'Unsupported worker recipe option'}
    }
    if($sources.Count -ne 17 -or $refs.Count -ne 165 -or $resources -ne 1 -or $outputs -ne 1 -or $options.Count -ne 10){throw 'Kit input inventory differs'}
    CopyPinned (Join-Path $PSScriptRoot 'Build-WorkerFromKit.ps1') 'FBDE12C067FE4E3646A97121CD7957B406DA5381B433E930168BEDEA5A747338' 'Build.ps1'
    CopyPinned (Join-Path $PSScriptRoot 'PublicCompiler.cs') 'D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB' 'compiler/PublicCompiler.cs'
    $scope='Private local worker IL build kit. Includes target source/resource/references. Requires Windows PowerShell 7 host with exactly pinned signed Roslyn DLLs; compiler host/runtime not bundled. No SDK/old-checkout inputs needed by Build.ps1. Not full browser/ReadyToRun/apphost kit, clean-PC certification or license clearance. Preserve source copyright headers; redistribution review required.'
    [IO.File]::WriteAllText((Join-Path $kit 'NOTICE.txt'),$scope)
    $files.Add(@{path='NOTICE.txt';sha256=(Get-FileHash -LiteralPath (Join-Path $kit 'NOTICE.txt')).Hash})
    $map=@{format=1;scope=$scope;sources=$sources;references=$refs;resource='resource/Dom.compact.js';options=$options;files=$files}
    $manifest=Join-Path $kit 'manifest.json';[IO.File]::WriteAllText($manifest,($map|ConvertTo-Json -Depth 6))
    $zip=Join-Path $out 'Lea-worker-IL-build-kit.zip';[IO.Compression.ZipFile]::CreateFromDirectory($kit,$zip,[IO.Compression.CompressionLevel]::Optimal,$false)
    $result.archive=$zip;$result.archiveSha256=(Get-FileHash -LiteralPath $zip).Hash
    $result.manifestSha256=(Get-FileHash -LiteralPath $manifest).Hash;$result.files=$files.Count;$result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    foreach($lease in $leases){$lease.Dispose()}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 5))
}
[pscustomobject]$result|ConvertTo-Json -Depth 5
