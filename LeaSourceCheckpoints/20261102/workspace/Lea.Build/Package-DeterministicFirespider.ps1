$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot
$out='I:\LeaBuildStorage\deterministic-firespider-package-20261102-v1'
if(Test-Path $out){throw 'Preserve existing package attempt'}
$holds=[Collections.Generic.List[IDisposable]]::new()
function Pin([string]$path,[string]$hash){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse package input'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
    if((Get-FileHash $path).Hash -cne $hash){throw ('Package input differs: '+$path)}
}
try{
    $receipt=Join-Path $workspace 'continuity-evidence/deterministic-host-workflows-v1/result.json'
    $receiptHash='F96D8CDC2195339F2D12E55CD89B2C7F2696B16EC550CA4C86A778FDAEFA9358'
    Pin $receipt $receiptHash
    $r=Get-Content $receipt -Raw|ConvertFrom-Json
    if(!$r.passed -or !$r.completed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.byteChanges.Count -or $r.aclChanges.Count -or $r.before.Count -ne 400 -or $r.runs.Count -ne 15){throw 'Workflow qualification incomplete'}
    foreach($run in $r.runs){if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0){throw 'Workflow run incomplete'}}
    $print=Join-Path $workspace 'continuity-evidence/deterministic-host-print-v2/result.json'
    $printHash='D39D190C0A6D0AD4483A9073D2CDA5A495415AD24BED4178C6FC688F6827A22A'
    Pin $print $printHash
    $p=Get-Content $print -Raw|ConvertFrom-Json
    if(!$p.passed -or !$p.completed -or !$p.inputsUnchanged -or $p.ownerAttention -or $p.byteChanges.Count -or $p.aclChanges.Count){throw 'Print qualification incomplete'}
    $sources=@(
        @{path='I:\LeaBuildStorage\source-20261031-v102\current-source.zip';sha256='0390584BFE7C3BEE17821BC341B99777D99A0513BAB5FC2778BC4F17C3DE9015';scope='Current root/Lea source checkpoint; external, not bundled'},
        @{path='I:\LeaBuildStorage\portable-sdk-source-package-20261101-v2\Lea-full-sdk-source-development-v3.zip';sha256='84118D67704FC3DB8D941E3384C2B624D4A0B56832F437F0B91A2FBD34E5FC0E';scope='Exact deterministic SDK input sources; external, not bundled'},
        @{path='I:\LeaBuildStorage\deterministic-firespider-source-20261102-v1\candidate-manifest.json';sha256='B5D5787E5574FB1CE7666F2A5B2B1EB1DE6E35B1C8F5CC50127A20FF971480C6';scope='Preserved host source and dependency inventory; external, not bundled'}
    )
    foreach($source in $sources){Pin $source.path $source.sha256}
    $runtime=[IO.Path]::GetFullPath($r.runtime)
    if(!$runtime.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Runtime outside workspace'}
    $rows=[Collections.Generic.List[object]]::new()
    $names=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach($file in $r.before){
        $name=[IO.Path]::GetRelativePath($runtime,$file.path).Replace('\','/')
        if([IO.Path]::IsPathRooted($name) -or $name.Contains(':') -or $name.Split('/') -contains '..' -or !$names.Add($name)){throw 'Unsafe package member'}
        Pin $file.path $file.sha256
        if((Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Qualified runtime ACL changed'}
        $rows.Add(@{path=$name;sha256=$file.sha256;bytes=(Get-Item $file.path).Length;source=$file.path})
    }
    if(@(Get-ChildItem $runtime -Recurse -File).Count -ne 400){throw 'Runtime inventory differs'}
    New-Item -ItemType Directory -Path $out|Out-Null
    $manifest=@{developmentOnly=$true;sourceIncluded=$false;sourceReferences=$sources;githubUploadVerified=$false;runtimeVersion='8.0.33';rollForward='Disable';requires=@('.NET Framework 4.8','Microsoft.NETCore.App 8.0.33 x64');hostSha256='B1B290CA5E90B0F5DD2460BFA3A90DCB81EF8F0ADF5DA96599D437B36B3F416E';workflowReceiptSha256=$receiptHash;printReceiptSha256=$printHash;files=@($rows|Select-Object path,sha256,bytes);limitations=@('Fresh ZIP extraction not yet tested','Intermittent scripting/startup timing failures unresolved','Not all-site compatibility or clean-PC qualification','Physical printing and full fidelity unverified','Third-party notices/redistribution review incomplete','Not a final release')}
    $manifest|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'lea-development-manifest.json') -Encoding utf8
    'Firespider with Lea - development build. Extract all files to a new folder. Requires .NET Framework 4.8 and Microsoft.NETCore.App 8.0.33 x64 (roll-forward disabled). Run Firespider Internet 19.exe. Do not replace an installed browser or its profile. This ZIP is compressed for storage, not faster execution. See lea-development-manifest.json for exact hashes, external source references and unresolved limits. No final release or all-sites claim.'|Set-Content (Join-Path $out 'DEVELOPMENT.txt') -Encoding utf8
    foreach($name in @('lea-development-manifest.json','DEVELOPMENT.txt')){if(!$names.Add($name)){throw 'Metadata collision'};$path=Join-Path $out $name;$rows.Add(@{path=$name;sha256=(Get-FileHash $path).Hash;bytes=(Get-Item $path).Length;source=$path})}
    $archive=Join-Path $out 'Firespider-Lea-deterministic-development-20261102.zip'
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{foreach($row in $rows){$null=[IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,$row.source,$row.path,[IO.Compression.CompressionLevel]::Optimal)}}finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{
        if($zip.Entries.Count -ne 402){throw 'ZIP inventory differs'}
        foreach($row in $rows){$entry=$zip.GetEntry($row.path);if(!$entry -or $entry.Length -ne $row.bytes){throw 'ZIP member size differs'};$stream=$entry.Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $row.sha256){throw 'ZIP readback differs'}}finally{$stream.Dispose()}}
    }finally{$zip.Dispose()}
    @{verified=$true;archive=$archive;sha256=(Get-FileHash $archive).Hash;bytes=(Get-Item $archive).Length;entries=402;runtimeFiles=400;sourceIncluded=$false;extractedExecutionVerified=$false;fullReleaseQualified=$false;members=$rows}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    Get-FileHash $archive
}finally{foreach($s in $holds){$s.Dispose()}}
