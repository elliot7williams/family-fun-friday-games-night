$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$native=Join-Path $root 'continuity-evidence/request-diagnostics-v1/verified-build-20261020-150037/native'
$proofPath=Join-Path $native 'result.json'
$proofHash='09690529339278525ACE678608BBD702A2AA3A70846B7EA3D5698EDE4843CD5D'
if((Get-FileHash $proofPath).Hash -cne $proofHash){throw 'Original decoder source receipt differs'}
$proof=Get-Content $proofPath -Raw|ConvertFrom-Json
if(!$proof.passed -or !$proof.completed -or !$proof.originalInputsUnchanged){throw 'Original decoder source receipt incomplete'}
$out='I:\LeaBuildStorage\sdk-image-worker-20261031-v1'
if(Test-Path $out){throw 'Preserve image SDK staging'}
$pins=@{ 'Mg.cs'='CDF6E334CF99742BC9F3427ECE57B14AB39A5433141040ED42120F43B7FA83F1';'Program.cs'='4B0EBB73E7ED355373DD75D45A83B6D160900F0BCB93F79536F4E7C5BC83DA44' }
$stage=Join-Path $out 'Lea.Image';New-Item -ItemType Directory -Path $stage | Out-Null
$rows=[Collections.Generic.List[object]]::new()
foreach($name in $pins.Keys){
    $relative='Lea.Image\'+$name
    $record=@($proof.sourceInputs|Where-Object path -ceq $relative)
    if($record.Count -ne 1 -or $record[0].sha256 -cne $pins[$name]){throw 'Decoder source pin absent from receipt'}
    $from=Join-Path $native ('source/'+$relative)
    if((Get-FileHash $from).Hash -cne $pins[$name]){throw 'Decoder source differs'}
    $to=Join-Path $stage $name;Copy-Item $from $to
    if((Get-FileHash $to).Hash -cne $pins[$name]){throw 'Decoder source copy differs'}
    $rows.Add(@{path=('Lea.Image/'+$name);sha256=$pins[$name];origin=$from})
}
$project=Join-Path $stage 'Lea.Image.csproj';Copy-Item (Join-Path $root 'Lea.Sdk/Lea.Image.csproj') $project
$xml=[xml](Get-Content $project -Raw)
if($xml.Project.PropertyGroup.TargetFrameworkVersion -cne 'v4.8' -or $xml.Project.PropertyGroup.UseSharedCompilation -cne 'false'){throw 'Image SDK project settings differ'}
$rows.Add(@{path='Lea.Image/Lea.Image.csproj';sha256=(Get-FileHash $project).Hash})
@{staged=$true;compiled=$false;runtimeQualified=$false;originalSourceProofSha256=$proofHash;originalWorkerSha256='FE744BE1D11E99B36A35F4E6CF12C45229AB7FF89134434E61438DE8DED3E535';frameworkChangedTo='v4.8';files=$rows}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'source-manifest.json') -Encoding utf8
Write-Output 'Image SDK staged: 2 exact sources, 1 net48 project; not compiled or security qualified'
