$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/page-color-campaign-contracts-v1'
if(Test-Path $out){throw 'Preserve contracts'}
$oldPath=Join-Path $PSScriptRoot 'Test-CurrentNativeHost.generated.ps1'
$newPath=Join-Path $PSScriptRoot 'Test-PageColorCampaign.ps1'
if((Get-FileHash $oldPath).Hash -cne 'B2468378EBFFA028D7D6D9584501CE17454573644E2E32C1A79F4A781A3A493A'){throw 'Original campaign source differs'}
function Facts([string]$Text,[string]$Command){
    $tokens=$null;$errors=$null;$ast=[Management.Automation.Language.Parser]::ParseInput($Text,[ref]$tokens,[ref]$errors)
    if($errors.Count){throw 'Campaign parse failed'}
    $groups=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.ForEachStatementAst]},$true)|ForEach-Object{
        @($_.Condition.FindAll({param($n)$n -is [Management.Automation.Language.StringConstantExpressionAst]},$true)|ForEach-Object Value) -join ','
    }|Where-Object {$_ -like 'clean,lea,*' -or $_ -like 'clean,restored,*'}|Sort-Object)
    $budgets=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.CommandAst] -and $n.GetCommandName() -ceq $Command},$true)|ForEach-Object{
        $last=$_.CommandElements[-1]
        if($last -is [Management.Automation.Language.ConstantExpressionAst]){$last.Value}
    }|Sort-Object)
    @{groups=$groups;budgets=$budgets}|ConvertTo-Json -Compress
}
$leases=[Collections.Generic.List[IDisposable]]::new()
try{
    foreach($path in @($oldPath,$newPath,$PSCommandPath)){$leases.Add([IO.File]::Open($path,'Open','Read','Read'))}
    $original=[IO.File]::ReadAllText($oldPath);$current=[IO.File]::ReadAllText($newPath)
    $expected=Facts $original 'SafeRun'
    if((Facts $current 'Run') -cne $expected){throw 'Host case lists or original execution budgets changed'}
    $rows=[Collections.Generic.List[object]]::new()
    $rows.Add(@{name='original-case-lists-and-30-90-100-second-budgets';passed=$true})
    foreach($case in @(
        @{name='budget-change-rejected';text=$current.Replace(') 90',') 91')},
        @{name='missing-case-rejected';text=$current.Replace("'split','edit','style-start'","'split','style-start'")}
    )){
        if($case.text -ceq $current -or (Facts $case.text 'Run') -ceq $expected){throw ('Negative control failed: '+$case.name)}
        $rows.Add(@{name=$case.name;passed=$true})
    }
    $null=New-Item -ItemType Directory -Path $out
    Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'controls.ps1')
    @{passed=$true;controls=$rows;facts=($expected|ConvertFrom-Json);scope='Static case/budget parity only, not runtime or complete campaign validation';runtimeExecuted=$false;inputs=@($oldPath,$newPath,$PSCommandPath|ForEach-Object{Get-FileHash $_}|Select-Object Path,Hash)}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'result.json')
    @{passed=$true;controls=$rows.Count}|ConvertTo-Json -Compress
}finally{foreach($s in $leases){$s.Dispose()}}
