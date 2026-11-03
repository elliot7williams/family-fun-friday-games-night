$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$receipt = Join-Path $root 'continuity-evidence/print-package-cancel-v5/default-restoration.json'
if (Test-Path $receipt) { throw 'Preserve restoration receipt' }
$priorPath = Join-Path $root 'continuity-evidence/print-package-cancel-v5/result.json'
if ((Get-FileHash $priorPath).Hash -cne 'F7DDA9DBBBA0AE6375B628942A7EB8EC0D26D9ED7629F93847E42D3F2644270B') { throw 'Original test receipt differs' }
$prior = Get-Content $priorPath -Raw | ConvertFrom-Json
$desired = @($prior.defaultPrintersBefore)
if ($desired.Count -ne 1 -or $desired[0] -cne 'HP06D9BB (HP ENVY 4520 series)') { throw 'Previous default differs' }
$before = @(Get-CimInstance Win32_Printer -Filter 'Default=True' | Select-Object -ExpandProperty Name)
if ($before.Count -ne 1 -or $before[0] -cne 'Microsoft XPS Document Writer') { throw 'Current default changed externally; do not overwrite' }
$modeBefore = (Get-ItemProperty 'HKCU:\Software\Microsoft\Windows NT\CurrentVersion\Windows' -Name LegacyDefaultPrinterMode).LegacyDefaultPrinterMode
if (@(Get-CimInstance Win32_Printer | Where-Object Name -CEQ $desired[0]).Count -ne 1) { throw 'Original printer unavailable' }
Add-Type -TypeDefinition @'
using System.Runtime.InteropServices;
public static class LeaRestorePrinterDefault {
    [DllImport("winspool.drv", EntryPoint="SetDefaultPrinterW", CharSet=CharSet.Unicode, SetLastError=true)]
    [return: MarshalAs(UnmanagedType.Bool)] public static extern bool Set(string name);
}
'@
if (![LeaRestorePrinterDefault]::Set($desired[0])) { throw [ComponentModel.Win32Exception]::new([Runtime.InteropServices.Marshal]::GetLastWin32Error()) }
$after = @(Get-CimInstance Win32_Printer -Filter 'Default=True' | Select-Object -ExpandProperty Name)
$modeAfter = (Get-ItemProperty 'HKCU:\Software\Microsoft\Windows NT\CurrentVersion\Windows' -Name LegacyDefaultPrinterMode).LegacyDefaultPrinterMode
$result = [ordered]@{ restored = $after.Count -eq 1 -and $after[0] -ceq $desired[0]; before = $before; after = $after;
    automaticManagementPreferenceUnchanged = $modeBefore -eq $modeAfter; legacyModeBefore = $modeBefore; legacyModeAfter = $modeAfter;
    printJobSubmitted = $false; userAuthorized = $true; finishedUtc = [DateTime]::UtcNow.ToString('o') }
[IO.File]::WriteAllText($receipt, ($result | ConvertTo-Json))
$result | ConvertTo-Json
if (!$result.restored -or !$result.automaticManagementPreferenceUnchanged) { throw 'Restoration incomplete; inspect receipt' }
