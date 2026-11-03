param([ValidateSet('Scheduling','Feature','Native','Bootstrap','LazyMap','EventMap','EventProbe','ResourceProfile','WorkingSet','Minifier','MinifierRuntime','PrivateWorker')][string]$Batch='Scheduling')
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
if($Batch -eq 'EventMap'){
    $out='I:/LeaBuildStorage/event-map-source-zips-20261102-v1'
    $names=@('print-safety-source-20261028.zip','print-render-source-20261028.zip','print-paginator-source-20261027.zip','print-frame-source-20261027.zip','owned-full-suite-source-20261026.zip','tested-parser-source-v1.zip','page-color-qualification-source-20261026.zip','decoder-queue-trace-source-v1.zip','full-tree-startup-source-20261026.zip','print-submission-source-20261028.zip','engine-recovery-2026-10-14/prototype-inputs-20261015-180833-722/Firespider-Lea-146-script-host-source_2026-10-14_15-36-41.zip','engine-recovery-2026-10-14/prototype-inputs-20261015-180833-722/Lea-current-source-2026-10-14.zip')
}
if($Batch -eq 'EventProbe'){
    $out='I:/LeaBuildStorage/event-probe-source-zips-20261102-v1'
    $names=@('print-text-source-20261027.zip','print-font-source-20261027.zip','print-unified-source-20261027.zip','print-capture-source-20261027.zip','print-package-probe-source-20261028.zip','print-snapshot-source-20261027.zip','print-geometry-source-20261027.zip','print-transfer-source-20261028.zip')
}
if($Batch -eq 'ResourceProfile'){
    $out='I:/LeaBuildStorage/resource-profile-source-zips-20261102-v1'
    $names=@('embedding-modern-worker-source-20261026.zip','library-qualification-source-20261026.zip','source-bundle-v1/verified-extraction/source-archives/Firespider-host-source.zip','source-bundle-v1/verified-extraction/source-archives/tested-parser-source-v1.zip')
}
if($Batch -eq 'WorkingSet'){
    $out='I:/LeaBuildStorage/working-set-source-zips-20261102-v1'
    $names=@('source-bundle-v1/verified-extraction/source-archives/tested-interpreter-inputs-v1.zip','source-bundle-v1/verified-extraction/source-archives/tested-parent-view-source-v1.zip','source-bundle-v1/verified-extraction/source-archives/tested-native-source-v1.zip','source-bundle-v1/verified-extraction/source-archives/tested-worker-source-v1.zip')
}
if($Batch -eq 'Minifier'){
    $out='I:/LeaBuildStorage/minifier-checkpoint-zips-20261102-v1'
    $names=@('private-preservation/Lea-hello-candidate-checkpoint-20261015-180152-529.zip','private-preservation/Lea-generator-checkpoint-20261017-214617-343-b89bfbf0.zip')
}
if($Batch -eq 'MinifierRuntime'){
    $out='I:/LeaBuildStorage/minifier-runtime-archive-20261102-v1'
    $names=@('portable-runtime-v1/Lea-Firespider-development-runtime.zip')
}
if($Batch -eq 'PrivateWorker'){
    $out='I:/LeaBuildStorage/private-worker-archives-20261102-v1'
    $names=@('private-preservation/Lea-clock-trace-checkpoint-20261015-073845-287.zip','private-preservation/Lea-startup-observer-checkpoint-20261015-000739-154.zip','private-preservation/Lea-tail-test-checkpoint-20261014-235819-385.zip','private-preservation/Lea-startup-capacity-checkpoint-20261016-203420-485.zip','ordered-flow-visuals-v2-backup.zip','engine-recovery-2026-10-14/prototype-inputs-20261015-180833-722/Firespider-Lea-146-script-runtime_2026-10-14_15-36-41.zip')
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
