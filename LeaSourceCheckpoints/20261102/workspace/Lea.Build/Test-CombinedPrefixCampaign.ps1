param([switch]$KeepForegroundHost,[ValidateRange(1,3)][int]$Attempts=3)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$candidate=Join-Path $root 'continuity-evidence/combined-prefix-startup-runtime-v1'
$out='I:\LeaBuildStorage\combined-prefix-startup-campaign-20261101-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/output and original reserve required'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'No native test/browser overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new();$observations=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$pin){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse campaign input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($pin -and $pin -cne $hash){throw ('Campaign input differs: '+$path)};$inputs.Add(@{path=$path;sha256=$hash})
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{diagnosticCaptured=$false;fullSuitePassed=$false;startupFailureResolved=$false;causeProven=$false;attemptLimit=$Attempts;inputs=$inputs;runs=$runs;observations=$observations;before=@();originalBefore=@()}
try{
    Hold $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $proof='I:\LeaBuildStorage\combined-prefix-startup-build-20261101-v1/result.json';Hold $proof '8F3A805C0EB0B21BAEC6892CA704E8A6EED8765BE8D498FF7EAB376BE40076F3'
    $prior=Get-Content $proof -Raw|ConvertFrom-Json
    if(!$prior.diagnosticCaptured -or !$prior.prefixVerified -or !$prior.inputsUnchanged -or !$prior.computedPassed -or $prior.ownerAttention -or $prior.changedFiles.Count -or $prior.before.Count -ne 65 -or $prior.originalBefore.Count -ne 65){throw 'Verified retired prefix diagnostic required'}
    $result.before=$prior.before;$result.originalBefore=$prior.originalBefore
    foreach($file in $prior.inputs){Hold $file.path $file.sha256}
    foreach($file in @($prior.before)+@($prior.originalBefore)){if((Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Prior runtime ACL differs'}}
    for($attempt=1;$attempt -le $Attempts;$attempt++){
        if((Get-PSDrive C).Free -lt 600MB){throw 'Original launch reserve lost between observations'}
        $run=Invoke-OwnedTestRun -Name ('prefix-'+$attempt) -Application (Join-Path $candidate 'runtime/Lea.Script.Tests.exe') -Arguments @(('"'+(Join-Path $candidate 'fixtures')+'"')) -WorkingDirectory (Join-Path $candidate 'runtime') -OutputDirectory $out -Seconds 300 -Records $runs
        if(!$run.exitObserved -or $run.timedOut -or $run.killRequested -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0){throw 'Diagnostic lifecycle incomplete; stop campaign'}
        $lines=@(Get-Content $run.stdout);$stderr=@(Get-Content $run.stderr)
        $records=@($lines|Where-Object {$_.StartsWith('COMBINED_WORKER_STARTUP ',[StringComparison]::Ordinal)})
        if($records.Count -ne 1){throw 'Prefix stopped outside target capture; retain evidence and stop'}
        $report=$records[0].Substring('COMBINED_WORKER_STARTUP '.Length)|ConvertFrom-Json
        $observations.Add(@{attempt=$attempt;report=$report;elapsedMs=$run.elapsedMs})
        if(!$report.valid -or !$report.exited -or !$report.observerRetired -or !$report.traceRestored -or $report.sampleError -or $report.overflow -or !$report.samples.Count -or $report.samples.Count -gt 60 -or $report.startupBudgetMs -ne 5000 -or $report.executionBudgetMs -ne 2000 -or $report.cpuRate -ne 2500 -or $report.memoryBytes -ne 134217728){throw 'Startup capture invalid'}
        $phases=@($stderr|Where-Object {$_ -cmatch '^LEA_SUITE_PHASE '})
        if($phases.Count -ne 59){throw 'Original prefix phase count differs'}
        for($i=0;$i -lt 29;$i++){if($phases[2*$i] -cnotmatch ('^LEA_SUITE_PHASE [0-9]+ '+$i+' begin$') -or $phases[2*$i+1] -cnotmatch ('^LEA_SUITE_PHASE [0-9]+ '+$i+' end$')){throw 'Original prefix sequence differs'}}
        if($phases[-1] -cnotmatch '^LEA_SUITE_PHASE [0-9]+ 29 begin$' -or @($stderr|Where-Object {$_ -ceq 'LEA_ALL_STARTUP_TRACE restored=true'}).Count -ne 1){throw 'Target/trace restoration differs'}
        $stops=@($lines|Where-Object {$_ -ceq 'Diagnostic stop after isolated computed-script assertion; not full suite.'}).Count
        if($report.scriptPassed){if(!$run.passed -or $stops -ne 1 -or $report.mutations -ne 6 -or $report.title -cne 'Lea 7'){throw 'Successful terminal assertion differs'}}
        elseif($run.exitCode -ne 1 -or $stops -or @($stderr|Where-Object {$_ -cmatch '^System.InvalidOperationException: FAIL isolated computed script:'}).Count -ne 1){throw 'Failed terminal assertion differs'}
        foreach($file in @($result.before)+@($result.originalBefore)){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256 -or (Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Runtime changed between observations; stop'}}
        if(!$report.scriptPassed){$result.targetFailureCaptured=$true;break}
        Write-Output ('Diagnostic observation '+$attempt+' captured; original computed assertion passed. Not full suite.')
    }
    $result.diagnosticCaptured=$observations.Count -eq $Attempts -or $result.targetFailureCaptured
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@(foreach($file in @($result.before)+@($result.originalBefore)){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256 -or (Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){$file.path}})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.diagnosticCaptured=$result.diagnosticCaptured -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaCombinedPrefixCampaignLeases=$holds}
    $result|ConvertTo-Json -Depth 16|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object diagnosticCaptured,targetFailureCaptured,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
