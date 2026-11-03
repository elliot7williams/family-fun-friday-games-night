$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-DecoderQueueFull.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve traced full-engine attempt'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-DecoderQueueNative.generated.ps1'))
$text=$text.Replace('decoder-queue-native-v1','decoder-queue-full-v1').Replace("'native-pictures'","'full-engine-traced'")
$old='@((''"''+$fixture+''"''),''--pics'') 180'
$new='@((''"''+$fixture+''"'')) 300'
if([regex]::Matches($text,[regex]::Escape($old)).Count -ne 1){throw 'Full diagnostic argument/deadline anchor differs'}
$text=$text.Replace($old,$new).Replace('nativeBehaviorPassed','fullBehaviorPassed')
$text=$text.Replace(".EndsWith('Lea picture limit smoke passed.',[StringComparison]::Ordinal)",".StartsWith('Lea smoke passed: bounded sticky positioning with compact k,',[StringComparison]::Ordinal)")
$text=$text.Replace('$result.fullSuitePassed=$false','$result.scriptingSuiteExecuted=$false')
$text=$text.Replace('Original picture-test sequence with queue phase diagnostics, not the full suite or reliability qualification','Full retained engine/view sequence with queue phase diagnostics; not scripting, host, or cold reliability qualification')
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
