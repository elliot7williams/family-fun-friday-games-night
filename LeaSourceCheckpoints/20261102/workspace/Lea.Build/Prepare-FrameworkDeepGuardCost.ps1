$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-FrameworkDeepGuardCost.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve deep guard control'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-FrameworkGuardCost.generated.ps1'))
$text=$text.Replace('framework-guard-cost-v1','framework-deep-guard-cost-v1').Replace('FrameworkGuardCost.cs','FrameworkDeepGuardCost.cs').Replace('D4199497A770CCFEEE14639744A2625A5D265C2E2F9306B1C69AB15715B6D349',(Get-FileHash -LiteralPath (Join-Path $PSScriptRoot 'FrameworkDeepGuardCost.cs')).Hash)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
