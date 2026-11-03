param(
    [Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$ControlReceiptSha256,
    [Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$LazyReceiptSha256
)
$ErrorActionPreference='Stop'
$root=Join-Path $PSScriptRoot '../continuity-evidence/temporal-startup-v1'
$out=Join-Path $root 'host-comparison-v1.json'
if(Test-Path -LiteralPath $out){throw 'Preserve comparison'}
$expected=@{control=$ControlReceiptSha256;lazy=$LazyReceiptSha256}
$reports=@{}
foreach($name in 'control','lazy'){
    $path=Join-Path $root "host-$name-v1/result.json"
    if((Get-FileHash -LiteralPath $path).Hash -cne $expected[$name]){throw 'Receipt changed'}
    $r=Get-Content -LiteralPath $path -Raw|ConvertFrom-Json
    if(!$r.completed -or $r.ownerAttention -or !$r.originalInputsUnchanged -or !$r.candidateBytesUnchanged -or $r.runs.Count -ne 15){throw 'Run incomplete or bytes changed'}
    foreach($run in $r.runs){if($run.killRequested -or !$run.exitObserved -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.retainedForOwner){throw 'Unclean parent retirement'}}
    foreach($file in $r.before){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'Candidate no longer matches'}}
    $reports[$name]=$r
}
$controlFiles=@{};$lazyFiles=@{}
foreach($name in 'control','lazy'){
    $r=$reports[$name];$map=if($name -eq 'control'){$controlFiles}else{$lazyFiles}
    foreach($file in $r.before){
        $relative=$file.path.Substring($r.candidate.Length+1)
        if($map.ContainsKey($relative)){throw 'Duplicate relative path'}
        $map[$relative]=$file.sha256
    }
}
if(@(Compare-Object @($controlFiles.Keys) @($lazyFiles.Keys) -CaseSensitive).Count){throw 'Runtime inventories differ'}
$different=@($controlFiles.Keys|Where-Object {$controlFiles[$_] -cne $lazyFiles[$_]})
if($different.Count -ne 1 -or $different[0] -cne 'ScriptWorker\Jint.dll'){throw 'Unexpected runtime delta'}
$expectedStages=@('settings-path','host-clean','host-lea','host-navigation','host-second','host-split','host-edit','host-style-start','host-script','exe-clean','exe-restored','exe-locked','exe-oversized','exe-lazy-background','exe-lazy-all')
$rows=@(foreach($stage in $expectedStages){
    $c=@($reports.control.runs|Where-Object name -ceq $stage);$l=@($reports.lazy.runs|Where-Object name -ceq $stage)
    if($c.Count -ne 1 -or $l.Count -ne 1){throw 'Missing/duplicate stage'}
    @{stage=$stage;controlPassed=$c[0].passed;lazyPassed=$l[0].passed;controlExit=$c[0].exitCode;lazyExit=$l[0].exitCode}
})
$result=@{comparisonCompleted=$true;controlReceiptSha256=$ControlReceiptSha256;lazyReceiptSha256=$LazyReceiptSha256;runtimeFiles=$controlFiles.Count;differentFiles=$different;stages=$rows;controlRawPassed=$reports.control.passed;lazyRawPassed=$reports.lazy.passed;performanceClaim=$false;releaseValidated=$false;scope='Exact matched host subset; ACL classifications and broader engine/security/reliability gates remain separate'}
[IO.File]::WriteAllText([IO.Path]::GetFullPath($out),($result|ConvertTo-Json -Depth 6),[Text.UTF8Encoding]::new($false))
$result|ConvertTo-Json -Depth 6
(Get-FileHash -LiteralPath $out).Hash
