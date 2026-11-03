param([switch]$KeepForegroundHost,[ValidateRange(1,9)][int]$Version=1,[switch]$Cpu)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root ('continuity-evidence/temporal-startup-v1/owned-thread-snapshot-control-v'+$Version)
if($Cpu){$out=Join-Path $root ('continuity-evidence/temporal-startup-v1/owned-thread-cpu-control-v'+$Version)}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin owner required'}
if((Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner required'}
if((Get-PSDrive C).Free -lt 600MB){throw 'Storage reserve required'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Expected){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($stream)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($Expected -and $Expected -cne $hash){throw 'Pinned control input differs'};$inputs.Add(@{path=$Path;sha256=$hash})
}
$null=New-Item -ItemType Directory -Path $out
$result=@{passed=$false;interpreterExecuted=$false;releaseQualified=$false;inputs=$inputs;runs=$runs}
try{
    Hold $PSCommandPath;Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe';Hold $compiler
    $exe=Join-Path $out 'ThreadSnapshotControl.exe'
    $arguments=@('/nologo','/noconfig','/nostdlib+','/target:exe','/platform:x64','/warnaserror+',('/out:"'+$exe+'"'))
    $refs='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    foreach($name in @('mscorlib.dll','System.dll','System.Core.dll','System.Web.Extensions.dll')){$path=Join-Path $refs $name;Hold $path;$arguments+=('/reference:"'+$path+'"')}
    $controlSource=if($Cpu){'Lea.Tests/ThreadCpuCalibration.cs'}else{'Lea.Tests/OwnedThreadSnapshotControl.cs'}
    foreach($relative in @('Lea.Build/OwnedThreadSnapshot.cs',$controlSource)){
        $from=Join-Path $root $relative;Hold $from
        $to=Join-Path $out ([IO.Path]::GetFileName($from));Copy-Item $from $to;Hold $to;$arguments+=('"'+$to+'"')
    }
    $built=Invoke-OwnedTestRun -Name 'compile-thread-control' -Application $compiler -Arguments $arguments -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$built.passed){throw 'Thread control compilation failed'}
    Hold $exe
    $report=Join-Path $out 'control.json'
    $tested=Invoke-OwnedTestRun -Name 'thread-control' -Application $exe -Arguments @('"'+$report+'"') -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$tested.passed){throw 'Native thread calibration failed'}
    $result.control=Get-Content $report -Raw|ConvertFrom-Json
    $expectedChecks=if($Cpu){35}else{8};$footer=if($Cpu){'THREAD_CPU_CALIBRATION PASS 35'}else{'THREAD_SNAPSHOT PASS 8'}
    $result.passed=$result.control.passed -and $result.control.checks -eq $expectedChecks -and @((Get-Content $tested.stdout)|Where-Object {$_ -ceq $footer}).Count -eq 1
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaThreadControlInputLeases=$holds}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 8))
    $result|Select-Object passed,control,inputsUnchanged,ownerAttention,error|ConvertTo-Json -Depth 4
}
