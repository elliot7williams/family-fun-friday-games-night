param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$source='I:\LeaBuildStorage\full-sdk-source-20261101-v3'
$out='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh reproducibility evidence/owner and original reserve required'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','Lea.EmbeddingSample','Lea.ScriptEmbeddingSample','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'No test/browser overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new();$products=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$expected){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse reproducibility input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($expected -and $hash -cne $expected){throw ('Reproducibility input differs: '+$path)};$inputs.Add(@{path=$path;sha256=$hash})
}
function Proof([string]$path,[string]$hash){Hold $path $hash;return Get-Content -LiteralPath $path -Raw|ConvertFrom-Json}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;binaryReproducible=$false;runtimeQualified=$false;fullSdkQualified=$false;inputs=$inputs;runs=$runs;products=$products;roots=@()}
try{
    Hold $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $manifest=Proof (Join-Path $source 'source-manifest.json') 'B5A0283803C7062DA98A7F3331D94940360E00045A06038168FCA94214CD114A'
    if(!$manifest.verified -or !$manifest.deterministicSettingsIncluded -or !$manifest.portableNativeBuildIncluded -or $manifest.productBinariesIncluded -or $manifest.files.Count -ne 132){throw 'Source-only deterministic candidate required'}
    $toolchain='I:\LeaBuildStorage\portable-native-build-20261101-v1/toolchain.json';Hold $toolchain '7B13FE166AD9AF8A3B142C40CB046EAABA1A33759B5A9C505F97B7A83678C003'
    $config=Get-Content $toolchain -Raw|ConvertFrom-Json
    foreach($file in $config.compilerFiles){Hold (Join-Path $config.compilerRoot $file.path) $file.sha256}
    foreach($file in $config.runtimeFiles){Hold (Join-Path $config.runtimeRoot $file.path) $file.sha256}
    $result.roots=@((Join-Path $out 'a'),(Join-Path $out 'different-depth/longer-source-directory-b'))
    foreach($directory in $result.roots){foreach($file in $manifest.files){
        if([IO.Path]::IsPathRooted($file.path) -or $file.path.Contains(':') -or $file.path.Split([char[]]@('/','\')) -contains '..'){throw 'Source copy escape'}
        $from=Join-Path $source $file.path;Hold $from $file.sha256;$to=Join-Path $directory $file.path
        New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null;Copy-Item -LiteralPath $from -Destination $to;Hold $to $file.sha256
    }}
    $pwsh=(Get-Process -Id $PID).Path;Hold $pwsh
    $dotnet='C:\Program Files\dotnet\dotnet.exe';Hold $dotnet;$tools='C:\Program Files\dotnet\sdk\8.0.427'
    foreach($name in @('MSBuild.dll','Microsoft.Build.dll','Roslyn/bincore/csc.dll','Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll')){Hold (Join-Path $tools $name)}
    for($i=0;$i -lt 2;$i++){
        if((Get-PSDrive C).Free -lt 600MB){throw 'Original build reserve lost'}
        $directory=$result.roots[$i]
        $arguments=@(('"'+(Join-Path $tools 'MSBuild.dll')+'"'),('"'+(Join-Path $directory 'SDK.Build.proj')+'"'),'/nologo','/m:1','/nodeReuse:false','/t:Ba','/v:minimal','/p:Configuration=Release','/p:UseSharedCompilation=false',('/p:LeaPowerShell="'+$pwsh+'"'),('/p:LeaNativeToolchain="'+$toolchain+'"'),'/p:LeaNativeToolchainSha256=7B13FE166AD9AF8A3B142C40CB046EAABA1A33759B5A9C505F97B7A83678C003')
        $built=Invoke-OwnedTestRun -Name ('build-'+$i) -Application $dotnet -Arguments $arguments -WorkingDirectory $directory -OutputDirectory $out -Seconds 120 -Records $runs
        if(!$built.passed -or @(Get-Content $built.stdout|Where-Object {$_ -match 'LEA_NATIVE_BUILT'}).Count -ne 1){throw 'Fresh complete SDK build failed'}
    }
    $relative=@('bin/Release/Lea.Engine.dll','bin/Release/Lea.Win.dll','bin/Release/Lea.Image.exe','bin/Release/Lea.Print.dll','bin/Release/Lea.Script.dll','bin/Release/Lea.Script.Win.dll','worker-il/Release/Lea.Script.Worker.dll','worker-il/Release/Lea.Script.Worker.pdb','Sample/bin/Release/Lea.EmbeddingSample.exe','ScriptSample/bin/Release/Lea.ScriptEmbeddingSample.exe')
    $relative+=@('Lea.Script.Worker.exe','Lea.Script.Worker.dll','Lea.Script.Worker.runtimeconfig.json','Lea.Script.Worker.deps.json','Jint.dll','Acornima.dll')|ForEach-Object {'bin/Release/ScriptWorker/'+$_}
    foreach($file in $relative){
        $a=Join-Path $result.roots[0] $file;$b=Join-Path $result.roots[1] $file;Hold $a;Hold $b
        $ha=(Get-FileHash $a).Hash;$hb=(Get-FileHash $b).Hash
        $products.Add(@{path=$file;first=$a;second=$b;sha256=$ha;secondSha256=$hb;equal=($ha -ceq $hb)})
    }
    $result.differences=@($products|Where-Object {!$_.equal})
    if($products.Count -ne 16 -or $result.differences.Count){throw 'Cross-directory product identities differ'}
    $reader=Join-Path $PSScriptRoot 'SdkBuildIdentity.cs';Hold $reader;Add-Type -Path $reader
    $result.metadata=@(foreach($file in @('bin/Release/Lea.Engine.dll','bin/Release/Lea.Win.dll','bin/Release/Lea.Script.dll','bin/Release/ScriptWorker/Lea.Script.Worker.dll')){
        $info=[SdkBuildIdentity]::Read((Join-Path $result.roots[0] $file))
        if(!$info.reproducibleDebugEntry -or @($info.codeViewPaths|Where-Object {$_ -notlike '/_/lea/*'}).Count){throw 'Deterministic marker/virtual debug path absent'};$info
    })
    $result.binaryReproducible=$true;$result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaSdkReproducibilityLeases=$holds}
    $result|ConvertTo-Json -Depth 10|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,binaryReproducible,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
