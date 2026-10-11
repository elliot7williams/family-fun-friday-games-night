param(
    [Parameter(Mandatory=$true)][Alias('c')][string]$Candidate,
    [Parameter(Mandatory=$true)][Alias('o')][string]$Out
)

$ErrorActionPreference = 'Stop'
$packer = Join-Path $PSScriptRoot 'Pack-FirespiderCandidate.ps1'
$candidateRoot = (Resolve-Path -LiteralPath $Candidate).Path
$root = [IO.Path]::GetFullPath($Out).TrimEnd('\')
if (Test-Path -LiteralPath $root) { throw 'Use a fresh package fixture folder.' }
New-Item -ItemType Directory -Path $root | Out-Null
$checks = 0
foreach ($case in @('changed-runtime', 'escaping-runtime', 'changed-source')) {
    $fixture = Join-Path $root $case
    $runtime = Join-Path $fixture 'runtime'
    New-Item -ItemType Directory -Path $runtime | Out-Null
    foreach ($name in @('Firespider Internet 19.exe', 'Lea.Engine.dll')) {
        Copy-Item -LiteralPath (Join-Path $candidateRoot "runtime\$name") -Destination $runtime
    }
    $project = Join-Path $fixture 'Firespider Internet.vbproj'
    Copy-Item -LiteralPath (Join-Path $candidateRoot 'Firespider Internet.vbproj') -Destination $project
    $engine = Join-Path $runtime 'Lea.Engine.dll'
    $input = [ordered]@{ path='Firespider Internet.vbproj'; bytes=(Get-Item $project).Length; sha256=(Get-FileHash $project).Hash }
    $selected = [ordered]@{ path='Lea.Engine.dll'; bytes=(Get-Item $engine).Length; sha256=(Get-FileHash $engine).Hash }
    if ($case -eq 'escaping-runtime') { $selected.path='../Firespider Internet.vbproj' }
    [ordered]@{ milestone=146; framework='net48'; optionalScript=$false; inputs=@($input); overlays=@(); runtimeInputs=@($selected) } |
        ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $fixture 'source-manifest.json') -Encoding UTF8
    if ($case -ne 'escaping-runtime') {
        $target = if ($case -eq 'changed-runtime') { $engine } else { $project }
        $stream = [IO.File]::Open($target, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite)
        try { $first=$stream.ReadByte(); $stream.Position=0; $stream.WriteByte([byte]($first -bxor 1)) } finally { $stream.Dispose() }
    }
    $output = Join-Path $root ("packages-$case")
    $message = ''
    try { & $packer -c $fixture -o $output | Out-Null }
    catch { $message=$_.Exception.Message }
    $pattern = switch ($case) {
        'changed-runtime' { 'Compiled runtime differs from selected input: Lea.Engine.dll' }
        'escaping-runtime' { 'Runtime path escapes candidate' }
        'changed-source' { 'Snapshot input changed: Firespider Internet.vbproj' }
    }
    if ($message -notmatch $pattern -or (Test-Path -LiteralPath $output)) { throw "$case rejection differs: $message" }
    $checks++
    Write-Output "PASS candidate package: $case"
}
Write-Output "PASS candidate packages: $checks checks; no browser launched."
