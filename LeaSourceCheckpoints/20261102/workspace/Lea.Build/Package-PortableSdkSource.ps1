param([switch]$Deterministic)
$ErrorActionPreference='Stop'
$stage='I:\LeaBuildStorage\full-sdk-source-20261101-v2'
$out='I:\LeaBuildStorage\portable-sdk-source-package-20261101-v1'
if($Deterministic){$stage='I:\LeaBuildStorage\full-sdk-source-20261101-v3';$out='I:\LeaBuildStorage\portable-sdk-source-package-20261101-v2'}
if(Test-Path -LiteralPath $out){throw 'Preserve source package'}
$manifest=Join-Path $stage 'source-manifest.json'
$manifestHash=if($Deterministic){'B5A0283803C7062DA98A7F3331D94940360E00045A06038168FCA94214CD114A'}else{'E796F6ED2A1AA4C5A18962A1A03D75359EA951A4A35DC117DF1411EEDD8E3A2C'}
$sourceCount=if($Deterministic){132}else{131}
if((Get-FileHash $manifest).Hash -cne $manifestHash){throw 'Exported SDK source differs'}
$source=Get-Content $manifest -Raw|ConvertFrom-Json
if(!$source.verified -or $source.productBinariesIncluded -or $source.files.Count -ne $sourceCount -or !$source.portableNativeBuildIncluded){throw 'Exact source-only inventory required'}
$files=@($source.files)+@(@{path='source-manifest.json';sha256=(Get-FileHash $manifest).Hash})
$holds=[Collections.Generic.List[IDisposable]]::new()
try{
    foreach($file in $files){
        if([IO.Path]::IsPathRooted($file.path) -or $file.path.Contains(':') -or $file.path.Split([char[]]@('/','\')) -contains '..'){throw 'Source package path escape'}
        $path=Join-Path $stage $file.path;$item=Get-Item -LiteralPath $path
        while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse source package input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
        $holds.Add([IO.File]::Open($path,'Open','Read','Read'))
        if((Get-FileHash -LiteralPath $path).Hash -cne $file.sha256){throw 'Source package input changed'}
    }
    New-Item -ItemType Directory -Path $out|Out-Null
    Add-Type -AssemblyName System.IO.Compression
    $name=if($Deterministic){'Lea-full-sdk-source-development-v3.zip'}else{'Lea-full-sdk-source-development-v2.zip'}
    $zip=Join-Path $out $name;$archive=[IO.Compression.ZipFile]::Open($zip,'Create')
    try{foreach($file in $files){$entry=$archive.CreateEntry($file.path,[IO.Compression.CompressionLevel]::Optimal);$input=[IO.File]::OpenRead((Join-Path $stage $file.path));$output=$entry.Open();try{$input.CopyTo($output)}finally{$input.Dispose();$output.Dispose()}}}finally{$archive.Dispose()}
    $archive=[IO.Compression.ZipFile]::OpenRead($zip)
    try{
        if($archive.Entries.Count -ne ($sourceCount+1)){throw 'Source archive inventory differs'}
        foreach($file in $files){$matches=@($archive.Entries|Where-Object FullName -ceq $file.path);if($matches.Count -ne 1){throw 'Source ZIP member absent/duplicated'};$stream=$matches[0].Open();try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()};if($hash -cne $file.sha256 -or (Get-FileHash -LiteralPath (Join-Path $stage $file.path)).Hash -cne $file.sha256){throw 'Source archive readback/input differs'}}
    }finally{$archive.Dispose()}
    @{verified=$true;archive=$zip;sha256=(Get-FileHash $zip).Hash;sourceMembers=$sourceCount;archiveMembers=($sourceCount+1);includesLeaProductBinaries=$false;thirdPartyDependencies=8;githubUploaded=$false;redistributionNoticesComplete=$false;scope='Local development source snapshot; excludes all bin/obj/worker-il products and local toolchain configuration. Not a release or licensing approval.'}|ConvertTo-Json|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    Get-Content (Join-Path $out 'result.json')
}finally{foreach($s in $holds){$s.Dispose()}}
