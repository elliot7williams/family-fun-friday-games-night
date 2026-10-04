param(
    [Alias('o')][string]$Out = (Join-Path $PSScriptRoot '..\dist\LeaScript')
)

$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$outPath = [IO.Path]::GetFullPath($Out)
$stamp = Get-Date -Format 'yyyy-MM-dd_HH-mm-ss'
$stage = Join-Path $outPath "Lea-js-146-browser_$stamp"
$zip = Join-Path $outPath "Lea-js-146-native_$stamp.zip"
$runtime = Join-Path $root 'Lea.Script.Browser\bin\Release\net462'
$tests = Join-Path $root 'Lea.Script.Tests\bin\Release\net462'
$names = @('Lea.Script.Browser.exe', 'Lea.Script.Browser.exe.config', 'Lea.Engine.dll',
    'Lea.Win.dll', 'Lea.Image.exe', 'Lea.Script.Win.dll', 'Lea.Script.dll', 'Jint.dll', 'Acornima.dll', 'System.Memory.dll', 'System.Buffers.dll',
    'System.Numerics.Vectors.dll', 'System.Runtime.CompilerServices.Unsafe.dll')
$workerNames = @('Lea.Script.Worker.exe', 'Lea.Script.Worker.exe.config', 'Jint.dll', 'Acornima.dll',
    'System.Memory.dll', 'System.Buffers.dll', 'System.Numerics.Vectors.dll', 'System.Runtime.CompilerServices.Unsafe.dll')

foreach ($name in $names) {
    if (-not (Test-Path -LiteralPath (Join-Path $runtime $name) -PathType Leaf)) { throw "Build Release first; missing $name" }
}
foreach ($name in @('Lea.Script.Tests.exe', 'Lea.Script.Tests.exe.config')) {
    if (-not (Test-Path -LiteralPath (Join-Path $tests $name) -PathType Leaf)) { throw "Build Release tests first; missing $name" }
}
foreach ($name in $workerNames) {
    if (-not (Test-Path -LiteralPath (Join-Path $runtime "ScriptWorker\$name") -PathType Leaf)) { throw "Build Release first; missing worker $name" }
    if ((Get-FileHash -LiteralPath (Join-Path $runtime "ScriptWorker\$name")).Hash -ne (Get-FileHash -LiteralPath (Join-Path $tests "ScriptWorker\$name")).Hash) {
        throw "Browser/test worker mismatch: $name"
    }
}
foreach ($name in $names | Where-Object { $_.EndsWith('.dll') -or $_ -eq 'Lea.Image.exe' }) {
    if ((Get-FileHash -LiteralPath (Join-Path $runtime $name)).Hash -ne (Get-FileHash -LiteralPath (Join-Path $tests $name)).Hash) {
        throw "Browser/test runtime mismatch: $name"
    }
}
if (Test-Path -LiteralPath $stage) { throw "Package stage already exists: $stage" }
New-Item -ItemType Directory -Path (Join-Path $stage 'ScriptWorker') | Out-Null
New-Item -ItemType Directory -Path (Join-Path $stage 'LeaForge\test-pages') | Out-Null
foreach ($name in $names) { Copy-Item -LiteralPath (Join-Path $runtime $name) -Destination $stage }
foreach ($name in @('Lea.Script.Tests.exe', 'Lea.Script.Tests.exe.config')) { Copy-Item -LiteralPath (Join-Path $tests $name) -Destination $stage }
foreach ($name in $workerNames) {
    Copy-Item -LiteralPath (Join-Path $runtime "ScriptWorker\$name") -Destination (Join-Path $stage 'ScriptWorker')
}
Copy-Item -LiteralPath (Join-Path $root 'Lea.Script\README.md') -Destination $stage
Copy-Item -LiteralPath (Join-Path $root 'Lea.Script\licenses') -Destination $stage -Recurse
Copy-Item -LiteralPath (Join-Path $root 'LeaForge\test-pages\javascript-runtime.html') -Destination (Join-Path $stage 'LeaForge\test-pages')
Copy-Item -LiteralPath (Join-Path $root 'LeaForge\test-pages\javascript-view.html') -Destination (Join-Path $stage 'LeaForge\test-pages')
New-Item -ItemType Directory -Path (Join-Path $stage 'assets') | Out-Null
Copy-Item -LiteralPath (Join-Path $root 'assets\lea.ico') -Destination (Join-Path $stage 'assets')

$files = @(Get-ChildItem -LiteralPath $stage -File -Recurse | Sort-Object FullName | ForEach-Object {
    [ordered]@{ path=$_.FullName.Substring($stage.Length + 1).Replace('\', '/'); bytes=$_.Length;
        sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
})
[ordered]@{ milestone=146; kind='native-script-browser'; framework='net462'; files=$files } |
    ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $stage 'manifest.json') -Encoding UTF8
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip -CompressionLevel Optimal
[pscustomobject]@{ Package=$zip; Bytes=(Get-Item -LiteralPath $zip).Length;
    SHA256=(Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash; Stage=$stage } | Format-List
