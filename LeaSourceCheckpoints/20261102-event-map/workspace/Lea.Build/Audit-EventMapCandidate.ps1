$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$out='I:/LeaBuildStorage/event-map-audit-20261102-v1'
if(Test-Path $out){throw 'Preserve event map audit'}
$source='I:/LeaBuildStorage/event-map-source-20261102-v1/source.json'
$build='I:/LeaBuildStorage/event-map-build-20261102-v1/result.json'
$native=Join-Path $root 'continuity-evidence/event-map-trusted-v1/result.json'
foreach($pin in @(@($source,'FE6451DEA2C77AF80A4D43020D5F6CEE8B7EFE47C24C32AB61ABF070F4CFC0F3'),@($build,'4EB46F1448E476A466B8DE01A179DF4EFDBA7DF8738DC5E00EE67878935F4202'),@($native,'D8D271F5B2BE4CCEAFD0859CAA53F8862ED2E962FD7F8551EF112EDDF6C980E8'))){if((Get-FileHash $pin[0]).Hash -cne $pin[1]){throw 'Pinned receipt differs'}}
$s=Get-Content $source -Raw|ConvertFrom-Json
$b=Get-Content $build -Raw|ConvertFrom-Json
$n=Get-Content $native -Raw|ConvertFrom-Json
$nodePath='I:/LeaBuildStorage/event-map-source-20261102-v1/node-event-test.json'
$node=Get-Content $nodePath -Raw|ConvertFrom-Json
if(!$s.verified -or !$s.inverseVerified -or $s.negativeControls -ne 12 -or $s.hostPolicyChanged -or $s.runtimeTested){throw 'Candidate projection boundary differs'}
foreach($f in $s.files){if((Get-FileHash $f.path).Hash -cne $f.sha256 -or (Get-FileHash $f.original).Hash -cne $f.originalSha256){throw 'Candidate source changed'}}
if(!$node.passed -or $node.checks -ne 320 -or $node.cases.Count -ne 4 -or $node.sourceProofSha256 -cne (Get-FileHash $source).Hash -or $node.testSha256 -cne (Get-FileHash (Join-Path $root 'Lea.Tests/EventMapProjection.test.cjs')).Hash){throw 'Mock test boundary differs'}
foreach($case in $node.cases){if($case.checks -ne 80 -or $case.finalMaps -ne 3 -or $case.eagerMaps -ne $(if($case.kind -ceq 'candidate'){0}else{1})){throw 'Allocation observations differ'}}
if(!$b.passed -or !$b.inputsUnchanged -or $b.ownerAttention -or $b.sha256 -cne 'B245644C86BC706F1F54CB896B138419A7D9CECD9150F525E0F7790BF5C01A2E' -or (Get-FileHash $b.binary).Hash -cne $b.sha256){throw 'Build proof differs'}
foreach($name in @('8.cs','10.cs','2.cs')){
    if((Get-FileHash "I:/LeaBuildStorage/event-map-build-20261102-v1/source/Lea.Script/$name").Hash -cne (Get-FileHash "I:/LeaBuildStorage/full-sdk-source-20261101-v3/Lea.Script/$name").Hash){throw 'Host budget/evaluation/checkpoint code changed'}
}
if($n.passed -or !$n.inputsUnchanged -or $n.ownerAttention -or $n.changedFiles.Count -or $n.cases.Count -ne 1 -or $n.cases[0].name -cne 'FirstUse' -or $n.cases[0].passed -or $n.cases[0].checks -ne 0 -or $n.runs.Count -ne 2){throw 'Native failure boundary differs'}
foreach($item in @($b.inputs)+@($n.inputs)){if((Get-FileHash $item.path).Hash -cne $item.sha256){throw 'Build/test input changed'}}
foreach($item in $n.before){if((Get-FileHash $item.path).Hash -cne $item.sha256 -or (Get-Acl $item.path).Sddl -cne $item.sddl){throw 'Runtime byte/ACL drift'}}
foreach($run in @($b.runs)+@($n.runs)){
    if(!$run.ownedJob -or !$run.exitObserved -or !$run.originalHandleCaptured -or !$run.childRetirementIndependentlyAudited -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or $run.timedOut -or $run.killRequested -or $run.unsafeToContinue -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Owned process retirement differs'}
}
if($n.runs[0].exitCode -ne 0 -or $n.runs[1].exitCode -ne 1){throw 'Native exits differ'}
$stderr=[IO.File]::ReadAllText($n.runs[1].stderr)
if(!$stderr.Contains('phase=bootstrap; reason=TimeoutException; stage=elements')){throw 'Typed bootstrap failure differs'}
New-Item -ItemType Directory -Path $out|Out-Null
$copies=[Collections.Generic.List[object]]::new()
$paths=@($source,$build,$native,$nodePath)+@(Get-ChildItem (Split-Path $native) -File|Where-Object Name -ne 'result.json'|Select-Object -ExpandProperty FullName)
$i=0
foreach($path in $paths){$to=Join-Path $out ((++$i).ToString('D2')+'-'+[IO.Path]::GetFileName($path));Copy-Item -LiteralPath $path -Destination $to;$hash=(Get-FileHash $path).Hash;if((Get-FileHash $to).Hash -cne $hash){throw 'Evidence copy mismatch'};$copies.Add(@{original=$path;copy=$to;sha256=$hash})}
@{verified=$true;nodeMockChecks=320;candidateQualified=$false;startupFixed=$false;nativeFirstUseAssertionsExecuted=$false;classListsExecuted=$false;workerBuilt=$false;runtimeFiles=$n.before.Count;hostPolicyUnchanged=$true;ownedRuns=3;allOwnedJobsRetired=$true;copies=$copies}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
