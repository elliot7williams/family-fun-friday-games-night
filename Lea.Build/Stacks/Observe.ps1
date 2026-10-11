param(
    [Parameter(Mandatory)][Alias('t')][string]$Test,
    [Parameter(Mandatory)][Alias('f')][string]$FixtureRoot,
    [Parameter(Mandatory)][Alias('i')][string]$Inspector,
    [Parameter(Mandatory)][Alias('o')][string]$Out,
    [Alias('w')][string]$WorkerStageRoot,
    [ValidateRange(500,3000)][Alias('s')][int]$SampleAfterMs = 2000
)

$ErrorActionPreference = 'Stop'
foreach ($path in @($Test,$FixtureRoot,$Inspector,$Out)) {
    if (-not [IO.Path]::IsPathRooted($path)) { throw 'Use absolute paths.' }
}
$Test = [IO.Path]::GetFullPath($Test)
$Inspector = [IO.Path]::GetFullPath($Inspector)
$Out = [IO.Path]::GetFullPath($Out)
if ((Split-Path $Test -Leaf) -ne 'Lea.Script.Tests.exe' -or
    (Split-Path $Inspector -Leaf) -ne 'Lea.Stacks.exe' -or
    -not (Test-Path -LiteralPath $Test -PathType Leaf) -or
    -not (Test-Path -LiteralPath $Inspector -PathType Leaf) -or
    -not (Test-Path -LiteralPath $FixtureRoot -PathType Container) -or
    (Test-Path -LiteralPath $Out)) { throw 'Supply the test/inspector executables, fixture root and a new output directory.' }
