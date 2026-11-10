param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root='C:\Users\Elliot\Documents\Family Fun Friday Games Night copy'
$out='I:\LeaBuildStorage\memory-policy-sdk-source-20261110-v1'
$source=Join-Path $out 'source'
$runtime=Join-Path $root 'continuity-evidence/memory-policy-sdk-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 650MB -or (Get-PSDrive I).Free -lt 256MB -or (Test-Path -LiteralPath (Join-Path $out 'result.json')) -or (Test-Path -LiteralPath $runtime) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/runtime and original reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image','Lea.ScriptEmbeddingSample' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Pin([string]$Path,[string]$Expected){
    $item=Get-Item -LiteralPath $Path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($stream)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($Expected -and $hash -cne $Expected){throw ('Input differs: '+$Path)};$inputs.Add(@{path=$Path;sha256=$hash})
}
function Copy-Pinned([string]$From,[string]$To,[string]$Expected){Pin $From $Expected;New-Item -ItemType Directory -Path (Split-Path $To -Parent) -Force|Out-Null;Copy-Item -LiteralPath $From -Destination $To;Pin $To (Get-FileHash -LiteralPath $From).Hash}
$result=@{passed=$false;inputs=$inputs;runs=$runs;before=@();completeSdk=$false;startupFixed=$false;hostAdopted=$false;fullScriptSuiteQualified=$false;globalSecurityQualified=$false}
$environment=@{};foreach($name in @('TEMP','TMP','DOTNET_CLI_HOME','NUGET_PACKAGES')){$environment[$name]=[Environment]::GetEnvironmentVariable($name,'Process')}
try{
    Pin $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'test.ps1')
    $manifestPath=Join-Path $out 'source.json';Pin $manifestPath '21414F48C7A32E6B2ED0865CBA79A8DE3018A4B581075A7244E5ED534AB7E183'
    $manifest=Get-Content -LiteralPath $manifestPath -Raw|ConvertFrom-Json
    if(!$manifest.verified -or $manifest.sourceMembers -ne 128 -or $manifest.dependencyMembers -ne 8 -or $manifest.files.Count -ne 136){throw 'Current SDK source inventory differs'}
    foreach($file in $manifest.files){Pin (Join-Path $source $file.path) $file.sha256}
    if(@(Get-ChildItem -LiteralPath $source -Recurse -File|Where-Object {$_.Extension -in @('.dll','.exe') -and !$_.FullName.StartsWith((Join-Path $source 'lib')+'\',[StringComparison]::OrdinalIgnoreCase)}).Count){throw 'No seeded Lea binaries allowed'}
    $toolPath=Join-Path $root 'continuity-evidence/modern-worker-v1/crossgen2-package-v4/result.json';Pin $toolPath '60D063F3D3F3F0C5E9036A4416C75F7590C701BAF84E15E964A0A1D53199B3EF'
    $tool=Get-Content -LiteralPath $toolPath -Raw|ConvertFrom-Json
    if(!$tool.passed -or $tool.version -cne '8.0.32' -or $tool.verifyExitCode -ne 0){throw 'Retained authenticated native compiler provenance required'}
    $compilers=@($tool.files|Where-Object path -match '\\tools\\crossgen2\.exe$')
    if($compilers.Count -ne 1){throw 'Unique pinned native compiler required'}
    foreach($file in $tool.files|Where-Object path -match '\\tools\\'){Pin $file.path $file.sha256}
    $owner=Join-Path 'C:\Users\Elliot\Documents\Family Fun Friday Games Night copy\Lea.Build' 'OwnedTestProcess.cs';Pin $owner 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owner
    $helper=Join-Path 'C:\Users\Elliot\Documents\Family Fun Friday Games Night copy\Lea.Build' 'Invoke-OwnedTestRun.ps1';Pin $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $dotnet='C:\Program Files\dotnet\dotnet.exe';Pin $dotnet
    $tools='C:\Program Files\dotnet\sdk\8.0.427';$msbuild=Join-Path $tools 'MSBuild.dll';Pin $msbuild
    foreach($name in @('Roslyn/bincore/csc.dll','Roslyn/bincore/csc.deps.json','Roslyn/bincore/csc.runtimeconfig.json','Roslyn/bincore/Microsoft.CodeAnalysis.dll','Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll')){Pin (Join-Path $tools $name)}
    $pwsh=(Get-Process -Id $PID).Path;Pin $pwsh
    $env:TEMP=Join-Path $out 'temp';$env:TMP=$env:TEMP;$env:DOTNET_CLI_HOME=Join-Path $out 'cli-home';$env:NUGET_PACKAGES=Join-Path $out 'nuget'
    foreach($path in @($env:TEMP,$env:DOTNET_CLI_HOME,$env:NUGET_PACKAGES)){New-Item -ItemType Directory -Path $path|Out-Null}
    $toolchain=Join-Path $out 'toolchain.json'
    $recorded=Invoke-OwnedTestRun -Name 'record-native-toolchain' -Application $pwsh -Arguments @('-NoProfile','-NonInteractive','-File',('"'+(Join-Path $source 'New-NativeToolchain.ps1')+'"'),'-CompilerRoot',('"'+(Split-Path $compilers[0].path -Parent)+'"'),'-RuntimeRoot','"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.33"','-Output',('"'+$toolchain+'"')) -WorkingDirectory $source -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$recorded.passed){throw 'Native toolchain record failed'};Pin $toolchain
    $config=Get-Content -LiteralPath $toolchain -Raw|ConvertFrom-Json
    foreach($file in $config.runtimeFiles){Pin (Join-Path $config.runtimeRoot $file.path) $file.sha256}
    function Build-Target([string]$Name,[string]$Target,[string[]]$Extra){
        $arguments=@(('"'+$msbuild+'"'),('"'+(Join-Path $source 'SDK.Build.proj')+'"'),'/nologo','/m:1','/nodeReuse:false',('/t:'+$Target),'/v:minimal','/p:Configuration=Release','/p:UseSharedCompilation=false')+@($Extra)
        return Invoke-OwnedTestRun -Name $Name -Application $dotnet -Arguments $arguments -WorkingDirectory $source -OutputDirectory $out -Seconds 120 -Records $runs
    }
    $framework=Build-Target 'current-framework-libraries' 'Bl' @()
    if(!$framework.passed){throw 'Current framework source build failed'}
    $il=Build-Target 'current-worker-il' 'Bi' @('/p:NuGetAudit=false')
    if(!$il.passed){throw 'Offline worker IL build failed'}
    foreach($name in @('Lea.Script.Worker.exe','Lea.Script.Worker.dll','Lea.Script.Worker.runtimeconfig.json','Lea.Script.Worker.deps.json','Jint.dll','Acornima.dll')){Pin (Join-Path $source ('worker-il/Release/'+$name))}
    $native=Build-Target 'current-worker-native' 'CompileWorkerNative' @(('/p:LeaPowerShell="'+$pwsh+'"'),('/p:LeaNativeToolchain="'+$toolchain+'"'),('/p:LeaNativeToolchainSha256='+(Get-FileHash -LiteralPath $toolchain).Hash))
    if(!$native.passed -or @(Get-Content -LiteralPath $native.stdout|Where-Object {$_ -match 'LEA_NATIVE_BUILT'}).Count -ne 1){throw 'Current worker native build failed'}
    $result.nativeProducts=@(Get-ChildItem -LiteralPath (Join-Path $source 'bin/Release/ScriptWorker') -File|ForEach-Object {Pin $_.FullName;@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;bytes=$_.Length}})
    if($result.nativeProducts.Count -ne 6){throw 'Six-file native worker closure required'}
    $samples=Build-Target 'current-consumer-samples' 'Bs' @()
    if(!$samples.passed){throw 'Current SDK sample builds failed'}
    $result.libraryProducts=@(foreach($name in @('Lea.Engine.dll','Lea.Win.dll','Lea.Image.exe','Lea.Print.dll','Lea.Script.dll','Lea.Script.Win.dll')){$path=Join-Path $source ('bin/Release/'+$name);Pin $path;@{path=$path;sha256=(Get-FileHash -LiteralPath $path).Hash}})
    $closures=@(@{source=(Join-Path $source 'Sample/bin/Release');target=(Join-Path $runtime 'text')},@{source=(Join-Path $source 'ScriptSample/bin/Release');target=(Join-Path $runtime 'script')})
    $bytes=($closures|ForEach-Object {Get-ChildItem -LiteralPath $_.source -Recurse -File}|Measure-Object Length -Sum).Sum
    if((Get-PSDrive C).Free -lt (600MB+$bytes+1MB)){throw 'Actual runtime allocation plus original reserve required'}
    foreach($closure in $closures){foreach($file in Get-ChildItem -LiteralPath $closure.source -Recurse -File){Copy-Pinned $file.FullName (Join-Path $closure.target ([IO.Path]::GetRelativePath($closure.source,$file.FullName))) ''}}
    $scriptRoot=Join-Path $runtime 'script';$gate=Join-Path $scriptRoot 'CurrentSdkWorkerGate.exe'
    $gateSource=Join-Path $root 'Lea.Tests/SdkScriptWorkerMain.cs';Pin $gateSource;Copy-Item -LiteralPath $gateSource -Destination (Join-Path $out 'SdkScriptWorkerMain.cs')
    Copy-Pinned (Join-Path $root 'Lea.Sdk/ScriptHost.config') ($gate+'.config') ''
    Copy-Pinned (Join-Path $root 'LeaForge/test-pages/sdk-script-local.html') (Join-Path $scriptRoot 'sdk-worker-gate.html') ''
    $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe';Pin $compiler
    $arguments=@('/nologo','/noconfig','/nostdlib+','/target:exe','/platform:x64','/warnaserror+',('/out:"'+$gate+'"'),('"'+$gateSource+'"'))
    $refs='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    foreach($name in @('mscorlib.dll','System.dll','System.Core.dll','System.Drawing.dll','System.Windows.Forms.dll')){$path=Join-Path $refs $name;Pin $path;$arguments+=('/reference:"'+$path+'"')}
    foreach($name in @('Lea.Engine.dll','Lea.Win.dll','Lea.Script.dll','Lea.Script.Win.dll','Jint.dll','Acornima.dll')){$arguments+=('/reference:"'+(Join-Path $scriptRoot $name)+'"')}
    $compiled=Invoke-OwnedTestRun -Name 'compile-current-worker-gate' -Application $compiler -Arguments $arguments -WorkingDirectory $scriptRoot -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compiled.passed){throw 'Current worker gate compilation failed'};Pin $gate
    $result.before=@(Get-ChildItem -LiteralPath $runtime -Recurse -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    $text=Join-Path $runtime 'text';$pictures=Join-Path $out 'text';New-Item -ItemType Directory -Path $pictures|Out-Null
    $textRun=Invoke-OwnedTestRun -Name 'current-text-consumer' -Application (Join-Path $text 'Lea.EmbeddingSample.exe') -Arguments @('--test',('"'+(Join-Path $text 'embedding.html')+'"'),('"'+$pictures+'"')) -WorkingDirectory $text -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$textRun.passed -or @(Get-Content -LiteralPath $textRun.stdout|Where-Object {$_ -ceq 'PASS LEA SDK EMBEDDING 14'}).Count -ne 1){throw 'Current text consumer failed'};$result.textChecks=14
    $scriptRun=Invoke-OwnedTestRun -Name 'current-script-consumer' -Application (Join-Path $scriptRoot 'Lea.ScriptEmbeddingSample.exe') -Arguments @('--test',('"'+$scriptRoot+'"')) -WorkingDirectory $scriptRoot -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$scriptRun.passed -or @(Get-Content -LiteralPath $scriptRun.stdout|Where-Object {$_ -ceq 'PASS LEA SDK SCRIPT SAMPLE 17'}).Count -ne 1){throw 'Current script consumer failed'};$result.scriptChecks=17
    $gateRun=Invoke-OwnedTestRun -Name 'current-isolated-worker-gate' -Application $gate -Arguments @('"'+(Join-Path $scriptRoot 'sdk-worker-gate.html')+'"') -WorkingDirectory $scriptRoot -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$gateRun.passed -or @(Get-Content -LiteralPath $gateRun.stdout|Where-Object {$_ -ceq 'PASS SDK WORKER 25'}).Count -ne 1){throw 'Current isolated worker gate failed'};$result.workerChecks=25
    $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    foreach($name in $environment.Keys){[Environment]::SetEnvironmentVariable($name,$environment[$name],'Process')}
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@($result.before|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256 -or (Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention -and !$result.error
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaCurrentWorkerInputLeases=$holds}
    $result|ConvertTo-Json -Depth 9|Set-Content -LiteralPath (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,textChecks,scriptChecks,workerChecks,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
