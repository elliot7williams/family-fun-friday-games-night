$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$base=Join-Path $root 'continuity-evidence/relative-font-host-v1/host-source'
$out='I:\LeaBuildStorage\deterministic-firespider-source-20261102-v1'
if(Test-Path $out){throw 'Preserve Firespider source candidate'}
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$files=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::OrdinalIgnoreCase)
function Pin([string]$path,[string]$expected){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse source refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
    if((Get-FileHash $path).Hash -cne $expected){throw ('Host candidate identity differs: '+$path)};$inputs.Add(@{path=$path;sha256=$expected})
}
function AddFile([string]$relative,[string]$from,[string]$hash,[string]$kind){
    if([IO.Path]::IsPathRooted($relative) -or $relative.Contains(':') -or $relative.Split([char[]]@('/','\')) -contains '..' -or $files.ContainsKey($relative)){throw 'Host member escape/collision'}
    Pin $from $hash;$files.Add($relative,@{path=$relative;origin=$from;sha256=$hash;kind=$kind})
}
try{
    $hostProof=Join-Path $root 'continuity-evidence/relative-font-host-v1/result.json';Pin $hostProof '452492446FCAE2FD9F2DFCC5A6185ED4F05B977B89FADD584A2B6DC83150A6F6'
    $hostRecord=Get-Content $hostProof -Raw|ConvertFrom-Json
    if(!$hostRecord.passed -or !$hostRecord.behaviorPassed -or !$hostRecord.inputsUnchanged -or $hostRecord.ownerAttention -or !$hostRecord.hostSourceInverseVerified -or !$hostRecord.commandSourceInverseVerified -or $hostRecord.byteChanges.Count -or $hostRecord.aclChanges.Count){throw 'Preserved source provenance incomplete'}
    $reproProof='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1/result.json';Pin $reproProof '39955D0AC2D23B60360000414FDB3702F928B4CA1537C074D2C6007BD174B199'
    $repro=Get-Content $reproProof -Raw|ConvertFrom-Json
    if(!$repro.passed -or !$repro.binaryReproducible -or !$repro.inputsUnchanged -or $repro.ownerAttention -or $repro.products.Count -ne 16 -or $repro.differences.Count){throw 'Deterministic SDK proof incomplete'}
    $suiteProof=Join-Path $root 'continuity-evidence/deterministic-full-replay-owned-v1/result.json';Pin $suiteProof 'C9A3C41F5A76A33C7D792B3F7FBC34ED3D7E55BAE64A8844B49C812FE4D3A4CE'
    $suite=Get-Content $suiteProof -Raw|ConvertFrom-Json
    if(!$suite.passed -or $suite.diagnosticOnly -or $suite.checks -ne 3412 -or !$suite.phasesVerified -or !$suite.inputsUnchanged -or $suite.changedFiles.Count -or $suite.ownerAttention){throw 'Exact engine scripting qualification incomplete'}
    $manifest='I:\LeaBuildStorage\full-sdk-source-20261101-v3/source-manifest.json';Pin $manifest 'B5A0283803C7062DA98A7F3331D94940360E00045A06038168FCA94214CD114A'
    $sdk=Get-Content $manifest -Raw|ConvertFrom-Json
    $sourceFiles=@(Get-ChildItem -LiteralPath $base -Recurse -File|Where-Object {$_.FullName -notmatch '\\(bin|obj)\\' -and $_.Extension -notin '.dll','.exe','.pdb' -and !$_.FullName.StartsWith((Join-Path $base 'Lea/ScriptWorker')+'\',[StringComparison]::OrdinalIgnoreCase)})
    if($sourceFiles.Count -ne 87){throw 'Inspected preserved source inventory differs'}
    foreach($file in $sourceFiles){
        $pins=@($hostRecord.inputs|Where-Object path -ceq $file.FullName|Select-Object -ExpandProperty sha256 -Unique)
        if($pins.Count -ne 1){throw 'Unique compiled host source provenance required'}
        AddFile ([IO.Path]::GetRelativePath($base,$file.FullName).Replace('\','/')) $file.FullName $pins[0] 'preserved-host-source'
    }
    $products=@($repro.products|Where-Object {$_.path -match '^bin/Release/[^/]+$' -or $_.path.StartsWith('bin/Release/ScriptWorker/')})
    if($products.Count -ne 12){throw 'Six Framework products plus six worker members required'}
    foreach($p in $products){
        if(!$p.equal -or $p.sha256 -cne $p.secondSha256){throw 'SDK product pair differs'};Pin $p.second $p.sha256
        AddFile ('Lea/'+$p.path.Substring('bin/Release/'.Length)) $p.first $p.sha256 'deterministic-sdk-product'
    }
    foreach($name in @('Jint.dll','Acornima.dll','System.Memory.dll','System.Buffers.dll','System.Numerics.Vectors.dll','System.Runtime.CompilerServices.Unsafe.dll')){
        $pins=@($sdk.files|Where-Object path -ceq ('lib/'+$name));if($pins.Count -ne 1){throw 'Exact dependency pin missing'}
        AddFile ('Lea/'+$name) (Join-Path $repro.roots[0] ('lib/'+$name)) $pins[0].sha256 'sdk-dependency'
    }
    if($files.Count -ne 105 -or @($files.Keys|Where-Object {$_.StartsWith('Lea/ScriptWorker/')} ).Count -ne 6){throw 'Coherent host/source closure differs'}
    New-Item -ItemType Directory -Path $out|Out-Null
    foreach($file in $files.Values){$to=Join-Path $out $file.path;New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null;Copy-Item -LiteralPath $file.origin -Destination $to;if((Get-FileHash $to).Hash -cne $file.sha256){throw 'Host source readback differs'}}
    @{verified=$true;source=$out;files=@($files.Values|Sort-Object path);inputs=$inputs;preservedHostSources=87;sdkProducts=12;thirdPartyDependencies=6;hostSourceUnchanged=$true;compiled=$false;browserExecuted=$false;installedBrowserChanged=$false;scope='Fresh pinned preserved Firespider source plus exact deterministic SDK closure; source compatibility and host runtime not yet qualified';runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'candidate-manifest.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'candidate-manifest.json')
}finally{foreach($s in $holds){$s.Dispose()}}