$workerPath = Join-Path (Split-Path $Test) 'ScriptWorker\Lea.Script.Worker.exe'
if (-not (Test-Path -LiteralPath $workerPath -PathType Leaf)) { throw 'Worker runtime is missing.' }
if ($WorkerStageRoot) {
    if (-not [IO.Path]::IsPathRooted($WorkerStageRoot) -or -not (Test-Path -LiteralPath $WorkerStageRoot -PathType Container)) { throw 'Worker staging root must be an existing absolute directory.' }
    $WorkerStageRoot = [IO.Path]::GetFullPath($WorkerStageRoot).TrimEnd('\')
}
$runtimeNames = @('Lea.Script.Worker.exe','Lea.Script.Worker.exe.config','Jint.dll','Acornima.dll',
    'System.Memory.dll','System.Buffers.dll','System.Numerics.Vectors.dll','System.Runtime.CompilerServices.Unsafe.dll')
$runtimeHashes = @{}
foreach ($name in $runtimeNames) { $runtimeHashes[$name] = (Get-FileHash -LiteralPath (Join-Path (Split-Path $workerPath) $name) -Algorithm SHA256).Hash }
New-Item -ItemType Directory -Path $Out | Out-Null

function Save($name,$value) {
    $bytes = [Text.UTF8Encoding]::new($false).GetBytes((ConvertTo-Json -InputObject $value -Depth 18))
    $stream = [IO.File]::Open((Join-Path $Out $name),[IO.FileMode]::CreateNew,[IO.FileAccess]::Write)
    try { $stream.Write($bytes,0,$bytes.Length) } finally { $stream.Dispose() }
}
function Hash($path) { (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash }
Save 'plan.json' ([pscustomobject]@{
    test=$Test; testSHA256=(Hash $Test); inspector=$Inspector; inspectorSHA256=(Hash $Inspector)
    worker=$workerPath; workerSHA256=(Hash $workerPath); sampleAfterMs=$SampleAfterMs
    maxSnapshots=32; inspectorWatchdogMs=5000; testWatchdogMs=600000
    startupBudgetMs=5000; externalObserver=$true; externalStartupTrace=$false; fixtureMayEnableTrace=$true; attempts=1
    ageBasis='Enumerated UTC creation time; exact native identity is verified by the inspector.'
    workerStageRoot=$WorkerStageRoot; workerRuntimeHashes=$runtimeHashes
    retries=0; expectedAssertions=2418; releaseVerified=$false
})
Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $Out 'Observe.ps1')
$priorTrace = $env:LEA_SCRIPT_STARTUP_TRACE
$env:LEA_SCRIPT_STARTUP_TRACE = $null
$process = $null
$shots = [Collections.Generic.List[object]]::new()
$seen = @{}
$driverError = $null
$timedOut = $false
$exitCode = $null
$clock = [Diagnostics.Stopwatch]::StartNew()
try {
    $process = Start-Process -FilePath $Test -ArgumentList ('"'+$FixtureRoot+'"') -WorkingDirectory $FixtureRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $Out 'full.log') -RedirectStandardError (Join-Path $Out 'full.err')
    $handle = $process.Handle
    while (-not $process.HasExited -and $clock.ElapsedMilliseconds -lt 600000) {
        if ($shots.Count -lt 32) {
            $children = @(Get-CimInstance Win32_Process -Filter ('ParentProcessId='+$process.Id) | Where-Object {
                $path=$_.ExecutablePath
                $path -and (Split-Path $path -Leaf) -eq 'Lea.Script.Worker.exe' -and
                    ($path -eq $workerPath -or ($WorkerStageRoot -and $path.StartsWith($WorkerStageRoot+'\',[StringComparison]::OrdinalIgnoreCase)))
            })
            foreach ($child in $children) {
                $key = [string]$child.ProcessId+':'+$child.CreationDate.ToUniversalTime().Ticks
                if (-not $seen.ContainsKey($key)) { $seen[$key] = @{beganMs=$clock.ElapsedMilliseconds; sampled=$false} }
                $age = ([DateTime]::UtcNow-$child.CreationDate.ToUniversalTime()).TotalMilliseconds
                if ($seen[$key].sampled -or $age -lt $SampleAfterMs -or $shots.Count -ge 32) { continue }
                $seen[$key].sampled = $true
                $worker = $null
                $reader = $null
                $name = 'stack-'+$shots.Count
                $report = Join-Path $Out ($name+'.json')
                $record = [ordered]@{pid=[int]$child.ProcessId; parentPid=$process.Id; worker=$child.ExecutablePath; runtimeVerified=$false; createdUtcTicks=$null; enumeratedUtcTicks=$child.CreationDate.ToUniversalTime().Ticks; observedMs=$clock.ElapsedMilliseconds; enumeratedAgeMs=$age; ageSinceFirstSeenMs=$clock.ElapsedMilliseconds-$seen[$key].beganMs; lastAssertions=@(Get-Content -LiteralPath (Join-Path $Out 'full.log') -Tail 4); actualExitCode=$null; timedOut=$false; error=$null; report=$report; elapsedMs=0}
                $sampleClock = [Diagnostics.Stopwatch]::StartNew()
                $leases = [Collections.Generic.List[IO.FileStream]]::new()
                try {
                    $directory = Split-Path $child.ExecutablePath
                    for ($folder=[IO.DirectoryInfo]::new($directory);$folder;$folder=$folder.Parent) {
                        if (($folder.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) { throw 'Worker path uses a reparse point.' }
                    }
                    foreach ($runtimeName in $runtimeNames) {
                        $file=Join-Path $directory $runtimeName
                        $info=Get-Item -LiteralPath $file
                        if (($info.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0 -or $info.Length -lt 1 -or $info.Length -gt 16777216) { throw 'Worker file is not a bounded ordinary file.' }
                        $leases.Add([IO.File]::Open($file,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read))
                        if ((Hash $file) -ne $runtimeHashes[$runtimeName]) { throw 'Worker runtime bytes differ from the selected test runtime.' }
                    }
                    $record.runtimeVerified = $true
                    $worker = Get-Process -Id $child.ProcessId -ErrorAction Stop
                    $identityHandle = $worker.Handle
                    $record.createdUtcTicks = $worker.StartTime.ToUniversalTime().Ticks
                    $arguments = '"'+$record.pid+'" "'+$record.parentPid+'" "'+$record.createdUtcTicks+'" "'+$record.worker+'" "'+$report+'"'
                    $reader = Start-Process -FilePath $Inspector -ArgumentList $arguments -WorkingDirectory (Split-Path $Inspector) -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $Out ($name+'.log')) -RedirectStandardError (Join-Path $Out ($name+'.err'))
                    $readerHandle = $reader.Handle
                    $record.timedOut = -not $reader.WaitForExit(5000)
                    if ($record.timedOut) { $reader.Kill() }
                    $reader.WaitForExit()
                    $record.actualExitCode = $reader.ExitCode
                } catch { $record.error = $_.ToString() }
                finally {
                    if ($reader) { if (-not $reader.HasExited) { $reader.Kill(); $reader.WaitForExit() }; $reader.Dispose() }
                    if ($worker) { $worker.Dispose() }
                    foreach ($lease in $leases) { $lease.Dispose() }
                    $record.elapsedMs = $sampleClock.ElapsedMilliseconds
                    $shots.Add([pscustomobject]$record)
                    Save ($name+'-result.json') ([pscustomobject]$record)
                }
            }
        }
        Start-Sleep -Milliseconds 125
    }
    $timedOut = -not $process.HasExited
} catch { $driverError = $_.ToString() }
finally {
    if ($process) {
        if (-not $process.HasExited) { $process.Kill() }
        $process.WaitForExit()
        $exitCode = $process.ExitCode
        $process.Dispose()
    }
    $env:LEA_SCRIPT_STARTUP_TRACE = $priorTrace
}
$lines = @(Get-Content -LiteralPath (Join-Path $Out 'full.log') -ErrorAction SilentlyContinue)
$completion = 'PASS Lea.Script: 2418 checks; JavaScript, isolated worker, and typed DOM.'
$passed = $exitCode -eq 0 -and -not $timedOut -and -not $driverError -and @($lines | Where-Object {$_ -eq $completion}).Count -eq 1
$result = [pscustomobject]@{
    actualExitCode=$exitCode; timedOut=$timedOut; driverError=$driverError; passed=$passed
    passingAssertions=@($lines | Where-Object {$_ -match '^PASS ' -and $_ -ne $completion}).Count
    elapsedMs=$clock.ElapsedMilliseconds; snapshots=$shots.ToArray(); observedWorkers=$seen.Count
    snapshotCapacityReached=($shots.Count -eq 32); externalObserver=$true; externalStartupTrace=$false; fixtureMayEnableTrace=$true
    attempts=1; retries=0; startupFixClaim=$false; releaseVerified=$false
}
Save 'result.json' $result
$result | Select-Object actualExitCode,timedOut,passed,passingAssertions,elapsedMs,observedWorkers,snapshotCapacityReached | ConvertTo-Json
if (-not $passed) { exit 1 }
