param([switch]$KeepForegroundHost,[switch]$PrivateBindings,[ValidatePattern('^[A-F0-9]{64}$')][string]$PrivateBindingManifestSha256)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$stage='I:\LeaBuildStorage\deterministic-firespider-source-20261102-v1'
$out='I:\LeaBuildStorage\deterministic-firespider-build-20261102-v1'
if($PrivateBindings){if(!$PrivateBindingManifestSha256){throw 'Pinned private host manifest required'};$stage='I:\LeaBuildStorage\private-binding-firespider-source-20261103-v1';$out='I:\LeaBuildStorage\private-binding-firespider-build-20261103-v1'}elseif($PrivateBindingManifestSha256){throw 'Unexpected private host pin'}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/evidence/reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$pin){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse build input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($pin -and $hash -cne $pin){throw ('Host build input differs: '+$path)};$inputs.Add(@{path=$path;sha256=$hash})
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;browserExecuted=$false;installedBrowserChanged=$false;releaseQualified=$false;products=@();scope='Compile preserved Firespider source against exact deterministic SDK; no runtime/host qualification inferred'}
try{
    Hold $PSCommandPath
    $manifestPin=if($PrivateBindings){$PrivateBindingManifestSha256}else{'B5D5787E5574FB1CE7666F2A5B2B1EB1DE6E35B1C8F5CC50127A20FF971480C6'}
    $manifest=Join-Path $stage 'candidate-manifest.json';Hold $manifest $manifestPin
    $plan=Get-Content $manifest -Raw|ConvertFrom-Json
    if(!$plan.verified -or !$plan.hostSourceUnchanged -or $plan.files.Count -ne 105 -or $plan.preservedHostSources -ne 87 -or $plan.sdkProducts -ne 12 -or $plan.thirdPartyDependencies -ne 6){throw 'Coherent host plan incomplete'}
    if($PrivateBindings -and (!$plan.privateBindingPair -or @($plan.files|Where-Object kind -ceq 'qualified-private-binding-product').Count -ne 2)){throw 'Matching private host source plan required'}
    $result.privateBindingPair=[bool]$PrivateBindings
    foreach($file in $plan.files){Hold (Join-Path $stage $file.path) $file.sha256}
    $proof=Join-Path $root 'continuity-evidence/firespider-source-rebuild-v1/result.json';Hold $proof '9DDF92B7273943C6CB5481A8CCC8033F09053BE2C5968DC5E53EDA14E0A5F3DA'
    $prior=Get-Content $proof -Raw|ConvertFrom-Json
    if(!$prior.passed -or !$prior.compilerPassed -or $prior.ownerAttention){throw 'Preserved compiler proof incomplete'}
    $compiler=$prior.compiler;Hold $compiler.path $compiler.sha256
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $bin=Join-Path $out 'bin'
    $args=@(('"'+(Join-Path $stage 'Firespider Internet.vbproj')+'"'),'/nologo','/m:1','/nodeReuse:false','/t:Rebuild','/v:minimal','/p:Configuration=Release','/p:GenerateManifests=false','/p:AutoGenerateBindingRedirects=true','/p:LeaScriptEnabled=true','/p:UseSharedCompilation=false',('/p:OutputPath="'+$bin+'"'))
    $run=Invoke-OwnedTestRun -Name 'compile-deterministic-firespider' -Application $compiler.path -Arguments $args -WorkingDirectory $stage -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$run.passed){throw 'Deterministic Firespider compilation failed'}
    foreach($file in $plan.files|Where-Object kind -ne 'preserved-host-source'){
        $path=Join-Path $bin $file.path.Substring('Lea/'.Length);Hold $path $file.sha256
    }
    if(@(Get-ChildItem (Join-Path $bin 'ScriptWorker') -File).Count -ne 6){throw 'Built worker closure differs'}
    $exe=Join-Path $bin 'Firespider Internet 19.exe';Hold $exe
    $result.binary=$exe;$result.hostSha256=(Get-FileHash $exe).Hash
    $result.products=@(Get-ChildItem $bin -Recurse -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;bytes=$_.Length}})
    $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaDeterministicHostBuildLeases=$holds}
    $result|ConvertTo-Json -Depth 8|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,hostSha256,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
