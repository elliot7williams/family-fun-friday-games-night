$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/worker-reference-pack-8032-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve reference restore attempt'}
if([IO.DriveInfo]::new('C:\').AvailableFreeSpace -lt 512MB){throw 'Preserve storage reserve'}
$receipt=Join-Path $workspace 'continuity-evidence/worker-archive-build-v1/result.json'
if((Get-FileHash $receipt).Hash -cne '15A9162F4B9E06F0D8455FC05D2250C62DC57127BACF56F9CAB6FC39FCADA75D'){throw 'Worker source provenance differs'}
$build=Get-Content $receipt -Raw|ConvertFrom-Json
if(!$build.passed -or !$build.inputsUnchanged -or !$build.byteIdentical -or $build.ownerAttention){throw 'Worker source proof incomplete'}
$prefix='C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.32\ref\net8.0\'
$refs=@($build.inputs|Where-Object {$_.path.StartsWith($prefix,[StringComparison]::OrdinalIgnoreCase)})
if($refs.Count -ne 163){throw 'Expected163 frozen reference inputs'}
$null=New-Item -ItemType Directory $out
Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
$result=[ordered]@{passed=$false;files=[Collections.Generic.List[object]]::new();error=$null;systemInstallationChanged=$false;runtimeExecuted=$false;scope='Project-local exact8.0.32 compile references, individually matched to retained worker build hashes; no SDK/runtime substitution'}
$receiptLease=[IO.File]::Open($receipt,'Open','Read','Read')
try{
    $package=Join-Path $out 'microsoft.netcore.app.ref.8.0.32.nupkg'
    $url='https://api.nuget.org/v3-flatcontainer/microsoft.netcore.app.ref/8.0.32/microsoft.netcore.app.ref.8.0.32.nupkg'
    Invoke-WebRequest -Uri $url -OutFile $package
    $result.package=@{url=$url;sha256=(Get-FileHash $package).Hash;sha512=(Get-FileHash $package -Algorithm SHA512).Hash;bytes=(Get-Item $package).Length}
    $stream=[IO.File]::Open($package,'Open','Read','Read')
    try{
        $zip=[IO.Compression.ZipArchive]::new($stream,[IO.Compression.ZipArchiveMode]::Read,$true)
        try{
            $target=Join-Path $out 'ref/net8.0';$null=New-Item -ItemType Directory $target
            $seen=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
            foreach($ref in $refs){
                $leaf=$ref.path.Substring($prefix.Length)
                if($leaf -cnotmatch '^[A-Za-z0-9_.-]+\.dll$' -or !$seen.Add($leaf)){throw 'Reference leaf identity differs'}
                $entries=@($zip.Entries|Where-Object FullName -ceq ('ref/net8.0/'+$leaf))
                if($entries.Count -ne 1 -or $entries[0].Length -gt 16MB){throw 'Package reference entry missing/ambiguous/oversized'}
                $input=$entries[0].Open()
                try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($input))}finally{$input.Dispose()}
                if($hash -cne $ref.sha256){throw ('Frozen reference hash differs: '+$leaf)}
                $to=Join-Path $target $leaf
                $input=$entries[0].Open();$output=[IO.File]::Open($to,'CreateNew','Write','None')
                try{$input.CopyTo($output);$output.Flush($true)}finally{$input.Dispose();$output.Dispose()}
                if((Get-FileHash $to).Hash -cne $ref.sha256){throw 'Extracted reference readback differs'}
                $result.files.Add(@{path=$to;sha256=$hash;originalPath=$ref.path})
            }
        }finally{$zip.Dispose()}
    }finally{$stream.Dispose()}
    $result.passed=$result.files.Count -eq 163
}catch{$result.error=$_.Exception.ToString()}
finally{
    $receiptLease.Dispose()
    $result|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'result.json')
    [pscustomobject]$result|Select-Object passed,@{n='references';e={$_.files.Count}},systemInstallationChanged,error|ConvertTo-Json
}
