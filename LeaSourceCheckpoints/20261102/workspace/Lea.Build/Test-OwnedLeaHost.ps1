param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/owned-lea-host-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
$identity=[Security.Principal.WindowsIdentity]::GetCurrent()
if(([Security.Principal.WindowsPrincipal]::new($identity)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin required'}
if(Test-Path -LiteralPath $out){throw 'Preserve owned-host attempt'}
if(Get-Variable LeaOwnedLaunchLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
if((Get-PSDrive C).Free -lt 512MB){throw 'Storage reserve required'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','Lea.Firespider.Boot','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap browser/tests'}
$global:LeaOwnedLaunchLeases=[Collections.Generic.List[object]]::new()
$null=New-Item -ItemType Directory -Path $out
$leases=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$hash){
    $s=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($s)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $hash){throw ('Input differs: '+$path)}
    $inputs.Add(@{path=$path;sha256=$hash})
}
$owner=$null;$stdout=$null;$stderr=$null
$result=[ordered]@{passed=$false;behaviorPassed=$false;retired=$false;streamsComplete=$false;executionBudgetMs=90000;retirementBudgetMs=5000;productionChanged=$false;limitsChanged=$false;fullSuiteIntegrated=$false;scope='Existing exact new638 host scripting subset under atomic owned outer job; not full scripting, new engine build or broad confinement certification'}
try{
    $source=Join-Path $PSScriptRoot 'OwnedTestProcess.cs'
    foreach($path in @($source,$PSCommandPath)){Hold $path (Get-FileHash $path).Hash}
    $controls=Join-Path $workspace 'continuity-evidence/owned-test-process-controls-v2/result.json'
    Hold $controls '7FB338C94345DF9C572F5C0ACDD70283BC9D244F93C6245C6C865A6E9AAA4025'
    $control=Get-Content $controls -Raw|ConvertFrom-Json
    if(!$control.passed -or !$control.inputsUnchanged -or $control.ownerAttention -or $control.controls.Count -ne 8){throw 'Ownership controls incomplete'}
    $sourceHash=@($control.inputs|Where-Object Path -ceq $source|ForEach-Object Hash)
    if($sourceHash.Count -ne 1 -or (Get-FileHash $source).Hash -cne $sourceHash[0]){throw 'Controlled owner source differs'}
    $receipt=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-library-current-native-v1/result.json'
    Hold $receipt 'C2731D13019074BDB7E82353DDE0B1DDC11D47FFF1AA58ABA8D1A157468008FA'
    $hostProof=Get-Content $receipt -Raw|ConvertFrom-Json
    if(!$hostProof.passed -or !$hostProof.originalInputsUnchanged -or !$hostProof.candidateBytesUnchanged -or $hostProof.ownerAttention){throw 'Host identity proof incomplete'}
    $runtime=$hostProof.candidate;$result.candidate=$runtime
    foreach($file in $hostProof.before){Hold $file.path $file.sha256}
    $result.before=@(foreach($file in $hostProof.before){@{path=$file.path;sha256=$file.sha256;sddl=(Get-Acl -LiteralPath $file.path).Sddl}})
    Add-Type -Path $source
    $clock=[Diagnostics.Stopwatch]::StartNew()
    $owner=[Lea.Build.OwnedTestProcess]::Start((Join-Path $runtime 'Lea.Firespider.Boot.exe'),('"'+$runtime+'" script'),$runtime)
    $result.assignedBeforeResume=$owner.AssignedBeforeResume
    $result.creationFileTime=$owner.CreationFileTime
    if(!$owner.Started -or !$owner.AssignedBeforeResume -or $owner.StartError){throw ('Owned host start failed: '+$owner.StartError)}
    $stdout=$owner.StandardOutput.ReadToEndAsync();$stderr=$owner.StandardError.ReadToEndAsync()
    while(!$owner.WaitForExit(50) -and $clock.ElapsedMilliseconds -lt 90000){}
    $result.elapsedMs=$clock.ElapsedMilliseconds
    $result.executionTimedOut=$result.elapsedMs -ge 90000 -or !$owner.WaitForExit(0)
    if($result.executionTimedOut){throw 'Original owned host execution deadline exceeded'}
    $result.exitCode=$owner.ExitCode
    $result.behaviorPassed=$result.exitCode -eq 0
}catch{$result.error=$_.Exception.ToString()}
finally{
    $retirement=[Diagnostics.Stopwatch]::StartNew()
    if($owner){
        try{
            $result.retired=$owner.TerminateAndWait(5000)
            if(!$result.retired){throw 'Original owned process/job retirement not verified'}
            $result.activeAfterRetirement=$owner.ActiveProcesses
            $result.originalExitObserved=$owner.WaitForExit(0)
            $result.terminationRequested=$owner.TerminationRequested
            if(!$stdout -and $owner.StandardOutput){$stdout=$owner.StandardOutput.ReadToEndAsync()}
            if(!$stderr -and $owner.StandardError){$stderr=$owner.StandardError.ReadToEndAsync()}
            while((!$stdout.IsCompleted -or !$stderr.IsCompleted) -and $retirement.ElapsedMilliseconds -lt 5000){[Threading.Thread]::Sleep(10)}
            if(!$stdout.IsCompleted -or !$stderr.IsCompleted -or $retirement.ElapsedMilliseconds -gt 5000){throw 'Shared retirement/drain deadline exceeded'}
            $text=$stdout.GetAwaiter().GetResult();$errorText=$stderr.GetAwaiter().GetResult()
            $result.streamsComplete=$true
            [IO.File]::WriteAllText((Join-Path $out 'host-script.stdout.txt'),$text)
            [IO.File]::WriteAllText((Join-Path $out 'host-script.stderr.txt'),$errorText)
            $result.exactCompletionVerified=@($text -split '\r?\n'|Where-Object {$_.StartsWith('PASS Lea boot: script;')}).Count -eq 1 -and @($text -split '\r?\n'|Where-Object {$_.StartsWith('PASS script host: ')}).Count -eq 1
            $owner.Dispose()
        }catch{$result.retirementError=$_.Exception.ToString();$global:LeaOwnedLaunchLeases.Add($owner)}
    }
    $result.retirementMs=$retirement.ElapsedMilliseconds
    $result.inputs=$inputs
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.byteChanges=@($result.before|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256})
    $result.aclChanges=@($result.before|Where-Object {(Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl})
    $result.ownerAttention=$global:LeaOwnedLaunchLeases.Count -ne 0
    $result.passed=$result.behaviorPassed -and !$result.error -and !$result.retirementError -and $result.retired -and $result.streamsComplete -and $result.exactCompletionVerified -and $result.inputsUnchanged -and !$result.byteChanges.Count -and !$result.aclChanges.Count -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaOwnedInputLeases=$leases}else{foreach($s in $leases){$s.Dispose()}}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 7))
    [pscustomobject]$result|Select-Object passed,behaviorPassed,retired,streamsComplete,ownerAttention,error,retirementError|ConvertTo-Json -Compress
}
