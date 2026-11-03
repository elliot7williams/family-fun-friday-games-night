param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$out='I:/LeaBuildStorage/private-binding-trusted-connectivity-20261102-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Test-Path $out) -or (Get-PSDrive C).Free -lt 600MB -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh output/owner/reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image','Lea.PrivateBindings.Connectivity' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new();$runs=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$hash){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $stream=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($stream)
    if(!$hash){$hash=(Get-FileHash $path).Hash};if((Get-FileHash $path).Hash -cne $hash){throw 'Trusted connectivity input differs'};$inputs.Add(@{path=$path;sha256=$hash})
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;before=@();browserAdopted=$false;engineBinariesChanged=$false;driverFilesAdded=@()}
try{
    Pin $PSCommandPath
    $priorPath='I:/LeaBuildStorage/private-binding-shared-native-20261102-v1/result.json';Pin $priorPath '746FE5992E89D2639E4D594D9681774DD289D91568360473704C753969E2620F'
    $prior=Get-Content $priorPath -Raw|ConvertFrom-Json
    if(!$prior.passed -or !$prior.completed -or !$prior.inputsUnchanged -or $prior.ownerAttention -or $prior.changedFiles.Count -or $prior.before.Count -ne 27){throw 'Current shared runtime qualification incomplete'}
    $runtime=$prior.runtime
    if(@(Get-ChildItem $runtime -Recurse -File).Count -ne 27){throw 'Exact27file runtime required'}
    foreach($file in $prior.before){Pin $file.path $file.sha256;if((Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Trusted runtime ACL changed'}}
    $featurePath=Join-Path $root 'continuity-evidence/private-binding-feature-isolated-v1/result.json';Pin $featurePath '8244E1A2816FF045B17445FE152CB49AD3AA0646097F443B7FAD2763010B70FD'
    $feature=Get-Content $featurePath -Raw|ConvertFrom-Json
    $fixture=@($feature.inputs|Where-Object {$_.path.Replace('\','/').EndsWith('/test-pages/javascript-contains.html',[StringComparison]::Ordinal)})
    if($fixture.Count -ne 1){throw 'Original connectivity fixture required'}
    Pin $fixture[0].path $fixture[0].sha256
    $cases=Join-Path $root 'Lea.Tests/IsConnectedCases.js';Pin $cases 'AE7B3FD5C013D83D2BFD3740890FD67E13BB8E46CA2C87DFC67B4011388623F6'
    Pin (Join-Path $root 'Lea.Tests/IsConnectedMain.cs') '8AE71E18B5CDB42D43D61FFD912FA638FD018EB21DB7A85DDE6850806B9162F4'
    Pin (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    Pin (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    $result.runtime=$runtime;$result.before=$prior.before
    $exe=Join-Path $runtime 'Lea.PrivateBindings.Connectivity.exe'
    $run=Invoke-OwnedTestRun -Name 'trusted-connectivity' -Application $exe -Arguments @(('"'+$fixture[0].path+'"'),('"'+$cases+'"'),'trusted') -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
    $footer='PASS Lea isConnected trusted: nonmutating read, descriptors, brands, read-only, detached reads/write guard, revoked permission, cycle limit and recovery.'
    $result.passed=$run.passed -and @(Get-Content $run.stdout|Where-Object {$_ -ceq $footer}).Count -eq 1
    $result.scope='Original existing trusted connectivity executable/cases; no new driver or binary changes. Original assertions and30s/5s. Cycle portion asserts typed limit/recovery, not diagnostic reason/phase attribution. No full-suite or startup reliability qualification.'
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@($result.before|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256 -or (Get-Acl $_.path).Sddl -cne $_.sddl})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention -and !$result.error
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaTrustedConnectivityLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json -Compress
}
