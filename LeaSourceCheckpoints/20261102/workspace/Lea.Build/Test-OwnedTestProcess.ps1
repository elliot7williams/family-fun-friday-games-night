param([switch]$KeepForegroundHost,[ValidateSet(1,2)][int]$Version=2)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace ('continuity-evidence/owned-test-process-controls-v'+$Version)
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Normal retained NoExit TTY required'}
$identity=[Security.Principal.WindowsIdentity]::GetCurrent()
if(([Security.Principal.WindowsPrincipal]::new($identity)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin required'}
if(Test-Path -LiteralPath $out){throw 'Preserve owned-launch controls'}
if(Get-Variable LeaOwnedLaunchLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
if((Get-PSDrive C).Free -lt 512MB){throw 'Storage reserve required'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','Lea.Firespider.Boot','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap browser/tests'}
$global:LeaOwnedLaunchLeases=[Collections.Generic.List[object]]::new()
$null=New-Item -ItemType Directory -Path $out
$source=Join-Path $PSScriptRoot 'OwnedTestProcess.cs'
$leases=[Collections.Generic.List[IDisposable]]::new()
foreach($path in @($source,$PSCommandPath)){$leases.Add([IO.File]::Open($path,'Open','Read','Read'))}
$pins=@(Get-FileHash $source,$PSCommandPath|Select-Object Path,Hash)
$rows=[Collections.Generic.List[object]]::new()
$result=[ordered]@{passed=$false;controls=$rows;inputs=$pins;limitsChanged=$false;productionChanged=$false;fullSuiteIntegrated=$false;scope='Standalone diagnostic JOB_LIST ownership controls, not engine/full-suite or sandbox qualification'}
function Encoded([string]$body){'-NoProfile -EncodedCommand '+[Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($body))}
function WaitTask($task,$clock,[int]$budget){
    while(!$task.IsCompleted -and $clock.ElapsedMilliseconds -lt $budget){[Threading.Thread]::Sleep(10)}
    if(!$task.IsCompleted){throw 'Controlled fixture readiness/output deadline exceeded'}
    return $task.GetAwaiter().GetResult()
}
function Case([string]$name,[string]$body,[bool]$forced,[int]$minimum,[bool]$fault=$false){
    $owner=$null;$outputTask=$null;$errorTask=$null;$lineTask=$null
    $row=[ordered]@{name=$name;passed=$false;retired=$false;streamsComplete=$false;executionBudgetMs=30000;retirementBudgetMs=5000}
    $clock=[Diagnostics.Stopwatch]::StartNew()
    try{
        $owner=[Lea.Build.OwnedTestProcess]::Start([Environment]::ProcessPath,(Encoded $body),$workspace,$fault)
        $row.assignedBeforeResume=$owner.AssignedBeforeResume
        $row.creationFileTime=$owner.CreationFileTime
        if(!$owner.AssignedBeforeResume -or !$owner.CreationFileTime){throw ('Original process was not retained/assigned: '+$owner.StartError)}
        $errorTask=$owner.StandardError.ReadToEndAsync()
        if($fault){
            if($owner.Started -or !$owner.StartError.Contains('Controlled post-create/pre-resume failure.')){throw 'Fault did not occur before resume'}
            $row.startError=$owner.StartError
            $outputTask=$owner.StandardOutput.ReadToEndAsync()
        }else{
            if(!$owner.Started -or $owner.StartError){throw ('Owned fixture failed to start: '+$owner.StartError)}
            $lineTask=$owner.StandardOutput.ReadLineAsync()
            $line=WaitTask $lineTask $clock 30000
            if($line -cne 'ready'){throw ('Fixture marker differs: '+$line)}
            $outputTask=$owner.StandardOutput.ReadToEndAsync()
        }
        if($name -eq 'parent-exit-child-live'){
            while(!$owner.WaitForExit(10) -and $clock.ElapsedMilliseconds -lt 30000){}
            if(!$owner.WaitForExit(0)){throw 'Parent did not exit under original handle'}
        }
        $row.activeBeforeRetirement=$owner.ActiveProcesses
        if($forced -and $row.activeBeforeRetirement -lt $minimum){throw 'Expected owned descendant count not observed'}
        if($forced){
            $refused=$false
            try{$owner.Dispose()}catch{$refused=$_.Exception.Message.Contains('retirement is unverified')}
            if(!$refused){throw 'Dispose accepted a live owned process tree'}
            $row.liveDisposeRejected=$true
        }else{
            while(!$owner.WaitForExit(10) -and $clock.ElapsedMilliseconds -lt 30000){}
            if(!$owner.WaitForExit(0) -or $clock.ElapsedMilliseconds -ge 30000){throw 'Normal fixture exceeded execution deadline'}
            if($owner.ExitCode -ne 7){throw 'Normal fixture exit code differs'}
        }
        $row.passed=$true
    }catch{$row.error=$_.Exception.ToString()}
    finally{
        $retirement=[Diagnostics.Stopwatch]::StartNew()
        if($owner){
            try{
                $row.retired=$owner.TerminateAndWait(5000)
                if(!$row.retired){throw 'Retain owned launcher: original process/job not retired'}
                $row.activeAfterRetirement=$owner.ActiveProcesses
                $row.originalExitObserved=$owner.WaitForExit(0)
                $row.exitCode=$owner.ExitCode
                $row.terminationRequested=$owner.TerminationRequested
                if(!$outputTask -and $owner.StandardOutput){
                    if($lineTask){$null=WaitTask $lineTask $retirement 5000}
                    $outputTask=$owner.StandardOutput.ReadToEndAsync()
                }
                if($outputTask){$row.stdout=WaitTask $outputTask $retirement 5000}
                if($errorTask){$row.stderr=WaitTask $errorTask $retirement 5000}
                $row.streamsComplete=[bool]($outputTask -and $errorTask -and $outputTask.IsCompleted -and $errorTask.IsCompleted)
                if(!$row.streamsComplete -or $retirement.ElapsedMilliseconds -gt 5000){throw 'Shared retirement/drain deadline exceeded'}
                if($fault -and $row.stdout.Length){throw 'Pre-resume fault executed user code'}
                if($name -eq 'normal-exit' -and ($row.stdout -cne ('normal-output'+[Environment]::NewLine) -or $row.stderr -cne ('normal-error'+[Environment]::NewLine))){throw 'Controlled output bytes differ'}
                $owner.Dispose()
            }catch{$row.retirementError=$_.Exception.ToString();$row.passed=$false;$global:LeaOwnedLaunchLeases.Add($owner)}
        }else{$row.passed=$false}
        $row.elapsedMs=$clock.ElapsedMilliseconds;$row.retirementMs=$retirement.ElapsedMilliseconds
        $rows.Add($row)
        [IO.File]::WriteAllText((Join-Path $out ($name+'.json')),($row|ConvertTo-Json -Depth 5))
    }
    if(!$row.passed -or $global:LeaOwnedLaunchLeases.Count){throw ('Owned-launch fixture failed: '+$name)}
}
try{
    Add-Type -Path $source
    if($Version -eq 2){
        $savedDirectory=[Environment]::CurrentDirectory
        try{
            [Environment]::CurrentDirectory=[IO.Path]::GetPathRoot([Environment]::ProcessPath)
            foreach($kind in @('application','directory')){
                $application=[Environment]::ProcessPath;$directory=$workspace
                if($kind -eq 'application'){$application=$application.Remove(2,1)}else{$directory=$directory.Remove(2,1)}
                if(!(Test-Path -LiteralPath $(if($kind -eq 'application'){[Environment]::ProcessPath}else{$workspace}))){throw 'Absolute control path missing'}
                $rejected=$false
                try{$unexpected=[Lea.Build.OwnedTestProcess]::Start($application,'-NoProfile',$directory)}catch{$rejected=$_.Exception.InnerException -is [ArgumentException]}
                if(!$rejected){if($unexpected){$null=$unexpected.TerminateAndWait(5000);$unexpected.Dispose()};throw 'Drive-relative path accepted'}
                $rows.Add(@{name=('drive-relative-'+$kind);passed=$true;processStarted=$false})
            }
        }finally{[Environment]::CurrentDirectory=$savedDirectory}
    }
    $shell=[Environment]::ProcessPath.Replace("'","''")
    $childArgs=Encoded 'Start-Sleep -Seconds 60'
    $spawn='$p=[Diagnostics.Process]::new();$p.StartInfo=[Diagnostics.ProcessStartInfo]::new('''+$shell+''');$p.StartInfo.Arguments='''+$childArgs+''';$p.StartInfo.UseShellExecute=$false;$p.StartInfo.CreateNoWindow=$true;if(!$p.Start()){throw ''child start failed''};'
    Case 'normal-exit' '[Console]::Out.WriteLine("ready");[Console]::Out.WriteLine("normal-output");[Console]::Error.WriteLine("normal-error");exit 7' $false 0
    Case 'forced-parent' '[Console]::Out.WriteLine("ready");Start-Sleep -Seconds 60' $true 1
    Case 'parent-and-child' ($spawn+'[Console]::Out.WriteLine("ready");Start-Sleep -Seconds 60') $true 2
    Case 'parent-exit-child-live' ($spawn+'[Console]::Out.WriteLine("ready");exit 0') $true 1
    $nested='Add-Type -Path '''+$source.Replace("'","''")+''';$o=[Lea.Build.OwnedTestProcess]::Start('''+$shell+''','''+$childArgs+''','''+$workspace.Replace("'","''")+''');if(!$o.Started){throw $o.StartError};[Console]::Out.WriteLine("ready");Start-Sleep -Seconds 60'
    Case 'nested-job-child' $nested $true 2
    Case 'post-create-fault' '[Console]::Out.WriteLine("MUST-NOT-RUN")' $true 1 $true
    $expected=if($Version -eq 2){8}else{6}
    $result.passed=$rows.Count -eq $expected -and @($rows|Where-Object {!$_.passed}).Count -eq 0
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($pins|Where-Object {(Get-FileHash $_.Path).Hash -cne $_.Hash}).Count -eq 0
    $result.ownerAttention=$global:LeaOwnedLaunchLeases.Count -ne 0
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaOwnedInputLeases=$leases}else{foreach($s in $leases){$s.Dispose()}}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 7))
    [pscustomobject]$result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json -Compress
}
