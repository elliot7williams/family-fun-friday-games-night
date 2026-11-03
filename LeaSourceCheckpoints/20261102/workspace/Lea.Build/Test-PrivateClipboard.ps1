param([switch]$KeepForegroundHost,[ValidateRange(1,9)][int]$Version=1)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent;$out=Join-Path $root ('continuity-evidence/private-clipboard-v'+$Version)
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin owner required'}
if((Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/evidence required'}
if((Get-PSDrive C).Free -lt 600MB){throw 'Storage reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Firespider.Boot','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Hash){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($stream)
    $actual=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($Hash -and $Hash -cne $actual){throw ('Input differs: '+$Path)};$inputs.Add(@{path=$Path;sha256=$actual})
}
$null=New-Item -ItemType Directory -Path $out
$result=[ordered]@{passed=$false;inputs=$inputs;runs=$runs;interactiveClipboardRead=$false;interactiveClipboardWritten=$false;securityQualified=$false;browserAdopted=$false}
try{
    Hold $PSCommandPath;Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path $owned;. $helper
    $proof=Join-Path $root 'continuity-evidence/document-copy-v1/result.json';Hold $proof '5D8CD2221F9273F3DDFC482F0A4C891744C155421B9C2A1306B0265B0D87F8FA'
    $prior=Get-Content $proof -Raw|ConvertFrom-Json
    if(!$prior.passed -or !$prior.inputsUnchanged -or $prior.ownerAttention){throw 'Copy candidate focused proof incomplete'}
    foreach($binding in @(@('Lea.Engine.dll','A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791'),@('Lea.Win.dll','E62D0E1B137DF7AF782CC655234999F13CEDE614B7B1B24DF152BB85CDE9571B'))){
        $from=Join-Path $root ('continuity-evidence/document-copy-v1/'+$binding[0]);Hold $from $binding[1]
        $to=Join-Path $out $binding[0];Copy-Item $from $to;Hold $to $binding[1]
    }
    foreach($name in @('document-selection.html','print-friendly.html')){
        $from=Join-Path $root ('LeaForge/test-pages/'+$name);Hold $from;Copy-Item $from (Join-Path $out $name)
    }
    $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe';Hold $compiler
    $refs='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    $exe=Join-Path $out 'PrivateClipboardChecks.exe'
    $arguments=@('/nologo','/noconfig','/nostdlib+','/target:exe','/platform:x64','/warnaserror+',('/out:"'+$exe+'"'),('/reference:"'+(Join-Path $out 'Lea.Engine.dll')+'"'),('/reference:"'+(Join-Path $out 'Lea.Win.dll')+'"'))
    foreach($name in @('mscorlib.dll','System.dll','System.Core.dll','System.Drawing.dll','System.Windows.Forms.dll')){
        $path=Join-Path $refs $name;Hold $path;$arguments+=('/reference:"'+$path+'"')
    }
    foreach($name in @('PrivateClipboardStation.cs','PrivateClipboardMain.cs')){
        $path=Join-Path $root ('Lea.Tests/'+$name);Hold $path;Copy-Item $path (Join-Path $out $name);$arguments+=('"'+$path+'"')
    }
    $build=Invoke-OwnedTestRun -Name 'compile-private-clipboard' -Application $compiler -Arguments $arguments -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$build.passed){throw 'Private clipboard helper compilation failed'}
    Hold $exe
    $run=Invoke-OwnedTestRun -Name 'private-clipboard' -Application $exe -Arguments @('"'+$out+'"') -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
    $details=Join-Path $out 'clipboard.json'
    if(Test-Path $details){
        $result.clipboard=Get-Content $details -Raw|ConvertFrom-Json
        if($result.clipboard.station){
            Add-Type -Path (Join-Path $out 'PrivateClipboardStation.cs')
            $result.stationProbeError=[PrivateClipboardStation]::ProbeAfterExit($result.clipboard.station)
            $result.stationGone=$result.stationProbeError -eq 2
        }
    }
    $result.passed=$run.passed -and $result.clipboard.passed -and $result.clipboard.privateEstablished -and $result.stationGone -and !$result.clipboard.interactiveClipboardRead -and !$result.clipboard.interactiveClipboardWritten
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaPrivateClipboardInputLeases=$holds}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 9))
    [pscustomobject]$result|Select-Object passed,stationGone,stationProbeError,ownerAttention,error,clipboard|ConvertTo-Json -Depth 5
}
