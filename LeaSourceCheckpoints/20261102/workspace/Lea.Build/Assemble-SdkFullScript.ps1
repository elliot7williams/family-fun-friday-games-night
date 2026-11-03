$ErrorActionPreference='Stop'
$preparation='I:\LeaBuildStorage\sdk-full-script-preparation-20261101-v1'
$bindings='I:\LeaBuildStorage\sdk-full-script-bindings-20261101-v2'
$entry='I:\LeaBuildStorage\sdk-full-script-entry-20261101-v1'
$out='I:\LeaBuildStorage\sdk-full-script-assembly-20261101-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve existing assembly evidence'}
$selected=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::OrdinalIgnoreCase)
$inputs=[Collections.Generic.List[object]]::new()
function Verify([string]$path,[string]$pin){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    if((Get-FileHash -LiteralPath $path).Hash -cne $pin){throw ('Assembly input differs: '+$path)}
    $inputs.Add(@{path=$path;sha256=$pin})
}
function SelectFile([string]$path,[string]$relative,[string]$pin,[bool]$replace=$false){
    if([IO.Path]::IsPathRooted($relative) -or $relative.Contains(':') -or $relative.Split('/') -contains '..'){throw 'Assembly path escape'}
    Verify $path $pin
    if($selected.ContainsKey($relative) -and !$replace){throw 'Unexpected assembly collision'}
    if(!$selected.ContainsKey($relative) -and $replace){throw ('Replacement has no baseline: '+$relative)}
    $selected[$relative]=@{path=$relative;origin=$path;sha256=$pin}
}
function Proof([string]$directory,[string]$name,[string]$pin){
    $path=Join-Path $directory $name;Verify $path $pin
    return Get-Content -LiteralPath $path -Raw|ConvertFrom-Json
}
$p=Proof $preparation 'preparation.json' 'C54650F589334D3D58512FF46047EC053857DE1A35D03CA64A7AC86C35664FD2'
$b=Proof $bindings 'bindings.json' 'C7C8A080179284DE563E4F04A1FD15EE497870420010150D3955E20D688DBDFB'
$e=Proof $entry 'result.json' '62D1148A4AB1F6C3F4CED16BABFC05C9A9B4BCD6323E40027BE7B09093BBDCD6'
if(!$p.verified -or !$b.verified -or !$e.passed -or !$e.inputsUnchanged -or $e.ownerAttention -or !$e.entryBinding.inverseVerified){throw 'Preparation evidence incomplete'}
foreach($file in $p.files){
    $relative=$file.path
    if($relative.StartsWith('baseline-runtime/')){$relative='runtime/'+$relative.Substring('baseline-runtime/'.Length)}
    elseif(!$relative.StartsWith('compiled-source/') -and !$relative.StartsWith('fixtures/')){continue}
    if($relative -in @('runtime/Lea.Script.Tests.exe','runtime/Lea.Script.Tests.pdb')){continue}
    SelectFile (Join-Path $preparation $file.path) $relative $file.sha256
}
foreach($file in $p.bindings){SelectFile (Join-Path $preparation ('sdk-components/'+$file.relative)) ('runtime/'+$file.relative) $file.sha256 $true}
foreach($file in $b.files){
    if($file.path.StartsWith('runtime/')){SelectFile (Join-Path $bindings $file.path) $file.path $file.sha256 $true}
}
foreach($name in @('CoreProbeBridge.cs','WorkerEntryBridge.cs','ModernComponents.cs')){
    $targets=@($selected.Keys|Where-Object {$_.StartsWith('compiled-source/') -and [IO.Path]::GetFileName($_) -ceq $name})
    if($targets.Count -ne 1){throw ('Unique compiled source required: '+$name)}
    if($name -ceq 'ModernComponents.cs'){$file=@($e.outputs|Where-Object {[IO.Path]::GetFileName($_.path) -ceq $name})}
    else{$file=@($b.files|Where-Object path -ceq ('source/'+$name));$file=@(@{path=(Join-Path $bindings $file[0].path);sha256=$file[0].sha256})}
    if($file.Count -ne 1){throw 'Projected source absent'}
    SelectFile $file[0].path $targets[0] $file[0].sha256 $true
}
foreach($pair in @(@('WorkerEntryNativeHost.exe','runtime/WorkerEntryProbe/WorkerEntryNativeHost.exe'),@('entry-manifest.json','runtime/WorkerEntryProbe/manifest.json'))){
    $file=@($e.outputs|Where-Object {[IO.Path]::GetFileName($_.path) -ceq $pair[0]})
    if($file.Count -ne 1){throw 'Entry output absent'}
    SelectFile $file[0].path $pair[1] $file[0].sha256 $true
}
if(@($selected.Keys|Where-Object {$_.StartsWith('compiled-source/')} ).Count -ne 60 -or @($selected.Keys|Where-Object {$_.StartsWith('fixtures/')} ).Count -ne 182){throw 'Campaign inventory differs'}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($row in $selected.Values){
    $to=Join-Path $out $row.path
    New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null
    Copy-Item -LiteralPath $row.origin -Destination $to
    if((Get-FileHash -LiteralPath $to).Hash -cne $row.sha256){throw 'Assembly readback differs'}
}
foreach($input in $inputs){if((Get-FileHash -LiteralPath $input.path).Hash -cne $input.sha256){throw 'Assembly inputs changed'}}
@{verified=$true;files=@($selected.Values|Sort-Object path);inputs=$inputs;compiledSources=60;fixtures=182;compiled=$false;executed=$false;notRunnable=$true;baselineTestBinariesExcluded=$true;requiredChecks=3412;requiredPhases=89;compileBudgetMs=120000;testBudgetMs=300000;cleanupBudgetMs=5000;pending=@('compiler recipe and dependency closure','compile main suite','fresh NTFS fixture relocation verification','owned full campaign');runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'assembly.json') -Encoding utf8
Get-FileHash (Join-Path $out 'assembly.json')
