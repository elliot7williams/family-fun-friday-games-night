param([switch]$KeepForegroundHost, [ValidateRange(1,99)][int]$Version = 1)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$out = Join-Path $root "continuity-evidence/print-package-transfer-v$Version"
if (!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected) { throw 'Retained NoExit TTY owner required' }
if ((Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)) { throw 'Fresh owner and evidence directory required' }
if ((Get-PSDrive C).Free -lt 600MB) { throw 'Storage reserve required' }
if (([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) { throw 'Nonadmin owner required' }
$global:LeaCompositeProcessLeases = [Collections.Generic.List[object]]::new()
$holds = [Collections.Generic.List[IDisposable]]::new()
$inputs = [Collections.Generic.List[object]]::new()
$runs = [Collections.Generic.List[object]]::new()
function Hold([string]$path, [string]$hash) {
    $stream = [IO.File]::Open($path, 'Open', 'Read', 'Read'); $holds.Add($stream)
    $actual = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if ($hash -and $actual -cne $hash) { throw "Input mismatch: $path" }
    $inputs.Add(@{ path = $path; sha256 = $actual })
}
$null = New-Item -ItemType Directory -Path $out
$result = [ordered]@{ passed = $false; inputs = $inputs; runs = $runs; printTargetCreated = $false; backendAdopted = $false }
$oldPath = $env:PATH
try {
    Hold $PSCommandPath; Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
    $owned = Join-Path $PSScriptRoot 'OwnedTestProcess.cs'
    Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    $helper = Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1'
    Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path $owned; . $helper
    $manifestPath = Join-Path $root 'continuity-evidence/print-package-cancel-v3/compiler-kit-manifest.json'
    Hold $manifestPath 'A7BF4C74CBD4EAC8E4D6099835B9B9520E1DDA59CCB952CD8C8DE14420EE4D19'
    $manifest = @(Get-Content $manifestPath -Raw | ConvertFrom-Json)
    if ($manifest.Count -ne 56) { throw 'Private compiler inventory differs' }
    foreach ($file in $manifest) { Hold $file.source $file.sha256; Hold $file.path $file.sha256 }
    $compiler = Join-Path $root 'continuity-evidence/print-package-cancel-v3/compiler-kit/cl.exe'
    $vc = 'C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\VC\Tools\MSVC\14.11.25503'
    $sdk = 'C:\Program Files (x86)\Windows Kits\10'
    $source = Join-Path $root 'Lea.Tests/PrintPackageTransfer.cpp'; Hold $source
    $fixture = Join-Path $root 'continuity-evidence/print-ticket-project-v1/bin/prepared-ticket.xps'; Hold $fixture
    $exe = Join-Path $out 'PrintPackageTransfer.exe'
    $env:PATH = (Split-Path $compiler) + ';' + $oldPath
    $arguments = @('/nologo','/EHsc','/W4','/WX','/MT','/DLEA_TRANSFER_PROBE',('/I"'+$vc+'\include"'))
    foreach ($part in @('ucrt','shared','um','winrt')) { $arguments += '/I"' + $sdk + '\Include\10.0.16299.0\' + $part + '"' }
    $arguments += @(('/Fe:"'+$exe+'"'),('/Fo:"'+(Join-Path $out 'transfer.obj')+'"'),('"'+$source+'"'),'/link',('/LIBPATH:"'+$vc+'\lib\x64"'),('/LIBPATH:"'+$sdk+'\Lib\10.0.16299.0\um\x64"'),('/LIBPATH:"'+$sdk+'\Lib\10.0.16299.0\ucrt\x64"'),'ole32.lib','shlwapi.lib')
    $build = Invoke-OwnedTestRun -Name 'compile-transfer' -Application $compiler -Arguments $arguments -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if (!$build.passed) { throw 'Native transfer compilation failed' }
    Hold $exe
    $probe = Invoke-OwnedTestRun -Name 'inspect-fixture' -Application $exe -Arguments @('"'+$fixture+'"') -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
    if (!$probe.passed) { throw 'Native fixture preflight failed' }
    $result.probe = Get-Content $probe.stdout -Raw | ConvertFrom-Json
    if (!$result.probe.passed -or $result.probe.pages -lt 1 -or $result.probe.printTargetCreated -or $result.probe.negativeChecks -ne 3) { throw 'Unexpected preflight result' }
    $result.passed = $true
} catch { $result.error = $_.Exception.ToString() }
finally {
    $env:PATH = $oldPath
    $result.inputsUnchanged = @($inputs | Where-Object { (Get-FileHash $_.path).Hash -cne $_.sha256 }).Count -eq 0
    $result.ownerAttention = $global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed = $result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if (!$result.ownerAttention) { foreach ($hold in $holds) { $hold.Dispose() } }
    else { $global:LeaTransferInputLeases = $holds }
    [IO.File]::WriteAllText((Join-Path $out 'result.json'), ($result | ConvertTo-Json -Depth 12))
    $result | ConvertTo-Json -Depth 4
}
