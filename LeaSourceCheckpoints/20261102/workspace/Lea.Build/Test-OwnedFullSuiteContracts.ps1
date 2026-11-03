$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/owned-full-suite-contracts-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve full-suite contracts'}
$runner=Join-Path $PSScriptRoot 'Test-OwnedFullScriptSuite.ps1'
$adapter=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1'
$source=Join-Path $PSScriptRoot 'OwnedTestProcess.cs'
$tokens=$null;$errors=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($runner,[ref]$tokens,[ref]$errors)
$assignments=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.AssignmentStatementAst] -and $n.Left.Extent.Text -ceq '$result.passed'},$true))
if($errors.Count -or $assignments.Count -ne 1){throw 'Expected one full-suite final gate'}
$gate=[scriptblock]::Create($assignments[0].Right.Extent.Text)
$rows=[Collections.Generic.List[object]]::new()
foreach($case in @(
    @{name='complete-clean';expected=$true},
    @{name='incomplete';key='completed';value=$false},
    @{name='behavior-failure';key='behaviorPassed';value=$false},
    @{name='missing-footer';key='footerVerified';value=$false},
    @{name='missing-trace-start';key='traceActivationVerified';value=$false},
    @{name='missing-trace-restore';key='traceEnvironmentRestored';value=$false},
    @{name='recorded-error';key='error';value='retained failure'},
    @{name='changed-input';key='inputsUnchanged';value=$false},
    @{name='acl-delta';key='changedFiles';value=@(@{bytesChanged=$false})},
    @{name='byte-delta';key='changedFiles';value=@(@{bytesChanged=$true})},
    @{name='unretired-owner';key='ownerAttention';value=$true}
)){
    $result=@{completed=$true;behaviorPassed=$true;footerVerified=$true;traceActivationVerified=$true;traceEnvironmentRestored=$true;error=$null;inputsUnchanged=$true;changedFiles=@();ownerAttention=$false}
    if($case.key){$result[$case.key]=$case.value}
    $expected=[bool]$case.expected
    if((& $gate) -ne $expected){throw ('Full-suite gate control failed: '+$case.name)}
    $rows.Add(@{name=$case.name;passed=$true})
}
$null=New-Item -ItemType Directory -Path $out
Add-Type -Path $source
. $adapter
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh registration control required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$records=[Collections.Generic.List[object]]::new()
$row=Invoke-OwnedTestRun -Name 'registration-rejected' -Application (Join-Path $out 'missing-application.exe') -Arguments @() -WorkingDirectory $workspace -OutputDirectory $out -Seconds 1 -Records $records
if($records.Count -ne 1 -or ![object]::ReferenceEquals($records[0],$row) -or $row.passed -or $row.startAttempted -or $row.unsafeToContinue -or $global:LeaCompositeProcessLeases.Count){throw 'Record registration before failure is not stable/fail-closed'}
$rows.Add(@{name='record-registered-before-prelaunch-failure';passed=$true})
[IO.File]::WriteAllText((Join-Path $out 'result.json'),(@{passed=$true;controls=$rows;ownedProcessesStarted=$false;gateExpression=$assignments[0].Right.Extent.Text;inputs=@(Get-FileHash $runner,$adapter,$source,$PSCommandPath|Select-Object Path,Hash)}|ConvertTo-Json -Depth 6))
@{passed=$true;controls=$rows.Count;ownedProcessesStarted=$false}|ConvertTo-Json
