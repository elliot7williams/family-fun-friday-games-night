$ErrorActionPreference='Stop'
$out='I:\LeaBuildStorage\sdk-reproducibility-audit-20261101-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve reproducibility audit'}
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$expected){$s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));if($expected -and $hash -cne $expected){throw ('Reproducibility audit differs: '+$path)};$inputs.Add(@{path=$path;sha256=$hash})}
function Proof([string]$path,[string]$hash){Pin $path $hash;return Get-Content -LiteralPath $path -Raw|ConvertFrom-Json}
try{
    $identity=Proof 'I:\LeaBuildStorage\sdk-build-identity-20261101-v1/result.json' '1459B734A83DB5C498642696EF562F1653D1A97974242EACB7DCDA3009AFAB90'
    $reader=Proof 'I:\LeaBuildStorage\sdk-identity-reader-controls-20261101-v1/result.json' 'F037C1A81C4C4A54DBF4075AE87FAE3E8C9042203339E62974B42D9F4CED457B'
    if(!$identity.verified -or !$reader.passed -or $reader.positiveControls -ne 3 -or $reader.negativeControls -ne 2 -or $reader.targetCodeExecuted){throw 'Identity reader evidence incomplete'}
    $old=Proof 'I:\LeaBuildStorage\full-sdk-source-20261101-v2/source-manifest.json' 'E796F6ED2A1AA4C5A18962A1A03D75359EA951A4A35DC117DF1411EEDD8E3A2C'
    $source=Proof 'I:\LeaBuildStorage\full-sdk-source-20261101-v3/source-manifest.json' 'B5A0283803C7062DA98A7F3331D94940360E00045A06038168FCA94214CD114A'
    if(!$source.verified -or !$source.deterministicSettingsIncluded -or $source.productBinariesIncluded -or $source.files.Count -ne 132 -or $old.files.Count -ne 131){throw 'Source-only candidate differs'}
    foreach($file in $old.files|Where-Object path -cne 'README.md'){$new=@($source.files|Where-Object path -ceq $file.path);if($new.Count -ne 1 -or $new[0].sha256 -cne $file.sha256){throw 'Non-build engine source changed'}}
    $build=Proof 'I:\LeaBuildStorage\sdk-reproducibility-20261101-v1/result.json' '39955D0AC2D23B60360000414FDB3702F928B4CA1537C074D2C6007BD174B199'
    $consumers=Proof 'I:\LeaBuildStorage\repro-sdk-consumers-20261101-v1/result.json' 'B8AA0C9ABCA57492875D09471D29AE809C17D89E748C2ACF3FD389161360C927'
    if(!$build.passed -or !$build.binaryReproducible -or $build.products.Count -ne 16 -or $build.differences.Count -or $build.runs.Count -ne 2 -or !$consumers.passed -or !$consumers.workerAclRestored -or $consumers.workerAclBefore.Count -ne 6 -or $consumers.runs.Count -ne 4){throw 'Complete reproducible builds and focused runtime required'}
    foreach($r in @($build,$consumers)){
        if(!$r.inputsUnchanged -or $r.ownerAttention){throw 'Build/runtime inputs or ownership incomplete'}
        foreach($file in $r.inputs){Pin $file.path $file.sha256}
        foreach($run in $r.runs){
            $budget=if($run.name.StartsWith('build-') -or $run.name.StartsWith('compile-')){120000}else{30000}
            if(!$run.passed -or $run.exitCode -ne 0 -or !$run.originalHandleCaptured -or !$run.exitObserved -or $run.timedOut -or $run.killRequested -or !$run.outputDrained -or !$run.childRetirementIndependentlyAudited -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Original child/process/budget guards differ'}
            Pin $run.stdout;Pin $run.stderr
        }
    }
    foreach($file in $build.products){Pin $file.first $file.sha256;Pin $file.second $file.secondSha256;if(!$file.equal -or $file.sha256 -cne $file.secondSha256){throw 'Raw binary hash equality absent'}}
    foreach($file in $consumers.workerAclBefore){if((Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Current worker ACL no longer restored'}}
    foreach($pair in @(@('text-consumer','PASS LEA SDK EMBEDDING 14'),@('script-consumer','PASS LEA SDK SCRIPT SAMPLE 17'),@('worker-gate','PASS SDK WORKER 25'))){$run=@($consumers.runs|Where-Object name -ceq $pair[0]);if($run.Count -ne 1 -or @(Get-Content $run[0].stdout|Where-Object {$_ -ceq $pair[1]}).Count -ne 1){throw 'Deterministic consumer footer differs'}}
    $targets=Join-Path $build.roots[0] 'Directory.Build.targets';$xml=[xml](Get-Content $targets -Raw)
    if($xml.Project.PropertyGroup.Deterministic -cne 'true' -or $xml.Project.PropertyGroup.PathMap.InnerText -cne '$(MSBuildThisFileDirectory)=/_/lea/'){throw 'Package-scoped build settings differ'}
    foreach($file in $inputs){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'Reproducibility audit input changed'}}
    New-Item -ItemType Directory -Path $out|Out-Null
    @{verified=$true;rawIdenticalProducts=16;freshSourceDirectories=2;ownedChildren=6;textChecks=14;scriptChecks=17;workerChecks=25;identityReaderPositiveControls=3;identityReaderNegativeControls=2;runtimeEngineSourceChanged=$false;crossMachineReproducibilityQualified=$false;broaderRegressionQualified=$false;timingFailuresResolved=$false;hostAdopted=$false;inputs=$inputs;scope='Same-machine two-directory complete builds with fixed compiler/references/toolchain and raw SHA equality, plus exact new-image focused runtime checks. No semantic masking used to accept products; no transfer of older broader passes.'}|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'result.json')
}finally{foreach($s in $holds){$s.Dispose()}}
