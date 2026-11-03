$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Test-DecoderAclDelta.ps1')
$sid = 'S-1-15-2-3597768014-2038560105-1077951562-531713394-1456559389-3372869253-1355503848'
$before = 'O:SYG:SYD:PAI(A;;FA;;;SY)'
$ace = "(A;;0x1200a9;;;$sid)"
$after = $before + $ace
$arguments = @{ Path='C:\private\Lea.Image.exe'; ExpectedDecoderPath='C:\private\Lea.Image.exe'; Before=$before; After=$after; ProfileName='Lea.Image.Decoder' }
$count = 0
function Check([string]$Name, [hashtable]$Changes, [bool]$Expected) {
    $values = $arguments.Clone()
    foreach ($key in $Changes.Keys) { $values[$key] = $Changes[$key] }
    $actual = Test-DecoderAclDelta @values
    if ($actual -ne $Expected) { throw "FAIL $Name : $actual" }
    $script:count++
    "PASS $Name"
}
Check 'exact fixed decoder grant' @{} $true
Check 'temporary profile' @{TemporaryProfile=$true} $false
Check 'script profile' @{ProfileName='Lea.Script.Worker'} $false
Check 'different directory' @{Path='C:\other\Lea.Image.exe'} $false
Check 'different executable' @{Path='C:\private\Lea.Script.Worker.exe'} $false
Check 'relative path' @{Path='Lea.Image.exe'} $false
Check 'no delta' @{After=$before} $false
Check 'full control' @{After=$before+"(A;;FA;;;$sid)"} $false
Check 'deny' @{After=$before+"(D;;0x1200a9;;;$sid)"} $false
Check 'inherited grant' @{After=$before+"(A;ID;0x1200a9;;;$sid)"} $false
Check 'inheritable grant' @{After=$before+"(A;OI;0x1200a9;;;$sid)"} $false
Check 'other profile SID' @{After=$before+'(A;;0x1200a9;;;S-1-15-2-1)'} $false
Check 'owner changed' @{After=$after.Replace('O:SY','O:BA')} $false
Check 'unrelated extra grant' @{After=$after+'(A;;FR;;;BU)'} $false
Check 'preexisting shared grant' @{Before=$after} $false
Check 'malformed descriptor' @{After='invalid'} $false
"PASS decoder ACL policy: $count checks; no filesystem ACL writes."
