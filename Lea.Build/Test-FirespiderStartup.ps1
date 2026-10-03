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
using System.Text;
public static class LeaStartupControl {
    private delegate bool EnumProc(IntPtr window, IntPtr state);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SendMessageTimeout(IntPtr window, uint message, IntPtr wParam,
        IntPtr lParam, uint flags, uint timeout, out UIntPtr result);
    [DllImport("user32.dll")]
    private static extern bool EnumChildWindows(IntPtr parent, EnumProc callback, IntPtr state);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr window, StringBuilder name, int count);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr window, StringBuilder text, int count);
    public static ulong Send(int window, uint message) {
        UIntPtr result;
        if (SendMessageTimeout(new IntPtr(window), message, IntPtr.Zero, IntPtr.Zero, 2, 2000, out result) == IntPtr.Zero)
            throw new InvalidOperationException("Startup control did not respond");
        return result.ToUInt64();
    }
    public static int Find(int parent, string classPart, string text) {
        IntPtr found = IntPtr.Zero;
        EnumChildWindows(new IntPtr(parent), delegate(IntPtr window, IntPtr state) {
            var name = new StringBuilder(256);
            GetClassName(window, name, name.Capacity);
            if (name.ToString().IndexOf(classPart, StringComparison.OrdinalIgnoreCase) < 0) return true;
            var value = new StringBuilder(256);
            if (text.Length > 0) GetWindowText(window, value, value.Capacity);
            if (text.Length == 0 || String.Equals(value.ToString(), text, StringComparison.Ordinal)) {
                found = window;
                return false;
            }
            return true;
        }, IntPtr.Zero);
        return found.ToInt32();
    }
}
'@
$rootPath = (Resolve-Path -LiteralPath $Root).Path
$exe = Join-Path $rootPath 'Firespider Internet 19.exe'
if (-not (Test-Path -LiteralPath $exe)) { throw "Missing browser: $exe" }
$imageWorker = Join-Path $rootPath 'Lea.Image.exe'
if (-not (Test-Path -LiteralPath $imageWorker)) { throw "Missing image worker: $imageWorker" }
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
            foreach ($name in @('Firespider Internet 19.exe', 'Firespider Internet 19.exe.config', 'Lea.Engine.dll', 'Lea.Win.dll', 'Lea.Image.exe')) {
                Copy-Item -LiteralPath (Join-Path $rootPath $name) -Destination $runtime
            }
            if (Test-Path -LiteralPath (Join-Path $rootPath 'Lea.Script.Win.dll')) {
                foreach ($name in @('Lea.Script.Win.dll', 'Lea.Script.dll', 'Jint.dll', 'Acornima.dll', 'System.Memory.dll', 'System.Buffers.dll', 'System.Numerics.Vectors.dll', 'System.Runtime.CompilerServices.Unsafe.dll')) {
                    Copy-Item -LiteralPath (Join-Path $rootPath $name) -Destination $runtime
                }
                Copy-Item -LiteralPath (Join-Path $rootPath 'ScriptWorker') -Destination $runtime -Recurse
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
            $mainHandle = $process.MainWindowHandle
            $title = $process.MainWindowTitle
            $lea = $title -match ' - Lea(?: in Firespider)?$'
            if ($mainHandle -ne [IntPtr]::Zero) {
                $tab = [LeaStartupControl]::Find($mainHandle.ToInt32(), 'SysTabControl32', '')
                if ($tab -ne 0) {
                    try { $tabCount = [LeaStartupControl]::Send($tab, 0x1304) } catch { $tabCount = 0 }
                }
                if ($case -eq 'lazy-all') {
                    $resume = [LeaStartupControl]::Find($mainHandle.ToInt32(), 'BUTTON', 'Resume')
                    $deferred = $resume -ne 0
                }
            }
            $names = New-Object 'System.Collections.Generic.List[string]'
            $topNames = New-Object 'System.Collections.Generic.List[string]'
            if ($lea -or $deferred -or $watch.Elapsed.TotalSeconds -ge ($Seconds + 60)) {
                $windows = [System.Windows.Automation.AutomationElement]::RootElement.FindAll(
                    [System.Windows.Automation.TreeScope]::Children, $condition)
                foreach ($window in $windows) {
                    $names.Add($window.Current.Name)
                    $topNames.Add($window.Current.Name)
                    if ($mainHandle -ne [IntPtr]::Zero -and $window.Current.NativeWindowHandle -ne $mainHandle.ToInt32()) {
                        $dialogNodes = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants,
                            [System.Windows.Automation.Condition]::TrueCondition)
                        foreach ($node in $dialogNodes) { $names.Add($node.Current.Name) }
                    }
                }
            }
            $errors = @($names | Where-Object {
                $_ -match 'InvalidCastException|NullReferenceException|Unhandled exception|just-in-time|Unable to cast|An error has occurred in the script'
            })
            $errors += @($topNames | Where-Object { $_ -eq 'Automatic Crash Recovery' })
            if ($errors.Count -gt 0) { throw "$case startup error dialog: $($errors -join ' | ')" }
            $ready = $lea -and ($case -ne 'restored' -or $title -match ' - Lea in Firespider$')
            if ($case -eq 'lazy-background') { $ready = $lea -and $tabCount -eq 2 -and $title -match ' - Lea in Firespider$' }
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
            [void][LeaStartupControl]::Send($resume, 0xF5)
            $resumeWatch = [Diagnostics.Stopwatch]::StartNew()
            do {
                $process.Refresh()
                $resumed = $process.MainWindowTitle -match ' - Lea in Firespider$'
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
