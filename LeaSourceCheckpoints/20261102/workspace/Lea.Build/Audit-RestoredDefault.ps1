$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$path = Join-Path $root 'continuity-evidence/print-package-cancel-v5/default-restoration-readback.json'
if (Test-Path $path) { throw 'Preserve readback evidence' }
Add-Type -TypeDefinition @'
using System.Text;
using System.Runtime.InteropServices;
public static class LeaDefaultPrinterReadback {
    [DllImport("winspool.drv", EntryPoint="GetDefaultPrinterW", CharSet=CharSet.Unicode, SetLastError=true)]
    [return: MarshalAs(UnmanagedType.Bool)] public static extern bool Get(StringBuilder name, ref uint count);
}
'@
$buffer = [Text.StringBuilder]::new(1024); [uint32]$count = 1024
if (![LeaDefaultPrinterReadback]::Get($buffer, [ref]$count)) { throw [ComponentModel.Win32Exception]::new([Runtime.InteropServices.Marshal]::GetLastWin32Error()) }
$registry = Get-ItemProperty 'HKCU:\Software\Microsoft\Windows NT\CurrentVersion\Windows' -Name Device,LegacyDefaultPrinterMode
$cim = @(Get-CimInstance Win32_Printer -Filter 'Default=True' | Select-Object -ExpandProperty Name)
$result = [ordered]@{ apiDefault = $buffer.ToString(); registryDevice = $registry.Device; cimDefault = $cim;
    restoredByApi = $buffer.ToString() -ceq 'HP06D9BB (HP ENVY 4520 series)'; legacyMode = $registry.LegacyDefaultPrinterMode;
    preferenceUnchanged = $registry.LegacyDefaultPrinterMode -eq 0; readOnly = $true; finishedUtc = [DateTime]::UtcNow.ToString('o') }
[IO.File]::WriteAllText($path, ($result | ConvertTo-Json))
$result | ConvertTo-Json
if (!$result.restoredByApi -or !$result.preferenceUnchanged) { throw 'Readback incomplete' }
