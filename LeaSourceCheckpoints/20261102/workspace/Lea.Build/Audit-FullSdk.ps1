$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$stage='I:\LeaBuildStorage\full-sdk-source-20261101-v1'
$out='I:\LeaBuildStorage\full-sdk-audit-20261101-v1'
if(Test-Path $out){throw 'Preserve aggregate SDK audit'}
if(@(Get-Process 'Lea.Script.Worker','Lea.ScriptEmbeddingSample','Lea.EmbeddingSample','FullSdkWorkerGate' -ErrorAction SilentlyContinue).Count){throw 'No test overlap'}
$holds=[Collections.Generic.List[IDisposable]]::new();$pins=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$expected){
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($expected -and $hash -cne $expected){throw ('Aggregate audit identity differs: '+$path)};$pins.Add(@{path=$path;sha256=$hash})
}
function ReadProof([string]$path,[string]$hash){Pin $path $hash;return Get-Content $path -Raw|ConvertFrom-Json}
try{
    $source=ReadProof (Join-Path $stage 'source-manifest.json') 'C32C06E18945BEEFDA035E4657BA8B30D2350012FD6B9DAC33FEFAB3D58100A3'
    if(!$source.verified -or $source.files.Count -ne 128 -or !$source.workerProjectInverseVerified -or $source.productBinariesIncluded -or $source.workerProjectChanges.Count -ne 3){throw 'Source assembly incomplete'}
    foreach($file in $source.files){Pin (Join-Path $stage $file.path) $file.sha256}
    foreach($name in @('Jint.dll','Acornima.dll')){if((Get-FileHash (Join-Path $stage ('lib/'+$name))).Hash -ceq (Get-FileHash (Join-Path $stage ('lib/worker/'+$name))).Hash){throw 'Parent/worker dependency boundary lost'}}
    $libraries=ReadProof 'I:\LeaBuildStorage\full-sdk-libraries-build-20261101-v1/result.json' '3AF0DA862CF1FA7C1E3E53E949B7EA0F37DE6593BEE8DDC5A02EFA8653522E6B'
    $native=ReadProof 'I:\LeaBuildStorage\full-sdk-native-build-20261101-v1/result.json' '20E2EE1F8A87BFC2B14CD581E48B59D1DE834E87EBA729DCB5A45CCF4E1A7893'
    $worker=ReadProof 'I:\LeaBuildStorage\full-sdk-worker-gate-20261101-v1/result.json' '479876832FDD6A811032A6857E4036A9DDDBBCA3DDB1AE34C2F6A0B49EEA1EFC'
    if(!$libraries.compiled -or $libraries.libraryProducts.Count -ne 6 -or !$native.workerIlCompiled -or !$native.workerNativeCompiled -or $native.nativeProducts.Count -ne 6){throw 'Component build coverage incomplete'}
    if(!$native.workerAclRestored -or !$worker.workerAclRestored){throw 'Worker ACL closure incomplete'}
    $closures=@(@($libraries,5),@($native,4),@($worker,2))
    foreach($closure in $closures){
        $proof=$closure[0]
        if(!$proof.passed -or !$proof.inputsUnchanged -or $proof.ownerAttention -or $proof.runs.Count -ne $closure[1]){throw 'Passing retired build/test prerequisite required'}
        foreach($file in $proof.inputs){Pin $file.path $file.sha256}
        foreach($run in $proof.runs){
            $budget=if($run.name.StartsWith('test-')){30000}else{120000}
            if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or $run.parentBudgetMs -ne $budget -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or !$run.exitObserved -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Original owned guard/retirement failure'}
        }
        foreach($file in $proof.workerAclBefore){if((Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Current worker ACL differs'}}
    }
    foreach($file in @($libraries.libraryProducts)+@($native.nativeProducts)){Pin $file.path $file.sha256}
    foreach($spec in @(@($libraries,'test-text-sample','PASS LEA SDK EMBEDDING 14'),@($libraries,'test-parent-contract','PASS SDK PARENT IO CONTRACT 3'),@($native,'test-aggregate-script-sample','PASS LEA SDK SCRIPT SAMPLE 17'),@($worker,'test-worker-gate','PASS SDK WORKER 25'))){
        $run=@($spec[0].runs|Where-Object name -ceq $spec[1]);if($run.Count -ne 1){throw 'Unique test run missing'}
        Pin $run[0].stdout ''
        if(@(Get-Content $run[0].stdout|Where-Object {$_ -ceq $spec[2]}).Count -ne 1){throw 'Exact terminal footer missing'}
    }
    $reader=Join-Path $root 'Lea.Build/ReadReadyToRun.cs';Pin $reader '';Add-Type -Path $reader
    foreach($name in @('Lea.Script.Worker.dll','Jint.dll','Acornima.dll')){if(![ReadReadyToRun]::IsX64((Join-Path $stage ('bin/Release/ScriptWorker/'+$name)))){throw 'Native closure lacks x64 ReadyToRun headers'}}
    New-Item -ItemType Directory -Path $out|Out-Null
    @{verified=$true;inputs=$pins;sourceMembers=128;frameworkProducts=6;nativeWorkerFiles=6;ownedProcesses=11;originalGuardsPreserved=$true;dependencyLanesSeparated=$true;completeSdk=$false;fullSuiteQualified=$false;hostAdopted=$false;noticesComplete=$false}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'result.json')
}finally{foreach($hold in $holds){$hold.Dispose()}}
