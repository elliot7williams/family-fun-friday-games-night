param(
    [string]$Source = 'L:\Firespider Internet',
    [string]$Out = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Firespider Source Backups')
)

$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath($Source)
if (-not (Test-Path -LiteralPath $root -PathType Container)) {
    throw "Firespider source folder was not found: $root"
}

$stamp = Get-Date -Format 'yyyy-MM-dd_HH-mm-ss'
$zip = Join-Path $Out ("Firespider-source-$stamp.zip")
$stage = Join-Path ([IO.Path]::GetTempPath()) ("Firespider-source-$([Guid]::NewGuid().ToString('N'))")
$excludedFolders = @(
    '.git', '.vs', 'bin', 'obj', '$(FSBUILDROOT)',
    'Firespider Internet-cache', 'Firespider Internet-SetupFiles'
)
$excludedExtensions = @('.application', '.dll', '.exe', '.manifest', '.pdb', '.suo', '.user')

New-Item -ItemType Directory -Path $Out -Force | Out-Null
New-Item -ItemType Directory -Path $stage -Force | Out-Null

try {
    Get-ChildItem -LiteralPath $root -Recurse -File | Where-Object {
        $file = $_
        $relative = $file.FullName.Substring($root.Length).TrimStart('\')
        $parts = $relative.Split('\')
        $leaf = $file.Name
        -not ($parts | Where-Object { $excludedFolders -contains $_ }) -and
        -not $leaf.StartsWith('._', [StringComparison]::Ordinal) -and
        $leaf -ne '.DS_Store' -and
        ($excludedExtensions -notcontains $file.Extension.ToLowerInvariant() -or $relative -eq 'My Project\app.manifest')
    } | ForEach-Object {
        $relative = $_.FullName.Substring($root.Length).TrimStart('\')
        $target = Join-Path $stage $relative
        New-Item -ItemType Directory -Path (Split-Path $target) -Force | Out-Null
        Copy-Item -LiteralPath $_.FullName -Destination $target
    }

    Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip -CompressionLevel Optimal
    Get-Item -LiteralPath $zip
}
finally {
    $resolvedStage = [IO.Path]::GetFullPath($stage)
    $tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
    if ($resolvedStage.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase) -and
        (Split-Path $resolvedStage -Leaf).StartsWith('Firespider-source-')) {
        Remove-Item -LiteralPath $resolvedStage -Recurse -Force -ErrorAction SilentlyContinue
    }
}
