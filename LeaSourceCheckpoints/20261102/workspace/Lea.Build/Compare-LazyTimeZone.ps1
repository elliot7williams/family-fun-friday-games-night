$ErrorActionPreference='Stop'
$root=Join-Path $PSScriptRoot '../continuity-evidence/temporal-startup-v1'
$out=Join-Path $root 'comparison.json'
if(Test-Path $out){throw 'Preserve previous comparison'}
$records=@()
foreach($name in @('control','lazy')){
    $path=Join-Path $root ('contract-'+$name+'.json')
    if((Get-FileHash $path).Hash -cne 'B43050C1A515863128E7F667E55E74931E019CE5DFC9F334B579FF071621EFA7'){throw 'Contract receipt differs'}
    $record=Get-Content $path -Raw|ConvertFrom-Json -AsHashtable
    if(!$record.passed -or $record.outputs.Count -ne 10 -or $record.tableCount -ne 216){throw 'Contract incomplete'}
    $records+=,$record
}
foreach($key in $records[0].outputs.Keys){
    if($records[1].outputs.Keys -cnotcontains $key -or $records[0].outputs[$key] -cne $records[1].outputs[$key]){throw ('Public lookup mismatch: '+$key)}
}
@{passed=$true;publicLookupCases=10;tableEntries=216;workerExecuted=$false;
  scope='Seven conditional candidate assertions and control/candidate functional output comparison only; control labels do not assert lazy behavior. Date/Temporal JS, native and performance verification remain pending.'}|ConvertTo-Json|Set-Content $out
Get-Content $out
