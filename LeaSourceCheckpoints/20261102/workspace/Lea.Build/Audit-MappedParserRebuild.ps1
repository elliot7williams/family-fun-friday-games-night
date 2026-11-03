$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/parser-mapped-build-v1'
$out=Join-Path $root 'rebuild-verification.json'
if(Test-Path -LiteralPath $out){throw 'Preserve parser verification'}
$path=Join-Path $root 'result.json'
if((Get-FileHash -LiteralPath $path).Hash -cne 'C68FFCBD0DC55FCB28C4992C9DDAD96FB2861999D46DEF31B8E0131ED5E2CD7F'){throw 'Parser build receipt changed'}
$build=Get-Content -LiteralPath $path -Raw|ConvertFrom-Json
if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.error -or $build.runs.Count -ne 1){throw 'Parser build incomplete'}
$run=$build.runs[0]
if(!$run.passed -or $run.exitCode -ne 0 -or $run.killRequested -or $run.timedOut -or !$run.exitObserved -or !$run.stdoutComplete -or !$run.stderrComplete){throw 'Compiler not retired cleanly'}
$old=Join-Path $workspace 'continuity-evidence/modern-worker-v1/parser-variant-v1/variant/Acornima.dll'
$new=Join-Path $root 'emit/Acornima.dll'
$expected='C3374BB12569649776AE440B81AF167E276858971CD9A6C34A30A6BB12698B7B'
foreach($file in @($old,$new)){if((Get-FileHash -LiteralPath $file).Hash -cne $expected){throw 'Parser IL changed'}}
if([Convert]::ToBase64String([IO.File]::ReadAllBytes($old)) -cne [Convert]::ToBase64String([IO.File]::ReadAllBytes($new))){throw 'Parser direct bytes differ'}
$compile=Get-Content -LiteralPath (Join-Path $root 'emit/compile-result.json') -Raw|ConvertFrom-Json
if(!$compile.emitted -or $compile.sourceCount -ne 211 -or $compile.resourceCount -ne 3 -or $compile.resourceStringCount -ne 146){throw 'Source/resource coverage differs'}
foreach($input in $build.inputs){if((Get-FileHash -LiteralPath $input.path).Hash -cne $input.sha256){throw 'Build driver input changed'}}
$result=@{archiveExtractedRebuildVerified=$true;directByteComparisonIdentical=$true;ilSha256=$expected;sourceFiles=210;emittedTrees=211;resourceSets=3;resourceStrings=146;elapsedMs=$run.elapsedMs;nativeRebuilt=$false;runtimeExecuted=$false;releaseCertified=$false}
[IO.File]::WriteAllText($out,($result|ConvertTo-Json))
$result|ConvertTo-Json
(Get-FileHash -LiteralPath $out).Hash
