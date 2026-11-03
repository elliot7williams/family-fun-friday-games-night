$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/source-bundle-v1'
$out=Join-Path $root 'extraction-verification.json'
$destination=Join-Path $root 'verified-extraction'
if((Test-Path -LiteralPath $out) -or (Test-Path -LiteralPath $destination)){throw 'Preserve bundle verification'}
if((Get-PSDrive C).Free -lt 768MB){throw 'Verification reserve required'}
function SafeNames($Entries){
    $names=@{}
    foreach($entry in $Entries){
        $name=$entry.FullName.TrimEnd('/')
        if(!$name -or $name.Contains('\') -or $name.StartsWith('/') -or $name.Contains(':') -or @($name.Split('/')|Where-Object {$_ -in @('','..','.')}).Count -or $names.ContainsKey($name)){throw 'Unsafe/duplicate bundle member'}
        $names[$name]=$true
    }
}
$result=@{passed=$false;error=$null;compilerExecuted=$false;runtimeExecuted=$false;portableBuildVerified=$false;releaseCertified=$false}
$source=[IO.File]::Open((Join-Path $root 'Lea-source-and-build-evidence.zip'),'Open','Read','Read')
try{
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($source)) -cne '7A3C730BF1B1C0E9CD20E69528AE85B45E43C009A3D0FC4A3C199EAED8418B48'){throw 'Bundle identity differs'}
    $source.Position=0;$zip=[IO.Compression.ZipArchive]::new($source,[IO.Compression.ZipArchiveMode]::Read,$true)
    try{
        SafeNames $zip.Entries
        $manifestEntry=$zip.GetEntry('manifest.json')
        $input=$manifestEntry.Open();$memory=[IO.MemoryStream]::new()
        try{$input.CopyTo($memory)}finally{$input.Dispose()}
        try{
            $bytes=$memory.ToArray()
            if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)) -cne '602D338813778D93534D854E8E66D8CE1CA583EED47A71AA9725C41D09CC59FB'){throw 'Manifest identity differs'}
            $manifest=[Text.Encoding]::UTF8.GetString($bytes)|ConvertFrom-Json
        }finally{$memory.Dispose()}
        if($manifest.format -ne 1 -or $manifest.files.Count -ne 68 -or $zip.Entries.Count -ne 70){throw 'Bundle inventory differs'}
        $total=($zip.Entries|Measure-Object Length -Sum).Sum
        if($total -gt 32MB){throw 'Unexpected extraction size'}
        $null=New-Item -ItemType Directory -Path $destination
        foreach($entry in $zip.Entries){
            $path=[IO.Path]::GetFullPath((Join-Path $destination $entry.FullName))
            if(!$path.StartsWith($destination+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Escaping extraction path'}
            $null=New-Item -ItemType Directory -Path (Split-Path $path) -Force
            $input=$entry.Open();$target=[IO.File]::Open($path,'CreateNew','Write','None')
            try{$input.CopyTo($target)}finally{$input.Dispose();$target.Dispose()}
        }
        $known=@{}
        foreach($file in $manifest.files){
            if($known.ContainsKey($file.path)){throw 'Duplicate manifest path'}
            $known[$file.path]=$true
            $path=Join-Path $destination $file.path
            if((Get-Item -LiteralPath $path).Length -ne $file.length -or (Get-FileHash -LiteralPath $path).Hash -cne $file.sha256){throw 'Extracted source changed'}
        }
        if(@(Get-ChildItem -LiteralPath $destination -Recurse -File).Count -ne 70){throw 'Extraction inventory differs'}
        $reader=[IO.StreamReader]::new($zip.GetEntry('README.txt').Open())
        try{if($reader.ReadToEnd() -cne $manifest.scope){throw 'Scope notice differs'}}finally{$reader.Dispose()}
        $nested=[Collections.Generic.List[object]]::new()
        foreach($archive in Get-ChildItem -LiteralPath (Join-Path $destination 'source-archives') -File){
            $inner=[IO.Compression.ZipFile]::OpenRead($archive.FullName)
            try{
                SafeNames $inner.Entries
                if($inner.Entries.Count -gt 2000 -or ($inner.Entries|Measure-Object Length -Sum).Sum -gt 128MB){throw 'Unexpected nested source size'}
                $code=@($inner.Entries|Where-Object FullName -Match '\.(cs|vb|js)$').Count
                if($code -lt 1){throw 'Source archive contains no recognized code'}
                $nested.Add(@{archive=$archive.Name;entries=$inner.Entries.Count;codeFiles=$code;pathsChecked=$true})
            }finally{$inner.Dispose()}
        }
        if($nested.Count -ne 6){throw 'Expected six source archives'}
        $result.verifiedFiles=$manifest.files.Count;$result.extractedFiles=70;$result.sourceArchives=$nested
        $result.passed=$true
    }finally{$zip.Dispose()}
}catch{$result.error=$_.Exception.ToString()}
finally{
    $source.Dispose()
    [IO.File]::WriteAllText($out,($result|ConvertTo-Json -Depth 6))
}
[pscustomobject]$result|ConvertTo-Json -Depth 6
