param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\combined-trusted-counters-20261101-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/output and original reserve required'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'No test/browser overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new();$runs=[Collections.Generic.List[object]]::new()
$samples=[Collections.Generic.List[object]]::new();$sampleErrors=[Collections.Generic.List[string]]::new();$sampling=@{last=-100L;overflow=$false}
function Hold([string]$path,[string]$pin){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse trusted diagnostic input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($pin -and $pin -cne $hash){throw ('Trusted diagnostic input differs: '+$path)};$inputs.Add(@{path=$path;sha256=$hash})
}
function Proof([string]$path,[string]$hash){Hold $path $hash;return Get-Content -LiteralPath $path -Raw|ConvertFrom-Json}
function Take-CombinedTrustedSample([Lea.Build.OwnedTestProcess]$Owner,[long]$Elapsed,[long]$StderrBytes){
    if($Elapsed-$sampling.last -lt 100 -or $sampleErrors.Count -or $Owner.WaitForExit(0)){return};$sampling.last=$Elapsed
    if($samples.Count -ge 512){$sampling.overflow=$true;return}
    $safe=$null;$held=$false
    try{
        $field=$Owner.GetType().GetField('process',[Reflection.BindingFlags]'Instance,NonPublic')
        if(!$field){throw 'Original owned process handle field missing'};$safe=$field.GetValue($Owner)
        if($safe -isnot [Runtime.InteropServices.SafeHandle] -or $safe.IsInvalid -or $safe.IsClosed){throw 'Original owned safe handle unavailable'}
        $safe.DangerousAddRef([ref]$held);$clock=[Diagnostics.Stopwatch]::StartNew()
        $native=[Lea.Build.ExternalProcessCounters]::Read($safe.DangerousGetHandle())
        $threads=[Lea.Build.OwnedThreadSnapshotV2]::Read($safe.DangerousGetHandle(),$Owner.Id,$Owner.CreationFileTime)
        $samples.Add(@{elapsedMs=$Elapsed;stderrBytesObserved=$StderrBytes;queryMs=$clock.Elapsed.TotalMilliseconds;native=$native;threads=$threads;originalHandleBorrowed=$true;ownerCreationFileTime=$Owner.CreationFileTime})
    }catch{if(!$Owner.WaitForExit(0)){$sampleErrors.Add($_.Exception.ToString())}}
    finally{if($held){$safe.DangerousRelease()}}
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{diagnosticCaptured=$false;fullSuitePassed=$false;productionChanged=$false;limitsChanged=$false;nativeIsolationQualified=$false;runtimeOnExfat=$true;inputs=$inputs;runs=$runs;samples=$samples;sampleErrors=$sampleErrors;originalBefore=@();before=@()}
try{
    Hold $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $counter=Join-Path $PSScriptRoot 'ExternalProcessCounters.cs';Hold $counter '128CCB2A4B1E97D6D98B37A6E6A9B70AF7FF15F3DFCBA66CAA274E2D8C95B886';Add-Type -Path $counter
    $thread=Join-Path $PSScriptRoot 'OwnedThreadSnapshotV2.cs';Hold $thread 'B1904ABBD7E56BC546BBAECFB88113374135825873FBD97222CF31CEA3D30858';Add-Type -Path $thread
    $control=Proof 'I:\LeaBuildStorage\combined-worker-startup-audit-20261101-v1/result.json' '3F5C097B3BB9834979267949F10603121838D7AEE4812BEF28A83721E4CA5ADB'
    if(!$control.verified -or $control.nativeControls -ne 13 -or !$control.samplerInverseVerified -or $control.samplerNegativeControls -ne 3){throw 'Current sampler controls incomplete'}
    $prior=Proof 'I:\LeaBuildStorage\combined-prefix-startup-build-20261101-v1/result.json' '8F3A805C0EB0B21BAEC6892CA704E8A6EED8765BE8D498FF7EAB376BE40076F3'
    if(!$prior.diagnosticCaptured -or !$prior.inputsUnchanged -or $prior.ownerAttention -or $prior.changedFiles.Count -or $prior.before.Count -ne 65 -or $prior.originalBefore.Count -ne 65){throw 'Retired combined source/runtime required'}
    $result.originalBefore=@($prior.before)+@($prior.originalBefore)
    foreach($file in $result.originalBefore){Hold $file.path $file.sha256;if((Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Original NTFS runtime ACL differs'}}
    $known=@{};foreach($file in $prior.inputs){if($known.ContainsKey($file.path) -and $known[$file.path] -cne $file.sha256){throw 'Ambiguous input identity'};$known[$file.path]=$file.sha256}
    $recipe='I:\LeaBuildStorage\combined-prefix-startup-build-20261101-v1/tests.rsp';Hold $recipe '105E0E5F8E81E5833846D8DB3422E03D1C5B8B2139FD1588BC743CBA9784CB1E'
    $projection=Proof 'I:\LeaBuildStorage\combined-prefix-startup-source-20261101-v3/source.json' 'B04B2DEE1525FE6F3A8FF81A572FCA727F62CD3FD6F99262580040118DA2A040'
    $program=@($projection.files|Where-Object {[IO.Path]::GetFileName($_.path) -ceq 'Program.cs'})
    if($program.Count -ne 1){throw 'Unique original Program source required'};Hold $program[0].origin $program[0].originalSha256
    $overlay=Join-Path $PSScriptRoot 'New-ExternalBootstrapProgram.ps1';Hold $overlay '4E6F069BA30215881CC27F3930FDD5807AB50649252D30BCCECCD518AAE403A0';. $overlay
    $original=[IO.File]::ReadAllText($program[0].origin);$projected=New-ExternalBootstrapProgram $original
    if(!$projected.inverseVerified -or !$projected.originalCallPreserved -or !$projected.assertionPreserved -or $projected.stopCount -ne 1){throw 'Unwrapped first-call projection incomplete'}
    foreach($bad in @($original.Replace('var execution = script.Ex(code);','MISSING_CALL;'),$original.Replace('Check(execution.Ok && execution.N == 6,','Check(false,'),($original+$original),$projected.text)){
        $denied=$false;try{$null=New-ExternalBootstrapProgram $bad}catch{$denied=$true};if(!$denied){throw 'First-call projection negative accepted'}
    }
    $source=Join-Path $out 'Program.cs';[IO.File]::WriteAllText($source,$projected.text,[Text.UTF8Encoding]::new($false));Hold $source
    $result.programInverseVerified=$true;$result.programNegativeControls=4;$result.firstCallUnwrapped=$true
    $runtime=Join-Path $out 'runtime';$fixtures=Join-Path $out 'fixtures'
    foreach($file in $prior.before){
        $relative=[IO.Path]::GetRelativePath((Join-Path $root 'continuity-evidence/combined-prefix-startup-runtime-v1/runtime'),$file.path)
        if([IO.Path]::IsPathRooted($relative) -or $relative.Split('\') -contains '..'){throw 'Runtime copy escape'}
        if($relative -in 'Lea.Script.Tests.exe','Lea.Script.Tests.pdb'){continue}
        $to=Join-Path $runtime $relative;New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null;Copy-Item -LiteralPath $file.path -Destination $to;Hold $to $file.sha256
    }
    $oldFixtures=Join-Path $root 'continuity-evidence/combined-prefix-startup-runtime-v1/fixtures'
    $fixtureCount=0
    foreach($path in $known.Keys|Where-Object {$_.StartsWith($oldFixtures+'\',[StringComparison]::OrdinalIgnoreCase)}){
        Hold $path $known[$path];$relative=[IO.Path]::GetRelativePath($oldFixtures,$path);$to=Join-Path $fixtures $relative
        New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null;Copy-Item -LiteralPath $path -Destination $to;Hold $to $known[$path];$fixtureCount++
    }
    if($fixtureCount -ne 182){throw 'Original fixture coverage differs'}
    $options=[Collections.Generic.List[string]]::new();$sources=0;$changed=0
    foreach($line in Get-Content $recipe){
        if($line -match '^"(.+\.cs)"$'){
            $from=$Matches[1];if(!$known.ContainsKey($from)){throw 'Source provenance missing'};Hold $from $known[$from]
            if([IO.Path]::GetFileName($from) -ceq 'Program.cs'){$from=$source;$changed++}
            $options.Add('"'+$from+'"');$sources++;continue
        }
        if($line -match '^/reference:"(.+)"$'){$from=$Matches[1];if(!$known.ContainsKey($from)){throw 'Reference provenance missing'};Hold $from $known[$from];$options.Add($line);continue}
        if($line.StartsWith('/out:')){$options.Add('/out:"'+(Join-Path $runtime 'Lea.Script.Tests.exe')+'"');continue}
        if($line.StartsWith('/pdb:')){$options.Add('/pdb:"'+(Join-Path $runtime 'Lea.Script.Tests.pdb')+'"');continue}
        if(!$line.StartsWith('/')){throw 'Unknown compiler recipe member'};$options.Add($line)
    }
    if($sources -ne 64 -or $changed -ne 1){throw 'Diagnostic source inventory differs'}
    $rsp=Join-Path $out 'compile.rsp';[IO.File]::WriteAllLines($rsp,$options,[Text.UTF8Encoding]::new($false));Hold $rsp
    $dotnet='C:\Program Files\dotnet\dotnet.exe';Hold $dotnet;$tools='C:\Program Files\dotnet\sdk\8.0.427\Roslyn\bincore'
    foreach($name in @('csc.dll','csc.deps.json','csc.runtimeconfig.json','Microsoft.CodeAnalysis.dll','Microsoft.CodeAnalysis.CSharp.dll')){Hold (Join-Path $tools $name)}
    $compiled=Invoke-OwnedTestRun -Name 'compile-trusted-prefix' -Application $dotnet -Arguments @(('"'+(Join-Path $tools 'csc.dll')+'"'),('@"'+$rsp+'"')) -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compiled.passed){throw 'Unwrapped trusted prefix compilation failed'}
    $result.before=@(Get-ChildItem -LiteralPath $runtime -Recurse -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash}})
    if($result.before.Count -ne 65){throw 'Exact65 diagnostic images required'};foreach($file in $result.before){Hold $file.path $file.sha256}
    $text=[IO.File]::ReadAllText($helper)
    $anchor="            if(@(`$readers|Where-Object overflow).Count){throw 'Test output exceeds4MiB guard'}"
    $insert="            Take-CombinedTrustedSample `$owner `$clock.ElapsedMilliseconds ([long](@(`$readers|Where-Object name -ceq 'stderr'|Measure-Object bytes -Sum).Sum))`r`n"+$anchor
    $edits=@(@('function Invoke-OwnedTestRun {','function Invoke-CombinedTrustedRun {'),@($anchor,$insert));$observed=$text
    foreach($edit in $edits){if($edit.Count -ne 2 -or [regex]::Matches($observed,[regex]::Escape($edit[0])).Count -ne 1){throw 'Owner projection hook ambiguous'};$observed=$observed.Replace($edit[0],$edit[1])}
    function Valid-Owner([string]$value){for($i=$edits.Count-1;$i -ge 0;$i--){if([regex]::Matches($value,[regex]::Escape($edits[$i][1])).Count -ne 1){return $false};$value=$value.Replace($edits[$i][1],$edits[$i][0])};return $value -ceq $text}
    if(!(Valid-Owner $observed)){throw 'Owner projection inverse differs'}
    foreach($bad in @(($observed+"`n"),($observed+$observed),$observed.Replace('5000','5001'))){if(Valid-Owner $bad){throw 'Owner projection negative accepted'}}
    $observer=Join-Path $out 'Invoke-CombinedTrustedRun.ps1';[IO.File]::WriteAllText($observer,$observed);Hold $observer;. $observer
    $result.ownerInverseVerified=$true;$result.ownerNegativeControls=3
    $tested=Invoke-CombinedTrustedRun -Name 'trusted-prefix' -Application (Join-Path $runtime 'Lea.Script.Tests.exe') -Arguments @(('"'+$fixtures+'"'),'--bootstrap-cost-diagnostic') -WorkingDirectory $runtime -OutputDirectory $out -Seconds 300 -Records $runs
    if($tested.timedOut -or $tested.killRequested -or !$tested.outputDrained -or !$tested.childRetirementIndependentlyAudited -or $sampleErrors.Count -or $sampling.overflow -or $samples.Count -lt 3){throw 'Trusted observation/retirement incomplete'}
    $stderr=@(Get-Content $tested.stderr);$stdout=@(Get-Content $tested.stdout);$phases=@($stderr|Where-Object {$_ -cmatch '^LEA_SUITE_PHASE '})
    if($phases.Count -ne 17){throw 'Original trusted prefix count differs'}
    for($i=0;$i -lt 8;$i++){if($phases[2*$i] -cnotmatch ('^LEA_SUITE_PHASE [0-9]+ '+$i+' begin$') -or $phases[2*$i+1] -cnotmatch ('^LEA_SUITE_PHASE [0-9]+ '+$i+' end$')){throw 'Original trusted prefix order differs'}}
    if($phases[-1] -cnotmatch '^LEA_SUITE_PHASE [0-9]+ 8 begin$' -or @($stderr|Where-Object {$_ -ceq 'LEA_ALL_STARTUP_TRACE restored=true'}).Count -ne 1){throw 'Computed phase/trace restoration missing'}
    $stops=@($stdout|Where-Object {$_ -ceq 'Diagnostic stop after computed-script assertion; not full suite.'}).Count
    $failures=@($stderr|Where-Object {$_ -cmatch '^System.InvalidOperationException: FAIL computed script:'})
    if(($tested.passed -and ($stops -ne 1 -or $failures.Count)) -or (!$tested.passed -and ($tested.exitCode -ne 1 -or $stops -or $failures.Count -ne 1))){throw 'Computed terminal outcome differs'}
    $result.computedPassed=$tested.passed;$result.computedFailures=$failures;$result.prefixVerified=$true;$result.diagnosticCaptured=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@(foreach($file in $result.originalBefore){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256 -or (Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){$file.path}})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.sampleOverflow=$sampling.overflow
    $result.diagnosticCaptured=$result.diagnosticCaptured -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaCombinedTrustedInputLeases=$holds}
    $result|ConvertTo-Json -Depth 15|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object diagnosticCaptured,computedPassed,prefixVerified,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
