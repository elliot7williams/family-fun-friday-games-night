$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/worker-archive-native-v1'
$output=Join-Path $root 'rebuild-verification.json'
if(Test-Path -LiteralPath $output){throw 'Preserve worker verification'}
$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$Path,[string]$Hash){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $Hash){throw 'Verification input changed'}
}
$result=@{passed=$false;error=$null;runtimeExecuted=$false;rawFailurePreserved=$true;releaseCertified=$false}
try{
    $receipt=Join-Path $root 'result.json';Hold $receipt 'A52E8CCEE757DAF7B5ED66DD4816A3BDC9BA962C238E67EA0E08479F365A5ED6'
    $build=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
    if($build.passed -or $build.error -or !$build.inputsUnchanged -or $build.ownerAttention -or !$build.byteIdenticalToTestedNative -or $build.maps.Count -ne 0 -or $build.runs.Count -ne 1){throw 'Unexpected raw build state'}
    $run=$build.runs[0]
    if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.stdoutComplete -or !$run.stderrComplete -or !$run.exitObserved){throw 'Compiler lifecycle incomplete'}
    foreach($input in $build.inputs){Hold $input.path $input.sha256}
    $sourceProof=Join-Path $workspace 'continuity-evidence/worker-archive-build-v1/result.json'
    Hold $sourceProof '15A9162F4B9E06F0D8455FC05D2250C62DC57127BACF56F9CAB6FC39FCADA75D'
    $source=Get-Content -LiteralPath $sourceProof -Raw|ConvertFrom-Json
    if(!$source.passed -or !$source.byteIdentical -or !$source.inputsUnchanged -or $source.ownerAttention -or $source.sources -ne 17){throw 'Source rebuild incomplete'}
    $oldRecipe=Join-Path $workspace 'continuity-evidence/worker-archive-build-v1/archive/recipes/native.rsp'
    Hold $oldRecipe 'AABDDE8A810BA4143501045AA128E4500479EE2273269F08353F88143AF83F03'
    $lines=[IO.File]::ReadAllLines($oldRecipe);$actual=[IO.File]::ReadAllLines((Join-Path $root 'native.rsp'))
    if($actual.Length -ne $lines.Length){throw 'Recipe length differs'}
    $outputs=0
    for($i=0;$i -lt $lines.Length;$i++){
        $expected=$lines[$i]
        if($i -eq 0){$expected='"'+$source.binary+'"'}
        elseif($expected.StartsWith('-o:')){$expected='-o:"'+(Join-Path $root 'Lea.Script.Worker.dll')+'"';$outputs++}
        if($actual[$i] -cne $expected){throw 'Native recipe differs beyond input/output relocation'}
        if($lines[$i] -match '^--?(?:map|mapcsv)(?:[:=]|$)'){throw 'Recipe unexpectedly requests maps'}
    }
    if($outputs -ne 1 -or @(Get-ChildItem -LiteralPath $root -Filter '*.csv' -File).Count){throw 'Unexpected native outputs'}
    $original=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/paired-worker-native-v1/Lea.Script.Worker.dll'
    $binary=Join-Path $root 'Lea.Script.Worker.dll'
    $expectedHash='60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'
    Hold $original $expectedHash;Hold $binary $expectedHash
    if([Convert]::ToBase64String([IO.File]::ReadAllBytes($original)) -cne [Convert]::ToBase64String([IO.File]::ReadAllBytes($binary))){throw 'Native bytes differ'}
    $helper=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/ParentResourceIlAudit.cs'
    Hold $helper 'B2438457F0CA2F9F54A9EB866335DF7EF23D50FB84F134F13B968C4FB6F557CC'
    Add-Type -Path $helper
    $result.il=[ParentResourceIlAudit]::Compare($source.binary,$binary)
    $result.byteIdenticalToTestedNative=$true;$result.recipeOnlyInputOutputRelocated=$true
    $result.expectedMaps=0;$result.rawFailureReason='Inherited two-map assertion is incompatible with the original worker recipe, which requests no maps.'
    $result.sha256=$expectedHash;$result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    foreach($lease in $leases){$lease.Dispose()}
    [IO.File]::WriteAllText($output,($result|ConvertTo-Json -Depth 7))
}
[pscustomobject]$result|ConvertTo-Json -Depth 7
