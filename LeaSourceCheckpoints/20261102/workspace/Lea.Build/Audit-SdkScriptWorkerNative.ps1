$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$directory='I:\LeaBuildStorage\sdk-script-worker-native-20261101-v1'
$path=Join-Path $directory 'result.json';$pin='81690529F6933FA641969386747B6C9549BBD14D91F29B5CF1D70BF908CE6F97'
if((Get-FileHash $path).Hash -cne $pin){throw 'Native worker receipt differs'}
$r=Get-Content $path -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.error -or $r.workerExecuted -or $r.runtimeQualified -or $r.runs.Count -ne 1 -or $r.outputs.Count -ne 6 -or $r.compilerVersion -cne '8.0.32' -or $r.runtimeVersion -cne '8.0.33'){throw 'Native build gate incomplete'}
$run=$r.runs[0]
if(!$run.passed -or $run.exitCode -ne 0 -or !$run.exitObserved -or $run.timedOut -or $run.killRequested -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or $run.parentBudgetMs -ne 120000 -or $run.elapsedMs -gt 120000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Native compile lifecycle differs'}
foreach($file in @($r.inputs)+@($r.outputs)){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Native compiler input/output changed'}}
Add-Type -Path (Join-Path $PSScriptRoot 'ReadReadyToRun.cs')
Add-Type -Path (Join-Path $PSScriptRoot 'ReadBootstrapResource.cs')
$runtime=Join-Path $directory 'ScriptWorker'
foreach($name in @('Lea.Script.Worker.dll','Jint.dll','Acornima.dll')){if(![ReadReadyToRun]::IsX64((Join-Path $runtime $name))){throw 'ReadyToRun header missing'}}
$resource=[ReadBootstrapResource]::Read((Join-Path $runtime 'Lea.Script.Worker.dll'))
$resourceHash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($resource))
if($resourceHash -cne 'EC4B6513A77230C3B8439568CC3FED41B7712CA3779182D38076880DCC83BC8A'){throw 'Native worker bootstrap differs'}
$options=(Get-Content (Join-Path $runtime 'Lea.Script.Worker.runtimeconfig.json') -Raw|ConvertFrom-Json).runtimeOptions
if($options.framework.version -cne '8.0.33' -or $options.rollForward -cne 'Disable'){throw 'Native runtime policy differs'}
$out=Join-Path $directory 'native-audit.json';if(Test-Path $out){throw 'Preserve native audit'}
@{verified=$true;receiptSha256=$pin;bootstrapResourceSha256=$resourceHash;compilerVersion='8.0.32';targetRuntimeVersion='8.0.33';readyToRunHeadersVerified=$true;completeNativeCoverageProven=$false;workerExecuted=$false;runtimeQualified=$false;completeSdk=$false;runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json|Set-Content $out -Encoding utf8
Get-FileHash $out
