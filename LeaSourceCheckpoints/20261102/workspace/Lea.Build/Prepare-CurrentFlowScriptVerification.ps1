$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$buildPath=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/current-flow-script-suite-v1/result.json'
$b=Get-Content -LiteralPath $buildPath -Raw|ConvertFrom-Json
if(!$b.passed -or !$b.inputsUnchanged -or $b.ownerAttention){throw 'Current suite build incomplete'}
$target=Join-Path $PSScriptRoot 'Verify-CurrentFlowScriptSuite.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve current suite binding verification'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Verify-NamedScriptSuite.generated.ps1'))
$text=$text.Replace('named-references-script-suite-v1','current-flow-script-suite-v1').Replace('87E291382607C8CACAA716D022D97A1A374B1A44EA2E91A4714AAD4C6CA94F09',(Get-FileHash -LiteralPath $buildPath).Hash)
$text=$text.Replace('9208520EE6A067FF38D39368B2E50E6B099E5120C977A071E8D3E09733A3AF17','52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC').Replace('E26135F8CE6BA76A76515D2874DFE219E172B212037CD567AD37042DA516289B','B2C1A05ABC9772857F69C9B6AE34C9E1CB69E51DFC35E8F03F60F70F4A523E6D')
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
