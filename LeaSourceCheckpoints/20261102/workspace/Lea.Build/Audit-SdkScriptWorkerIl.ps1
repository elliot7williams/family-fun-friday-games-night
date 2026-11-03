$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$receipt='I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-script-worker\result.json'
$pin='2DCE0B8004660AD5FD0A8621D748F9373D26547B7E1453C735EF965BE353EBD9'
if((Get-FileHash $receipt).Hash -cne $pin){throw 'Worker IL build receipt differs'}
$r=Get-Content $receipt -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.scriptWorkerIlCompiled -or $r.readyToRun -or $r.workerExecuted -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.error -or $r.runs.Count -ne 1 -or $r.outputs.Count -ne 6){throw 'Worker IL build gate incomplete'}
$run=$r.runs[0]
if(!$run.passed -or $run.exitCode -ne 0 -or !$run.exitObserved -or $run.timedOut -or $run.killRequested -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or $run.parentBudgetMs -ne 120000 -or $run.elapsedMs -gt 120000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Worker compile lifecycle differs'}
foreach($file in @($r.inputs)+@($r.outputs)){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Worker build input/product changed'}}
$stage='I:\LeaBuildStorage\sdk-script-worker-20261101-v1'
$runtime=Join-Path $stage 'bin/Release/ScriptWorker'
$options=(Get-Content (Join-Path $runtime 'Lea.Script.Worker.runtimeconfig.json') -Raw|ConvertFrom-Json).runtimeOptions
if($options.framework.name -cne 'Microsoft.NETCore.App' -or $options.framework.version -cne '8.0.33' -or $options.rollForward -cne 'Disable'){throw 'Generated worker runtime policy differs'}
$manifest=Get-Content (Join-Path $stage 'source-manifest.json') -Raw|ConvertFrom-Json
foreach($name in @('Jint.dll','Acornima.dll')){
    $rows=@($manifest.files|Where-Object path -ceq ('lib/'+$name))
    if($rows.Count -ne 1 -or (Get-FileHash (Join-Path $runtime $name)).Hash -cne $rows[0].sha256){throw 'Copied worker interpreter/parser differs'}
}
$reader=Join-Path $PSScriptRoot 'ReadBootstrapResource.cs';Add-Type -Path $reader
$bytes=[ReadBootstrapResource]::Read((Join-Path $runtime 'Lea.Script.Worker.dll'))
$resourceHash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes))
if($resourceHash -cne 'EC4B6513A77230C3B8439568CC3FED41B7712CA3779182D38076880DCC83BC8A'){throw 'Worker embedded DOM resource differs'}
$out=Join-Path $stage 'il-audit.json';if(Test-Path $out){throw 'Preserve worker IL audit'}
@{verified=$true;buildReceiptSha256=$pin;products=$r.outputs;bootstrapResourceSha256=$resourceHash;runtimeVersion='8.0.33';rollForward='Disable';workerExecuted=$false;readyToRunQualified=$false;runtimeQualified=$false;completeSdk=$false;readerSha256=(Get-FileHash $reader).Hash;runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 5|Set-Content $out -Encoding utf8
Get-FileHash $out
