$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Test-DeterministicContainsRecovery.ps1'
if((Get-FileHash $from).Hash -cne '5C41FAA35D0515D0605DB2F86EE57D8ABD73A80A2EC1020BA38C47F180381F2E'){throw 'Original DOM test template differs'}
$to=Join-Path $PSScriptRoot 'Test-ConnectivityDomRegressions-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve DOM regression runner'}
$text=[IO.File]::ReadAllText($from).Replace('continuity-evidence/deterministic-contains-recovery-v1','continuity-evidence/connectivity-dom-regressions-v1')
$text=$text.Replace('$_.path -ceq ''fixtures/LeaForge/test-pages/javascript-contains.html''','$_.path -cin @(''fixtures/LeaForge/test-pages/javascript-contains.html'',''fixtures/LeaForge/test-pages/javascript-selectors.html'',''fixtures/LeaForge/test-pages/javascript-subtree.html'')').Replace('$files.Count -ne 23','$files.Count -ne 25')
$needle='    $bytes=0L'
if([regex]::Matches($text,[regex]::Escape($needle)).Count -ne 1){throw 'Unique candidate binding boundary required'}
$bindings=@'
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
'@
$text=$text.Replace($needle,$bindings+[Environment]::NewLine+$needle)
$start=$text.IndexOf('    $run=Invoke-OwnedTestRun')
$end=$text.IndexOf('}catch{', $start)
if($start -lt 0 -or $end -le $start){throw 'Focused run boundaries differ'}
$body=@'
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
'@
$text=$text.Substring(0,$start)+$body+[Environment]::NewLine+$text.Substring($end)
$text=$text.Replace('Existing --contains isolated entry on fresh exact-byte NTFS closure; no full-prefix/cold-equivalence claim','Unchanged original focused containment/subtree isolated and native assertions on exact connectivity candidate. No full DOM/full-suite/host/reliability qualification.')
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'DOM regression runner syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
