$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$directory=Join-Path $root 'continuity-evidence/contains-recovery-full-prefix-owned-v1'
$receipt=Join-Path $directory 'result.json'
$out='I:\LeaBuildStorage\contains-recovery-prefix-audit-20261102-v1'
if(Test-Path $out){throw 'Preserve diagnostic audit'}
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$expected){
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($expected -and $hash -cne $expected){throw 'Recovery audit identity differs'};$inputs.Add(@{path=$path;sha256=$hash})
}
function ReadMode([string[]]$lines,[string]$mode){
    $labels=@(1..13|ForEach-Object {'code '+$_})+@('cycle','code 14','code 15','code 16','bridge','code 17')
    $index=0;$begin=$null;$previous=-1L;$rows=[Collections.Generic.List[object]]::new()
    foreach($line in $lines|Where-Object {$_.StartsWith('LEA_CONTAINS_RECOVERY '+$mode+' ')}){
        if($line -cnotmatch '^LEA_CONTAINS_RECOVERY (trusted|isolated) (code [0-9]+|cycle|bridge) (begin|end) ([0-9]+)(?: status=(ok|limit))?$'){throw 'Malformed recovery marker'}
        $label=$Matches[2];$edge=$Matches[3];$time=[long]$Matches[4];$status=$Matches[5]
        if($index -ge $labels.Count -or $label -cne $labels[$index] -or $time -lt $previous){throw 'Recovery sequence differs'};$previous=$time
        if($edge -ceq 'begin'){if($null -ne $begin -or $status){throw 'Duplicate/invalid begin'};$begin=$time}
        else{
            $expected=if($label -in 'cycle','bridge'){'limit'}else{'ok'}
            if($null -eq $begin -or $status -cne $expected){throw 'Missing begin/typed status differs'}
            $rows.Add(@{label=$label;beginMs=$begin;endMs=$time;durationMs=($time-$begin);status=$status});$index++;$begin=$null
        }
    }
    if($index -ne 19 -or $null -ne $begin){throw 'Expected all19 complete recovery operations'}
    return $rows.ToArray()
}
try{
    Pin $PSCommandPath ''
    Pin $receipt '1D0BCC947674AED3C0CA66295365CAD009FD474F8F0CB9736AF4D5F21229F7A7'
    $r=Get-Content $receipt -Raw|ConvertFrom-Json
    if(!$r.passed -or !$r.completed -or !$r.diagnosticOnly -or $r.releaseQualified -or $r.limitsChanged -or $r.checks -ne 3412 -or !$r.phasesVerified -or !$r.footerVerified -or !$r.traceActivationVerified -or !$r.traceEnvironmentRestored -or !$r.inputsUnchanged -or $r.changedFiles.Count -or $r.ownerAttention -or $r.before.Count -ne 65 -or $r.runs.Count -ne 1){throw 'Diagnostic full-prefix scope incomplete'}
    $run=$r.runs[0]
    if(!$run.passed -or $run.exitCode -ne 0 -or $run.parentBudgetMs -ne 300000 -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000 -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or !$run.exitObserved -or $run.killRequested -or $run.timedOut){throw 'Original process guards differ'}
    foreach($file in $r.inputs){Pin $file.path $file.sha256}
    foreach($file in $r.before){Pin $file.path $file.sha256;if((Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Current diagnostic ACL differs'}}
    Pin $run.stderr '';Pin $run.stdout ''
    $lines=@(Get-Content $run.stderr)
    $trusted=@(ReadMode $lines 'trusted');$isolated=@(ReadMode $lines 'isolated')
    if(@($lines|Where-Object {$_.StartsWith('LEA_CONTAINS_RECOVERY ')}).Count -ne 76){throw 'Unexpected checkpoint count'}
    $negative=0
    $valid=@($lines|Where-Object {$_.StartsWith('LEA_CONTAINS_RECOVERY isolated ')})
    $badCases=[Collections.Generic.List[object]]::new()
    $badCases.Add(@{name='duplicate begin';lines=(@($valid[0])+$valid)})
    $badCases.Add(@{name='missing begin';lines=$valid[1..($valid.Count-1)]})
    $badCases.Add(@{name='wrong typed status';lines=@($valid|ForEach-Object {$_ -creplace 'cycle end ([0-9]+) status=limit','cycle end $1 status=ok'})})
    foreach($bad in $badCases){
        $denied=$false;try{$null=ReadMode $bad.lines 'isolated'}catch{$denied=$true};if(!$denied){throw ('Malformed checkpoint trace accepted: '+$bad.name)};$negative++
    }
    New-Item -ItemType Directory -Path $out|Out-Null
    foreach($p in @($receipt,$run.stdout,$run.stderr)){$to=Join-Path $out ([IO.Path]::GetFileName($p));Copy-Item $p $to;if((Get-FileHash $to).Hash -cne (Get-FileHash $p).Hash){throw 'Audit raw copy differs'}}
    @{verified=$true;inputs=$inputs;checks=3412;phases=89;runtimeFiles=65;checkpointMarkers=76;negativeControls=$negative;trusted=$trusted;isolated=$isolated;diagnosticOnly=$true;timingFailureResolved=$false;releaseQualified=$false;scope='Successful instrumented full prefix, unchanged runtime bytes/ACL and original process guards; observation does not reproduce/explain previous uninstrumented timeout'}|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'audit.json')
}finally{foreach($s in $holds){$s.Dispose()}}
