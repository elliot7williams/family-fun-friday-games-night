$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'New-ExternalBootstrapProgram.ps1')
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$out=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/external-bootstrap-tools-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve existing tool evidence'}
$path=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/numeric-references-script-suite-v1/source/Lea.Script.Tests/Program.cs'
if((Get-FileHash -LiteralPath $path).Hash -cne '132A2F90B895C9650AA4B8D5B85E307CC2A7AE10BE77363082178F369814322E'){throw 'Suite source differs'}
$original=[IO.File]::ReadAllText($path)
$overlay=New-ExternalBootstrapProgram $original
$call='var execution = script.Ex(code);'
$check='Check(execution.Ok && execution.N == 6, "computed script: " + execution.S + "; " + execution.M);'
$rows=[Collections.Generic.List[object]]::new()
$rows.Add(@{case='real-source';passed=$overlay.inverseVerified -and $overlay.assertionPreserved -and $overlay.originalCallPreserved -and $overlay.stopCount -eq 1 -and !$overlay.text.Contains('BootstrapCostObserver')})
foreach($test in @(
    @{name='missing-call';text=$original.Replace($call,'')},
    @{name='duplicate-call';text=$original+"`r`n"+$call},
    @{name='missing-assertion';text=$original.Replace($check,'')},
    @{name='duplicate-assertion';text=$original+"`r`n"+$check},
    @{name='repeated-overlay';text=$overlay.text}
)){
    $rejected=$false
    try{$null=New-ExternalBootstrapProgram $test.text}catch{$rejected=$_.Exception.Message -ceq 'External diagnostic overlay shape differs'}
    $rows.Add(@{case=$test.name;passed=$rejected})
}
Add-Type -Path (Join-Path $PSScriptRoot 'ExternalProcessCounters.cs')
$process=[Diagnostics.Process]::GetCurrentProcess()
try{
    $handle=$process.Handle
    $before=[Lea.Build.ExternalProcessCounters]::Read($handle)
    Start-Sleep -Milliseconds 150
    $after=[Lea.Build.ExternalProcessCounters]::Read($handle)
}finally{$process.Dispose()}
$rows.Add(@{case='native-counter-sanity';passed=$after.CpuTicks -ge $before.CpuTicks -and $after.ReadBytes -ge $before.ReadBytes -and $after.WriteBytes -ge $before.WriteBytes -and $after.WorkingSetBytes -gt 0 -and $after.TotalPhysicalBytes -gt 0 -and $after.AvailablePhysicalBytes -le $after.TotalPhysicalBytes -and $after.MemoryLoadPercent -le 100})
$rejected=$false
try{$null=[Lea.Build.ExternalProcessCounters]::Read([IntPtr]::Zero)}catch{$rejected=$_.Exception.InnerException -is [ComponentModel.Win32Exception] -or $_.Exception -is [ComponentModel.Win32Exception]}
$rows.Add(@{case='invalid-handle-rejected';passed=$rejected})
$result=@{passed=@($rows|Where-Object {!$_.passed}).Count -eq 0;checks=$rows;before=$before;after=$after;engineExecuted=$false;releaseQualified=$false;inputs=@(Get-FileHash -LiteralPath $path,(Join-Path $PSScriptRoot 'ExternalProcessCounters.cs'),(Join-Path $PSScriptRoot 'New-ExternalBootstrapProgram.ps1'),$PSCommandPath | Select-Object Path,Hash)}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 6))
$result|ConvertTo-Json -Depth 6
if(!$result.passed){throw 'External bootstrap tool validation failed'}
