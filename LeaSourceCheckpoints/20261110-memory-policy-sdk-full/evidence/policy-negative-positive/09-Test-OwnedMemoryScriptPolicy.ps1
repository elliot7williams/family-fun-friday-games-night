param([switch]$KeepForegroundHost,[string]$Core='',[string]$ExpectedCoreSha256='',[switch]$Patched)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=if($Patched){'I:\LeaBuildStorage\memory-script-policy-patched-20261110-v1'}else{'I:\LeaBuildStorage\memory-script-policy-negative-20261110-v1'}
if(!$Core){$Core=Join-Path $root 'continuity-evidence/memory-document-view-v1/runtime/Lea.Engine.dll';$ExpectedCoreSha256='F05E474C73BAAE19278EE4C80D9D617BD361432B3AADCD472BEA680280E9CBFD'}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Test-Path -LiteralPath $out) -or (Get-PSDrive C).Free -lt 600MB -or $ExpectedCoreSha256 -cnotmatch '^[A-F0-9]{64}$' -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/output/reserve and exact core pin required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$runs=[Collections.Generic.List[object]]::new();$inputs=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$Path,[string]$Hash=''){$s=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($s);$actual=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));if($Hash -and $actual -cne $Hash){throw 'Policy input differs'};$inputs.Add(@{path=$Path;sha256=$actual})}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;runs=$runs;inputs=$inputs;coreSha256=$ExpectedCoreSha256;hostAdopted=$false;completeSecurityQualified=$false}
try{
 Hold $PSCommandPath;Hold $Core $ExpectedCoreSha256
 $driver=Join-Path $root 'Lea.Tests/MemoryScriptPolicyMain.cs';Hold $driver;Copy-Item -LiteralPath $driver -Destination $out
 Copy-Item -LiteralPath $Core -Destination (Join-Path $out 'Lea.Engine.dll');Hold (Join-Path $out 'Lea.Engine.dll') $ExpectedCoreSha256
 $owner=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owner 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owner
 $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
 $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe';Hold $compiler
 $exe=Join-Path $out 'MemoryScriptPolicy.exe'
 $run=Invoke-OwnedTestRun -Name 'compile-memory-script-policy' -Application $compiler -Arguments @('/nologo','/target:exe',('/out:"'+$exe+'"'),('/reference:"'+$Core+'"'),('"'+$driver+'"')) -WorkingDirectory $out -OutputDirectory $out -Seconds 60 -Records $runs
 if(!$run.passed){throw 'Policy probe compilation failed'};Hold $exe
 $run=Invoke-OwnedTestRun -Name 'memory-script-policy' -Application $exe -Arguments @() -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
 if(!$run.passed -or @((Get-Content -LiteralPath $run.stdout)|Where-Object {$_ -ceq 'PASS Lea memory script policy: 7 checks'}).Count -ne 1){throw 'Memory script policy regression failed'}
 $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
 $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
 $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
 $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
 if(!$result.ownerAttention){foreach($h in $holds){$h.Dispose()}}else{$global:LeaMemoryPolicyHolds=$holds}
 $result|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $out 'result.json') -Encoding utf8
 $result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
