$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\sdk-print-20261031-v2'
if(Test-Path $out){throw 'Preserve print SDK staging'}
$proofPath=Join-Path $root 'continuity-evidence/relative-font-print-project-v1/result.json'
$proofPin='A71A3BC94E9584876DCE5480027EBAD4585A2900E6F01651C65D4869FA2D8002'
if((Get-FileHash $proofPath).Hash -cne $proofPin){throw 'Print source receipt differs'}
$proof=Get-Content $proofPath -Raw|ConvertFrom-Json
if(!$proof.passed -or !$proof.inputsUnchanged -or $proof.ownerAttention){throw 'Print source baseline incomplete'}
$source=Join-Path $root 'continuity-evidence/relative-font-print-project-v1/source'
$files=@(Get-ChildItem $source -File|Where-Object Extension -in @('.cs','.csproj'))
if($files.Count -ne 23){throw 'Print source coverage differs'}
$stage=Join-Path $out 'Lea.Print';New-Item -ItemType Directory -Path $stage | Out-Null
$rows=[Collections.Generic.List[object]]::new()
foreach($file in $files){
    $pins=@($proof.inputs|Where-Object path -ceq $file.FullName|Select-Object -ExpandProperty sha256 -Unique)
    if($pins.Count -ne 1 -or (Get-FileHash $file.FullName).Hash -cne $pins[0]){throw 'Print source not in tested baseline'}
    $to=Join-Path $stage $file.Name;Copy-Item $file.FullName $to
    if((Get-FileHash $to).Hash -cne $pins[0]){throw 'Print source copy differs'}
    $rows.Add(@{path=('Lea.Print/'+$file.Name);sha256=$pins[0]})
}
$project=Join-Path $stage 'Lea.Print.csproj';$xml=[xml](Get-Content $project -Raw)
$group=$xml.DocumentElement.SelectSingleNode("*[local-name()='PropertyGroup']");$ns=$xml.DocumentElement.NamespaceURI
foreach($pair in @(
    @('Configuration','Release'),@('LangVersion','12.0'),@('UseSharedCompilation','false'),
    @('OutputPath','..\bin\$(Configuration)\'),@('EnableWpfPrint','true'),@('EnableDocumentSelection','true'),
    @('LeaCorePath','..\bin\$(Configuration)\Lea.Engine.dll'),@('LeaViewPath','..\bin\$(Configuration)\Lea.Win.dll')
)){
    $node=$group.SelectSingleNode("*[local-name()='"+$pair[0]+"']")
    if(!$node){$node=$xml.CreateElement($pair[0],$ns);$null=$group.AppendChild($node)}
    $node.InnerText=$pair[1]
    if($pair[0] -in @('Configuration','EnableWpfPrint','EnableDocumentSelection','LeaCorePath','LeaViewPath')){$node.SetAttribute('Condition',("'`$("+$pair[0]+")' == ''"))}
}
$xml.Save($project)
$row=@($rows|Where-Object path -ceq 'Lea.Print/Lea.Print.csproj')[0]
$row.originalSha256=$row.sha256;$row.sha256=(Get-FileHash $project).Hash
$bin=Join-Path $out 'bin/Release';New-Item -ItemType Directory -Path $bin | Out-Null
$references=[Collections.Generic.List[object]]::new()
foreach($pair in @(@('Lea.Engine.dll','DC71E2B9DA704135CA022E5BBC2F1C57BA8046621C41E40AB98C7DC354BD6213'),@('Lea.Win.dll','6F907862F57C7640A8B0E7BE2C22DFEDD18B3966D472958FCFCEDA53A00F0F52'))){
    $from=Join-Path $root ('continuity-evidence/sdk-portable-source-v1/bundle/Sample/bin/Release/'+$pair[0])
    if((Get-FileHash $from).Hash -cne $pair[1]){throw 'SDK reference differs'}
    $to=Join-Path $bin $pair[0];Copy-Item $from $to
    if((Get-FileHash $to).Hash -cne $pair[1]){throw 'SDK reference copy differs'}
    $references.Add(@{path=('bin/Release/'+$pair[0]);sha256=$pair[1]})
}
foreach($row in $rows){if((Get-FileHash (Join-Path $out $row.path)).Hash -cne $row.sha256){throw 'Print staging readback differs'}}
@{staged=$true;files=$rows;references=$references;sourceReceiptSha256=$proofPin;runtimeQualified=$false;completeSdk=$false}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'source-manifest.json') -Encoding utf8
Write-Output 'Print SDK staged: 22 unchanged C# sources, projected relative net48 project, two pinned SDK references; not runtime qualified.'
