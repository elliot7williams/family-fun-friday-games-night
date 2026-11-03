$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$out='I:/LeaBuildStorage/private-binding-audit-20261102-v1'
if(Test-Path $out){throw 'Preserve private binding audit'}
$source='I:/LeaBuildStorage/private-binding-source-20261102-v1/source.json'
$build='I:/LeaBuildStorage/private-binding-build-20261102-v1/result.json'
$native=Join-Path $root 'continuity-evidence/private-binding-comparison-v1/result.json'
foreach($pin in @(@($source,'9CD78B418ED5CE1216123B7093CD0CB3593EF0D7812414FF4D35D718CC266BA2'),@($build,'D32732C0BF3B548DB649966100D4B074663E07544BC9C25A648A80D6723AB02C'),@($native,'2D4758CB6BCC81CB2DAE266A7BB55F07CA49D92E66182A47FA9FC19651899CF4'))){if((Get-FileHash $pin[0]).Hash -cne $pin[1]){throw 'Pinned minification receipt differs'}}
$s=Get-Content $source -Raw|ConvertFrom-Json
$b=Get-Content $build -Raw|ConvertFrom-Json
$n=Get-Content $native -Raw|ConvertFrom-Json
if(!$s.verified -or !$s.structuralAstVerified -or !$s.deterministic -or $s.negativeControls -ne 4 -or $s.lexicalEquivalenceProven -or $s.hostPolicyChanged -or $s.originalBytes -ne 27635 -or $s.minifiedBytes -ne 21342 -or $s.savedBytes -ne 6293 -or $s.options.compress -ne $false -or !$s.options.keep_fnames -or !$s.options.keep_classnames -or $s.options.mangle.eval -or $s.toolVersion -cne '5.44.1'){throw 'Private minification boundary differs'}
foreach($file in $s.files){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-FileHash $file.original).Hash -cne $file.originalSha256){throw 'Minification input/output changed'}}
if((Get-FileHash (Join-Path $root 'Lea.Build/Minify-PrivateBindings.cjs')).Hash -cne $s.scriptSha256 -or (Get-FileHash 'I:/LeaBuildStorage/javascript-tools-20261102-v1/package-lock.json').Hash -cne $s.lockSha256 -or (Get-FileHash (Join-Path $root 'Lea.Build/JavaScriptToolchain/package-lock.json')).Hash -cne $s.lockSha256){throw 'Tooling identity changed'}
$lock=Get-Content 'I:/LeaBuildStorage/javascript-tools-20261102-v1/package-lock.json' -Raw|ConvertFrom-Json -AsHashtable
if($lock.packages.'node_modules/terser'.integrity -cne 'sha512-t/R3R/n0MSwnnazuPpPNVO60LX0SKL45pyl9YlvxIdkH0Of7D5qM2EVe+yASRIlY5pZ73nclYJfNANGWPwFDZw=='){throw 'Pinned package integrity differs'}
$behavior='I:/LeaBuildStorage/private-binding-source-20261102-v1/node-behavior-v2.json'
$m=Get-Content $behavior -Raw|ConvertFrom-Json
if(!$m.passed -or $m.checks -ne 312 -or $m.cases.Count -ne 4 -or $m.apiMetadataComparisons -ne 2 -or $m.sourceProofSha256 -cne (Get-FileHash $source).Hash -or $m.testSha256 -cne (Get-FileHash 'I:/LeaBuildStorage/private-binding-source-20261102-v1/DerivedBehaviorV2.test.cjs').Hash){throw 'Behavior proof differs'}
foreach($case in $m.cases){if($case.checks -ne 78 -or $case.eagerMaps -ne 3 -or $case.finalMaps -ne 3){throw 'Behavior case differs'}}
if(!$b.passed -or !$b.inputsUnchanged -or $b.ownerAttention -or $b.sha256 -cne '406C5634638D75C66892BD95429760CB4A5C74C94277913D1DF467147E30E75F' -or (Get-FileHash $b.binary).Hash -cne $b.sha256 -or (Get-Item $b.binary).Length -ne 101376){throw 'Minified parent build differs'}
foreach($file in @('8.cs','10.cs','2.cs')){if((Get-FileHash "I:/LeaBuildStorage/private-binding-build-20261102-v1/source/Lea.Script/$file").Hash -cne (Get-FileHash "I:/LeaBuildStorage/global-install-build-20261102-v1/source/Lea.Script/$file").Hash){throw 'Host policy/bootstrap evaluation changed'}}
if(!$n.passed -or !$n.comparisonCompleted -or !$n.baselineReused -or !$n.inputsUnchanged -or $n.changedFiles.Count -or $n.ownerAttention -or $n.before.Count -ne 20 -or $n.runs.Count -ne 5 -or $n.observations.Count -ne 4 -or $n.runtimeQualified){throw 'Native comparison boundary differs'}
if(($n.observations.variant -join '|') -cne 'baseline|candidate|candidate|baseline' -or @($n.observations|Where-Object {!$_.passed -or !$_.report.ok}).Count){throw 'Fixed ABBA outcomes differ'}
foreach($item in @($b.inputs)+@($n.inputs)){if((Get-FileHash $item.path).Hash -cne $item.sha256){throw 'Build/comparison input changed'}}
foreach($file in $n.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Runtime bytes/ACL changed'}}
foreach($run in @($b.runs)+@($n.runs)){if(!$run.passed -or !$run.ownedJob -or !$run.exitObserved -or !$run.originalHandleCaptured -or !$run.childRetirementIndependentlyAudited -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or $run.timedOut -or $run.killRequested -or $run.unsafeToContinue -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Owned retirement differs'}}
New-Item -ItemType Directory -Path $out|Out-Null
$copies=[Collections.Generic.List[object]]::new();$i=0
$paths=@($source,$build,$native,$behavior)+@($n.runs|ForEach-Object {$_.stdout;$_.stderr})
foreach($from in $paths){$to=Join-Path $out ((++$i).ToString('D2')+'-'+[IO.Path]::GetFileName($from));Copy-Item -LiteralPath $from -Destination $to;$hash=(Get-FileHash $from).Hash;if((Get-FileHash $to).Hash -cne $hash){throw 'Evidence readback mismatch'};$copies.Add(@{original=$from;copy=$to;sha256=$hash})}
@{verified=$true;sourceBytesSaved=6293;parentBytesSaved=6144;nodeMockChecks=312;metadataComparisons=2;fixedNativeProbesPassed=4;allSixOwnedRunsRetired=$true;startupFixed=$false;performanceImprovementProven=$false;fullSemanticSuitePassed=$false;isolatedWorkerBuilt=$false;runtimeQualified=$false;copies=$copies}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
