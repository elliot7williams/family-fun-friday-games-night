$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-DecoderOwnerProof.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve owner proof runner'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-DecoderOwner.generated.ps1'))
$text=$text.Replace('decoder-owner-v1','decoder-owner-proof-v1')
$anchor="    `$options.Add('`"'+`$copy+'`"');`$options.Add('/main:DecoderRetirementProof')"
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Owner proof compile anchor differs'}
$extra=@'
    $options.Add('"'+$copy+'"')
    $ownerProof=Join-Path $workspace 'Lea.Tests/DecoderOwnerProof.cs';Hold $ownerProof (Get-FileHash -LiteralPath $ownerProof).Hash
    $copy=Join-Path $outRoot 'DecoderOwnerProof.cs';Copy-Item -LiteralPath $ownerProof -Destination $copy;Hold $copy (Get-FileHash -LiteralPath $ownerProof).Hash
    $options.Add('"'+$copy+'"');$options.Add('/main:DecoderOwnerProof')
'@
$text=$text.Replace($anchor,$extra)
$text=$text.Replace("`$lines[-1] -ceq 'Decoder retirement: checks=11; failures=0; no native worker launched'", "(`$lines -ccontains 'Decoder retirement: checks=11; failures=0; no native worker launched') -and `$lines[-1] -ceq 'Decoder owner: checks=7; failures=0; no native worker launched'")
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
