param([switch]$KeepForegroundHost,[switch]$Portable)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\full-sdk-worker-gate-20261101-v1'
$candidate=Join-Path $root 'continuity-evidence/full-sdk-script-sample-v1'
if($Portable){$out='I:\LeaBuildStorage\portable-sdk-worker-gate-20261101-v1';$candidate=Join-Path $root 'continuity-evidence/portable-full-sdk-samples-v1/script'}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained foreground owner required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh output/owner and original reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.ScriptEmbeddingSample','SdkScriptLocal' -ErrorAction SilentlyContinue).Count){throw 'No test/browser overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$pin){
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($pin -and $hash -cne $pin){throw 'Aggregate worker gate input differs'};$inputs.Add(@{path=$path;sha256=$hash})
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;completeSdk=$false;hostAdopted=$false;inputs=$inputs;runs=$runs}
try{
    Hold $PSCommandPath
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $proof='I:\LeaBuildStorage\full-sdk-native-build-20261101-v1/result.json';$proofHash='20E2EE1F8A87BFC2B14CD581E48B59D1DE834E87EBA729DCB5A45CCF4E1A7893'
    if($Portable){$proof='I:\LeaBuildStorage\portable-full-sdk-build-20261101-v1/result.json';$proofHash='17AAF573A110588C3D9A0C43944465B0504B44865CBBAFDFBE6C9D319C29C2C7'}
    Hold $proof $proofHash
    $native=Get-Content $proof -Raw|ConvertFrom-Json
    $expectedCandidate=if($Portable){Join-Path $native.candidate 'script'}else{$native.candidate}
    if(!$native.passed -or !$native.inputsUnchanged -or !$native.workerAclRestored -or $native.ownerAttention -or $expectedCandidate -cne $candidate){throw 'Passing retired aggregate sample required'}
    foreach($file in $native.inputs|Where-Object {$_.path.StartsWith($candidate+'\',[StringComparison]::OrdinalIgnoreCase)}){Hold $file.path $file.sha256}
    $runtime=Join-Path $candidate 'ScriptWorker'
    $result.workerAclBefore=@(foreach($file in $native.workerAclBefore){if((Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Previous worker ACL not restored'};@{path=$file.path;sddl=$file.sddl}})
    if($result.workerAclBefore.Count -ne 6){throw 'Exact six file ACL closure required'}
    $fixtureSource=Join-Path $root 'LeaForge/test-pages/sdk-script-local.html';Hold $fixtureSource
    $fixture=Join-Path $candidate 'sdk-worker-gate.html'
    $exe=Join-Path $candidate 'FullSdkWorkerGate.exe'
    if((Test-Path $exe) -or (Test-Path $fixture) -or (Test-Path ($exe+'.config'))){throw 'Fresh probe files required'}
    Copy-Item -LiteralPath $fixtureSource -Destination $fixture;Hold $fixture (Get-FileHash $fixtureSource).Hash
    $config=Join-Path $root 'Lea.Sdk/ScriptHost.config';Hold $config;Copy-Item -LiteralPath $config -Destination ($exe+'.config');Hold ($exe+'.config') (Get-FileHash $config).Hash
    $source=Join-Path $root 'Lea.Tests/SdkScriptWorkerMain.cs';Hold $source
    $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe';Hold $compiler
    $args=@('/nologo','/noconfig','/nostdlib+','/target:exe','/platform:x64','/warnaserror+',('/out:"'+$exe+'"'))
    $refs='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    foreach($name in @('mscorlib.dll','System.dll','System.Core.dll','System.Drawing.dll','System.Windows.Forms.dll')){$path=Join-Path $refs $name;Hold $path;$args+=('/reference:"'+$path+'"')}
    foreach($name in @('Lea.Engine.dll','Lea.Win.dll','Lea.Script.dll','Lea.Script.Win.dll','Jint.dll','Acornima.dll')){$args+=('/reference:"'+(Join-Path $candidate $name)+'"')}
    $args+=('"'+$source+'"')
    $compiled=Invoke-OwnedTestRun -Name 'compile-worker-gate' -Application $compiler -Arguments $args -WorkingDirectory $candidate -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compiled.passed){throw 'Aggregate worker gate compile failed'};Hold $exe
    $tested=Invoke-OwnedTestRun -Name 'test-worker-gate' -Application $exe -Arguments @(('"'+$fixture+'"')) -WorkingDirectory $candidate -OutputDirectory $out -Seconds 30 -Records $runs
    $result.passed=$tested.passed -and @(Get-Content $tested.stdout|Where-Object {$_ -ceq 'PASS SDK WORKER 25'}).Count -eq 1
}catch{$result.error=$_.Exception.ToString()}
finally{
    if($result.workerAclBefore){$result.workerAclRestored=@($result.workerAclBefore|Where-Object {(Get-Acl $_.path).Sddl -cne $_.sddl}).Count -eq 0;$result.passed=$result.passed -and $result.workerAclRestored}
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaFullWorkerGateLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,workerAclRestored,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
