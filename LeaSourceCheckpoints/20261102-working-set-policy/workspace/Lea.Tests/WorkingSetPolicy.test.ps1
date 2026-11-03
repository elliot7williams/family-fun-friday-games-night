$ErrorActionPreference='Stop'
. (Join-Path (Split-Path $PSScriptRoot) 'Lea.Build/Read-WorkingSetPolicy.ps1')
$sample=[pscustomobject]@{pid=5;minimumBytes=204800;maximumBytes=1413120;flags=10;hardMinimum=$false;hardMaximum=$false}
$r=Read-WorkingSetPolicy $sample $sample 5
if(!$r.verified -or !$r.unchanged -or $r.hardMaximumObserved){throw 'Soft policy invalid'}
$hard=$sample|ConvertTo-Json|ConvertFrom-Json;$hard.flags=6;$hard.hardMaximum=$true
$r=Read-WorkingSetPolicy $sample $hard 5
if($r.unchanged -or !$r.hardMaximumObserved){throw 'Policy transition invalid'}
$checks=2
foreach($case in @('identity','bounds','negative','contradictory','decode','type','missing')){
    $bad=$sample|ConvertTo-Json|ConvertFrom-Json
    switch($case){
        identity {$bad.pid=6}
        bounds {$bad.minimumBytes=$bad.maximumBytes+1}
        negative {$bad.flags=-1}
        contradictory {$bad.flags=12;$bad.hardMaximum=$true}
        decode {$bad.hardMaximum=$true}
        type {$bad.hardMinimum='false'}
        missing {$bad.PSObject.Properties.Remove('pid')}
    }
    $rejected=$false;try{$null=Read-WorkingSetPolicy $sample $bad 5}catch{$rejected=$true}
    if(!$rejected){throw "Invalid policy accepted: $case"};$checks++
}
"PASS working-set policy parser: $checks cases"
