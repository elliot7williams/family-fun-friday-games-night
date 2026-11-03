$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$root=Join-Path $workspace 'continuity-evidence/print-styles-native-v1'
$out=Join-Path $root 'bound-outcome.json'
if(Test-Path $out){throw 'Preserve outcome'}
$pins=@{
    focused=@('continuity-evidence/print-styles-v3/result.json','445574DF56FB88273F314D07496DF63656C7BCAB4F448AF88463F29DA0750188')
    native=@('continuity-evidence/print-styles-native-v1/result.json','01C7672A37F2EE7947358C832ABA8BCE87CEB68EE3D5BA686478E4119826721E')
    classification=@('continuity-evidence/print-styles-native-v1/acl-observation.json','05334BEF379AE4092F6C49DC2CD91C7D45BA1DF3B8AED7180783194784E15249')
}
$proof=@{}
foreach($name in $pins.Keys){$path=Join-Path $workspace $pins[$name][0];if((Get-FileHash $path).Hash -cne $pins[$name][1]){throw 'Receipt differs'};$proof[$name]=Get-Content $path -Raw|ConvertFrom-Json}
$f=$proof.focused;$n=$proof.native;$c=$proof.classification
if(!$f.passed -or !$f.inputsUnchanged -or $f.ownerAttention -or $f.runs.Count -ne 4){throw 'Focused incomplete'}
if($n.passed -or !$n.behaviorPassed -or !$n.fullSuiteFooter -or !$n.inputsUnchanged -or !$n.bytesUnchanged -or !$n.fixtureBytesUnchanged -or $n.ownerAttention -or $n.runs.Count -ne 1 -or $n.fixtureBefore.Count -ne 179){throw 'Native proof incomplete'}
if(!$c.classified -or $c.observations.Count -ne 3 -or $c.rawAuditPassed -or $c.filesystemAclModified -or $c.productionPolicyBroadened){throw 'Classification differs'}
foreach($run in @($f.runs)+@($n.runs)){
    if(!$run.passed -or !$run.originalHandleCaptured -or !$run.exitObserved -or $run.exitCode -ne 0 -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.timedOut -or $run.killRequested -or $run.unsafeToContinue -or $run.sharedRetirementDrainBudgetMs -ne 5000){throw 'Owned exit/output proof differs'}
}
if($n.runs[0].parentBudgetMs -ne 300000 -or $f.runs[3].parentBudgetMs -ne 30000){throw 'Budgets differ'}
foreach($pin in @($f.inputs)+@($n.inputs)){if((Get-FileHash $pin.path).Hash -cne $pin.sha256){throw 'Input changed'}}
foreach($pin in @($f.before)+@($n.before)){if((Get-FileHash $pin.path).Hash -cne $pin.sha256){throw 'Binary changed'}}
$core='A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791'
foreach($relative in @('continuity-evidence/print-styles-v3/Lea.Engine.dll','continuity-evidence/print-styles-native-v1/runtime/Lea.Engine.dll')){if((Get-FileHash (Join-Path $workspace $relative)).Hash -cne $core){throw 'Core pairing differs'}}
@{focusedPassed=$true;nativeFunctionalPassed=$true;knownPersistentGrantClassificationOnly=$true;rawAclAuditPassed=$false;filesystemAclModified=$false;policyBroadened=$false;profileDeletionProven=$false;fullScriptingMatched=$false;styleRendererIntegrated=$false;hostAdopted=$false;releaseCertified=$false;coreSha256=$core;receipts=$pins;scope='Exact focused and native result binding; not raw-audit green or full compatibility/security/printing release qualification'}|ConvertTo-Json -Depth 5|Set-Content $out
Get-FileHash $out
