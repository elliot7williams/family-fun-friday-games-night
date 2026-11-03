$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\sdk-pooled-io-pilot-20261101-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve prior pilot evidence'}
$cases=@(
    @{name='baseline';folder='sdk-script-io-benchmark-baseline-v3';parent='906E3CAAE4C160C7FDCF9D7E79B08076B9A12F46A76715AE02CAE9E72983764C'},
    @{name='pooled';folder='sdk-script-pooled-io-benchmark-v1';parent='AFA6D95F21EC093F20F88177FD0B3710B956B1775958317C4BE6ED09864341B6'}
)
$records=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$path,[string]$pin){
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($pin -and $hash -cne $pin){throw 'Pilot input differs'}
    return $hash
}
try{
    foreach($case in $cases){
        $dir=Join-Path $root ('continuity-evidence/'+$case.folder)
        $receipt=Join-Path $dir 'result.json';$receiptHash=Hold $receipt
        $r=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
        if(!$r.passed -or !$r.benchmarkOnly -or !$r.inputsUnchanged -or !$r.workerAclRestored -or $r.ownerAttention -or $r.warmupCalls -ne 0 -or $r.measuredRequests -ne 32 -or $r.runs.Count -ne 2){throw 'Completed matching pilot required'}
        foreach($run in $r.runs){if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained){throw 'Clean owned pilot run required'}}
        if($r.runs[0].parentBudgetMs -ne 120000 -or $r.runs[1].parentBudgetMs -ne 30000){throw 'Original deadlines required'}
        foreach($input in $r.inputs){$null=Hold $input.path $input.sha256}
        foreach($file in $r.workerAclBefore){if((Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Worker ACL differs after pilot'}}
        $null=Hold (Join-Path $dir 'Lea.Script.dll') $case.parent
        $stdout=$r.runs[1].stdout;$stdoutHash=Hold $stdout
        $lines=@(Get-Content -LiteralPath $stdout)
        if(@($lines|Where-Object {$_ -ceq 'PASS SDK IO BENCHMARK 37'}).Count -ne 1){throw 'Exact benchmark footer required'}
        $header=@($lines|Where-Object {$_ -cmatch '^BENCHMARK retire [0-9]+ 32$'})
        if($header.Count -ne 1){throw 'Both variants must preserve per-request retirement'}
        $frequency=[double]($header[0].Split(' ')[2]);if($frequency -le 0){throw 'Invalid timestamp frequency'}
        $first=@($lines|Where-Object {$_ -cmatch '^FIRST [0-9]+$'});if($first.Count -ne 1){throw 'First-use observation missing'}
        $samples=@($lines|Where-Object {$_ -cmatch '^SAMPLE [0-9]+ [0-9]+$'})
        if($samples.Count -ne 32){throw 'Exact sample inventory required'}
        $ms=@(for($i=0;$i -lt 32;$i++){$parts=$samples[$i].Split(' ');if([int]$parts[1] -ne $i){throw 'Sample sequence differs'};1000*[double]$parts[2]/$frequency})
        $sorted=@($ms|Sort-Object)
        $worker=@($r.inputs|Where-Object {$_.path.StartsWith((Join-Path $dir 'ScriptWorker')+'\',[StringComparison]::OrdinalIgnoreCase)}|ForEach-Object {([IO.Path]::GetFileName($_.path))+':'+$_.sha256}|Sort-Object -Unique)
        if($worker.Count -ne 6){throw 'Exact six-file worker required'}
        $testSource=@($r.inputs|Where-Object {$_.path.EndsWith('\Lea.Tests\SdkSessionIoBenchmarkMain.cs',[StringComparison]::OrdinalIgnoreCase)}|Select-Object -ExpandProperty sha256 -Unique)
        if($testSource.Count -ne 1){throw 'Benchmark source provenance missing'}
        $records.Add(@{name=$case.name;receipt=$receipt;receiptSha256=$receiptHash;stdoutSha256=$stdoutHash;parent=$case.parent;worker=$worker;testSource=$testSource[0];firstMs=1000*[double]$first[0].Split(' ')[1]/$frequency;samplesMs=$ms;meanMs=($ms|Measure-Object -Average).Average;medianMs=($sorted[15]+$sorted[16])/2;p95Ms=$sorted[30];maxMs=$sorted[31]})
    }
    if(($records[0].worker -join '|') -cne ($records[1].worker -join '|') -or $records[0].testSource -cne $records[1].testSource){throw 'Worker or benchmark source differs'}
    New-Item -ItemType Directory -Path $out|Out-Null
    $result=@{verified=$true;pilotOnly=$true;order='baseline then pooled';warmupCalls=0;requestsPerCase=32;coldFailureResolved=$false;browserSpeedupProven=$false;fullSuiteQualified=$false;cases=$records}
    $result|ConvertTo-Json -Depth 8|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $records|ForEach-Object {[pscustomobject]$_}|Select-Object name,firstMs,meanMs,medianMs,p95Ms,maxMs|Format-Table
}finally{foreach($hold in $holds){$hold.Dispose()}}
