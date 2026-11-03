$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'New-BootstrapCostProgram.ps1')
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$out=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/bootstrap-cost-overlay-tests-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve overlay tests'}
$null=New-Item -ItemType Directory -Path $out
$path=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/numeric-references-script-suite-v1/source/Lea.Script.Tests/Program.cs'
if((Get-FileHash -LiteralPath $path).Hash -cne '132A2F90B895C9650AA4B8D5B85E307CC2A7AE10BE77363082178F369814322E'){throw 'Suite source differs'}
$original=[IO.File]::ReadAllText($path)
$call='var execution = script.Ex(code);'
$check='Check(execution.Ok && execution.N == 6, "computed script: " + execution.S + "; " + execution.M);'
$overlay=New-BootstrapCostProgram $original
$rows=[Collections.Generic.List[object]]::new()
$rows.Add(@{case='real-source';passed=$overlay.inverseVerified -and $overlay.assertionPreserved -and $overlay.edits -eq 2 -and $overlay.stopCount -eq 1})
foreach($test in @(
    @{name='missing-call';text=$original.Replace($call,'')},
    @{name='duplicate-call';text=$original+"`r`n"+$call},
    @{name='missing-assertion';text=$original.Replace($check,'')},
    @{name='duplicate-assertion';text=$original+"`r`n"+$check},
    @{name='repeated-overlay';text=$overlay.text}
)){
    $rejected=$false
    try{$null=New-BootstrapCostProgram $test.text}catch{$rejected=$_.Exception.Message -ceq 'Diagnostic overlay shape differs'}
    $rows.Add(@{case=$test.name;passed=$rejected})
}
$result=@{passed=@($rows|Where-Object {!$_.passed}).Count -eq 0;checks=$rows;compilerExecuted=$false;runtimeExecuted=$false}
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 4))
[pscustomobject]$result|ConvertTo-Json -Depth 4
