$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/archived-parent-view-build-v3'
$output=Join-Path $root 'comparison.json'
if(Test-Path -LiteralPath $output){throw 'Preserve comparison'}
$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$Path,[string]$Expected){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $Expected){throw 'Changed comparison input'}
}
$result=@{passed=$false;modules=[Collections.Generic.List[object]]::new();error=$null;runtimeExecuted=$false}
try{
    $receipt=Join-Path $root 'result.json'
    Hold $receipt '9D29961B492A52A457A32E75AB11BC8EAB2A955A3096480F417DA0484EC54630'
    $build=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
    if(!$build.passed -or $build.ownerAttention -or !$build.inputsUnchanged -or $build.modules.Count -ne 2){throw 'Build incomplete'}
    $il=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/ParentResourceIlAudit.cs'
    $api=Join-Path $workspace 'continuity-evidence/modern-worker-v1/InterpreterAssemblyAudit.cs'
    Hold $il 'B2438457F0CA2F9F54A9EB866335DF7EF23D50FB84F134F13B968C4FB6F557CC'
    Hold $api '372098D022AF9E29B14CFF5AE57574F101B333AFFE4F94019720D315E443DAB9'
    Add-Type -Path @($il,$api)
    foreach($module in $build.modules){
        $original=if($module.module -ceq 'parent'){Join-Path $workspace 'continuity-evidence/permission-contract-v1/early-limits-v1/Lea.Script.dll'}elseif($module.module -ceq 'view'){Join-Path $workspace 'continuity-evidence/permission-contract-v1/retry-view-v1/Lea.Win.dll'}else{throw 'Unknown module'}
        Hold $original $module.originalTestedSha256;Hold $module.binary $module.sha256
        $comparison=[ParentResourceIlAudit]::Compare($original,$module.binary)
        $a=[InterpreterAssemblyAudit]::Read($original);$b=[InterpreterAssemblyAudit]::Read($module.binary)
        foreach($property in @('Api','Resources','AllMethods','AssemblyReferences','OptimizationMethods')){
            if(@(Compare-Object -ReferenceObject $a.$property -DifferenceObject $b.$property -CaseSensitive).Count){throw "Metadata/resource mismatch: $property"}
        }
        if($a.Identity -cne $b.Identity){throw 'Identity mismatch'}
        $result.modules.Add(@{module=$module.module;originalSha256=$module.originalTestedSha256;rebuiltSha256=$module.sha256;il=$comparison;apiResourcesReferencesIdentical=$true})
    }
    $result.passed=$result.modules.Count -eq 2
}catch{$result.error=$_.Exception.ToString()}
finally{
    foreach($lease in $leases){$lease.Dispose()}
    [IO.File]::WriteAllText($output,($result|ConvertTo-Json -Depth 7))
}
[pscustomobject]$result|ConvertTo-Json -Depth 7
