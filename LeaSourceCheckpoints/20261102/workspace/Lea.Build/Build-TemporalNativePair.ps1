param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$modern=Join-Path $workspace 'continuity-evidence/modern-worker-v1'
$outRoot=Join-Path $root 'native-pair-v2'
if (!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected) { throw 'Retained NoExit TTY required' }
$id=[Security.Principal.WindowsIdentity]::GetCurrent()
if (!$id.User.Value.EndsWith('-1001') -or ([Security.Principal.WindowsPrincipal]::new($id)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) { throw 'Normal nonadmin owner required' }
if (Test-Path $outRoot) { throw 'Preserve existing native experiment' }
if (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue) { throw 'Fresh owner required' }
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if ((Get-FileHash $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF') { throw 'Runner changed' }
$t=$null;$e=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$t,[ref]$e)
foreach ($name in @('Space','NewOutputReader','PumpOutput','Run')) {
    $defs=@($ast.FindAll({param($n) $n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq $name},$false))
    if ($e.Count -or $defs.Count -ne 1) { throw 'Runner shape changed' }
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$path,[string]$expected) {
    $full=[IO.Path]::GetFullPath($path)
    $stream=[IO.File]::Open($full,'Open','Read','Read');$leases.Add($stream)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if (!$expected -or $hash -cne $expected) { throw ('Input changed: '+$full) }
    $inputs.Add(@{path=$full;sha256=$hash})
}
$result=[ordered]@{passed=$false;candidate=$outRoot;inputs=$inputs;runs=$runs;variants=[Collections.Generic.List[object]]::new();error=$null;workerExecuted=$false;productionChanged=$false;performanceClaim=$false}
Space
if ((Get-PSDrive C).Free -lt 768MB) { throw '768 MB initial reserve required' }
$null=New-Item -ItemType Directory $outRoot
try {
    Hold $PSCommandPath (Get-FileHash $PSCommandPath).Hash
    Copy-Item $PSCommandPath (Join-Path $outRoot 'build.ps1')
    $audit=Join-Path $root 'resolved-il-v5.json'
    Hold $audit '9E3E72DC317C7A127BFDB7C8CDDFC8860E4356CCD7ED1F17A93B4E22DF909C36'
    $proof=Get-Content $audit -Raw|ConvertFrom-Json
    if (!$proof.scopedIlComparisonPassed -or !$proof.inputsUnchanged) { throw 'Scoped comparison required' }
    $build=Join-Path $root 'build-v1/result.json'
    Hold $build '7D82A029AA5977798CF8B323920D606E21DFE4D803243914378EC1CAD8761D29'
    $pair=Get-Content $build -Raw|ConvertFrom-Json
    if (!$pair.passed -or !$pair.inputsUnchanged) { throw 'IL pair incomplete' }
    $package=Join-Path $modern 'crossgen2-package-v4/result.json'
    Hold $package '60D063F3D3F3F0C5E9036A4416C75F7590C701BAF84E15E964A0A1D53199B3EF'
    $tool=Get-Content $package -Raw|ConvertFrom-Json
    if (!$tool.passed -or $tool.version -cne '8.0.32' -or $tool.verifyExitCode -ne 0) { throw 'Compiler attestation incomplete' }
    $archivePath=Join-Path $modern 'crossgen2-package-v4/crossgen2.nupkg'
    Hold $archivePath '96E758A7D8A29BB89856CA81A9BA728861801E60C36EB1D73D631D866B9A94B5'
    $archive=[IO.Compression.ZipFile]::OpenRead($archivePath)
    try {
        foreach ($f in $tool.files) {
            $expected=$f.sha256
            if (!$expected) {
                if ([IO.Path]::GetFileName($f.path) -cne '[Content_Types].xml') { throw 'Unexpected unpinned package file' }
                $entry=$archive.GetEntry('[Content_Types].xml')
                if (!$entry -or $entry.Length -ne $f.bytes) { throw 'Package metadata entry mismatch' }
                $stream=$entry.Open()
                try { $expected=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) } finally { $stream.Dispose() }
                $result.recoveredPackageEntry=@{entry=$entry.FullName;sha256=$expected;archiveSha256=$tool.packageSha256;reason='Historical receipt null hash; verified against pinned archive entry'}
            }
            Hold $f.path $expected
        }
    } finally { $archive.Dispose() }
    $oldPath=Join-Path $modern 'r2r-build-v1/result.json'
    Hold $oldPath '027198FDA46C70558A0B05A4572000FDE1B82E8EDC39E7E6C34A554337409077'
    $old=Get-Content $oldPath -Raw|ConvertFrom-Json
    if (!$old.passed -or !$old.inputsUnchanged) { throw 'Original reference recipe incomplete' }
    $recipe=Join-Path $modern 'r2r-build-v1/Jint.dll.rsp'
    Hold $recipe '51DAD95FFE000EF204650E1EEFA57C59D24B301BD5FD02DD1C702A951EE05FC0'
    $helper=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/ParentResourceIlAudit.cs'
    Hold $helper 'B2438457F0CA2F9F54A9EB866335DF7EF23D50FB84F134F13B968C4FB6F557CC'
    Add-Type -Path $helper
    foreach ($name in @('control','lazy')) {
        Space
        $variant=@($pair.runs|Where-Object name -ceq $name)
        if ($variant.Count -ne 1) { throw 'Missing IL variant' }
        Hold $variant[0].binary $variant[0].sha256
        $folder=Join-Path $outRoot $name;$null=New-Item -ItemType Directory $folder
        $binary=Join-Path $folder 'Jint.dll';$arguments=[Collections.Generic.List[string]]::new();$first=$true;$outputs=0
        foreach ($line in [IO.File]::ReadAllLines($recipe)) {
            if ($first) { $arguments.Add('"'+$variant[0].binary+'"');$first=$false;continue }
            if ($line.StartsWith('-o:')) { $arguments.Add('-o:"'+$binary+'"');$outputs++;continue }
            if ($line.StartsWith('-r:')) {
                $path=$line.Substring(3).Trim('"');$hashes=@($old.inputs|Where-Object path -ceq $path|ForEach-Object sha256|Sort-Object -Unique)
                if ($hashes.Count -ne 1) { throw 'Missing reference provenance' }
                Hold $path $hashes[0]
            }
            $arguments.Add($line)
        }
        if ($outputs -ne 1) { throw 'Unexpected output count' }
        $arguments.Add('--opt-cross-module:System.Private.CoreLib');$arguments.Add('--opt-cross-module:System.Collections.Concurrent');$arguments.Add('--mapcsv')
        $rsp=Join-Path $folder 'native.rsp';[IO.File]::WriteAllLines($rsp,$arguments,[Text.UTF8Encoding]::new($false));Hold $rsp (Get-FileHash $rsp).Hash
        $compile=Run ('compile-'+$name) (Join-Path $modern 'crossgen2-package-v4/extracted/tools/crossgen2.exe') @(('"@'+$rsp+'"')) 120
        if (!$compile.passed) { throw ('Compiler failed: '+$name) }
        $comparison=[ParentResourceIlAudit]::Compare($variant[0].binary,$binary)
        $maps=@(Get-ChildItem $folder -Filter '*.csv' -File)
        if ($maps.Count -ne 2) { throw 'Expected two compilation maps' }
        $result.variants.Add(@{name=$name;input=$variant[0].binary;inputSha256=$variant[0].sha256;binary=$binary;sha256=(Get-FileHash $binary).Hash;bytes=(Get-Item $binary).Length;ilComparison=$comparison;maps=@($maps|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash}})})
    }
    $result.passed=$result.variants.Count -eq 2
} catch { $result.error=$_.Exception.ToString() }
finally {
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    if (!$result.inputsUnchanged -or $result.ownerAttention) { $result.passed=$false }
    foreach ($lease in $leases) { $lease.Dispose() }
    $result|ConvertTo-Json -Depth 10|Set-Content (Join-Path $outRoot 'result.json')
    $global:LeaTemporalNativeResult=$result
    [pscustomobject]$result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json -Compress
}
