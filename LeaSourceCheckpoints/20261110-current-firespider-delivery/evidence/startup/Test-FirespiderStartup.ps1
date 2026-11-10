param(
    [Alias('r')][string]$Root = 'L:\Firespider Internet',
    [ValidateSet('clean', 'restored', 'locked', 'oversized', 'lazy-background', 'lazy-all', 'all')]
    [Alias('s')][string]$Scenario = 'all',
    [Alias('t')][ValidateRange(3, 60)][int]$Seconds = 8,
    [string]$Evidence,
    [switch]$ContractsOnly
)

# Run with Windows PowerShell -STA for the .NET Framework UI Automation client.
$ErrorActionPreference = 'Stop'
function Test-StartupReady([string]$Case, [string]$Title, [int]$Tabs, [bool]$Deferred, [int]$Resume, [int]$Selected = 0) {
    if ($Selected -ne 0) { return $false }
    $fixture = $Title -match ' - Lea in Firespider$'
    $lea = $Title -match ' - Lea(?: in Firespider)?$'
    if ($Case -eq 'lazy-all') { return $Deferred -and $Resume -ne 0 -and -not $lea -and $Tabs -eq 2 }
    $expected = if ($Case -eq 'restored' -or $Case -eq 'lazy-background') { 2 } else { 1 }
    return $fixture -and $Tabs -eq $expected
}
function Set-StartupSettings([xml]$Xml, [string]$Case, [string]$Page) {
    if (-not ([Uri]$Page).IsFile) { throw 'Startup fixture must be a local file URI' }
    $mode = if ($Case.StartsWith('lazy-')) { $Case.Substring(5) } else { 'off' }
    $values = @{
        lazyRestoreMode=$mode; ps=''; rs=''; cleanExit='e'; dashboard=''; h=$Page; nf=$Page;
        backupEnabled=''; frEnabled=''; recoveryEnabled=''; hibernateEnabled=''; watchEnabled='';
        portableMode=''; analyticsEnabled=''; rws=''; t=''; el=''
    }
    foreach ($name in $values.Keys) {
        $setting = $Xml.SelectSingleNode("/configuration/userSettings/Firespider_Internet.My.MySettings/setting[@name='$name']")
        if (-not $setting -or $setting.serializeAs -ne 'String') { throw "Missing string startup setting: $name" }
        $value = $setting.SelectSingleNode('value')
        if (-not $value) { throw "Missing startup value: $name" }
        $value.InnerText = $values[$name]
    }
    # Preserve valid collection serialization while removing saved recovery,
    # groups, identities, automation, styles and URL policies in this copy.
    foreach ($name in @('ls', 'recovery', 'tg', 'au', 'idp', 'us', 'ce', 'fr', 'watchSources', 'watchItems')) {
        $setting = $Xml.SelectSingleNode("/configuration/userSettings/Firespider_Internet.My.MySettings/setting[@name='$name']")
        if (-not $setting -or $setting.serializeAs -ne 'Xml') { throw "Missing startup collection: $name" }
        $array = $setting.SelectSingleNode('value/ArrayOfString')
        if (-not $array) { throw "Missing startup collection: $name" }
        while ($array.HasChildNodes) { [void]$array.RemoveChild($array.FirstChild) }
    }
    return $values
}
function Save-StartupEvidence($Record) {
    if (-not $Evidence) { return }
    $path = Join-Path $Evidence ($Record.Scenario + '.json')
    $bytes = [Text.UTF8Encoding]::new($false).GetBytes((ConvertTo-Json -InputObject $Record -Depth 12))
    $file = [IO.File]::Open($path, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write)
    try { $file.Write($bytes, 0, $bytes.Length) } finally { $file.Dispose() }
}
if ($ContractsOnly) {
    $script:startupChecks = 0
    function Check([bool]$Expected, [bool]$Actual, [string]$Name) {
        if ($Expected -ne $Actual) { throw "Startup contract failed: $Name" }
        $script:startupChecks++
    }
    Check $true (Test-StartupReady restored 'Firespider - Lea in Firespider' 2 $false 0) 'restored two tabs'
    Check $false (Test-StartupReady restored 'Firespider - Lea in Firespider' 1 $false 0) 'restored lost tab'
    Check $false (Test-StartupReady restored 'Firespider - Lea in Firespider' 3 $false 0) 'restored extra tab'
    Check $false (Test-StartupReady restored 'Firespider - Lea' 2 $false 0) 'restored pending document'
    Check $true (Test-StartupReady clean 'Firespider - Lea in Firespider' 1 $false 0) 'clean fixture'
    Check $false (Test-StartupReady clean 'Firespider - Lea' 1 $false 0) 'clean pending document'
    Check $false (Test-StartupReady clean 'Firespider - Lea in Firespider' 0 $false 0) 'clean no tabs'
    Check $false (Test-StartupReady clean 'Firespider - Lea in Firespider' 2 $false 0) 'clean unexpected restore'
    Check $true (Test-StartupReady locked 'Firespider - Lea in Firespider' 1 $false 0) 'locked fallback'
    Check $true (Test-StartupReady oversized 'Firespider - Lea in Firespider' 1 $false 0) 'oversized fallback'
    Check $true (Test-StartupReady lazy-background 'Firespider - Lea in Firespider' 2 $false 0) 'background selected loaded'
    Check $false (Test-StartupReady lazy-background 'Firespider - Lea in Firespider' 1 $false 0) 'background missing tab'
    Check $true (Test-StartupReady lazy-all 'Firespider' 2 $true 42) 'all deferred'
    Check $false (Test-StartupReady lazy-all 'Firespider' 2 $true 0) 'all missing resume'
    Check $false (Test-StartupReady lazy-all 'Firespider' 1 $true 42) 'all missing tab'
    Check $false (Test-StartupReady lazy-all 'Firespider - Lea' 2 $true 42) 'all unexpectedly loaded'
    Check $false (Test-StartupReady restored 'Firespider - Lea in Firespider' 2 $false 0 -1) 'no selection'
    Check $false (Test-StartupReady restored 'Firespider - Lea in Firespider' 2 $false 0 1) 'wrong selection'
    Check $false (Test-StartupReady lazy-all 'Firespider' 2 $true 42 1) 'all wrong selection'
    [xml]$template = Get-Content -LiteralPath (Join-Path $Root 'Firespider Internet 19.exe.config') -Raw
    $originalXml = $template.OuterXml
    [xml]$dirty = $originalXml
    foreach ($name in @('ls', 'recovery', 'tg', 'au', 'idp', 'us', 'ce', 'fr', 'watchSources', 'watchItems')) {
        $array = $dirty.SelectSingleNode("/configuration/userSettings/Firespider_Internet.My.MySettings/setting[@name='$name']/value/ArrayOfString")
        $item = $dirty.CreateElement('string'); $item.InnerText = 'https://must-not-open.invalid/'
        [void]$array.AppendChild($item)
    }
    foreach ($case in @('clean', 'restored', 'lazy-background', 'lazy-all')) {
        [xml]$copy = $dirty.OuterXml
        $values = Set-StartupSettings $copy $case 'file:///C:/fixture/start.html'
        $wantedMode = if ($case.StartsWith('lazy-')) { $case.Substring(5) } else { 'off' }
        Check $true ($values.lazyRestoreMode -eq $wantedMode) ("mode " + $case)
        $wrongValues = @($values.Keys | Where-Object {
            $copy.SelectSingleNode("/configuration/userSettings/Firespider_Internet.My.MySettings/setting[@name='$_']/value").InnerText -cne $values[$_]
        })
        Check $true ($wrongValues.Count -eq 0) ("normalized values " + $case)
        $populated = @($copy.SelectNodes('/configuration/userSettings/Firespider_Internet.My.MySettings/setting[@name="ls" or @name="recovery" or @name="tg" or @name="au" or @name="idp" or @name="us" or @name="ce" or @name="fr" or @name="watchSources" or @name="watchItems"]/value/ArrayOfString/string'))
        Check $true ($populated.Count -eq 0) ("no persisted interference " + $case)
    }
    Check $true ($template.OuterXml -eq $originalXml) 'source settings preserved'
    $rejected = $false
    try { [void](Set-StartupSettings $copy clean 'https://must-not-open.invalid/') } catch { $rejected = $true }
    Check $true $rejected 'nonlocal fixture rejected'
    Write-Output ("PASS Lea startup contract: " + $script:startupChecks + ' checks; no browser launched.')
    return
}
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
    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumProc callback, IntPtr state);
    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr window, out uint pid);
    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr window);
    public static string[] TopWindows(int pid) {
        var result = new System.Collections.Generic.List<string>();
        EnumWindows(delegate(IntPtr window, IntPtr state) {
            uint owner; GetWindowThreadProcessId(window, out owner);
            if (owner != (uint)pid) return true;
            var name = new StringBuilder(256);
            var text = new StringBuilder(1024);
            GetClassName(window, name, name.Capacity);
            GetWindowText(window, text, text.Capacity);
            result.Add(window.ToInt64().ToString() + " | " + name + " | " + text);
            return true;
        }, IntPtr.Zero);
        return result.ToArray();
    }
    public static ulong Send(int window, uint message) {
        UIntPtr result;
        if (SendMessageTimeout(new IntPtr(window), message, IntPtr.Zero, IntPtr.Zero, 2, 2000, out result) == IntPtr.Zero)
            throw new InvalidOperationException("Startup control did not respond");
        return result.ToUInt64();
    }
    public static int Find(int parent, string classPart, string text) {
        return Find(parent, classPart, text, false);
    }
    public static int Find(int parent, string classPart, string text, bool visibleOnly) {
        IntPtr found = IntPtr.Zero;
        EnumChildWindows(new IntPtr(parent), delegate(IntPtr window, IntPtr state) {
            if (visibleOnly && !IsWindowVisible(window)) return true;
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
if ([Reflection.AssemblyName]::GetAssemblyName($exe).GetPublicKeyToken().Length -ne 0) { throw 'Fresh-path startup isolation requires an unsigned browser' }
$sourceConfigHash = (Get-FileHash -LiteralPath "$exe.config" -Algorithm SHA256).Hash
$fixturePath = Join-Path $rootPath 'Lea\start.html'
if (-not (Test-Path -LiteralPath $fixturePath -PathType Leaf)) { throw "Missing start fixture: $fixturePath" }
if ((Get-Content -LiteralPath $fixturePath -Raw) -notmatch '<title>\s*Lea in Firespider\s*</title>') { throw 'Unexpected startup fixture title' }
if ($Evidence) {
    $Evidence = (Resolve-Path -LiteralPath $Evidence).Path
    foreach ($case in $cases) {
        if (Test-Path -LiteralPath (Join-Path $Evidence ($case + '.json'))) { throw "Existing startup evidence: $case" }
    }
}
$oldSession = $env:LEA_FIRESPIDER_SESSION
foreach ($case in $cases) {
    $session = Join-Path ([IO.Path]::GetTempPath()) ("lea-startup-$([Guid]::NewGuid().ToString('N')).txt")
    $process = $null
    $lock = $null
    $runtime = $null
    $exe = $baseExe
    $runRoot = $rootPath
    $failure = $null
    $record = $null
    $snapshot = $null
    $configHash = $null
    $fixtureUris = @()
    $settingsValues = $null
    $forcedCleanup = $false
    $cleanupErrors = New-Object 'System.Collections.Generic.List[string]'
    try {
        if (Get-Process -Name 'Firespider Internet 19' -ErrorAction SilentlyContinue) {
            throw 'Close the existing Firespider process before running executable startup checks'
        }
        $env:LEA_FIRESPIDER_SESSION = $session
        # A new executable path isolates user.config for EVERY scenario;
        # LEA_FIRESPIDER_SESSION alone only isolates the Lea session file.
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
        New-Item -ItemType Directory -Path (Join-Path $runtime 'Lea') | Out-Null
        Copy-Item -LiteralPath $fixturePath -Destination (Join-Path $runtime 'Lea\start.html')
        $page = Join-Path $runtime 'Lea\start.html'
        $settingsValues = Set-StartupSettings $xml $case ([Uri]$page).AbsoluteUri
        $xml.Save("$exe.config")
        $configHash = (Get-FileHash -LiteralPath "$exe.config" -Algorithm SHA256).Hash
        if ($case -eq 'restored' -or $case.StartsWith('lazy-')) {
            $page = Join-Path $runtime 'Lea\start.html'
            if (-not (Test-Path -LiteralPath $page)) { throw "Missing start fixture: $page" }
            $fixtureUris = @(([Uri]$page).AbsoluteUri, ([Uri]$page).AbsoluteUri)
            [IO.File]::WriteAllLines($session, $fixtureUris)
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
            $selected = -1
            $tab = 0
            $resume = 0
            $process.Refresh()
            if ($process.HasExited) { throw "$case startup exited early: $($process.ExitCode)" }
            $mainHandle = $process.MainWindowHandle
            $title = $process.MainWindowTitle
            $lea = $title -match ' - Lea(?: in Firespider)?$'
            if ($mainHandle -ne [IntPtr]::Zero) {
                $tab = [LeaStartupControl]::Find($mainHandle.ToInt32(), 'SysTabControl32', '')
                if ($tab -ne 0) {
                    try {
                        $tabCount = [LeaStartupControl]::Send($tab, 0x1304)
                        $selected = [int][LeaStartupControl]::Send($tab, 0x130B)
                    } catch { $tabCount = 0; $selected = -1 }
                }
                if ($case -eq 'lazy-all') {
                    $resume = [LeaStartupControl]::Find($mainHandle.ToInt32(), 'BUTTON', 'Resume', $true)
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
            $ready = Test-StartupReady $case $title $tabCount $deferred $resume $selected
            $snapshot = [PSCustomObject]@{ ElapsedMs=$watch.ElapsedMilliseconds; Handle=$mainHandle.ToInt64(); Title=$title; Tabs=$tabCount; Selected=$selected; Deferred=$deferred; Resume=$resume; Ready=$ready; TopNames=@($topNames) }
            if ($ready -and $firstReady -lt 0) { $firstReady = $watch.ElapsedMilliseconds }
            $observed = $ready -and $watch.Elapsed.TotalSeconds -ge $Seconds
            if (-not $observed) { Start-Sleep -Milliseconds 250 }
        } while (-not $observed -and $watch.Elapsed.TotalSeconds -lt ($Seconds + 60))
        if (-not $ready) { throw "$case startup did not expose a ready Lea document: title=$title; tabs=$tabCount; handle=$mainHandle; elapsed=$($watch.ElapsedMilliseconds)ms" }
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
                if ($process.HasExited) { throw 'Lazy startup exited during Resume' }
                $resumedTabs = 0
                $resumedSelected = -1
                $resumedTab = [LeaStartupControl]::Find($process.MainWindowHandle.ToInt32(), 'SysTabControl32', '')
                if ($resumedTab -ne 0) {
                    $resumedTabs = [LeaStartupControl]::Send($resumedTab, 0x1304)
                    $resumedSelected = [int][LeaStartupControl]::Send($resumedTab, 0x130B)
                }
                $resumed = Test-StartupReady restored $process.MainWindowTitle $resumedTabs $false 0 $resumedSelected
                if (-not $resumed) { Start-Sleep -Milliseconds 100 }
            } while (-not $resumed -and $resumeWatch.Elapsed.TotalSeconds -lt 15)
            if (-not $resumed) { throw 'Lazy startup Resume did not load Lea' }
            $process.Refresh()
            if (@($process.Modules | Where-Object { $_.ModuleName -ieq 'mshtml.dll' }).Count -gt 0) { throw 'Lazy resume loaded the legacy engine' }
        }
        $record = [PSCustomObject]@{ Passed=$true; Scenario = $case; Responsive = $true; Lea = $lea; Tabs = $tabCount; DeferredSelection = $deferred; Resumed = $resumed; LegacyEngine = $false; ReadyMs = $firstReady; ObservedMs = $watch.ElapsedMilliseconds; Title = $process.MainWindowTitle; Executable=$exe; ConfigSHA256=$configHash; SourceConfigSHA256=$sourceConfigHash; ProfileIsolation='Fresh unsigned executable path; standard settings provider'; Settings=$settingsValues; Session=$session; InitialUris=$fixtureUris; BudgetSeconds=($Seconds + 60); ResumeBudgetSeconds=15; LastObservation=$snapshot; ForcedCleanup=$false }
    }
    catch {
        $failure = $_
        $native = @()
        $probeError = $null
        try { if ($process -and -not $process.HasExited) { $process.Refresh(); $native = @([LeaStartupControl]::TopWindows($process.Id)) } }
        catch { $probeError = $_.Exception.Message }
        $record = [PSCustomObject]@{ Passed=$false; Scenario=$case; Error=$failure.Exception.Message; Executable=$exe; ConfigSHA256=$configHash; SourceConfigSHA256=$sourceConfigHash; ProfileIsolation='Fresh unsigned executable path; standard settings provider'; Settings=$settingsValues; Session=$session; InitialUris=$fixtureUris; BudgetSeconds=($Seconds + 60); ResumeBudgetSeconds=15; LastObservation=$snapshot; NativeWindows=$native; ProbeError=$probeError; ForcedCleanup=$false }
    }
    finally {
        try {
            if ($process -and -not $process.HasExited) {
                if (-not $runtime -or -not [string]::Equals([IO.Path]::GetFullPath($process.Path), [IO.Path]::GetFullPath($exe), [StringComparison]::OrdinalIgnoreCase)) { throw 'Startup cleanup process identity differs' }
                [void]$process.CloseMainWindow()
                if (-not $process.WaitForExit(3000)) {
                    $forcedCleanup = $true
                    Stop-Process -InputObject $process -Force
                    if (-not $process.WaitForExit(3000)) { throw 'Startup test process did not exit' }
                }
            }
        } catch { $cleanupErrors.Add($_.Exception.Message) }
        finally {
            try { if ($lock) { $lock.Dispose() } }
            catch { $cleanupErrors.Add($_.Exception.Message) }
            finally { $env:LEA_FIRESPIDER_SESSION = $oldSession }
        }
        # Retain files if process cleanup failed. Never remove an unchecked path.
        if ($cleanupErrors.Count -eq 0) {
            try {
                if (Test-Path -LiteralPath $session) { Remove-Item -LiteralPath $session -Force }
                if ($runtime) {
                    $resolved = [IO.Path]::GetFullPath($runtime)
                    $temporary = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\') + '\'
                    if (-not $resolved.StartsWith($temporary, [StringComparison]::OrdinalIgnoreCase) -or -not (Split-Path $resolved -Leaf).StartsWith('lea-startup-runtime-')) { throw 'Startup cleanup directory outside owned temporary root' }
                    Remove-Item -LiteralPath $resolved -Recurse -Force
                }
            } catch { $cleanupErrors.Add($_.Exception.Message) }
        }
        if ($process) { $process.Dispose() }
    }
    if ($record) {
        $record.ForcedCleanup = $forcedCleanup
        $record | Add-Member -NotePropertyName CleanupErrors -NotePropertyValue @($cleanupErrors)
        if ($cleanupErrors.Count -gt 0 -or $forcedCleanup) { $record.Passed = $false }
        try { Save-StartupEvidence $record }
        catch {
            if ($failure) {
                Write-Warning ("Startup evidence write also failed: " + $_.Exception.Message)
                throw $failure
            }
            throw
        }
    }
    if ($failure) { throw $failure }
    if ($cleanupErrors.Count -gt 0) { throw ("Startup cleanup failed: " + ($cleanupErrors -join ' | ')) }
    if ($forcedCleanup) { throw 'Startup required forced process cleanup' }
    $record
}
