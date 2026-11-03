$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-CurrentNativeHost.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve current native host campaign'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-CurrentFlowHost.generated.ps1'))
$text=$text.Replace('host-current-flow-v1','host-current-native-v1')
$start=$text.IndexOf("    `$coreProof=`$workspace+'\continuity-evidence\decoder-launch-full-v2\result.json'")
$end=$text.IndexOf("    `$policy=`$workspace+",$start)
if($start -lt 0 -or $end -le $start){throw 'Native host proof span differs'}
$binding=@'
    $coreProof=$workspace+'\continuity-evidence\nested-overlap-regression-v1\result.json'
    $null=Pin $coreProof 'E60A49F48671E92B80279171E5FC274BB4D7CFF2020D185A86729B7A38F04B6E'
    $core=Get-Content -LiteralPath $coreProof -Raw|ConvertFrom-Json
    $observation=$workspace+'\continuity-evidence\nested-overlap-regression-v1\outcome-observation.json'
    $null=Pin $observation '89AD895CA8DD8A560A9A3DB217AA1FD7955FE42D9D0F147898F13C60149E0F2A'
    $observed=Get-Content -LiteralPath $observation -Raw|ConvertFrom-Json
    if(!$core.behaviorPassed -or !$core.inputsUnchanged -or !$core.bytesUnchanged -or $core.ownerAttention -or !$observed.nativeBehaviorPassed -or !$observed.exactTerminalFooterVerified -or $observed.originalParentBudgetMs -ne 300000 -or $observed.productionPolicyBroadened){throw 'Current native execution evidence incomplete'}
    foreach($input in @($core.inputs)+@($core.before)){$null=Pin $input.path $input.sha256}
    $result.priorFullRawAuditPassed=$core.passed
'@
$text=$text.Remove($start,$end-$start).Insert($start,$binding+"`r`n")
$old="CopyExact (`$workspace+'\continuity-evidence\decoder-launch-proof-v2\runtime\Lea.Win.dll') (`$result.candidate+'\Lea.Win.dll') 'B2C1A05ABC9772857F69C9B6AE34C9E1CB69E51DFC35E8F03F60F70F4A523E6D'"
$new="CopyExact (`$workspace+'\continuity-evidence\current-hit-geometry-v3\Lea.Win.dll') (`$result.candidate+'\Lea.Win.dll') '74956DA3FBBF28BC1DF281691FE7F8215C74CA67608AA61F6A6540E3BB0714AC'"
if([regex]::Matches($text,[regex]::Escape($old)).Count -ne 1){throw 'Current host view binding differs'}
$text=$text.Replace($old,$new)
$anchor='    foreach($file in Get-ChildItem $result.candidate -Recurse -File){$null=Pin $file.FullName}'
$replacement=@'
    $diagnosticProof=$workspace+'\continuity-evidence\split-host-diagnostic-v2\result.json'
    $null=Pin $diagnosticProof '7999E9A2CE4AA3B834FB1E731CDBC8C091F00F9E79F82CCDC0202D1745A1FFBC'
    $diagnostic=Get-Content -LiteralPath $diagnosticProof -Raw|ConvertFrom-Json
    if(!$diagnostic.passed -or !$diagnostic.inputsUnchanged -or !$diagnostic.candidateBytesUnchanged -or $diagnostic.ownerAttention){throw 'Failure-only diagnostic launcher binding incomplete'}
    CopyExact ($workspace+'\continuity-evidence\split-host-diagnostic-v2\runtime\Lea.Firespider.Boot.exe') ($result.candidate+'\Lea.Firespider.Boot.exe') '2C44FB4A92088FBF9B548B426B267163E5900A1BD04794BF9843EA869E9532D4'
    foreach($file in Get-ChildItem $result.candidate -Recurse -File){$null=Pin $file.FullName}
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Diagnostic launcher binding anchor differs'}
$text=$text.Replace($anchor,$replacement)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
