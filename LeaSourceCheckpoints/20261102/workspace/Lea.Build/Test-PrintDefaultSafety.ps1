$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$out = Join-Path $root 'continuity-evidence/print-default-safety-v1'
if (Test-Path $out) { throw 'Preserve existing evidence' }
$source = Join-Path $root 'Lea.Tests/PrintPackageLease.cs'
$hold = [IO.File]::Open($source, 'Open', 'Read', 'Read')
try {
    $hash = (Get-FileHash $source).Hash
    Add-Type -Path $source
    $checks = 0
    function Check($condition, $name) { if (!$condition) { throw "Failed: $name" }; $script:checks++ }
    function Reject($action, $name) {
        $denied = $false
        try { & $action } catch { $denied = $true }
        Check $denied $name
    }
    foreach ($mode in @($null, 0, -1, 2, [int]::MaxValue)) {
        $state = [Lea.PrintProbe.PrinterState]::new('fixture', $mode)
        Reject { [Lea.PrintProbe.PrinterSafety]::RequireManualDefault($state) } 'automatic/unknown/invalid mode'
    }
    foreach ($name in @($null, '', ' ')) {
        $state = [Lea.PrintProbe.PrinterState]::new($name, 1)
        Reject { [Lea.PrintProbe.PrinterSafety]::RequireManualDefault($state) } 'missing printer'
    }
    Reject { [Lea.PrintProbe.PrinterSafety]::RequireManualDefault($null) } 'missing snapshot'
    $manual = [Lea.PrintProbe.PrinterState]::new('fixture', 1)
    [Lea.PrintProbe.PrinterSafety]::RequireManualDefault($manual); Check $true 'known manual state allowed'
    Check ([Lea.PrintProbe.PrinterSafety]::Unchanged($manual, [Lea.PrintProbe.PrinterState]::new('fixture', 1))) 'same snapshot'
    Check (![Lea.PrintProbe.PrinterSafety]::Unchanged($manual, [Lea.PrintProbe.PrinterState]::new('other', 1))) 'printer change detected'
    Check (![Lea.PrintProbe.PrinterSafety]::Unchanged($manual, [Lea.PrintProbe.PrinterState]::new('fixture', 0))) 'preference change detected'
    Check (![Lea.PrintProbe.PrinterSafety]::Unchanged($manual, $null)) 'missing final snapshot rejected'
    $before = [Lea.PrintProbe.PrinterSafety]::Capture()
    Check (![string]::IsNullOrWhiteSpace($before.Name)) 'current-user API name captured'
    if ($before.LegacyMode -ne 1) {
        Reject { [Lea.PrintProbe.PrinterSafety]::RequireManualDefault($before) } 'current unsafe preference blocked'
    }
    $after = [Lea.PrintProbe.PrinterSafety]::Capture()
    Check ([Lea.PrintProbe.PrinterSafety]::Unchanged($before, $after)) 'system state unchanged'
    Check ((Get-FileHash $source).Hash -ceq $hash) 'source unchanged'
    $result = [ordered]@{ passed = $true; checks = $checks; before = $before; after = $after;
        sourceSha256 = $hash; printTargetCreated = $false; preferenceWritten = $false;
        scope = 'Pure guard predicate, snapshot comparison and actual read-only current-user API; no native target activation or production host qualification' }
    $null = New-Item -ItemType Directory -Path $out
    Copy-Item $source (Join-Path $out 'PrintPackageLease.cs')
    Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
    [IO.File]::WriteAllText((Join-Path $out 'result.json'), ($result | ConvertTo-Json -Depth 5))
    $result | ConvertTo-Json -Depth 5
} finally { $hold.Dispose() }
