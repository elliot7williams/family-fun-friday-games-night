$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\combined-trusted-counter-audit-20261101-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve trusted counter audit'}
$inputs=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new()
function Pin([string]$path,[string]$expected){
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($expected -and $hash -cne $expected){throw ('Trusted audit input differs: '+$path)};$inputs.Add(@{path=$path;sha256=$hash})
}
function Proof([string]$path,[string]$hash){Pin $path $hash;return Get-Content -LiteralPath $path -Raw|ConvertFrom-Json}
function Valid-Samples($samples,[long]$pidSeen,[long]$birth){
    if($samples.Count -lt 3 -or $samples.Count -gt 512){return $false}
    $wall=-1L;$cpu=-1L;$faults=-1L;$reads=-1L;$writes=-1L;$stderr=-1L
    foreach($sample in $samples){
        $n=$sample.native;$t=$sample.threads
        if(!$sample.originalHandleBorrowed -or $sample.ownerCreationFileTime -ne $birth -or !$n -or !$t -or !$t.OriginalProcessHandleVerified -or $t.ProcessId -ne $pidSeen -or $t.ProcessCreated -ne $birth -or $sample.elapsedMs -le $wall -or $sample.queryMs -lt 0 -or $sample.stderrBytesObserved -lt $stderr -or $n.CpuTicks -lt $cpu -or $n.PageFaults -lt $faults -or $n.ReadBytes -lt $reads -or $n.WriteBytes -lt $writes -or !$t.Threads.Count -or $t.Threads.Count -gt 4096){return $false}
        $ids=[Collections.Generic.HashSet[long]]::new()
        foreach($thread in $t.Threads){if($thread.Id -le 0 -or !$ids.Add($thread.Id) -or $thread.Created -le 0 -or $thread.CpuTicks -lt 0 -or $thread.State -gt 9){return $false}}
        $wall=$sample.elapsedMs;$stderr=$sample.stderrBytesObserved;$cpu=$n.CpuTicks;$faults=$n.PageFaults;$reads=$n.ReadBytes;$writes=$n.WriteBytes
    }
    return $true
}
function Parsing-Order($method){
    $parse=@($method.instructions|Where-Object {$_ -match ' call ParseScriptGuarded$'})
    $execute=@($method.instructions|Where-Object {$_ -match ' call Execute$'})
    if($parse.Count -ne 1 -or $execute.Count -ne 1){return $false}
    $a=[Convert]::ToInt32($parse[0].Substring(0,6),16);$b=[Convert]::ToInt32($execute[0].Substring(0,6),16)
    if($a -ge $b -or @($method.instructions|Where-Object {$_ -match '\bcall(virt)? (ExecuteWithConstraints|ResetConstraints)(<spec>)?$'}).Count){return $false}
    foreach($row in $method.instructions){$offset=[Convert]::ToInt32($row.Substring(0,6),16);if($offset -gt $a -and $offset -lt $b -and $row -match '^[0-9A-F]{6} (br|beq|bge|bgt|ble|blt|bne|switch|leave)'){return $false}}
    return $true
}
try{
    $r=Proof 'I:\LeaBuildStorage\combined-trusted-counters-20261101-v1/result.json' '3652E3E658105CB7BEBC0EB7DFD112D3026B93940C275222746B548125BB921C'
    if(!$r.diagnosticCaptured -or !$r.computedPassed -or !$r.prefixVerified -or !$r.inputsUnchanged -or $r.changedFiles.Count -or $r.ownerAttention -or $r.sampleErrors.Count -or $r.sampleOverflow -or !$r.firstCallUnwrapped -or !$r.programInverseVerified -or $r.programNegativeControls -ne 4 -or !$r.ownerInverseVerified -or $r.ownerNegativeControls -ne 3 -or !$r.runtimeOnExfat -or $r.nativeIsolationQualified -or $r.before.Count -ne 65 -or $r.originalBefore.Count -ne 130 -or $r.runs.Count -ne 2){throw 'Trusted diagnostic scope differs'}
    foreach($file in $r.inputs){Pin $file.path $file.sha256}
    foreach($file in $r.originalBefore){if((Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Retained NTFS ACL differs'}}
    foreach($run in $r.runs){$budget=if($run.name.StartsWith('compile-')){120000}else{300000};if(!$run.passed -or $run.exitCode -ne 0 -or !$run.originalHandleCaptured -or !$run.exitObserved -or $run.timedOut -or $run.killRequested -or !$run.outputDrained -or !$run.childRetirementIndependentlyAudited -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Original compiler/capture process guards failed'};Pin $run.stdout;Pin $run.stderr}
    $tested=$r.runs[1]
    $birth=if($tested.birthUtc -is [datetime]){$tested.birthUtc.ToUniversalTime().ToFileTimeUtc()}else{[datetime]::ParseExact($tested.birthUtc,'o',[Globalization.CultureInfo]::InvariantCulture,[Globalization.DateTimeStyles]::RoundtripKind).ToUniversalTime().ToFileTimeUtc()}
    if(!(Valid-Samples $r.samples $tested.pid $birth)){throw 'Original-handle sample sequence invalid'}
    foreach($kind in 1..5){
        $bad=$r.samples|ConvertTo-Json -Depth 12|ConvertFrom-Json
        switch($kind){1{$bad[0].originalHandleBorrowed=$false}2{$bad[-1].threads.ProcessCreated++}3{$bad[-1].elapsedMs=-1}4{$bad[-1].native.CpuTicks=-1}5{$bad[0].threads.OriginalProcessHandleVerified=$false}}
        if(Valid-Samples $bad $tested.pid $birth){throw 'Sample negative accepted'}
    }
    $lines=@(Get-Content $tested.stderr);$phases=@($lines|Where-Object {$_ -cmatch '^LEA_SUITE_PHASE '})
    if($phases.Count -ne 17 -or $phases[-1] -cnotmatch '^LEA_SUITE_PHASE [0-9]+ 8 begin$' -or @($lines|Where-Object {$_ -ceq 'LEA_ALL_STARTUP_TRACE restored=true'}).Count -ne 1){throw 'Original prefix/trace scope differs'}
    for($i=0;$i -lt 8;$i++){if($phases[2*$i] -cnotmatch ('^LEA_SUITE_PHASE [0-9]+ '+$i+' begin$') -or $phases[2*$i+1] -cnotmatch ('^LEA_SUITE_PHASE [0-9]+ '+$i+' end$')){throw 'Trusted prefix order differs'}}
    if(@(Get-Content $tested.stdout|Where-Object {$_ -ceq 'Diagnostic stop after computed-script assertion; not full suite.'}).Count -ne 1){throw 'Explicit trusted diagnostic stop missing'}
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Pin $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    $original=[IO.File]::ReadAllText($helper);$observed=[IO.File]::ReadAllText('I:\LeaBuildStorage\combined-trusted-counters-20261101-v1/Invoke-CombinedTrustedRun.ps1')
    $anchor="            if(@(`$readers|Where-Object overflow).Count){throw 'Test output exceeds4MiB guard'}"
    $insert="            Take-CombinedTrustedSample `$owner `$clock.ElapsedMilliseconds ([long](@(`$readers|Where-Object name -ceq 'stderr'|Measure-Object bytes -Sum).Sum))`r`n"+$anchor
    if([regex]::Matches($observed,[regex]::Escape($insert)).Count -ne 1 -or $observed.Replace($insert,$anchor).Replace('function Invoke-CombinedTrustedRun {','function Invoke-OwnedTestRun {') -cne $original){throw 'Independent owner hook inverse differs'}
    $raw=Get-Content -LiteralPath $tested.stderr -Raw
    $marker=[regex]::Match($raw,'(?m)^LEA_SUITE_PHASE [0-9]+ 8 begin\r?\n')
    if(!$marker.Success){throw 'Observed phase8 marker missing'}
    $threshold=[Text.Encoding]::UTF8.GetByteCount($raw.Substring(0,$marker.Index+$marker.Length))
    $window=@($r.samples|Where-Object {$_.stderrBytesObserved -ge $threshold})
    if($window.Count -lt 2){throw 'Too few samples after observed phase8 marker'}
    $first=$window[0];$last=$window[-1]
    $map=Proof 'I:\LeaBuildStorage\combined-script-phase-map-20261101-v1/result.json' '2E8A2ABC34FAB942D03A683A70BA59ADD51DFB75FDE340B37532AA0C37BA78A0'
    if(!$map.verified -or $map.interpreterExecuted -or $map.targetAssembliesLoaded -or $map.runtimeJintSha256 -cne 'B54618A4E3FFE33344D478D3A1580658C4D91819B3B97A24326CB3626CB7FD1A' -or $map.runtimeParentSha256 -cne '36AB8810EA87DDC6D3A840B629772210408F1ABEAF7341E204442E1275D79304'){throw 'Matched PE map differs'}
    foreach($file in $map.inputs){Pin $file.path $file.sha256}
    $parsing=@($map.vm.methods|Where-Object {$_.type -ceq 'Jint.Engine' -and $_.name -ceq 'Execute' -and @($_.instructions|Where-Object {$_ -match ' call ParseScriptGuarded$'}).Count})
    if($parsing.Count -ne 2){throw 'String overload map coverage differs'}
    foreach($method in $parsing){if(!(Parsing-Order $method)){throw 'Parser/prepared call-site order differs'}}
    foreach($kind in 1..3){
        $bad=$parsing[0]|ConvertTo-Json -Depth 8|ConvertFrom-Json
        switch($kind){1{$bad.instructions=@($bad.instructions|Where-Object {$_ -notmatch ' call ParseScriptGuarded$'})}2{$bad.instructions=@($bad.instructions|ForEach-Object {if($_ -match ' call Execute$'){'000000 call Execute'}else{$_}})}3{$bad.instructions+= '000000 call ResetConstraints'}}
        if(Parsing-Order $bad){throw 'IL order negative accepted'}
    }
    foreach($input in $inputs){if((Get-FileHash $input.path).Hash -cne $input.sha256){throw 'Audit input changed'}}
    New-Item -ItemType Directory -Path $out|Out-Null
    @{verified=$true;firstCallUnwrapped=$true;sampleNegativeControls=5;ilOrderNegativeControls=3;samples=$r.samples.Count;observedPhase8Samples=$window.Count;observedMarkerBytes=$threshold;observedWindowMs=($last.elapsedMs-$first.elapsedMs);observedCpuMs=(($last.native.CpuTicks-$first.native.CpuTicks)/10000.0);pageFaultDelta=($last.native.PageFaults-$first.native.PageFaults);logicalReadBytesDelta=($last.native.ReadBytes-$first.native.ReadBytes);parserCallSitesPrecedePreparedExecute=$true;scriptPassed=$true;fullSuitePassed=$false;nativeIsolationQualified=$false;causeProven=$false;failureResolved=$false;inputs=$inputs;scope='Observed phase8 window includes trusted VM/bootstrap/page work, not exclusive page-execution time. Linear selected IL call sites, not whole-method CFG proof. I: diagnostic images are not ACL qualification; original130NTFS ACLs remain unchanged. Both earlier failures remain unresolved.'}|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'result.json')
}finally{foreach($s in $holds){$s.Dispose()}}
