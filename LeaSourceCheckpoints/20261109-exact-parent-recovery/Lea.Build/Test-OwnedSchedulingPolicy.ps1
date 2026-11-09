param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\owned-scheduling-policy-20261109-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Get-PSDrive I).Free -lt 16MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/output and original reserves required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$expected){
    $stream=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($stream)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($expected -and $hash -cne $expected){throw 'Scheduling control input differs'}
    $inputs.Add(@{path=$path;sha256=$hash})
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;diagnosticOnly=$true;interpreterExecuted=$false;settingsChanged=$false;nativeIsolationQualified=$false;inputs=$inputs;runs=$runs}
try{
    Pin $PSCommandPath
    $owner=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Pin $owner 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owner
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Pin $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $options=[Collections.Generic.List[string]]::new()
    $exe=Join-Path $out 'Lea.ProcessPolicy.Tests.exe'
    foreach($argument in @('/nologo','/noconfig','/nostdlib+','/target:exe','/optimize+',('/out:"'+$exe+'"'))){$options.Add($argument)}
    foreach($name in @('Lea.Build/CurrentSchedulingPolicy.cs','Lea.Tests/SchedulingPolicyMain.cs','Lea.Build/DiagnosticScriptAssemblyInfo.cs')){
        $path=Join-Path $root $name;Pin $path;$options.Add('"'+$path+'"')
    }
    foreach($name in @('mscorlib','System','System.Core','System.Web.Extensions')){
        $path='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\'+$name+'.dll';Pin $path;$options.Add('/reference:"'+$path+'"')
    }
    $rsp=Join-Path $out 'control.rsp';[IO.File]::WriteAllLines($rsp,$options);Pin $rsp
    $dotnet='C:\Program Files\dotnet\dotnet.exe';Pin $dotnet
    $toolchain='C:\Program Files\dotnet\sdk\8.0.427\Roslyn\bincore'
    foreach($name in @('csc.dll','csc.deps.json','csc.runtimeconfig.json','Microsoft.CodeAnalysis.dll','Microsoft.CodeAnalysis.CSharp.dll')){Pin (Join-Path $toolchain $name)}
    $compile=Invoke-OwnedTestRun -Name 'compile-scheduling-control' -Application $dotnet -Arguments @(('"'+(Join-Path $toolchain 'csc.dll')+'"'),('@"'+$rsp+'"')) -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compile.passed){throw 'Scheduling control compile failed'}
    Pin $exe;$result.binarySha256=(Get-FileHash $exe).Hash
    $run=Invoke-OwnedTestRun -Name 'owned-scheduling-control' -Application $exe -Arguments @() -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$run.passed){throw 'Scheduling control failed'}
    $lines=@(Get-Content -LiteralPath $run.stdout)
    if($lines.Count -ne 1 -or !$lines[0].StartsWith('LEA_SCHEDULING_POLICY ')){throw 'One scheduling report required'}
    $report=$lines[0].Substring('LEA_SCHEDULING_POLICY '.Length)|ConvertFrom-Json
    $birth=[datetime]::Parse($run.birthUtc,[Globalization.CultureInfo]::InvariantCulture,[Globalization.DateTimeStyles]::RoundtripKind).ToFileTimeUtc()
    if(!$report.selfIdentityStable -or $report.settingsChanged -or $report.before.Pid -ne $run.pid -or $report.after.Pid -ne $run.pid -or $report.before.BirthFileTime -ne $birth -or $report.after.BirthFileTime -ne $birth -or $report.before.Tid -ne $report.after.Tid){throw 'Original owned child identity differs'}
    $result.report=$report;$result.originalChildIdentityVerified=$true;$result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention -and !$result.error
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaSchedulingControlLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,originalChildIdentityVerified,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
