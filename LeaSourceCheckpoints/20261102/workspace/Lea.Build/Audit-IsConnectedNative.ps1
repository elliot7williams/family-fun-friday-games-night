$ErrorActionPreference='Stop'
$folder=Join-Path (Split-Path $PSScriptRoot) 'continuity-evidence/is-connected-native-v1'
$out='I:\LeaBuildStorage\is-connected-native-audit-20261102-v1'
if(Test-Path $out){throw 'Preserve native connectivity audit'}
$receipt=Join-Path $folder 'result.json'
if((Get-FileHash $receipt).Hash -cne 'E7692D6487E00789EF94D591B8EA83D9E4331C7B34FB2D6A9BBEEC3A3AEA08BF'){throw 'Native receipt differs'}
$r=Get-Content $receipt -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.before.Count -ne 18 -or $r.runs.Count -ne 2){throw 'Native campaign incomplete'}
foreach($run in $r.runs){
    if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.originalHandleCaptured -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.sharedRetirementDrainBudgetMs -ne 5000){throw 'Owned native boundaries not met'}
}
if($r.runs[0].parentBudgetMs -ne 120000 -or $r.runs[1].parentBudgetMs -ne 30000){throw 'Native test budgets differ'}
foreach($file in $r.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Native input changed'}}
foreach($file in $r.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Native runtime bytes/ACL differ'}}
if(@(Get-Content $r.runs[1].stdout|Where-Object {$_.StartsWith('PASS Lea isConnected native:')}).Count -ne 1){throw 'Unique native footer required'}
foreach($name in @('connected-desktop.png','detached-desktop.png','detached-narrow.png')){if(!(Test-Path (Join-Path $folder $name))){throw 'Native render evidence missing'}}
New-Item -ItemType Directory -Path $out|Out-Null
$copies=[Collections.Generic.List[object]]::new()
foreach($file in Get-ChildItem $folder -File){Copy-Item -LiteralPath $file.FullName -Destination $out;$hash=(Get-FileHash $file.FullName).Hash;if((Get-FileHash (Join-Path $out $file.Name)).Hash -cne $hash){throw 'Native evidence copy differs'};$copies.Add(@{name=$file.Name;sha256=$hash})}
@{evidenceVerified=$true;nativeRuntimePassed=$true;runtimeFiles=18;files=$copies;hostAdopted=$false;scope='Single isConnected native-view lifecycle/activation/retirement case; images retained, manual visual review recorded in feature documentation. Not prior DOM/full scripting/browser release qualification.'}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
