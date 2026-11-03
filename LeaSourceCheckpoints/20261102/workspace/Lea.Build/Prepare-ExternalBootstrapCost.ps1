$ErrorActionPreference='Stop'
$source=Join-Path $PSScriptRoot 'Test-SameContextBootstrapCost.ps1'
$target=Join-Path $PSScriptRoot 'Test-ExternalBootstrapCost.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve existing external runner'}
$text=[IO.File]::ReadAllText($source)
function Replace-One([string]$old,[string]$new){
    if([regex]::Matches($script:text,[regex]::Escape($old)).Count -ne 1){throw 'Runner transformation anchor differs'}
    $script:text=$script:text.Replace($old,$new)
}
Replace-One 'same-context-bootstrap-cost-v4' 'external-bootstrap-cost-v1'
Replace-One "    `$calibration=Join-Path `$workspace 'continuity-evidence/temporal-startup-v1/native-counter-calibration-v2/result.json'" "    `$calibration=Join-Path `$workspace 'continuity-evidence/temporal-startup-v1/external-bootstrap-tools-v1/result.json'"
Replace-One '468022F6F7178448A97D590B404C0F50BE9DE8B10312B4CF0AF5C0548F076D66' '4E74FEF6F9BBC68FF0419DFA5EF3F472F79DC5D0508887748245B2CB352DA810'
Replace-One "    if(!`$control.passed -or !`$control.inputsUnchanged -or `$control.ownerAttention -or !`$control.observation.gcMonotonic -or !`$control.observation.allocationSupported -or !`$control.observation.allocationAdvanced){throw 'Counter calibration incomplete'}" "    if(!`$control.passed -or `$control.checks.Count -ne 8){throw 'External tool checks incomplete'}"
Replace-One 'New-BootstrapCostProgram.ps1' 'New-ExternalBootstrapProgram.ps1'
Replace-One '776C702F074F4E49471815286D1B546A3E10A526727C2637F0CB382792E9F163' '4E6F069BA30215881CC27F3930FDD5807AB50649252D30BCCECCD518AAE403A0'
Replace-One 'New-BootstrapCostProgram ([IO.File]::ReadAllText($from))' 'New-ExternalBootstrapProgram ([IO.File]::ReadAllText($from))'
$start=$text.IndexOf("    `$helper=Join-Path `$PSScriptRoot 'BootstrapCostObserver.cs'")
$end=$text.IndexOf("    `$rsp=Join-Path `$outRoot 'compile.rsp'",$start)
if($start -lt 0 -or $end -le $start){throw 'Helper span differs'}
$text=$text.Remove($start,$end-$start).Insert($start,@'
    $helper=Join-Path $PSScriptRoot 'ExternalProcessCounters.cs'
    Hold $helper '128CCB2A4B1E97D6D98B37A6E6A9B70AF7FF15F3DFCBA66CAA274E2D8C95B886'
    Add-Type -Path $helper
    $result.samples=[Collections.Generic.List[object]]::new()
    $result.sampleErrors=[Collections.Generic.List[string]]::new()
    $result.sampleOverflow=$false
    $runText=(Get-Command Run).Definition
    $anchor='while($true){foreach($r in $readers){PumpOutput $r};'
    if([regex]::Matches($runText,[regex]::Escape($anchor)).Count -ne 1){throw 'Sampling anchor differs'}
    $sample={
while($true){foreach($r in $readers){PumpOutput $r};
    if(!$p.HasExited -and $result.sampleErrors.Count -eq 0){
        if($result.samples.Count -ge 1024){$result.sampleOverflow=$true}
        else{
            try{
                $query=[Diagnostics.Stopwatch]::StartNew()
                $snapshot=[Lea.Build.ExternalProcessCounters]::Read($p.Handle)
                $result.samples.Add(@{elapsedMs=$clock.ElapsedMilliseconds;queryMs=$query.Elapsed.TotalMilliseconds;native=$snapshot})
            }catch{
                if(!$p.HasExited){$result.sampleErrors.Add($_.Exception.ToString())}
            }
        }
    }
}
    }.ToString()
    $sample=$sample.Substring(0,$sample.LastIndexOf('}'))
    $runText=$runText.Replace($anchor,$sample)
    . ([scriptblock]::Create('function RunExternal {'+$runText+'}'))

'@)
$start=$text.IndexOf('    $lines=@(Get-Content -LiteralPath $run.stdout|Where-Object')
$end=$text.IndexOf('    $phases=@(Get-Content -LiteralPath $run.stderr',$start)
if($start -lt 0 -or $end -le $start){throw 'Report span differs'}
$text=$text.Remove($start,$end-$start).Insert($start,@'
    if(!$run.exitObserved -or $run.timedOut -or $run.killRequested -or !$run.stdoutComplete -or !$run.stderrComplete){throw 'External diagnostic lifecycle incomplete'}
    if($result.samples.Count -lt 2 -or $result.sampleErrors.Count -or $result.sampleOverflow){throw 'External samples incomplete'}

'@)
Replace-One "`$run=Run 'same-context-cost'" "`$run=RunExternal 'external-cost'"
Replace-One "    if(`$report.executionOk -and `$report.mutations -eq 6 -and (`$stops -ne 1 -or !`$run.passed)){throw 'Successful diagnostic did not stop cleanly'}" @'
    $failureLines=@(Get-Content -LiteralPath $run.stderr|Where-Object {$_.StartsWith('System.InvalidOperationException: FAIL computed script:')})
    if($run.passed){if($stops -ne 1 -or $failureLines.Count){throw 'Successful diagnostic stop differs'}}
    elseif($run.exitCode -ne 1 -or $stops -ne 0 -or $failureLines.Count -ne 1){throw 'Unexpected diagnostic failure'}
    $result.computedPassed=$run.passed
    $result.computedFailure=$failureLines
'@
Replace-One '    $result.observation=$report;$result.overlayInverseVerified=$true;$result.diagnosticCaptured=$true' '    $result.overlayInverseVerified=$true;$result.diagnosticCaptured=$true'
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
