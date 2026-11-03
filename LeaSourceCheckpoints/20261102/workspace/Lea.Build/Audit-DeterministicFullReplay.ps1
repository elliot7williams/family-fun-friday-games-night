$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$directory=Join-Path $root 'continuity-evidence/deterministic-full-replay-owned-v1'
$receipt=Join-Path $directory 'result.json'
$out='I:\LeaBuildStorage\deterministic-full-replay-audit-20261102-v1'
if(Test-Path $out){throw 'Preserve replay audit'}
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$expected){
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($expected -and $hash -cne $expected){throw 'Replay audit identity differs'};$inputs.Add(@{path=$path;sha256=$hash})
}
function CheckRun($run){
    if(!$run.passed -or !$run.ownedJob -or !$run.originalHandleCaptured -or !$run.childRetirementIndependentlyAudited -or !$run.exitObserved -or $run.exitCode -ne 0 -or !$run.outputDrained -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.parentBudgetMs -ne 300000 -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000 -or $run.activeAfterRetirement -ne 0 -or $run.timedOut -or $run.killRequested -or $run.retainedForOwner -or $run.unsafeToContinue){throw 'Original full replay process guards differ'}
}
try{
    Pin $PSCommandPath ''
    Pin $receipt 'C9A3C41F5A76A33C7D792B3F7FBC34ED3D7E55BAE64A8844B49C812FE4D3A4CE'
    $r=Get-Content $receipt -Raw|ConvertFrom-Json
    if(!$r.passed -or !$r.completed -or $r.diagnosticOnly -or !$r.uninstrumentedReplay -or $r.releaseQualified -or $r.limitsChanged -or $r.checks -ne 3412 -or !$r.phasesVerified -or !$r.footerVerified -or !$r.recoveryMarkersAbsent -or !$r.traceActivationVerified -or !$r.traceEnvironmentRestored -or !$r.inputsUnchanged -or $r.changedFiles.Count -or $r.ownerAttention -or $r.before.Count -ne 65 -or $r.runs.Count -ne 1){throw 'Exact replay scope incomplete'}
    CheckRun $r.runs[0]
    foreach($file in $r.inputs){Pin $file.path $file.sha256}
    foreach($file in $r.before){Pin $file.path $file.sha256;if((Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Replay ACL changed'}}
    $assemblyPath='I:\LeaBuildStorage\deterministic-replay-assembly-20261102-v1/assembly.json'
    Pin $assemblyPath '6E8A9EBAEB14BC046E2DEDACC514FA0B5439DE8F261F4A2CE63209B2ED75A29F'
    $assembly=Get-Content $assemblyPath -Raw|ConvertFrom-Json
    if(!$assembly.verified -or $assembly.diagnosticOnly -or !$assembly.uninstrumentedReplay -or $assembly.changes.Count -ne 3 -or !$assembly.dispatcherInverseVerified){throw 'Replay source boundary differs'}
    foreach($pair in @(@('Program.cs','E4C8C28FE50A3B8AA76344D618BE4CCCE6E9B998DB5E2688DA102A462949EF0A'),@('Contains.cs','809837EC846B255428FA99FF6A25452D29CAD31F9EBDDECE90D34BA11DA9600B'))){Pin ('I:\LeaBuildStorage\deterministic-replay-assembly-20261102-v1/compiled-source/source/Lea.Script.Tests/'+$pair[0]) $pair[1]}
    $run=$r.runs[0];Pin $run.stdout '';Pin $run.stderr ''
    $footer=Join-Path $PSScriptRoot 'Read-LeaSuiteFooter.ps1';Pin $footer 'CE0005069067DDA4810317C0B9BFAF83339CD0DC929713458F76336FAC82D3B2';. $footer
    $null=Read-LeaSuiteFooter -Lines @(Get-Content $run.stdout) -ExpectedChecks 3412
    $lines=@(Get-Content $run.stderr)
    if(@($lines|Where-Object {$_.StartsWith('LEA_CONTAINS_RECOVERY ')}).Count){throw 'Added recovery instrumentation present'}
    $markers=@($lines|Where-Object {$_ -cmatch '^LEA_SUITE_PHASE [0-9]+ [0-9]+ (begin|end)$'})
    if($markers.Count -ne 178){throw 'All89 phase pairs required'}
    $previous=-1L
    for($i=0;$i -lt 178;$i++){
        if($markers[$i] -cnotmatch '^LEA_SUITE_PHASE ([0-9]+) ([0-9]+) (begin|end)$'){throw 'Malformed phase'}
        $time=[long]$Matches[1];$index=[int]$Matches[2];$edge=$Matches[3]
        if($time -lt $previous -or $index -ne [Math]::Floor($i/2) -or $edge -cne $(if($i%2 -eq 0){'begin'}else{'end'})){throw 'Phase sequence differs'};$previous=$time
    }
    $negative=0
    foreach($key in @('ownedJob','outputDrained','childRetirementIndependentlyAudited')){
        $bad=$run|ConvertTo-Json|ConvertFrom-Json;$bad.$key=$false
        $denied=$false;try{CheckRun $bad}catch{$denied=$true};if(!$denied){throw 'Malformed run accepted'};$negative++
    }
    New-Item -ItemType Directory -Path $out|Out-Null
    foreach($p in @($receipt,$run.stdout,$run.stderr)){$to=Join-Path $out ([IO.Path]::GetFileName($p));Copy-Item $p $to;if((Get-FileHash $p).Hash -cne (Get-FileHash $to).Hash){throw 'Raw replay archive differs'}}
    @{verified=$true;inputs=$inputs;checks=3412;phases=89;runtimeFiles=65;negativeControls=$negative;elapsedMs=$run.elapsedMs;deadlineMarginMs=(300000-$run.elapsedMs);diagnosticOnly=$false;uninstrumentedReplay=$true;releaseQualified=$false;timingFailureResolved=$false;scope='One fresh full replay, unchanged Contains and Program plus entry-only rebinding. Does not prove reliability, explain prior timeout or qualify Firespider/clean-machine/all-sites'}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'audit.json')
}finally{foreach($s in $holds){$s.Dispose()}}
