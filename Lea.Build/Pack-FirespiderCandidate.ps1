param(
    [Parameter(Mandatory=$true)][Alias('c')][string]$Candidate,
    [Alias('o')][string]$Out = (Join-Path $PSScriptRoot '..\dist\Firespider')
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$root = (Resolve-Path -LiteralPath $Candidate).Path.TrimEnd('\')
$runtime = Join-Path $root 'runtime'
$manifestPath = Join-Path $root 'source-manifest.json'
$source = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ($source.milestone -notin @(145, 146) -or $source.framework -ne 'net48') { throw 'Expected a supported net48 source manifest (145 or 146).' }
$milestone = [int]$source.milestone
if (-not (Test-Path -LiteralPath (Join-Path $runtime 'Firespider Internet 19.exe'))) { throw 'Missing compiled candidate runtime.' }
$sourceFiles = @{}
foreach ($item in @($source.inputs) + @($source.overlays)) { $sourceFiles[$item.path] = $item }
foreach ($item in $sourceFiles.Values) {
    $path = [IO.Path]::GetFullPath((Join-Path $root $item.path))
    if (-not $path.StartsWith($root + '\', [StringComparison]::OrdinalIgnoreCase)) { throw "Snapshot path escapes candidate: $($item.path)" }
    if ((Get-Item -LiteralPath $path).Length -ne $item.bytes -or (Get-FileHash -LiteralPath $path).Hash -ne $item.sha256) {
        throw "Snapshot input changed: $($item.path)"
    }
}
foreach ($item in @($source.runtimeInputs)) {
    if ($null -eq $item -or $item.path -eq 'Lea.Script.Browser.exe.config') { continue }
    $path = [IO.Path]::GetFullPath((Join-Path $runtime $item.path))
    if (-not $path.StartsWith($runtime + '\', [StringComparison]::OrdinalIgnoreCase)) { throw "Runtime path escapes candidate: $($item.path)" }
    if ((Get-Item -LiteralPath $path).Length -ne $item.bytes -or (Get-FileHash -LiteralPath $path).Hash -ne $item.sha256) {
        throw "Compiled runtime differs from selected input: $($item.path)"
    }
}
$sourceFiles['source-manifest.json'] = @{ path='source-manifest.json'; bytes=(Get-Item -LiteralPath $manifestPath).Length; sha256=(Get-FileHash -LiteralPath $manifestPath).Hash }
$sourceEntries = @($sourceFiles.Values | ForEach-Object {
    [pscustomobject]@{ path=$_.path; full=(Join-Path $root $_.path) }
})
if ($source.optionalScript) {
    $sourceEntries += @(Get-ChildItem -LiteralPath (Join-Path $runtime 'licenses') -File -Recurse | ForEach-Object {
        [pscustomobject]@{ path=('licenses/' + $_.FullName.Substring((Join-Path $runtime 'licenses').Length + 1).Replace('\', '/')); full=$_.FullName }
    })
}
$runtimeEntries = @(Get-ChildItem -LiteralPath $runtime -File -Recurse | Where-Object {
    $_.Name -ne 'Lea.Firespider.Boot.exe' -and $_.Name -ne 'Firespider Internet 3.xml'
} | ForEach-Object { [pscustomobject]@{ path=$_.FullName.Substring($runtime.Length + 1).Replace('\', '/'); full=$_.FullName } })
$outPath = [IO.Path]::GetFullPath($Out)
if ($outPath -ne [IO.Path]::GetPathRoot($outPath)) { $outPath = $outPath.TrimEnd('\') }
if ($outPath -eq $root -or $outPath.StartsWith($root + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Package output must be outside the candidate source folder.' }
if (-not (Test-Path -LiteralPath $outPath -PathType Container)) {
    New-Item -ItemType Directory -Path $outPath | Out-Null
}
$mode = if ($source.optionalScript) { 'script' } else { 'small' }
$stamp = Get-Date -Format 'yyyy-MM-dd_HH-mm-ss'

function Write-Package([string]$kind, [object[]]$entries) {
    $zip = Join-Path $outPath "Firespider-Lea-$milestone-$mode-$kind`_$stamp.zip"
    $records = @($entries | Sort-Object path | ForEach-Object {
        [ordered]@{ path=$_.path; bytes=(Get-Item -LiteralPath $_.full).Length; sha256=(Get-FileHash -LiteralPath $_.full).Hash }
    })
    $archive = [IO.Compression.ZipFile]::Open($zip, [IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($item in $entries) {
            $null = [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $item.full, $item.path, [IO.Compression.CompressionLevel]::Optimal)
        }
        $entry = $archive.CreateEntry('package-manifest.json')
        $stream = $entry.Open()
        $writer = New-Object IO.StreamWriter($stream, (New-Object Text.UTF8Encoding($false)))
        try {
            $writer.Write(([ordered]@{ milestone=$milestone; kind=$kind; framework='net48'; optionalScript=$source.optionalScript; files=$records } | ConvertTo-Json -Depth 6))
        }
        finally { $writer.Dispose(); $stream.Dispose() }
    }
    finally { $archive.Dispose() }
    [pscustomobject]@{ Kind=$kind; Package=$zip; Bytes=(Get-Item -LiteralPath $zip).Length; Files=($entries.Count + 1); SHA256=(Get-FileHash -LiteralPath $zip).Hash }
}

Write-Package 'runtime' $runtimeEntries
Write-Package 'host-source' $sourceEntries
