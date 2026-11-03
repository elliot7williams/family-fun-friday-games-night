param([ValidateSet('Scheduling','Feature','Native','Bootstrap','LazyMap')][string]$Batch='Scheduling')
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot))
$allowed=Join-Path $root 'continuity-evidence'
$out='I:/LeaBuildStorage/isolated-scheduling-source-zips-20261102-v1'
$names=@('source-bundle-v1/Lea-source-and-build-evidence.zip','worker-build-kit-v1/Lea-worker-IL-build-kit.zip','project-compression-source-20261027.zip')
if($Batch -eq 'Feature'){
    $out='I:/LeaBuildStorage/matched-feature-source-zips-20261102-v1'
    $names=@('tested-interpreter-inputs-v1.zip','ordered-flow-regression-v3-source.zip','print-data-source-20261028-v64.zip','modern-worker-v1/interpreter-source-v1/source.zip','modern-worker-v1/parser-source-v1/source.zip','current-native-host-checkpoint-20261026.zip','nested-flow-qualification-v1.zip')
}
if($Batch -eq 'Native'){
    $out='I:/LeaBuildStorage/native-view-source-zips-20261102-v1'
    $names=@('print-data-source-20261028-v61.zip','print-data-source-20261028-v62.zip')
}
if($Batch -eq 'Bootstrap'){
    $out='I:/LeaBuildStorage/bootstrap-function-source-zips-20261102-v1'
    $names=@('native-selection-full-source-20261027.zip','native-selection-source-20261027.zip','print-wpf-layout-source-20261027.zip','print-friendly-source-20261028-v6.zip','print-friendly-source-20261028-v5.zip','print-fault-source-20261028.zip','classlist-paint-source-20261026.zip','worker-startup-source-20261026.zip','decoder-owner-source-v1.zip','network-and-host-source-20261026.zip')
}
if($Batch -eq 'LazyMap'){
    $out='I:/LeaBuildStorage/lazy-map-source-zips-20261102-v1'
    $names=@('print-friendly-source-20261028-v2.zip','print-friendly-source-20261028-v3.zip','print-friendly-source-20261028-v4.zip','print-friendly-wip-source-20261028.zip','print-wpf-source-20261027.zip','print-framework-source-20261027.zip')
}
if(Test-Path $out){throw 'Preserve archive relocation'}
$rows=[Collections.Generic.List[object]]::new()
foreach($name in $names){
    $path=[IO.Path]::GetFullPath((Join-Path $allowed $name))
    if(!$path.StartsWith($allowed+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Explicit archive path escape'}
    $item=Get-Item -LiteralPath $path
    $walk=$item;while($walk){if($walk.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse archive refused'};$walk=if($walk -is [IO.FileInfo]){$walk.Directory}else{$walk.Parent}}
    if(@(Get-Item -LiteralPath $path -Stream *|Where-Object Stream -ne ':$DATA').Count){throw 'Alternate stream preservation required'}
    $rows.Add(@{source=$path;destination=(Join-Path $out $name);bytes=$item.Length;sha256=(Get-FileHash $path).Hash;sddl=(Get-Acl $path).Sddl;lastWriteUtc=$item.LastWriteTimeUtc;verified=$false;removed=$false})
}
if(($rows|Measure-Object bytes -Sum).Sum -gt 20MB){throw 'Bounded source ZIP selection required'}
New-Item -ItemType Directory -Path $out|Out-Null
function Save{$rows|ConvertTo-Json -Depth 4|Set-Content (Join-Path $out 'relocation.json') -Encoding utf8}
foreach($row in $rows){
    New-Item -ItemType Directory -Path (Split-Path $row.destination) -Force|Out-Null
    $input=[IO.File]::Open($row.source,'Open','Read','Read')
    try{
        $output=[IO.File]::Open($row.destination,'CreateNew','Write','None')
        try{$input.CopyTo($output);$output.Flush($true)}finally{$output.Dispose()}
        if((Get-Item $row.destination).Length -ne $row.bytes -or (Get-FileHash $row.destination).Hash -cne $row.sha256 -or (Get-FileHash $row.source).Hash -cne $row.sha256){throw 'ZIP readback differs'}
        $row.verified=$true;Save
    }finally{$input.Dispose()}
    if(!$row.verified -or (Get-FileHash $row.source).Hash -cne $row.sha256){throw 'Verified source ZIP removal precondition differs'}
    Remove-Item -LiteralPath $row.source
    $row.removed=!(Test-Path $row.source);Save
}
@{verified=@($rows|Where-Object {!$_.verified -or !$_.removed}).Count -eq 0;files=$rows.Count;bytes=($rows|Measure-Object bytes -Sum).Sum;workingSourceTouched=$false;currentBuildsTouched=$false;rawFailureEvidenceTouched=$false}|ConvertTo-Json -Compress
