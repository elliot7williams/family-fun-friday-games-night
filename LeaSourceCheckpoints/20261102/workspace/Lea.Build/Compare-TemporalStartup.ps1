$ErrorActionPreference='Stop'
$root=Join-Path $PSScriptRoot '../continuity-evidence/temporal-startup-v1'
$out=Join-Path $root 'startup-comparison-v1.json'
if(Test-Path -LiteralPath $out){throw 'Preserve comparison'}
$expected=@{control='3A782A0D95AB1A1D1B404BD8B7258F4E8FA902B2D028E4C47AE68D55B6016765';lazy='96F8EC304616E83AFA1B65D9878610BA391EEF38426144A85E26EB290DFBA545'}
$rows=@(foreach($name in 'control','lazy'){
    $path=Join-Path $root "startup-$name-v1/result.json"
    if((Get-FileHash -LiteralPath $path).Hash -cne $expected[$name]){throw 'Run receipt changed'}
    $r=Get-Content -LiteralPath $path -Raw|ConvertFrom-Json
    $p=$r.report
    if(!$r.completed -or $r.error -or $r.exitCode -ne 0 -or !$r.inputsUnchanged -or $r.ownerAttention -or
        !$p.executionOk -or !$p.guardsVerified -or !$p.cleanupVerified -or $p.cleanupPending -or
        !$p.observerRetired -or $p.observerError -or $p.samplesOverflow -or @($p.samples|Where-Object counterError).Count){throw 'Diagnostic incomplete'}
    foreach($file in $r.before){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256 -or (Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Staged bytes/ACL changed'}}
    foreach($input in $r.inputs){if((Get-FileHash -LiteralPath $input.path).Hash -cne $input.sha256){throw 'Original input changed'}}
    $text=($p.samples|ForEach-Object text)-join "`n"
    function Marker([string]$stage){
        $values=@([regex]::Matches($text,('startup '+[regex]::Escape($stage)+': wall=(\d+) ms'))|ForEach-Object {[int]$_.Groups[1].Value}|Sort-Object -Unique)
        if($values.Count -ne 1){throw ('Marker missing or conflicting: '+$stage)}
        return $values[0]
    }
    $options=Marker 'options';$configured=Marker 'configured';$hello=Marker 'hello'
    @{variant=$name;receiptSha256=$expected[$name];wallMs=$p.wallMs;optionsMs=$options;configuredMs=$configured;optionsToConfiguredMs=$configured-$options;helloMs=$hello;samples=$p.samples.Count;filesAndAclsUnchanged=$r.before.Count;executionGuardsCleanupVerified=$true}
})
$result=@{diagnosticPairCompleted=$true;rows=$rows;performanceGatePassed=$false;browserGatePassed=$false;scope='One sequential control/lazy pair; no cold-start, causal speedup or reliability claim';next='Run matched actual Firespider host-script and broader behavior checks before adoption'}
[IO.File]::WriteAllText([IO.Path]::GetFullPath($out),($result|ConvertTo-Json -Depth 5),[Text.UTF8Encoding]::new($false))
$result|ConvertTo-Json -Depth 5
(Get-FileHash -LiteralPath $out).Hash
