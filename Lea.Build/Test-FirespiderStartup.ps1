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
$rootPath = (Resolve-Path -LiteralPath $Root).Path
$exe = Join-Path $rootPath 'Firespider Internet 19.exe'
if (-not (Test-Path -LiteralPath $exe)) { throw "Missing browser: $exe" }
$cases = if ($Scenario -eq 'all') { @('clean', 'restored', 'locked', 'oversized') } else { @($Scenario) }
$oldSession = $env:LEA_FIRESPIDER_SESSION
foreach ($case in $cases) {
    $session = Join-Path ([IO.Path]::GetTempPath()) ("lea-startup-$([Guid]::NewGuid().ToString('N')).txt")
    $process = $null
    $lock = $null
    try {
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
        $process.Refresh()
        if ($process.HasExited) { throw "$case startup exited early: $($process.ExitCode)" }
        if (-not $process.Responding) { throw "$case startup stopped responding" }
        $condition = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::ProcessIdProperty, $process.Id)
        $windows = [System.Windows.Automation.AutomationElement]::RootElement.FindAll(
            [System.Windows.Automation.TreeScope]::Children, $condition)
        if ($windows.Count -eq 0) { throw "$case startup has no accessible window" }
        $names = New-Object 'System.Collections.Generic.List[string]'
        foreach ($window in $windows) {
            $names.Add($window.Current.Name)
            $nodes = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants,
                [System.Windows.Automation.Condition]::TrueCondition)
            foreach ($node in $nodes) { $names.Add($node.Current.Name) }
        }
        $errors = @($names | Where-Object {
            $_ -match 'InvalidCastException|NullReferenceException|Unhandled exception|just-in-time|Unable to cast|An error has occurred in the script'
        })
        if ($errors.Count -gt 0) { throw "$case startup error dialog: $($errors -join ' | ')" }
        if ($case -eq 'restored' -and -not ($names -contains 'Lea in Firespider')) {
            throw 'Saved Lea tabs did not appear in the executable startup test'
        }
        [PSCustomObject]@{ Scenario = $case; Responsive = $true; Windows = $windows.Count; Title = $process.MainWindowTitle }
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
