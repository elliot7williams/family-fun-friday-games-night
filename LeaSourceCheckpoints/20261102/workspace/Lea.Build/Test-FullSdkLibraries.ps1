param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$stage='I:\LeaBuildStorage\full-sdk-source-20261101-v1'
$out='I:\LeaBuildStorage\full-sdk-libraries-build-20261101-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained foreground owner required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Test-Path (Join-Path $stage 'bin')) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh output/source-only build and storage reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.EmbeddingSample','Lea.ScriptEmbeddingSample','SdkScriptLocal' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$pin){
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($pin -and $hash -cne $pin){throw ('Full SDK input differs: '+$path)}
    $inputs.Add(@{path=$path;sha256=$hash})
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;compiled=$false;nativeWorkerCompiled=$false;nativeWorkerQualified=$false;completeSdk=$false;inputs=$inputs;runs=$runs}
try{
    Hold $PSCommandPath
    Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $manifest=Join-Path $stage 'source-manifest.json';Hold $manifest 'C32C06E18945BEEFDA035E4657BA8B30D2350012FD6B9DAC33FEFAB3D58100A3'
    $source=Get-Content $manifest -Raw|ConvertFrom-Json
    if(!$source.verified -or $source.files.Count -ne 128 -or !$source.workerProjectInverseVerified -or $source.productBinariesIncluded -or $source.thirdPartyDependencyFiles -ne 8){throw 'Source-only aggregate required'}
    foreach($file in $source.files){Hold (Join-Path $stage $file.path) $file.sha256}
    $compiler='C:\Program Files\dotnet\dotnet.exe';Hold $compiler
    $toolchain='C:\Program Files\dotnet\sdk\8.0.427'
    foreach($name in @('MSBuild.dll','Microsoft.Build.dll','Roslyn/bincore/csc.dll','Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll')){Hold (Join-Path $toolchain $name)}
    function Build([string]$name,[string]$project,[string]$target){
        $args=@(('"'+(Join-Path $toolchain 'MSBuild.dll')+'"'),('"'+$project+'"'),'/nologo','/m:1','/nodeReuse:false',('/t:'+$target),'/v:minimal','/p:Configuration=Release','/p:UseSharedCompilation=false')
        $run=Invoke-OwnedTestRun -Name $name -Application $compiler -Arguments $args -WorkingDirectory $stage -OutputDirectory $out -Seconds 120 -Records $runs
        if(!$run.passed){throw ('Aggregate project build failed: '+$name)}
    }
    Build 'build-framework-libraries' (Join-Path $stage 'SDK.Build.proj') 'BuildLibraries'
    $products=@(foreach($name in @('Lea.Engine.dll','Lea.Win.dll','Lea.Image.exe','Lea.Print.dll','Lea.Script.dll','Lea.Script.Win.dll')){
        $path=Join-Path $stage ('bin/Release/'+$name);Hold $path;@{path=$path;sha256=(Get-FileHash $path).Hash}
    })
    $result.compiled=$true;$result.libraryProducts=$products
    if(Test-Path (Join-Path $stage 'bin/Release/ScriptWorker')){throw 'Unexpected native worker substitution'}
    Build 'build-text-sample' (Join-Path $stage 'Sample/EmbeddingSample.csproj') 'Build'
    $sample=Join-Path $stage 'Sample/bin/Release/Lea.EmbeddingSample.exe';Hold $sample
    $sampleRun=Invoke-OwnedTestRun -Name 'test-text-sample' -Application $sample -Arguments @('--test',('"'+(Join-Path $stage 'Sample/embedding.html')+'"'),('"'+$out+'"')) -WorkingDirectory (Split-Path $sample) -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$sampleRun.passed -or @(Get-Content $sampleRun.stdout|Where-Object {$_ -ceq 'PASS LEA SDK EMBEDDING 14'}).Count -ne 1){throw 'New aggregate text embedding failed'}
    $probeSource=Join-Path $root 'Lea.Tests/SdkParentIoContractMain.cs';Hold $probeSource
    $probe=Join-Path $out 'ParentIoContract.exe';$csc=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe';Hold $csc
    $args=@('/nologo','/noconfig','/nostdlib+','/target:exe','/platform:x64','/warnaserror+',('/out:"'+$probe+'"'))
    $refs='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    foreach($name in @('mscorlib.dll','System.dll','System.Core.dll','System.Drawing.dll','System.Windows.Forms.dll')){$path=Join-Path $refs $name;Hold $path;$args+=('/reference:"'+$path+'"')}
    foreach($name in @('Lea.Engine.dll','Lea.Win.dll','Lea.Script.dll')){$path=Join-Path $stage ('bin/Release/'+$name);$args+=('/reference:"'+$path+'"')}
    $args+=('"'+$probeSource+'"')
    $compiled=Invoke-OwnedTestRun -Name 'compile-parent-contract' -Application $csc -Arguments $args -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compiled.passed){throw 'New parent contract compile failed'};Hold $probe
    $probeRuntime=Join-Path $out 'contract-runtime';New-Item -ItemType Directory -Path $probeRuntime|Out-Null
    foreach($file in Get-ChildItem (Join-Path $stage 'bin/Release') -File -Filter '*.dll'){
        Hold $file.FullName;$to=Join-Path $probeRuntime $file.Name;Copy-Item -LiteralPath $file.FullName -Destination $to;Hold $to (Get-FileHash $file.FullName).Hash
    }
    $to=Join-Path $probeRuntime 'ParentIoContract.exe';Copy-Item -LiteralPath $probe -Destination $to;Hold $to (Get-FileHash $probe).Hash
    $contract=Invoke-OwnedTestRun -Name 'test-parent-contract' -Application $to -Arguments @(('"'+(Join-Path $stage 'Sample/embedding.html')+'"')) -WorkingDirectory $probeRuntime -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$contract.passed -or @(Get-Content $contract.stdout|Where-Object {$_ -ceq 'PASS SDK PARENT IO CONTRACT 3'}).Count -ne 1){throw 'New aggregate parent contract failed'}
    $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaFullSdkLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object compiled,passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
