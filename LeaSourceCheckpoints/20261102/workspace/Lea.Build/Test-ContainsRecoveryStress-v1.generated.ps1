param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'continuity-evidence/contains-recovery-stress-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/evidence/reserve required'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','Lea.Image','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$pin){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($pin -and $hash -cne $pin){throw 'Focused recovery input differs'};$inputs.Add(@{path=$path;sha256=$hash})
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;before=@();fullSuitePassed=$false;timingFailureResolved=$false;assertionsChanged=$false;scope='Three separate existing uninstrumented --contains isolated processes; each original30s with verified retirement and byte/ACL equality before next attempt. Runtime path reused; no full-prefix/cold-cache/reliability-release claim'}
try{
    Hold $PSCommandPath
    $receipt=Join-Path $root 'continuity-evidence/sdk-full-script-suite-v8/staging.json'
    Hold $receipt '90FD0D60BA1CCE04C0923D6E18E7FB5D218F4F04C129DD49F97477AFFD2384CA'
    $staging=Get-Content $receipt -Raw|ConvertFrom-Json
    if(!$staging.verified -or $staging.executed -or $staging.compiledSources -ne 60){throw 'Exact fresh-source inventory required'}
    $files=@($staging.files|Where-Object {$_.path -match '^runtime/[^/]+$' -or $_.path -match '^runtime/ScriptWorker/[^/]+$' -or $_.path -ceq 'fixtures/LeaForge/test-pages/javascript-contains.html'})
    if($files.Count -ne 23 -or @($files|Where-Object {$_.path -like 'runtime/ScriptWorker/*'}).Count -ne 6){throw 'Focused closure differs'}
    $bytes=0L
    foreach($file in $files){Hold $file.origin $file.sha256;$bytes+=(Get-Item $file.origin).Length}
    if((Get-Volume -DriveLetter C).FileSystem -cne 'NTFS' -or (Get-PSDrive C).Free -lt (600MB+$bytes)){throw 'Original reserve after fresh staging required'}
    $before=[Collections.Generic.List[object]]::new()
    foreach($file in $files){
        if([IO.Path]::IsPathRooted($file.path) -or $file.path.Contains(':') -or $file.path.Split('/') -contains '..'){throw 'Staging path escape'}
        $to=Join-Path $out $file.path;New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null
        Copy-Item -LiteralPath $file.origin -Destination $to;Hold $to $file.sha256
        $before.Add(@{path=$to;sha256=$file.sha256;sddl=(Get-Acl $to).Sddl})
    }
    $result.before=$before
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $trace=Join-Path $PSScriptRoot 'Read-AllStartupTraceProof.ps1';Hold $trace '6454310B2E41C38B25C9A7085687BCADF9C7D7F95B3DF7D96E7F702DC9241CDF';. $trace
    $result.attempts=[Collections.Generic.List[object]]::new()
    for($iteration=1;$iteration -le 3;$iteration++){
    $run=Invoke-OwnedTestRun -Name ('contains-isolated-'+$iteration) -Application (Join-Path $out 'runtime/Lea.Script.Tests.exe') -Arguments @(('"'+(Join-Path $out 'fixtures')+'"'),'--contains','isolated') -WorkingDirectory (Join-Path $out 'runtime') -OutputDirectory $out -Seconds 30 -Records $runs
    $lines=@(Get-Content $run.stdout)
    $result.footerVerified=@($lines|Where-Object {$_ -ceq 'PASS Lea.Script contains isolated: 22 checks.'}).Count -eq 1
    $result.recoveryAssertionVerified=@($lines|Where-Object {$_ -clike 'PASS isolated contains: limit resets and recovers; ok;*'}).Count -eq 1
    $proof=Read-AllStartupTraceProof -Lines @(Get-Content $run.stderr) -StreamsComplete ([bool]($run.stdoutComplete -and $run.stderrComplete)) -ExitObserved ([bool]$run.exitObserved) -TimedOut ([bool]$run.timedOut) -KillRequested ([bool]$run.killRequested)
    $result.traceActivationVerified=$proof.traceActivationVerified;$result.traceEnvironmentRestored=$proof.traceEnvironmentRestored
    $result.passed=$run.passed -and $result.footerVerified -and $result.recoveryAssertionVerified -and $result.traceActivationVerified -and $result.traceEnvironmentRestored
        $result.attempts.Add(@{iteration=$iteration;passed=$result.passed;footerVerified=$result.footerVerified;recoveryAssertionVerified=$result.recoveryAssertionVerified;traceActivationVerified=$result.traceActivationVerified;traceEnvironmentRestored=$result.traceEnvironmentRestored})
        if(!$result.passed){throw 'Focused recovery attempt failed; stop rather than retry'}
        foreach($file in $result.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Runtime byte/ACL drift between attempts'}}
        if($global:LeaCompositeProcessLeases.Count -or (Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)){throw 'Unretired process prevents next attempt'}
    }
    if($runs.Count -ne 3 -or $result.attempts.Count -ne 3){throw 'Three completed attempts required'}
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@(foreach($file in $result.before){$sddl=(Get-Acl $file.path).Sddl;$hash=(Get-FileHash $file.path).Hash;if($sddl -cne $file.sddl -or $hash -cne $file.sha256){@{path=$file.path;originalSddl=$file.sddl;currentSddl=$sddl;originalSha256=$file.sha256;currentSha256=$hash}}})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaContainsRecoveryLeases=$holds}
    $result|ConvertTo-Json -Depth 8|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
