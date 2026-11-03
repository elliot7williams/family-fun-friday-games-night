function Get-ConsolidatedBuildArchive {
    param([Parameter(Mandatory)][string]$OriginalPath,[Parameter(Mandatory)][string]$Destination,[switch]$ValidateOnly)
    $root=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
    $source=[IO.Path]::GetFullPath((Join-Path $root 'dist/LeaExes'))
    $original=[IO.Path]::GetFullPath($OriginalPath)
    if([IO.Path]::GetDirectoryName($original) -cne $source){throw 'Only explicit old build archive directory supported'}
    $store=$null;$entry=$null;$receipt=$null
    foreach($version in 1..3){
        $candidate=Join-Path $root ('continuity-evidence/solid-build-archives-v'+$version)
        $manifestPath=Join-Path $candidate 'originals.json';$receiptPath=Join-Path $candidate 'consolidation.json'
        if(!(Test-Path $receiptPath)){continue}
        $state=Get-Content $receiptPath -Raw|ConvertFrom-Json
        if(!$state.verified){continue}
        if((Get-FileHash $manifestPath).Hash -cne $state.manifestSha256){throw 'Verified consolidation manifest required'}
        $manifest=Get-Content $manifestPath -Raw|ConvertFrom-Json -DateKind String
        $matches=@($manifest.originals|Where-Object path -CEQ $original)
        if($matches.Count -gt 1 -or ($matches.Count -and $entry)){throw 'Ambiguous consolidated archive'}
        if($matches.Count){
            if(!$ValidateOnly -and !$state.recoveryPathVerified){throw 'Recovery qualification incomplete'}
            $store=$candidate;$entry=$matches[0];$receipt=$state
        }
    }
    if(!$entry){throw 'Original archive is not in verified container'}
    $names=@('Lea-runtime-210-ready_2026-09-19_16-01-25.zip','Lea-runtime-211-ready_2026-09-19_20-45-48.zip',
        'Lea-runtime-212-ready_2026-09-20_01-51-52.zip','Lea-runtime-213-ready_2026-09-20_06-23-03.zip',
        'Lea-runtime-214-ready_2026-09-20_07-00-46.zip','Lea-runtime-215-ready_2026-09-20_14-51-15.zip',
        'Lea-runtime-216-ready_2026-09-20_19-26-05.zip','Lea-runtime-217-ready_2026-09-22_16-26-29.zip')
    $names+=@('Lea-runtime-218-ready_2026-09-22_20-59-28.zip','Lea-runtime-219-ready_2026-09-22_22-06-32.zip','Lea-runtime-220-ready_2026-09-23_04-11-39.zip','Lea-runtime-221-ready_2026-09-23_08-09-47.zip','Lea-runtime-222-ready_2026-09-23_09-09-06.zip','Lea-runtime-224-ready_2026-09-23_16-45-23.zip')
    if($entry.name -notin $names -or $entry.path -cne (Join-Path $source $entry.name)){throw 'Original archive identity differs'}
    $destination=[IO.Path]::GetFullPath($Destination)
    $allowed=[IO.Path]::GetFullPath((Join-Path $store 'recovery-temp'))
    $shared=[IO.Path]::GetFullPath((Join-Path $root 'continuity-evidence/solid-build-archives-v1/recovery-temp'))
    if((!$destination.StartsWith($allowed+'\',[StringComparison]::OrdinalIgnoreCase) -and !$destination.StartsWith($shared+'\',[StringComparison]::OrdinalIgnoreCase)) -or (Test-Path $destination)){throw 'Fresh bounded recovery scratch directory required'}
    $ancestor=[IO.Path]::GetDirectoryName($destination)
    while(!(Test-Path -LiteralPath $ancestor)){$ancestor=[IO.Path]::GetDirectoryName($ancestor)}
    $walk=Get-Item -LiteralPath $ancestor
    while($walk){if($walk.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Recovery ancestor link'};$walk=$walk.Parent}
    $pack=Join-Path $store 'older-builds.7z'
    if((Get-FileHash $pack).Hash -cne $receipt.containerSha256){throw 'Consolidated container differs'}
    if((Get-PSDrive C).Free -lt 32MB+$entry.length){throw 'Recovery workspace reserve required; not a build waiver'}
    $tar=Join-Path $env:WINDIR 'System32/tar.exe'
    if((Get-FileHash $tar).Hash -cne '2CA7DB442FEAFA8E6D7A88C7DDDF3113128F85701A4EB1182C71CFBF3D479A56'){throw 'Tested recovery tool differs'}
    $null=New-Item -ItemType Directory -Path $destination
    & $tar -xkf $pack -C $destination $entry.name
    if($LASTEXITCODE -ne 0){throw 'Container extraction failed; retain scratch evidence'}
    $path=Join-Path $destination $entry.name
    $files=@(Get-ChildItem -LiteralPath $destination -Force)
    if($files.Count -ne 1 -or $files[0].PSIsContainer -or $files[0].Attributes -band [IO.FileAttributes]::ReparsePoint -or (Get-Item $path).Length -ne $entry.length -or (Get-FileHash $path).Hash -cne $entry.sha256){throw 'Restored archive differs; retain scratch evidence'}
    return $path
}
