param(
    [Parameter(Mandatory=$true)][ValidateSet('Contract','Native')][string]$Phase,
    [Parameter(Mandatory=$true)][string]$Proof,
    [Parameter(Mandatory=$true)][string]$Tool,
    [string]$Root = 'L:\Lea Development',
    [ValidateSet('Snapshot','Suspended')][string]$Mode='Snapshot'
)
$ErrorActionPreference='Stop'
$Proof=(Resolve-Path -LiteralPath $Proof).ProviderPath
$Tool=(Resolve-Path -LiteralPath $Tool).ProviderPath
function Hash([string]$Path){(Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash}
function Save([string]$Name,$Value){
    $bytes=[Text.UTF8Encoding]::new($false).GetBytes((ConvertTo-Json -InputObject $Value -Depth 12))
    $stream=[IO.File]::Open((Join-Path $Proof $Name),[IO.FileMode]::CreateNew,[IO.FileAccess]::Write)
    try{$stream.Write($bytes,0,$bytes.Length)}finally{$stream.Dispose()}
}
function Invoke-Tool([string]$Name,[string[]]$Arguments){
    $started=[DateTime]::UtcNow;$code=$null;$process=$null
    try{
        $quoted=($Arguments | ForEach-Object {'"'+$_+'"'}) -join ' '
        $process=Start-Process -FilePath $Tool -ArgumentList $quoted -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $Proof ($Name+'.log')) -RedirectStandardError (Join-Path $Proof ($Name+'.err'))
        $null=$process.Handle
        if(-not $process.WaitForExit(60000)){throw 'Snapshot helper watchdog'}
        $process.WaitForExit();$code=$process.ExitCode
        return $code
    }finally{
        if($process -and -not $process.HasExited){Stop-Process -Id $process.Id -Force;$process.WaitForExit()}
        if($process){$process.Dispose()}
        Save ($Name+'-result.json') ([pscustomobject]@{startedUtc=$started;finishedUtc=[DateTime]::UtcNow;exitCode=$code;toolSHA256=(Hash $Tool);attempts=1})
    }
}
function Arguments($Target,[string]$Image,[string]$Sha,[long]$Birth,[int]$Parent,[string]$Name){
    $command=if($Mode -eq 'Suspended'){'--suspend'}else{'--sample'}
    @($command,[string]$Target.Id,[string]$Parent,$Image,$Sha,[string]$Birth,(Join-Path $Proof ($Name+'.json')))
}
if($Phase -eq 'Contract'){
    $fixturePath=Join-Path $Proof 'Lea.StackFixture.exe'
    if(Test-Path -LiteralPath $fixturePath){throw 'Use a new contract directory'}
    & 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe' /nologo /target:exe /platform:x64 /optimize+ ('/out:'+$fixturePath) (Join-Path $Root 'Lea.Build\StartupStacks\Fixture.cs') 1> (Join-Path $Proof 'fixture-build.log') 2> (Join-Path $Proof 'fixture-build.err')
    if($LASTEXITCODE -ne 0){throw 'Fixture compilation failed'}
    $fixture=$null;$checks=0
    try{
        $fixture=Start-Process -FilePath $fixturePath -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $Proof 'fixture.log') -RedirectStandardError (Join-Path $Proof 'fixture.err')
        $null=$fixture.Handle
        $watch=[Diagnostics.Stopwatch]::StartNew()
        while(-not ((Get-Content (Join-Path $Proof 'fixture.log') -Raw) -match 'Lea stack fixture ready')){if($fixture.HasExited -or $watch.ElapsedMilliseconds -gt 10000){throw 'Fixture not ready'};Start-Sleep -Milliseconds 50}
        $birth=$fixture.StartTime.ToUniversalTime().ToFileTimeUtc();$sha=Hash $fixturePath
        $cases=@(
            [pscustomobject]@{name='wrong-parent';parent=$PID+1;image=$fixturePath;sha=$sha;birth=$birth;target=$fixture},
            [pscustomobject]@{name='wrong-image';parent=$PID;image=(Join-Path $Proof 'other\Lea.StackFixture.exe');sha=$sha;birth=$birth;target=$fixture},
            [pscustomobject]@{name='wrong-sha';parent=$PID;image=$fixturePath;sha=('0'*64);birth=$birth;target=$fixture},
            [pscustomobject]@{name='wrong-birth';parent=$PID;image=$fixturePath;sha=$sha;birth=$birth+1;target=$fixture},
            [pscustomobject]@{name='wrong-pid';parent=$PID;image=$fixturePath;sha=$sha;birth=$birth;target=(Get-Process -Id $PID)}
        )
        foreach($case in $cases){
            $code=Invoke-Tool $case.name (Arguments $case.target $case.image $case.sha $case.birth $case.parent $case.name)
            $report=Get-Content (Join-Path $Proof ($case.name+'.json')) -Raw | ConvertFrom-Json
            if($code -ne 1 -or $report.passed -or $report.captureAttempted -or $report.captureReady){throw ('Identity rejection failed: '+$case.name)}
            $checks++
        }
        $positive=Arguments $fixture $fixturePath $sha $birth $PID 'fixture-stack'
        if((Invoke-Tool 'fixture-stack' $positive) -ne 0){throw 'Fixture snapshot failed'}
        $report=Get-Content (Join-Path $Proof 'fixture-stack.json') -Raw | ConvertFrom-Json
        if(-not $report.passed -or -not $report.captureReady -or -not $report.captureReleased -or -not @($report.threads.frames | Where-Object {$_.method.Contains('Lea.Diagnostics.StackFixture.WaitForSnapshot')}).Count -or $fixture.HasExited){throw 'Known fixture frame, release or liveness not proved'}
        $checks++
        $before=Hash (Join-Path $Proof 'fixture-stack.json')
        if((Invoke-Tool 'existing-report' $positive) -ne 2 -or (Hash (Join-Path $Proof 'fixture-stack.json')) -ne $before){throw 'Existing snapshot was replaced'}
        $checks++
        if((Invoke-Tool 'invalid-usage' @('--sample','bad')) -ne 2){throw 'Invalid CLI accepted'}
        $checks++
    }finally{
        if($fixture -and -not $fixture.HasExited){Stop-Process -Id $fixture.Id -Force;$fixture.WaitForExit()}
        if($fixture){$fixture.Dispose()}
    }
    Save 'contract-result.json' ([pscustomobject]@{passed=$true;checks=$checks;mode=$Mode;fixtureSHA256=$sha;toolSHA256=(Hash $Tool);fixtureRetired=$true})
    Write-Output ('PASS controlled stack contracts: '+$checks)
}else{
    $sdk=Join-Path $Root 'Lea.Script.Tests\bin\Release\net462';$image=Join-Path $sdk 'ScriptWorker\Lea.Script.Worker.exe';$sha=Hash $image
    $native=$null;$target=$null;$code=$null;$snapshotCode=$null;$reason=$null;$started=[DateTime]::UtcNow
    try{
        $env:TEMP=$env:TMP='C:\Users\Elliot\AppData\Local\Temp'
        $native=Start-Process -FilePath (Join-Path $sdk 'Lea.Script.Tests.exe') -ArgumentList ('"'+$Root+'" --selector-observe "'+(Join-Path $Proof 'native-flow.json')+'"') -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $Proof 'native.log') -RedirectStandardError (Join-Path $Proof 'native.err')
        $null=$native.Handle;$watch=[Diagnostics.Stopwatch]::StartNew()
        while(-not $native.HasExited -and $watch.ElapsedMilliseconds -lt 15000){
            $rows=@(Get-CimInstance Win32_Process -Filter ('ParentProcessId='+$native.Id+" AND Name='Lea.Script.Worker.exe'"))
            if($rows.Count){$target=Get-Process -Id $rows[0].ProcessId;$null=$target.Handle;break}
            Start-Sleep -Milliseconds 50
        }
        if($target){
            $birth=$target.StartTime.ToUniversalTime().ToFileTimeUtc()
            while(-not $target.HasExited -and ([DateTime]::UtcNow-[DateTime]::FromFileTimeUtc($birth)).TotalMilliseconds -lt 2500){Start-Sleep -Milliseconds 25}
            if($target.HasExited){$reason='First worker retired before its planned snapshot age; no replacement sampled.'}
            else{$snapshotCode=Invoke-Tool 'native-stack' (Arguments $target $image $sha $birth $native.Id 'native-stack')}
        }else{$reason='No first worker was observed; no replacement or retry launched.'}
        if(-not $native.WaitForExit(120000)){throw 'Native diagnostic watchdog'}
        $native.WaitForExit();$code=$native.ExitCode
    }finally{
        if($native -and -not $native.HasExited){Stop-Process -Id $native.Id -Force;$native.WaitForExit()}
        if($native){$native.Dispose()};if($target){$target.Dispose()}
        Save 'native-result.json' ([pscustomobject]@{startedUtc=$started;finishedUtc=[DateTime]::UtcNow;exitCode=$code;snapshotExitCode=$snapshotCode;snapshotSkippedReason=$reason;attempts=1;snapshotAgeMs=2500;mode=$Mode;diagnosticOnly=$true;nativeDeadlineChanged=$false;testSHA256=(Hash (Join-Path $sdk 'Lea.Script.Tests.exe'));workerSHA256=$sha})
    }
    Get-Content (Join-Path $Proof 'native.log') -Tail 3
    Get-Content (Join-Path $Proof 'native.err') -Tail 5
}
