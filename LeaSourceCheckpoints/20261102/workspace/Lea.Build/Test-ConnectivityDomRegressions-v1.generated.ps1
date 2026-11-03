param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'continuity-evidence/connectivity-dom-regressions-v1'
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
$result=@{passed=$false;inputs=$inputs;runs=$runs;before=@();fullSuitePassed=$false;timingFailureResolved=$false;assertionsChanged=$false;scope='Unchanged original focused containment/subtree isolated and native assertions on exact connectivity candidate. No full DOM/full-suite/host/reliability qualification.'}
try{
    Hold $PSCommandPath
    $receipt=Join-Path $root 'continuity-evidence/sdk-full-script-suite-v8/staging.json'
    Hold $receipt '90FD0D60BA1CCE04C0923D6E18E7FB5D218F4F04C129DD49F97477AFFD2384CA'
    $staging=Get-Content $receipt -Raw|ConvertFrom-Json
    if(!$staging.verified -or $staging.executed -or $staging.compiledSources -ne 60){throw 'Exact fresh-source inventory required'}
    $files=@($staging.files|Where-Object {$_.path -match '^runtime/[^/]+$' -or $_.path -match '^runtime/ScriptWorker/[^/]+$' -or $_.path -cin @('fixtures/LeaForge/test-pages/javascript-contains.html','fixtures/LeaForge/test-pages/javascript-selectors.html','fixtures/LeaForge/test-pages/javascript-subtree.html')})
    if($files.Count -ne 25 -or @($files|Where-Object {$_.path -like 'runtime/ScriptWorker/*'}).Count -ne 6){throw 'Focused closure differs'}
    $parentProof='I:\LeaBuildStorage\is-connected-script-build-20261102-v1/result.json';Hold $parentProof '2596ED5B2B5E5210324DBE403C1526E029E4F99240080C4434DCD028445D46F9'
    $parent=Get-Content $parentProof -Raw|ConvertFrom-Json
    $workerProof='I:\LeaBuildStorage\is-connected-worker-build-20261102-v1/result.json';Hold $workerProof 'DB8F6F426B4773E821A7930B46BA74B59E46802B540BFFB89DD775B5550AFCB6'
    $worker=Get-Content $workerProof -Raw|ConvertFrom-Json
    if(!$parent.passed -or !$parent.inputsUnchanged -or $parent.ownerAttention -or $parent.sha256 -cne 'FBD50D216D724A4BFCA0488C53CB1B648BC659AAE19F800AE7E129B207EBBD4C' -or !$worker.passed -or !$worker.inputsUnchanged -or $worker.ownerAttention -or $worker.products.Count -ne 6){throw 'Matching connectivity candidate incomplete'}
    $replacements=@{'runtime/Lea.Script.dll'=@{origin=$parent.binary;sha256=$parent.sha256}}
    foreach($product in $worker.products){$key='runtime/ScriptWorker/'+[IO.Path]::GetFileName($product.path);if($replacements.ContainsKey($key)){throw 'Duplicate worker replacement'};$replacements.Add($key,@{origin=$product.path;sha256=$product.sha256})}
    $rebound=0
    foreach($file in $files){if($replacements.ContainsKey($file.path)){$replacement=$replacements[$file.path];$file.origin=$replacement.origin;$file.sha256=$replacement.sha256;$rebound++}}
    if($rebound -ne 7){throw 'All exact parent/worker bindings required'}
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
    $result.cases=[Collections.Generic.List[object]]::new();$result.passed=$true
    $definitions=@(
        @{name='contains-isolated';arguments=@('--contains','isolated');footer='contains isolated';checks=22},
        @{name='subtree-isolated';arguments=@('--subtree-isolated');footer='subtree isolated';checks=0},
        @{name='contains-native';arguments=@('--contains','native',('"'+(Join-Path $out 'contains')+'"'));footer='contains native';checks=0},
        @{name='subtree-native';arguments=@('--subtree-view',('"'+(Join-Path $out 'subtree')+'"'));footer='subtree view';checks=0}
    )
    foreach($case in $definitions){
        $arguments=@(('"'+(Join-Path $out 'fixtures')+'"'))+$case.arguments
        $run=Invoke-OwnedTestRun -Name $case.name -Application (Join-Path $out 'runtime/Lea.Script.Tests.exe') -Arguments $arguments -WorkingDirectory (Join-Path $out 'runtime') -OutputDirectory $out -Seconds 30 -Records $runs
        $lines=@(Get-Content $run.stdout)
        $pattern='^PASS Lea\.Script '+[regex]::Escape($case.footer)+': ([0-9]+) checks\.$'
        $footers=@($lines|Where-Object {$_ -cmatch $pattern})
        $count=if($footers.Count -eq 1 -and $footers[0] -cmatch $pattern){[int]$Matches[1]}else{0}
        $proof=Read-AllStartupTraceProof -Lines @(Get-Content $run.stderr) -StreamsComplete ([bool]($run.stdoutComplete -and $run.stderrComplete)) -ExitObserved ([bool]$run.exitObserved) -TimedOut ([bool]$run.timedOut) -KillRequested ([bool]$run.killRequested)
        $ok=$run.passed -and $footers.Count -eq 1 -and $count -gt 0 -and ($case.checks -eq 0 -or $count -eq $case.checks) -and $proof.traceActivationVerified -and $proof.traceEnvironmentRestored
        $result.cases.Add(@{name=$case.name;passed=$ok;checks=$count;traceActivationVerified=$proof.traceActivationVerified;traceEnvironmentRestored=$proof.traceEnvironmentRestored})
        if(!$ok){$result.passed=$false;throw ('Original DOM regression failed: '+$case.name)}
        foreach($file in $result.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'DOM runtime drift prevents next case'}}
        if($global:LeaCompositeProcessLeases.Count -or (Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)){throw 'DOM process retirement incomplete'}
    }
    $result.completed=$result.cases.Count -eq 4
    $result.passed=$result.passed -and $result.completed
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
