param(
    [Parameter(Mandatory=$true)][string]$TestScript,
    [Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedSha256,
    [Parameter(Mandatory=$true)][string]$Root,
    [Parameter(Mandatory=$true)][ValidateSet('clean','restored','locked','oversized','lazy-background','lazy-all')][string]$Scenario,
    [Parameter(Mandatory=$true)][string]$Evidence,
    [switch]$VerifyOnly
)
$ErrorActionPreference='Stop'

# Windows PowerShell's script-backed Get-FileHash may be unavailable. Keep the
# compatibility function local to this test scope; no module or policy changes.
function Get-FileHash {
    param(
        [Parameter(Mandatory=$true)][string]$LiteralPath,
        [ValidateSet('SHA256')][string]$Algorithm='SHA256'
    )
    $stream=[IO.File]::Open($LiteralPath,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)
    $sha=[Security.Cryptography.SHA256]::Create()
    try { [pscustomobject]@{ Hash=[BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-',''); Path=$LiteralPath; Algorithm=$Algorithm } }
    finally { $sha.Dispose(); $stream.Dispose() }
}

$lease=[IO.File]::Open($TestScript,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)
$sha=[Security.Cryptography.SHA256]::Create()
try {
    if($lease.Length-gt256KB){throw 'Startup test source exceeds its reviewed bound.'}
    $actual=[BitConverter]::ToString($sha.ComputeHash($lease)).Replace('-','')
    if($actual-cne$ExpectedSha256){throw 'Startup test source changed.'}
    if($VerifyOnly){Write-Output 'PASS startup source hash contract; no browser launched.';return}
    $lease.Position=0
    $reader=[IO.StreamReader]::new($lease,[Text.Encoding]::UTF8,$true,4096,$true)
    try { $body=$reader.ReadToEnd() } finally { $reader.Dispose() }
    & ([scriptblock]::Create($body)) -Root $Root -Scenario $Scenario -Seconds 3 -Evidence $Evidence
} finally { $sha.Dispose(); $lease.Dispose() }
