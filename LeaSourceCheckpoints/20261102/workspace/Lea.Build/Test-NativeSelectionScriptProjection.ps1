$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/native-selection-script-projection-controls-v1'
if(Test-Path $out){throw 'Preserve projection controls'}
function FinallyText([string]$Text){
    $tokens=$null;$errors=$null
    $ast=[Management.Automation.Language.Parser]::ParseInput($Text,[ref]$tokens,[ref]$errors)
    $blocks=@($ast.EndBlock.Statements|Where-Object{$_ -is [Management.Automation.Language.TryStatementAst] -and $_.Finally})
    if($errors.Count -or $blocks.Count -ne 1){throw 'Unique top-level finally required'}
    $blocks[0].Finally.Extent.Text.Replace([string][char]13,'')
}
function RunText([string]$Text){
    $tokens=$null;$errors=$null
    $ast=[Management.Automation.Language.Parser]::ParseInput($Text,[ref]$tokens,[ref]$errors)
    $calls=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.CommandAst] -and $n.GetCommandName() -ceq 'Invoke-OwnedTestRun'},$true))
    if($errors.Count -or $calls.Count -ne 1){throw 'Unique owned script run required'}
    $calls[0].Extent.Text.Replace([string][char]13,'')
}
$old=Join-Path $PSScriptRoot 'Test-OwnedFullScriptSuite.ps1'
$new=Join-Path $PSScriptRoot 'Test-OwnedNativeSelectionScriptSuite.generated.ps1'
$original=[IO.File]::ReadAllText($old);$candidate=[IO.File]::ReadAllText($new)
$expectedFinally=FinallyText $original;$expectedRun=RunText $original
if((FinallyText $candidate) -cne $expectedFinally -or (RunText $candidate) -cne $expectedRun){throw 'Runtime/trace/retirement contract changed'}
$rows=[Collections.Generic.List[object]]::new()
$rows.Add(@{name='original-finally-identical';passed=$true})
$rows.Add(@{name='original-owned-call-and-300-second-budget-identical';passed=$true})
foreach($case in @(
    @{name='changed-budget-rejected';text=$candidate.Replace('-Seconds 300','-Seconds 301');kind='run'},
    @{name='trace-finally-change-rejected';text=$candidate.Replace('$result.traceEnvironmentRestored=$proof.traceEnvironmentRestored','$result.traceEnvironmentRestored=$true');kind='finally'}
)){
    if($case.text -ceq $candidate){throw 'Negative mutation did not apply'}
    $same=if($case.kind -eq 'run'){(RunText $case.text) -ceq $expectedRun}else{(FinallyText $case.text) -ceq $expectedFinally}
    if($same){throw 'Negative mutation was accepted'}
    $rows.Add(@{name=$case.name;passed=$true})
}
$preparation=Join-Path $workspace 'continuity-evidence/native-selection-script-preparation-v2/result.json'
$proof=Get-Content $preparation -Raw|ConvertFrom-Json
if(!$proof.passed -or !$proof.inverseVerified -or !$proof.generatedSyntaxVerified -or $proof.generated.Count -ne 4){throw 'Preparation proof incomplete'}
foreach($file in $proof.generated){if((Get-FileHash $file.Path).Hash -cne $file.Hash){throw 'Generated source changed after projection'}}
$rows.Add(@{name='four-generated-source-pins-and-inverses';passed=$true})
$null=New-Item -ItemType Directory -Path $out
Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'controls.ps1')
@{passed=$true;controls=$rows;runtimeExecuted=$false;scope='Static exact finally/owned-call parity and mutation rejection, not runtime qualification';inputs=@($old,$new,$preparation,$PSCommandPath|ForEach-Object{Get-FileHash $_}|Select-Object Path,Hash)}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'result.json')
@{passed=$true;controls=$rows.Count}|ConvertTo-Json -Compress
