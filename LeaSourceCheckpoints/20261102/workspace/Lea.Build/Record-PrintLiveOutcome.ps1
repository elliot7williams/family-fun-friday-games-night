$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$root=Join-Path $workspace 'continuity-evidence/print-live-native-v1'
$out=Join-Path $root 'bound-outcome.json'
if(Test-Path $out){throw 'Preserve outcome'}
$pins=@{
    focused=@('continuity-evidence/print-live-capture-v1/result.json','751F1EF75A7E184DFE245D1DEFE7732EBBB4A58C5E34AD284DFB716AAE57F123')
    native=@('continuity-evidence/print-live-native-v1/result.json','0978357E67724437D6D2B80F834061A90EB85C5E3AFF4CF8886410D50FD3730D')
    classification=@('continuity-evidence/print-live-native-v1/acl-observation.json','19CEB20C031B7A2888A386043582E00F515E793BF610AAC9E0920E8D5C3BC285')
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
$view='568A26396DA540C56546447DDC6277878ED5A5A71AF9712559BF8E3A894DADF8'
foreach($folder in @('print-live-capture-v1','print-live-native-v1/runtime')){
    foreach($name in @('Lea.Engine.dll','Lea.Win.dll')){
        $expected=if($name -ceq 'Lea.Engine.dll'){$core}else{$view}
        if((Get-FileHash (Join-Path $workspace ('continuity-evidence/'+$folder+'/'+$name))).Hash -cne $expected){throw 'Core/view pairing differs'}
    }
}
@{focusedPassed=$true;nativeFunctionalPassed=$true;liveStyledCaptureSubsetPassed=$true;knownPersistentGrantClassificationOnly=$true;rawAclAuditPassed=$false;filesystemAclModified=$false;policyBroadened=$false;profileDeletionProven=$false;fullScriptingMatched=$false;hostAdopted=$false;releaseCertified=$false;coreSha256=$core;viewSha256=$view;receipts=$pins;scope='Exact focused and native core/view result binding; not raw-audit green or full compatibility/security/printing release qualification'}|ConvertTo-Json -Depth 5|Set-Content $out
Get-FileHash $out
