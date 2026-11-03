$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/script-adapter-source-build-v2'
$output=Join-Path $root 'comparison.json'
if(Test-Path -LiteralPath $output){throw 'Preserve comparison'}
$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$Path,[string]$Expected){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $Expected){throw 'Changed comparison input'}
}
$result=@{passed=$false;error=$null;runtimeExecuted=$false}
try{
    $receipt=Join-Path $root 'result.json'
    Hold $receipt '7178C265B7BCAF50738DC8F610432F4C6B282DAB77442D689D06437CD3BD3E35'
    $build=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
    if(!$build.passed -or $build.ownerAttention -or !$build.inputsUnchanged){throw 'Build incomplete'}
    $il=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/ParentResourceIlAudit.cs'
    $api=Join-Path $workspace 'continuity-evidence/modern-worker-v1/InterpreterAssemblyAudit.cs'
    Hold $il 'B2438457F0CA2F9F54A9EB866335DF7EF23D50FB84F134F13B968C4FB6F557CC'
    Hold $api '372098D022AF9E29B14CFF5AE57574F101B333AFFE4F94019720D315E443DAB9'
    Add-Type -Path @($il,$api)
    $original=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-source-rebuilt-v1/runtime/Lea.Script.Win.dll'
    Hold $original '6E42A5D57F88519E19037659F7ADEAA204AD753D8E6FA88346F9854A81081912'
    Hold $build.binary $build.sha256
    $result.il=[ParentResourceIlAudit]::Compare($original,$build.binary)
    $a=[InterpreterAssemblyAudit]::Read($original);$b=[InterpreterAssemblyAudit]::Read($build.binary)
    foreach($property in @('Api','Resources','AllMethods','AssemblyReferences','OptimizationMethods')){
        if(@(Compare-Object -ReferenceObject $a.$property -DifferenceObject $b.$property -CaseSensitive).Count){throw "Metadata/resource mismatch: $property"}
    }
    if($a.Identity -cne $b.Identity){throw 'Identity mismatch'}
    $result.originalSha256=(Get-FileHash -LiteralPath $original).Hash
    $result.rebuiltSha256=$build.sha256
    $result.apiResourcesReferencesIdentical=$true;$result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    foreach($lease in $leases){$lease.Dispose()}
    [IO.File]::WriteAllText($output,($result|ConvertTo-Json -Depth 7))
}
[pscustomobject]$result|ConvertTo-Json -Depth 7
