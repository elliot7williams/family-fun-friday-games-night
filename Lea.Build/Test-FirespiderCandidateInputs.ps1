param(
    [Parameter(Mandatory=$true)][Alias('r')][string]$Runtime,
    [Parameter(Mandatory=$true)][Alias('o')][string]$Out
)

$ErrorActionPreference = 'Stop'
$builder = Join-Path $PSScriptRoot 'Build-FirespiderCandidate.ps1'
$root = [IO.Path]::GetFullPath($Out).TrimEnd('\')
if (Test-Path -LiteralPath $root) { throw 'Use a fresh contract fixture folder.' }
$runtimeRoot = (Resolve-Path -LiteralPath $Runtime).Path
New-Item -ItemType Directory -Path $root | Out-Null
$source = Join-Path $root 'source'
New-Item -ItemType Directory -Path $source | Out-Null
$project = New-Object Xml.XmlDocument
$project.LoadXml('<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003" />')
$project.Save((Join-Path $source 'Firespider Internet.vbproj'))
$checks = 0

function Reject([string]$name, [string]$runtimePath, [string]$pattern, [string]$outputPath = '') {
    if ($outputPath -eq '') { $outputPath = Join-Path $root ("out-$name") }
    $existed = Test-Path -LiteralPath $outputPath
    $message = ''
    try { & $builder -s $source -r $runtimePath -o $outputPath | Out-Null }
    catch { $message = $_.Exception.Message }
    if ($message -notmatch $pattern) { throw "$name rejection differs: $message" }
    if (-not $existed -and (Test-Path -LiteralPath $outputPath)) { throw "$name created candidate output before rejecting inputs" }
    $script:checks++
    Write-Output "PASS candidate input: $name"
}

function Fixture([string]$name, [string[]]$files) {
    $path = Join-Path $root $name
    New-Item -ItemType Directory -Path $path | Out-Null
    foreach ($file in $files) {
        $target = Join-Path $path $file
        New-Item -ItemType Directory -Path (Split-Path $target) -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path $runtimeRoot $file) -Destination $target
    }
    return $path
}

if ((Get-Command $builder).Parameters['Runtime'].Aliases -notcontains 'r') { throw 'Runtime short alias is missing.' }
$checks++
Write-Output 'PASS candidate input: runtime alias'
Reject 'absent-folder' (Join-Path $root 'absent') 'does not exist'
Reject 'file-not-folder' (Join-Path $source 'Firespider Internet.vbproj') 'Runtime must be a folder'
Reject 'nested-output' $runtimeRoot 'separate from the runtime input folder' (Join-Path $runtimeRoot 'contract-output')
Reject 'source-output' $runtimeRoot 'separate from the live source folder' (Join-Path $source 'contract-output')
$core = @('Lea.Engine.dll', 'Lea.Win.dll', 'Lea.Image.exe')
Reject 'missing-core' (Fixture 'no-core' @()) 'Missing runtime input: Lea.Engine.dll'
Reject 'missing-parent' (Fixture 'no-parent' $core) 'Missing runtime input: Lea.Script.Win.dll'
$parent = @('Lea.Script.Win.dll', 'Lea.Script.dll', 'Jint.dll', 'Acornima.dll', 'System.Memory.dll',
    'System.Buffers.dll', 'System.Numerics.Vectors.dll', 'System.Runtime.CompilerServices.Unsafe.dll')
Reject 'missing-worker' (Fixture 'no-worker' ($core + $parent)) 'Missing runtime input: ScriptWorker/Lea.Script.Worker.exe'
$worker = @('Lea.Script.Worker.exe', 'Lea.Script.Worker.exe.config', 'Jint.dll', 'Acornima.dll', 'System.Memory.dll',
    'System.Buffers.dll', 'System.Numerics.Vectors.dll', 'System.Runtime.CompilerServices.Unsafe.dll') |
    ForEach-Object { "ScriptWorker/$_" }
Reject 'missing-config' (Fixture 'no-config' ($core + $parent + $worker)) 'Missing runtime input: Lea.Script.Browser.exe.config'
$occupied = Join-Path $root 'occupied'
New-Item -ItemType Directory -Path $occupied | Out-Null
Copy-Item -LiteralPath (Join-Path $runtimeRoot 'Lea.Engine.dll') -Destination $occupied
$before = (Get-FileHash -LiteralPath (Join-Path $occupied 'Lea.Engine.dll')).Hash
Reject 'existing-output' $runtimeRoot 'Candidate folder already exists' $occupied
if ($before -ne (Get-FileHash -LiteralPath (Join-Path $occupied 'Lea.Engine.dll')).Hash -or
    @(Get-ChildItem -LiteralPath $occupied -File -Recurse).Count -ne 1) { throw 'Existing output changed.' }
Write-Output "PASS candidate inputs: $checks checks; no compiler or browser launched."
