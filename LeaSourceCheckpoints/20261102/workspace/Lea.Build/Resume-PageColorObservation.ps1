param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/page-color-host-v3'
$receipt=Join-Path $out 'result.json';$output=Join-Path $out 'resume-result.json'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
$identity=[Security.Principal.WindowsIdentity]::GetCurrent()
if(([Security.Principal.WindowsPrincipal]::new($identity)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin required'}
if(Test-Path $output){throw 'Preserve continuation evidence'}
if((Get-PSDrive C).Free -lt 600MB){throw 'Storage reserve required'}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','Lea.Firespider.Boot','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap tests'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$leases=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Hash){
    $s=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($s)
    if(!$Hash){$Hash=(Get-FileHash $Path).Hash}
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $Hash){throw ('Input differs: '+$Path)}
    $inputs.Add(@{path=$Path;sha256=$Hash})
}
$result=[ordered]@{passed=$false;runs=$runs;inputs=$inputs;scope='Resume prepared diagnostic whose original attempt started no processes; raw result retained';before=@()}
try{
    Hold $PSCommandPath
    Hold $receipt '80005D6BCA602D68ECE902C096121AA8078731161583C8B291F43966E57E2A67'
    $prior=Get-Content $receipt -Raw|ConvertFrom-Json
    if($prior.runs.Count -or $prior.ownerAttention -or !$prior.inputsUnchanged -or !$prior.error.Contains('Storage reserve required')){throw 'Not an unstarted storage-only preparation'}
    foreach($pin in $prior.inputs){Hold $pin.path $pin.sha256}
    Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    . (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    $runtime=$prior.candidate
    $rsp=Join-Path $out 'harness.rsp';Hold $rsp
    $wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261026.ps1';Hold $wrapper 'C3EBB3801FAFB80DA031AAB69FE4837EEB4E4EECD3907F9704F9D6186FA42F0A'
    $run=Invoke-OwnedTestRun -Name 'compile-observation' -Application (Get-Process -Id $PID).Path -Arguments @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',(Get-FileHash $rsp).Hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') -WorkingDirectory $workspace -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$run.passed){throw 'Observation harness compilation failed'}
    $v2=Join-Path $workspace 'continuity-evidence/page-color-host-v2'
    $v2receipt=Join-Path $v2 'result.json';Hold $v2receipt 'EDF91113117D6882F0CA39D9D457C4A52A2F5E7390E894EBF58DA317263BF1D0'
    foreach($name in @('BrowserFeatures10.vb','FirespiderLea.vb')){
        $path=Join-Path $v2 ('host-source/'+$name);Hold $path (Get-FileHash (Join-Path $out ('host-source/'+$name))).Hash
    }
    $exe=Join-Path $v2 'host-bin/Firespider Internet 19.exe';Hold $exe '8E5568E57BEF08FCB45735ABEC6BD54D2B827D5F17E0CAABE4CEF87F41A87E76'
    Copy-Item -LiteralPath $exe -Destination (Join-Path $runtime 'Firespider Internet 19.exe')
    $result.before=@(Get-ChildItem $runtime -Recurse -File|ForEach-Object{@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}})
    $run=Invoke-OwnedTestRun -Name 'observe-theme' -Application (Join-Path $runtime 'Lea.Firespider.Boot.exe') -Arguments @(('"'+$runtime+'"'),'about') -WorkingDirectory $runtime -OutputDirectory $out -Seconds 90 -Records $runs
    $result.behaviorPassed=$run.passed
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@($result.before|Where-Object{(Get-FileHash $_.path).Hash -cne $_.sha256 -or (Get-Acl $_.path).Sddl -cne $_.sddl})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.behaviorPassed -and !$result.error -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaPageColorInputLeases=$leases}else{foreach($s in $leases){$s.Dispose()}}
    [IO.File]::WriteAllText($output,($result|ConvertTo-Json -Depth 8))
    [pscustomobject]$result|Select-Object passed,behaviorPassed,ownerAttention,error|ConvertTo-Json -Compress
}
