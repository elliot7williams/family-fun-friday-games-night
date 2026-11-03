param([string]$Destination='I:\LeaBuildStorage\github-source-20261102-v1')
$ErrorActionPreference='Stop'
function G { & git -c safe.directory=$Destination -c maintenance.auto=false -c gc.auto=0 -C $Destination @args; if($LASTEXITCODE){throw 'Git verification command failed'} }
$branch='codex/lea-source-backup-20261102-v1'
if((G branch --show-current) -cne $branch){throw 'Wrong backup branch'}
$prefix='LeaSourceCheckpoints/20261102'
$snapshot=Join-Path $Destination $prefix
$manifest=Get-Content (Join-Path $snapshot 'manifest.json') -Raw | ConvertFrom-Json
foreach($row in $manifest.files){if((Get-FileHash (Join-Path $snapshot $row.path)).Hash -cne $row.sha256){throw 'Snapshot source changed'}}
G add -f -- $prefix
G add --renormalize -- $prefix
$staged=G diff --cached --name-only
if($staged){
    foreach($path in $staged){if(!$path.StartsWith($prefix+'/')){throw 'Unrelated staged file'}}
    G -c user.name='Elliot Williams' -c user.email='58864955+elliot7williams@users.noreply.github.com' commit -m 'Preserve exact source checkpoint bytes without line-ending conversion'
}
$paths=@($manifest.files|ForEach-Object {$prefix+'/'+$_.path})
$paths+="$prefix/manifest.json"
$paths+="$prefix/.gitattributes"
$full=@($paths|ForEach-Object {(Join-Path $Destination $_).Replace('\','/')})
$hashes=@($full | & git -c safe.directory=$Destination -C $Destination hash-object --no-filters --stdin-paths)
if($LASTEXITCODE -or $hashes.Count -ne $paths.Count){throw 'Raw blob verification failed'}
$tree=@{}
foreach($line in (G ls-tree -r '--format=%(objectname) %(path)' HEAD -- $prefix)){
    $parts=$line.Split(' ',2);$tree[$parts[1]]=$parts[0]
}
if($tree.Count -ne $paths.Count){throw 'Tracked snapshot inventory mismatch'}
for($i=0;$i -lt $paths.Count;$i++){if($tree[$paths[$i]] -cne $hashes[$i]){throw "Git blob differs from original bytes: $($paths[$i])"}}
G push origin "HEAD:refs/heads/$branch"
$head=(G rev-parse HEAD).Trim()
$remote=G ls-remote origin "refs/heads/$branch"
if(($remote -split '\s+')[0] -cne $head){throw 'Remote head mismatch'}
$readback=Join-Path $Destination 'remote-manifest.json'
Invoke-WebRequest -Uri "https://raw.githubusercontent.com/elliot7williams/family-fun-friday-games-night/$head/$prefix/manifest.json" -OutFile $readback
if((Get-FileHash $readback).Hash -cne (Get-FileHash (Join-Path $snapshot 'manifest.json')).Hash){throw 'Independent remote manifest mismatch'}
[ordered]@{verifiedRemote=$true;exactGitBlobs=$paths.Count;sourceFiles=$manifest.files.Count;commit=$head;branch=$branch;remoteManifestSha256=(Get-FileHash $readback).Hash;release=$false;sourceCheckpoint=128}|ConvertTo-Json|Set-Content (Join-Path $Destination 'verified-backup-receipt.json') -Encoding utf8
Get-Content (Join-Path $Destination 'verified-backup-receipt.json')
