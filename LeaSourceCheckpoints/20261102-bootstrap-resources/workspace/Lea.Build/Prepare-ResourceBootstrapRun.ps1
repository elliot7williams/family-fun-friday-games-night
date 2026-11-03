$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Test-TrustedBootstrapProfile-v3.generated.ps1'
$to=Join-Path $PSScriptRoot 'Test-TrustedBootstrapProfile-v4.generated.ps1'
if(Test-Path $to){throw 'Preserve resource diagnostic launcher'}
if((Get-FileHash $from).Hash -cne '1E141082364CFFD41D866A60105963536015C7E43C146509830C139C1B09D231'){throw 'Detailed profile run template changed'}
$original=[IO.File]::ReadAllText($from)
$pairs=@(
    @('continuity-evidence/trusted-bootstrap-profile-v3','continuity-evidence/trusted-bootstrap-profile-v4'),
    @('trusted-bootstrap-profile-build-20261102-v3','trusted-bootstrap-profile-build-20261102-v4'),
    @('40B540471BEBE09DD44A14642DDF378F0CC7AF1ED59C1C9D2B34918956202FE3','8849F3956D4BC6D6E97E444BFB6451C58102515F5EA8FE3B07B9DA19011FDEEE'),
    @('A03E3608E2D98CEB8CEDBD6882052692E8AE98CA5552C3545C35F42422EA807E','7F2373FE748206C2CE66366F6BC35B43B9AEBC82B3B5DA3565FCCE47D0CCC338'),
    @('    foreach($file in $dlls){',("    `$stageBytes=[long]((`$dlls|Measure-Object Length -Sum).Sum)+256KB`n    if((Get-PSDrive C).Free -lt (600MB+`$stageBytes)){throw 'Reserve plus full diagnostic closure required'}`n    foreach(`$file in `$dlls){"))
)
$text=$original
foreach($pair in $pairs){if($pair.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique resource run boundary required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Resource run inverse differs'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
