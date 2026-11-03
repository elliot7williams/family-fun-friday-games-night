param([switch]$KeepForegroundHost,[ValidateSet(1,2,3)][int]$Version=3,[switch]$Isolated=$true)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$out=Join-Path $root 'continuity-evidence/global-install-isolated-scheduling-v1'
if($Isolated){if($Version -ne 3){throw 'Isolated connectivity requires fresh worker candidate'};$out=Join-Path $root 'continuity-evidence/global-install-isolated-scheduling-v1'}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Test-Path $out) -or (Get-PSDrive C).Free -lt 600MB -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/output/reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No test/browser overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new();$runs=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$hash){$s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);if(!$hash){$hash=(Get-FileHash $path).Hash};if((Get-FileHash $path).Hash -cne $hash){throw 'Connectivity test input differs'};$inputs.Add(@{path=$path;sha256=$hash})}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;before=@();isolatedWorkerTested=$false;browserAdopted=$false}
try{
    Pin $PSCommandPath
    $buildPath='I:\LeaBuildStorage\global-install-build-20261102-v1/result.json';Pin $buildPath 'FA61813EBDA435DFF4DC1138E450FE2C385AC3156534B507DB9216AA9878B21F'
    $b=Get-Content $buildPath -Raw|ConvertFrom-Json
    if(!$b.passed -or !$b.inputsUnchanged -or $b.ownerAttention -or $b.sha256 -cne 'F5BDB1FF4CAD596781E91D4FDAEED421E8B22C199EF97852BD96481D778E3B7B'){throw 'Connectivity build incomplete'}
    $runtime=Join-Path $out 'runtime';New-Item -ItemType Directory -Path $runtime|Out-Null
    $dlls=@(Get-ChildItem (Split-Path $b.binary) -Filter '*.dll' -File)
    if($dlls.Count -ne 8){throw 'Eight-file trusted closure required'}
    if(!$Isolated){throw 'This campaign requires isolated scheduling'}
    $workerReserveProof='I:/LeaBuildStorage/global-install-worker-build-20261102-v1/result.json';Pin $workerReserveProof '21EF3E9F03E594F562596DA7008A97A282CFE49D8CEC6C1D8DBFBDA2E73B112C'
    $workerReserve=Get-Content $workerReserveProof -Raw|ConvertFrom-Json
    if(!$workerReserve.passed -or !$workerReserve.inputsUnchanged -or $workerReserve.ownerAttention -or $workerReserve.products.Count -ne 6 -or $workerReserve.resourceSha256 -cne '088847DC78DC1B70C5852A2C8934601110F82179A889AC9ADB1D1FF210A1B8EB'){throw 'Matching worker reserve proof incomplete'}
    $stageBytes=[long](($dlls|Measure-Object Length -Sum).Sum)+[long](($workerReserve.products|Measure-Object bytes -Sum).Sum)+857600+350720+256KB
    $source=Join-Path $root 'Lea.Tests/IsolatedSchedulingRegressionMain.cs';Pin $source
    Pin (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    Pin (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    $exe=Join-Path $runtime 'Lea.IsConnected.Tests.exe'
    $config=Join-Path $root 'continuity-evidence/deterministic-full-replay-v1/runtime/Lea.Script.Tests.exe.config'
    Pin $config 'BBBBB19D16CE7AE2715A07FC065A55FAA0BB8DE5BC46E4F243D496320C9FC2EC'
    Copy-Item -LiteralPath $config -Destination ($exe+'.config')
    $options=[Collections.Generic.List[string]]::new()
    foreach($arg in @('/nologo','/noconfig','/nostdlib+','/target:exe','/langversion:12.0','/optimize+','/deterministic+',('/out:"'+$exe+'"'),('"'+$source+'"'))){$options.Add($arg)}
    foreach($name in @('mscorlib','System','System.Core','System.Drawing','System.Windows.Forms','System.Web.Extensions')){$path='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\'+$name+'.dll';Pin $path;$options.Add('/reference:"'+$path+'"')}
    foreach($name in @('Lea.Engine.dll','Lea.Script.dll')){$options.Add('/reference:"'+(Join-Path $runtime $name)+'"')}
    $rsp=Join-Path $out 'compile.rsp';[IO.File]::WriteAllLines($rsp,$options);Pin $rsp
    $dotnet='C:\Program Files\dotnet\dotnet.exe';Pin $dotnet
    $csc='C:\Program Files\dotnet\sdk\8.0.427\Roslyn\bincore\csc.dll';Pin $csc
    $compile=Invoke-OwnedTestRun -Name 'compile-connectivity-test' -Application $dotnet -Arguments @(('"'+$csc+'"'),('@"'+$rsp+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compile.passed){throw 'Connectivity harness compile failed'}
    $result.candidateOnly=$true;$result.completed=$false;$result.cases=[Collections.Generic.List[object]]::new()
    $result.before=@(Get-ChildItem $runtime -Recurse -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}})
    $trace=Join-Path $PSScriptRoot 'Read-AllStartupTraceProof.ps1';Pin $trace '6454310B2E41C38B25C9A7085687BCADF9C7D7F95B3DF7D96E7F702DC9241CDF';. $trace
    foreach($name in @('Timers','Frames')){
        $run=Invoke-OwnedTestRun -Name ('isolated-'+$name) -Application $exe -Arguments @(('"'+$testLibrary+'"'),$name,('"'+$fixture+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
        $pattern='^PASS Lea isolated scheduling '+$name+': ([0-9]+) checks\.$'
        $footers=@(Get-Content $run.stdout|Where-Object {$_ -cmatch $pattern})
        $count=if($footers.Count -eq 1 -and $footers[0] -cmatch $pattern){[int]$Matches[1]}else{0}
        $proof=Read-AllStartupTraceProof -Lines @(Get-Content $run.stderr) -StreamsComplete ([bool]($run.stdoutComplete -and $run.stderrComplete)) -ExitObserved ([bool]$run.exitObserved) -TimedOut ([bool]$run.timedOut) -KillRequested ([bool]$run.killRequested)
        $ok=$run.passed -and $count -gt 0 -and $proof.traceActivationVerified -and $proof.traceEnvironmentRestored
        $result.cases.Add(@{name=$name;passed=$ok;checks=$count;traceActivationVerified=$proof.traceActivationVerified;traceEnvironmentRestored=$proof.traceEnvironmentRestored})
        if(!$ok){throw ('Original trusted scheduling suite failed: '+$name)}
        foreach($file in $result.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Scheduling runtime drift prevents continuation'}}
        if($global:LeaCompositeProcessLeases.Count -or (Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)){throw 'Scheduling retirement incomplete'}
    }
    $result.isolatedWorkerTested=$true;$result.completed=$result.cases.Count -eq 2;$result.passed=$result.completed
    $result.scope='Original compiled isolated Timers.Suite and Frames.Run(isolated), unchanged assertions and deadlines, matching worker. Live timer guard observations, owned jobs, trace activation/restoration and bytes/ACL checks. Excludes scheduler/protocol/native/host/full-suite qualification; no retries.'
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@($result.before|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256 -or (Get-Acl $_.path).Sddl -cne $_.sddl})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention -and !$result.error
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaConnectivityLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json -Compress
}
