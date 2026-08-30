param(
    [string]$Root = 'L:\Firespider Internet',
    [ValidateSet('clean', 'restored', 'locked', 'oversized', 'lazy-background', 'lazy-all', 'all')]
    [string]$Scenario = 'all',
    [ValidateRange(3, 60)][int]$Seconds = 8
)

# Run with Windows PowerShell -STA for the .NET Framework UI Automation client.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Configuration
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class LeaStartupControl {
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SendMessageTimeout(IntPtr window, uint message, IntPtr wParam,
        IntPtr lParam, uint flags, uint timeout, out UIntPtr result);
    public static ulong Send(int window, uint message) {
        UIntPtr result;
        if (SendMessageTimeout(new IntPtr(window), message, IntPtr.Zero, IntPtr.Zero, 2, 2000, out result) == IntPtr.Zero)
            throw new InvalidOperationException("Startup control did not respond");
        return result.ToUInt64();
    }
}
'@
$rootPath = (Resolve-Path -LiteralPath $Root).Path
$exe = Join-Path $rootPath 'Firespider Internet 19.exe'
if (-not (Test-Path -LiteralPath $exe)) { throw "Missing browser: $exe" }
$map = New-Object System.Configuration.ExeConfigurationFileMap
$map.ExeConfigFilename = "$exe.config"
$config = [System.Configuration.ConfigurationManager]::OpenMappedExeConfiguration($map, [System.Configuration.ConfigurationUserLevel]::None)
$settings = $config.GetSection('userSettings/Firespider_Internet.My.MySettings')
if (-not $settings -or $settings.Settings.Count -eq 0) { throw 'Missing executable settings section' }
$cases = if ($Scenario -eq 'all') { @('clean', 'restored', 'locked', 'oversized', 'lazy-background', 'lazy-all') } else { @($Scenario) }
$baseExe = $exe
$oldSession = $env:LEA_FIRESPIDER_SESSION
foreach ($case in $cases) {
    $session = Join-Path ([IO.Path]::GetTempPath()) ("lea-startup-$([Guid]::NewGuid().ToString('N')).txt")
    $process = $null
    $lock = $null
    $runtime = $null
    $exe = $baseExe
    $runRoot = $rootPath
    try {
        if (Get-Process -Name 'Firespider Internet 19' -ErrorAction SilentlyContinue) {
            throw 'Close the existing Firespider process before running executable startup checks'
        }
        $env:LEA_FIRESPIDER_SESSION = $session
        if ($case.StartsWith('lazy-')) {
            # A fresh executable path isolates these default settings from the user's profile.
            $runtime = Join-Path ([IO.Path]::GetTempPath()) ("lea-startup-runtime-$([Guid]::NewGuid().ToString('N'))")
            New-Item -ItemType Directory -Path $runtime | Out-Null
            foreach ($name in @('Firespider Internet 19.exe', 'Firespider Internet 19.exe.config', 'Lea.Engine.dll', 'Lea.Win.dll')) {
                Copy-Item -LiteralPath (Join-Path $rootPath $name) -Destination $runtime
            }
            $exe = Join-Path $runtime 'Firespider Internet 19.exe'
            $runRoot = $runtime
            $xml = New-Object Xml.XmlDocument
            $xml.Load("$exe.config")
            $values = @{ lazyRestoreMode=$case.Substring(5); ps=''; rs=''; cleanExit='e'; dashboard=''; backupEnabled=''; frEnabled=''; recoveryEnabled=''; hibernateEnabled=''; watchEnabled=''; portableMode='' }
            foreach ($name in $values.Keys) {
                $value = $xml.SelectSingleNode("/configuration/userSettings/Firespider_Internet.My.MySettings/setting[@name='$name']/value")
                if (-not $value) { throw "Missing lazy startup setting: $name" }
                $value.InnerText = $values[$name]
            }
            $xml.Save("$exe.config")
        }
        if ($case -eq 'restored' -or $case.StartsWith('lazy-')) {
            $page = Join-Path $rootPath 'Lea\start.html'
            if (-not (Test-Path -LiteralPath $page)) { throw "Missing start fixture: $page" }
            [IO.File]::WriteAllLines($session, @(([Uri]$page).AbsoluteUri, ([Uri]$page).AbsoluteUri))
        } elseif ($case -eq 'locked') {
            $lock = [IO.File]::Open($session, [IO.FileMode]::Create, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
        } elseif ($case -eq 'oversized') {
            $stream = [IO.File]::Create($session)
            try { $stream.SetLength(65537) } finally { $stream.Dispose() }
        }
        $watch = [Diagnostics.Stopwatch]::StartNew()
        $process = Start-Process -FilePath $exe -WorkingDirectory $runRoot -WindowStyle Hidden -PassThru
        $condition = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::ProcessIdProperty, $process.Id)
        $firstReady = -1L
        $lea = $false
        do {
            $lea = $false
            $deferred = $false
            $tabCount = 0
            $resume = $null
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
                    if ($node.Current.Name.StartsWith('[Suspended]')) { $deferred = $true }
                    # This legacy WinForms runtime exposes standard controls as native panes.
                    if ($node.Current.ClassName -like 'WindowsForms10.SysTabControl32.*') {
                        $tabCount = [LeaStartupControl]::Send($node.Current.NativeWindowHandle, 0x1304)
                    }
                    if ($node.Current.ClassName -like 'WindowsForms10.BUTTON.*' -and $node.Current.Name -eq 'Resume') { $resume = $node }
                }
            }
            $errors = @($names | Where-Object {
                $_ -match 'InvalidCastException|NullReferenceException|Unhandled exception|just-in-time|Unable to cast|An error has occurred in the script|Automatic Crash Recovery'
            })
            if ($errors.Count -gt 0) { throw "$case startup error dialog: $($errors -join ' | ')" }
            $ready = $lea -and ($case -ne 'restored' -or $names.Contains('Lea in Firespider'))
            if ($case -eq 'lazy-background') { $ready = $lea -and $tabCount -eq 2 -and $names.Contains('Lea in Firespider') }
            if ($case -eq 'lazy-all') { $ready = $deferred -and $resume -and -not $lea -and $tabCount -eq 2 }
            if ($ready -and $firstReady -lt 0) { $firstReady = $watch.ElapsedMilliseconds }
            $observed = $ready -and $watch.Elapsed.TotalSeconds -ge $Seconds
            if (-not $observed) { Start-Sleep -Milliseconds 250 }
        } while (-not $observed -and $watch.Elapsed.TotalSeconds -lt ($Seconds + 60))
        if (-not $ready) { throw "$case startup did not expose a ready Lea document" }
        if (-not $process.Responding) { throw "$case startup stopped responding" }
        if (@($process.Modules | Where-Object { $_.ModuleName -ieq 'mshtml.dll' }).Count -gt 0) {
            throw "$case startup loaded the legacy MSHTML engine"
        }
        $resumed = $false
        if ($case -eq 'lazy-all') {
            if (-not $resume) { throw 'Lazy startup did not expose a Resume button' }
            [void][LeaStartupControl]::Send($resume.Current.NativeWindowHandle, 0xF5)
            $resumeWatch = [Diagnostics.Stopwatch]::StartNew()
            do {
                $windows = [System.Windows.Automation.AutomationElement]::RootElement.FindAll([System.Windows.Automation.TreeScope]::Children, $condition)
                foreach ($window in $windows) {
                    $nodes = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
                    foreach ($node in $nodes) {
                        if ($node.Current.ClassName -eq 'Lea.Win.V' -and $node.Current.Name -eq 'Lea in Firespider') { $resumed = $true }
                    }
                }
                if (-not $resumed) { Start-Sleep -Milliseconds 100 }
            } while (-not $resumed -and $resumeWatch.Elapsed.TotalSeconds -lt 15)
            if (-not $resumed) { throw 'Lazy startup Resume did not load Lea' }
            $process.Refresh()
            if (@($process.Modules | Where-Object { $_.ModuleName -ieq 'mshtml.dll' }).Count -gt 0) { throw 'Lazy resume loaded the legacy engine' }
        }
        [PSCustomObject]@{ Scenario = $case; Responsive = $true; Lea = $lea; Tabs = $tabCount; DeferredSelection = $deferred; Resumed = $resumed; LegacyEngine = $false; ReadyMs = $firstReady; ObservedMs = $watch.ElapsedMilliseconds; Title = $process.MainWindowTitle }
    }
    finally {
        if ($process -and -not $process.HasExited) {
            [void]$process.CloseMainWindow()
            if (-not $process.WaitForExit(3000)) {
                Stop-Process -Id $process.Id -Force
                if (-not $process.WaitForExit(3000)) { throw 'Startup test process did not exit' }
            }
        }
        if ($lock) { $lock.Dispose() }
        $env:LEA_FIRESPIDER_SESSION = $oldSession
        if (Test-Path -LiteralPath $session) { Remove-Item -LiteralPath $session -Force }
        if ($runtime) {
            $resolved = [IO.Path]::GetFullPath($runtime)
            $temporary = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
            if ($resolved.StartsWith($temporary, [StringComparison]::OrdinalIgnoreCase) -and (Split-Path $resolved -Leaf).StartsWith('lea-startup-runtime-')) {
                Remove-Item -LiteralPath $resolved -Recurse -Force
            }
        }
    }
}
