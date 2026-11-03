$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-DecoderLaunchProof.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve launch proof'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-DecoderLaunch.generated.ps1'))
$text=$text.Replace('decoder-launch-v1','decoder-launch-proof-v1')
$anchor="    `$options.Add('`"'+`$copy+'`"');`$options.Add('/main:DecoderOwnerProof')"
$extra=@'
    $options.Add('"'+$copy+'"')
    $launchProof=Join-Path $workspace 'Lea.Tests/DecoderLaunchProof.cs';Hold $launchProof (Get-FileHash -LiteralPath $launchProof).Hash
    $copy=Join-Path $outRoot 'DecoderLaunchProof.cs';Copy-Item -LiteralPath $launchProof -Destination $copy;Hold $copy (Get-FileHash -LiteralPath $launchProof).Hash
    $options.Add('"'+$copy+'"');$options.Add('/main:DecoderLaunchProof')
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Launch proof source anchor differs'}
$text=$text.Replace($anchor,$extra)
$text=$text.Replace("`$lines[-1] -ceq 'Decoder owner: checks=7; failures=0; no native worker launched'", "(`$lines -ccontains 'Decoder owner: checks=7; failures=0; no native worker launched') -and `$lines[-1] -ceq 'Decoder launch: checks=11; failures=0; no native worker launched'")
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
