$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-CurrentNativeScriptSuite.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve current native full script attempt'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-CurrentFlowScriptSuite.generated.ps1'))
$text=$text.Replace('current-flow-script-run-v1','current-native-script-run-v1').Replace('current-flow-script-suite-v1','current-native-script-suite-v1')
$text=$text.Replace('6FC8EE19097D2EA29FCC0FCFEE0F53A9C57611C706A22483801B91938FB47087','916925B96F63ECFE75526F6BCBF742326441FF840FFD137C3CCE9CC56DA6C7CB').Replace('A89C275C73976AB90CAFD2AB3DD761F92D1973E470BD4274E74577B5742945F3','4785FEFBCE687D8BDA182A8B660126F94A6B1AAA1686B117422385FDC47F8C75')
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
