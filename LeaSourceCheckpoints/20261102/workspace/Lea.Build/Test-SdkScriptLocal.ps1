param([switch]$KeepForegroundHost,[switch]$HostConfig,[switch]$TailTrace,[switch]$Timeline,[switch]$TimelineExport,[switch]$Worker,[ValidateRange(1,9)][int]$Version=1,[switch]$Adapter,[switch]$SessionIoReuse,[switch]$SessionIoLifecycle,[switch]$SessionIoBenchmark,[switch]$PooledIo,[switch]$PooledIoLifecycle,[switch]$PooledIoNative,[switch]$ParentIoContract)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'continuity-evidence/sdk-script-local-v1'
if($HostConfig){$out=Join-Path $root 'continuity-evidence/sdk-script-local-configured-v1'}
if($TailTrace){if(!$HostConfig){throw 'Tail trace requires configured host'};$out=Join-Path $root 'continuity-evidence/sdk-script-local-tail-trace-v1'}
if($Timeline){if(!$TailTrace){throw 'Timeline requires tail trace host'};$out=Join-Path $root 'continuity-evidence/sdk-script-local-timeline-v1'}
if($TimelineExport){if(!$Timeline){throw 'Timeline export requires timeline host'};$out=Join-Path $root 'continuity-evidence/sdk-script-local-timeline-export-v1'}
if($Version -ne 1 -and !$Worker){throw 'Versioned attempts require worker mode'}
if($Worker){if(!$HostConfig -or $TailTrace -or $Timeline -or $TimelineExport){throw 'Worker tests require configured uninstrumented SDK parent'};$out=Join-Path $root ('continuity-evidence/sdk-script-worker-runtime-v'+$Version)}
if($Adapter){if(!$Worker){throw 'Adapter requires full worker candidate'};$out=Join-Path $root ('continuity-evidence/sdk-script-adapter-runtime-v'+$Version)}
if($SessionIoReuse){if(!$Worker){throw 'Session I/O requires separate worker candidate'};$out=Join-Path $root $(if($Adapter){'continuity-evidence/sdk-script-session-io-adapter-v'+$Version}else{'continuity-evidence/sdk-script-session-io-runtime-v'+$Version})}
if($SessionIoLifecycle){if(!$SessionIoReuse -or $Adapter){throw 'Queue lifetime checks require separate session I/O worker candidate'};$out=Join-Path $root ('continuity-evidence/sdk-script-session-io-lifecycle-v'+$Version)}
if($SessionIoBenchmark){if(!$Worker -or $Adapter -or $SessionIoLifecycle){throw 'Benchmark requires separate worker mode'};$label=if($SessionIoReuse){'candidate'}else{'baseline'};$out=Join-Path $root ('continuity-evidence/sdk-script-io-benchmark-'+$label+'-v'+$Version)}
if($PooledIo){if(!$Worker -or $SessionIoReuse -or $SessionIoLifecycle){throw 'Pooled I/O requires separate worker or adapter mode'};$out=Join-Path $root $(if($SessionIoBenchmark){'continuity-evidence/sdk-script-pooled-io-benchmark-v'+$Version}elseif($Adapter){'continuity-evidence/sdk-script-pooled-io-adapter-v'+$Version}else{'continuity-evidence/sdk-script-pooled-io-runtime-v'+$Version})}
if($PooledIoLifecycle){if(!$PooledIo -or $Adapter -or $SessionIoBenchmark){throw 'Pooled queue checks require separate pooled worker candidate'};$out=Join-Path $root ('continuity-evidence/sdk-script-pooled-io-lifecycle-v'+$Version)}
if($PooledIoNative){if(!$PooledIo -or $Adapter -or $PooledIoLifecycle -or $SessionIoBenchmark){throw 'Native pooled checks require separate pooled worker candidate'};$out=Join-Path $root ('continuity-evidence/sdk-script-pooled-io-native-v'+$Version)}
if($ParentIoContract){if(!$Worker -or $Adapter -or $SessionIoReuse -or $SessionIoLifecycle -or $SessionIoBenchmark -or $PooledIoLifecycle -or $PooledIoNative){throw 'Parent I/O contract requires separate uninstrumented candidate'};$label=if($PooledIo){'pooled'}else{'baseline'};$out=Join-Path $root ('continuity-evidence/sdk-parent-io-contract-'+$label+'-v'+$Version)}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin owner required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/evidence and reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No test/browser overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$pin){$s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));if($pin -and $pin -cne $hash){throw 'SDK script input differs'};$inputs.Add(@{path=$path;sha256=$hash})}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;workerQualified=$false;remoteScriptingEnabled=$false;completeSdk=$false}
try{
    Hold $PSCommandPath
    Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $proof='I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-script-libraries-net48\result.json';$proofPin='E3B542B8CAD81E81321E30EBCDF6682F2AC0E371A3DCF6BE0A5B299498793AD7'
    if($TailTrace){$proof='I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-script-libraries-net48-tail-trace\result.json';$proofPin='7B34A4351373D92BF72251EFE877A8DA43C80988B4A58DA74D18B4FB996E11D5';$result.diagnosticOnly=$true}
    if($Timeline){$proof='I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-script-libraries-net48-tail-trace-timeline\result.json';$proofPin='BC249B5D1881D2D1598F484A92CD69B93157EE9663AD49761B3EA4F47CE9F20B'}
    if($TimelineExport){$proof='I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-script-libraries-net48-tail-trace-timeline-export\result.json';$proofPin='1C7A26F184B810B2EEA7CBBC3419E152CB0EFEC41709D614B34CD89350645D1F'}
    if($SessionIoReuse){$proof='I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-script-libraries-net48-session-io\result.json';$proofPin='FA92DF2B826A0918CAA47818D918C6A20B6A417BAB1255DC0E930EF832512BD3';$result.sessionIoReusePrototype=$true}
    if($PooledIo){$proof='I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-script-libraries-net48-pooled-io\result.json';$proofPin='8730DBD801165B3E815A54777E35F4628D143AC608C0A66F160CBE7CFCC64DB0';$result.pooledIoPrototype=$true}
    Hold $proof $proofPin
    $build=Get-Content $proof -Raw|ConvertFrom-Json
    if(!$build.passed -or !$build.scriptLibrariesCompiled -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.outputs.Count -ne 2){throw 'Script source build incomplete'}
    $stage='I:\LeaBuildStorage\sdk-script-20261101-net48-v1'
    if($TailTrace){$stage='I:\LeaBuildStorage\sdk-script-20261101-tail-trace-v1'}
    if($Timeline){$stage='I:\LeaBuildStorage\sdk-script-20261101-timeline-v1'}
    if($TimelineExport){$stage='I:\LeaBuildStorage\sdk-script-20261101-timeline-export-v1'}
    if($SessionIoReuse){$stage='I:\LeaBuildStorage\sdk-script-20261101-session-io-v1'}
    if($PooledIo){$stage='I:\LeaBuildStorage\sdk-script-20261101-pooled-io-v2'}
    $manifest=Join-Path $stage 'source-manifest.json'
    $pins=@($build.inputs|Where-Object path -ceq $manifest|Select-Object -ExpandProperty sha256 -Unique)
    if($pins.Count -ne 1){throw 'Script manifest provenance missing'};Hold $manifest $pins[0]
    $files=@((Get-Content $manifest -Raw|ConvertFrom-Json).files|Where-Object {$_.path -match '\.dll$'})
    foreach($file in $files){$from=Join-Path $stage $file.path;Hold $from $file.sha256;$to=Join-Path $out ([IO.Path]::GetFileName($file.path));Copy-Item $from $to;Hold $to $file.sha256}
    foreach($file in $build.outputs){Hold $file.path $file.sha256;$to=Join-Path $out ([IO.Path]::GetFileName($file.path));Copy-Item $file.path $to;Hold $to $file.sha256}
    if($ParentIoContract){$result.parentIoContractOnly=$true;$result.nativeWorkerLaunched=$false}
    if($Worker -and !$ParentIoContract){
        $workerProof='I:\LeaBuildStorage\sdk-script-worker-native-20261101-v1\result.json';Hold $workerProof '81690529F6933FA641969386747B6C9549BBD14D91F29B5CF1D70BF908CE6F97'
        $native=Get-Content $workerProof -Raw|ConvertFrom-Json
        if(!$native.passed -or !$native.inputsUnchanged -or $native.ownerAttention -or $native.outputs.Count -ne 6){throw 'Native worker build incomplete'}
        $runtime=Join-Path $out 'ScriptWorker';New-Item -ItemType Directory -Path $runtime|Out-Null
        $result.workerAclBefore=@(foreach($file in $native.outputs){Hold $file.path $file.sha256;$to=Join-Path $runtime ([IO.Path]::GetFileName($file.path));Copy-Item $file.path $to;Hold $to $file.sha256;@{path=$to;sddl=(Get-Acl $to).Sddl}})
    }
    $source=Join-Path $root $(if($ParentIoContract){'Lea.Tests/SdkParentIoContractMain.cs'}elseif($PooledIoNative){'Lea.Tests/SdkPooledNativeMain.cs'}elseif($PooledIoLifecycle){'Lea.Tests/SdkPooledIoMain.cs'}elseif($SessionIoBenchmark){'Lea.Tests/SdkSessionIoBenchmarkMain.cs'}elseif($SessionIoLifecycle){'Lea.Tests/SdkSessionIoMain.cs'}elseif($Adapter){'Lea.Tests/SdkScriptAdapterMain.cs'}elseif($Worker){'Lea.Tests/SdkScriptWorkerMain.cs'}else{'Lea.Tests/SdkScriptLocalMain.cs'});Hold $source
    Copy-Item $source (Join-Path $out ([IO.Path]::GetFileName($source)))
    $fixtureSource=Join-Path $root 'LeaForge/test-pages/sdk-script-local.html';Hold $fixtureSource
    $fixture=Join-Path $out 'sdk-script-local.html';Copy-Item $fixtureSource $fixture;Hold $fixture (Get-FileHash $fixtureSource).Hash
    if($Adapter){$fixtureSource=Join-Path $root 'LeaForge/test-pages/sdk-script-adapter.html';Hold $fixtureSource;$fixture=Join-Path $out 'sdk-script-adapter.html';Copy-Item $fixtureSource $fixture;Hold $fixture (Get-FileHash $fixtureSource).Hash}
    $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe';Hold $compiler
    $exe=Join-Path $out 'SdkScriptLocal.exe'
    if($HostConfig){$config=Join-Path $root 'Lea.Sdk/ScriptHost.config';Hold $config;Copy-Item $config ($exe+'.config');Hold ($exe+'.config') (Get-FileHash $config).Hash;$result.hostBindingConfiguration=$true}
    $arguments=@('/nologo','/noconfig','/nostdlib+','/target:exe','/platform:x64','/warnaserror+',('/out:"'+$exe+'"'))
    $refs='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    foreach($name in @('mscorlib.dll','System.dll','System.Core.dll','System.Drawing.dll','System.Windows.Forms.dll')){$path=Join-Path $refs $name;Hold $path;$arguments+=('/reference:"'+$path+'"')}
    foreach($name in @('Lea.Engine.dll','Lea.Win.dll','Lea.Script.dll','Lea.Script.Win.dll','Jint.dll','Acornima.dll')){$arguments+=('/reference:"'+(Join-Path $out $name)+'"')}
    $arguments+=('"'+$source+'"')
    $compiled=Invoke-OwnedTestRun -Name 'compile-local-script' -Application $compiler -Arguments $arguments -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compiled.passed){throw 'SDK script test compilation failed'};Hold $exe
    $testArguments=@('"'+$fixture+'"')
    if($SessionIoBenchmark){$testArguments+=@($(if($SessionIoReuse){'retain'}else{'retire'}));$result.benchmarkOnly=$true;$result.warmupCalls=0;$result.measuredRequests=32}
    $tested=Invoke-OwnedTestRun -Name 'sdk-local-script' -Application $exe -Arguments $testArguments -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
    $footer=if($ParentIoContract){'PASS SDK PARENT IO CONTRACT 3'}elseif($PooledIoNative){'PASS SDK POOLED NATIVE 25'}elseif($PooledIoLifecycle){'PASS SDK POOLED IO 20'}elseif($SessionIoBenchmark){'PASS SDK IO BENCHMARK 37'}elseif($SessionIoLifecycle){'PASS SDK SESSION IO 38'}elseif($Adapter){'PASS SDK ADAPTER 26'}elseif($Worker){'PASS SDK WORKER 25'}else{'PASS SDK LOCAL SCRIPT 21'}
    $result.passed=$tested.passed -and @(Get-Content $tested.stdout|Where-Object {$_ -ceq $footer}).Count -eq 1
}catch{$result.error=$_.Exception.ToString()}
finally{
    if($Worker -and $result.workerAclBefore){$result.workerAclRestored=@($result.workerAclBefore|Where-Object {(Get-Acl $_.path).Sddl -cne $_.sddl}).Count -eq 0;$result.passed=$result.passed -and $result.workerAclRestored}
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaLocalScriptLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
