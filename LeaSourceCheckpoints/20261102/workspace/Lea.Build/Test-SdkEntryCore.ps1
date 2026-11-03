param([switch]$KeepForegroundHost,[ValidateRange(1,3)][int]$Version=1)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$build='I:\LeaBuildStorage\sdk-entry-host-focused-20261101-v'+$Version
$out=Join-Path $root ('continuity-evidence/sdk-entry-core-runtime-v'+$Version)
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/evidence/reserve required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new();$files=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$pin){$s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));if($pin -and $hash -cne $pin){throw 'Entry runtime input differs'};$inputs.Add(@{path=$path;sha256=$hash})}
function Stage([string]$from,[string]$to,[string]$pin){Hold $from $pin;New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null;Copy-Item -LiteralPath $from -Destination $to;Hold $to $pin;$files.Add(@{path=$to;sha256=$pin;sddl=(Get-Acl -LiteralPath $to).Sddl})}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;before=$files;fullSuitePassed=$false;pageCodeEvaluated=$false;profileDeletionProven=$false}
try{
    Hold $PSCommandPath
    $buildPin=if($Version -eq 2){'00A3889A2FF0B110A7CCA009748DECB603C01147783976202D526D9E75AD9DE4'}else{'E74FDC5708A3A327E701710D690D8A2ED7996D06B5FBB8BA36CDD5518F5D59F9'}
    if($Version -eq 3){$buildPin='34DDBDD42C53357A260614A5C802491B1442464F810BEFAAB795C5B0BA2BF5DA'}
    $proof=Join-Path $build 'result.json';Hold $proof $buildPin;$compiled=Get-Content $proof -Raw|ConvertFrom-Json
    if(!$compiled.passed -or !$compiled.inputsUnchanged -or $compiled.ownerAttention -or !$compiled.hostWorkerPinInverseVerified -or $compiled.hostWorkerPinNegativeControls -ne 3){throw 'Updated host build incomplete'}
    if($Version -ge 2 -and (!$compiled.parentPinInverseVerified -or $compiled.parentPinNegativeControls -ne 3)){throw 'Combined host parent proof incomplete'}
    foreach($file in $compiled.outputs){Hold $file.path $file.sha256}
    $manifest=Get-Content (Join-Path $build 'entry-manifest.json') -Raw|ConvertFrom-Json -AsHashtable
    $fixture=Join-Path $out 'WorkerEntryFixture';$hostDirectory=Join-Path $out 'host'
    foreach($name in $manifest.fixtureFiles.Keys){
        if([IO.Path]::GetFileName($name) -cne $name){throw 'Fixture path escape'}
        $from=if($name.StartsWith('WorkerEntryCore.')){Join-Path 'I:\LeaBuildStorage\sdk-entry-core-20261101-v1' $name}else{Join-Path 'I:\LeaBuildStorage\sdk-full-script-assembly-20261101-v2\runtime\WorkerEntryFixture' $name}
        if($Version -eq 2){$from=if($name.StartsWith('WorkerEntryCore.')){Join-Path 'I:\LeaBuildStorage\sdk-entry-core-20261101-v2' $name}else{Join-Path 'I:\LeaBuildStorage\sdk-full-script-bindings-20261101-v3/runtime/WorkerEntryFixture' $name}}
        if($Version -eq 3){$from=if($name.StartsWith('WorkerEntryCore.')){Join-Path 'I:\LeaBuildStorage\sdk-entry-core-20261101-v3' $name}else{Join-Path 'I:\LeaBuildStorage\sdk-full-script-bindings-20261101-v4/runtime/WorkerEntryFixture' $name}}
        Stage $from (Join-Path $fixture $name) $manifest.fixtureFiles[$name]
    }
    if($manifest.fixtureFiles.Count -ne 7){throw 'Exactly seven interpreter-free fixture files required'}
    foreach($name in $manifest.files.Keys){
        if([IO.Path]::GetFileName($name) -cne $name){throw 'Host member escape'}
        $from=if($name -ceq 'WorkerEntryNativeHost.exe'){Join-Path $build $name}else{Join-Path 'I:\LeaBuildStorage\sdk-full-script-assembly-20261101-v2\runtime\WorkerEntryProbe' $name}
        if($Version -eq 2 -and $name -cne 'WorkerEntryNativeHost.exe'){$from=Join-Path 'I:\LeaBuildStorage\sdk-full-script-bindings-20261101-v3/runtime/WorkerEntryProbe' $name}
        if($Version -eq 3 -and $name -cne 'WorkerEntryNativeHost.exe'){$from=Join-Path 'I:\LeaBuildStorage\sdk-full-script-bindings-20261101-v4/runtime/WorkerEntryProbe' $name}
        Stage $from (Join-Path $hostDirectory $name) $manifest.files[$name]
    }
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $reader=Join-Path $root 'continuity-evidence/temporal-startup-v1/ReadEmbeddedEntryResource.cs';Hold $reader '0F4A2FA89F91C0142435A975FEE1EE0331803E6EFFDD4896D7BDFA9E1B874644';Add-Type -Path $reader
    $binary=Join-Path $hostDirectory 'WorkerEntryNativeHost.exe';$resource=[ReadEmbeddedEntryResource]::Read($binary)|ConvertFrom-Json -AsHashtable
    if($resource.directory -cne $fixture -or $resource.files.Count -ne 7){throw 'Embedded focused fixture path differs'}
    foreach($name in $resource.files.Keys){if($resource.files[$name] -cne $manifest.fixtureFiles[$name]){throw 'Embedded fixture hash differs'}}
    if((Get-PSDrive C).Free -lt 600MB){throw 'Preserve reserve after fixture staging'}
    $run=Invoke-OwnedTestRun -Name 'entry-core-native' -Application $binary -Arguments @(('"'+$fixture+'"')) -WorkingDirectory $hostDirectory -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$run.passed -or (Get-Item $run.stderr).Length){throw 'Confined entry host failed'}
    $report=Get-Content $run.stdout -Raw|ConvertFrom-Json
    $workerHash=if($Version -eq 2){'5BF8B589731CB1A0A5DE38B0DAE1AF516DBF21DB5982141C9C013C55E6D39596'}else{'F8BE188734FB0B747C131E396269E9698A02E257ED2DCF6DCB1DB7DECADFC81C'}
    if($Version -eq 3){$workerHash='E777E0CAE71D694BBA96E43E01BEED102B2B512B0B24B4ABE11AF2CDB3E7C2C6'}
    if(!$report.passed -or $report.checks.Count -ne 15 -or @($report.checks|Where-Object {!$_.passed -or !$_.name}).Count -or !$report.core.passed -or $report.core.workerSha256 -cne $workerHash -or !$report.childTerminal -or !$report.nativeIdentityVerified -or !$report.guardsVerified -or $report.nativeExit -ne 0 -or $report.jobFlags -ne 5 -or $report.cpuRateBasisPoints -ne 2500 -or !$report.outputComplete -or $report.cleanupTerminatedChild -or !$report.aclRestored -or !$report.hashesUnchanged -or $report.pageCodeEvaluated){throw 'Confined entry report identity/coverage/guards differ'}
    $result.report=$report;$result.checks=15;$result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@($files|Where-Object {(Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl -or (Get-FileHash $_.path).Hash -cne $_.sha256})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaSdkEntryCoreRuntimeLeases=$holds}
    $result|ConvertTo-Json -Depth 9|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
