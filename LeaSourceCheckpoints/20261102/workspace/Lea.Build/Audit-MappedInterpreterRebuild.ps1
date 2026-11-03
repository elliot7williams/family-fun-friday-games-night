$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/interpreter-mapped-build-v1'
$out=Join-Path $root 'rebuild-verification.json'
if(Test-Path -LiteralPath $out){throw 'Preserve verification'}
$buildPath=Join-Path $root 'result.json'
if((Get-FileHash -LiteralPath $buildPath).Hash -cne '958AB08056C7F0FE234C7C126F9DC4A77B9DBAC7D98E053A27ADED82E27CA37C'){throw 'Build receipt changed'}
$build=Get-Content -LiteralPath $buildPath -Raw|ConvertFrom-Json
if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.error -or $build.runs.Count -ne 1){throw 'Build incomplete'}
$run=$build.runs[0]
if(!$run.passed -or $run.exitCode -ne 0 -or $run.killRequested -or $run.timedOut -or !$run.exitObserved -or !$run.stdoutComplete -or !$run.stderrComplete){throw 'Compiler not clean'}
$old=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/build-v1/lazy/Jint.dll'
$new=Join-Path $root 'emit/Jint.dll'
$expected='13B3EF3BA07B3DE50572D07D1B485A1ACDEBE307BE935C6E6F5D5771C492E32F'
foreach($path in @($old,$new)){if((Get-FileHash -LiteralPath $path).Hash -cne $expected){throw 'Interpreter IL identity changed'}}
if([Convert]::ToBase64String([IO.File]::ReadAllBytes($old)) -cne [Convert]::ToBase64String([IO.File]::ReadAllBytes($new))){throw 'Direct byte comparison differs'}
$compile=Get-Content -LiteralPath (Join-Path $root 'emit/compile-result.json') -Raw|ConvertFrom-Json
if(!$compile.emitted -or $compile.sourceCount -ne 699 -or $compile.sha256 -cne $expected){throw 'Emit evidence differs'}
foreach($input in $build.inputs){if((Get-FileHash -LiteralPath $input.path).Hash -cne $input.sha256){throw 'Build driver input changed'}}
$result=@{archiveExtractedRebuildVerified=$true;directByteComparisonIdentical=$true;ilSha256=$expected;sourceFiles=698;emittedTrees=699;elapsedMs=$run.elapsedMs;nativeRebuilt=$false;runtimeExecuted=$false;releaseCertified=$false;scope='Exact IL rebuild from fresh archive extraction with pinned external references/compiler; native/parser/host delivery still incomplete'}
[IO.File]::WriteAllText($out,($result|ConvertTo-Json))
$result|ConvertTo-Json
(Get-FileHash -LiteralPath $out).Hash
