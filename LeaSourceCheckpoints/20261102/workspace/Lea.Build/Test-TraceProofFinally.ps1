$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$output=Join-Path $workspace 'continuity-evidence/trace-proof-finally-controls-v1'
if(Test-Path -LiteralPath $output){throw 'Preserve finally controls'}
$runner=Join-Path $PSScriptRoot 'Test-CurrentNativeScriptSuiteV2.generated.ps1'
. (Join-Path $PSScriptRoot 'Read-AllStartupTraceProof.ps1')
$tokens=$null;$errors=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($runner,[ref]$tokens,[ref]$errors)
$blocks=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.TryStatementAst] -and $n.Finally -and $n.Finally.Extent.Text.Contains('$result.inputsUnchanged=')},$true))
if($errors.Count -or $blocks.Count -ne 1){throw 'Expected one runner cleanup finally'}
$checks=@($blocks[0].Finally.FindAll({param($n)$n -is [Management.Automation.Language.IfStatementAst] -and $n.Extent.Text.Contains('$result.traceProofEvaluated=$false')},$true))
if($checks.Count -ne 1){throw 'Expected trace proof inside finally'}
$check=[scriptblock]::Create($checks[0].Extent.Text)
$initial=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.AssignmentStatementAst] -and $n.Left.Extent.Text -ceq '$result.traceEnvironmentRestored'},$true)|Where-Object {$_.Right.Extent.Text -ceq '$SuiteVersion -ne 7'})
if($initial.Count -ne 1){throw 'Fail-closed initial state missing'}
$null=New-Item -ItemType Directory -Path $output
$outRoot=Join-Path $output 'fixture';$null=New-Item -ItemType Directory -Path $outRoot
$stderr=Join-Path $outRoot 'full-script-timed.stderr.txt'
[IO.File]::WriteAllLines($stderr,@('LEA_ALL_STARTUP_TRACE enabled','LEA_ALL_STARTUP_TRACE restored=true'))
$rows=[Collections.Generic.List[object]]::new()
foreach($name in @('normal','unsafe-throw','incomplete-stream','unobserved-exit','missing-record','duplicate-record','wrong-path','helper-unavailable')){
    $SuiteVersion=7;$traceProofReady=$name -ne 'helper-unavailable'
    $record=@{name='full-script-timed';stderr=$stderr;stdoutComplete=$true;stderrComplete=$true;exitObserved=$true;timedOut=$false;killRequested=$false}
    if($name -eq 'unsafe-throw'){$record.timedOut=$true;$record.killRequested=$true}
    if($name -eq 'incomplete-stream'){$record.stderrComplete=$false}
    if($name -eq 'unobserved-exit'){$record.exitObserved=$false}
    if($name -eq 'wrong-path'){$record.stderr='not-the-owned-log'}
    $runs=@($record)
    if($name -eq 'missing-record'){$runs=@()}
    if($name -eq 'duplicate-record'){$runs=@($record,$record)}
    $result=[ordered]@{traceEnvironmentRestored=$false;error=$null}
    try{if($name -eq 'unsafe-throw'){throw 'controlled unsafe Run failure'}}
    catch{$result.error=$_.Exception.Message}
    finally{& $check}
    if($result.traceEnvironmentRestored -ne ($name -eq 'normal')){throw ('Finally control failed: '+$name)}
    if($name -eq 'unsafe-throw' -and (!$result.traceProofEvaluated -or $result.error -cne 'controlled unsafe Run failure')){throw 'Unsafe failure was hidden or proof skipped'}
    $rows.Add(@{name=$name;passed=$true})
}
$actualRoot=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/current-native-script-run-v8'
$actualProcess=Join-Path $actualRoot 'full-script-timed.result.json'
$actualStderr=Join-Path $actualRoot 'full-script-timed.stderr.txt'
if((Get-FileHash $actualProcess).Hash -cne '06FB33D39DD593146FEE529D9251D2E7245D4B1732CD1BF755532586DFC3681F' -or (Get-FileHash $actualStderr).Hash -cne '101F733CF233EB59EED4ECFA8D17527965180A72288FB856CF6AD552C94B0EA7'){throw 'Actual failure evidence differs'}
$outRoot=$actualRoot;$runs=@(Get-Content $actualProcess -Raw|ConvertFrom-Json);$traceProofReady=$true
$result=[ordered]@{traceEnvironmentRestored=$false;error='preserved actual forced-stop failure'}
try{throw 'controlled replay of unsafe Run'}catch{}finally{& $check}
if(!$result.traceActivationVerified -or $result.traceEnvironmentRestored -or !$result.traceProofEvaluated -or $result.error -cne 'preserved actual forced-stop failure'){throw 'Actual forced-stop replay was not fail-closed'}
$rows.Add(@{name='actual-V8-throw-replay';passed=$true})
[IO.File]::WriteAllText((Join-Path $output 'result.json'),(@{passed=$true;controls=$rows;nativeProcessesStarted=$false;runnerFinallyAstVerified=$true;inputs=@(Get-FileHash $runner,$PSCommandPath,(Join-Path $PSScriptRoot 'Read-AllStartupTraceProof.ps1'),$actualProcess,$actualStderr|Select-Object Path,Hash)}|ConvertTo-Json -Depth 6))
@{passed=$true;controls=$rows.Count;runnerFinallyAstVerified=$true;nativeProcessesStarted=$false}|ConvertTo-Json
