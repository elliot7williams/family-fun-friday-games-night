$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$output=Join-Path $workspace 'continuity-evidence/tested-parent-view-source-v1'
if(Test-Path -LiteralPath $output){throw 'Preserve previous export'}
if([IO.DriveInfo]::new('C:\').AvailableFreeSpace -lt 512MB){throw 'Working reserve required'}
$null=New-Item -ItemType Directory -Path $output
$rows=[Collections.Generic.List[object]]::new()
$modules=@(
    @{name='parent';folder='early-limits-v1';hash='E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5'},
    @{name='view';folder='retry-view-v1';hash='E26135F8CE6BA76A76515D2874DFE219E172B212037CD567AD37042DA516289B'}
)
foreach($module in $modules){
    $directory=Join-Path $workspace ('continuity-evidence/permission-contract-v1/'+$module.folder)
    $receiptPath=Join-Path $directory 'result.json'
    $receipt=Get-Content -LiteralPath $receiptPath -Raw|ConvertFrom-Json
    if(!$receipt.passed -or !$receipt.inputsUnchanged -or $receipt.binarySha256 -cne $module.hash){throw 'Build proof mismatch'}
    $destination=Join-Path $output $module.name
    $null=New-Item -ItemType Directory -Path $destination
    $known=@{}
    foreach($input in $receipt.inputs){$known[$input.path]=$input.sha256}
    foreach($source in $receipt.sourceProofs){$known[$source.copy]=$source.sha256}
    $responsePath=Join-Path $directory 'compile.rsp'
    $dependencies=[Collections.Generic.List[object]]::new()
    foreach($line in Get-Content -LiteralPath $responsePath){
        $path=$null;$kind=$null
        if($line -match '^"(.+\.cs)"$'){$path=$Matches[1];$kind='source'}
        elseif($line -match '^/resource:"([^"]+)"'){$path=$Matches[1];$kind='resource'}
        elseif($line -match '^/reference:"([^"]+)"$'){$path=$Matches[1];$kind='reference'}
        if(!$path){continue}
        if(!$known.ContainsKey($path)){throw "Unpinned compiler input: $path"}
        if((Get-FileHash -LiteralPath $path).Hash -cne $known[$path]){throw "Changed compiler input: $path"}
        $relative=$null
        if($kind -ne 'reference'){
            $relative=$module.name+'/'+$kind+'/'+[IO.Path]::GetFileName($path)
            $target=Join-Path $output $relative
            if(Test-Path -LiteralPath $target){throw 'Duplicate archive path'}
            $null=New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($target)) -Force
            Copy-Item -LiteralPath $path -Destination $target
            if((Get-FileHash -LiteralPath $target).Hash -cne $known[$path]){throw 'Export mismatch'}
        }
        $dependencies.Add(@{kind=$kind;originalPath=$path;archivePath=$relative;sha256=$known[$path]})
    }
    Copy-Item -LiteralPath $receiptPath -Destination (Join-Path $destination 'build-receipt.json')
    Copy-Item -LiteralPath $responsePath -Destination (Join-Path $destination 'original-compile.rsp')
    $rows.Add(@{module=$module.name;testedBinarySha256=$module.hash;buildReceiptSha256=(Get-FileHash -LiteralPath $receiptPath).Hash;responseSha256=(Get-FileHash -LiteralPath $responsePath).Hash;dependencies=$dependencies})
}
$notice='Exact tested parent/view source snapshots only. Includes embedded resources and original absolute-path response files. Reference binaries are NOT included; their original paths and hashes are recorded. Engine, image, worker, interpreter and Firespider host source are NOT included. Not a complete rebuild kit, portable project, release or remote backup.'
[IO.File]::WriteAllText((Join-Path $output 'README.txt'),$notice)
[IO.File]::WriteAllText((Join-Path $output 'manifest.json'),(@{scope=$notice;modules=$rows}|ConvertTo-Json -Depth 8))
$files=@(Get-ChildItem -LiteralPath $output -Recurse -File)
$zipPath=$output+'.zip'
if(Test-Path -LiteralPath $zipPath){throw 'Preserve archive'}
[IO.Compression.ZipFile]::CreateFromDirectory($output,$zipPath,[IO.Compression.CompressionLevel]::Optimal,$false)
$zip=[IO.Compression.ZipFile]::OpenRead($zipPath)
try{
    if($zip.Entries.Count -ne $files.Count){throw 'Archive entry count differs'}
    foreach($file in $files){
        $relative=[IO.Path]::GetRelativePath($output,$file.FullName).Replace('\','/')
        $entry=$zip.GetEntry($relative)
        if(!$entry -or $entry.Length -ne $file.Length){throw 'Missing or truncated archive entry'}
        $stream=$entry.Open()
        try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($hash -cne (Get-FileHash -LiteralPath $file.FullName).Hash){throw 'Archive content mismatch'}
    }
}finally{$zip.Dispose()}
[pscustomobject]@{verified=$true;files=$files.Count;archive=$zipPath;sha256=(Get-FileHash -LiteralPath $zipPath).Hash;scope=$notice}|ConvertTo-Json
