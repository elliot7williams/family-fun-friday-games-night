$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$out='I:/LeaBuildStorage/private-binding-regression-audit-20261102-v1'
if(Test-Path $out){throw 'Preserve private binding regression audit'}
$domPath='I:/LeaBuildStorage/private-binding-shared-dom-20261102-v1/result.json'
$schedulingPath='I:/LeaBuildStorage/private-binding-shared-scheduling-20261102-v1/result.json'
$nativePath='I:/LeaBuildStorage/private-binding-shared-native-20261102-v1/result.json'
foreach($pin in @(@($domPath,'246C2EDB4717DAE02F6CEA6E2A7D1975B6B3A08DF23ECB569B9922B1E832B836'),@($schedulingPath,'3015572627EA0427C896C52C2EB697C2CE2AEAE986B94119B17902F05286B4CC'),@($nativePath,'746FE5992E89D2639E4D594D9681774DD289D91568360473704C753969E2620F'))){if((Get-FileHash $pin[0]).Hash -cne $pin[1]){throw 'Pinned regression receipt differs'}}
$d=Get-Content $domPath -Raw|ConvertFrom-Json
$s=Get-Content $schedulingPath -Raw|ConvertFrom-Json
$n=Get-Content $nativePath -Raw|ConvertFrom-Json
. (Join-Path $PSScriptRoot 'Read-AllStartupTraceProof.ps1')
$reports=@($d,$s,$n);$expectedCounts=@(23,25,27);$expectedRuns=@(5,5,2)
for($index=0;$index -lt $reports.Count;$index++){
    $r=$reports[$index]
    if(!$r.passed -or !$r.completed -or !$r.runtimeReused -or $r.engineBinariesChanged -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.before.Count -ne $expectedCounts[$index] -or $r.runs.Count -ne $expectedRuns[$index] -or $r.driverFilesAdded.Count -ne 2 -or $r.browserAdopted){throw 'Regression qualification boundary differs'}
    foreach($input in $r.inputs){if((Get-FileHash $input.path).Hash -cne $input.sha256){throw 'Regression input changed'}}
    foreach($file in $r.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Regression byte/ACL drift'}}
    for($i=0;$i -lt $r.runs.Count;$i++){
        $run=$r.runs[$i];$budget=if($i -eq 0){120000}else{30000}
        if(!$run.passed -or $run.exitCode -ne 0 -or !$run.exitObserved -or !$run.outputDrained -or !$run.originalHandleCaptured -or !$run.ownedJob -or !$run.childRetirementIndependentlyAudited -or $run.activeAfterRetirement -ne 0 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.timedOut -or $run.killRequested -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Regression owned retirement differs'}
        if($index -lt 2 -and $i -gt 0){
            $trace=Read-AllStartupTraceProof -Lines @(Get-Content $run.stderr) -StreamsComplete ([bool]($run.stdoutComplete -and $run.stderrComplete)) -ExitObserved ([bool]$run.exitObserved) -TimedOut ([bool]$run.timedOut) -KillRequested ([bool]$run.killRequested)
            if(!$trace.traceActivationVerified -or !$trace.traceEnvironmentRestored){throw 'Regression trace gate failed'}
        }
    }
}
if(($d.cases.name -join '|') -cne 'Selectors|Tree|Attributes|ClassLists' -or ($d.cases.checks -join '|') -cne '48|48|66|100' -or @($d.cases|Where-Object {!$_.passed -or !$_.traceActivationVerified -or !$_.traceEnvironmentRestored}).Count){throw 'DOM cases differ'}
if(($s.cases.name -join '|') -cne 'Timers|Frames|Timers|Frames' -or ($s.cases.mode -join '|') -cne 'trusted|trusted|isolated|isolated' -or ($s.cases.checks -join '|') -cne '59|26|59|27' -or @($s.cases|Where-Object {!$_.passed -or !$_.traceActivationVerified -or !$_.traceEnvironmentRestored}).Count){throw 'Scheduling cases differ'}
foreach($record in @(@{report=$d;prefix='DOM'},@{report=$s;prefix='Scheduling'})){
    $r=$record.report
    for($i=0;$i -lt 4;$i++){
        $case=$r.cases[$i]
        $prefix=if($record.prefix -ceq 'DOM'){'PASS Lea DOM regression '}elseif($case.mode -ceq 'trusted'){'PASS Lea scheduling '}else{'PASS Lea isolated scheduling '}
        $footer=$prefix+$case.name+': '+$case.checks+' checks.'
        if(@(Get-Content $r.runs[$i+1].stdout|Where-Object {$_ -ceq $footer}).Count -ne 1){throw 'Unique original assertion footer required'}
    }
}
if(@(Get-Content $s.runs[3].stdout|Where-Object {$_ -ceq 'LEA_SCHEDULING_GUARDS 21'}).Count -ne 1){throw 'Live isolated timer guard observations differ'}
if((Get-FileHash (Join-Path $root 'Lea.Tests/SchedulingRegressionMain.cs')).Hash -cne 'B6F9B190C6CE045E0592B13B39990B53068805E5F67A9CF3811A07F573F404CB' -or (Get-FileHash (Join-Path $root 'Lea.Tests/IsolatedSchedulingRegressionMain.cs')).Hash -cne '59A0525B2418F211085AF431B3ACE903DD06DB063177F491791FA729B971924B' -or (Get-FileHash (Join-Path $root 'Lea.Tests/IsConnectedNativeMain.cs')).Hash -cne 'BBBF3D3842E9D827398BCF107E4F0DF6552FD90606FECE5BEA7A18C5B5263EC1'){throw 'Original scheduling/native assertions changed'}
$priorPath=Join-Path $root 'continuity-evidence/private-binding-feature-isolated-v1/result.json'
if((Get-FileHash $priorPath).Hash -cne '8244E1A2816FF045B17445FE152CB49AD3AA0646097F443B7FAD2763010B70FD'){throw 'Prior21file proof changed'}
$prior=Get-Content $priorPath -Raw|ConvertFrom-Json
foreach($pair in @(@{before=$prior;after=$d},@{before=$d;after=$s},@{before=$s;after=$n})){
    foreach($old in $pair.before.before){$match=@($pair.after.before|Where-Object path -CEQ $old.path);if($match.Count -ne 1 -or $match[0].sha256 -cne $old.sha256 -or $match[0].sddl -cne $old.sddl){throw 'Prior shared runtime member changed'}}
    $added=@($pair.after.before|Where-Object {$_.path -cnotin @($pair.before.before.path)})
    if($added.Count -ne 2 -or @($added|Where-Object {$_.path -cnotin $pair.after.driverFilesAdded}).Count){throw 'Only two new driver files allowed per campaign'}
}
if(@(Get-ChildItem $n.runtime -Recurse -File).Count -ne 27){throw 'Current27file inventory drift'}
if((Get-FileHash (Join-Path $n.runtime 'Lea.Script.dll')).Hash -cne '406C5634638D75C66892BD95429760CB4A5C74C94277913D1DF467147E30E75F' -or (Get-FileHash (Join-Path $n.runtime 'ScriptWorker/Lea.Script.Worker.dll')).Hash -cne 'F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0'){throw 'Matched regression pair differs'}
$oldNativePath='I:/LeaBuildStorage/global-install-shared-native-20261102-v1/result.json'
if((Get-FileHash $oldNativePath).Hash -cne 'EB81FF97CE8BCFCC677D5497A891F05090D050B204ABFAF1FF6C12089B6CF62B'){throw 'Older rendered fixture proof changed'}
$oldNative=Get-Content $oldNativePath -Raw|ConvertFrom-Json
$images=[Collections.Generic.List[object]]::new()
if(!$n.nativeOnly -or $n.images.Count -ne 3){throw 'Native image boundary differs'}
foreach($image in $n.images){
    $name=[IO.Path]::GetFileName($image.path)
    $old=@($oldNative.images|Where-Object {[IO.Path]::GetFileName($_.path) -ceq $name})
    if($old.Count -ne 1 -or (Get-FileHash $old[0].path).Hash -cne $old[0].sha256 -or $old[0].sha256 -cne $image.sha256 -or (Get-FileHash $image.path).Hash -cne $image.sha256 -or (Get-Item $image.path).Length -gt 2MB){throw 'Native renders must match recorded baseline bytes'}
    $bytes=[IO.File]::ReadAllBytes($image.path)
    if($bytes.Length -lt 33 -or [Convert]::ToHexString($bytes[0..7]) -cne '89504E470D0A1A0A' -or [Text.Encoding]::ASCII.GetString($bytes,12,4) -cne 'IHDR'){throw 'PNG header differs'}
    $width=[Net.IPAddress]::NetworkToHostOrder([BitConverter]::ToInt32($bytes,16));$height=[Net.IPAddress]::NetworkToHostOrder([BitConverter]::ToInt32($bytes,20))
    if($width -ne $(if($name -ceq 'detached-narrow.png'){420}else{940}) -or $height -ne 700){throw 'Native PNG dimensions differ'}
    $images.Add(@{name=$name;width=$width;height=$height;sha256=$image.sha256;matchesBaseline=$true})
}
New-Item -ItemType Directory -Path $out|Out-Null
$copies=[Collections.Generic.List[object]]::new();$i=0
$paths=@($domPath,$schedulingPath,$nativePath)+@($reports|ForEach-Object {$_.runs|ForEach-Object {$_.stdout;$_.stderr}})+@($n.images.path)
foreach($from in $paths){$to=Join-Path $out ((++$i).ToString('D2')+'-'+[IO.Path]::GetFileName($from));Copy-Item -LiteralPath $from -Destination $to;$hash=(Get-FileHash $from).Hash;if((Get-FileHash $to).Hash -cne $hash){throw 'Regression evidence readback differs'};$copies.Add(@{original=$from;copy=$to;sha256=$hash})}
@{verified=$true;domChecks=262;schedulingChecks=171;isolatedTimerGuardObservations=21;nativeViewPassed=$true;images=$images;allTwelveOwnedRunsRetired=$true;runtimeFiles=27;original21Unchanged=$true;driverFilesAdded=6;startupReliabilityProven=$false;fullSuiteQualified=$false;hostAdopted=$false;copies=$copies}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
