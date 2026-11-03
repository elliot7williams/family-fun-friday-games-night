param([ValidateSet(1,2)][int]$Version=2)
$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Test-GlobalInstallScheduling-v2.generated.ps1'
if((Get-FileHash $from).Hash -cne '6217862EF77C9554702F2036A0085397FF6403D7F6017290B8C292623A039280'){throw 'Trusted scheduling template differs'}
$to=Join-Path $PSScriptRoot "Test-GlobalInstallIsolatedScheduling-v$Version.generated.ps1"
if(Test-Path $to){throw 'Preserve isolated scheduling campaign'}
$text=[IO.File]::ReadAllText($from)
$original=$text
$text=$text.Replace('[int]$Version=2,[switch]$Isolated','[int]$Version=3,[switch]$Isolated=$true').Replace('continuity-evidence/global-install-scheduling-v2','continuity-evidence/global-install-isolated-scheduling-v1').Replace('continuity-evidence/is-connected-isolated-v1','continuity-evidence/global-install-isolated-scheduling-v1')
$text=$text.Replace("if(`$Isolated){throw 'This campaign qualifies trusted scheduling only'}","if(!`$Isolated){throw 'This campaign requires isolated scheduling'}")
$text=$text.Replace('is-connected-worker-build-20261102-v1','global-install-worker-build-20261102-v1').Replace('DB8F6F426B4773E821A7930B46BA74B59E46802B540BFFB89DD775B5550AFCB6','21EF3E9F03E594F562596DA7008A97A282CFE49D8CEC6C1D8DBFBDA2E73B112C').Replace('Lea.Tests/SchedulingRegressionMain.cs','Lea.Tests/IsolatedSchedulingRegressionMain.cs')
$anchor='    $stageBytes='
$a=$text.IndexOf($anchor);$b=$text.IndexOf("`n",$a)
if($a -lt 0 -or $b -le $a){throw 'Isolated reserve projection boundary differs'}
$reserve=@'
    $workerReserveProof='I:/LeaBuildStorage/global-install-worker-build-20261102-v1/result.json';Pin $workerReserveProof '21EF3E9F03E594F562596DA7008A97A282CFE49D8CEC6C1D8DBFBDA2E73B112C'
    $workerReserve=Get-Content $workerReserveProof -Raw|ConvertFrom-Json
    if(!$workerReserve.passed -or !$workerReserve.inputsUnchanged -or $workerReserve.ownerAttention -or $workerReserve.products.Count -ne 6 -or $workerReserve.resourceSha256 -cne '088847DC78DC1B70C5852A2C8934601110F82179A889AC9ADB1D1FF210A1B8EB'){throw 'Matching worker reserve proof incomplete'}
    $stageBytes=[long](($dlls|Measure-Object Length -Sum).Sum)+[long](($workerReserve.products|Measure-Object bytes -Sum).Sum)+857600+350720+256KB
'@
$text=$text.Substring(0,$a)+$reserve+$text.Substring($b)
$anchor="    foreach(`$name in @('Timers','Frames')){"
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Isolated trace boundary differs'}
$trace=@'
    $trace=Join-Path $PSScriptRoot 'Read-AllStartupTraceProof.ps1';Pin $trace '6454310B2E41C38B25C9A7085687BCADF9C7D7F95B3DF7D96E7F702DC9241CDF';. $trace
'@
$text=$text.Replace($anchor,$trace+[Environment]::NewLine+$anchor).Replace("('trusted-'+`$name)","('isolated-'+`$name)").Replace('PASS Lea scheduling ','PASS Lea isolated scheduling ')
$anchor='        $ok=$run.passed -and $count -gt 0'
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Isolated guard projection differs'}
$guard=@'
        $proof=Read-AllStartupTraceProof -Lines @(Get-Content $run.stderr) -StreamsComplete ([bool]($run.stdoutComplete -and $run.stderrComplete)) -ExitObserved ([bool]$run.exitObserved) -TimedOut ([bool]$run.timedOut) -KillRequested ([bool]$run.killRequested)
        $ok=$run.passed -and $count -gt 0 -and $proof.traceActivationVerified -and $proof.traceEnvironmentRestored
'@
$text=$text.Replace($anchor,$guard).Replace('checks=$count})','checks=$count;traceActivationVerified=$proof.traceActivationVerified;traceEnvironmentRestored=$proof.traceEnvironmentRestored})')
$text=$text.Replace('$result.completed=$result.cases.Count -eq 2;','$result.isolatedWorkerTested=$true;$result.completed=$result.cases.Count -eq 2;')
$text=$text.Replace('Original compiled trusted Timers.Suite and Frames.Run(trusted), unchanged assertions and budgets. Does not run timer scheduler/native/isolated branches or frame scheduler/protocol/native/isolated tests. No retry or release qualification.','Original compiled isolated Timers.Suite and Frames.Run(isolated), unchanged assertions and deadlines, matching worker. Live timer guard observations, owned jobs, trace activation/restoration and bytes/ACL checks. Excludes scheduler/protocol/native/host/full-suite qualification; no retries.')
if($Version -eq 2){$text=$text.Replace('continuity-evidence/global-install-isolated-scheduling-v1','continuity-evidence/global-install-isolated-scheduling-v2')}
foreach($critical in @('foreach($file in $dlls)','$workerProof=','$sdkPath=','$testLibrary=','$fixture=','600MB+$stageBytes','Copy-Item -LiteralPath $file.FullName','Copy-Item -LiteralPath $file.path')){
    $expected=[regex]::Matches($original,[regex]::Escape($critical)).Count
    if($expected -lt 1 -or [regex]::Matches($text,[regex]::Escape($critical)).Count -ne $expected){throw ('Critical isolated runner operation lost: '+$critical)}
}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count -or !$text.Contains('[switch]$Isolated=$true') -or $text.Contains("('trusted-'+`$name)")){throw 'Isolated scheduling syntax/projection invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
