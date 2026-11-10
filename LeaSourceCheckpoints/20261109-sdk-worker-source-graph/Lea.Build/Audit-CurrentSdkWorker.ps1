param([Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$ReceiptSha256)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$folder='I:\LeaBuildStorage\sdk-worker-source-graph-20261109-v1'
$source=Join-Path $folder 'source'
$out='I:\LeaBuildStorage\sdk-worker-source-graph-audit-20261109-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve worker source audit'}
$receipt=Join-Path $folder 'result.json'
if((Get-FileHash -LiteralPath $receipt).Hash -cne $ReceiptSha256){throw 'Worker source receipt differs'}
$r=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.error -or $r.changedFiles.Count -or $r.nativeProducts.Count -ne 6 -or $r.libraryProducts.Count -ne 6 -or $r.runs.Count -ne 9 -or $r.textChecks -ne 14 -or $r.scriptChecks -ne 17 -or $r.workerChecks -ne 25 -or $r.completeSdk -or $r.startupFixed -or $r.hostAdopted -or $r.fullScriptSuiteQualified -or $r.globalSecurityQualified){throw 'Worker source scope differs'}
foreach($file in $r.inputs){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw ('Input drift: '+$file.path)}}
foreach($file in $r.before){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256 -or (Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Runtime byte/ACL drift'}}
for($i=0;$i -lt $r.runs.Count;$i++){
    $run=$r.runs[$i];$budget=if($i -eq 0 -or $i -gt 5){30000}else{120000}
    if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.ownedJob -or !$run.originalHandleCaptured -or !$run.childRetirementIndependentlyAudited -or !$run.exitObserved -or !$run.outputDrained -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.captureOverflow -or $run.error){throw 'Owned run/deadline differs'}
}
foreach($pair in @(@($r.runs[6].stdout,'PASS LEA SDK EMBEDDING 14'),@($r.runs[7].stdout,'PASS LEA SDK SCRIPT SAMPLE 17'),@($r.runs[8].stdout,'PASS SDK WORKER 25'))){if(@(Get-Content -LiteralPath $pair[0]|Where-Object {$_ -ceq $pair[1]}).Count -ne 1){throw 'Runtime footer differs'}}
$sourcePath=Join-Path $folder 'source.json'
if((Get-FileHash -LiteralPath $sourcePath).Hash -cne '51A823667E96D3ED4C3EC526E0747B8C19B1EBBE59A8A05324503DBEB4FED9C9'){throw 'Export manifest differs'}
$manifest=Get-Content -LiteralPath $sourcePath -Raw|ConvertFrom-Json
if(!$manifest.verified -or $manifest.sourceMembers -ne 128 -or $manifest.dependencyMembers -ne 8 -or $manifest.files.Count -ne 136){throw 'Export inventory differs'}
foreach($file in @($manifest.inputs)+@($manifest.files|ForEach-Object {@{path=(Join-Path $source $_.path);sha256=$_.sha256}})){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'Export source drift'}}
$frameworkAudit='I:\LeaBuildStorage\sdk-print-source-graph-audit-20261109-v1\audit.json'
if((Get-FileHash -LiteralPath $frameworkAudit).Hash -cne '24A2A32A5FAAB645E3A44E7915725AC4EB9C42CC6CE6DC9C56DDEDEB201A8E03'){throw 'Preceding framework audit differs'}
$prior=Get-Content -LiteralPath $frameworkAudit -Raw|ConvertFrom-Json
foreach($file in $prior.sourceFiles){if((Get-FileHash -LiteralPath (Join-Path $source $file.path)).Hash -cne $file.sha256){throw 'Preceding source changed'}}
foreach($product in $r.libraryProducts){
    if((Get-FileHash -LiteralPath $product.path).Hash -cne $product.sha256){throw 'Framework output drift'}
    $same=@($prior.products|Where-Object {[IO.Path]::GetFileName($_.path) -ceq [IO.Path]::GetFileName($product.path)})
    if($same.Count -ne 1 -or $same[0].sha256 -cne $product.sha256){throw 'Framework product changed; do not transfer earlier qualification'}
}
$original='I:\LeaBuildStorage\github-source-checkpoint-20261109-v1\LeaSourceCheckpoints\20261102\mature-sdk'
$originalManifest=Join-Path $original 'source-manifest.json'
if((Get-FileHash -LiteralPath $originalManifest).Hash -cne 'B5A0283803C7062DA98A7F3331D94940360E00045A06038168FCA94214CD114A'){throw 'Original SDK inventory differs'}
$workerSources=@((Get-Content -LiteralPath $originalManifest -Raw|ConvertFrom-Json).files|Where-Object {$_.path -match '^Lea.Script.Worker/[^/]+\.cs$'})
if($workerSources.Count -ne 17){throw 'Worker C# source count differs'}
foreach($file in $workerSources){if((Get-FileHash -LiteralPath (Join-Path $source $file.path)).Hash -cne $file.sha256){throw 'Worker C# behavior changed'}}
foreach($name in @('Lea.Script/Dom.compact.js','Lea.Script.Worker/Dom.compact.js')){if((Get-FileHash -LiteralPath (Join-Path $source $name)).Hash -cne '196BCE962F2C2F375C77644D0AE19485820F599170D4D80013829A6ECD2886E3'){throw 'Parent/worker bootstrap resource mismatch'}}
$worker=Join-Path $source 'bin/Release/ScriptWorker'
$cache=Get-Content -LiteralPath ($worker+'.build.json') -Raw|ConvertFrom-Json
if(!$cache.succeeded -or $cache.runtimeVersion -cne '8.0.33' -or $cache.compilerVersion -cne '8.0.32' -or $cache.outputs.Count -ne 6 -or (Get-ChildItem -LiteralPath $worker -File -Force).Count -ne 6){throw 'Native build cache scope differs'}
foreach($file in $cache.inputs){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'Native compile input drift'}}
foreach($file in $cache.outputs){if((Get-FileHash -LiteralPath (Join-Path $worker $file.name)).Hash -cne $file.sha256){throw 'Native output drift'}}
Add-Type -Path (Join-Path $source 'NativeWorker.Tools.cs')
foreach($name in @('Lea.Script.Worker.dll','Jint.dll','Acornima.dll')){if(![Lea.Sdk.NativeWorkerTools]::IsX64ReadyToRun((Join-Path $worker $name))){throw 'Native PE header differs'}}
$policy=(Get-Content -LiteralPath (Join-Path $worker 'Lea.Script.Worker.runtimeconfig.json') -Raw|ConvertFrom-Json).runtimeOptions
if($policy.framework.name -cne 'Microsoft.NETCore.App' -or $policy.framework.version -cne '8.0.33' -or $policy.rollForward -cne 'Disable'){throw 'Worker runtime policy differs'}
$runtime=Join-Path $root 'continuity-evidence/sdk-worker-source-graph-v1'
foreach($product in $r.nativeProducts){if((Get-FileHash -LiteralPath $product.path).Hash -cne $product.sha256 -or (Get-FileHash -LiteralPath (Join-Path $runtime ('script/ScriptWorker/'+[IO.Path]::GetFileName($product.path)))).Hash -cne $product.sha256){throw 'Runtime worker is not native producer output'}}
foreach($product in $r.libraryProducts|Where-Object {[IO.Path]::GetExtension($_.path) -ceq '.dll'}){if((Get-FileHash -LiteralPath (Join-Path $runtime ('script/'+[IO.Path]::GetFileName($product.path)))).Hash -cne $product.sha256){throw 'Script consumer is not framework producer output'}}
$xml=[xml](Get-Content -LiteralPath (Join-Path $source 'Lea.Script.Worker/Lea.Script.Worker.csproj') -Raw)
if($xml.Project.PropertyGroup.OutputPath -cne '..\worker-il\$(Configuration)\'){throw 'Worker IL must remain separate from native runtime'}
New-Item -ItemType Directory -Path $out|Out-Null
$copies=@(foreach($file in Get-ChildItem -LiteralPath $folder -File|Where-Object Extension -in @('.json','.txt','.ps1','.cs')){Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $out $file.Name);$hash=(Get-FileHash -LiteralPath $file.FullName).Hash;if((Get-FileHash -LiteralPath (Join-Path $out $file.Name)).Hash -cne $hash){throw 'Raw evidence copy differs'};@{path=(Join-Path $out $file.Name);sha256=$hash}})
$pictures=@(foreach($dir in @((Join-Path $folder 'text'),(Join-Path $runtime 'script'))){Get-ChildItem -LiteralPath $dir -Filter '*.png' -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash}}})
@{verified=$true;receiptSha256=$ReceiptSha256;sourceReceiptSha256=(Get-FileHash -LiteralPath $sourcePath).Hash;sourceMembers=128;dependencyMembers=8;ownedRuns=9;textChecks=14;scriptChecks=17;workerChecks=25;runtimeFiles=$r.before.Count;libraryProducts=$r.libraryProducts;nativeProducts=$r.nativeProducts;copies=$copies;pictures=$pictures;nativeBuildReceiptSha256=(Get-FileHash -LiteralPath ($worker+'.build.json')).Hash;scriptSha256=(Get-FileHash -LiteralPath $PSCommandPath).Hash;completeSdk=$false;startupFixed=$false;hostAdopted=$false;fullScriptSuiteQualified=$false;globalSecurityQualified=$false;scope='Current unified source builds and bounded consumer/isolated-worker fixtures, not full scripting/reliability/site/security or browser release.'}|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'audit.json')|Format-List Hash
