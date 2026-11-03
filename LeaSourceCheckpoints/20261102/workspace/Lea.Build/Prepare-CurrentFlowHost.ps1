$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-CurrentFlowHost.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve current flow host attempt'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-NamedReferenceHost.generated.ps1'))
$text=$text.Replace('host-named-references-v1','host-current-flow-v1')
$start=$text.IndexOf("    `$coreProof=`$workspace+'\continuity-evidence\named-references-regression-v1\result.json'")
$end=$text.IndexOf("    `$policy=`$workspace+",$start)
if($start -lt 0 -or $end -le $start){throw 'Host core proof span differs'}
$binding=@'
    $coreProof=$workspace+'\continuity-evidence\decoder-launch-full-v2\result.json'
    $null=Pin $coreProof 'A06A2CD21986FFBDF882753FB5D639CF4112B24F5686A27385F3A9268CEAC3DE'
    $core=Get-Content -LiteralPath $coreProof -Raw|ConvertFrom-Json
    $observation=$workspace+'\continuity-evidence\decoder-launch-full-v2\outcome-observation.json'
    $null=Pin $observation 'FA96EC2A08E8CEC9E2F10244F7EE078223A10DFF43924EB671B44BBEE223DF92'
    $observed=Get-Content -LiteralPath $observation -Raw|ConvertFrom-Json
    if(!$core.inputsUnchanged -or !$core.candidateBytesUnchanged -or $core.ownerAttention -or !$observed.behaviorVerified -or !$observed.terminalFooterMatchesExactSource -or $observed.exitCode -ne 0 -or $observed.deadlineMs -ne 300000){throw 'Current matched full behavior evidence incomplete'}
    foreach($input in $core.inputs){$null=Pin $input.path $input.sha256}
    $result.priorFullRawAuditPassed=$core.passed
'@
$text=$text.Remove($start,$end-$start).Insert($start,$binding+"`r`n")
$text=$text.Replace('\continuity-evidence\named-references-v1\candidate\Lea.Engine.dll','\continuity-evidence\ordered-flow-v6\candidate\Lea.Engine.dll').Replace('9208520EE6A067FF38D39368B2E50E6B099E5120C977A071E8D3E09733A3AF17','52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC')
$old="CopyExact (`$permissionRoot+'\retry-view-v1\Lea.Win.dll') (`$result.candidate+'\Lea.Win.dll') 'E26135F8CE6BA76A76515D2874DFE219E172B212037CD567AD37042DA516289B'"
$new="CopyExact (`$workspace+'\continuity-evidence\decoder-launch-proof-v2\runtime\Lea.Win.dll') (`$result.candidate+'\Lea.Win.dll') 'B2C1A05ABC9772857F69C9B6AE34C9E1CB69E51DFC35E8F03F60F70F4A523E6D'"
if([regex]::Matches($text,[regex]::Escape($old)).Count -ne 1){throw 'Current host view anchor differs'}
$text=$text.Replace($old,$new)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
