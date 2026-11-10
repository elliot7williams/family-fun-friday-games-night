param([Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$ReceiptSha256)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$folder='I:\LeaBuildStorage\memory-policy-sdk-source-20261110-v1'
$source=Join-Path $folder 'source'
$out='I:\LeaBuildStorage\memory-policy-sdk-audit-20261110-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve SDK audit'}
function Pin([string]$Path,[string]$Hash){if((Get-FileHash -LiteralPath $Path).Hash -cne $Hash){throw ('SDK audit input differs: '+$Path)}}
$receipt=Join-Path $folder 'result.json';Pin $receipt $ReceiptSha256
$r=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.error -or $r.changedFiles.Count -or $r.nativeProducts.Count -ne 6 -or $r.libraryProducts.Count -ne 6 -or $r.runs.Count -ne 9 -or $r.textChecks -ne 14 -or $r.scriptChecks -ne 17 -or $r.workerChecks -ne 25 -or $r.completeSdk -or $r.fullScriptSuiteQualified -or $r.globalSecurityQualified){throw 'SDK scope incomplete'}
foreach($file in $r.inputs){Pin $file.path $file.sha256}
foreach($file in $r.before){Pin $file.path $file.sha256;if((Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'SDK runtime ACL drift'}}
for($i=0;$i -lt $r.runs.Count;$i++){$run=$r.runs[$i];$budget=if($i -eq 0 -or $i -gt 5){30000}else{120000};if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.ownedJob -or !$run.originalHandleCaptured -or !$run.childRetirementIndependentlyAudited -or !$run.exitObserved -or !$run.outputDrained -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.captureOverflow -or $run.error){throw 'Original owned SDK run contract differs'}}
foreach($pair in @(@($r.runs[6].stdout,'PASS LEA SDK EMBEDDING 14'),@($r.runs[7].stdout,'PASS LEA SDK SCRIPT SAMPLE 17'),@($r.runs[8].stdout,'PASS SDK WORKER 25'))){if(@((Get-Content -LiteralPath $pair[0])|Where-Object {$_ -ceq $pair[1]}).Count -ne 1){throw 'SDK consumer footer missing'}}
$sourcePath=Join-Path $folder 'source.json';Pin $sourcePath '21414F48C7A32E6B2ED0865CBA79A8DE3018A4B581075A7244E5ED534AB7E183'
$manifest=Get-Content -LiteralPath $sourcePath -Raw|ConvertFrom-Json
if(!$manifest.verified -or $manifest.sourceMembers -ne 128 -or $manifest.dependencyMembers -ne 8 -or $manifest.files.Count -ne 136 -or @($manifest.files|Where-Object changed).Count -ne 3 -or $manifest.seededLeaProducts){throw 'SDK source closure differs'}
foreach($file in $manifest.inputs){Pin $file.path $file.sha256}
foreach($file in $manifest.files){Pin (Join-Path $source $file.path) $file.sha256;if($file.changed -ne ($file.path -cin @('Lea.Engine/E.cs','Lea.Engine/P.cs','Lea.Win/V.cs'))){throw 'Changes outside guarded core/view'}}
$prepPath=Join-Path $folder 'preparation.json';Pin $prepPath '7DA2E84DFEE3ED7AB8589756CA46C3737DD01277A2776892CA1E675EE6325572'
$prep=Get-Content -LiteralPath $prepPath -Raw|ConvertFrom-Json
if(!$prep.verified -or !$prep.inverseVerified -or $prep.assertionsChanged -or $prep.deadlinesChanged -or $prep.nativeFallbackAllowed){throw 'Original SDK assertions/limits not retained'}
Pin $prep.runner $prep.runnerSha256
$template=Join-Path $PSScriptRoot 'Test-CurrentSdkWorker.ps1';Pin $template $prep.templateSha256
$inverse=[IO.File]::ReadAllText($prep.runner)
$inverse=$inverse.Replace(("'"+$PSScriptRoot.Replace("'","''")+"'"),'$PSScriptRoot')
$inverse=$inverse.Replace(("`$root='"+$root.Replace("'","''")+"'"),'$root=Split-Path $PSScriptRoot -Parent').Replace($folder,'I:\LeaBuildStorage\sdk-worker-source-graph-20261109-v1').Replace('continuity-evidence/memory-policy-sdk-v1','continuity-evidence/sdk-worker-source-graph-v1').Replace($prep.sourceSha256,'51A823667E96D3ED4C3EC526E0747B8C19B1EBBE59A8A05324503DBEB4FED9C9')
if($inverse -cne [IO.File]::ReadAllText($template)){throw 'SDK runner inverse differs'}
foreach($file in @($r.libraryProducts)+@($r.nativeProducts)){Pin $file.path $file.sha256}
$worker=Join-Path $source 'bin/Release/ScriptWorker'
$nativeReceipt=$worker+'.build.json'
$cache=Get-Content -LiteralPath $nativeReceipt -Raw|ConvertFrom-Json
if(!$cache.succeeded -or $cache.runtimeVersion -cne '8.0.33' -or $cache.compilerVersion -cne '8.0.32' -or $cache.outputs.Count -ne 6 -or (Get-ChildItem -LiteralPath $worker -File -Force).Count -ne 6){throw 'Native build cache scope differs'}
foreach($file in $cache.inputs){Pin $file.path $file.sha256}
foreach($file in $cache.outputs){Pin (Join-Path $worker $file.name) $file.sha256}
Add-Type -Path (Join-Path $source 'NativeWorker.Tools.cs')
foreach($name in @('Lea.Script.Worker.dll','Jint.dll','Acornima.dll')){if(![Lea.Sdk.NativeWorkerTools]::IsX64ReadyToRun((Join-Path $worker $name))){throw 'Native runtime PE header differs'}}
$policy=(Get-Content -LiteralPath (Join-Path $worker 'Lea.Script.Worker.runtimeconfig.json') -Raw|ConvertFrom-Json).runtimeOptions
if($policy.framework.name -cne 'Microsoft.NETCore.App' -or $policy.framework.version -cne '8.0.33' -or $policy.rollForward -cne 'Disable'){throw 'Runtime policy changed'}
$runtime=Join-Path $root 'continuity-evidence/memory-policy-sdk-v1'
foreach($product in $r.nativeProducts){Pin (Join-Path $runtime ('script/ScriptWorker/'+[IO.Path]::GetFileName($product.path))) $product.sha256}
foreach($product in $r.libraryProducts|Where-Object {[IO.Path]::GetExtension($_.path) -ceq '.dll'}){Pin (Join-Path $runtime ('script/'+[IO.Path]::GetFileName($product.path))) $product.sha256}
New-Item -ItemType Directory -Path $out|Out-Null
$copies=@(foreach($file in Get-ChildItem -LiteralPath $folder -File|Where-Object Extension -in @('.json','.txt','.ps1','.cs')){$to=Join-Path $out $file.Name;Copy-Item -LiteralPath $file.FullName -Destination $to;$hash=(Get-FileHash -LiteralPath $file.FullName).Hash;Pin $to $hash;@{path=$to;sha256=$hash}})
@{verified=$true;receiptSha256=$ReceiptSha256;sourceReceiptSha256=(Get-FileHash -LiteralPath $sourcePath).Hash;sourceMembers=128;dependencyMembers=8;sourceOverrides=3;ownedRuns=9;textChecks=14;scriptChecks=17;workerChecks=25;runtimeFiles=$r.before.Count;libraryProducts=$r.libraryProducts;nativeProducts=$r.nativeProducts;copies=$copies;completeSdkQualified=$false;memorySpecificChecksRerun=$false;fullScriptSuiteQualified=$false;hostAdopted=$false;globalSecurityQualified=$false;scriptSha256=(Get-FileHash -LiteralPath $PSCommandPath).Hash}|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'audit.json')
