$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$native=Join-Path $workspace 'continuity-evidence/request-diagnostics-v1/verified-build-20261020-150037/native'
$output=Join-Path $workspace 'continuity-evidence/tested-native-source-v1'
if((Test-Path -LiteralPath $output) -or (Test-Path -LiteralPath ($output+'.zip'))){throw 'Preserve export'}
$receiptPath=Join-Path $native 'result.json'
if((Get-FileHash -LiteralPath $receiptPath).Hash -cne '09690529339278525ACE678608BBD702A2AA3A70846B7EA3D5698EDE4843CD5D'){throw 'Native proof changed'}
$receipt=Get-Content -LiteralPath $receiptPath -Raw|ConvertFrom-Json
if(!$receipt.passed -or !$receipt.completed -or !$receipt.originalInputsUnchanged){throw 'Native build incomplete'}
if((Get-PSDrive C).Free -lt 512MB){throw 'Working reserve required'}
$null=New-Item -ItemType Directory -Path $output
$modules=[Collections.Generic.List[object]]::new()
foreach($spec in @(@('Lea.Engine','Lea.Engine.dll','88690DBF2B1244AF7D3EBF402954A0E41E46CA1917CCE4A57851E29BB710BF9F'),@('Lea.Image','Lea.Image.exe','FE744BE1D11E99B36A35F4E6CF12C45229AB7FF89134434E61438DE8DED3E535'))){
    $binary=Join-Path $native ('runtime/'+$spec[1])
    $row=@($receipt.binaries|Where-Object sha256 -ceq $spec[2])
    if($row.Count -ne 1 -or (Get-FileHash -LiteralPath $binary).Hash -cne $spec[2]){throw 'Binary identity differs'}
    $known=@{};$sources=[Collections.Generic.List[object]]::new()
    foreach($input in $receipt.sourceInputs|Where-Object{$_.path.StartsWith($spec[0]+'\',[StringComparison]::Ordinal)}){
        $from=Join-Path $native ('source/'+$input.path)
        if((Get-FileHash -LiteralPath $from).Hash -cne $input.sha256){throw 'Source identity differs'}
        $target=Join-Path $output $input.path
        $null=New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($target)) -Force
        Copy-Item -LiteralPath $from -Destination $target
        if((Get-FileHash -LiteralPath $target).Hash -cne $input.sha256){throw 'Source copy differs'}
        $known[$from]=$input.sha256
        $sources.Add(@{archivePath=$input.path;originalPath=$from;sha256=$input.sha256})
    }
    $recipe=Join-Path $native ($spec[0]+'.rsp')
    $references=[Collections.Generic.List[object]]::new();$sourceCount=0
    foreach($line in Get-Content -LiteralPath $recipe){
        if($line -match '^"(.+\.cs)"$'){
            if(!$known.ContainsKey($Matches[1])){throw 'Compiler source missing from inventory'}
            $sourceCount++
        }elseif($line -match '^/reference:"([^"]+)"$'){
            $references.Add(@{path=$Matches[1];currentSha256=(Get-FileHash -LiteralPath $Matches[1]).Hash;historicalReferenceHashProven=$false})
        }elseif($line -match '^/(resource|embed|win32icon|win32res):'){throw 'Additional compiler asset requires explicit export'}
    }
    if($sourceCount -ne @($sources|Where-Object archivePath -like '*.cs').Count){throw 'Source coverage differs'}
    Copy-Item -LiteralPath $recipe -Destination (Join-Path $output ($spec[0]+'/original-compile.rsp'))
    $modules.Add(@{module=$spec[0];testedBinarySha256=$spec[2];sourceCount=$sourceCount;sources=$sources;references=$references;responseSha256=(Get-FileHash -LiteralPath $recipe).Hash})
}
Copy-Item -LiteralPath $receiptPath -Destination (Join-Path $output 'original-build-receipt.json')
$scope='Frozen tested core-engine and image-helper sources/project files with original compiler recipes. Reference binaries not included; current reference hashes inventoried, historical hashes not proven. No independent rebuild or complete browser/source package claim. Original native build pass does not erase its separate failed browser gates.'
[IO.File]::WriteAllText((Join-Path $output 'README.txt'),$scope)
[IO.File]::WriteAllText((Join-Path $output 'manifest.json'),(@{scope=$scope;modules=$modules}|ConvertTo-Json -Depth 8))
$files=@(Get-ChildItem -LiteralPath $output -Recurse -File)
$archive=$output+'.zip'
[IO.Compression.ZipFile]::CreateFromDirectory($output,$archive,[IO.Compression.CompressionLevel]::Optimal,$false)
$zip=[IO.Compression.ZipFile]::OpenRead($archive)
try{
    if($zip.Entries.Count -ne $files.Count){throw 'Archive count differs'}
    foreach($file in $files){
        $name=[IO.Path]::GetRelativePath($output,$file.FullName).Replace('\','/')
        $entry=$zip.GetEntry($name)
        if(!$entry -or $entry.Length -ne $file.Length){throw 'Archive entry missing/truncated'}
        $stream=$entry.Open()
        try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($hash -cne (Get-FileHash -LiteralPath $file.FullName).Hash){throw 'Archive hash differs'}
    }
}finally{$zip.Dispose()}
[pscustomobject]@{verified=$true;files=$files.Count;modules=@($modules|ForEach-Object{@{name=$_.module;sources=$_.sourceCount}});archive=$archive;sha256=(Get-FileHash -LiteralPath $archive).Hash}|ConvertTo-Json -Depth 4
