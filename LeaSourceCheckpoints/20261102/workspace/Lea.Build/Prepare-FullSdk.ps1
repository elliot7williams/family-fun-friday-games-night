param([ValidateRange(3,9)][int]$Version=3)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\full-sdk-source-20261101-v'+$Version
if(Test-Path -LiteralPath $out){throw 'Preserve full SDK staging'}
$rows=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::OrdinalIgnoreCase)
$inputs=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new()
function Pin([string]$path,[string]$hash){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse SDK source refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
    $actual=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($hash -and $actual -cne $hash){throw ('SDK source differs: '+$path)}
    $inputs.Add(@{path=$path;sha256=$actual});return $actual
}
function Stage([string]$from,[string]$relative,[string]$hash){
    if([IO.Path]::IsPathRooted($relative) -or $relative.Contains(':') -or $relative.Split('/') -contains '..' -or $rows.ContainsKey($relative)){throw 'SDK member escape or collision'}
    $hash=Pin $from $hash;$to=Join-Path $out $relative
    New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null
    Copy-Item -LiteralPath $from -Destination $to
    if((Get-FileHash $to).Hash -cne $hash){throw 'SDK copy readback differs'}
    $rows.Add($relative,@{path=$relative;sha256=$hash;origin=$from})
}
function Proof([string]$path,[string]$hash){$null=Pin $path $hash;return Get-Content -LiteralPath $path -Raw|ConvertFrom-Json}
New-Item -ItemType Directory -Path $out|Out-Null
try{
    $base=Join-Path $root 'continuity-evidence/sdk-print-package-source-v1'
    $proof=Proof (Join-Path $base 'result.json') '649AD2862712AF78954243DBBE56E80D004D31E09ADD440757A205619D69F362'
    if(!$proof.passed -or !$proof.inputsUnchanged -or $proof.ownerAttention -or !$proof.aggregateSourceBuildPassed -or $proof.freshSourceFiles -ne 76){throw 'Qualified aggregate source campaign required'}
    $bundle=Join-Path $base 'bundle'
    $selected=@($proof.inputs|Where-Object {$_.path.StartsWith($bundle+'\',[StringComparison]::OrdinalIgnoreCase) -and $_.path -notmatch '\\(bin|obj)\\'})
    if($selected.Count -ne 76){throw 'Original source-only inventory differs'}
    foreach($file in $selected){
        $relative=[IO.Path]::GetRelativePath($bundle,$file.path).Replace('\','/')
        if($relative -notin 'README.md','SDK.Build.proj'){Stage $file.path $relative $file.sha256}
    }
    $parentRoot='I:\LeaBuildStorage\sdk-script-20261101-net48-v1'
    $parent=Proof (Join-Path $parentRoot 'source-manifest.json') 'A2CCB6C761E7A12B45FBDF1AD8113B4D2243084AC0C7211A64C0E827A37B4528'
    if($parent.files.Count -ne 28){throw 'Mature parent source inventory differs'}
    foreach($file in $parent.files){if(!$file.path.StartsWith('bin/')){Stage (Join-Path $parentRoot $file.path) $file.path $file.sha256}}
    $workerRoot='I:\LeaBuildStorage\sdk-script-worker-20261101-v1'
    $worker=Proof (Join-Path $workerRoot 'source-manifest.json') '593B1AED8AE8328AA33E3BE5B42A6728FB733210069A030513B66438F5BC56EA'
    if($worker.files.Count -ne 22 -or $worker.sourceFiles -ne 17 -or $worker.runtimeVersion -cne '8.0.33' -or $worker.rollForward -cne 'Disable'){throw 'Worker source inventory differs'}
    foreach($file in $worker.files){
        $relative=$file.path;if($relative.StartsWith('lib/')){$relative='lib/worker/'+[IO.Path]::GetFileName($relative)}
        Stage (Join-Path $workerRoot $file.path) $relative $file.sha256
    }
    $project=Join-Path $out 'Lea.Script.Worker/Lea.Script.Worker.csproj';$xml=[xml](Get-Content $project -Raw)
    $changes=[Collections.Generic.List[object]]::new()
    foreach($name in @('Jint','Acornima')){
        $nodes=@($xml.Project.ItemGroup.Reference|Where-Object Include -ceq $name)
        $expected='..\lib\'+$name+'.dll'
        if($nodes.Count -ne 1 -or $nodes[0].HintPath -cne $expected){throw 'Original worker dependency path differs'}
        $replacement='..\lib\worker\'+$name+'.dll';$nodes[0].HintPath=$replacement
        $changes.Add(@{name=$name;old=$expected;replacement=$replacement})
    }
    if($xml.Project.PropertyGroup.OutputPath -cne '..\bin\$(Configuration)\ScriptWorker\'){throw 'Original worker output differs'}
    $old='..\bin\$(Configuration)\ScriptWorker\';$replacement='..\worker-il\$(Configuration)\'
    $xml.Project.PropertyGroup.OutputPath=$replacement;$changes.Add(@{name='OutputPath';old=$old;replacement=$replacement})
    $inverse=[xml]$xml.OuterXml
    foreach($change in $changes){if($change.name -ceq 'OutputPath'){$inverse.Project.PropertyGroup.OutputPath=$change.old}else{($inverse.Project.ItemGroup.Reference|Where-Object Include -ceq $change.name).HintPath=$change.old}}
    $original=[xml](Get-Content $project -Raw)
    if($inverse.OuterXml -cne $original.OuterXml){throw 'Worker XML projection inverse differs'}
    $row=$rows['Lea.Script.Worker/Lea.Script.Worker.csproj'];$row.projectedFromSha256=$row.sha256
    $xml.Save($project);$row.sha256=(Get-FileHash $project).Hash
    $sampleProof=Proof 'I:\LeaBuildStorage\script-embedding-sample-build-20261101-v4/result.json' 'F6356225C78B5113177DFA50CC71336D2030150AD0427EFC534AF38130EF4187'
    if(!$sampleProof.passed -or !$sampleProof.inputsUnchanged -or !$sampleProof.workerAclRestored -or $sampleProof.ownerAttention){throw 'Qualified script consumer source required'}
    foreach($name in @('ScriptEmbeddingSample.cs','ScriptEmbeddingSample.csproj','script-embedding.html','ScriptHost.config')){
        $from=Join-Path $root ('Lea.Sdk/'+$name)
        $pins=@($sampleProof.inputs|Where-Object path -ceq $from|Select-Object -ExpandProperty sha256 -Unique)
        if($pins.Count -ne 1){throw 'Tested sample source pin missing'};Stage $from ('ScriptSample/'+$name) $pins[0]
    }
    Stage (Join-Path $root 'Lea.Sdk/SDK.Full.Build.proj') 'SDK.Build.proj' ''
    foreach($name in @('Build-NativeWorker.ps1','New-NativeToolchain.ps1','NativeWorker.Tools.cs','Directory.Build.targets')){Stage (Join-Path $root ('Lea.Sdk/'+$name)) $name ''}
    Stage (Join-Path $root 'Lea.Sdk/SDK_FULL.md') 'README.md' ''
    if($rows.Count -ne 132){throw ('Combined source inventory differs: '+$rows.Count)}
    if(@(Get-ChildItem $out -Recurse -File|Where-Object {$_.Extension -ceq '.exe' -or ($_.Extension -ceq '.dll' -and !$_.FullName.StartsWith((Join-Path $out 'lib')+'\',[StringComparison]::OrdinalIgnoreCase))}).Count){throw 'Prebuilt product found in source-only candidate'}
    foreach($row in $rows.Values){if((Get-FileHash (Join-Path $out $row.path)).Hash -cne $row.sha256){throw 'Final SDK readback differs'}}
    foreach($file in $inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'SDK source changed'}}
    @{verified=$true;files=@($rows.Values|Sort-Object path);inputs=$inputs;workerProjectChanges=$changes;workerProjectInverseVerified=$true;productBinariesIncluded=$false;thirdPartyDependencyFiles=8;portableNativeBuildIncluded=$true;deterministicSettingsIncluded=$true;crossDirectoryReproducibilityVerified=$false;compiled=$false;nativeWorkerQualified=$false;completeSdk=$false;noticesComplete=$false;runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'source-manifest.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'source-manifest.json')
}finally{foreach($hold in $holds){$hold.Dispose()}}
