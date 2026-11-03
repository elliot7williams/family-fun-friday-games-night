$ErrorActionPreference='Stop'
$helper=Join-Path $PSScriptRoot 'Initialize-CurrentFlowScriptSuite.generated.ps1'
$builder=Join-Path $PSScriptRoot 'Build-CurrentFlowScriptSuite.generated.ps1'
if((Test-Path -LiteralPath $helper) -or (Test-Path -LiteralPath $builder)){throw 'Preserve current flow script preparation'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Initialize-NamedScriptSuite.generated.ps1'))
foreach($pair in @(
    @('Initialize-NamedScriptSuite','Initialize-CurrentFlowScriptSuite'),
    @('continuity-evidence/named-references-v1/candidate/Lea.Engine.dll','continuity-evidence/ordered-flow-v6/candidate/Lea.Engine.dll'),
    @('9208520EE6A067FF38D39368B2E50E6B099E5120C977A071E8D3E09733A3AF17','52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC'),
    @('continuity-evidence/permission-contract-v1/retry-view-v1/Lea.Win.dll','continuity-evidence/decoder-launch-proof-v2/runtime/Lea.Win.dll'),
    @('E26135F8CE6BA76A76515D2874DFE219E172B212037CD567AD37042DA516289B','B2C1A05ABC9772857F69C9B6AE34C9E1CB69E51DFC35E8F03F60F70F4A523E6D')
)){
    if($pair.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Current script binding anchor differs'}
    $text=$text.Replace($pair[0],$pair[1])
}
[IO.File]::WriteAllText($helper,$text,[Text.UTF8Encoding]::new($false))
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Build-NamedScriptSuite.generated.ps1'))
$text=$text.Replace('named-references-script-suite-v1','current-flow-script-suite-v1').Replace('Initialize-NamedScriptSuite','Initialize-CurrentFlowScriptSuite').Replace('7654B764BEFB3103FC2DB9A7BF76C79CADE7A36EB5FAE1E7640F22ADA9225ECA',(Get-FileHash -LiteralPath $helper).Hash)
$text=$text.Replace('Compile-only matched named-reference candidate suite; runtime validation separate','Compile-only matched current flow core/view suite; runtime validation separate')
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($builder,$text,[Text.UTF8Encoding]::new($false))
