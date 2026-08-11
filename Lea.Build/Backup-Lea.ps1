param(
    [string]$Out = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Lea Source Backups')
)

$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$stamp = Get-Date -Format 'yyyy-MM-dd_HH-mm-ss'
$zip = Join-Path $Out ("Lea-source-$stamp.zip")
$stage = Join-Path ([IO.Path]::GetTempPath()) ("Lea-source-$([Guid]::NewGuid().ToString('N'))")
$names = @(
    'assets', 'Lea', 'Lea.Build', 'Lea.Core', 'Lea.Engine', 'Lea.Firespider',
    'Lea.Micro', 'Lea.Win', 'Lea.Win.Test', 'LeaBrowser', 'LeaForge', 'LeaPlus'
)

New-Item -ItemType Directory -Path $Out -Force | Out-Null
New-Item -ItemType Directory -Path $stage -Force | Out-Null

try {
    foreach ($name in $names) {
        $source = Join-Path $root $name
        if (-not (Test-Path -LiteralPath $source)) { continue }

        Get-ChildItem -LiteralPath $source -Recurse -File | Where-Object {
            $_.FullName -notmatch '\\(bin|obj|dist|\.git|\.vs)\\'
        } | ForEach-Object {
            $relative = $_.FullName.Substring($root.Length).TrimStart('\')
            $target = Join-Path $stage $relative
            New-Item -ItemType Directory -Path (Split-Path $target) -Force | Out-Null
            Copy-Item -LiteralPath $_.FullName -Destination $target
        }
    }

    Copy-Item -LiteralPath (Join-Path $root 'Lea.sln') -Destination $stage
    Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip -CompressionLevel Optimal
    Get-Item -LiteralPath $zip
}
finally {
    $resolvedStage = [IO.Path]::GetFullPath($stage)
    $tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
    if ($resolvedStage.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase) -and
        (Split-Path $resolvedStage -Leaf).StartsWith('Lea-source-')) {
        Remove-Item -LiteralPath $resolvedStage -Recurse -Force -ErrorAction SilentlyContinue
    }
}
