param([ValidateSet(1,2)][int]$Attempt=1)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace ('continuity-evidence/archived-native-build-v'+$Attempt)
$out=Join-Path $root 'comparison.json'
if(Test-Path -LiteralPath $out){throw 'Preserve comparison'}
$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$Path,[string]$Expected){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $Expected){throw 'Comparison input changed'}
}
$result=@{passed=$false;modules=[Collections.Generic.List[object]]::new();error=$null;runtimeExecuted=$false}
try{
    $buildHash=if($Attempt -eq 1){'C72B87A88BB5D164EA178BBAD36329C120142AF432D9A206240B2DBCF36AFA23'}else{'FF5F10C3F971579BB51228B57E2D0D3921BF75727E8F8C3647A82F62C7603259'}
    $path=Join-Path $root 'result.json';Hold $path $buildHash
    $build=Get-Content -LiteralPath $path -Raw|ConvertFrom-Json
    if(!$build.passed -or $build.ownerAttention -or !$build.inputsUnchanged -or $build.modules.Count -ne 2){throw 'Incomplete build'}
    $il=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/ParentResourceIlAudit.cs'
    $api=Join-Path $workspace 'continuity-evidence/modern-worker-v1/InterpreterAssemblyAudit.cs'
    Hold $il 'B2438457F0CA2F9F54A9EB866335DF7EF23D50FB84F134F13B968C4FB6F557CC'
    Hold $api '372098D022AF9E29B14CFF5AE57574F101B333AFFE4F94019720D315E443DAB9'
    Add-Type -Path @($il,$api)
    foreach($module in $build.modules){
        $name=switch -CaseSensitive ($module.module){'Lea.Engine'{'Lea.Engine.dll'} 'Lea.Image'{'Lea.Image.exe'} default{throw 'Unknown module'}}
        $original=Join-Path $workspace ('continuity-evidence/request-diagnostics-v1/verified-build-20261020-150037/native/runtime/'+$name)
        Hold $original $module.originalTestedSha256;Hold $module.binary $module.sha256
        $comparison=[ParentResourceIlAudit]::Compare($original,$module.binary)
        $a=[InterpreterAssemblyAudit]::Read($original);$b=[InterpreterAssemblyAudit]::Read($module.binary)
        foreach($property in @('Api','Resources','AllMethods','AssemblyReferences','OptimizationMethods')){
            if(@(Compare-Object -ReferenceObject $a.$property -DifferenceObject $b.$property -CaseSensitive).Count){throw "Metadata mismatch: $property"}
        }
        if($a.Identity -cne $b.Identity){throw 'Assembly identity changed'}
        $result.modules.Add(@{module=$module.module;originalSha256=$module.originalTestedSha256;rebuiltSha256=$module.sha256;il=$comparison;checkedApiResourcesReferencesIdentical=$true})
    }
    $result.passed=$result.modules.Count -eq 2
}catch{$result.error=$_.Exception.ToString()}
finally{
    foreach($lease in $leases){$lease.Dispose()}
    [IO.File]::WriteAllText($out,($result|ConvertTo-Json -Depth 7))
}
[pscustomobject]$result|ConvertTo-Json -Depth 6
