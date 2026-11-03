param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$out='I:/LeaBuildStorage/private-binding-shared-scheduling-20261102-v1'
$runtime=Join-Path $root 'continuity-evidence/private-binding-trusted-v1/runtime'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Test-Path $out) -or (Get-PSDrive C).Free -lt (600MB+256KB) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/output and reserve plus driver space required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image','Lea.PrivateScheduling.Tests' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new();$runs=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$hash){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
    if(!$hash){$hash=(Get-FileHash $path).Hash};if((Get-FileHash $path).Hash -cne $hash){throw 'Scheduling input differs'};$inputs.Add(@{path=$path;sha256=$hash})
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;completed=$false;inputs=$inputs;runs=$runs;before=@();cases=[Collections.Generic.List[object]]::new();runtime=$runtime;runtimeReused=$true;engineBinariesChanged=$false;driverFilesAdded=@();browserAdopted=$false}
try{
    Pin $PSCommandPath
    $proof='I:/LeaBuildStorage/private-binding-shared-dom-20261102-v1/result.json';Pin $proof '246C2EDB4717DAE02F6CEA6E2A7D1975B6B3A08DF23ECB569B9922B1E832B836'
    $prior=Get-Content $proof -Raw|ConvertFrom-Json
    if(!$prior.passed -or !$prior.completed -or !$prior.inputsUnchanged -or $prior.ownerAttention -or $prior.changedFiles.Count -or $prior.before.Count -ne 23 -or @(Get-ChildItem $runtime -Recurse -File).Count -ne 23){throw 'Qualified scheduling runtime boundary differs'}
    foreach($file in $prior.before){
        if(!$file.path.StartsWith($runtime+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase) -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Shared runtime origin/ACL differs'}
        Pin $file.path $file.sha256
    }
    if((Get-FileHash (Join-Path $runtime 'Lea.Script.dll')).Hash -cne '406C5634638D75C66892BD95429760CB4A5C74C94277913D1DF467147E30E75F' -or (Get-FileHash (Join-Path $runtime 'ScriptWorker/Lea.Script.Worker.dll')).Hash -cne 'F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0'){throw 'Matched scheduling pair differs'}
    $library=Join-Path $runtime 'Lea.Script.Tests.exe';Pin $library '6E46584EA1539D331D8FC17D73F8AC59081BF6BF98E31A7B89F0BF6375890D48'
    $manifestPath=Join-Path $root 'continuity-evidence/sdk-full-script-suite-v8/staging.json';Pin $manifestPath '90FD0D60BA1CCE04C0923D6E18E7FB5D218F4F04C129DD49F97477AFFD2384CA'
    $manifest=Get-Content $manifestPath -Raw|ConvertFrom-Json
    $fixture=@($manifest.files|Where-Object path -CEQ 'fixtures/LeaForge/test-pages/javascript-view.html')
    if($fixture.Count -ne 1){throw 'Original scheduling fixture required'}
    Pin $fixture[0].origin $fixture[0].sha256
    $source=Join-Path $root 'Lea.Tests/PrivateBindingSchedulingMain.cs';Pin $source
    $trustedSource=Join-Path $root 'Lea.Tests/SchedulingRegressionMain.cs';Pin $trustedSource 'B6F9B190C6CE045E0592B13B39990B53068805E5F67A9CF3811A07F573F404CB'
    $isolatedSource=Join-Path $root 'Lea.Tests/IsolatedSchedulingRegressionMain.cs';Pin $isolatedSource '59A0525B2418F211085AF431B3ACE903DD06DB063177F491791FA729B971924B'
    Pin (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    Pin (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    $exe=Join-Path $runtime 'Lea.PrivateScheduling.Tests.exe';$config=$exe+'.config'
    if((Test-Path $exe) -or (Test-Path $config)){throw 'Fresh scheduling driver names required'}
    $originalConfig=Join-Path $runtime 'Lea.IsConnected.Tests.exe.config';Pin $originalConfig 'BBBBB19D16CE7AE2715A07FC065A55FAA0BB8DE5BC46E4F243D496320C9FC2EC'
    Copy-Item -LiteralPath $originalConfig -Destination $config;$result.driverFilesAdded=@($exe,$config)
    $options=[Collections.Generic.List[string]]::new()
    foreach($arg in @('/nologo','/noconfig','/nostdlib+','/target:exe','/main:PrivateBindingSchedulingMain','/langversion:12.0','/optimize+','/deterministic+',('/out:"'+$exe+'"'))){$options.Add($arg)}
    foreach($file in @($source,$trustedSource,$isolatedSource)){$options.Add('"'+$file+'"')}
    foreach($name in @('mscorlib','System','System.Core','System.Drawing','System.Windows.Forms','System.Web.Extensions')){$path='C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/'+$name+'.dll';Pin $path;$options.Add('/reference:"'+$path+'"')}
    foreach($name in @('Lea.Engine.dll','Lea.Script.dll')){$options.Add('/reference:"'+(Join-Path $runtime $name)+'"')}
    $rsp=Join-Path $out 'compile.rsp';[IO.File]::WriteAllLines($rsp,$options);Pin $rsp
    $dotnet='C:/Program Files/dotnet/dotnet.exe';Pin $dotnet
    $csc='C:/Program Files/dotnet/sdk/8.0.427/Roslyn/bincore/csc.dll';Pin $csc
    $compile=Invoke-OwnedTestRun -Name 'compile-scheduling-driver' -Application $dotnet -Arguments @(('"'+$csc+'"'),('@"'+$rsp+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compile.passed){throw 'Scheduling driver compile failed'}
    $result.before=@(Get-ChildItem $runtime -Recurse -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}})
    if($result.before.Count -ne 25){throw 'Only two scheduling driver additions allowed'}
    $trace=Join-Path $PSScriptRoot 'Read-AllStartupTraceProof.ps1';Pin $trace '6454310B2E41C38B25C9A7085687BCADF9C7D7F95B3DF7D96E7F702DC9241CDF';. $trace
    foreach($case in @(@{mode='trusted';name='Timers';checks=59},@{mode='trusted';name='Frames';checks=26},@{mode='isolated';name='Timers';checks=59},@{mode='isolated';name='Frames';checks=27})){
        $run=Invoke-OwnedTestRun -Name ($case.mode+'-'+$case.name) -Application $exe -Arguments @(('"'+$library+'"'),$case.name,('"'+$fixture[0].origin+'"'),$case.mode) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
        $prefix=if($case.mode -ceq 'trusted'){'PASS Lea scheduling '}else{'PASS Lea isolated scheduling '}
        $pattern='^'+$prefix+$case.name+': ([0-9]+) checks\.$'
        $footers=@(Get-Content $run.stdout|Where-Object {$_ -cmatch $pattern})
        $count=if($footers.Count -eq 1 -and $footers[0] -cmatch $pattern){[int]$Matches[1]}else{0}
        $proof=Read-AllStartupTraceProof -Lines @(Get-Content $run.stderr) -StreamsComplete ([bool]($run.stdoutComplete -and $run.stderrComplete)) -ExitObserved ([bool]$run.exitObserved) -TimedOut ([bool]$run.timedOut) -KillRequested ([bool]$run.killRequested)
        $ok=$run.passed -and $count -eq $case.checks -and $proof.traceActivationVerified -and $proof.traceEnvironmentRestored
        $result.cases.Add(@{mode=$case.mode;name=$case.name;passed=$ok;checks=$count;traceActivationVerified=$proof.traceActivationVerified;traceEnvironmentRestored=$proof.traceEnvironmentRestored})
        if(!$ok){throw ('Original scheduling suite failed: '+$case.mode+'-'+$case.name)}
        foreach($file in $result.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Scheduling runtime drift prevents continuation'}}
    }
    $result.completed=$result.cases.Count -eq 4;$result.passed=$result.completed
    $result.scope='Original trusted and isolated Timers.Suite/Frames.Run assertions, fresh process per case, original120s/30s/5s. Trace gates and shared pair bytes/ACLs retained. No scheduler/protocol/native/host/full-suite or reliability qualification.'
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@($result.before|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256 -or (Get-Acl $_.path).Sddl -cne $_.sddl})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention -and !$result.error
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaSharedSchedulingLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,completed,inputsUnchanged,ownerAttention,error|ConvertTo-Json -Compress
}
