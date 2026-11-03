$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Test-StartupExit.ps1'
$target=Join-Path $PSScriptRoot 'Test-StartupStall.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve runner'}
$text=[IO.File]::ReadAllText($base)
$changes=@(
    @('startup-exit-v2','startup-stall-v1'),
    @('StartupExitProbe.cs','StartupStallProbe.cs'),
    @('intentionalChildExitCode=23','intentionalHandshakeWithheld=$true'),
    @('Injected confined pre-handshake exit; not intermittent timeout reproduction or release certification','Withheld parent handshake transport with live confined worker; not actual worker stall causality or release certification'),
    @("Run 'injected-exit'","Run 'withheld-handshake'")
)
foreach($pair in $changes){
    if(!$text.Contains($pair[0])){throw 'Runner anchor missing'}
    $text=$text.Replace($pair[0],$pair[1])
}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Runner syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
