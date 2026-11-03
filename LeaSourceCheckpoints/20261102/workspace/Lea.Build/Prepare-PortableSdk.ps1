$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\portable-sdk-20261031-v1'
if(Test-Path $out){throw 'Preserve portable SDK staging'}
$rows=[Collections.Generic.List[object]]::new()
New-Item -ItemType Directory -Path $out | Out-Null
function CopyVerified([string]$from,[string]$relative,[string]$pin){
    if([IO.Path]::IsPathRooted($relative) -or $relative.Split('/') -contains '..' -or $relative.Contains(':')){throw 'SDK destination escape'}
    if((Get-FileHash $from).Hash -cne $pin){throw 'Portable SDK source differs'}
    $to=Join-Path $out $relative;New-Item -ItemType Directory -Path (Split-Path $to) -Force | Out-Null
    Copy-Item -LiteralPath $from -Destination $to
    if((Get-FileHash $to).Hash -cne $pin){throw 'Portable SDK copy differs'}
    $rows.Add(@{path=$relative;sha256=$pin;origin=$from})
}
function ReadProof([string]$path,[string]$pin){
    if((Get-FileHash $path).Hash -cne $pin){throw 'SDK prerequisite receipt differs'}
    $r=Get-Content $path -Raw|ConvertFrom-Json
    if(!$r.passed -or !$r.inputsUnchanged -or $r.ownerAttention){throw 'SDK prerequisite incomplete'}
    return $r
}
$sdk='I:\LeaBuildStorage\core-view-sdk-20261031-v3'
$build=ReadProof 'I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-attempt2\result.json' '55595335946DB681D98788BDCB219900FE9CF9DCC75D60CE64FD03BD9ED4325A'
$imageRoot='I:\LeaBuildStorage\sdk-image-worker-20261031-v1'
$image=ReadProof 'I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-image-worker\result.json' '95F4A5D6DC02D49A9A27DFF1245010385FD4BF349ADA253962D895C814E277A0'
foreach($spec in @(@($sdk,$build,45),@($imageRoot,$image,3))){
    $path=Join-Path $spec[0] 'source-manifest.json';$pins=@($spec[1].inputs|Where-Object path -ceq $path|Select-Object -ExpandProperty sha256 -Unique)
    if($pins.Count -ne 1 -or (Get-FileHash $path).Hash -cne $pins[0]){throw 'Portable source manifest differs'}
    $manifest=Get-Content $path -Raw|ConvertFrom-Json
    if($manifest.files.Count -ne $spec[2]){throw 'Portable source coverage differs'}
    foreach($file in $manifest.files){CopyVerified (Join-Path $spec[0] $file.path) $file.path $file.sha256}
}
$embeddingRoot='I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-embedding'
$embedding=ReadProof (Join-Path $embeddingRoot 'result.json') '5BE9728AFC2E3C758227BA6B74B21EDD8462C1FDC03F8578A7EDED27712D5BD9'
foreach($name in @('EmbeddingSample.cs','EmbeddingSample.csproj','embedding.html')){
    $path=Join-Path $embeddingRoot ('sample/'+$name);$pins=@($embedding.inputs|Where-Object path -ceq $path|Select-Object -ExpandProperty sha256 -Unique)
    if($pins.Count -ne 1){throw 'Portable sample source pin missing'};CopyVerified $path ('Sample/'+$name) $pins[0]
}
$project=Join-Path $out 'Sample/EmbeddingSample.csproj';$xml=[xml](Get-Content $project -Raw)
$ns=$xml.DocumentElement.NamespaceURI
$item=$xml.CreateElement('Content',$ns);$item.SetAttribute('Include','$(LeaSdkBin)\Lea.Image.exe')
foreach($pair in @(@('Link','Lea.Image.exe'),@('CopyToOutputDirectory','PreserveNewest'))){$child=$xml.CreateElement($pair[0],$ns);$child.InnerText=$pair[1];$null=$item.AppendChild($child)}
$null=$xml.Project.ItemGroup.AppendChild($item);$xml.Save($project)
$record=@($rows|Where-Object path -ceq 'Sample/EmbeddingSample.csproj')[0];$record.projectedFromSha256=$record.sha256;$record.sha256=(Get-FileHash $project).Hash
foreach($file in @($build.outputs)+@($image.outputs)){
    $name=[IO.Path]::GetFileName($file.path)
    CopyVerified $file.path ('bin/Release/'+$name) $file.sha256
    CopyVerified $file.path ('Sample/bin/Release/'+$name) $file.sha256
}
$sampleExe=Join-Path $embeddingRoot 'sample/bin/Release/Lea.EmbeddingSample.exe'
$pin=@($embedding.inputs|Where-Object path -ceq $sampleExe|Select-Object -ExpandProperty sha256 -Unique)
if($pin.Count -ne 1){throw 'Portable sample EXE pin missing'};CopyVerified $sampleExe 'Sample/bin/Release/Lea.EmbeddingSample.exe' $pin[0]
$html=Join-Path $out 'Sample/embedding.html';CopyVerified $html 'Sample/bin/Release/embedding.html' (Get-FileHash $html).Hash
foreach($pair in @(@('SDK.Build.proj','SDK.Build.proj'),@('PORTABLE.md','README.md'))){$from=Join-Path $root ('Lea.Sdk/'+$pair[0]);CopyVerified $from $pair[1] (Get-FileHash $from).Hash}
foreach($row in $rows){if((Get-FileHash (Join-Path $out $row.path)).Hash -cne $row.sha256){throw 'Portable final readback differs'}}
@{staged=$true;completeSdk=$false;aggregateBuildVerified=$false;viewImageLoadingVerified=$false;files=$rows}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'bundle-manifest.json') -Encoding utf8
Write-Output ('Portable SDK staged/readback verified: '+$rows.Count+' files; aggregate build and view images unverified')
