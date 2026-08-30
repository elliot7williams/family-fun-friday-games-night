param(
    [string]$Root = 'L:\Firespider Internet',
    [ValidateSet('clean', 'restored', 'locked', 'oversized', 'all')]
    [string]$Scenario = 'all',
    [ValidateRange(3, 60)][int]$Seconds = 8
)

# Run with Windows PowerShell -STA for the .NET Framework UI Automation client.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Configuration
$rootPath = (Resolve-Path -LiteralPath $Root).Path
$exe = Join-Path $rootPath 'Firespider Internet 19.exe'
if (-not (Test-Path -LiteralPath $exe)) { throw "Missing browser: $exe" }
$map = New-Object System.Configuration.ExeConfigurationFileMap
$map.ExeConfigFilename = "$exe.config"
$config = [System.Configuration.ConfigurationManager]::OpenMappedExeConfiguration($map, [System.Configuration.ConfigurationUserLevel]::None)
$settings = $config.GetSection('userSettings/Firespider_Internet.My.MySettings')
if (-not $settings -or $settings.Settings.Count -eq 0) { throw 'Missing executable settings section' }
$cases = if ($Scenario -eq 'all') { @('clean', 'restored', 'locked', 'oversized') } else { @($Scenario) }
$oldSession = $env:LEA_FIRESPIDER_SESSION
foreach ($case in $cases) {
    $session = Join-Path ([IO.Path]::GetTempPath()) ("lea-startup-$([Guid]::NewGuid().ToString('N')).txt")
    $process = $null
    $lock = $null
    try {
        if (Get-Process -Name 'Firespider Internet 19' -ErrorAction SilentlyContinue) {
            throw 'Close the existing Firespider process before running executable startup checks'
        }
        $env:LEA_FIRESPIDER_SESSION = $session
        if ($case -eq 'restored') {
            $page = Join-Path $rootPath 'Lea\start.html'
            if (-not (Test-Path -LiteralPath $page)) { throw "Missing start fixture: $page" }
            [IO.File]::WriteAllLines($session, @(([Uri]$page).AbsoluteUri, ([Uri]$page).AbsoluteUri))
        } elseif ($case -eq 'locked') {
            $lock = [IO.File]::Open($session, [IO.FileMode]::Create, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
        } elseif ($case -eq 'oversized') {
            $stream = [IO.File]::Create($session)
            try { $stream.SetLength(65537) } finally { $stream.Dispose() }
        }
        $process = Start-Process -FilePath $exe -WorkingDirectory $rootPath -WindowStyle Hidden -PassThru
        Start-Sleep -Seconds $Seconds
        $condition = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::ProcessIdProperty, $process.Id)
        $until = [DateTime]::UtcNow.AddSeconds(60)
        $lea = $false
        do {
            $lea = $false
            $process.Refresh()
            if ($process.HasExited) { throw "$case startup exited early: $($process.ExitCode)" }
            $windows = [System.Windows.Automation.AutomationElement]::RootElement.FindAll(
                [System.Windows.Automation.TreeScope]::Children, $condition)
            $names = New-Object 'System.Collections.Generic.List[string]'
            foreach ($window in $windows) {
                $names.Add($window.Current.Name)
                $nodes = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants,
                    [System.Windows.Automation.Condition]::TrueCondition)
                foreach ($node in $nodes) {
                    $names.Add($node.Current.Name)
                    if ($node.Current.ClassName -eq 'Lea.Win.V') { $lea = $true }
                }
            }
            $errors = @($names | Where-Object {
                $_ -match 'InvalidCastException|NullReferenceException|Unhandled exception|just-in-time|Unable to cast|An error has occurred in the script|Automatic Crash Recovery'
            })
            if ($errors.Count -gt 0) { throw "$case startup error dialog: $($errors -join ' | ')" }
            $ready = $lea -and ($case -ne 'restored' -or $names.Contains('Lea in Firespider'))
            if (-not $ready) { Start-Sleep -Milliseconds 250 }
        } while (-not $ready -and [DateTime]::UtcNow -lt $until)
        if (-not $ready) { throw "$case startup did not expose a ready Lea document" }
        if (-not $process.Responding) { throw "$case startup stopped responding" }
        if (@($process.Modules | Where-Object { $_.ModuleName -ieq 'mshtml.dll' }).Count -gt 0) {
            throw "$case startup loaded the legacy MSHTML engine"
        }
        [PSCustomObject]@{ Scenario = $case; Responsive = $true; Lea = $lea; LegacyEngine = $false; Title = $process.MainWindowTitle }
    }
    finally {
        if ($process -and -not $process.HasExited) {
            [void]$process.CloseMainWindow()
            if (-not $process.WaitForExit(3000)) { Stop-Process -Id $process.Id -Force }
        }
        if ($lock) { $lock.Dispose() }
        $env:LEA_FIRESPIDER_SESSION = $oldSession
        if (Test-Path -LiteralPath $session) { Remove-Item -LiteralPath $session -Force }
    }
}
