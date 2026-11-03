$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$out='I:/LeaBuildStorage/private-binding-pair-audit-20261102-v1'
if(Test-Path $out){throw 'Preserve private binding pair audit'}
$trusted=Join-Path $root 'continuity-evidence/private-binding-trusted-v1/result.json'
$worker='I:/LeaBuildStorage/private-binding-worker-build-20261102-v1/result.json'
$isolated=Join-Path $root 'continuity-evidence/private-binding-feature-isolated-v1/result.json'
foreach($pin in @(@($trusted,'D62490714583EC7956AAFCE575579BF81F4EF8F9D1C1288A6C271B3930A43952'),@($worker,'302C3003416F9095BA56C214FF6FD8B4F05A82F102CBAAF53B23DC3FAEE6E398'),@($isolated,'8244E1A2816FF045B17445FE152CB49AD3AA0646097F443B7FAD2763010B70FD'))){if((Get-FileHash $pin[0]).Hash -cne $pin[1]){throw 'Pinned pair receipt differs'}}
$t=Get-Content $trusted -Raw|ConvertFrom-Json
$w=Get-Content $worker -Raw|ConvertFrom-Json
$n=Get-Content $isolated -Raw|ConvertFrom-Json
foreach($report in @($t,$w,$n)){if(!$report.passed -or !$report.inputsUnchanged -or $report.ownerAttention){throw 'Pair report incomplete'};foreach($input in $report.inputs){if((Get-FileHash $input.path).Hash -cne $input.sha256){throw 'Pair input changed'}}}
if($t.changedFiles.Count -or !$t.completed -or $t.cases.Count -ne 2 -or $t.cases[0].name -cne 'FirstUse' -or $t.cases[0].checks -ne 6 -or $t.cases[1].name -cne 'ClassLists' -or $t.cases[1].checks -ne 50 -or @($t.cases|Where-Object {!$_.passed}).Count -or $t.before.Count -ne 13){throw 'Trusted semantic case boundary differs'}
$harness=[IO.File]::ReadAllText((Join-Path $root 'Lea.Tests/PrivateBindingTrustedMain.cs'))
$inverse=$harness.Replace('internal static class PrivateBindingTrustedMain','internal static class EventMapTrustedMain').Replace('PASS Lea private-binding trusted ','PASS Lea event-map trusted ')
if($inverse -cne [IO.File]::ReadAllText((Join-Path $root 'Lea.Tests/EventMapTrustedMain.cs'))){throw 'Trusted assertions changed'}
if($w.products.Count -ne 6 -or $w.resourceSha256 -cne '196BCE962F2C2F375C77644D0AE19485820F599170D4D80013829A6ECD2886E3' -or $w.workerExecuted -or $w.runtimeQualified -or $w.hostAdopted){throw 'Worker build boundary differs'}
foreach($product in $w.products){if((Get-FileHash $product.path).Hash -cne $product.sha256 -or (Get-Item $product.path).Length -ne $product.bytes){throw 'Worker product changed'}}
$config=Get-Content 'I:/LeaBuildStorage/private-binding-worker-build-20261102-v1/ScriptWorker/Lea.Script.Worker.runtimeconfig.json' -Raw|ConvertFrom-Json
if($config.runtimeOptions.framework.version -cne '8.0.33' -or $config.runtimeOptions.rollForward -cne 'Disable'){throw 'Worker runtime policy differs'}
if(!$n.isolatedWorkerTested -or $n.browserAdopted -or $n.changedFiles.Count -or $n.before.Count -ne 21){throw 'Isolated feature boundary differs'}
if((Get-FileHash (Join-Path $root 'Lea.Tests/IsConnectedMain.cs')).Hash -cne '8AE71E18B5CDB42D43D61FFD912FA638FD018EB21DB7A85DDE6850806B9162F4' -or (Get-FileHash (Join-Path $root 'Lea.Tests/IsConnectedCases.js')).Hash -cne 'AE7B3FD5C013D83D2BFD3740890FD67E13BB8E46CA2C87DFC67B4011388623F6'){throw 'Original isolated assertions changed'}
foreach($old in $t.before){$match=@($n.before|Where-Object path -CEQ $old.path);if($match.Count -ne 1 -or $match[0].sha256 -cne $old.sha256 -or $match[0].sddl -cne $old.sddl){throw 'Prior trusted runtime changed'}}
$runtime=Split-Path $t.before[0].path
if(@(Get-ChildItem $runtime -Recurse -File).Count -ne 21){throw 'Current shared inventory differs'}
$allowed=@('Lea.PrivateBindings.Connectivity.exe','Lea.PrivateBindings.Connectivity.exe.config')+@($w.products|ForEach-Object {'ScriptWorker/'+[IO.Path]::GetFileName($_.path)})
foreach($file in @($n.before|Where-Object {$_.path -cnotin @($t.before.path)})){
    $relative=[IO.Path]::GetRelativePath($runtime,$file.path).Replace('\','/')
    if($relative -cnotin $allowed){throw 'Unexpected new pair file'}
}
foreach($file in $n.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Pair runtime byte/ACL drift'}}
foreach($run in @($t.runs)+@($w.runs)+@($n.runs)){if(!$run.passed -or !$run.ownedJob -or !$run.exitObserved -or !$run.originalHandleCaptured -or !$run.childRetirementIndependentlyAudited -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or $run.timedOut -or $run.killRequested -or $run.unsafeToContinue -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Pair ownership differs'}}
$parent='I:/LeaBuildStorage/private-binding-build-20261102-v1/source/bin/Release/Lea.Script.dll'
$workerDll='I:/LeaBuildStorage/private-binding-worker-build-20261102-v1/ScriptWorker/Lea.Script.Worker.dll'
foreach($path in @($parent,$workerDll)){
    $assembly=[Reflection.Assembly]::LoadFile($path);$stream=$assembly.GetManifestResourceStream('Lea.Script.Dom.js')
    if(!$stream){throw 'Missing embedded DOM resource'}
    try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
    if($hash -cne $w.resourceSha256){throw 'Actual embedded parent/worker resource mismatch'}
}
New-Item -ItemType Directory -Path $out|Out-Null
$copies=[Collections.Generic.List[object]]::new();$i=0
$paths=@($trusted,$worker,$isolated)+@(@($t.runs)+@($w.runs)+@($n.runs)|ForEach-Object {$_.stdout;$_.stderr})
foreach($from in $paths){$to=Join-Path $out ((++$i).ToString('D2')+'-'+[IO.Path]::GetFileName($from));Copy-Item -LiteralPath $from -Destination $to;$hash=(Get-FileHash $from).Hash;if((Get-FileHash $to).Hash -cne $hash){throw 'Pair evidence readback mismatch'};$copies.Add(@{original=$from;copy=$to;sha256=$hash})}
@{verified=$true;trustedChecks=56;firstUseChecks=6;classListChecks=50;matchingWorkerBuilt=$true;embeddedResourcesMatch=$true;isolatedConnectivityPassed=$true;allSevenOwnedRunsRetired=$true;runtimeFiles=21;priorThirteenFilesUnchanged=$true;startupFixed=$false;fullSuiteQualified=$false;browserAdopted=$false;copies=$copies}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
