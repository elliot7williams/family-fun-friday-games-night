$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$path=Join-Path $PSScriptRoot 'Invoke-UiaProcessPair.ps1'
$out=Join-Path $root 'continuity-evidence/uia-pair-completion-v1'
if(Test-Path $out){throw 'Fresh decision-check evidence required'}
$hold=[IO.File]::Open($path,'Open','Read','Read')
try{
    $hash=(Get-FileHash $path).Hash
    $tokens=$null;$errors=$null
    $ast=[Management.Automation.Language.Parser]::ParseFile($path,[ref]$tokens,[ref]$errors)
    if($errors.Count){throw 'Pair source does not parse'}
    function Decision([string]$Name){
        $nodes=@($ast.FindAll({param($node) $node -is [Management.Automation.Language.AssignmentStatementAst] -and $node.Left.Extent.Text -ceq $Name},$true))
        if($nodes.Count -ne 1){throw 'Decision expression ambiguous'}
        [scriptblock]::Create($nodes[0].Right.Extent.Text)
    }
    $deadline=Decision '$retirementDeadlineExceeded';$gate=Decision '$passed'
    $records=@(foreach($case in @(
        @(0,2,$true,$true,$true),@(4999,2,$true,$true,$true),@(5000,2,$true,$true,$true),
        @(5001,2,$true,$true,$false),@(5004,2,$true,$true,$false),
        @(0,1,$true,$true,$false),@(0,2,$false,$true,$false),@(0,2,$true,$false,$false)
    )){
        $retirementMs=[long]$case[0];$leases=@(for($i=0;$i -lt $case[1];$i++){@{record=@{passed=[bool]$case[2]}}});$stopSent=[bool]$case[3]
        $retirementDeadlineExceeded=& $deadline
        $actual=[bool](& $gate)
        if($actual -ne $case[4] -or $retirementDeadlineExceeded -ne ($retirementMs -gt 5000)){throw 'Completion decision boundary failed'}
        [pscustomobject]@{retirementMs=$retirementMs;members=$leases.Count;membersPassed=$case[2];stopSent=$stopSent;accepted=$actual}
    })
    if((Get-FileHash $path).Hash -cne $hash){throw 'Decision source changed'}
    $null=New-Item -ItemType Directory -Path $out
    Copy-Item $path (Join-Path $out 'Invoke-UiaProcessPair.ps1')
    Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
    $result=@{passed=$true;checks=$records.Count;helperSha256=$hash;cases=$records;decisionOnly=$true;nativeCleanupProven=$false}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 5))
    if((Get-Content (Join-Path $out 'result.json') -Raw|ConvertFrom-Json).checks -ne 8){throw 'Decision receipt readback differs'}
    Write-Output 'UIA_PAIR_COMPLETION PASS 8 decision-only checks'
}finally{$hold.Dispose()}
