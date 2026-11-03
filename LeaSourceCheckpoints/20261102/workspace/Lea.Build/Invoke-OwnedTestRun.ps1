function New-LeaOwnedOutputReader($stream,[string]$path,[string]$name){
    $file=[IO.File]::Open($path,'CreateNew','Write','Read')
    try{
        $buffer=[byte[]]::new(16384)
        @{name=$name;path=$path;stream=$stream;file=$file;buffer=$buffer;task=$stream.ReadAsync($buffer,0,$buffer.Length);done=$false;disabled=$false;overflow=$false;bytes=0L;retainedBytes=0L;error=$null}
    }catch{$file.Dispose();throw}
}
function Pump-LeaOwnedOutput($reader){
    if($reader.done -or $reader.disabled -or !$reader.task.IsCompleted){return}
    try{
        $n=$reader.task.GetAwaiter().GetResult()
        if(!$n){$reader.done=$true;return}
        $reader.bytes+=$n
        if($reader.bytes -gt 4MB){$reader.overflow=$true}
        if(!$reader.overflow){$reader.file.Write($reader.buffer,0,$n);$reader.retainedBytes+=$n}
        $reader.task=$reader.stream.ReadAsync($reader.buffer,0,$reader.buffer.Length)
    }catch{$reader.disabled=$true;$reader.error=$_.Exception.ToString();throw}
}
function Invoke-OwnedTestRun {
    param(
        [Parameter(Mandatory)][ValidatePattern('^[a-zA-Z0-9-]{1,80}$')][string]$Name,
        [Parameter(Mandatory)][string]$Application,
        [Parameter(Mandatory)][AllowEmptyCollection()][string[]]$Arguments,
        [Parameter(Mandatory)][string]$WorkingDirectory,
        [Parameter(Mandatory)][string]$OutputDirectory,
        [Parameter(Mandatory)][ValidateRange(1,300)][int]$Seconds,
        [Collections.Generic.List[object]]$Records
    )
    if(!(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue) -or $global:LeaCompositeProcessLeases.Count -or (Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh verified retained owner ledger required'}
    if(!('Lea.Build.OwnedTestProcess' -as [type])){throw 'Load pinned OwnedTestProcess source first'}
    if(![IO.Path]::IsPathFullyQualified($OutputDirectory) -or !(Test-Path -LiteralPath $OutputDirectory -PathType Container)){throw 'Existing absolute output directory required'}
    if((Get-PSDrive C).Free -lt 512MB){throw 'Storage reserve required'}
    $stdout=Join-Path $OutputDirectory ($Name+'.stdout.txt');$stderr=Join-Path $OutputDirectory ($Name+'.stderr.txt')
    if((Test-Path -LiteralPath $stdout) -or (Test-Path -LiteralPath $stderr)){throw 'Preserve existing process evidence'}
    $owner=$null;$failure=$null;$readers=[Collections.Generic.List[object]]::new()
    $clock=[Diagnostics.Stopwatch]::new()
    $row=@{name=$Name;passed=$false;started=$false;startAttempted=$false;startReturned=$false;startUnknown=$false;originalHandleCaptured=$false;exitObserved=$false;pid=0;birthUtc=$null;exitCode=$null;timedOut=$false;killRequested=$false;stdout=$stdout;stderr=$stderr;stdoutComplete=$false;stderrComplete=$false;outputDrained=$false;captureOverflow=$false;parentBudgetMs=$Seconds*1000;sharedRetirementDrainBudgetMs=5000;childRetirementIndependentlyAudited=$false;retainedForOwner=$false;ownedJob=$true}
    if($null -ne $Records){$Records.Add($row)}
    try{
        if(![IO.Path]::IsPathFullyQualified($Application) -or ![IO.File]::Exists($Application) -or ![IO.Path]::IsPathFullyQualified($WorkingDirectory) -or ![IO.Directory]::Exists($WorkingDirectory)){throw 'Invalid fully qualified launch paths'}
        $clock.Start();$row.startAttempted=$true
        $owner=[Lea.Build.OwnedTestProcess]::Start($Application,([string]::Join(' ',$Arguments)),$WorkingDirectory)
        $row.startReturned=$true;$row.pid=$owner.Id
        $row.originalHandleCaptured=$owner.CreationFileTime -gt 0
        $row.started=$row.originalHandleCaptured
        if($row.originalHandleCaptured){$row.birthUtc=[datetime]::FromFileTimeUtc($owner.CreationFileTime).ToString('o')}
        if(!$owner.Started -or !$owner.AssignedBeforeResume -or $owner.StartError){throw ('Owned launch failed: '+$owner.StartError)}
        $readers.Add((New-LeaOwnedOutputReader $owner.StandardOutput.BaseStream $stdout 'stdout'))
        $readers.Add((New-LeaOwnedOutputReader $owner.StandardError.BaseStream $stderr 'stderr'))
        while($true){
            foreach($reader in $readers){
                for($round=0;$round -lt 8 -and !$reader.done -and !$reader.disabled -and $reader.task.IsCompleted;$round++){Pump-LeaOwnedOutput $reader}
            }
            if(@($readers|Where-Object overflow).Count){throw 'Test output exceeds4MiB guard'}
            $remaining=$Seconds*1000-$clock.ElapsedMilliseconds
            if($remaining -le 0){$row.timedOut=$true;throw 'Original process deadline exceeded'}
            if($owner.WaitForExit([int][Math]::Min(10,$remaining))){$row.exitObserved=$true;$row.exitCode=$owner.ExitCode;break}
        }
        $row.elapsedMs=$clock.ElapsedMilliseconds
        if($row.elapsedMs -gt $Seconds*1000){$row.timedOut=$true;throw 'Original process completed after deadline'}
    }catch{$failure=$_.Exception}
    finally{
        $retirement=[Diagnostics.Stopwatch]::StartNew()
        $row.elapsedMs=$clock.ElapsedMilliseconds
        if($row.startAttempted -and !$row.startReturned){$row.startUnknown=$true}
        if($owner){
            foreach($streamName in @('stdout','stderr')){
                if(@($readers|Where-Object name -ceq $streamName).Count -eq 0){
                    try{
                        $stream=if($streamName -eq 'stdout'){$owner.StandardOutput}else{$owner.StandardError}
                        if($stream){$readers.Add((New-LeaOwnedOutputReader $stream.BaseStream $(if($streamName -eq 'stdout'){$stdout}else{$stderr}) $streamName))}
                    }catch{if(!$failure){$failure=$_.Exception}}
                }
            }
            try{
                # Allow ordinary console/worker teardown within, not after, the shared5s.
                if(!$failure -and $row.exitObserved -and $row.exitCode -eq 0){
                    while($owner.ActiveProcesses -gt 0 -and $retirement.ElapsedMilliseconds -lt 1000){
                        foreach($reader in $readers){Pump-LeaOwnedOutput $reader}
                        [Threading.Thread]::Sleep(10)
                    }
                }
                $remaining=[int][Math]::Max(0,5000-$retirement.ElapsedMilliseconds)
                $row.childRetirementIndependentlyAudited=$owner.TerminateAndWait($remaining)
                $row.activeAfterRetirement=$owner.ActiveProcesses
                $row.killRequested=$owner.TerminationRequested
                $row.exitObserved=$row.started -and $owner.WaitForExit(0)
                if($row.exitObserved){$row.exitCode=$owner.ExitCode}
            }catch{$row.retirementError=$_.Exception.ToString()}
            while($retirement.ElapsedMilliseconds -lt 5000 -and @($readers|Where-Object {!$_.done -and !$_.disabled}).Count){
                foreach($reader in $readers){try{Pump-LeaOwnedOutput $reader}catch{if(!$failure){$failure=$_.Exception}}}
                [Threading.Thread]::Sleep(10)
            }
        }
        $row.outputDrained=@($readers|Where-Object {!$_.done -or $_.disabled}).Count -eq 0
        $row.captureOverflow=@($readers|Where-Object overflow).Count -ne 0
        foreach($streamName in @('stdout','stderr')){
            $selected=@($readers|Where-Object name -ceq $streamName)
            $row[$streamName+'Complete']=$selected.Count -eq 1 -and $selected[0].done -and !$selected[0].disabled -and !$selected[0].overflow
        }
        $row.retainedForOwner=$row.startUnknown -or ($owner -and (!$row.childRetirementIndependentlyAudited -or !$row.outputDrained -or ($row.started -and !$row.exitObserved)))
        if(!$row.retainedForOwner){
            try{
                foreach($reader in $readers){$reader.file.Flush($true);$reader.file.Dispose()}
                if($retirement.ElapsedMilliseconds -gt 5000){throw 'Shared retirement/drain deadline exceeded'}
                if($owner){$owner.Dispose()}
            }catch{$row.retainedForOwner=$true;$row.retirementError=$_.Exception.ToString()}
        }
        foreach($reader in $readers){$row[$reader.name+'Bytes']=$reader.bytes;$row[$reader.name+'RetainedBytes']=$reader.retainedBytes;if($reader.error){$row[$reader.name+'Error']=$reader.error}}
        $row.retirementAndDrainElapsedMs=$retirement.ElapsedMilliseconds
        if($row.retainedForOwner){
            $global:LeaCompositeProcessLeases.Add(@{process=$owner;readers=$readers;record=$row;kind='owned-job'})
            $global:LeaCompositeUnverifiedChild=$row
        }
        $row.error=if($failure){$failure.ToString()}else{$null}
        $row.unsafeToContinue=$row.retainedForOwner
        $row.passed=!$failure -and !$row.timedOut -and !$row.killRequested -and !$row.retainedForOwner -and $row.exitObserved -and $row.exitCode -eq 0 -and $row.stdoutComplete -and $row.stderrComplete -and $row.childRetirementIndependentlyAudited
        $row.finishedUtc=[datetime]::UtcNow.ToString('o')
        [IO.File]::WriteAllText((Join-Path $OutputDirectory ($Name+'.result.json')),($row|ConvertTo-Json -Depth 6))
    }
    if($row.unsafeToContinue){throw ('Retain original owned-job foreground owner: '+$Name)}
    return $row
}
