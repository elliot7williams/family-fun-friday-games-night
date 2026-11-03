param([ValidateRange(1,9999)][int]$Version=1)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$destination='I:\LeaBuildStorage\source-20261031-v'+$Version
if(Test-Path -LiteralPath $destination){throw 'Preserve existing checkpoint'}
$extensions=@('.cs','.csproj','.vb','.vbproj','.sln','.props','.targets','.json','.js','.html','.css','.md','.ps1','.cmd','.cpp','.h','.config','.resx','.txt')
$files=@(Get-ChildItem -LiteralPath $root -File -Force | Where-Object {$_.Extension -in $extensions})
foreach($directory in Get-ChildItem -LiteralPath $root -Directory | Where-Object {$_.Name -like 'Lea*'}){
    $files+=@(Get-ChildItem -LiteralPath $directory.FullName -Recurse -File -Force | Where-Object {
        $_.Extension -in $extensions -and $_.FullName -notmatch '\\(bin|obj|node_modules)\\'
    })
}
if(($files|Measure-Object Length -Sum).Sum -gt 64MB){throw 'Source scope unexpectedly large'}
foreach($file in $files){
    if($file.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse file refused'}
    $parent=$file.Directory
    while($parent){if($parent.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse ancestor refused'};$parent=$parent.Parent}
}
New-Item -ItemType Directory -Path $destination | Out-Null
Add-Type -AssemblyName System.IO.Compression
$zipPath=Join-Path $destination 'current-source.zip'
$archive=[IO.Compression.ZipFile]::Open($zipPath,'Create')
$rows=[Collections.Generic.List[object]]::new()
try{
    foreach($file in $files){
        $relative=[IO.Path]::GetRelativePath($root,$file.FullName).Replace('\','/')
        $input=[IO.File]::Open($file.FullName,'Open','Read','Read')
        try{
            $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($input));$input.Position=0
            $entry=$archive.CreateEntry($relative,[IO.Compression.CompressionLevel]::Optimal)
            $output=$entry.Open();try{$input.CopyTo($output)}finally{$output.Dispose()}
            $rows.Add([pscustomobject]@{path=$relative;bytes=$input.Length;sha256=$hash})
        }finally{$input.Dispose()}
    }
}finally{$archive.Dispose()}
$archive=[IO.Compression.ZipFile]::OpenRead($zipPath)
try{
    if($archive.Entries.Count -ne $rows.Count){throw 'Entry count mismatch'}
    foreach($row in $rows){
        $entry=$archive.GetEntry($row.path);if(!$entry -or $entry.Length -ne $row.bytes){throw 'Entry length mismatch'}
        $stream=$entry.Open();try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($hash -cne $row.sha256 -or (Get-FileHash -LiteralPath (Join-Path $root $row.path)).Hash -cne $row.sha256){throw 'Source or archive mismatch'}
    }
}finally{$archive.Dispose()}
$result=[ordered]@{verified=$true;files=$rows.Count;sourceBytes=($rows|Measure-Object bytes -Sum).Sum;archive=$zipPath;sha256=(Get-FileHash $zipPath).Hash;scope='Root text sources and Lea* source trees, excluding bin/obj; historical frozen source remains in checkpoint v64';githubUploaded=$false;entries=$rows}
$result|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $destination 'result.json') -Encoding utf8
$result|Select-Object verified,files,sourceBytes,archive,sha256,githubUploaded|Format-List
