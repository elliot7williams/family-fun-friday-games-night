param([switch]$KeepForegroundHost,[ValidateRange(1,99)][int]$Version=2,[switch]$SafetyOnly)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$prefix=if($SafetyOnly){'print-default-net48-v'}else{'print-transfer-render-v'}
$out = Join-Path $root ('continuity-evidence/'+$prefix+$Version)
if (!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected) { throw 'Retained NoExit TTY owner required' }
if ((Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)) { throw 'Fresh owner/evidence required' }
if ((Get-PSDrive C).Free -lt 600MB) { throw 'Storage reserve required' }
$global:LeaCompositeProcessLeases = [Collections.Generic.List[object]]::new()
$holds = [Collections.Generic.List[IDisposable]]::new(); $inputs = [Collections.Generic.List[object]]::new(); $runs = [Collections.Generic.List[object]]::new()
function Hold([string]$path, [string]$hash) {
    $s = [IO.File]::Open($path,'Open','Read','Read'); $holds.Add($s)
    $actual = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if ($hash -and $actual -cne $hash) { throw "Pin differs: $path" }
    $inputs.Add(@{ path = $path; sha256 = $actual })
}
$null = New-Item -ItemType Directory -Path $out
$result = [ordered]@{ passed = $false; inputs = $inputs; runs = $runs; printJobSubmitted = $false }
try {
    Hold $PSCommandPath; Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
    $owned = Join-Path $PSScriptRoot 'OwnedTestProcess.cs'; Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    $helper = Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1'; Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path $owned; . $helper
    $probeName=if($SafetyOnly){'Lea.Tests/PrintDefaultSafetyMain.cs'}else{'Lea.Tests/RenderPrintTransfer.cs'}
    $probe = Join-Path $root $probeName; Hold $probe; Copy-Item $probe (Join-Path $out 'probe.cs')
    $prior = Join-Path $root 'continuity-evidence/print-package-cancel-v5/result.json'; Hold $prior 'F7DDA9DBBBA0AE6375B628942A7EB8EC0D26D9ED7629F93847E42D3F2644270B'
    $receipt = Get-Content $prior -Raw | ConvertFrom-Json
    $source = Join-Path $root 'continuity-evidence/print-ticket-project-v1/bin/prepared-ticket.xps'
    $returned = Join-Path $root 'continuity-evidence/print-package-cancel-v5/completed-output.xps'
    $pin = @($receipt.inputs | Where-Object path -eq $source); if ($pin.Count -ne 1) { throw 'Fixture producer missing' }
    Hold $source $pin[0].sha256; Hold $returned $receipt.completedOutputSha256
    $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe';Hold $compiler
    $refs='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    $application=Join-Path $out 'RenderPrintTransfer.exe'
    $compileArguments=@('/nologo','/noconfig','/nostdlib+','/target:exe','/platform:x64','/warnaserror+',('/out:"'+$application+'"'))
    foreach($name in @('mscorlib.dll','System.dll','System.Core.dll','System.Xml.dll','PresentationCore.dll','PresentationFramework.dll','WindowsBase.dll','ReachFramework.dll','System.Xaml.dll','System.Web.Extensions.dll')){
        $reference=Join-Path $refs $name;Hold $reference;$compileArguments+=('/reference:"'+$reference+'"')
    }
    $compileArguments+=('"'+$probe+'"')
    if($SafetyOnly){
        $leaseSource=Join-Path $root 'Lea.Tests/PrintPackageLease.cs';Hold $leaseSource;Copy-Item $leaseSource (Join-Path $out 'PrintPackageLease.cs')
        $compileArguments+=('"'+$leaseSource+'"')
    }
    $build=Invoke-OwnedTestRun -Name 'compile-render' -Application $compiler -Arguments $compileArguments -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$build.passed){throw 'Render compilation failed'}
    Hold $application
    $arguments = @(('"'+$source+'"'),('"'+$returned+'"'),('"'+$out+'"'))
    if($SafetyOnly){
        $bridge=Join-Path $root 'continuity-evidence/print-package-cancel-v5/LeaPrintTransfer.dll'
        $bridgePin=@($receipt.inputs|Where-Object path -eq $bridge);if($bridgePin.Count -ne 1){throw 'Bridge producer pin missing'}
        Hold $bridge $bridgePin[0].sha256
        $arguments=@(('"'+$bridge+'"'),('"'+$out+'"'))
    }
    $run = Invoke-OwnedTestRun -Name 'render-output' -Application $application -Arguments $arguments -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
    if (!$run.passed) { throw 'Owned render failed' }
    if($SafetyOnly){
        $result.safety=Get-Content (Join-Path $out 'safety.json') -Raw|ConvertFrom-Json
        $result.passed=$result.safety.passed -and $result.safety.checks -eq 19 -and !$result.safety.printTargetCreated
    }else{
        $result.render = Get-Content (Join-Path $out 'render.json') -Raw | ConvertFrom-Json
        $result.passed = $result.render.rendered
    }
} catch { $result.error = $_.Exception.ToString() }
finally {
    $result.inputsUnchanged = @($inputs | Where-Object { (Get-FileHash $_.path).Hash -cne $_.sha256 }).Count -eq 0
    $result.ownerAttention = $global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed = $result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if (!$result.ownerAttention) { foreach ($h in $holds) { $h.Dispose() } } else { $global:LeaRenderInputLeases = $holds }
    [IO.File]::WriteAllText((Join-Path $out 'result.json'), ($result | ConvertTo-Json -Depth 9))
    [pscustomobject]$result | Select-Object passed,error,ownerAttention,inputsUnchanged,render | ConvertTo-Json -Depth 7
}
