param(
    [Alias('s')][string]$Source = 'L:\Firespider Internet',
    [Alias('o')][string]$Out = '',
    [Alias('ns')][switch]$NoScript,
    [Alias('r')][string]$Runtime = ''
)

$ErrorActionPreference = 'Stop'
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$sourceRoot = (Resolve-Path -LiteralPath $Source).Path.TrimEnd('\')
$stamp = Get-Date -Format 'yyyy-MM-dd_HH-mm-ss'
if ($Out -eq '') { $Out = Join-Path ([IO.Path]::GetTempPath()) "Lea-Firespider-146-$stamp" }
$stage = [IO.Path]::GetFullPath($Out).TrimEnd('\')
if (Test-Path -LiteralPath $stage) { throw "Candidate folder already exists: $stage" }
if ($stage.StartsWith($sourceRoot + '\', [StringComparison]::OrdinalIgnoreCase) -or $stage -eq $sourceRoot) {
    throw 'Candidate output must be separate from the live source folder.'
}
$projectName = 'Firespider Internet.vbproj'
$projectPath = Join-Path $sourceRoot $projectName
[xml]$project = Get-Content -LiteralPath $projectPath -Raw
$ns = New-Object Xml.XmlNamespaceManager($project.NameTable)
$ns.AddNamespace('m', $project.DocumentElement.NamespaceURI)
if ($Runtime -eq '') { $Runtime = Join-Path $repo 'Lea.Script.Browser\bin\Release\net462' }
$runtimeRoot = (Resolve-Path -LiteralPath $Runtime).Path.TrimEnd('\')
if (-not (Test-Path -LiteralPath $runtimeRoot -PathType Container)) { throw 'Runtime must be a folder.' }
if ($stage.StartsWith($runtimeRoot + '\', [StringComparison]::OrdinalIgnoreCase) -or $stage -eq $runtimeRoot) {
    throw 'Candidate output must be separate from the runtime input folder.'
}
$coreFiles = @('Lea.Engine.dll', 'Lea.Win.dll', 'Lea.Image.exe')
$obsolete = @('AxInterop.DIRECTORSHOCKWAVELib', 'AxInterop.ShockwaveFlashObjects',
    'Interop.DIRECTORSHOCKWAVELib', 'Interop.ShockwaveFlashObjects', 'Interop.SWDIRLib',
    'AxDIRECTORSHOCKWAVELib', 'AxShockwaveFlashObjects', 'DIRECTORSHOCKWAVELib', 'ShockwaveFlashObjects', 'SWDIRLib')
$scriptFiles = @('Lea.Script.Win.dll', 'Lea.Script.dll', 'Jint.dll', 'Acornima.dll',
    'System.Memory.dll', 'System.Buffers.dll', 'System.Numerics.Vectors.dll', 'System.Runtime.CompilerServices.Unsafe.dll')
$workerFiles = @('Lea.Script.Worker.exe', 'Lea.Script.Worker.exe.config', 'Jint.dll', 'Acornima.dll',
    'System.Memory.dll', 'System.Buffers.dll', 'System.Numerics.Vectors.dll', 'System.Runtime.CompilerServices.Unsafe.dll')
$runtimeNames = @($coreFiles)
if (-not $NoScript) {
    $runtimeNames += $scriptFiles
    $runtimeNames += @($workerFiles | ForEach-Object { "ScriptWorker/$_" })
    $runtimeNames += 'Lea.Script.Browser.exe.config'
}
$runtimeInputs = @(foreach ($name in $runtimeNames) {
    $path = Join-Path $runtimeRoot $name
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing runtime input: $name" }
    [ordered]@{ path=$name; bytes=(Get-Item -LiteralPath $path).Length; sha256=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash }
})
function Check-RuntimeFile([string]$root, $item) {
    $path = Join-Path $root $item.path
    if ((Get-Item -LiteralPath $path).Length -ne $item.bytes -or
        (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $item.sha256) {
        throw "Runtime input changed: $($item.path)"
    }
}
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe'
$reference = Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
if (-not (Test-Path -LiteralPath $compiler) -or -not (Test-Path -LiteralPath $reference)) {
    throw 'This candidate build requires the installed .NET Framework MSBuild and 4.8 reference assemblies.'
}
New-Item -ItemType Directory -Path $stage | Out-Null
$copied = @{}

function Copy-SourceFile([string]$path) {
    $absolute = [IO.Path]::GetFullPath($path)
    if (-not $absolute.StartsWith($sourceRoot + '\', [StringComparison]::OrdinalIgnoreCase)) { throw "Source item escapes project: $absolute" }
    if (-not (Test-Path -LiteralPath $absolute -PathType Leaf)) { throw "Missing project input: $absolute" }
    $relative = $absolute.Substring($sourceRoot.Length + 1)
    if ($copied.ContainsKey($relative)) { return }
    $target = Join-Path $stage $relative
    New-Item -ItemType Directory -Path (Split-Path $target) -Force | Out-Null
    Copy-Item -LiteralPath $absolute -Destination $target
    $hash = (Get-FileHash -LiteralPath $absolute -Algorithm SHA256).Hash
    if ($hash -ne (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash) { throw "Input changed while copying: $relative" }
    $copied[$relative] = [ordered]@{ path=$relative.Replace('\', '/'); bytes=(Get-Item -LiteralPath $target).Length; sha256=$hash }
    if ([IO.Path]::GetExtension($absolute) -eq '.resx') {
        [xml]$resource = Get-Content -LiteralPath $absolute -Raw
        foreach ($data in $resource.SelectNodes('/root/data')) {
            if ([string]$data.type -notlike 'System.Resources.ResXFileRef,*') { continue }
            $file = ([string]$data.value).Split(';')[0]
            Copy-SourceFile (Join-Path (Split-Path $absolute) $file)
        }
    }
}

Copy-SourceFile $projectPath
Copy-SourceFile (Join-Path $sourceRoot 'app.config')
foreach ($item in $project.SelectNodes('//m:Compile|//m:EmbeddedResource|//m:Content|//m:None|//m:BaseApplicationManifest', $ns)) {
    $path = Join-Path $sourceRoot ([string]$item.Include)
    if (Test-Path -LiteralPath $path -PathType Container) { continue }
    Copy-SourceFile $path
}
$removed = @()
$provided = @()
foreach ($item in @($project.SelectNodes('//m:Reference|//m:Import[@Include]|//m:PublishFile', $ns))) {
    $identity = ([string]$item.Include).Split(',')[0]
    if ($obsolete -notcontains $identity) { continue }
    foreach ($file in $copied.Values | Where-Object { $_.path.EndsWith('.vb') }) {
        if ([IO.File]::ReadAllText((Join-Path $stage $file.path)) -match '(?i)\b(AxDIRECTORSHOCKWAVELib|AxShockwaveFlashObjects|DIRECTORSHOCKWAVELib|ShockwaveFlashObjects|SWDIRLib)\b') {
            throw "Historic COM references are still used by $($file.path); do not remove them automatically."
        }
    }
    $removed += $identity
    $null = $item.ParentNode.RemoveChild($item)
}
foreach ($referenceItem in $project.SelectNodes('//m:Reference[not(m:HintPath)]/m:Private', $ns)) {
    $identity = ([string]$referenceItem.ParentNode.Include).Split(',')[0]
    if ($referenceItem.InnerText -eq 'True' -and (Test-Path -LiteralPath (Join-Path $reference "$identity.dll") -PathType Leaf)) {
        $provided += [string]$referenceItem.ParentNode.Include
        $referenceItem.InnerText = 'False'
    }
}
$framework = $project.SelectSingleNode('//m:TargetFrameworkVersion', $ns)
if ($null -eq $framework) { throw 'Host project has no target framework property.' }
$framework.InnerText = 'v4.8'
$scriptProperty = $project.SelectSingleNode('//m:LeaScriptEnabled', $ns)
if ($null -eq $scriptProperty) {
    $scriptProperty = $project.CreateElement('LeaScriptEnabled', $project.DocumentElement.NamespaceURI)
    $null = $project.SelectSingleNode('/m:Project/m:PropertyGroup', $ns).AppendChild($scriptProperty)
}
$scriptProperty.InnerText = if ($NoScript) { 'false' } else { 'true' }
$project.Save((Join-Path $stage $projectName))
foreach ($hint in $project.SelectNodes('//m:Reference/m:HintPath', $ns)) { Copy-SourceFile (Join-Path $sourceRoot $hint.InnerText) }
$lea = Join-Path $stage 'Lea'
New-Item -ItemType Directory -Path $lea -Force | Out-Null
foreach ($name in @('FirespiderLea.vb', 'FirespiderSplit.vb')) {
    Copy-Item -LiteralPath (Join-Path $repo "Lea.Firespider\$name") -Destination $stage
}
foreach ($name in @('FirespiderLea.targets', 'start.html')) {
    Copy-Item -LiteralPath (Join-Path $repo "Lea.Firespider\$name") -Destination $lea
}
foreach ($name in $coreFiles + $(if ($NoScript) { @() } else { $scriptFiles })) {
    Copy-Item -LiteralPath (Join-Path $runtimeRoot $name) -Destination $lea
}
if (-not $NoScript) {
    New-Item -ItemType Directory -Path (Join-Path $lea 'ScriptWorker') | Out-Null
    foreach ($name in $workerFiles) {
        Copy-Item -LiteralPath (Join-Path $runtimeRoot "ScriptWorker\$name") -Destination (Join-Path $lea 'ScriptWorker')
    }
}
$output = Join-Path $stage 'runtime'
$enabled = if ($NoScript) { 'false' } else { 'true' }
[xml]$config = Get-Content -LiteralPath (Join-Path $stage 'app.config') -Raw
$supported = $config.SelectSingleNode('/configuration/startup/supportedRuntime')
if ($null -eq $supported) { throw 'Candidate configuration has no supported runtime entry.' }
$supported.SetAttribute('sku', '.NETFramework,Version=v4.8')
if (-not $NoScript) {
    [xml]$bindings = Get-Content -LiteralPath (Join-Path $runtimeRoot 'Lea.Script.Browser.exe.config') -Raw
    $bindingNs = New-Object Xml.XmlNamespaceManager($bindings.NameTable)
    $bindingNs.AddNamespace('a', 'urn:schemas-microsoft-com:asm.v1')
    $configNs = New-Object Xml.XmlNamespaceManager($config.NameTable)
    $configNs.AddNamespace('a', 'urn:schemas-microsoft-com:asm.v1')
    $runtimeNode = $config.SelectSingleNode('/configuration/runtime')
    if ($null -eq $runtimeNode) { $runtimeNode = $config.CreateElement('runtime'); $null = $config.DocumentElement.AppendChild($runtimeNode) }
    $bindingNode = $runtimeNode.SelectSingleNode('a:assemblyBinding', $configNs)
    if ($null -eq $bindingNode) { $bindingNode = $config.CreateElement('assemblyBinding', 'urn:schemas-microsoft-com:asm.v1'); $null = $runtimeNode.AppendChild($bindingNode) }
    foreach ($dependency in $bindings.SelectNodes('/configuration/runtime/a:assemblyBinding/a:dependentAssembly', $bindingNs)) {
        $identity = $dependency.SelectSingleNode('a:assemblyIdentity', $bindingNs).GetAttribute('name')
        foreach ($existing in @($bindingNode.SelectNodes('a:dependentAssembly', $configNs))) {
            if ($existing.SelectSingleNode('a:assemblyIdentity', $configNs).GetAttribute('name') -eq $identity) { $null = $bindingNode.RemoveChild($existing) }
        }
        $null = $bindingNode.AppendChild($config.ImportNode($dependency, $true))
    }
}
$config.Save((Join-Path $stage 'app.config'))
foreach ($item in $runtimeInputs) {
    Check-RuntimeFile $runtimeRoot $item
    if ($item.path -ne 'Lea.Script.Browser.exe.config') { Check-RuntimeFile $lea $item }
}
# Override the copied project only. Its original live target and outputs are untouched.
& $compiler (Join-Path $stage $projectName) /nologo /tv:4.0 /t:Rebuild /verbosity:minimal "/p:Configuration=Release;TargetFrameworkVersion=v4.8;LeaScriptEnabled=$enabled;OutputPath=$output\;GenerateManifests=false;AutoGenerateBindingRedirects=true"
if ($LASTEXITCODE -ne 0) { throw "Candidate build failed; preserved inputs: $stage" }
foreach ($item in $runtimeInputs) {
    Check-RuntimeFile $runtimeRoot $item
    if ($item.path -ne 'Lea.Script.Browser.exe.config') {
        Check-RuntimeFile $lea $item
        Check-RuntimeFile $output $item
    }
}
$exe = Join-Path $output 'Firespider Internet 19.exe'
Copy-Item -LiteralPath (Join-Path $repo 'LeaForge\test-pages') -Destination (Join-Path $output 'test-pages') -Recurse
New-Item -ItemType Directory -Path (Join-Path $output 'LeaForge') | Out-Null
Copy-Item -LiteralPath (Join-Path $repo 'LeaForge\test-pages') -Destination (Join-Path $output 'LeaForge\test-pages') -Recurse
Copy-Item -LiteralPath (Join-Path $repo 'assets') -Destination $output -Recurse
if (-not $NoScript) {
    Copy-Item -LiteralPath (Join-Path $repo 'Lea.Script\licenses') -Destination $output -Recurse
    Copy-Item -LiteralPath (Join-Path $repo 'Lea.Script\README.md') -Destination (Join-Path $output 'Lea.Script.README.md')
}
$overlays = @(Get-ChildItem -LiteralPath $lea -File -Recurse; Get-Item -LiteralPath (Join-Path $stage 'FirespiderLea.vb'), (Join-Path $stage 'FirespiderSplit.vb'), (Join-Path $stage $projectName), (Join-Path $stage 'app.config')) |
    ForEach-Object { [ordered]@{ path=$_.FullName.Substring($stage.Length + 1).Replace('\', '/'); bytes=$_.Length; sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash } }
[ordered]@{ milestone=146; source=$sourceRoot; framework='net48'; optionalScript=(-not $NoScript); runtimeSource=$runtimeRoot; runtimeInputs=$runtimeInputs; removedUnusedCom=$removed; frameworkProvided=$provided; inputs=@($copied.Values | Sort-Object { $_.path }); overlays=@($overlays) } |
    ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $stage 'source-manifest.json') -Encoding UTF8
[pscustomobject]@{ Candidate=$stage; Runtime=$output; EXE=$exe; SHA256=(Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash; SourceFiles=$copied.Count; OptionalScript=(-not $NoScript) } | Format-List
