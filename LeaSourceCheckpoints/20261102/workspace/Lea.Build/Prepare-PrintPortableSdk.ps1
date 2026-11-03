$ErrorActionPreference='Stop'
$out='I:\LeaBuildStorage\portable-print-sdk-20261031-v1'
if(Test-Path $out){throw 'Preserve print aggregate stage'}
$rows=[Collections.Generic.List[object]]::new()
function CopyPinned([string]$from,[string]$relative,[string]$pin){
    if([IO.Path]::IsPathRooted($relative) -or $relative.Contains(':') -or $relative.Split('/') -contains '..'){throw 'Stage path escape'}
    if((Get-FileHash $from).Hash -cne $pin){throw 'Stage input differs'}
    $to=Join-Path $out $relative;New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null
    Copy-Item $from $to
    if((Get-FileHash $to).Hash -cne $pin){throw 'Stage copy differs'}
    $rows.Add(@{path=$relative;sha256=$pin})
}
$prior='I:\LeaBuildStorage\portable-sdk-20261031-v1'
$manifest=Join-Path $prior 'bundle-manifest.json'
if((Get-FileHash $manifest).Hash -cne '18A18FBEEBBE8DC0587E12467B2332A84CCC7A4929F373022DF506D92BD0ADB6'){throw 'Base SDK manifest differs'}
$base=Get-Content $manifest -Raw|ConvertFrom-Json
$sourceFiles=@($base.files|Where-Object {$_.path -notmatch '(^|/)bin/'})
if($sourceFiles.Count -ne 53){throw 'Base source coverage differs'}
foreach($file in $sourceFiles){CopyPinned (Join-Path $prior $file.path) $file.path $file.sha256}
$proofPath='I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-print-library\result.json'
if((Get-FileHash $proofPath).Hash -cne 'BC0C7001376EC1FF19FFCBEF36987F0A99282A6A79E30421EE4937F6F1B46E55'){throw 'Print build receipt differs'}
$proof=Get-Content $proofPath -Raw|ConvertFrom-Json
if(!$proof.passed -or !$proof.inputsUnchanged -or $proof.ownerAttention){throw 'Print build incomplete'}
$print='I:\LeaBuildStorage\sdk-print-20261031-v2'
$manifest=Join-Path $print 'source-manifest.json'
$pins=@($proof.inputs|Where-Object path -ceq $manifest|Select-Object -ExpandProperty sha256 -Unique)
if($pins.Count -ne 1 -or (Get-FileHash $manifest).Hash -cne $pins[0]){throw 'Print source inventory differs'}
$printFiles=(Get-Content $manifest -Raw|ConvertFrom-Json).files
if($printFiles.Count -ne 23){throw 'Print source coverage differs'}
foreach($file in $printFiles){CopyPinned (Join-Path $print $file.path) $file.path $file.sha256}
$path=Join-Path $out 'SDK.Build.proj';$xml=[xml](Get-Content $path -Raw)
$ns=$xml.DocumentElement.NamespaceURI;$target=$xml.DocumentElement.SelectSingleNode("*[local-name()='Target' and @Name='Build']")
$sample=$target.SelectSingleNode("*[local-name()='MSBuild' and @Projects='Sample\EmbeddingSample.csproj']")
if(!$sample){throw 'Aggregate sample target missing'}
$task=$xml.CreateElement('MSBuild',$ns)
foreach($pair in @(@('Projects','Lea.Print\Lea.Print.csproj'),@('Targets','Build'),@('Properties','Configuration=$(Configuration);UseSharedCompilation=false'),@('BuildInParallel','false'))){$task.SetAttribute($pair[0],$pair[1])}
$null=$target.InsertBefore($task,$sample);$xml.Save($path)
$path=Join-Path $out 'Sample/EmbeddingSample.csproj';$xml=[xml](Get-Content $path -Raw)
$ns=$xml.DocumentElement.NamespaceURI;$group=$xml.DocumentElement.SelectSingleNode("*[local-name()='ItemGroup' and *[local-name()='Reference']]")
if(!$group){throw 'Sample reference group missing'}
$reference=$xml.CreateElement('Reference',$ns);$reference.SetAttribute('Include','Lea.Print')
foreach($pair in @(@('HintPath','$(LeaSdkBin)\Lea.Print.dll'),@('Private','true'))){$child=$xml.CreateElement($pair[0],$ns);$child.InnerText=$pair[1];$null=$reference.AppendChild($child)}
$null=$group.AppendChild($reference);$xml.Save($path)
foreach($relative in @('SDK.Build.proj','Sample/EmbeddingSample.csproj')){
    $row=@($rows|Where-Object path -ceq $relative)[0];$row.originalSha256=$row.sha256;$row.sha256=(Get-FileHash (Join-Path $out $relative)).Hash
}
foreach($row in $rows){if((Get-FileHash (Join-Path $out $row.path)).Hash -cne $row.sha256){throw 'Final stage readback differs'}}
@{staged=$true;printIncluded=$true;completeSdk=$false;files=$rows;prebuiltBinaries=0;aggregateBuildVerified=$false}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'bundle-manifest.json') -Encoding utf8
Write-Output 'Print aggregate staged: 76 source/project/doc files, no prebuilt binaries; build and new-product runtime tests pending.'
