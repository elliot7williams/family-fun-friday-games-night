param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$out='I:/LeaBuildStorage/global-install-shared-native-20261102-v1'
$runtime=Join-Path $root 'continuity-evidence/global-install-isolated-scheduling-v2/runtime'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Test-Path $out) -or (Get-PSDrive C).Free -lt (600MB+256KB) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/output and reserve plus driver space required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image','Lea.GlobalNative.Tests' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new();$runs=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$hash){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
    if(!$hash){$hash=(Get-FileHash $path).Hash};if((Get-FileHash $path).Hash -cne $hash){throw 'Shared DOM input differs'};$inputs.Add(@{path=$path;sha256=$hash})
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;completed=$false;inputs=$inputs;runs=$runs;before=@();cases=[Collections.Generic.List[object]]::new();runtime=$runtime;runtimeReused=$true;engineBinariesChanged=$false;driverFilesAdded=@();browserAdopted=$false}
try{
    Pin $PSCommandPath
    $proof='I:/LeaBuildStorage/global-install-shared-dom-20261102-v1/result.json';Pin $proof 'DC438755F25368344B992F131C27DFE892DF74FB72EE2C39EE1AAC4E08C41106'
    $prior=Get-Content $proof -Raw|ConvertFrom-Json
    if($prior.passed -or $prior.completed -or !$prior.inputsUnchanged -or $prior.ownerAttention -or $prior.changedFiles.Count -or $prior.before.Count -ne 21 -or @(Get-ChildItem $runtime -Recurse -File).Count -ne 21 -or $prior.runs.Count -ne 5 -or $prior.cases.Count -ne 4 -or $prior.cases[3].passed -or $prior.error -cnotlike '*Original DOM suite failed: ClassLists*'){throw 'Expected clean partial DOM boundary differs'}
    foreach($run in $prior.runs){if(!$run.exitObserved -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.unsafeToContinue -or $run.retainedForOwner -or $run.timedOut -or $run.killRequested){throw 'Prior DOM ownership incomplete'}}
    if(!(Get-Content $prior.runs[4].stderr -Raw).Contains('Script budget exceeded. phase=bootstrap; reason=TimeoutException; stage=events')){throw 'Expected typed bootstrap failure differs'}
    foreach($file in $prior.before){
        if(!$file.path.StartsWith($runtime+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase) -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Shared runtime origin/ACL differs'}
        Pin $file.path $file.sha256
    }
    if((Get-FileHash (Join-Path $runtime 'Lea.Script.dll')).Hash -cne 'F5BDB1FF4CAD596781E91D4FDAEED421E8B22C199EF97852BD96481D778E3B7B' -or (Get-FileHash (Join-Path $runtime 'ScriptWorker/Lea.Script.Worker.dll')).Hash -cne 'A72EF9C070FCDE2BA57630FFFF6BEAD4ABD157F6365FA092C0A721AB3EACAFF4'){throw 'Matched shared engine/worker differs'}
    $testLibrary=Join-Path $runtime 'Lea.Script.Tests.exe';Pin $testLibrary '6E46584EA1539D331D8FC17D73F8AC59081BF6BF98E31A7B89F0BF6375890D48'
    $fixture=Join-Path $root 'LeaForge/test-pages/is-connected-native.html';Pin $fixture
    $source=Join-Path $root 'Lea.Tests/IsConnectedNativeMain.cs';Pin $source 'BBBF3D3842E9D827398BCF107E4F0DF6552FD90606FECE5BEA7A18C5B5263EC1'
    Pin (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    Pin (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    $exe=Join-Path $runtime 'Lea.GlobalNative.Tests.exe';$config=$exe+'.config'
    if((Test-Path $exe) -or (Test-Path $config)){throw 'Fresh driver names required'}
    $originalConfig=Join-Path $runtime 'Lea.IsConnected.Tests.exe.config';Pin $originalConfig 'BBBBB19D16CE7AE2715A07FC065A55FAA0BB8DE5BC46E4F243D496320C9FC2EC'
    Copy-Item -LiteralPath $originalConfig -Destination $config;$result.driverFilesAdded=@($exe,$config)
    $options=[Collections.Generic.List[string]]::new()
    foreach($arg in @('/nologo','/noconfig','/nostdlib+','/target:exe','/langversion:12.0','/optimize+','/deterministic+',('/out:"'+$exe+'"'),('"'+$source+'"'))){$options.Add($arg)}
    foreach($name in @('mscorlib','System','System.Core','System.Drawing','System.Windows.Forms','System.Web.Extensions')){$path='C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/'+$name+'.dll';Pin $path;$options.Add('/reference:"'+$path+'"')}
    foreach($name in @('Lea.Engine.dll','Lea.Script.dll','Lea.Win.dll','Lea.Script.Win.dll')){$options.Add('/reference:"'+(Join-Path $runtime $name)+'"')}
    $rsp=Join-Path $out 'compile.rsp';[IO.File]::WriteAllLines($rsp,$options);Pin $rsp
    $dotnet='C:/Program Files/dotnet/dotnet.exe';Pin $dotnet
    $csc='C:/Program Files/dotnet/sdk/8.0.427/Roslyn/bincore/csc.dll';Pin $csc
    $compile=Invoke-OwnedTestRun -Name 'compile-native-driver' -Application $dotnet -Arguments @(('"'+$csc+'"'),('@"'+$rsp+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compile.passed){throw 'Shared DOM driver compile failed'}
    $result.before=@(Get-ChildItem $runtime -Recurse -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}})
    if($result.before.Count -ne 23){throw 'Only two new driver files allowed'}
    $result.nativeOnly=$true
    $run=Invoke-OwnedTestRun -Name 'native-connectivity' -Application $exe -Arguments @(('"'+$fixture+'"'),('"'+$out+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
    $footer='PASS Lea isConnected native: inline lifecycle, real activation, field retention, detached editor cleanup, desktop/narrow render, guarded worker retirement.'
    $verifiedFooter=@(Get-Content $run.stdout|Where-Object {$_ -ceq $footer}).Count -eq 1
    $result.images=@(foreach($name in @('connected-desktop.png','detached-desktop.png','detached-narrow.png')){$path=Join-Path $out $name;if(Test-Path $path){@{path=$path;sha256=(Get-FileHash $path).Hash}}})
    $result.passed=$run.passed -and $verifiedFooter -and $result.images.Count -eq 3
    $result.completed=$result.passed
    $result.scope='Independent unchanged native V connectivity fixture: inline lifecycle, real activation/edit retention/detachment, desktop+narrow paint, sandbox guards and exact worker retirement. Same engine/worker bytes; two new driver files only. Not a ClassLists retry, Firespider host/full-suite or startup-reliability qualification.'
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@($result.before|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256 -or (Get-Acl $_.path).Sddl -cne $_.sddl})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention -and !$result.error
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaSharedDomInputLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,completed,inputsUnchanged,ownerAttention,error|ConvertTo-Json -Compress
}
