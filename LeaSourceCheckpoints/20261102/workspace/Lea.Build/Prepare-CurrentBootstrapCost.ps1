$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-CurrentBootstrapCost.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve current bootstrap cost attempt'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-SameContextBootstrapCost.ps1'))
$text=$text.Replace('same-context-bootstrap-cost-v4','current-flow-bootstrap-cost-v1').Replace('numeric-references-script-suite-v1','current-flow-script-suite-v1').Replace('8992F34CA977FA5DF8F65501E8495342F53D5538EC524682FC826B6B4C192071','6FC8EE19097D2EA29FCC0FCFEE0F53A9C57611C706A22483801B91938FB47087')
$anchor="    `$calibration=Join-Path `$workspace"
$binding=@'
    $failure=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/current-flow-script-run-v1/result.json'
    Hold $failure '0856EF5249F6188C647C4CB0FD23403F46CC6DD5486DCE03AE0AC324DE2BC59C'
    $failed=Get-Content -LiteralPath $failure -Raw|ConvertFrom-Json
    if(!$failed.inputsUnchanged -or $failed.ownerAttention -or $failed.changedFiles.Count -ne 0 -or $failed.passed){throw 'Current failed runtime binding incomplete'}
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Current diagnostic failure anchor differs'}
$text=$text.Replace($anchor,$binding+"`r`n"+$anchor)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
