$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$proof=Join-Path $workspace 'continuity-evidence/decoder-launch-proof-v1/result.json'
$p=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
if(!$p.passed -or !$p.inputsUnchanged -or !$p.candidateUnchanged -or $p.ownerAttention){throw 'Launch controls incomplete'}
$target=Join-Path $PSScriptRoot 'Test-DecoderLaunchFull.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve launch full attempt'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-OrderedFlowV5Regression.generated.ps1'))
$text=$text.Replace('ordered-flow-regression-v3','decoder-launch-full-v1').Replace('decoder-owner-proof-v1/result.json','decoder-launch-proof-v1/result.json').Replace('C397B4C7BF909BCA2FB5B9031AD2B7D2BB3873C63EDE33FAB877804BEAE56423',(Get-FileHash -LiteralPath $proof).Hash)
$anchor="    `$b=Get-Content -LiteralPath `$basePath -Raw|ConvertFrom-Json"
$binding=@'
    $testProof=Join-Path $workspace 'continuity-evidence/ordered-flow-regression-v4/result.json';Hold $testProof '9A1C1B073D72892D652542D9CEAF32C2D5620696B217C5C103309802A597C9C3'
    $test=Get-Content -LiteralPath $testProof -Raw|ConvertFrom-Json
    if(!$test.inputsUnchanged -or !$test.bytesUnchanged -or $test.ownerAttention -or !$test.nestedModelOverlayInverseVerified -or !$test.testOverlayInverseVerified){throw 'Updated test source binding incomplete'}
    $testRows=@($test.before|Where-Object {[IO.Path]::GetFileName($_.path) -ceq 'Lea.Tests.exe'})
    if($testRows.Count -ne 1){throw 'Updated test binary ambiguous'}
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Launch test receipt anchor differs'}
$text=$text.Replace($anchor,$anchor+"`r`n"+$binding)
$anchor="        if(`$name -ceq 'Lea.Engine.dll'){`$from=`$core.binary;`$hash=`$core.sha256}"
$binding="        if(`$name -ceq 'Lea.Tests.exe'){`$from=`$testRows[0].path;`$hash=`$testRows[0].sha256}"
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Launch test copy anchor differs'}
$text=$text.Replace($anchor,$anchor+"`r`n"+$binding)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
