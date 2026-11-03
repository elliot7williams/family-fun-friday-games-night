param([ValidateSet('Analyze','Cleanup')][string]$Mode = 'Analyze')
$ErrorActionPreference = 'Stop'
$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) { throw 'Administrator approval is required.' }
$root = Split-Path $PSScriptRoot -Parent
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss-fff'
$directory = Join-Path $root "continuity-evidence/windows-component-$($Mode.ToLowerInvariant())-$stamp"
New-Item -ItemType Directory -Path $directory -ErrorAction Stop | Out-Null
$before = (Get-PSDrive C).Free
$arguments = @('/Online','/Cleanup-Image','/English','/NoRestart')
if ($Mode -eq 'Analyze') { $arguments += '/AnalyzeComponentStore' }
else {
    if ((Test-Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending') -or
        (Test-Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired')) {
        throw 'A reboot is pending; no cleanup was started.'
    }
    $arguments += '/StartComponentCleanup'
}
& "$env:windir\System32\dism.exe" @arguments 2>&1 | Out-File (Join-Path $directory 'dism-output.txt') -Encoding utf8
$code = $LASTEXITCODE
[ordered]@{ mode=$Mode; arguments=$arguments; exitCode=$code; freeBefore=$before;
    freeAfter=(Get-PSDrive C).Free; finishedUtc=[DateTime]::UtcNow.ToString('o');
    resetBase=$false; restartRequested=$false } | ConvertTo-Json | Set-Content (Join-Path $directory 'result.json') -Encoding utf8
exit $code
