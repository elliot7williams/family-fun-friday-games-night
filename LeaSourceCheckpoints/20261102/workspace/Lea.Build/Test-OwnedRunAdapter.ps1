param([switch]$KeepForegroundHost,[ValidateSet(1,2)][int]$Version=2)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace ('continuity-evidence/owned-run-adapter-controls-v'+$Version)
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
$identity=[Security.Principal.WindowsIdentity]::GetCurrent()
if(([Security.Principal.WindowsPrincipal]::new($identity)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin required'}
if(Test-Path -LiteralPath $out){throw 'Preserve adapter controls'}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
if((Get-PSDrive C).Free -lt 512MB){throw 'Storage reserve required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$null=New-Item -ItemType Directory -Path $out
$leases=[Collections.Generic.List[IDisposable]]::new()
$source=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';$adapter=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1'
foreach($path in @($source,$adapter,$PSCommandPath)){$leases.Add([IO.File]::Open($path,'Open','Read','Read'))}
$pins=@(Get-FileHash $source,$adapter,$PSCommandPath|Select-Object Path,Hash)
$rows=[Collections.Generic.List[object]]::new()
$result=[ordered]@{passed=$false;controls=$rows;inputs=$pins;productionChanged=$false;scope='Standalone owned Run adapter controls, no full-suite gate attempted'}
function Args([string]$body){@('-NoProfile','-EncodedCommand',[Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($body)))}
try{
    Add-Type -Path $source
    . $adapter
    $normal=Invoke-OwnedTestRun -Name 'normal' -Application ([Environment]::ProcessPath) -Arguments (Args '[Console]::Out.WriteLine("normal");[Console]::Error.WriteLine("error-channel");exit 0') -WorkingDirectory $workspace -OutputDirectory $out -Seconds 30
    if(!$normal.passed -or !$normal.childRetirementIndependentlyAudited -or $normal.activeAfterRetirement -ne 0 -or $normal.killRequested -or (Get-Content $normal.stdout -Raw) -cne ('normal'+[Environment]::NewLine)){throw 'Normal adapter control failed'}
    $rows.Add(@{name='normal';passed=$true})
    $failed=Invoke-OwnedTestRun -Name 'exit-seven' -Application ([Environment]::ProcessPath) -Arguments (Args 'exit 7') -WorkingDirectory $workspace -OutputDirectory $out -Seconds 30
    if($failed.passed -or $failed.exitCode -ne 7 -or !$failed.childRetirementIndependentlyAudited -or $failed.unsafeToContinue){throw 'Failed child was promoted or cleanup not proved'}
    $rows.Add(@{name='nonzero-stays-failed';passed=$true})
    $timeout=Invoke-OwnedTestRun -Name 'timeout' -Application ([Environment]::ProcessPath) -Arguments (Args 'Start-Sleep -Seconds 60') -WorkingDirectory $workspace -OutputDirectory $out -Seconds 1
    if($timeout.passed -or !$timeout.timedOut -or !$timeout.killRequested -or !$timeout.childRetirementIndependentlyAudited -or !$timeout.outputDrained -or $timeout.unsafeToContinue){throw 'Timeout failed to preserve behavioral failure and independent retirement'}
    $rows.Add(@{name='timeout-stays-failed-job-retired';passed=$true})
    $shell=[Environment]::ProcessPath.Replace("'","''")
    $child=Args 'Start-Sleep -Seconds 60'
    $body='$p=[Diagnostics.Process]::new();$p.StartInfo=[Diagnostics.ProcessStartInfo]::new('''+$shell+''');$p.StartInfo.Arguments='''+([string]::Join(' ',$child))+''';$p.StartInfo.UseShellExecute=$false;$p.StartInfo.CreateNoWindow=$true;if(!$p.Start()){exit 5};exit 0'
    $orphan=Invoke-OwnedTestRun -Name 'live-descendant' -Application ([Environment]::ProcessPath) -Arguments (Args $body) -WorkingDirectory $workspace -OutputDirectory $out -Seconds 30
    if($orphan.passed -or !$orphan.killRequested -or !$orphan.childRetirementIndependentlyAudited -or $orphan.activeAfterRetirement -ne 0 -or $orphan.unsafeToContinue){throw 'Live descendant did not fail gate and retire safely'}
    $rows.Add(@{name='orphan-stays-failed-job-retired';passed=$true})
    $overflow=Invoke-OwnedTestRun -Name 'overflow' -Application ([Environment]::ProcessPath) -Arguments (Args '[Console]::Out.Write(("x" * (5MB)));Start-Sleep -Seconds 60') -WorkingDirectory $workspace -OutputDirectory $out -Seconds 60
    if($overflow.passed -or !$overflow.captureOverflow -or $overflow.stdoutComplete -or !$overflow.outputDrained -or !$overflow.childRetirementIndependentlyAudited -or $overflow.unsafeToContinue -or (Get-Item $overflow.stdout).Length -gt 4MB){throw 'Output cap/drain control failed'}
    $rows.Add(@{name='overflow-stays-failed-bounded-and-drained';passed=$true})
    $result.passed=$rows.Count -eq 5
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($pins|Where-Object {(Get-FileHash $_.Path).Hash -cne $_.Hash}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaOwnedAdapterInputLeases=$leases}else{foreach($s in $leases){$s.Dispose()}}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 7))
    [pscustomobject]$result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json -Compress
}
