$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\sdk-script-worker-20261101-v1'
if(Test-Path $out){throw 'Preserve SDK worker staging'}
$rows=[Collections.Generic.List[object]]::new()
function Stage([string]$path,[string]$relative,[string]$pin){
    if([IO.Path]::IsPathRooted($relative) -or $relative.Contains(':') -or $relative.Split('/') -contains '..'){throw 'Worker staging path escape'}
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Worker reparse input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    if((Get-FileHash $path).Hash -cne $pin){throw 'Worker source pin differs'}
    $to=Join-Path $out $relative;New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null;Copy-Item $path $to
    if((Get-FileHash $to).Hash -cne $pin){throw 'Worker source copy differs'}
    $rows.Add(@{path=$relative;sha256=$pin;origin=$path})
}
$sourceRoot=Join-Path $root 'continuity-evidence/tested-worker-source-v1'
$manifest=Join-Path $sourceRoot 'manifest.json'
if((Get-FileHash $manifest).Hash -cne '8CFB71ED27CB70EF901D6C904A52D2A4135479B9C9CBF8FD31B8931FE9BD2420'){throw 'Tested worker source inventory differs'}
$sources=Get-Content $manifest -Raw|ConvertFrom-Json
if($sources.nativeSha256 -cne '60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'){throw 'Wrong original worker identity'}
$selected=@($sources.files|Where-Object {$_.archivePath -match '^source/[^/]+\.cs$|^resource/Dom\.compact\.js$'})
if($selected.Count -ne 18){throw 'Worker source coverage differs'}
foreach($file in $selected){Stage (Join-Path $sourceRoot $file.archivePath) ('Lea.Script.Worker/'+[IO.Path]::GetFileName($file.archivePath)) $file.sha256}
$proofPath=Join-Path $root 'continuity-evidence/temporal-startup-v1/relative-font-script-suite-v1/result.json'
$proofPin='39B11B7FFB696DED434C9E0313DEED6744E0A3A08590BB8B14977C064EA6608C'
if((Get-FileHash $proofPath).Hash -cne $proofPin){throw 'Matched worker runtime proof differs'}
$proof=Get-Content $proofPath -Raw|ConvertFrom-Json
if(!$proof.passed -or !$proof.inputsUnchanged -or $proof.ownerAttention){throw 'Matched worker runtime build incomplete'}
foreach($name in @('Jint.dll','Acornima.dll')){
    $path=Join-Path (Join-Path $proof.runtime 'ScriptWorker') $name
    $pins=@($proof.files|Where-Object path -ceq $path|Select-Object -ExpandProperty sha256 -Unique)
    if($pins.Count -ne 1){throw 'Worker dependency identity missing'}
    Stage $path ('lib/'+$name) $pins[0]
}
foreach($pair in @(@('Lea.Script.Worker.csproj','Lea.Script.Worker/Lea.Script.Worker.csproj'),@('Worker.NuGet.Config','Lea.Script.Worker/NuGet.Config'))){$from=Join-Path $root ('Lea.Sdk/'+$pair[0]);Stage $from $pair[1] (Get-FileHash $from).Hash}
if($rows.Count -ne 22){throw 'SDK worker inventory differs'}
foreach($row in $rows){if((Get-FileHash (Join-Path $out $row.path)).Hash -cne $row.sha256){throw 'Worker staging readback differs'}}
@{staged=$true;files=$rows;runtimeVersion='8.0.33';rollForward='Disable';originalWorkerSha256=$sources.nativeSha256;runtimeSourceProofSha256=$proofPin;sourceFiles=17;resources=1;compiled=$false;readyToRun=$false;runtimeQualified=$false;noticesComplete=$false;completeSdk=$false}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'source-manifest.json') -Encoding utf8
Write-Output 'SDK worker staged: 17 unchanged C# files, exact bootstrap, relative net8 project, offline NuGet config and two pinned worker dependencies. IL/native/runtime tests pending.'
