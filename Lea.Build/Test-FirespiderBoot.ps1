param(
    [Alias('r')][string]$Root = 'L:\Firespider Internet',
    [ValidateSet('all', 'clean', 'lea', 'legacy', 'recovery', 'private', 'second', 'limit', 'invalid', 'slow', 'locked', 'menus', 'commands', 'navigation', 'links', 'suspend', 'groups', 'lazy', 'lazy-background', 'lazy-all', 'bulk', 'windows', 'watch', 'domains', 'domain-private', 'split', 'split-life', 'split-limits', 'edit', 'paste', 'policy', 'policy-start', 'failures', 'failure-start', 'hibernate', 'hibernate-start', 'style-start', 'styles', 'load-life', 'pictures', 'decode', 'input', 'cache', 'script')]
    [Alias('c')][string]$Case = 'all',
    [Alias('p')][switch]$Profile,
    [Alias('obs')][string]$Observe = '',
    [ValidateSet('demand', 'script')][Alias('os')][string]$ObserveScope = 'demand'
)

$ErrorActionPreference = 'Stop'
$Case = $Case.ToLowerInvariant()
if ($Observe -eq '' -and $PSBoundParameters.ContainsKey('ObserveScope')) {
    throw 'An observation scope requires a report path.'
}
if ($Observe -ne '') {
    if ($Case -notin @('script', 'all')) { throw 'Observation is supported only for the script case or the complete matrix.' }
    $Observe = [IO.Path]::GetFullPath($Observe)
    if ([IO.Path]::GetFileName($Observe).IndexOfAny([IO.Path]::GetInvalidFileNameChars()) -ge 0) { throw 'Invalid observation report filename.' }
    if (Test-Path -LiteralPath $Observe) { throw 'The observation report must not already exist.' }
    if (-not (Test-Path -LiteralPath (Split-Path -Parent $Observe) -PathType Container)) { throw 'The observation report directory must already exist.' }
}
$rootPath = (Resolve-Path -LiteralPath $Root).Path
if ($Observe -ne '' -and -not (Test-Path -LiteralPath (Join-Path $rootPath 'Lea.Script.Win.dll') -PathType Leaf)) {
    throw 'Worker observation requires a script-enabled host runtime.'
}
# Double trailing backslashes before the closing native argument quote.
$rootArgument = '"' + $rootPath + ('\' * ($rootPath.Length - $rootPath.TrimEnd('\').Length)) + '"'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$source = Join-Path $PSScriptRoot '..\Lea.Firespider\Boot.cs'
$observation = Join-Path $PSScriptRoot 'WorkerObservation.cs'
$exe = Join-Path $rootPath 'Lea.Firespider.Boot.exe'
& $compiler /nologo /target:exe "/out:$exe" /reference:System.dll /reference:System.Core.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /reference:System.Configuration.dll /reference:System.Web.Extensions.dll "/reference:$rootPath\Lea.Win.dll" "/reference:$rootPath\Lea.Engine.dll" $source $observation
if ($LASTEXITCODE -ne 0) { throw 'Firespider boot smoke compilation failed' }
$contract = @(& $exe --clipboard-contract)
$contract
if ($LASTEXITCODE -ne 0) { throw 'Clipboard fixture contract failed' }
if (@($contract | Where-Object { $_ -eq 'PASS Lea clipboard contract: 14 checks; no clipboard access.' }).Count -ne 1) {
    throw 'Clipboard fixture contract exited without completing'
}
$observationContract = @(& $exe --observation-contract)
$observationContract
if ($LASTEXITCODE -ne 0) { throw 'Observation fixture contract failed' }
if (@($observationContract | Where-Object { $_ -eq 'PASS Lea observation contract: 12 checks; no browser workers or clipboard access.' }).Count -ne 1) {
    throw 'Observation fixture contract exited without completing'
}
$cases = if ($Case -eq 'all') { @('clean', 'lea', 'legacy', 'recovery', 'private', 'second', 'limit', 'invalid', 'slow', 'locked', 'menus', 'commands', 'navigation', 'links', 'suspend', 'groups', 'lazy', 'lazy-background', 'lazy-all', 'bulk', 'windows', 'watch', 'domains', 'domain-private', 'split', 'split-life', 'split-limits', 'edit', 'paste', 'policy', 'policy-start', 'failures', 'failure-start', 'hibernate', 'hibernate-start', 'style-start', 'styles', 'load-life', 'pictures', 'decode', 'input', 'cache') } else { @($Case) }
if ($Case -eq 'all' -and (Test-Path -LiteralPath (Join-Path $rootPath 'Lea.Script.Win.dll'))) { $cases += 'script' }
foreach ($name in $cases) {
    $log = Join-Path ([IO.Path]::GetTempPath()) ("lea-boot-$([Guid]::NewGuid().ToString('N'))")
    $process = $null
    $oldProfile = $env:LEA_FIRESPIDER_PROFILE
    try {
        if ($Profile) { $env:LEA_FIRESPIDER_PROFILE = '1' }
        $arguments = @($rootArgument, $name)
        if ($Observe -ne '' -and $name -eq 'script') {
            $arguments += '"' + $Observe + '"'
            if ($ObserveScope -eq 'script') { $arguments += 'script' }
        }
        $process = Start-Process -FilePath $exe -WorkingDirectory $rootPath -ArgumentList $arguments -WindowStyle Hidden -PassThru -RedirectStandardOutput "$log.out" -RedirectStandardError "$log.err"
        # Keep the native handle so Windows PowerShell can read the exit code.
        $null = $process.Handle
        if (-not $process.WaitForExit(60000)) {
            Get-Content -LiteralPath "$log.out", "$log.err"
            throw "Firespider boot case timed out: $name"
        }
        $process.WaitForExit()
        $output = @(Get-Content -LiteralPath "$log.out")
        $output
        Get-Content -LiteralPath "$log.err"
        if ($process.ExitCode -ne 0) { throw "Firespider boot case failed: $name ($($process.ExitCode))" }
        if (@($output | Where-Object { $_.StartsWith("PASS Lea boot: $name;") }).Count -ne 1) {
            throw "Firespider boot case exited without completing: $name"
        }
        if ($name -eq 'decode') {
            $workerLine = @($output | Where-Object { $_ -match '; worker=(\d+)$' })
            if ($workerLine.Count -ne 1) { throw 'Image decode case did not report one worker process' }
            $workerId = [int][regex]::Match($workerLine[0], 'worker=(\d+)$').Groups[1].Value
            $until = [DateTime]::UtcNow.AddSeconds(3)
            do {
                $child = Get-Process -Id $workerId -ErrorAction SilentlyContinue
                if (-not $child -or $child.ProcessName -ne 'Lea.Image') { break }
                Start-Sleep -Milliseconds 25
            } while ([DateTime]::UtcNow -lt $until)
            if ($child -and $child.ProcessName -eq 'Lea.Image') { throw "Image worker remained after host exit: $workerId" }
        }
    }
    finally {
        $env:LEA_FIRESPIDER_PROFILE = $oldProfile
        if ($process -and -not $process.HasExited) { Stop-Process -Id $process.Id -Force }
        if ($process) { $process.Dispose() }
        foreach ($path in @("$log.out", "$log.err")) {
            if (Test-Path -LiteralPath $path) { Remove-Item -LiteralPath $path -Force }
        }
    }
}
