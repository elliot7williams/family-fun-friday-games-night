$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\portable-sdk-build-audit-20261101-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve portable SDK audit'}
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$expected){$s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));if($expected -and $hash -cne $expected){throw ('SDK audit differs: '+$path)};$inputs.Add(@{path=$path;sha256=$hash})}
function Proof([string]$path,[string]$hash){Pin $path $hash;return Get-Content -LiteralPath $path -Raw|ConvertFrom-Json}
try{
    $native=Proof 'I:\LeaBuildStorage\portable-native-build-20261101-v1/result.json' 'DB9683B3B9F271930993BC72036E093A948A48B0184DF998FE05D070F2FD17FA'
    $full=Proof 'I:\LeaBuildStorage\portable-full-sdk-build-20261101-v1/result.json' '17AAF573A110588C3D9A0C43944465B0504B44865CBBAFDFBE6C9D319C29C2C7'
    $gate=Proof 'I:\LeaBuildStorage\portable-sdk-worker-gate-20261101-v1/result.json' '9F293CD17DED933C51D4D9D55EE6896AB6DE77B99B8DE4B6688CEB6548AB7ADC'
    foreach($r in @($native,$full,$gate)){
        if(!$r.passed -or !$r.inputsUnchanged -or $r.ownerAttention){throw 'Passing retired exact-candidate campaign required'}
        foreach($file in $r.inputs){Pin $file.path $file.sha256}
        foreach($run in $r.runs){
            $budget=if($run.name -in 'record-toolchain','test-text-consumer','test-script-consumer','test-worker-gate'){30000}else{120000}
            $expected=if($run.name.StartsWith('deny-')){1}else{0}
            if(!$run.exitObserved -or !$run.originalHandleCaptured -or !$run.ownedJob -or $run.exitCode -ne $expected -or $run.timedOut -or $run.killRequested -or !$run.outputDrained -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.captureOverflow -or !$run.childRetirementIndependentlyAudited -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Original build/control/consumer process guards differ'}
            Pin $run.stdout;Pin $run.stderr
        }
    }
    if(!$native.identicalToQualifiedWorker -or $native.checks.Count -ne 8 -or $native.runs.Count -ne 9 -or $native.outputs.Count -ne 6 -or !$full.sourceBuildPassed -or !$full.samplesPassed -or !$full.workerAclRestored -or $full.runs.Count -ne 3 -or $full.libraryProducts.Count -ne 6 -or $full.nativeProducts.Count -ne 6 -or !$gate.workerAclRestored -or $gate.runs.Count -ne 2){throw 'Expected SDK build/sample/worker coverage differs'}
    $expectedFailures=@{'deny-missing-manifest'='Provide a verified native-toolchain manifest';'deny-wrong-hash'='Native build input hash differs';'deny-reference-inventory'='Toolchain inventory changed';'deny-il-dependency'='ReadyToRun interpreter dependencies required';'deny-runtime-policy'='Exact worker runtime policy required';'deny-tampered-cache'='Native build input hash differs'}
    foreach($name in $expectedFailures.Keys){$run=@($native.runs|Where-Object name -ceq $name);if($run.Count -ne 1 -or (Get-Content $run[0].stdout -Raw) -notmatch [regex]::Escape($expectedFailures[$name])){throw 'Negative failed outside its intended guard'}}
    foreach($r in @($full,$gate)){foreach($file in $r.workerAclBefore){if((Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Current worker ACL no longer restored'}}}
    foreach($file in @($native.outputs)+@($full.libraryProducts)+@($full.nativeProducts)){Pin $file.path $file.sha256}
    $text=@($full.runs|Where-Object name -ceq 'test-text-consumer')[0];$script=@($full.runs|Where-Object name -ceq 'test-script-consumer')[0];$worker=@($gate.runs|Where-Object name -ceq 'test-worker-gate')[0]
    foreach($pair in @(@($text,'PASS LEA SDK EMBEDDING 14'),@($script,'PASS LEA SDK SCRIPT SAMPLE 17'),@($worker,'PASS SDK WORKER 25'))){if(@(Get-Content $pair[0].stdout|Where-Object {$_ -ceq $pair[1]}).Count -ne 1){throw 'Exact consumer/worker footer differs'}}
    $stage='I:\LeaBuildStorage\full-sdk-source-20261101-v2'
    $source=Proof (Join-Path $stage 'source-manifest.json') 'E796F6ED2A1AA4C5A18962A1A03D75359EA951A4A35DC117DF1411EEDD8E3A2C'
    if(!$source.verified -or $source.productBinariesIncluded -or !$source.portableNativeBuildIncluded -or $source.files.Count -ne 131 -or $source.thirdPartyDependencyFiles -ne 8){throw 'Source-only package differs'}
    foreach($file in $source.files){Pin (Join-Path $stage $file.path) $file.sha256}
    $project=[xml](Get-Content (Join-Path $stage 'SDK.Build.proj') -Raw)
    if($project.Project.DefaultTargets -cne 'BuildLibraries'){throw 'Default build unexpectedly selects native tools'}
    $aliases=@{Bl='BuildLibraries';Bi='BuildWorkerIl';Bn='BuildWorkerNative';Bs='BuildSamples';Ba='BuildAll'}
    foreach($name in $aliases.Keys){$target=@($project.Project.Target|Where-Object Name -ceq $name);if($target.Count -ne 1 -or $target[0].DependsOnTargets -cne $aliases[$name]){throw 'Short alias differs'}}
    $nativeTarget=@($project.Project.Target|Where-Object Name -ceq 'CompileWorkerNative')[0]
    if($nativeTarget.Exec.Timeout -ne 120000 -or $nativeTarget.Exec.Command -notmatch '\$\(MSBuildThisFileDirectory\)Build-NativeWorker.ps1'){throw 'Portable native target changed'}
    foreach($file in $inputs){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'SDK audit input changed'}}
    New-Item -ItemType Directory -Path $out|Out-Null
    @{verified=$true;portableNativeControls=8;completeExportedSourceBuild=$true;sourceFiles=131;textConsumerChecks=14;scriptConsumerChecks=17;workerChecks=25;ownedChildren=14;sourceOnlyBundleIncludesThirdPartyDependencies=$true;sourceOnlyBundleIncludesLeaProducts=$false;broaderCandidateRegressionQualified=$false;fullSdkQualified=$false;hostAdopted=$false;timingFailuresResolved=$false;inputs=$inputs;scope='Exact local source-build and focused consumer/isolation coverage only. New DLL hashes require broader regression; earlier startup/execution failures and all six release gates remain open.'}|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'result.json')
}finally{foreach($s in $holds){$s.Dispose()}}
