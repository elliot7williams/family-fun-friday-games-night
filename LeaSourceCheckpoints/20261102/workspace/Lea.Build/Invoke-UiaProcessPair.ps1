function Invoke-UiaProcessPair([string]$Exe,[string]$Directory,[Collections.Generic.List[object]]$Records,[bool]$Control=$false,[string]$ServerExe='',[bool]$LeaFixture=$false){
    if($global:LeaCompositeProcessLeases.Count -or (Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh original-owner ledger required'}
    $leases=[Collections.Generic.List[object]]::new()
    $nonce=[guid]::NewGuid().ToString('N');$ready=Join-Path $Directory 'server-ready.json';$stop=Join-Path $Directory 'server-stop.txt'
    $clock=[Diagnostics.Stopwatch]::StartNew();$failure=$null;$timedOut=$false;$stopSent=$false
    function Start-PairMember([string]$Name,[string]$Arguments,[string]$Application){
        $row=@{name=$Name;passed=$false;parentBudgetMs=30000;sharedRetirementDrainBudgetMs=5000;originalHandleCaptured=$false;assignedBeforeResume=$false;exitObserved=$false;outputDrained=$false;killRequested=$false;stdout=(Join-Path $Directory ($Name+'.stdout.txt'));stderr=(Join-Path $Directory ($Name+'.stderr.txt'))}
        $lease=@{process=$null;readers=[Collections.Generic.List[object]]::new();record=$row;startReturned=$false}
        $leases.Add($lease);$global:LeaCompositeProcessLeases.Add($lease);$Records.Add($row)
        $owner=[Lea.Build.OwnedTestProcess]::Start($Application,$Arguments,$Directory)
        $lease.process=$owner;$lease.startReturned=$true
        $row.pid=$owner.Id;$row.birth=$owner.CreationFileTime;$row.originalHandleCaptured=$owner.CreationFileTime -gt 0;$row.assignedBeforeResume=$owner.AssignedBeforeResume
        if(!$owner.Started -or !$row.originalHandleCaptured -or !$owner.AssignedBeforeResume -or $owner.StartError){throw 'Atomic original-handle pair launch incomplete'}
        $lease.readers.Add((New-LeaOwnedOutputReader $owner.StandardOutput.BaseStream $row.stdout 'stdout'))
        $lease.readers.Add((New-LeaOwnedOutputReader $owner.StandardError.BaseStream $row.stderr 'stderr'))
        return $lease
    }
    function Pump-Pair {
        foreach($lease in $leases){foreach($reader in $lease.readers){
            for($i=0;$i -lt 8 -and !$reader.done -and !$reader.disabled -and $reader.task.IsCompleted;$i++){Pump-LeaOwnedOutput $reader}
            if($reader.overflow){throw 'Pair capture exceeds original4MiB guard'}
        }}
    }
    try{
        if(!$ServerExe){$ServerExe=$Exe}
        $server=Start-PairMember 'pair-server' ('--server "'+$ready+'" "'+$stop+'" '+$nonce) $ServerExe
        while(!(Test-Path $ready)){
            Pump-Pair
            if($server.process.WaitForExit(0)){throw 'Provider exited before readiness'}
            if($clock.ElapsedMilliseconds -ge 30000){$timedOut=$true;throw 'Pair readiness deadline exceeded'}
            [Threading.Thread]::Sleep(10)
        }
        if((Get-Item $ready).Length -gt 1024){throw 'Readiness record too large'}
        $handshake=Get-Content $ready -Raw|ConvertFrom-Json
        if($handshake.nonce -cne $nonce -or $handshake.pid -ne $server.process.Id -or [long]$handshake.handle -eq 0 -or $server.process.WaitForExit(0)){throw 'Original provider readiness identity differs'}
        if($clock.ElapsedMilliseconds -ge 30000){$timedOut=$true;throw 'Pair deadline exceeded before client launch'}
        $clientArguments='--client '+[long]$handshake.handle+' '+$server.process.Id
        if($Control){$clientArguments+=' --control'}
        if($LeaFixture){$clientArguments+=' --lea'}
        $client=Start-PairMember 'pair-client' $clientArguments $Exe
        if($client.process.Id -eq $server.process.Id){throw 'Client/provider must be separate processes'}
        while($true){
            Pump-Pair
            if($clock.ElapsedMilliseconds -ge 30000){$timedOut=$true;throw 'Original30s pair deadline exceeded'}
            if($client.process.WaitForExit(0) -and !$stopSent){[IO.File]::WriteAllText($stop,$nonce,[Text.UTF8Encoding]::new($false));$stopSent=$true}
            if($stopSent -and $server.process.WaitForExit(0)){break}
            if($clock.ElapsedMilliseconds -ge 30000){$timedOut=$true;throw 'Original30s pair deadline exceeded'}
            [Threading.Thread]::Sleep(10)
        }
    }catch{$failure=$_.Exception.ToString()}
    finally{
        $elapsed=$clock.ElapsedMilliseconds;if($elapsed -gt 30000){$timedOut=$true}
        $retirement=[Diagnostics.Stopwatch]::StartNew()
        # Same ordinary-teardown allowance as the pinned single-process helper,
        # shared across both owners and inside the existing five-second budget.
        while(!$failure -and $retirement.ElapsedMilliseconds -lt 1000){
            $ordinary=@($leases|Where-Object {$_.process -and $_.process.WaitForExit(0) -and $_.process.ExitCode -eq 0 -and $_.process.ActiveProcesses -gt 0})
            if(!$ordinary.Count){break}
            try{Pump-Pair}catch{if(!$failure){$failure=$_.Exception.ToString()}}
            [Threading.Thread]::Sleep(10)
        }
        foreach($lease in $leases){
            try{
                if(!$lease.startReturned -or !$lease.process){throw 'Retain unknown pair launch'}
                $owner=$lease.process;$row=$lease.record
                $row.childRetirementIndependentlyAudited=$owner.TerminateAndWait([int][Math]::Max(0,5000-$retirement.ElapsedMilliseconds))
                $row.activeAfterRetirement=$owner.ActiveProcesses;$row.killRequested=$owner.TerminationRequested
                $row.exitObserved=$owner.WaitForExit(0);if($row.exitObserved){$row.exitCode=$owner.ExitCode}
            }catch{$lease.record.retirementError=$_.Exception.ToString()}
        }
        while($retirement.ElapsedMilliseconds -lt 5000 -and @($leases|ForEach-Object {$_.readers}|Where-Object {!$_.done -and !$_.disabled}).Count){
            try{Pump-Pair}catch{if(!$failure){$failure=$_.Exception.ToString()}}
            [Threading.Thread]::Sleep(10)
        }
        foreach($lease in $leases){
            $row=$lease.record;$row.elapsedMs=$elapsed;$row.timedOut=$timedOut
            $row.outputDrained=$lease.readers.Count -eq 2 -and @($lease.readers|Where-Object {!$_.done -or $_.disabled -or $_.overflow}).Count -eq 0
            $safe=$lease.startReturned -and $lease.process -and $row.exitObserved -and $row.childRetirementIndependentlyAudited -and $row.activeAfterRetirement -eq 0 -and $row.outputDrained -and $retirement.ElapsedMilliseconds -le 5000
            if($safe){
                try{
                    foreach($reader in $lease.readers){$reader.file.Flush($true);$reader.file.Dispose()}
                    if($retirement.ElapsedMilliseconds -gt 5000){throw 'Shared pair retirement/drain deadline exceeded'}
                    $lease.process.Dispose();$null=$global:LeaCompositeProcessLeases.Remove($lease)
                }catch{$safe=$false;$row.retirementError=$_.Exception.ToString()}
            }
            if(!$safe){$global:LeaCompositeUnverifiedChild=$row}
            $row.passed=$safe -and !$failure -and !$timedOut -and !$row.killRequested -and $row.exitCode -eq 0
            $row.retirementAndDrainElapsedMs=$retirement.ElapsedMilliseconds
            [IO.File]::WriteAllText((Join-Path $Directory ($row.name+'.result.json')),($row|ConvertTo-Json -Depth 5))
        }
    }
    $retirementMs=$retirement.ElapsedMilliseconds
    $retirementDeadlineExceeded=$retirementMs -gt 5000
    $passed=$leases.Count -eq 2 -and @($leases|Where-Object {!$_.record.passed}).Count -eq 0 -and $stopSent -and !$retirementDeadlineExceeded
    return @{passed=$passed;error=$failure;elapsedMs=$elapsed;timedOut=$timedOut;sharedRetirementDrainBudgetMs=5000;retirementMs=$retirementMs;retirementDeadlineExceeded=$retirementDeadlineExceeded;stopSent=$stopSent;separateProcesses=$leases.Count -eq 2 -and $leases[0].record.pid -ne $leases[1].record.pid}
}
