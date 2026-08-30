param(
    [string]$Root = 'L:\Firespider Internet',
    [ValidateSet('all', 'clean', 'lea', 'legacy', 'recovery', 'private', 'second', 'limit', 'invalid', 'slow', 'locked', 'menus', 'commands', 'navigation', 'links', 'suspend', 'groups', 'lazy', 'lazy-background', 'lazy-all', 'bulk', 'windows', 'watch', 'domains', 'domain-private')]
    [string]$Case = 'all',
    [switch]$Profile
)

$ErrorActionPreference = 'Stop'
$Case = $Case.ToLowerInvariant()
$rootPath = (Resolve-Path -LiteralPath $Root).Path
# Double trailing backslashes before the closing native argument quote.
$rootArgument = '"' + $rootPath + ('\' * ($rootPath.Length - $rootPath.TrimEnd('\').Length)) + '"'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$source = Join-Path $PSScriptRoot '..\Lea.Firespider\Boot.cs'
$exe = Join-Path $rootPath 'Lea.Firespider.Boot.exe'
& $compiler /nologo /target:exe "/out:$exe" /reference:System.dll /reference:System.Core.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /reference:System.Configuration.dll "/reference:$rootPath\Lea.Win.dll" "/reference:$rootPath\Lea.Engine.dll" $source
if ($LASTEXITCODE -ne 0) { throw 'Firespider boot smoke compilation failed' }
$cases = if ($Case -eq 'all') { @('clean', 'lea', 'legacy', 'recovery', 'private', 'second', 'limit', 'invalid', 'slow', 'locked', 'menus', 'commands', 'navigation', 'links', 'suspend', 'groups', 'lazy', 'lazy-background', 'lazy-all', 'bulk', 'windows', 'watch', 'domains', 'domain-private') } else { @($Case) }
foreach ($name in $cases) {
    $log = Join-Path ([IO.Path]::GetTempPath()) ("lea-boot-$([Guid]::NewGuid().ToString('N'))")
    $process = $null
    $oldProfile = $env:LEA_FIRESPIDER_PROFILE
    try {
        if ($Profile) { $env:LEA_FIRESPIDER_PROFILE = '1' }
        $process = Start-Process -FilePath $exe -WorkingDirectory $rootPath -ArgumentList @($rootArgument, $name) -WindowStyle Hidden -PassThru -RedirectStandardOutput "$log.out" -RedirectStandardError "$log.err"
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
