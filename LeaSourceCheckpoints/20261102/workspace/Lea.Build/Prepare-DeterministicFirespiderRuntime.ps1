$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\deterministic-firespider-runtime-20261102-v1'
if(Test-Path $out){throw 'Preserve coherent runtime preparation'}
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$expected){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse runtime input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
    if((Get-FileHash $path).Hash -cne $expected){throw ('Runtime preparation identity differs: '+$path)};$inputs.Add(@{path=$path;sha256=$expected})
}
function SafeRelative([string]$base,[string]$path){
    $relative=[IO.Path]::GetRelativePath($base,$path).Replace('\','/')
    if([IO.Path]::IsPathRooted($relative) -or $relative.Contains(':') -or $relative.Split('/') -contains '..'){throw 'Runtime member escape'}
    return $relative
}
try{
    $priorPath=Join-Path $root 'continuity-evidence/temporal-startup-v1/host-relative-font-owned-v1/result.json'
    Pin $priorPath 'D9EEB35C3B9AB747640505EF07CBD03BB6D27FBD491F66F253E44F76D3D72A3B'
    $prior=Get-Content $priorPath -Raw|ConvertFrom-Json
    if(!$prior.passed -or !$prior.completed -or !$prior.inputsUnchanged -or $prior.ownerAttention -or $prior.before.Count -ne 399){throw 'Historical harness/runtime provenance incomplete'}
    $buildPath='I:\LeaBuildStorage\deterministic-firespider-build-20261102-v1/result.json'
    Pin $buildPath '6660BFB843153EDF96C629276F2AF119AD989879E5D225E41A752F9046AF398F'
    $build=Get-Content $buildPath -Raw|ConvertFrom-Json
    if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.products.Count -ne 22 -or $build.hostSha256 -cne 'B1B290CA5E90B0F5DD2460BFA3A90DCB81EF8F0ADF5DA96599D437B36B3F416E'){throw 'Exact new host build incomplete'}
    $selected=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach($file in $prior.before){
        $relative=SafeRelative $prior.candidate $file.path;Pin $file.path $file.sha256
        $selected.Add($relative,@{path=$relative;origin=$file.path;sha256=$file.sha256;role='preserved-asset-or-harness'})
    }
    $changes=[Collections.Generic.List[object]]::new()
    $bin=Split-Path $build.binary -Parent
    foreach($file in $build.products){
        $relative=SafeRelative $bin $file.path;Pin $file.path $file.sha256
        $original=if($selected.ContainsKey($relative)){$selected[$relative].sha256}else{$null}
        $selected[$relative]=@{path=$relative;origin=$file.path;sha256=$file.sha256;role='current-build-output'}
        $changes.Add(@{path=$relative;originalSha256=$original;sha256=$file.sha256})
    }
    $worker=@($selected.Keys|Where-Object {$_.StartsWith('ScriptWorker/',[StringComparison]::OrdinalIgnoreCase)})
    if($worker.Count -ne 6 -or @($worker|Where-Object {$selected[$_].role -cne 'current-build-output'}).Count){throw 'Coherent modern worker closure differs'}
    if(!$selected.ContainsKey('Lea.Firespider.Boot.exe') -or $selected['Lea.Firespider.Boot.exe'].sha256 -cne '2C44FB4A92088FBF9B548B426B267163E5900A1BD04794BF9843EA869E9532D4'){throw 'Exact preserved harness required'}
    New-Item -ItemType Directory -Path $out|Out-Null
    $rows=[Collections.Generic.List[object]]::new();$bytes=0L
    foreach($file in $selected.Values){
        $to=Join-Path $out ('runtime/'+$file.path);New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null
        Copy-Item -LiteralPath $file.origin -Destination $to
        if((Get-FileHash $to).Hash -cne $file.sha256){throw 'Coherent runtime readback differs'}
        $row=@{path=$file.path;origin=$file.origin;sha256=$file.sha256;role=$file.role;bytes=(Get-Item $to).Length};$rows.Add($row);$bytes+=$row.bytes
    }
    @{verified=$true;files=@($rows|Sort-Object path);inputs=$inputs;buildOutputsBound=22;changes=$changes;baselineMembers=399;members=$rows.Count;bytes=$bytes;hostSha256=$build.hostSha256;nativeAclQualified=$false;browserExecuted=$false;installedBrowserChanged=$false;scope='Preserved assets/harness plus exact new host+SDK outputs, staged on exFAT only; fresh NTFS startup/workflow validation pending';pending=@('fresh NTFS staging with reserve','settings/navigation/actual EXE smokes','full host workflows and current runtime byte/ACL audit')}|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'runtime-manifest.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'runtime-manifest.json')
}finally{foreach($s in $holds){$s.Dispose()}}
