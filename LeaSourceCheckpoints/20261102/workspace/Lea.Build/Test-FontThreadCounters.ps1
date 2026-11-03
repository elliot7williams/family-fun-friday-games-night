param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'continuity-evidence/temporal-startup-v1/font-thread-counters-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin owner required'}
if((Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner required'}
if((Get-PSDrive C).Free -lt 600MB){throw 'Storage reserve required'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','Lea.Firespider.Boot','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
$samples=[Collections.Generic.List[object]]::new();$sampleErrors=[Collections.Generic.List[string]]::new();$sampling=@{last=-100L;overflow=$false}
function Hold([string]$Path,[string]$Expected){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($stream)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($Expected -and $hash -cne $Expected){throw 'Thread diagnostic input differs'};$inputs.Add(@{path=$Path;sha256=$hash})
}
function Take-FontThreadSample([Lea.Build.OwnedTestProcess]$Owner,[long]$Elapsed,[long]$StderrBytes){
    if($Elapsed-$sampling.last -lt 100 -or $sampleErrors.Count -or $Owner.WaitForExit(0)){return}
    $sampling.last=$Elapsed
    if($samples.Count -ge 512){$sampling.overflow=$true;return}
    $held=$false;$safe=$null
    try{
        $field=$Owner.GetType().GetField('process',[Reflection.BindingFlags]'Instance,NonPublic')
        if(!$field){throw 'Original handle field absent'}
        $safe=$field.GetValue($Owner)
        if($safe -isnot [Runtime.InteropServices.SafeHandle] -or $safe.IsInvalid -or $safe.IsClosed){throw 'Original safe handle unavailable'}
        $safe.DangerousAddRef([ref]$held);$clock=[Diagnostics.Stopwatch]::StartNew()
        $snapshot=[Lea.Build.OwnedThreadSnapshot]::Read($safe.DangerousGetHandle(),$Owner.Id,$Owner.CreationFileTime)
        $samples.Add(@{elapsedMs=$Elapsed;stderrBytesObserved=$StderrBytes;queryMs=$clock.Elapsed.TotalMilliseconds;snapshot=$snapshot})
    }catch{if(!$Owner.WaitForExit(0)){$sampleErrors.Add($_.Exception.ToString())}}
    finally{if($held){$safe.DangerousRelease()}}
}
$null=New-Item -ItemType Directory -Path $out
$result=@{diagnosticCaptured=$false;fullSuitePassed=$false;runtimeReusedReadOnly=$true;limitsChanged=$false;inputs=$inputs;runs=$runs;samples=$samples;sampleErrors=$sampleErrors}
try{
    Hold $PSCommandPath;Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
    $controlPath=Join-Path $root 'continuity-evidence/temporal-startup-v1/owned-thread-snapshot-control-v1/result.json';Hold $controlPath 'BCC044C4CE8A57C497A7E5D13A339F8A2F3227F4457956A70650FDA5B380E41D'
    $control=Get-Content $controlPath -Raw|ConvertFrom-Json
    if(!$control.passed -or !$control.inputsUnchanged -or $control.ownerAttention -or $control.control.checks -ne 8){throw 'Thread sampler control incomplete'}
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $native=Join-Path $PSScriptRoot 'OwnedThreadSnapshot.cs';Hold $native '43F229376D069D3BF7CA7BC9CFDA5F0F36AD9BABC80DB125ADF800A40E552D52';Add-Type -Path $native
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    $original=[IO.File]::ReadAllText($helper)
    $anchor="            if(@(`$readers|Where-Object overflow).Count){throw 'Test output exceeds4MiB guard'}"
    $insert="            Take-FontThreadSample `$owner `$clock.ElapsedMilliseconds ([long](@(`$readers|Where-Object name -ceq 'stderr'|Measure-Object bytes -Sum).Sum))`r`n"+$anchor
    $edits=@(@('function Invoke-OwnedTestRun {','function Invoke-FontThreadRun {'),@($anchor,$insert))
    $text=$original
    foreach($edit in $edits){if($edit.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($edit[0])).Count -ne 1){throw 'Thread observer anchor ambiguous'};$text=$text.Replace($edit[0],$edit[1])}
    function Valid([string]$Value){for($i=$edits.Count-1;$i -ge 0;$i--){if([regex]::Matches($Value,[regex]::Escape($edits[$i][1])).Count -ne 1){return $false};$Value=$Value.Replace($edits[$i][1],$edits[$i][0])};return $Value -ceq $original}
    if(!(Valid $text)){throw 'Thread observer inverse differs'}
    foreach($bad in @(($text+"`n"),($text+$text),$text.Replace('5000','5001'))){if($bad -ceq $text -or (Valid $bad)){throw 'Thread observer negative accepted'}}
    $observed=Join-Path $out 'Invoke-FontThreadRun.ps1';[IO.File]::WriteAllText($observed,$text);Hold $observed;. $observed
    $result.ownerProjectionInverseVerified=$true;$result.ownerProjectionNegativeControls=3
    $oldPath=Join-Path $root 'continuity-evidence/temporal-startup-v1/font-bootstrap-counters-v1/result.json';Hold $oldPath '0A5F0A16D34A3F566EE47B02A317C05BF47452C865330A8DE54A600A1841F345'
    $old=Get-Content $oldPath -Raw|ConvertFrom-Json
    if(!$old.diagnosticCaptured -or !$old.inputsUnchanged -or $old.ownerAttention -or $old.changedFiles.Count -or $old.before.Count -ne 65){throw 'Read-only prefix runtime proof incomplete'}
    $result.before=$old.before
    foreach($file in $old.before){Hold $file.path $file.sha256;if((Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Retained runtime ACL changed'}}
    $runtime=Join-Path (Split-Path $oldPath) 'runtime'
    $buildPath=Join-Path $root 'continuity-evidence/temporal-startup-v1/font-script-suite-v1/result.json';Hold $buildPath 'E87409ED36E23F7C968490DDF206AAEF2D9D990AC9FAED2E144C1712A72EEF7E'
    $build=Get-Content $buildPath -Raw|ConvertFrom-Json
    foreach($file in $build.fixtureFiles){Hold $file.path $file.sha256}
    $tested=Invoke-FontThreadRun -Name 'thread-prefix' -Application (Join-Path $runtime 'Lea.Script.Tests.exe') -Arguments @(('"'+$build.fixtureSource+'"'),'--bootstrap-cost-diagnostic') -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
    if($tested.timedOut -or $tested.killRequested -or !$tested.outputDrained -or !$tested.childRetirementIndependentlyAudited){throw 'Thread diagnostic lifecycle incomplete'}
    $lines=@(Get-Content $tested.stderr);$phases=@($lines|Where-Object {$_ -match '^LEA_SUITE_PHASE '})
    if($phases.Count -ne 17){throw 'Prefix phase count differs'}
    for($i=0;$i -lt 8;$i++){if($phases[2*$i] -notmatch ('^LEA_SUITE_PHASE \d+ '+$i+' begin$') -or $phases[2*$i+1] -notmatch ('^LEA_SUITE_PHASE \d+ '+$i+' end$')){throw 'Prefix phase order differs'}}
    if($phases[16] -notmatch '^LEA_SUITE_PHASE \d+ 8 begin$'){throw 'Computed phase missing'}
    $stops=@((Get-Content $tested.stdout)|Where-Object {$_ -ceq 'Diagnostic stop after computed-script assertion; not full suite.'}).Count
    $failures=@($lines|Where-Object {$_ -match '^System.InvalidOperationException: FAIL computed script:'})
    if(($tested.passed -and ($stops -ne 1 -or $failures.Count)) -or (!$tested.passed -and ($tested.exitCode -ne 1 -or $stops -ne 0 -or $failures.Count -ne 1))){throw 'Diagnostic terminal behavior differs'}
    if($sampleErrors.Count -or $sampling.overflow -or $samples.Count -lt 3){throw 'Thread observations incomplete'}
    $result.computedPassed=$tested.passed;$result.prefixVerified=$true;$result.earlyStopCount=$stops;$result.computedFailures=$failures;$result.diagnosticCaptured=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@(foreach($file in $result.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){$file.path}})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.sampleOverflow=$sampling.overflow
    $result.diagnosticCaptured=$result.diagnosticCaptured -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaFontThreadInputLeases=$holds}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 10))
    $result|Select-Object diagnosticCaptured,computedPassed,prefixVerified,inputsUnchanged,changedFiles,ownerAttention,error|ConvertTo-Json
}
