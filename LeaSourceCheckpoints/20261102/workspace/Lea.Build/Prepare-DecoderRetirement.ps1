$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-DecoderRetirement.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve prior retirement runner'}
$source=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-DecoderQueueTrace.generated.ps1'))
$source=$source.Replace('decoder-queue-trace-v1','decoder-retirement-v1')
$anchor="    `$options.Add('`"'+`$copy+'`"');`$options.Add('/main:DecoderQueueTraceProof')"
if([regex]::Matches($source,[regex]::Escape($anchor)).Count -ne 1){throw 'Retirement compile anchor differs'}
$extra=@'
    $options.Add('"'+$copy+'"')
    $retirement=Join-Path $workspace 'Lea.Tests/DecoderRetirementProof.cs';Hold $retirement (Get-FileHash -LiteralPath $retirement).Hash
    $copy=Join-Path $outRoot 'DecoderRetirementProof.cs';Copy-Item -LiteralPath $retirement -Destination $copy;Hold $copy (Get-FileHash -LiteralPath $retirement).Hash
    $options.Add('"'+$copy+'"');$options.Add('/main:DecoderRetirementProof')
'@
$source=$source.Replace($anchor,$extra)
$source=$source.Replace("`$lines[-1] -ceq 'Decoder queue trace: checks=32; failures=0; no native worker launched'", "(`$lines -ccontains 'Decoder queue trace: checks=32; failures=0; no native worker launched') -and `$lines[-1] -ceq 'Decoder retirement: checks=11; failures=0; no native worker launched'")
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($source,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$source,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
