$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$base=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/Test-SourceCrossModuleProbes.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '07558FC4ABF4F8A8E0136DACCA18104546382B644FA994CDEC54D53ECA3961B1'){throw 'Baseline probe builder changed'}
$target=Join-Path (Split-Path $base) 'Test-NormalTemporalProbes.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve normal-worker probes'}
$text=[IO.File]::ReadAllText($base)
$text=$text.Replace("`$outRoot=Join-Path `$PSScriptRoot 'source-cross-module-probes-v1'","`$outRoot=Join-Path `$workspace 'continuity-evidence/temporal-startup-v1/normal-component-probes-v1'")
$old='$from=Join-Path $variant.directory $name;$null=PinInput $from $file[0].sha256;$to=Join-Path $directory $name;Copy-Item -LiteralPath $from -Destination $to;$null=PinInput $to $file[0].sha256'
$new=@'
$from=Join-Path $variant.directory $name;$expected=$file[0].sha256
                if($name -ceq 'Jint.dll'){$from=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/native-pair-v2/lazy/Jint.dll';$expected='67F3E7D75EDADCBC0110FD6BB7AC26E56784D3650A83C2AE0AA23DD67EB03E6D'}
                $null=PinInput $from $expected;$to=Join-Path $directory $name;Copy-Item -LiteralPath $from -Destination $to;$null=PinInput $to $expected
'@
if($text.IndexOf($old) -lt 0 -or $text.IndexOf($old) -ne $text.LastIndexOf($old)){throw 'Copy anchor ambiguous'}
$text=$text.Replace($old,$new)
$anchor='    $priorPath=Join-Path $modern'
$proof=@'
    $nativeProof=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/native-pair-v2/literal-input-verification.json'
    $null=PinInput $nativeProof '98A773D0A4B40B2D116F8BAEA1B4CF1F20D5D9C6607C1FEB297EE4805C9428C8'
    $native=Get-Content -LiteralPath $nativeProof -Raw|ConvertFrom-Json
    if(!$native.passed){throw 'Native pair incomplete'}
'@
if(!$text.Contains($anchor)){throw 'Proof anchor missing'}
$text=$text.Replace($anchor,$proof+"`n"+$anchor)
$tokens=$null;$errors=$null;[void][Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Generated syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
(Get-FileHash -LiteralPath $target).Hash
