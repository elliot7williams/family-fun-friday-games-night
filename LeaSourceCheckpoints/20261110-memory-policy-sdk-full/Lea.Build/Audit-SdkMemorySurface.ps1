$ErrorActionPreference='Stop'
$out='I:\LeaBuildStorage\memory-policy-sdk-memory-audit-20261110-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve SDK memory audit'}
$partial='I:\LeaBuildStorage\memory-policy-sdk-memory-20261110-v1'
$resume='I:\LeaBuildStorage\memory-policy-sdk-policy-resume-20261110-v1'
$pPath=Join-Path $partial 'result.json';$rPath=Join-Path $resume 'result.json'
if((Get-FileHash -LiteralPath $pPath).Hash -cne '859E8E8910AEEEC4559050DA6323239E365E82130E2FCF59880950BCF5108CDA' -or (Get-FileHash -LiteralPath $rPath).Hash -cne 'FF94C29F1F28DABD0861FE6CA477188EC6CAABDA5FA80AC0091BA9A415E6987B'){throw 'SDK memory receipts differ'}
$p=Get-Content -LiteralPath $pPath -Raw|ConvertFrom-Json;$r=Get-Content -LiteralPath $rPath -Raw|ConvertFrom-Json
if($p.passed -or $p.completed -or $p.runs.Count -ne 5 -or $p.error -cne 'System.Management.Automation.RuntimeException: Storage reserve required' -or !$r.passed -or $r.runs.Count -ne 1 -or $r.partialSha256 -cne '859E8E8910AEEEC4559050DA6323239E365E82130E2FCF59880950BCF5108CDA' -or $r.freeCAtResume -lt 650MB -or $r.causeOfSpaceDropProven){throw 'Storage-stop/resume boundary differs'}
$expected=@('compile-sdk-memory-model','compile-sdk-memory-view','compile-sdk-memory-policy','sdk-memory-model','sdk-memory-view','sdk-memory-policy')
$runs=@($p.runs)+@($r.runs)
for($i=0;$i -lt $runs.Count;$i++){$run=$runs[$i];$budget=if($i -lt 3){60000}else{30000};if($run.name -cne $expected[$i] -or !$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.ownedJob -or !$run.originalHandleCaptured -or !$run.childRetirementIndependentlyAudited -or !$run.exitObserved -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000 -or $run.unsafeToContinue){throw 'SDK memory owned run differs'}}
foreach($record in @($p,$r)){
 if(!$record.inputsUnchanged -or $record.ownerAttention -or $record.before.Count -ne 5 -or $record.byteChanges.Count -or $record.aclChanges.Count){throw 'SDK memory runtime boundary differs'}
 foreach($f in $record.inputs){if((Get-FileHash -LiteralPath $f.path).Hash -cne $f.sha256){throw 'SDK memory input drift'}}
 foreach($f in $record.before){if((Get-FileHash -LiteralPath $f.path).Hash -cne $f.sha256 -or (Get-Acl -LiteralPath $f.path).Sddl -cne $f.sddl){throw 'SDK memory runtime drift'}}
}
foreach($f in $p.before){$same=@($r.before|Where-Object path -CEQ $f.path);if($same.Count -ne 1 -or $same[0].sha256 -cne $f.sha256 -or $same[0].sddl -cne $f.sddl){throw 'Resume changed tested runtime'}}
foreach($pair in @(@($runs[3].stdout,'PASS Lea memory documents: 12 model checks'),@($runs[4].stdout,'PASS Lea memory view: 17 checks at720/360; no HTML in isolated TEMP'),@($runs[5].stdout,'PASS Lea memory script policy: 7 checks'))){if(@((Get-Content -LiteralPath $pair[0])|Where-Object {$_ -ceq $pair[1]}).Count -ne 1){throw 'SDK memory footer missing'}}
foreach($pair in @(@('Lea.Engine.dll','B11754880B8F767C789D1385DE2702FF15EAD92CFDBECB1CE02DC145F28F3DD5'),@('Lea.Win.dll','C83E47CC6787C8814E36383B83AD0D2DC5E2726ABF3086678AEA6DC0F4F19C22'))){if((Get-FileHash -LiteralPath (Join-Path $p.runtime $pair[0])).Hash -cne $pair[1]){throw 'SDK memory product pair differs'}}
New-Item -ItemType Directory -Path $out|Out-Null
$copies=@();foreach($spec in @(@{folder=$partial;prefix='partial'},@{folder=$resume;prefix='resume'})){foreach($f in Get-ChildItem -LiteralPath $spec.folder -File){$to=Join-Path $out ($spec.prefix+'-'+$f.Name);Copy-Item -LiteralPath $f.FullName -Destination $to;$h=(Get-FileHash -LiteralPath $f.FullName).Hash;if((Get-FileHash -LiteralPath $to).Hash -cne $h){throw 'SDK memory evidence copy differs'};$copies+=@{path=$to;sha256=$h}}}
@{verified=$true;modelChecks=12;viewChecks=17;policyChecks=7;ownedRuns=6;completedAcrossStorageResume=$true;originalPartialRewritten=$false;runtimeBytesAndAclsStable=$true;coreSha256='B11754880B8F767C789D1385DE2702FF15EAD92CFDBECB1CE02DC145F28F3DD5';viewSha256='C83E47CC6787C8814E36383B83AD0D2DC5E2726ABF3086678AEA6DC0F4F19C22';hostAdopted=$false;fullScriptSuiteQualified=$false;globalSecurityQualified=$false;copies=$copies;scriptSha256=(Get-FileHash -LiteralPath $PSCommandPath).Hash}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'audit.json')
