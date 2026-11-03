$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$receipt=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/current-native-script-suite-v1/result.json'
if((Get-FileHash -LiteralPath $receipt).Hash -cne '916925B96F63ECFE75526F6BCBF742326441FF840FFD137C3CCE9CC56DA6C7CB'){throw 'Matched build receipt differs'}
$target=Join-Path $PSScriptRoot 'Verify-CurrentNativeScriptSuite.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve matched binding verification'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Verify-CurrentFlowScriptSuite.generated.ps1'))
$text=$text.Replace('current-flow-script-suite-v1','current-native-script-suite-v1').Replace('6FC8EE19097D2EA29FCC0FCFEE0F53A9C57611C706A22483801B91938FB47087','916925B96F63ECFE75526F6BCBF742326441FF840FFD137C3CCE9CC56DA6C7CB').Replace('B2C1A05ABC9772857F69C9B6AE34C9E1CB69E51DFC35E8F03F60F70F4A523E6D','74956DA3FBBF28BC1DF281691FE7F8215C74CA67608AA61F6A6540E3BB0714AC')
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
