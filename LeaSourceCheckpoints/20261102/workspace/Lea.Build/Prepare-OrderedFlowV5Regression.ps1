$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$proof=Join-Path $workspace 'continuity-evidence/ordered-flow-v6/result.json'
$p=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
if(!$p.passed -or !$p.inputsUnchanged -or $p.ownerAttention){throw 'Ordered V5 proof incomplete'}
$target=Join-Path $PSScriptRoot 'Test-OrderedFlowV5Regression.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve V5 full regression'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-DecoderOwnerFull.generated.ps1'))
$text=$text.Replace('decoder-owner-full-v1','ordered-flow-regression-v3')
$anchor="    `$b=Get-Content -LiteralPath `$basePath -Raw|ConvertFrom-Json"
$binding=@'
    $coreProof=Join-Path $workspace 'continuity-evidence/ordered-flow-v6/result.json';Hold $coreProof 'CORE_RECEIPT'
    $core=Get-Content -LiteralPath $coreProof -Raw|ConvertFrom-Json
    if(!$core.passed -or !$core.inputsUnchanged -or $core.ownerAttention){throw 'V5 core binding incomplete'}
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'V5 core receipt anchor differs'}
$text=$text.Replace($anchor,$anchor+"`r`n"+$binding.Replace('CORE_RECEIPT',(Get-FileHash -LiteralPath $proof).Hash))
$anchor="        if(`$name -ceq 'Lea.Win.dll'){`$from=`$trace.viewBuild.binary;`$hash=`$trace.viewBuild.sha256}"
$binding="        if(`$name -ceq 'Lea.Engine.dll'){`$from=`$core.binary;`$hash=`$core.sha256}"
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'V5 core copy anchor differs'}
$text=$text.Replace($anchor,$anchor+"`r`n"+$binding)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
