param([ValidateRange(1,99)][int]$Version=1)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\core-view-sdk-20261031-v'+$Version
if(Test-Path -LiteralPath $out){throw 'Preserve SDK staging attempt'}
if((Get-PSDrive I).Free -lt 256MB){throw 'SDK source staging reserve required'}
$rows=[Collections.Generic.List[object]]::new()
New-Item -ItemType Directory -Path $out | Out-Null
function Stage([string]$folder,[string]$proofRelative,[string]$proofHash,[string]$responseName,[int]$expected){
    $proofPath=Join-Path $root $proofRelative
    if((Get-FileHash $proofPath).Hash -cne $proofHash){throw 'SDK source evidence differs'}
    $proof=Get-Content $proofPath -Raw|ConvertFrom-Json
    if(!$proof.passed -or !$proof.inputsUnchanged -or $proof.ownerAttention){throw 'SDK source evidence incomplete'}
    $response=Join-Path (Split-Path $proofPath -Parent) $responseName
    $responsePins=@($proof.inputs|Where-Object path -ceq $response|Select-Object -ExpandProperty sha256 -Unique)
    if($responsePins.Count -ne 1 -or (Get-FileHash $response).Hash -cne $responsePins[0]){throw 'SDK recipe pin differs'}
    $destination=Join-Path $out $folder
    New-Item -ItemType Directory -Path $destination | Out-Null
    $names=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach($line in Get-Content $response){
        if($line -notmatch '^"(.+\.cs)"$'){continue}
        $source=$Matches[1];$name=[IO.Path]::GetFileName($source)
        if(!$names.Add($name)){throw 'Ambiguous SDK source name'}
        $pins=@($proof.inputs|Where-Object path -ceq $source|Select-Object -ExpandProperty sha256 -Unique)
        if($pins.Count -ne 1 -or (Get-FileHash $source).Hash -cne $pins[0]){throw 'SDK source pin differs'}
        $parent=Get-Item -LiteralPath $source
        if($parent.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'SDK linked source refused'}
        $parent=$parent.Directory
        while($parent){if($parent.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'SDK linked ancestor refused'};$parent=$parent.Parent}
        $target=Join-Path $destination $name;Copy-Item -LiteralPath $source -Destination $target
        if((Get-FileHash $target).Hash -cne $pins[0]){throw 'SDK source copy differs'}
        $rows.Add(@{path=($folder+'/'+$name);sha256=$pins[0];origin=$source;proofSha256=$proofHash})
    }
    if($names.Count -ne $expected){throw 'SDK source count differs'}
    $template=Join-Path $root ('Lea.Sdk/'+$folder+'.csproj')
    $project=Join-Path $destination ($folder+'.csproj');Copy-Item -LiteralPath $template -Destination $project
    $xml=[xml](Get-Content $project -Raw)
    if($xml.Project.PropertyGroup.TargetFrameworkVersion -cne 'v4.8' -or [IO.File]::ReadAllText($project).Contains($root)){throw 'SDK project not portable'}
    $rows.Add(@{path=($folder+'/'+$folder+'.csproj');sha256=(Get-FileHash $project).Hash;origin=$template})
}
Stage 'Lea.Engine' 'continuity-evidence/font-relative-candidate-v1/result.json' 'D828D1B49F3E9DC6C30E2C8186004834F6B8929172A19D645592AC02B06306DB' 'font-core.rsp' 34
Stage 'Lea.Win' 'continuity-evidence/font-relative-render-v1/result.json' '5FCA5A4730ECA3F2894533665FC22CAA9BA520FA49A2B71622B8B45D295E03A0' 'font-view.rsp' 9
Copy-Item -LiteralPath (Join-Path $root 'Lea.Sdk/README.md') -Destination (Join-Path $out 'README.md')
$manifest=@{staged=$true;compiled=$false;completeSdk=$false;runtimeQualified=$false;sourceCoreSha256='4DA8BF1968218A97AF3EC053DDE0E22EEB218669EF1F3B4BCFEA3A39F8ABDFBA';sourceViewSha256='CA51FC818B119529EA8F7104D0753C5779070A7CCF0AD157675A564C4755C584';files=$rows}
$manifest|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $out 'source-manifest.json') -Encoding utf8
foreach($row in $rows){if((Get-FileHash (Join-Path $out $row.path)).Hash -cne $row.sha256){throw 'SDK final readback differs'}}
Write-Output ('SDK sources verified: '+$rows.Count+' files; compilation and embedding not yet verified')
