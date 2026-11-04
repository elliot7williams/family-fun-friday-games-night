param([switch]$PrivateBindings,[ValidatePattern('^[A-F0-9]{64}$')][string]$PrivateBindingManifestSha256,[ValidatePattern('^[A-F0-9]{64}$')][string]$PrivateBindingBuildSha256)
$ErrorActionPreference='Stop'
$base='I:\LeaBuildStorage\deterministic-firespider-runtime-20261102-v1'
$out='I:\LeaBuildStorage\deterministic-firespider-runtime-audit-20261102-v1'
if($PrivateBindings){if(!$PrivateBindingManifestSha256 -or !$PrivateBindingBuildSha256){throw 'Pinned private runtime/build required'};$base='I:\LeaBuildStorage\private-binding-firespider-runtime-20261103-v1';$out='I:\LeaBuildStorage\private-binding-firespider-runtime-audit-20261103-v1'}elseif($PrivateBindingManifestSha256 -or $PrivateBindingBuildSha256){throw 'Unexpected private runtime pins'}
if(Test-Path $out){throw 'Preserve existing audit'}
function Member([string]$name){
    if(!$name -or [IO.Path]::IsPathRooted($name) -or $name.Contains(':') -or $name.Contains('\') -or $name.Split('/') -contains '..' -or $name.Split('/') -contains '.' -or $name.Split('/') -contains ''){throw 'Unsafe runtime member'}
    return Join-Path (Join-Path $base 'runtime') $name
}
$manifest=Join-Path $base 'runtime-manifest.json'
$manifestPin=if($PrivateBindings){$PrivateBindingManifestSha256}else{'72D5699F0A392633EF9B967BCDE5E6AB7961FC8405C5F5CB76DE15E342BA2C17'}
if((Get-FileHash $manifest).Hash -cne $manifestPin){throw 'Runtime manifest identity differs'}
$m=Get-Content $manifest -Raw|ConvertFrom-Json
if(!$m.verified -or $m.members -ne 400 -or $m.files.Count -ne 400 -or $m.buildOutputsBound -ne 22 -or $m.browserExecuted -or $m.nativeAclQualified){throw 'Runtime boundary differs'}
$buildPath='I:\LeaBuildStorage\deterministic-firespider-build-20261102-v1\result.json'
$buildPin='6660BFB843153EDF96C629276F2AF119AD989879E5D225E41A752F9046AF398F'
if($PrivateBindings){$buildPath='I:\LeaBuildStorage\private-binding-firespider-build-20261103-v1/result.json';$buildPin=$PrivateBindingBuildSha256}
if((Get-FileHash $buildPath).Hash -cne $buildPin){throw 'Build proof differs'}
$b=Get-Content $buildPath -Raw|ConvertFrom-Json
if($PrivateBindings -and (!$m.privateBindingPair -or !$b.privateBindingPair -or !$b.passed -or !$b.inputsUnchanged -or $b.ownerAttention)){throw 'Private host build/runtime boundary differs'}
$names=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$bytes=0L
foreach($file in $m.files){
    $path=Member $file.path
    if(!$names.Add($file.path)){throw 'Duplicate runtime member'}
    $item=Get-Item -LiteralPath $path
    $ancestor=$item
    while($ancestor){if($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse runtime refused'};$ancestor=if($ancestor -is [IO.FileInfo]){$ancestor.Directory}else{$ancestor.Parent}}
    if($item.Length -ne $file.bytes -or (Get-FileHash $path).Hash -cne $file.sha256){throw ('Runtime bytes differ: '+$file.path)}
    $bytes+=$item.Length
}
if(@(Get-ChildItem (Join-Path $base 'runtime') -Recurse -File).Count -ne $names.Count -or $bytes -ne $m.bytes){throw 'Runtime inventory differs'}
foreach($product in $b.products){
    $relative=[IO.Path]::GetRelativePath((Split-Path $b.binary),$product.path).Replace('\','/')
    $row=@($m.files|Where-Object path -CEQ $relative)
    if($row.Count -ne 1 -or $row[0].role -cne 'current-build-output' -or $row[0].sha256 -cne $product.sha256){throw 'Current build output not bound'}
}
if(@($m.files|Where-Object role -CEQ 'current-build-output').Count -ne 22 -or @($m.files|Where-Object {$_.path.StartsWith('ScriptWorker/',[StringComparison]::OrdinalIgnoreCase)}).Count -ne 6){throw 'Output closure differs'}
$negatives=0
foreach($bad in @('../outside','C:/outside','ScriptWorker/../outside','/outside','ScriptWorker\outside','assets//outside')){
    $rejected=$false;try{$null=Member $bad}catch{$rejected=$true}
    if(!$rejected){throw 'Unsafe-member negative control accepted'};$negatives++
}
New-Item -ItemType Directory -Path $out|Out-Null
@{passed=$true;members=$names.Count;bytes=$bytes;currentOutputs=22;workerMembers=6;unsafePathControls=$negatives;hostSha256=$m.hostSha256;browserExecuted=$false;nativeAclQualified=$false;manifestSha256=(Get-FileHash $manifest).Hash;scope='Independent runtime inventory and exact build binding audit only; fresh NTFS browser tests pending'}|ConvertTo-Json|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
