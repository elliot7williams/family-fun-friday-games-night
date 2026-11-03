param([string]$Destination='I:\LeaBuildStorage\github-source-20261102-v1',[switch]$ResumeClone)
$ErrorActionPreference='Stop'
$repo='https://github.com/elliot7williams/family-fun-friday-games-night.git'
$branch='codex/lea-source-backup-20261102-v1'
if($ResumeClone){
    $origin=& git -c safe.directory=$Destination -C $Destination remote get-url origin
    $current=& git -c safe.directory=$Destination -C $Destination branch --show-current
    $dirty=& git -c safe.directory=$Destination -C $Destination status --porcelain
    if($LASTEXITCODE -or $origin -cne $repo -or $current -cne 'codex/lea-engine-foundation' -or $dirty){throw 'Resume requires unchanged verified clone'}
}else{
    if(Test-Path -LiteralPath $Destination){throw 'Existing destination retained'}
    & git clone --depth 1 --single-branch --branch codex/lea-engine-foundation $repo $Destination
    if($LASTEXITCODE){throw 'Clone failed'}
}
Push-Location $Destination
try {
    & git -c safe.directory=$Destination switch -c $branch
    if($LASTEXITCODE){throw 'Branch creation failed'}
    $snapshot=Join-Path $Destination 'LeaSourceCheckpoints/20261102'
    New-Item -ItemType Directory -Path $snapshot | Out-Null
    $rows=[Collections.Generic.List[object]]::new()
    function Save-Source([string]$source,[string]$relative,[string]$expected='') {
        $hash=(Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
        if($expected -and $hash -cne $expected){throw "Input changed: $relative"}
        $target=Join-Path $snapshot $relative
        New-Item -ItemType Directory -Path (Split-Path $target -Parent) -Force | Out-Null
        Copy-Item -LiteralPath $source -Destination $target
        if((Get-FileHash -LiteralPath $target).Hash -cne $hash){throw 'Copy mismatch'}
        $rows.Add([pscustomobject]@{path=$relative.Replace('\','/');sha256=$hash;bytes=(Get-Item -LiteralPath $target).Length})
    }
    $checkpoint='I:\LeaBuildStorage\source-20261031-v128'
    $proof=Get-Content (Join-Path $checkpoint 'result.json') -Raw | ConvertFrom-Json
    if(!$proof.verified -or (Get-FileHash $proof.archive).Hash -cne $proof.sha256){throw 'Checkpoint not verified'}
    Add-Type -AssemblyName System.IO.Compression
    $zip=[IO.Compression.ZipFile]::OpenRead($proof.archive)
    try {
        foreach($row in $proof.entries){
            if($row.path -notmatch '^Lea[^/]*/' -and $row.path -notin @('PROJECT_CONTEXT.md','AGENTS.md')){continue}
            if($row.path -match '(^|/)\.\.?(/|$)' -or [IO.Path]::IsPathRooted($row.path)){throw 'Unsafe entry'}
            $entry=$zip.GetEntry($row.path)
            $target=Join-Path $snapshot ('workspace/'+$row.path)
            New-Item -ItemType Directory -Path (Split-Path $target -Parent) -Force | Out-Null
            $input=$entry.Open();$output=[IO.File]::Create($target)
            try{$input.CopyTo($output)}finally{$input.Dispose();$output.Dispose()}
            if((Get-FileHash $target).Hash -cne $row.sha256){throw 'Archive member mismatch'}
            $rows.Add([pscustomobject]@{path=('workspace/'+$row.path);sha256=$row.sha256;bytes=$row.bytes})
        }
    } finally {$zip.Dispose()}
    $sdk='I:\LeaBuildStorage\full-sdk-source-20261101-v3'
    $extensions=@('.cs','.csproj','.js','.json','.ps1','.md','.props','.targets','.proj','.config','.html','.css','.txt')
    foreach($file in Get-ChildItem -LiteralPath $sdk -Recurse -File){
        if($file.Extension -in $extensions -and $file.FullName -notmatch '\\(lib|bin|obj)\\'){
            Save-Source $file.FullName ('mature-sdk/'+[IO.Path]::GetRelativePath($sdk,$file.FullName))
        }
    }
    foreach($candidate in @('global-install','lazy-map')){
        $directory="I:\LeaBuildStorage\$candidate-source-20261102-v1"
        $source=Get-Content (Join-Path $directory 'source.json') -Raw | ConvertFrom-Json
        foreach($file in $source.files){Save-Source $file.path ("candidates/$candidate/"+$file.name) $file.sha256}
        Save-Source (Join-Path $directory 'source.json') "candidates/$candidate/source.json"
    }
    $sensitive=@(Get-ChildItem $snapshot -Recurse -File | Select-String -Pattern '-----BEGIN (RSA |EC |OPENSSH )?PRIVATE KEY-----|gh[pousr]_[A-Za-z0-9]{30,}|github_pat_[A-Za-z0-9_]{40,}|AKIA[0-9A-Z]{16}' -List)
    if($sensitive.Count){throw 'Potential credentials found; do not publish'}
    [ordered]@{date='2026-11-02';sourceCheckpoint=128;archiveSha256=$proof.sha256;release=$false;thirdPartyBinariesIncluded=$false;files=$rows}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $snapshot 'manifest.json') -Encoding utf8
    [IO.File]::WriteAllText((Join-Path $snapshot '.gitattributes'),"* -text`n")
    & git -c safe.directory=$Destination add -f -- LeaSourceCheckpoints/20261102
    if($LASTEXITCODE){throw 'Stage failed'}
    & git -c safe.directory=$Destination -c user.name='Elliot Williams' -c user.email='58864955+elliot7williams@users.noreply.github.com' commit -m 'Back up Lea source checkpoint 128 and distinct mature SDK candidates'
    if($LASTEXITCODE){throw 'Commit failed'}
    & git -c safe.directory=$Destination push origin "HEAD:refs/heads/$branch"
    if($LASTEXITCODE){throw 'Push failed; local checkpoint preserved'}
    $head=(& git -c safe.directory=$Destination rev-parse HEAD).Trim()
    $remote=(& git -c safe.directory=$Destination ls-remote origin "refs/heads/$branch")
    if($LASTEXITCODE -or ($remote -split '\s+')[0] -cne $head){throw 'Remote verification failed'}
    [ordered]@{verifiedRemote=$true;commit=$head;branch=$branch;repository=$repo;files=$rows.Count;scope='Source only; mature SDK, workspace Lea trees, two candidates; excludes binaries, third-party libraries and raw test evidence'}|ConvertTo-Json|Set-Content (Join-Path $Destination 'backup-receipt.json') -Encoding utf8
} finally {Pop-Location}
