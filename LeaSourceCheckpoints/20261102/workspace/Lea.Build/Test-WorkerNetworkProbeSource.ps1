$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/worker-network-probe-source-controls-v1'
if(Test-Path $out){throw 'Preserve source controls'}
$source=Join-Path $workspace 'continuity-evidence/worker-local-reference-build-v1/archive/source'
$program=[IO.File]::ReadAllText((Join-Path $source 'Program.cs')).Replace("`r`n","`n")
$probe=[IO.File]::ReadAllText((Join-Path $source 'NetworkProbe.cs')).Replace("`r`n","`n")
. (Join-Path $PSScriptRoot 'New-WorkerNetworkProbeSource.ps1')
$overlay=New-WorkerNetworkProbeSource $program $probe
$rows=[Collections.Generic.List[object]]::new()
if(!$overlay.inverseVerified){throw 'Inverse proof missing'}
$rows.Add(@{name='exact-overlay-inverse';passed=$true})
foreach($statement in @('client = new TcpClient();','connect = client.BeginConnect(IPAddress.Loopback, port, null, null);','var remaining = (int)Math.Max(0, 500 - clock.ElapsedMilliseconds);','if (!wait.WaitOne(remaining)) return 2;','client.EndConnect(connect);','public void Dispose() { if (client != null) client.Close(); }')){
    if([regex]::Matches($overlay.probe,[regex]::Escape($statement)).Count -ne 1){throw 'Original probe operation changed'}
}
$rows.Add(@{name='original-socket-denial-wait-and-cleanup-statements';passed=$true})
foreach($case in @(
    @{name='missing-program-call';program=$program.Replace('new NetworkProbe(Wire.Number(init, "p"))','new NetworkProbe(0)');probe=$probe},
    @{name='duplicate-program-call';program=($program+"`n"+'                using (var probe = new NetworkProbe(Wire.Number(init, "p")))');probe=$probe},
    @{name='missing-constructor';program=$program;probe=$probe.Replace('internal NetworkProbe(int port)','internal NetworkProbe(long port)')},
    @{name='duplicate-client';program=$program;probe=($probe+"`n"+'                client = new TcpClient();')},
    @{name='missing-connect';program=$program;probe=$probe.Replace('                connect = client.BeginConnect(IPAddress.Loopback, port, null, null);','')},
    @{name='repeated-program-overlay';program=$overlay.program;probe=$probe},
    @{name='repeated-probe-overlay';program=$program;probe=$overlay.probe}
)){
    $rejected=$false
    try{$null=New-WorkerNetworkProbeSource $case.program $case.probe}catch{$rejected=$_.Exception.Message -ceq 'Network diagnostic anchor differs'}
    if(!$rejected){throw ('Control failed: '+$case.name)}
    $rows.Add(@{name=$case.name;passed=$true})
}
$null=New-Item -ItemType Directory $out
@{passed=$true;controls=$rows;runtimeExecuted=$false;inputs=@(Get-FileHash -LiteralPath $PSCommandPath,(Join-Path $PSScriptRoot 'New-WorkerNetworkProbeSource.ps1'),(Join-Path $source 'Program.cs'),(Join-Path $source 'NetworkProbe.cs')|Select-Object Path,Hash)}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'result.json')
@{passed=$true;controls=$rows.Count;runtimeExecuted=$false}|ConvertTo-Json
