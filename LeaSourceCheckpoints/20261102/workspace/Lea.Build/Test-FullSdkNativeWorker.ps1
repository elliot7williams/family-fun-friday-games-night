param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$stage='I:\LeaBuildStorage\full-sdk-source-20261101-v1'
$out='I:\LeaBuildStorage\full-sdk-native-build-20261101-v1'
$candidate=Join-Path $root 'continuity-evidence/full-sdk-script-sample-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained foreground owner required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Test-Path $candidate) -or (Test-Path (Join-Path $stage 'worker-il')) -or (Test-Path (Join-Path $stage 'bin/Release/ScriptWorker')) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh native candidate/owner/reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.EmbeddingSample','Lea.ScriptEmbeddingSample','SdkScriptLocal' -ErrorAction SilentlyContinue).Count){throw 'No test/browser overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$pin){
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($pin -and $hash -cne $pin){throw ('Native SDK input differs: '+$path)};$inputs.Add(@{path=$path;sha256=$hash})
}
function CopyVerified([string]$from,[string]$to){
    Hold $from;New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null
    Copy-Item -LiteralPath $from -Destination $to;Hold $to (Get-FileHash $from).Hash
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;workerIlCompiled=$false;workerNativeCompiled=$false;completeSdk=$false;hostAdopted=$false;inputs=$inputs;runs=$runs}
try{
    Hold $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $proof='I:\LeaBuildStorage\full-sdk-libraries-build-20261101-v1/result.json';Hold $proof '3AF0DA862CF1FA7C1E3E53E949B7EA0F37DE6593BEE8DDC5A02EFA8653522E6B'
    $libraries=Get-Content $proof -Raw|ConvertFrom-Json
    if(!$libraries.passed -or !$libraries.compiled -or !$libraries.inputsUnchanged -or $libraries.ownerAttention -or $libraries.libraryProducts.Count -ne 6){throw 'New aggregate libraries required'}
    foreach($file in $libraries.libraryProducts){Hold $file.path $file.sha256}
    $manifest=Join-Path $stage 'source-manifest.json';Hold $manifest 'C32C06E18945BEEFDA035E4657BA8B30D2350012FD6B9DAC33FEFAB3D58100A3'
    $source=Get-Content $manifest -Raw|ConvertFrom-Json
    if(!$source.verified -or !$source.workerProjectInverseVerified -or $source.files.Count -ne 128){throw 'Source assembly incomplete'}
    foreach($file in $source.files){Hold (Join-Path $stage $file.path) $file.sha256}
    $dotnet='C:\Program Files\dotnet\dotnet.exe';Hold $dotnet;$toolchain='C:\Program Files\dotnet\sdk\8.0.427'
    foreach($name in @('MSBuild.dll','Microsoft.Build.dll','Roslyn/bincore/csc.dll','Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll')){Hold (Join-Path $toolchain $name)}
    function Build([string]$name,[string]$target){
        $args=@(('"'+(Join-Path $toolchain 'MSBuild.dll')+'"'),('"'+(Join-Path $stage 'SDK.Build.proj')+'"'),'/nologo','/m:1','/nodeReuse:false',('/t:'+$target),'/v:minimal','/p:Configuration=Release','/p:UseSharedCompilation=false')
        $run=Invoke-OwnedTestRun -Name $name -Application $dotnet -Arguments $args -WorkingDirectory $stage -OutputDirectory $out -Seconds 120 -Records $runs
        if(!$run.passed){throw ('Aggregate target failed: '+$target)}
    }
    Build 'build-worker-il' 'BuildWorkerIl';$result.workerIlCompiled=$true
    $ilRoot=Join-Path $stage 'worker-il/Release';$nativeRoot=Join-Path $out 'ScriptWorker'
    $names=@('Lea.Script.Worker.exe','Lea.Script.Worker.dll','Lea.Script.Worker.runtimeconfig.json','Lea.Script.Worker.deps.json','Jint.dll','Acornima.dll')
    foreach($name in $names){$path=Join-Path $ilRoot $name;Hold $path;if($name -ceq 'Lea.Script.Worker.dll'){$il=$path}else{CopyVerified $path (Join-Path $nativeRoot $name)}}
    $config=Get-Content (Join-Path $nativeRoot 'Lea.Script.Worker.runtimeconfig.json') -Raw|ConvertFrom-Json
    if($config.runtimeOptions.framework.version -cne '8.0.33' -or $config.runtimeOptions.rollForward -cne 'Disable'){throw 'Exact worker runtime policy required'}
    $toolProof=Join-Path $root 'continuity-evidence/modern-worker-v1/crossgen2-package-v4/result.json';Hold $toolProof '60D063F3D3F3F0C5E9036A4416C75F7590C701BAF84E15E964A0A1D53199B3EF'
    $tool=Get-Content $toolProof -Raw|ConvertFrom-Json
    if(!$tool.passed -or $tool.version -cne '8.0.32' -or $tool.verifyExitCode -ne 0){throw 'Verified native compiler required'}
    foreach($file in $tool.files|Where-Object {$_.path -match '\\tools\\'}){if(!$file.sha256){throw 'Unpinned native compiler member'};Hold $file.path $file.sha256}
    $compilers=@($tool.files|Where-Object {$_.path -match '\\tools\\crossgen2\.exe$'})
    if($compilers.Count -ne 1){throw 'Unique native compiler required'}
    $native=Join-Path $nativeRoot 'Lea.Script.Worker.dll'
    $options=@(('"'+$il+'"'),('-o:"'+$native+'"'),'--targetos:windows','--targetarch:x64','-O')
    $references=@(Get-ChildItem 'C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.33' -File -Filter '*.dll')
    if($references.Count -lt 150 -or $references.Count -gt 256){throw 'Native reference inventory differs'}
    foreach($file in $references){Hold $file.FullName;$options+=('-r:"'+$file.FullName+'"')}
    foreach($name in @('Jint.dll','Acornima.dll')){$options+=('-r:"'+(Join-Path $nativeRoot $name)+'"')}
    $rsp=Join-Path $out 'native.rsp';[IO.File]::WriteAllLines($rsp,$options,[Text.UTF8Encoding]::new($false));Hold $rsp
    $compiled=Invoke-OwnedTestRun -Name 'compile-native-worker' -Application $compilers[0].path -Arguments @('@"'+$rsp+'"') -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compiled.passed){throw 'New aggregate native compilation failed'};Hold $native
    $reader=Join-Path $PSScriptRoot 'ReadReadyToRun.cs';Hold $reader;Add-Type -Path $reader
    foreach($name in @('Lea.Script.Worker.dll','Jint.dll','Acornima.dll')){if(![ReadReadyToRun]::IsX64((Join-Path $nativeRoot $name))){throw 'x64 ReadyToRun closure required'}}
    $result.workerNativeCompiled=$true;$result.nativeProducts=@(foreach($name in $names){$path=Join-Path $nativeRoot $name;@{path=$path;sha256=(Get-FileHash $path).Hash}})
    foreach($name in $names){CopyVerified (Join-Path $nativeRoot $name) (Join-Path $stage ('bin/Release/ScriptWorker/'+$name))}
    Build 'build-consumer-samples' 'BuildSamples'
    $runtime=Join-Path $stage 'ScriptSample/bin/Release';$members=@(Get-ChildItem $runtime -Recurse -File)
    foreach($name in $names){if((Get-FileHash (Join-Path $runtime ('ScriptWorker/'+$name))).Hash -cne (Get-FileHash (Join-Path $nativeRoot $name)).Hash){throw 'Consumer worker closure differs'}}
    if((Get-Volume -DriveLetter C).FileSystem -cne 'NTFS' -or (Get-PSDrive C).Free -lt (600MB+($members|Measure-Object Length -Sum).Sum)){throw 'Preserve full NTFS staging reserve'}
    foreach($file in $members){CopyVerified $file.FullName (Join-Path $candidate ([IO.Path]::GetRelativePath($runtime,$file.FullName)))}
    $result.workerAclBefore=@(Get-ChildItem (Join-Path $candidate 'ScriptWorker') -File|ForEach-Object {@{path=$_.FullName;sddl=(Get-Acl $_.FullName).Sddl}})
    $tested=Invoke-OwnedTestRun -Name 'test-aggregate-script-sample' -Application (Join-Path $candidate 'Lea.ScriptEmbeddingSample.exe') -Arguments @('--test',('"'+$candidate+'"')) -WorkingDirectory $candidate -OutputDirectory $out -Seconds 30 -Records $runs
    $result.passed=$tested.passed -and @(Get-Content $tested.stdout|Where-Object {$_ -ceq 'PASS LEA SDK SCRIPT SAMPLE 17'}).Count -eq 1
    $result.candidate=$candidate
}catch{$result.error=$_.Exception.ToString()}
finally{
    if($result.workerAclBefore){$result.workerAclRestored=@($result.workerAclBefore|Where-Object {(Get-Acl $_.path).Sddl -cne $_.sddl}).Count -eq 0;$result.passed=$result.passed -and $result.workerAclRestored}
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaFullNativeSdkLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object workerIlCompiled,workerNativeCompiled,passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
