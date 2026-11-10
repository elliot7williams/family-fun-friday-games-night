param([switch]$GuardedSdk)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\current-firespider-runtime-20261110-v1'
if($GuardedSdk){$out='I:\LeaBuildStorage\guarded-sdk-host-runtime-20261110-v1'}
if(Test-Path -LiteralPath $out){throw 'Preserve current runtime'}
$inputs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
function Pin([string]$Path,[string]$Hash){
    $item=Get-Item -LiteralPath $Path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse runtime source refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($s)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $Hash){throw ('Runtime source differs: '+$Path)};$inputs.Add(@{path=$Path;sha256=$Hash})
}
function Relative([string]$Base,[string]$Path){$value=[IO.Path]::GetRelativePath($Base,$Path).Replace('\','/');if([IO.Path]::IsPathRooted($value) -or $value.Contains(':') -or $value.Split('/') -contains '..'){throw 'Runtime scope escape'};return $value}
try{
    Pin $PSCommandPath (Get-FileHash -LiteralPath $PSCommandPath).Hash
    $priorPath=Join-Path $root 'continuity-evidence/private-binding-host-workflows-v1/result.json';Pin $priorPath '3BEF980EFE9D141AD7E0830BC7EBCF1B59808A899D17532AC14301BBA3A48956'
    $prior=Get-Content -LiteralPath $priorPath -Raw|ConvertFrom-Json
    if(!$prior.passed -or !$prior.completed -or !$prior.inputsUnchanged -or $prior.ownerAttention -or $prior.before.Count -ne 400 -or $prior.byteChanges.Count -or $prior.aclChanges.Count){throw 'Preserved host assets/harness provenance differs'}
    $buildPath='I:\LeaBuildStorage\current-firespider-build-20261109-v1\result.json';$buildHash='CD72727F31282AABB42DABBEE2F55BCA69C63A9352A3528C8EB5657A03A2B408';$hostHash='9513A0BD7C620589F4AB332CE19F155686C01104B1E8D99A5DECAAF1814F2586'
    if($GuardedSdk){$buildPath='I:\LeaBuildStorage\guarded-sdk-host-build-20261110-v1\result.json';$buildHash='1399CDED2BB6634A2B339F3B1BDC9FEFC68580F840BD2A866AAAE968A3BC83A2';$hostHash='1E390343125011CAA3F78582E91FAFC036E07A31F7E81B4B727ED8503FC39606'}
    Pin $buildPath $buildHash
    $build=Get-Content -LiteralPath $buildPath -Raw|ConvertFrom-Json
    if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.products.Count -ne 22 -or $build.hostSha256 -cne $hostHash -or ($GuardedSdk -and !$build.guardedSdkProductsVerified)){throw 'Current build proof differs'}
    $selected=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach($file in $prior.before){$name=Relative $prior.runtime $file.path;Pin $file.path $file.sha256;if((Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Preserved runtime ACL changed'};$selected.Add($name,@{path=$name;origin=$file.path;sha256=$file.sha256;role='preserved-asset-or-harness'})}
    $buildBin=if($GuardedSdk){Join-Path (Split-Path $buildPath -Parent) 'bin'}else{Split-Path $build.binary -Parent}
    foreach($file in $build.products){$name=Relative $buildBin $file.path;Pin $file.path $file.sha256;$selected[$name]=@{path=$name;origin=$file.path;sha256=$file.sha256;role='current-build-output'}}
    if($selected.Count -ne 400 -or @($selected.Values|Where-Object role -CEQ 'current-build-output').Count -ne 22 -or @($selected.Keys|Where-Object {$_.StartsWith('ScriptWorker/')} ).Count -ne 6){throw 'Current 400-member closure differs'}
    if($selected['Lea.Firespider.Boot.exe'].sha256 -cne '2C44FB4A92088FBF9B548B426B267163E5900A1BD04794BF9843EA869E9532D4'){throw 'Original boot harness identity differs'}
    New-Item -ItemType Directory -Path $out|Out-Null
    $rows=@(foreach($file in $selected.Values|Sort-Object path){$to=Join-Path $out ('runtime/'+$file.path);New-Item -ItemType Directory -Path (Split-Path $to -Parent) -Force|Out-Null;Copy-Item -LiteralPath $file.origin -Destination $to;if((Get-FileHash -LiteralPath $to).Hash -cne $file.sha256){throw 'Runtime readback differs'};@{path=$file.path;origin=$file.origin;sha256=$file.sha256;role=$file.role;bytes=(Get-Item -LiteralPath $to).Length}})
    foreach($file in $inputs){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'Input drift'}}
    @{verified=$true;currentSourceSdk=$true;files=$rows;inputs=$inputs;members=400;buildOutputsBound=22;hostSha256=$build.hostSha256;bytes=($rows|Measure-Object bytes -Sum).Sum;browserExecuted=$false;installedBrowserChanged=$false;nativeAclQualified=$false;fullReleaseQualified=$false}|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $out 'runtime-manifest.json') -Encoding utf8
    Get-FileHash -LiteralPath (Join-Path $out 'runtime-manifest.json')|Format-List Hash
}finally{foreach($lease in $leases){$lease.Dispose()}}
