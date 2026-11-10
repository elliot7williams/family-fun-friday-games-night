$ErrorActionPreference='Stop'
$out='I:\LeaBuildStorage\memory-script-policy-audit-20261110-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve policy audit'}
$specs=@(
 @{folder='I:\LeaBuildStorage\memory-script-policy-negative-20261110-v1';hash='9A8286E821DA1F5953EFAB6CD36194161C8246141E4D74501434F92C46E5BDD9';negative=$true;budgets=@(60000,30000)},
 @{folder='I:\LeaBuildStorage\memory-script-policy-patched-20261110-v1';hash='8C04272AAB9A8079AD71D5AE834692893DD47E3F4AF45AAE59D9C2789F5F0ACF';negative=$false;budgets=@(60000,30000)},
 @{folder='I:\LeaBuildStorage\memory-document-policy-view-20261110-v1';hash='4E40211A9FD553BA9F2536ADB24BE05BCAE0B35CB39AB4DD50142A5AE0E2FDE8';negative=$false;budgets=@(120000,60000,30000,120000,60000,30000)}
)
foreach($spec in $specs){
 $path=Join-Path $spec.folder 'result.json';if((Get-FileHash -LiteralPath $path).Hash -cne $spec.hash){throw 'Policy receipt differs'}
 $r=Get-Content -LiteralPath $path -Raw|ConvertFrom-Json
 if($r.passed -eq $spec.negative -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.runs.Count -ne $spec.budgets.Count){throw 'Policy receipt boundary incomplete'}
 for($i=0;$i -lt $r.runs.Count;$i++){$run=$r.runs[$i];$exit=if($spec.negative -and $i -eq 1){1}else{0};if($run.exitCode -ne $exit -or !$run.originalHandleCaptured -or !$run.ownedJob -or !$run.exitObserved -or !$run.childRetirementIndependentlyAudited -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.timedOut -or $run.killRequested -or $run.unsafeToContinue -or $run.parentBudgetMs -ne $spec.budgets[$i] -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Owned policy run differs'}}
 foreach($f in $r.inputs){if((Get-FileHash -LiteralPath $f.path).Hash -cne $f.sha256){throw 'Policy input drift'}}
 foreach($f in @($r.before)){if($null -ne $f -and ((Get-FileHash -LiteralPath $f.path).Hash -cne $f.sha256 -or (Get-Acl -LiteralPath $f.path).Sddl -cne $f.sddl)){throw 'Policy runtime bytes/ACL drift'}}
 if($r.byteChanges.Count -or $r.aclChanges.Count){throw 'Policy runtime changed'}
}
$negative=Get-Content -LiteralPath (Join-Path $specs[0].folder 'memory-script-policy.stdout.txt') -Raw
$positive=Get-Content -LiteralPath (Join-Path $specs[1].folder 'memory-script-policy.stdout.txt') -Raw
foreach($line in @('OBSERVE automatic title=AUTO_RAN; commands=2; alerts=1','OBSERVE manual commands=1','OBSERVE resize scripting=True; toggle scripting=True')){if(!$negative.Contains($line)){throw 'Negative reproduction evidence missing'}}
foreach($line in @('OBSERVE automatic title=Safe memory; commands=0; alerts=0','OBSERVE manual commands=0','OBSERVE resize scripting=False; toggle scripting=False','PASS Lea memory script policy: 7 checks')){if(!$positive.Contains($line)){throw 'Patched policy evidence missing'}}
$old='I:\LeaBuildStorage\memory-document-view-20261110-v1\source'
$new='I:\LeaBuildStorage\memory-document-policy-view-20261110-v1\source'
$pairs=@(@('p.Je = je;','p.Je = je && !p.Mm;'),@('page.Je = je;','page.Je = je && !page.Mm;'),@('MediaRules(rules, vw, vh, dp, th, rm, pc, je ?','MediaRules(rules, vw, vh, dp, th, rm, pc, p.Je ?'),@('MediaRules(all, vw, vh, dp, th, rm, pc, je ?','MediaRules(all, vw, vh, dp, th, rm, pc, page.Je ?'),@('if (!je) return;','if (!p.Je) return;'),@('if (!je) return 0;','if (!je || page.Mm) return 0;'))
foreach($folder in @('Lea.Engine','Lea.Win')){foreach($f in Get-ChildItem -LiteralPath (Join-Path $old $folder) -File){$path=Join-Path $new ($folder+'/'+$f.Name);if($f.Name -eq 'E.cs'){$inverse=[IO.File]::ReadAllText($path);foreach($pair in $pairs){if([regex]::Matches($inverse,[regex]::Escape($pair[1])).Count -ne 1){throw 'Unique policy fix boundary differs'};$inverse=$inverse.Replace($pair[1],$pair[0])};if($inverse -cne [IO.File]::ReadAllText($f.FullName)){throw 'Changes beyond six policy guards'}}elseif((Get-FileHash -LiteralPath $path).Hash -cne (Get-FileHash -LiteralPath $f.FullName).Hash){throw 'Unrelated core/view source changed'}}}
New-Item -ItemType Directory -Path $out|Out-Null
$copies=@();$i=0
foreach($spec in $specs){foreach($f in Get-ChildItem -LiteralPath $spec.folder -File|Where-Object Extension -in @('.json','.txt','.cs','.ps1')){$to=Join-Path $out ((++$i).ToString('D2')+'-'+$f.Name);Copy-Item -LiteralPath $f.FullName -Destination $to;$hash=(Get-FileHash -LiteralPath $f.FullName).Hash;if((Get-FileHash -LiteralPath $to).Hash -cne $hash){throw 'Policy evidence copy mismatch'};$copies+=@{path=$to;sha256=$hash}}}
@{verified=$true;negativeReproduced=$true;policyChecks=7;coreModelChecks=12;viewChecks=17;sixGuardOnlySourceInverse=$true;coreSha256='11E2190C797233789BCEC1830382881563CD55C62378F57D1E639E4791F203DD';viewSha256='71F4ED73AF9247317A3E5A385992139A145251F137C79D855307031632443567';hostAdopted=$false;completeSecurityQualified=$false;copies=$copies;scriptSha256=(Get-FileHash -LiteralPath $PSCommandPath).Hash}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'audit.json')
