param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$partial='I:\LeaBuildStorage\memory-policy-sdk-memory-20261110-v1\result.json'
$out='I:\LeaBuildStorage\memory-policy-sdk-policy-resume-20261110-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Test-Path -LiteralPath $out) -or (Get-PSDrive C).Free -lt 650MB -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Restored reserve and fresh output/owner required'}
if(@(Get-Process 'Firespider*','Lea.Script.Worker','Lea.Script.Tests','Lea.Image','Lea.ScriptEmbeddingSample' -ErrorAction SilentlyContinue).Count){throw 'No overlapping tests'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Hash=''){$s=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($s);$actual=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));if($Hash -and $actual -cne $Hash){throw 'Resume input differs'};$inputs.Add(@{path=$Path;sha256=$actual})}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;runs=$runs;inputs=$inputs;before=@();partialSha256='859E8E8910AEEEC4559050DA6323239E365E82130E2FCF59880950BCF5108CDA';freeCAtResume=(Get-PSDrive C).Free;causeOfSpaceDropProven=$false;hostAdopted=$false;globalSecurityQualified=$false}
try{
 Hold $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination $out
 Hold $partial $result.partialSha256
 $p=Get-Content -LiteralPath $partial -Raw|ConvertFrom-Json
 if($p.passed -or $p.completed -or !$p.inputsUnchanged -or $p.ownerAttention -or $p.runs.Count -ne 5 -or $p.before.Count -ne 5 -or $p.byteChanges.Count -or $p.aclChanges.Count -or $p.error -cne 'System.Management.Automation.RuntimeException: Storage reserve required'){throw 'Exact five-run storage stop required'}
 foreach($run in $p.runs){if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.ownedJob -or !$run.originalHandleCaptured -or !$run.childRetirementIndependentlyAudited -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0){throw 'Earlier partial run not safely retired'}}
 if(@($p.runs|Where-Object name -CEQ 'sdk-memory-policy').Count){throw 'Never replay an already-started policy test'}
 foreach($file in $p.inputs){Hold $file.path $file.sha256}
 foreach($file in $p.before){Hold $file.path $file.sha256;if((Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Partial runtime ACL differs'}}
 $result.before=$p.before
 $owner=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owner 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owner
 $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
 $run=Invoke-OwnedTestRun -Name 'sdk-memory-policy' -Application (Join-Path $p.runtime 'SdkMemory-policy.exe') -Arguments @() -WorkingDirectory $p.runtime -OutputDirectory $out -Seconds 30 -Records $runs
 if(!$run.passed -or @((Get-Content -LiteralPath $run.stdout)|Where-Object {$_ -ceq 'PASS Lea memory script policy: 7 checks'}).Count -ne 1){throw 'SDK memory policy failed'}
 $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
 $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
 $result.byteChanges=@($result.before|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256})
 $result.aclChanges=@($result.before|Where-Object {(Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl})
 $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
 $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.byteChanges.Count -and !$result.aclChanges.Count -and !$result.ownerAttention
 if(!$result.ownerAttention){foreach($h in $holds){$h.Dispose()}}else{$global:LeaSdkPolicyResumeLeases=$holds}
 $result|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $out 'result.json') -Encoding utf8
 $result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
