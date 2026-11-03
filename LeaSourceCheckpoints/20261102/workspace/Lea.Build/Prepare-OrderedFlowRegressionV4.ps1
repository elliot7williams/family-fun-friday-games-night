$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-OrderedFlowRegression.v4.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve nested model full regression'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-OrderedFlowRegression.v2.generated.ps1'))
$text=$text.Replace('ordered-flow-regression-v2','ordered-flow-regression-v4')
$text=$text.Replace('continuity-evidence/ordered-flow-v4/candidate/Lea.Engine.dll','continuity-evidence/ordered-flow-v6/candidate/Lea.Engine.dll').Replace('D0A8061162EF716A9B5F940AC80119BF84E98F6EFFF3821737AA80541262E7F6','52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC')
$text=$text.Replace('continuity-evidence/named-reference-visuals-v4/runtime/Lea.Win.dll','continuity-evidence/decoder-owner-proof-v1/runtime/Lea.Win.dll').Replace('9D9C7A3937B59206514D5631164A442744D9F5728CC43952AD643A86A5EB4867','12A9CAF4880BA5A377018BECEF25FBE27740B8455F02CA3E16F5CA7FB642DF9C')
$anchor="    `$overlay=Join-Path `$outRoot 'Smoke.cs';"
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Nested model overlay anchor differs'}
$overlay=@'
    $visualProof=Join-Path $workspace 'continuity-evidence/nested-flow-visuals-v1/result.json';$null=Pin $visualProof 'CABAC7356A80E3464C95B49106B934930E97C9F705DB23FE3CC815A7CD444660'
    $v=Get-Content -LiteralPath $visualProof -Raw|ConvertFrom-Json
    if(!$v.passed -or !$v.inputsUnchanged -or !$v.candidateUnchanged -or $v.ownerAttention){throw 'Nested model/geometry proof incomplete'}
    $helper=Join-Path $PSScriptRoot 'New-OrderedFlowSmokeSource.ps1';$null=Pin $helper;. $helper
    $updated=New-OrderedFlowSmokeSource $updated
    $result.nestedModelOverlayInverseVerified=$true
'@
$text=$text.Replace($anchor,$overlay+"`r`n"+$anchor)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
