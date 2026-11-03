param([ValidateSet('Removal','Tree')][string]$Target='Removal')
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace ('continuity-evidence/full-'+$Target.ToLowerInvariant()+'-observer-source-controls-v1')
if(Test-Path $out){throw 'Preserve source controls'}
. (Join-Path $PSScriptRoot 'New-WorkerStartupObservationSource.ps1')
$path=Join-Path $workspace ('continuity-evidence/temporal-startup-v1/current-native-script-suite-v4/source/Lea.Script.Tests/'+$Target+'.cs')
$original=[IO.File]::ReadAllText($path)
$candidate=New-WorkerStartupObservationSource $original
$rows=[Collections.Generic.List[object]]::new()
if(!$candidate.inverseVerified -or !$candidate.originalAssertionsPreserved){throw 'Inverse proof missing'}
$rows.Add(@{name='real-source-inverse';passed=$true})
foreach($case in @(
    @{name='wrong-startup-budget';text=$original.Replace('W.On(view, null, 5000);','W.On(view, null, 6000);')},
    @{name='duplicate-startup-config';text=($original+"`n"+'                W.On(view, null, 5000);')},
    @{name='missing-await';text=$original.Replace('                var page = await view.GoA(fixture);','')},
    @{name='duplicate-await';text=($original+"`n"+'                var page = await view.GoA(fixture);')},
    @{name='repeated-overlay';text=$candidate.text}
)){
    $rejected=$false
    try{$null=New-WorkerStartupObservationSource $case.text}catch{$rejected=$_.Exception.Message -in @('Startup observation already applied','Startup observation anchor differs')}
    if(!$rejected){throw ('Source control failed: '+$case.name)}
    $rows.Add(@{name=$case.name;passed=$true})
}
$null=New-Item -ItemType Directory $out
@{passed=$true;controls=$rows;runtimeExecuted=$false;inputs=@(Get-FileHash -LiteralPath $path,$PSCommandPath,(Join-Path $PSScriptRoot 'New-WorkerStartupObservationSource.ps1')|Select-Object Path,Hash)}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'result.json')
@{passed=$true;controls=$rows.Count}|ConvertTo-Json
