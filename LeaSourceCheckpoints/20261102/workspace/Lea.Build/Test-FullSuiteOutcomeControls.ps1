$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/full-suite-outcome-controls-v2'
if(Test-Path $out){throw 'Preserve outcome controls'}
$path=Join-Path $PSScriptRoot 'Test-CurrentNativeScriptSuiteV2.generated.ps1'
$tokens=$null;$errors=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($path,[ref]$tokens,[ref]$errors)
$assignments=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.AssignmentStatementAst] -and $n.Left.Extent.Text -ceq '$result.passed'},$true))
if($errors.Count -or $assignments.Count -ne 1 -or !$assignments[0].Right.Extent.Text.Contains('$result.footerVerified')){throw 'Expected one footer-gated outcome expression'}
$expression=[scriptblock]::Create($assignments[0].Right.Extent.Text)
$rows=[Collections.Generic.List[object]]::new()
foreach($case in @(
    @{name='complete-clean-result';key=$null;value=$null;expected=$true},
    @{name='not-completed';key='completed';value=$false;expected=$false},
    @{name='child-failed';key='behaviorPassed';value=$false;expected=$false},
    @{name='missing-terminal-footer';key='footerVerified';value=$false;expected=$false},
    @{name='recorded-error';key='error';value='fixture failure';expected=$false},
    @{name='changed-input';key='inputsUnchanged';value=$false;expected=$false},
    @{name='unretired-owner';key='ownerAttention';value=$true;expected=$false},
    @{name='unrestored-trace-environment';key='traceEnvironmentRestored';value=$false;expected=$false},
    @{name='raw-private-file-delta';key='changedFiles';value=@(@{path='fixture';bytesChanged=$false});expected=$false}
)){
    $result=@{completed=$true;behaviorPassed=$true;footerVerified=$true;error=$null;inputsUnchanged=$true;ownerAttention=$false;changedFiles=@();traceEnvironmentRestored=$true}
    if($case.key){$result[$case.key]=$case.value}
    if((& $expression) -ne $case.expected){throw ('Outcome control failed: '+$case.name)}
    $rows.Add(@{name=$case.name;passed=$true})
}
$null=New-Item -ItemType Directory $out
@{passed=$true;controls=$rows;runtimeExecuted=$false;runnerSha256=(Get-FileHash $path).Hash;expression=$assignments[0].Right.Extent.Text}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'result.json')
@{passed=$true;controls=$rows.Count}|ConvertTo-Json
