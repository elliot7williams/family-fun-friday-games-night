param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/font-bootstrap-counters-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin owner required'}
if((Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner required'}
if((Get-PSDrive C).Free -lt 600MB){throw 'Storage reserve required'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','Lea.Firespider.Boot','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'No overlapping browser/tests'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Expected){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($stream)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($Expected -and $hash -cne $Expected){throw 'Pinned diagnostic input differs'};$inputs.Add(@{path=$Path;sha256=$hash})
}
$null=New-Item -ItemType Directory -Path $out
$fontSamples=[Collections.Generic.List[object]]::new();$fontSampleErrors=[Collections.Generic.List[string]]::new()
$fontSampling=@{overflow=$false;last=-50L}
$result=@{diagnosticCaptured=$false;fullSuitePassed=$false;limitsChanged=$false;productionChanged=$false;inputs=$inputs;runs=$runs;samples=$fontSamples;sampleErrors=$fontSampleErrors}
function Take-FontOwnedSample([Lea.Build.OwnedTestProcess]$Owner,[long]$Elapsed,[long]$StderrBytes){
    if($Elapsed-$fontSampling.last -lt 50 -or $fontSampleErrors.Count -or $Owner.WaitForExit(0)){return}
    $fontSampling.last=$Elapsed
    if($fontSamples.Count -ge 512){$fontSampling.overflow=$true;return}
    $held=$false;$safe=$null
    try{
        $field=$Owner.GetType().GetField('process',[Reflection.BindingFlags]'Instance,NonPublic')
        if(!$field){throw 'Original process handle field missing'}
        $safe=$field.GetValue($Owner)
        if($safe -isnot [Runtime.InteropServices.SafeHandle] -or $safe.IsClosed -or $safe.IsInvalid){throw 'Original safe handle unavailable'}
        $safe.DangerousAddRef([ref]$held)
        $clock=[Diagnostics.Stopwatch]::StartNew()
        $native=[Lea.Build.ExternalProcessCounters]::Read($safe.DangerousGetHandle())
        $fontSamples.Add(@{elapsedMs=$Elapsed;stderrBytesObserved=$StderrBytes;queryMs=$clock.Elapsed.TotalMilliseconds;native=$native;originalHandleBorrowed=$true;ownerCreationFileTime=$Owner.CreationFileTime})
    }catch{if(!$Owner.WaitForExit(0)){$fontSampleErrors.Add($_.Exception.ToString())}}
    finally{if($held){$safe.DangerousRelease()}}
}
try{
    Hold $PSCommandPath;Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $counter=Join-Path $PSScriptRoot 'ExternalProcessCounters.cs';Hold $counter '128CCB2A4B1E97D6D98B37A6E6A9B70AF7FF15F3DFCBA66CAA274E2D8C95B886';Add-Type -Path $counter
    $calibration=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/external-bootstrap-tools-v1/result.json';Hold $calibration '4E74FEF6F9BBC68FF0419DFA5EF3F472F79DC5D0508887748245B2CB352DA810'
    $calibrated=Get-Content $calibration -Raw|ConvertFrom-Json
    if(!$calibrated.passed -or $calibrated.checks.Count -ne 8){throw 'Counter calibration missing'}
    $original=[IO.File]::ReadAllText($helper)
    $anchor="            if(@(`$readers|Where-Object overflow).Count){throw 'Test output exceeds4MiB guard'}"
    $insert="            Take-FontOwnedSample `$owner `$clock.ElapsedMilliseconds ([long](@(`$readers|Where-Object name -ceq 'stderr'|Measure-Object bytes -Sum).Sum))`r`n"+$anchor
    $edits=@(@('function Invoke-OwnedTestRun {','function Invoke-FontObservedRun {'),@($anchor,$insert))
    $text=$original
    foreach($edit in $edits){if($edit.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($edit[0])).Count -ne 1){throw 'Sampler projection hook ambiguous'};$text=$text.Replace($edit[0],$edit[1])}
    function Valid([string]$Value){for($i=$edits.Count-1;$i -ge 0;$i--){if([regex]::Matches($Value,[regex]::Escape($edits[$i][1])).Count -ne 1){return $false};$Value=$Value.Replace($edits[$i][1],$edits[$i][0])};return $Value -ceq $original}
    if(!(Valid $text)){throw 'Sampler projection inverse differs'}
    foreach($bad in @(($text+"`n"),($text+$text),$text.Replace('5000','5001'))){if($bad -ceq $text -or (Valid $bad)){throw 'Sampler projection negative accepted'}}
    $observed=Join-Path $out 'Invoke-FontObservedRun.ps1';[IO.File]::WriteAllText($observed,$text);Hold $observed;. $observed
    $result.ownerProjectionInverseVerified=$true;$result.ownerProjectionNegativeControls=3
    $base=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/font-script-suite-v1'
    $proof=Join-Path $base 'result.json';Hold $proof 'E87409ED36E23F7C968490DDF206AAEF2D9D990AC9FAED2E144C1712A72EEF7E'
    $build=Get-Content $proof -Raw|ConvertFrom-Json
    if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.sources.Count -ne 60 -or $build.files.Count -ne 65){throw 'Matched suite incomplete'}
    $failure=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/font-script-owned-run-v1/result.json';Hold $failure '60D6C8E23E088C8955B6F914A8C040FA2DB8335D43AF58323B03590EC40EC8EA'
    $runtime=Join-Path $out 'runtime';$null=New-Item -ItemType Directory -Path $runtime
    $known=@{};foreach($file in @($build.inputs)+@($build.files)){$known[$file.path]=$file.sha256}
    foreach($file in $build.files){
        Hold $file.path $file.sha256
        $relative=[IO.Path]::GetRelativePath($build.runtime,$file.path)
        if([IO.Path]::IsPathRooted($relative) -or $relative.Split('\') -contains '..'){throw 'Runtime path escape'}
        if($relative -cin @('Lea.Script.Tests.exe','Lea.Script.Tests.pdb')){continue}
        $target=Join-Path $runtime $relative;$null=New-Item -ItemType Directory -Path (Split-Path $target) -Force;Copy-Item $file.path $target;Hold $target $file.sha256
    }
    $overlay=Join-Path $PSScriptRoot 'New-ExternalBootstrapProgram.ps1';Hold $overlay '4E6F069BA30215881CC27F3930FDD5807AB50649252D30BCCECCD518AAE403A0';. $overlay
    $recipe=Join-Path $base 'tests.rsp';Hold $recipe
    $options=[Collections.Generic.List[string]]::new();$count=0;$changed=0
    foreach($line in Get-Content $recipe){
        if($line.StartsWith('/out:')){$line='/out:"'+(Join-Path $runtime 'Lea.Script.Tests.exe')+'"'}
        elseif($line.StartsWith('/pdb:')){$line='/pdb:"'+(Join-Path $runtime 'Lea.Script.Tests.pdb')+'"'}
        elseif($line -match '^"(.+\.cs)"$'){
            $from=$Matches[1];$relative=[IO.Path]::GetRelativePath((Join-Path $base 'source'),$from);$pin=@($build.sources|Where-Object path -ceq $relative)
            if($pin.Count -ne 1){throw 'Diagnostic source pin missing'};Hold $from $pin[0].sha256
            $target=Join-Path $out ('source/'+$relative);$null=New-Item -ItemType Directory -Path (Split-Path $target) -Force
            if($relative -ceq 'Lea.Script.Tests\Program.cs'){
                $projected=New-ExternalBootstrapProgram ([IO.File]::ReadAllText($from))
                if(!$projected.inverseVerified -or !$projected.assertionPreserved -or !$projected.originalCallPreserved -or $projected.stopCount -ne 1){throw 'Prefix projection invalid'}
                [IO.File]::WriteAllText($target,$projected.text);$changed++
            }else{Copy-Item $from $target}
            Hold $target;$line='"'+$target+'"';$count++
        }elseif($line -match '^/reference:"(.+)"$'){
            $from=$Matches[1];if(!$known.ContainsKey($from)){throw 'Reference provenance missing'};Hold $from $known[$from]
            if($from.StartsWith($build.runtime+'\',[StringComparison]::OrdinalIgnoreCase)){$from=$from.Replace($build.runtime,$runtime)}
            $line='/reference:"'+$from+'"'
        }
        $options.Add($line)
    }
    if($count -ne 60 -or $changed -ne 1){throw 'Diagnostic source coverage differs'}
    $response=Join-Path $out 'compile.rsp';[IO.File]::WriteAllLines($response,$options);Hold $response
    $compiler=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261026.ps1';Hold $compiler 'C3EBB3801FAFB80DA031AAB69FE4837EEB4E4EECD3907F9704F9D6186FA42F0A'
    Hold (Join-Path $PSScriptRoot 'PublicCompiler.cs') 'D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB'
    $compiled=Invoke-OwnedTestRun -Name 'compile-font-prefix' -Application (Get-Process -Id $PID).Path -Arguments @('-NoProfile','-File',('"'+$compiler+'"'),'-ResponseFile',('"'+$response+'"'),'-ExpectedResponseSha256',(Get-FileHash $response).Hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compiled.passed){throw 'Diagnostic compilation failed'}
    $result.before=@(Get-ChildItem $runtime -Recurse -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}})
    if($result.before.Count -ne 65){throw 'Expected65 diagnostic runtime files'}
    foreach($file in $result.before){Hold $file.path $file.sha256}
    $tested=Invoke-FontObservedRun -Name 'font-prefix' -Application (Join-Path $runtime 'Lea.Script.Tests.exe') -Arguments @(('"'+$build.fixtureSource+'"'),'--bootstrap-cost-diagnostic') -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
    if($tested.timedOut -or $tested.killRequested -or !$tested.outputDrained -or !$tested.childRetirementIndependentlyAudited){throw 'Diagnostic lifecycle incomplete'}
    $stderr=@(Get-Content $tested.stderr);$stdout=@(Get-Content $tested.stdout)
    $phases=@($stderr|Where-Object {$_ -match '^LEA_SUITE_PHASE '})
    if($phases.Count -ne 17){throw 'Expected exact0-7 prefix then8begin'}
    for($i=0;$i -lt 8;$i++){if($phases[2*$i] -notmatch ('^LEA_SUITE_PHASE \d+ '+$i+' begin$') -or $phases[2*$i+1] -notmatch ('^LEA_SUITE_PHASE \d+ '+$i+' end$')){throw 'Prefix order differs'}}
    if($phases[16] -notmatch '^LEA_SUITE_PHASE \d+ 8 begin$'){throw 'Computed phase missing'}
    $stops=@($stdout|Where-Object {$_ -ceq 'Diagnostic stop after computed-script assertion; not full suite.'}).Count
    $failures=@($stderr|Where-Object {$_ -match '^System.InvalidOperationException: FAIL computed script:'})
    if(($tested.passed -and ($stops -ne 1 -or $failures.Count)) -or (!$tested.passed -and ($tested.exitCode -ne 1 -or $stops -ne 0 -or $failures.Count -ne 1))){throw 'Diagnostic terminal behavior differs'}
    if($fontSampleErrors.Count -or $fontSampling.overflow -or $fontSamples.Count -lt 3){throw 'External sampling incomplete'}
    $result.computedPassed=$tested.passed;$result.computedFailure=$failures;$result.earlyStopCount=$stops;$result.prefixVerified=$true
    $result.diagnosticCaptured=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@(foreach($file in $result.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){$file.path}})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.sampleOverflow=$fontSampling.overflow
    $result.diagnosticCaptured=$result.diagnosticCaptured -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaFontCounterInputLeases=$holds}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 9))
    $result|Select-Object diagnosticCaptured,computedPassed,prefixVerified,inputsUnchanged,changedFiles,ownerAttention,error|ConvertTo-Json
}
