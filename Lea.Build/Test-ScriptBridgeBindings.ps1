param(
    [Parameter(Mandatory=$true)][string]$Runtime,
    [Parameter(Mandatory=$true)][string]$Report,
    [string]$Ildasm = (Join-Path ${env:ProgramFiles(x86)} 'Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\ildasm.exe')
)
$ErrorActionPreference = 'Stop'
$runtimePath = (Resolve-Path -LiteralPath $Runtime).ProviderPath
$tool = (Resolve-Path -LiteralPath $Ildasm).ProviderPath
$reportPath = [IO.Path]::GetFullPath($Report)
if (Test-Path -LiteralPath $reportPath) { throw 'Use a new report path.' }
if (-not (Test-Path -LiteralPath (Split-Path $reportPath) -PathType Container)) { throw 'Report directory does not exist.' }

function Hash([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash }
function Save-New([string]$Path, [string]$Text) {
    $bytes = [Text.UTF8Encoding]::new($false).GetBytes($Text)
    $stream = [IO.File]::Open($Path, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write)
    try { $stream.Write($bytes, 0, $bytes.Length) } finally { $stream.Dispose() }
}

# Inspect compiled call targets, not source spelling or observed timings.
$targets = @(
    [pscustomobject]@{ name='trusted'; file='Lea.Script.dll'; method='Lea.Script.Js::Start' },
    [pscustomobject]@{ name='worker'; file='ScriptWorker\Lea.Script.Worker.exe'; method='Lea.Script.Worker.Program::Run' }
)
$started = [DateTime]::UtcNow
$methods = @()
foreach ($target in $targets) {
    $path = Join-Path $runtimePath $target.file
    $before = Hash $path
    $lines = @(& $tool $path /text /nobar (('/item:') + $target.method))
    if ($LASTEXITCODE -ne 0) { throw ('IL disassembly failed: ' + $target.method) }
    $il = $lines -join "`n"
    $ilPath = Join-Path (Split-Path $reportPath) (([IO.Path]::GetFileNameWithoutExtension($reportPath)) + '-' + $target.name + '.il')
    Save-New $ilPath $il
    $direct = [regex]::Matches($il, 'callvirt\s+instance class \[Jint\]Jint\.Engine \[Jint\]Jint\.Engine::SetValue\(string,\s+class \[Jint\]Jint\.Native\.JsValue\)').Count
    $generic = [regex]::Matches($il, 'Engine::SetValue<').Count
    $total = [regex]::Matches($il, 'Engine::SetValue(?:<|\()').Count
    if ((Hash $path) -ne $before) { throw 'Runtime changed during inspection.' }
    $methods += [pscustomobject]@{
        name=$target.name; method=$target.method; file=$target.file
        bytes=(Get-Item -LiteralPath $path).Length; sha256=$before
        directCalls=$direct; genericCalls=$generic; totalCalls=$total
        passed=($direct -eq 2 -and $generic -eq 0 -and $total -eq 2)
        disassembly=[IO.Path]::GetFileName($ilPath); disassemblySHA256=(Hash $ilPath)
    }
}
$passed = @($methods | Where-Object { -not $_.passed }).Count -eq 0
$result = [pscustomobject]@{
    startedUtc=$started; finishedUtc=[DateTime]::UtcNow; runtime=$runtimePath
    disassembler=$tool; disassemblerSHA256=(Hash $tool); methods=$methods; passed=$passed
    startupCauseResolved=$false; speedupEstablished=$false
}
Save-New $reportPath (ConvertTo-Json -InputObject $result -Depth 8)
if (-not $passed) { throw 'Bridge bindings must call SetValue(string, JsValue) directly in both startup methods.' }
Write-Output 'PASS Lea.Script bridge bindings: both startup methods use two direct JsValue calls.'
