param(
    [Parameter(Mandatory=$true)][string]$Source,
    [Parameter(Mandatory=$true)][string]$Out,
    [string]$ReuseNativeFrom = '',
    [ValidateSet('diagnostic-urls')][string[]]$ExtraSmokeModes=@()
)

$ErrorActionPreference = 'Stop'
$sourceRoot = [IO.Path]::GetFullPath($Source).TrimEnd('\')
$outputRoot = [IO.Path]::GetFullPath($Out).TrimEnd('\')
$workspace = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..')).TrimEnd('\')
if (-not $outputRoot.StartsWith($workspace + '\', [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Output must be a new directory inside this workspace.'
}
if (Test-Path -LiteralPath $outputRoot) { throw 'Output already exists; previous evidence is never overwritten.' }
if ($outputRoot.StartsWith($sourceRoot + '\', [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Output cannot be inside the source snapshot.'
}
$compiler = (Get-Process -Id $PID).Path
$bridge = Join-Path $PSScriptRoot 'Invoke-NativeCompiler.ps1'
$references = 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6.1'
if ($PSVersionTable.PSVersion.Major -lt 7) { throw 'PowerShell 7 is required.' }
function CheckSpace {
    if ([IO.DriveInfo]::new([IO.Path]::GetPathRoot($outputRoot)).AvailableFreeSpace -lt 512MB) {
        throw 'At least 512 MiB free space is required.'
    }
}
function RejectLinks([string]$path) {
    $walk = [IO.Path]::GetFullPath($path)
    while ($walk) {
        if ((Get-Item -LiteralPath $walk -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) {
            throw "Linked path refused: $walk"
        }
        $walk = [IO.Path]::GetDirectoryName($walk)
    }
}
CheckSpace
RejectLinks $sourceRoot
RejectLinks ([IO.Path]::GetDirectoryName($outputRoot))
$null = New-Item -ItemType Directory -Path $outputRoot
$copyRoot = Join-Path $outputRoot 'source'
$pins = [Collections.Generic.List[object]]::new()
$runs = [Collections.Generic.List[object]]::new()
$result = [ordered]@{ completed=$false; passed=$false; source=$sourceRoot; output=$outputRoot;
    sourceInputs=$pins; runs=$runs; originalInputsUnchanged=$false; binaries=@();
    fullBrowserCertified=$false; scriptStartupTested=$false; failure=$null;
    compilerBridgeSha256=(Get-FileHash -LiteralPath $bridge -Algorithm SHA256).Hash }
function RunOwned([string]$name, [string]$exe, [string[]]$arguments, [int]$seconds) {
    CheckSpace
    $stdout = Join-Path $outputRoot ($name + '.stdout.txt')
    $stderr = Join-Path $outputRoot ($name + '.stderr.txt')
    $clock = [Diagnostics.Stopwatch]::StartNew()
    $p = Start-Process -FilePath $exe -ArgumentList $arguments -WorkingDirectory $copyRoot `
        -WindowStyle Hidden -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
    $timedOut = -not $p.WaitForExit($seconds * 1000)
    if ($timedOut) { $p.Kill($true); $p.WaitForExit() }
    $p.Refresh()
    $row = [ordered]@{ name=$name; pid=$p.Id; exitCode=$p.ExitCode; timedOut=$timedOut;
        elapsedMs=$clock.ElapsedMilliseconds; stdout=$stdout; stderr=$stderr }
    $runs.Add($row)
    $p.Dispose()
    if ($timedOut -or $row.exitCode -ne 0) { throw "Failed $name; inspect retained logs." }
}
try {
    foreach ($part in @('Lea.Engine','Lea.Image','Lea.Win','Lea.Win.Test','Lea.Tests','assets','LeaForge\test-pages')) {
        $directory = Join-Path $sourceRoot $part
        RejectLinks $directory
        $files = @(Get-ChildItem -LiteralPath $directory -Recurse -File -Force)
        foreach ($file in $files) {
            $relative = $file.FullName.Substring($sourceRoot.Length + 1)
            if ($relative -match '(?i)(^|\\)(bin|obj|\.git)(\\|$)') { continue }
            RejectLinks $file.FullName
            $hash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
            $destination = Join-Path $copyRoot $relative
            $null = New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($destination)) -Force
            Copy-Item -LiteralPath $file.FullName -Destination $destination
            if ((Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash -ne $hash) { throw 'Snapshot copy mismatch.' }
            $pins.Add(@{ path=$relative; sha256=$hash })
        }
    }
    $runtime = Join-Path $outputRoot 'runtime'
    $null = New-Item -ItemType Directory -Path $runtime
    $projects = @('Lea.Engine','Lea.Image','Lea.Win','Lea.Win.Test','Lea.Tests')
    if ($ReuseNativeFrom) {
        $priorRoot = [IO.Path]::GetFullPath($ReuseNativeFrom).TrimEnd('\')
        if (-not $priorRoot.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase)) { throw 'Reuse must remain in workspace.' }
        RejectLinks $priorRoot
        $receipt = Join-Path $priorRoot 'result.json'
        $prior = Get-Content -LiteralPath $receipt -Raw | ConvertFrom-Json
        if (-not $prior.completed -or -not $prior.originalInputsUnchanged -or $prior.source -ne $sourceRoot) {
            throw 'Reuse source boundary differs.'
        }
        if ($prior.sourceInputs.Count -ne $pins.Count) { throw 'Reuse input count differs.' }
        foreach ($pin in $pins) {
            $matches = @($prior.sourceInputs | Where-Object { $_.path -ceq $pin.path -and $_.sha256 -ceq $pin.sha256 })
            if ($matches.Count -ne 1) { throw 'Reuse source hash differs.' }
        }
        foreach ($name in @('Lea.Engine','Lea.Image','Lea.Win','Lea.Win.Test')) {
            $priorRuns = @($prior.runs | Where-Object { $_.name -ceq ('build-'+$name) -and $_.exitCode -eq 0 -and -not $_.timedOut })
            if ($priorRuns.Count -ne 1) { throw 'Reuse build was not successful.' }
        }
        foreach ($binary in $prior.binaries) {
            $name = [IO.Path]::GetFileName($binary.path)
            if ($name -notin @('Lea.Engine.dll','Lea.Image.exe','Lea.Win.dll','Lea.Win.Test.exe')) { continue }
            $file = Join-Path $priorRoot ('runtime\'+$name)
            RejectLinks $file
            if ((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash -cne $binary.sha256) { throw 'Reuse binary hash differs.' }
            Copy-Item -LiteralPath $file -Destination (Join-Path $runtime $name)
            if ((Get-FileHash -LiteralPath (Join-Path $runtime $name) -Algorithm SHA256).Hash -cne $binary.sha256) { throw 'Reuse copy hash differs.' }
        }
        if (@(Get-ChildItem -LiteralPath $runtime -File).Count -ne 4) { throw 'Reuse requires all four native binaries.' }
        $result.reusedReceipt = @{ path=$receipt; sha256=(Get-FileHash -LiteralPath $receipt -Algorithm SHA256).Hash }
        $projects = @('Lea.Tests')
    }
    foreach ($project in $projects) {
        $path = Join-Path $copyRoot ($project + '\' + $project + '.csproj')
        [xml]$xml = Get-Content -LiteralPath $path -Raw
        $ns = [Xml.XmlNamespaceManager]::new($xml.NameTable)
        $ns.AddNamespace('m', $xml.DocumentElement.NamespaceURI)
        $type = [string]$xml.SelectSingleNode('//m:OutputType', $ns).InnerText
        $extension = if ($type -eq 'Library') { '.dll' } else { '.exe' }
        $target = Join-Path $runtime ($project + $extension)
        $arguments = [Collections.Generic.List[string]]::new()
        foreach ($option in @('/nologo','/nostdlib+','/optimize+','/define:TRACE','/langversion:latest',
            ('/target:' + $type.ToLowerInvariant()), ('/out:"' + $target + '"'),
            ('/reference:"' + (Join-Path $references 'mscorlib.dll') + '"'),
            ('/reference:"' + (Join-Path $references 'System.Core.dll') + '"'))) { $arguments.Add($option) }
        foreach ($reference in $xml.SelectNodes('//m:Reference', $ns)) {
            $name = ([string]$reference.Include).Split(',')[0]
            $arguments.Add('/reference:"' + (Join-Path $references ($name + '.dll')) + '"')
        }
        foreach ($reference in $xml.SelectNodes('//m:ProjectReference', $ns)) {
            if ([string]$reference.ReferenceOutputAssembly -eq 'false') { continue }
            $arguments.Add('/reference:"' + (Join-Path $runtime ([string]$reference.Name + '.dll')) + '"')
        }
        foreach ($inputFile in $xml.SelectNodes('//m:Compile', $ns)) {
            $arguments.Add('"' + (Join-Path ([IO.Path]::GetDirectoryName($path)) ([string]$inputFile.Include)) + '"')
        }
        $icon = $xml.SelectSingleNode('//m:ApplicationIcon', $ns)
        if ($icon) { $arguments.Add('/win32icon:"' + (Join-Path ([IO.Path]::GetDirectoryName($path)) $icon.InnerText) + '"') }
        $response = Join-Path $outputRoot ($project + '.rsp')
        $arguments | Set-Content -LiteralPath $response -Encoding UTF8
        $hash = (Get-FileHash -LiteralPath $response -Algorithm SHA256).Hash
        RunOwned ('build-' + $project) $compiler @('-NoProfile','-File',('"'+$bridge+'"'),
            '-ResponseFile',('"'+$response+'"'),'-ExpectedResponseSha256',$hash) 120
    }
    $test = Join-Path $runtime 'Lea.Tests.exe'
    foreach ($mode in (@('blank','attributes','tokens','select') + $ExtraSmokeModes)) {
        RunOwned ('test-' + $mode) $test @(('"' + $copyRoot + '"'), ('--' + $mode)) 60
    }
    $result.passed = $true
} catch {
    $result.failure = $_.Exception.ToString()
} finally {
    try {
        foreach ($pin in $pins) {
            if ((Get-FileHash -LiteralPath (Join-Path $sourceRoot $pin.path) -Algorithm SHA256).Hash -ne $pin.sha256) {
                throw ('Original source changed: ' + $pin.path)
            }
        }
        $result.originalInputsUnchanged = $true
    } catch { $result.passed=$false; $result.failure = $_.Exception.ToString() }
    $result.binaries = @(Get-ChildItem -LiteralPath $outputRoot -Recurse -File -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match '\\runtime\\' -and $_.Extension -in @('.exe','.dll') } |
        ForEach-Object { @{ path=$_.FullName.Substring($outputRoot.Length+1); bytes=$_.Length;
            sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash } })
    $result.completed = $true
    $result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $outputRoot 'result.json') -Encoding UTF8
}
Write-Output (Join-Path $outputRoot 'result.json')
if (-not $result.passed) { throw $result.failure }
