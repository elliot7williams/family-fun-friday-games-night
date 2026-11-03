function Expand-VerifiedLeaRuntime {
    param([Parameter(Mandatory)][string]$Archive,[Parameter(Mandatory)][string]$ExpectedArchiveSha256,
        [Parameter(Mandatory)][string]$ExpectedManifestSha256,[Parameter(Mandatory)][string]$Destination)
    $ErrorActionPreference='Stop'
    if(Test-Path -LiteralPath $Destination){throw 'Extraction destination must be new'}
    $stream=[IO.File]::Open($Archive,'Open','Read','Read')
    try{
        if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $ExpectedArchiveSha256){throw 'Archive identity differs'}
        $stream.Position=0
        $zip=[IO.Compression.ZipArchive]::new($stream,[IO.Compression.ZipArchiveMode]::Read,$true)
        try{
            $entries=@{};foreach($entry in $zip.Entries){
                $name=$entry.FullName
                if($entries.ContainsKey($name) -or $name.Contains('\') -or $name.StartsWith('/') -or $name.Contains(':') -or @($name.Split('/')|Where-Object {$_ -in @('','..','.')}).Count){throw 'Unsafe or duplicate archive path'}
                $entries[$name]=$entry
            }
            if(!$entries.ContainsKey('manifest.json') -or $entries['manifest.json'].Length -gt 1MB){throw 'Invalid manifest entry'}
            $input=$entries['manifest.json'].Open();$memory=[IO.MemoryStream]::new()
            try{$input.CopyTo($memory)}finally{$input.Dispose()}
            try{
                $bytes=$memory.ToArray()
                if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)) -cne $ExpectedManifestSha256){throw 'Manifest identity differs'}
                $manifest=[Text.Encoding]::UTF8.GetString($bytes)|ConvertFrom-Json
            }finally{$memory.Dispose()}
            if($manifest.format -ne 1 -or $manifest.files.Count -ne 398 -or $entries.Count -ne 399){throw 'Unexpected package inventory'}
            $names=@{};$total=0L
            foreach($file in $manifest.files){
                if(!$file.path.StartsWith('runtime/',[StringComparison]::Ordinal) -or $names.ContainsKey($file.path) -or !$entries.ContainsKey($file.path) -or $file.sha256 -cnotmatch '^[A-F0-9]{64}$' -or $file.length -lt 0 -or $file.length -ne $entries[$file.path].Length){throw 'Manifest inventory mismatch'}
                $names[$file.path]=$true;$total+=$file.length
            }
            if($total -gt 64MB -or (Get-PSDrive C).Free -lt ($total+512MB)){throw 'Extraction size/reserve exceeded'}
            $root=[IO.Path]::GetFullPath($Destination)
            $walk=[IO.Path]::GetDirectoryName($root)
            while($walk){if((Get-Item -LiteralPath $walk -Force).Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Linked destination refused'};$walk=[IO.Path]::GetDirectoryName($walk)}
            $null=New-Item -ItemType Directory -Path $root
            foreach($file in $manifest.files){
                $path=[IO.Path]::GetFullPath((Join-Path $root $file.path))
                if(!$path.StartsWith($root+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Escaping entry refused'}
                $null=New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($path)) -Force
                $input=$entries[$file.path].Open();$target=[IO.File]::Open($path,'CreateNew','Write','None')
                try{$input.CopyTo($target)}finally{$input.Dispose();$target.Dispose()}
                if((Get-FileHash -LiteralPath $path).Hash -cne $file.sha256){throw 'Extracted file identity differs'}
            }
            return $manifest
        }finally{$zip.Dispose()}
    }finally{$stream.Dispose()}
}
