$ErrorActionPreference='Stop'
$helper=Join-Path $PSScriptRoot 'Initialize-CurrentNativeScriptSuite.generated.ps1'
$builder=Join-Path $PSScriptRoot 'Build-CurrentNativeScriptSuite.generated.ps1'
if((Test-Path -LiteralPath $helper) -or (Test-Path -LiteralPath $builder)){throw 'Preserve matched native script preparation'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Initialize-CurrentFlowScriptSuite.generated.ps1'))
foreach($pair in @(
    @('Initialize-CurrentFlowScriptSuite','Initialize-CurrentNativeScriptSuite'),
    @('continuity-evidence/decoder-launch-proof-v2/runtime/Lea.Win.dll','continuity-evidence/current-hit-geometry-v3/Lea.Win.dll'),
    @('B2C1A05ABC9772857F69C9B6AE34C9E1CB69E51DFC35E8F03F60F70F4A523E6D','74956DA3FBBF28BC1DF281691FE7F8215C74CA67608AA61F6A6540E3BB0714AC'),
    @('Invoke-PublicCompiler.20261022.ps1','Invoke-PublicCompiler.20261026.ps1'),
    @('1A04EE9A191D522955C633F97A24C9C0921A5738F76D0DF36DB1C92A574EEA03','C3EBB3801FAFB80DA031AAB69FE4837EEB4E4EECD3907F9704F9D6186FA42F0A')
)){
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Script initialization binding anchor differs'}
    $text=$text.Replace($pair[0],$pair[1])
}
[IO.File]::WriteAllText($helper,$text,[Text.UTF8Encoding]::new($false))
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Build-CurrentFlowScriptSuite.generated.ps1'))
$text=$text.Replace('current-flow-script-suite-v1','current-native-script-suite-v1').Replace('Initialize-CurrentFlowScriptSuite','Initialize-CurrentNativeScriptSuite')
$old='25F0FF98FADC0B17E36C344DCCD724B379DB6886DF7034D1406665DD2EEEF0FF'
if([regex]::Matches($text,$old).Count -ne 1){throw 'Helper identity binding anchor differs'}
$text=$text.Replace($old,(Get-FileHash -LiteralPath $helper).Hash)
$text=$text.Replace('Invoke-PublicCompiler.20261022.ps1','Invoke-PublicCompiler.20261026.ps1').Replace('1A04EE9A191D522955C633F97A24C9C0921A5738F76D0DF36DB1C92A574EEA03','C3EBB3801FAFB80DA031AAB69FE4837EEB4E4EECD3907F9704F9D6186FA42F0A')
$text=$text.Replace('Compile-only matched current flow core/view suite; runtime validation separate','Compile-only matched current native core/view suite; runtime validation separate')
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($builder,$text,[Text.UTF8Encoding]::new($false))
