$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-SplitHostDiagnosticV2.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve corrected split diagnostic'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-SplitHostDiagnostic.ps1'))
$text=$text.Replace('split-host-diagnostic-v1','split-host-diagnostic-v2')
$anchor='    $options=[Collections.Generic.List[string]]::new()'
$extra=@'
    $corlib=Join-Path (Split-Path $build.compiler.path) 'mscorlib.dll'
    Hold $corlib (Get-FileHash -LiteralPath $corlib).Hash
    $options.Add('/nostdlib+')
    $options.Add('/reference:"'+$corlib+'"')
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Framework reference anchor differs'}
$text=$text.Replace($anchor,$anchor+"`r`n"+$extra)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
