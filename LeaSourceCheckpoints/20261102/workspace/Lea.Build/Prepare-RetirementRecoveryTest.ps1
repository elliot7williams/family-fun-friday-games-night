$ErrorActionPreference='Stop'
$template=Join-Path $PSScriptRoot 'Test-DeterministicContainsRecovery.ps1'
if((Get-FileHash $template).Hash -cne '5C41FAA35D0515D0605DB2F86EE57D8ABD73A80A2EC1020BA38C47F180381F2E'){throw 'Recovery template differs'}
$to=Join-Path $PSScriptRoot 'Test-RetirementRecovery-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve recovery test'}
$text=[IO.File]::ReadAllText($template).Replace('continuity-evidence/deterministic-contains-recovery-v1','continuity-evidence/retirement-recovery-v1')
$needle='    $bytes=0L'
if([regex]::Matches($text,[regex]::Escape($needle)).Count -ne 1){throw 'Recovery staging boundary differs'}
$overlay=@'
    $buildPath='I:\LeaBuildStorage\worker-retirement-trace-build-20261102-v2\result.json'
    Hold $buildPath '8B971B3212FEDAD1F5F01380FDC963F4F862BB683CB01B77356535774537ECEC'
    $build=Get-Content $buildPath -Raw|ConvertFrom-Json
    if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or !$build.diagnosticOnly -or $build.sha256 -cne 'E5896C436C840022CC4B840F866F7D7F5D92B5E0CA1B546B20AC19C5A1FB1111'){throw 'Diagnostic build incomplete'}
    $selected=@($files|Where-Object path -CEQ 'runtime/Lea.Script.dll')
    if($selected.Count -ne 1){throw 'Unique script DLL substitution required'}
    $result.originalScriptSha256=$selected[0].sha256
    $selected[0].origin=$build.binary;$selected[0].sha256=$build.sha256
    $result.diagnosticOnly=$true
'@
$text=$text.Replace($needle,$overlay+[Environment]::NewLine+$needle)
$text=$text.Replace('Existing --contains isolated entry on fresh exact-byte NTFS closure; no full-prefix/cold-equivalence claim','Existing --contains isolated assertions with one diagnostic script DLL substitution; original runtime/limits otherwise; no full-prefix or release qualification')
$text=$text.Replace("    `$result.passed=`$run.passed", "    `$result.retirementMarkerLines=@(Get-Content `$run.stderr|Where-Object {`$_.StartsWith('LEA_RETIREMENT_TRACE ')}).Count"+[Environment]::NewLine+"    `$result.passed=(`$result.retirementMarkerLines -gt 0) -and `$run.passed")
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Recovery projection syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
