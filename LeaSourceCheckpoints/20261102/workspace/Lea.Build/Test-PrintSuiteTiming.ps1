param([switch]$KeepForegroundHost,[ValidateRange(1,9)][int]$Version=1)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root ('continuity-evidence/print-suite-timing-v'+$Version)
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin owner required'}
if((Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh evidence/owner required'}
if((Get-PSDrive C).Free -lt 600MB){throw 'Storage reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Firespider.Boot','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Hash){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($stream)
    $actual=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($Hash -and $actual -cne $Hash){throw ('Input differs: '+$Path)}
    $inputs.Add(@{path=$Path;sha256=$actual})
}
$null=New-Item -ItemType Directory -Path $out
$result=[ordered]@{passed=$false;diagnosticOnly=$true;fullRegressionQualified=$false;inputs=$inputs;runs=$runs}
try{
    Hold $PSCommandPath;Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path $owned;. $helper
    $proofPath=Join-Path $root 'continuity-evidence/print-html-project-v1/result.json'
    Hold $proofPath '7586782F4CFEBBA5893B75647CEF29B1C5966474714430A10233E02AC0FB79A6'
    $proof=Get-Content $proofPath -Raw|ConvertFrom-Json
    if(!$proof.inputsUnchanged -or $proof.ownerAttention -or !$proof.runs[0].passed){throw 'Candidate build ownership incomplete'}
    $baselinePath=Join-Path $root 'continuity-evidence/print-live-capture-v1/result.json'
    Hold $baselinePath '751F1EF75A7E184DFE245D1DEFE7732EBBB4A58C5E34AD284DFB716AAE57F123'
    $baseline=Get-Content $baselinePath -Raw|ConvertFrom-Json
    foreach($name in @('Lea.Engine.dll','Lea.Win.dll','PrintTextChecks.exe','native-selection.html','print-document.html','print-styles.html','print-styled-document.html')){
        $from=Join-Path $root ('continuity-evidence/print-html-project-v1/bin/'+$name)
        $prior=Join-Path $root ('continuity-evidence/print-live-capture-v1/'+$name)
        $pins=@($baseline.inputs|Where-Object path -ceq $prior|Select-Object -ExpandProperty sha256 -Unique)
        if($pins.Count -ne 1){throw 'Baseline pin missing'}
        Hold $from $pins[0];$to=Join-Path $out $name;Copy-Item $from $to;Hold $to $pins[0]
    }
    $library=Join-Path $root 'continuity-evidence/print-html-project-v1/bin/Lea.Print.dll'
    Hold $library 'F14AD95878DC0824BA02DF146F8047D5BC750062B30A250CA42236E7C650ED6C'
    Copy-Item $library (Join-Path $out 'Lea.Print.dll');Hold (Join-Path $out 'Lea.Print.dll') 'F14AD95878DC0824BA02DF146F8047D5BC750062B30A250CA42236E7C650ED6C'
    $source=Join-Path $root 'Lea.Tests/PrintSuiteTiming.cs';Hold $source
    Copy-Item $source (Join-Path $out 'PrintSuiteTiming.cs')
    $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe';Hold $compiler
    $refs='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    $exe=Join-Path $out 'PrintSuiteTiming.exe'
    $arguments=@('/nologo','/noconfig','/nostdlib+','/target:exe','/platform:x64','/warnaserror+',('/out:"'+$exe+'"'),('"'+$source+'"'))
    foreach($name in @('mscorlib.dll','System.dll')){$reference=Join-Path $refs $name;Hold $reference;$arguments+=('/reference:"'+$reference+'"')}
    $build=Invoke-OwnedTestRun -Name 'compile-timing' -Application $compiler -Arguments $arguments -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$build.passed){throw 'Timing host compilation failed'}
    Hold $exe
    $run=Invoke-OwnedTestRun -Name 'suite-timing' -Application $exe -Arguments @(('"'+(Join-Path $out 'PrintTextChecks.exe')+'"'),('"'+(Join-Path $out 'native-selection.html')+'"')) -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
    $result.footerVerified=@(Get-Content $run.stdout|Where-Object {$_ -ceq 'PASS Lea live styled capture and foundation'}).Count -eq 1
    $result.passed=$run.passed -and $result.footerVerified
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaPrintTimingInputLeases=$holds}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 9))
    [pscustomobject]$result|Select-Object passed,diagnosticOnly,fullRegressionQualified,footerVerified,ownerAttention,error|ConvertTo-Json
}
