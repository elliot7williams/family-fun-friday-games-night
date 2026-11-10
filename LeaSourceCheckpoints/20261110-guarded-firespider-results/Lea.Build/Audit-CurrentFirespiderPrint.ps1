param([Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$PrintSha256,[switch]$GuardedSdk)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$folder=Join-Path $root 'continuity-evidence/current-source-host-print-v1'
$out='I:\LeaBuildStorage\current-firespider-print-audit-20261110-v1'
if($GuardedSdk){$folder=Join-Path $root 'continuity-evidence/guarded-sdk-host-print-v1';$out='I:\LeaBuildStorage\guarded-sdk-host-print-audit-20261110-v1'}
if(Test-Path -LiteralPath $out){throw 'Preserve current print audit'}
$path=Join-Path $folder 'result.json'
if((Get-FileHash -LiteralPath $path).Hash -cne $PrintSha256){throw 'Print receipt differs'}
$r=Get-Content -LiteralPath $path -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.completed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.error -or $r.byteChanges.Count -or $r.aclChanges.Count -or $r.before.Count -ne 403 -or $r.runs.Count -ne 1 -or !$r.decoderPolicyAccepted){throw 'Current print scope incomplete'}
$run=$r.runs[0]
if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.originalHandleCaptured -or !$run.ownedJob -or !$run.childRetirementIndependentlyAudited -or !$run.exitObserved -or !$run.outputDrained -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne 90000 -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.captureOverflow -or $run.error){throw 'Print ownership/deadline differs'}
foreach($file in $r.inputs){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'Print input drift'}}
foreach($file in $r.before){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256 -or (Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Print byte/ACL drift'}}
$manifestPath='I:\LeaBuildStorage\current-firespider-runtime-20261110-v1\runtime-manifest.json'
$manifestHash='2836AC6BA77C2DB096C5365EED4D7B6E48D56A82A0A4DFDBEBFD2CCE3DD6E1AB';$hostHash='9513A0BD7C620589F4AB332CE19F155686C01104B1E8D99A5DECAAF1814F2586'
if($GuardedSdk){$manifestPath='I:\LeaBuildStorage\guarded-sdk-host-runtime-20261110-v1\runtime-manifest.json';$manifestHash='A566E5A5AEF6F7898D8B1DE8230D19C171FDB8F01804F58186AD96BC269FB8A0';$hostHash='1E390343125011CAA3F78582E91FAFC036E07A31F7E81B4B727ED8503FC39606'}
if((Get-FileHash -LiteralPath $manifestPath).Hash -cne $manifestHash){throw 'Current runtime manifest differs'}
$manifest=Get-Content -LiteralPath $manifestPath -Raw|ConvertFrom-Json
foreach($file in $manifest.files){$copy=Join-Path $r.runtime $file.path;$before=@($r.before|Where-Object path -CEQ $copy);if($before.Count -ne 1 -or $before[0].sha256 -cne $file.sha256){throw 'Print does not use exact qualified host runtime'}}
if((Get-FileHash -LiteralPath (Join-Path $r.runtime 'Firespider Internet 19.exe')).Hash -cne $hostHash -or (Get-FileHash -LiteralPath (Join-Path $r.runtime 'Lea.Firespider.PrintProbe.exe')).Hash -cne '84985AC08FD5A7DC018CF6AD75006633734298153D327D53834C6B37DF30839B'){throw 'Host/probe identity differs'}
$sourcePath='I:\LeaBuildStorage\current-firespider-print-source-20261110-v1\source.json'
$sourceHash='C0B08B37A0481E8793AB4850AB79ABBD0233A726052441D24D1BDDFF7E8BD577'
if($GuardedSdk){$sourcePath='I:\LeaBuildStorage\guarded-sdk-host-print-source-20261110-v1\source.json';$sourceHash='9138BF370EE43C52704DCE84EF58F9F618D5462253B5C8F26DAE6A4C4D9E2938'}
if((Get-FileHash -LiteralPath $sourcePath).Hash -cne $sourceHash){throw 'Print projection receipt differs'}
$s=Get-Content -LiteralPath $sourcePath -Raw|ConvertFrom-Json
if(!$s.verified -or !$s.inverseVerified -or $s.assertionsChanged -or $s.printBudgetMs -ne 90000 -or $s.retirementBudgetMs -ne 5000 -or $s.queueSubmissionAllowed -or (Get-FileHash -LiteralPath $s.source).Hash -cne $s.sha256){throw 'Print projection scope differs'}
$original=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-PrivateBindingHostPrint-v1.generated.ps1'))
$inverse=[IO.File]::ReadAllText($s.source).Replace(("'"+$PSScriptRoot.Replace("'","''")+"'"),'$PSScriptRoot')
for($i=$s.replacements.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($s.replacements[$i][1],$s.replacements[$i][0])}
if($inverse -cne $original){throw 'Original print assertions changed'}
$lines=@(Get-Content -LiteralPath $run.stdout)
foreach($footer in @('PASS Lea browser print preview: click/hover/modal/reentrancy/Escape/detached lifetime/privacy/image and busy guards','PASS Lea browser print command: click/hover/cancel/native-owner/reentry/changed-tab/busy/unknown guards; no queue submission')){if(@($lines|Where-Object {$_ -ceq $footer}).Count -ne 1){throw 'Exact print/preview footer differs'}}
New-Item -ItemType Directory -Path $out|Out-Null
$specs=@(@{folder=$folder;prefix='run'},@{folder=(Split-Path $sourcePath -Parent);prefix='source'});if(!$GuardedSdk){$specs+=@{folder='I:\LeaBuildStorage\current-firespider-print-preflight-20261110-v1';prefix='preflight'}}
$copies=@(foreach($spec in $specs){foreach($file in Get-ChildItem -LiteralPath $spec.folder -File|Where-Object Extension -in @('.json','.txt','.ps1')){$to=Join-Path $out ($spec.prefix+'-'+$file.Name);Copy-Item -LiteralPath $file.FullName -Destination $to;$hash=(Get-FileHash -LiteralPath $file.FullName).Hash;if((Get-FileHash -LiteralPath $to).Hash -cne $hash){throw 'Print evidence readback differs'};@{path=$to;sha256=$hash}}})
@{verified=$true;printSha256=$PrintSha256;hostSha256=$manifest.hostSha256;runtimeMembers=403;ownedRuns=1;printCommandPassed=$true;previewPassed=$true;queueSubmitted=$false;physicalPrintingQualified=$false;runtimeBytesAndAclStable=$true;assertionsChanged=$false;limitsChanged=$false;startupReliabilityFixed=$false;hostAdopted=$false;releaseQualified=$false;copies=$copies;scriptSha256=(Get-FileHash -LiteralPath $PSCommandPath).Hash}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'audit.json')|Format-List Hash
