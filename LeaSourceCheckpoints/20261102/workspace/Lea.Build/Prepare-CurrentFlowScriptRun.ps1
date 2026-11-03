$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/current-flow-script-suite-v1'
$buildPath=Join-Path $root 'result.json';$bindingPath=Join-Path $root 'binding-verification.json'
$build=Get-Content -LiteralPath $buildPath -Raw|ConvertFrom-Json
$binding=Get-Content -LiteralPath $bindingPath -Raw|ConvertFrom-Json
if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or !$binding.passed){throw 'Current matched suite proof incomplete'}
$target=Join-Path $PSScriptRoot 'Test-CurrentFlowScriptSuite.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve current flow full suite attempt'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-NamedScriptSuite.generated.ps1'))
$text=$text.Replace('named-references-script-run-v1','current-flow-script-run-v1').Replace('named-references-script-suite-v1','current-flow-script-suite-v1')
$text=$text.Replace('87E291382607C8CACAA716D022D97A1A374B1A44EA2E91A4714AAD4C6CA94F09',(Get-FileHash -LiteralPath $buildPath).Hash).Replace('895FF5CEA53490CBD7ECF20081CEF4B5EBD134F2FF363194B32420BEEBF45336',(Get-FileHash -LiteralPath $bindingPath).Hash)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
