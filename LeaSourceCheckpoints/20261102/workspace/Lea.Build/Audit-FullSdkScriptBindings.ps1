$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\full-sdk-script-bindings-audit-20261101-v1'
if(Test-Path $out){throw 'Preserve binding audit'}
$worker='5BF8B589731CB1A0A5DE38B0DAE1AF516DBF21DB5982141C9C013C55E6D39596'
$old='60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'
$holds=[Collections.Generic.List[IDisposable]]::new();$pins=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$hash){
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$actual=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($hash -and $actual -cne $hash){throw ('Binding audit identity differs: '+$path)};$pins.Add(@{path=$path;sha256=$actual})
}
function Proof([string]$path,[string]$hash){Pin $path $hash;return Get-Content $path -Raw|ConvertFrom-Json}
try{
    $sources=Proof 'I:\LeaBuildStorage\sdk-component-probe-sources-20261101-v3/sources.json' '32A7635DA8C3C6566451F0445B895BD80052796F4CEBCFD968D7989182EF4526'
    if(!$sources.verified -or $sources.assertionsChanged -or $sources.files.Count -ne 6 -or $sources.selectedWorkerSha256 -cne $worker){throw 'Projected probe source coverage differs'}
    foreach($file in $sources.files){
        Pin $file.path $file.sha256;Pin $file.origin $file.originalSha256
        if(!$file.inverseVerified -or $file.negativeControls -ne 3 -or [IO.File]::ReadAllText($file.path).Replace($worker,$old) -cne [IO.File]::ReadAllText($file.origin)){throw 'Hash-only probe text inverse differs'}
    }
    $entrySource=Proof 'I:\LeaBuildStorage\sdk-entry-core-source-20261101-v2/source.json' '55499C1D1959A08377AD2648DA9ECD29272FBD5FDB5908930A4186683829ACD9'
    Pin $entrySource.source $entrySource.sha256;Pin $entrySource.origin $entrySource.originalSha256
    if(!$entrySource.verified -or !$entrySource.inverseVerified -or $entrySource.negativeControls -ne 3 -or $entrySource.identityReplacements -ne 1 -or $entrySource.assertionsChanged -or [IO.File]::ReadAllText($entrySource.source).Replace($worker,$old) -cne [IO.File]::ReadAllText($entrySource.origin)){throw 'Entry source inverse differs'}
    $probes=Proof 'I:\LeaBuildStorage\sdk-component-probes-20261101-v3/result.json' '436983AEF289AC8B205F8878FBF7BD31FCFF079FB14F9B99B6D3DCB26F7FC953'
    $entry=Proof 'I:\LeaBuildStorage\sdk-entry-core-20261101-v2/result.json' '6B7FC1DE3C6A425F07146E244BC31CEEA5390C63195E0969735533B65CE85F72'
    $runtime=Proof (Join-Path $root 'continuity-evidence/sdk-component-probes-runtime-v3/result.json') '49904DB790CB7286FC8DC1887B5793616D5D649EC2BEAF9866195FD90EDF3F86'
    foreach($closure in @(@($probes,5,120000),@($entry,1,120000),@($runtime,5,30000))){
        $r=$closure[0]
        if(!$r.passed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.error -or $r.runs.Count -ne $closure[1]){throw 'Passing retired projection campaign required'}
        foreach($file in $r.inputs){Pin $file.path $file.sha256}
        foreach($run in $r.runs){if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or $run.parentBudgetMs -ne $closure[2] -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or !$run.exitObserved -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Original compiler/runtime retirement guards failed'}}
        foreach($file in $r.outputs){Pin $file.path $file.sha256}
    }
    if(!$probes.dispatcherInverseVerified -or $probes.dispatcherChanges.Count -ne 5 -or $probes.workerModule.sha256 -cne $worker -or $runtime.workerEntryPointExecuted -or $runtime.pageCodeEvaluated -or $runtime.nativeIsolationTest -or $runtime.changedFiles.Count -or $runtime.reports.Count -ne 5){throw 'Probe scope or dispatcher closure differs'}
    foreach($file in $runtime.before){Pin $file.path $file.sha256;if((Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Current probe ACL differs'}}
    $counts=@{markers=21;frames=24;clock=15;wire=93;bootstrap=14}
    foreach($report in $runtime.reports){if(!$counts.ContainsKey($report.component) -or $report.checks -ne $counts[$report.component] -or $report.componentSha256 -cne $worker){throw 'Exact component report scope differs'}}
    $bindingRoot='I:\LeaBuildStorage\sdk-full-script-bindings-20261101-v3'
    $bindings=Proof (Join-Path $bindingRoot 'bindings.json') '9E759782A6A76F56C28DE08CB0237E35657CBAFDBBB0C0CA0317A72BFBA34EE5'
    if(!$bindings.verified -or !$bindings.notRunnable -or $bindings.assertionsChanged -or $bindings.sourceChanges.Count -ne 4 -or $bindings.componentManifestsRebound -ne 5 -or $bindings.workerHashConstantsRebound -ne 2 -or $bindings.entryFixtureFiles -ne 7 -or !$bindings.entryHostCompilationPending -or !$bindings.entryManifestRebindingPending -or $bindings.requiredChecks -ne 3412 -or $bindings.requiredPhases -ne 89 -or $bindings.testBudgetMs -ne 300000){throw 'Partial full-suite binding boundary differs'}
    foreach($file in $bindings.files){Pin (Join-Path $bindingRoot $file.path) $file.sha256}
    foreach($change in $bindings.sourceChanges){if(!$change.inverseVerified){throw 'Binding source inverse not verified'}}
    foreach($name in $counts.Keys){
        $spec=Get-Content (Join-Path $bindingRoot ('runtime/ComponentProbes/'+$name+'/manifest.json')) -Raw|ConvertFrom-Json -AsHashtable
        if($spec.files['Lea.Script.Worker.dll'] -cne $worker -or $spec.checks -ne $counts[$name]){throw 'Partial runtime module binding differs'}
    }
    New-Item -ItemType Directory -Path $out|Out-Null
    @{verified=$true;inputs=$pins;workerSha256=$worker;componentGroups=5;projectedProbeSources=6;compiledEntryCompanion=$true;componentOnly=$true;workerEntryPointExecuted=$false;entryCompanionRuntimeQualified=$false;fullSuiteAssembled=$false;fullSuitePassed=$false;hostAdopted=$false;completeSdk=$false}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'result.json')
}finally{foreach($hold in $holds){$hold.Dispose()}}
