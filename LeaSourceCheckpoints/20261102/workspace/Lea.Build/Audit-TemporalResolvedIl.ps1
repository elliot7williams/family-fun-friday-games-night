$ErrorActionPreference = 'Stop'
$root = Join-Path $PSScriptRoot '../continuity-evidence/temporal-startup-v1'
$out = Join-Path $root 'resolved-il-v5.json'
if (Test-Path $out) { throw 'Preserve existing receipt' }
if ((Get-PSDrive C).Free -lt 512MB) { throw '512 MB working reserve required' }
$buildPath = Join-Path $root 'build-v1/result.json'
if ((Get-FileHash $buildPath).Hash -cne '7D82A029AA5977798CF8B323920D606E21DFE4D803243914378EC1CAD8761D29') { throw 'Build receipt changed' }
$build = Get-Content $buildPath -Raw | ConvertFrom-Json
if (!$build.passed -or !$build.inputsUnchanged) { throw 'Build incomplete' }
$control = @($build.runs | Where-Object name -ceq 'control')
$lazy = @($build.runs | Where-Object name -ceq 'lazy')
if ($control.Count -ne 1 -or $lazy.Count -ne 1) { throw 'Missing pair' }
$sources = @((Join-Path $root 'TemporalInstructionShape.cs'), (Join-Path $root 'TemporalResolvedIl.cs'))
$pins = @(@($build.runs.binary) + $sources + @($PSCommandPath) | ForEach-Object { Get-FileHash -LiteralPath $_ })
foreach ($row in $build.runs) { if ((Get-FileHash $row.binary).Hash -cne $row.sha256) { throw 'Binary changed' } }
$refs = @(Get-ChildItem (Join-Path $PSHOME 'ref') -Filter '*.dll' | ForEach-Object FullName)
Add-Type -Path $sources -ReferencedAssemblies $refs -CompilerOptions '/nowarn:1701'
$checks = [TemporalResolvedIl]::SelfTest($control[0].binary)
$result = [TemporalResolvedIl]::Compare($control[0].binary, $lazy[0].binary)
$expectedMethods = @(
    'method:self:Jint.Native.Temporal/DefaultTimeZoneProvider::CanonicalizeTimeZone:32:0:1:String(String)',
    'method:self:Jint.Native.Temporal/DefaultTimeZoneProvider::.cctor:0:0:0:Void()'
)
$dictionary = '18:assembly:System.Collections:8.0.0.0::0:B03F5F7F11D50A3A/System.Collections.Generic/Dictionary`2<String,String>'
$fieldPrefix = 'field:self:Jint.Native.Temporal/DefaultTimeZoneProvider::IanaCanonicalCasing:'
$field = @($result.fieldChanges)
$fieldExpected = $field.Count -eq 1 -and $field[0].control -ceq ($fieldPrefix + $dictionary) -and
    $field[0].lazy -ceq ($fieldPrefix + '18:assembly:System.Runtime:8.0.0.0::0:B03F5F7F11D50A3A/System/Lazy`1<' + $dictionary + '>') -and
    $field[0].controlAttributes -eq 49 -and $field[0].lazyAttributes -eq 49 -and
    $field[0].controlOffset -eq -1 -and $field[0].lazyOffset -eq -1
$scoped = $fieldExpected -and $result.resolvedInstructionChanges.Count -eq 2 -and
    @($result.resolvedInstructionChanges | Where-Object { $_ -cnotin $expectedMethods }).Count -eq 0 -and
    $result.headerLocalExceptionChanges.Count -eq 0 -and $result.methodMetadataChanges.Count -eq 0 -and $checks.Count -eq 9
foreach ($pin in $pins) { if ((Get-FileHash -LiteralPath $pin.Path).Hash -cne $pin.Hash) { throw 'Input changed during audit' } }
if ((Get-PSDrive C).Free -lt 512MB) { throw 'Reserve lost before receipt write' }
$json = [pscustomobject]@{ scopedIlComparisonPassed=$scoped; result=$result; checkerSelfTests=$checks; inputsUnchanged=$true; inputs=$pins } | ConvertTo-Json -Depth 7
[IO.File]::WriteAllText([IO.Path]::GetFullPath($out), $json, [Text.UTF8Encoding]::new($false))
$saved = Get-Content $out -Raw | ConvertFrom-Json
if (!$saved.inputsUnchanged -or $saved.checkerSelfTests.Count -ne 9 -or $saved.scopedIlComparisonPassed -ne $scoped) { throw 'Receipt readback failed' }
[pscustomobject]@{ scopedIlComparisonPassed=$scoped; result=$result; checkerSelfTests=$checks; receipt=(Get-FileHash $out).Hash } | ConvertTo-Json -Depth 7
if (!$scoped) { throw 'Scoped IL/field comparison failed; retain evidence' }
