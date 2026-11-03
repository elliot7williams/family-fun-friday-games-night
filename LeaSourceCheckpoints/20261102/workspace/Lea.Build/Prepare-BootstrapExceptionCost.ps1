$ErrorActionPreference='Stop'
$source=Join-Path $PSScriptRoot 'Test-SameContextBootstrapCost.ps1'
$target=Join-Path $PSScriptRoot 'Test-BootstrapExceptionCost.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve exception diagnostic runner'}
$text=[IO.File]::ReadAllText($source)
$changes=@(
    @('same-context-bootstrap-cost-v4','bootstrap-exception-cost-v1'),
    @('BootstrapCostObserver.cs','BootstrapExceptionObserver.cs'),
    @('F0F8AE9A7560130A3CC681C7B27C20AE1CD90D182DD86B208C9BFF324C5013D5',(Get-FileHash -LiteralPath (Join-Path $PSScriptRoot 'BootstrapExceptionObserver.cs')).Hash),
    @("if(!`$report.observerRetired -or `$report.observerError -or `$report.samplesOverflow -or `$report.durationExceeded -or !`$report.cpuBracketsValid -or !`$report.allocationThreadMatched -or `$null -eq `$report.allocationBytes -or `$report.allocationBytes -lt 0){throw 'Observer incomplete'}", "if(!`$report.handlerRemoved -or `$report.overflow -or `$report.callbackErrors -ne 0){throw 'Exception observer incomplete'}")
)
foreach($pair in $changes){
    $expected=if($pair[0] -ceq 'BootstrapCostObserver.cs'){2}else{1}
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne $expected){throw 'Exception diagnostic anchor differs'}
    $text=$text.Replace($pair[0],$pair[1])
}
$tokens=$null;$errors=$null
$anchor="    if(`$report.executionOk -and `$report.mutations -eq 6 -and (`$stops -ne 1 -or !`$run.passed)){throw 'Successful diagnostic did not stop cleanly'}"
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Exception outcome anchor differs'}
$extra=@'
    $failures=@(Get-Content -LiteralPath $run.stderr|Where-Object {$_.StartsWith('System.InvalidOperationException: FAIL computed script:')})
    if(!$report.executionOk -and ($run.exitCode -ne 1 -or $stops -ne 0 -or $failures.Count -ne 1)){throw 'Unexpected exception diagnostic outcome'}
'@
$text=$text.Replace($anchor,$anchor+"`r`n"+$extra)
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
