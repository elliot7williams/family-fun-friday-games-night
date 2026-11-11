param([switch]$KeepForegroundHost,[switch]$MemoryReport,[switch]$GuardedSdk)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$base=if($MemoryReport){'I:\LeaBuildStorage\memory-report-host-source-20261110-v1'}else{'I:\LeaBuildStorage\accessibility-host-source-20261110-v1'}
$out=if($MemoryReport){'I:\LeaBuildStorage\memory-report-host-build-20261110-v1'}else{'I:\LeaBuildStorage\accessibility-host-build-20261110-v1'}
if($GuardedSdk){if(!$MemoryReport){throw 'Guarded SDK requires memory report host'};$base='I:\LeaBuildStorage\guarded-sdk-host-source-20261110-v1';$out='I:\LeaBuildStorage\guarded-sdk-host-build-20261110-v1'}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Test-Path -LiteralPath $out) -or (Get-PSDrive C).Free -lt 600MB -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Original reserve and fresh output/owner required'}
if(@(Get-Process 'Firespider*','Lea.Script.Worker','Lea.Script.Tests','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No test/browser overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$runs=[Collections.Generic.List[object]]::new();$inputs=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$Path,[string]$Hash=''){$s=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($s);$actual=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));if($Hash -and $actual -cne $Hash){throw 'Build input differs'};$inputs.Add(@{path=$Path;sha256=$actual})}
$result=@{passed=$false;runs=$runs;inputs=$inputs;products=@();browserExecuted=$false;installedBrowserChanged=$false;releaseQualified=$false}
$oldTemp=$env:TEMP;$oldTmp=$env:TMP
New-Item -ItemType Directory -Path $out|Out-Null
try{
 Hold $PSCommandPath
 $planPath=Join-Path $base 'source.json';Hold $planPath
 $plan=Get-Content -LiteralPath $planPath -Raw|ConvertFrom-Json
 $expectedChanges=if($MemoryReport){4}else{2}
 if(!$plan.verified -or !$plan.routeInverseVerified -or !$plan.adapterPrefixUnchanged -or $plan.files.Count -ne 105 -or @($plan.files|Where-Object changed).Count -ne $expectedChanges -or ($MemoryReport -and !$plan.memoryReport)){throw 'Scoped source candidate required'}
 foreach($file in $plan.files){Hold (Join-Path $plan.source $file.path) $file.sha256}
 foreach($file in $plan.fragments){Hold $file.path $file.sha256}
 if($GuardedSdk){if(!$plan.guardedSourceSdk -or $plan.fullSuiteAuditSha256 -cne 'D4F27CA15CA471D79AD33078752890321E0CC8BEAD83CA198FBCF85601F5A2BA' -or $plan.sdkProducts.Count -ne 12){throw 'Qualified guarded SDK binding required'};foreach($file in $plan.inputs){Hold $file.path $file.sha256}}
 $compilerProof=Join-Path $root 'continuity-evidence/firespider-source-rebuild-v1/result.json'
 Hold $compilerProof '9DDF92B7273943C6CB5481A8CCC8033F09053BE2C5968DC5E53EDA14E0A5F3DA'
 $previous=Get-Content -LiteralPath $compilerProof -Raw|ConvertFrom-Json
 if(!$previous.passed -or !$previous.compilerPassed -or $previous.ownerAttention){throw 'Compiler provenance required'}
 Hold $previous.compiler.path $previous.compiler.sha256
 $owner=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owner 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owner
 $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
 $env:TEMP=Join-Path $out 'temp';$env:TMP=$env:TEMP;New-Item -ItemType Directory -Path $env:TEMP|Out-Null
 $bin=Join-Path $out 'bin'
 $args=@(('"'+(Join-Path $plan.source 'Firespider Internet.vbproj')+'"'),'/nologo','/m:1','/nodeReuse:false','/t:Rebuild','/v:minimal','/p:Configuration=Release','/p:GenerateManifests=false','/p:AutoGenerateBindingRedirects=true','/p:LeaScriptEnabled=true','/p:UseSharedCompilation=false',('/p:OutputPath="'+$bin+'"'))
 $run=Invoke-OwnedTestRun -Name 'compile-accessibility-host' -Application $previous.compiler.path -Arguments $args -WorkingDirectory $plan.source -OutputDirectory $out -Seconds 120 -Records $runs
 if(!$run.passed){throw 'Accessibility host compilation failed'}
 $result.products=@(Get-ChildItem -LiteralPath $bin -Recurse -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash}})
 if($result.products.Count -ne 22){throw 'Complete22file output required'}
 if($GuardedSdk){foreach($file in $plan.sdkProducts){if(!$file.path.StartsWith('Lea/')){throw 'Unexpected SDK output role'};$product=Join-Path $bin $file.path.Substring(4);if((Get-FileHash -LiteralPath $product).Hash -cne $file.sha256){throw 'Built host SDK product differs from qualified SDK'}};$result.guardedSdkProductsVerified=$true}
 $result.hostSha256=(Get-FileHash -LiteralPath (Join-Path $bin 'Firespider Internet 19.exe')).Hash
 $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
 $env:TEMP=$oldTemp;$env:TMP=$oldTmp
 $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
 $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
 $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
 if(!$result.ownerAttention){foreach($h in $holds){$h.Dispose()}}else{$global:LeaAccessibilityHostHolds=$holds}
 $result|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $out 'result.json') -Encoding utf8
 $result|Select-Object passed,hostSha256,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
