$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Test-CurrentTemporalSuite.generated.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '19346102FF0C9B256DAE4D37AE30B4E776161C9F69CF883F44C5B8EE39489977'){throw 'V1 runner changed'}
$target=Join-Path $PSScriptRoot 'Test-CurrentTemporalSuiteV2.generated.ps1'
if(Test-Path $target){throw 'Preserve V2 run'}
$text=[IO.File]::ReadAllText($base)
foreach($pair in @(
    @('current-lazy-run-v1','current-lazy-run-v2'),
    @('current-lazy-suite-v1','current-lazy-suite-v2'),
    @('8D446126AB56CF9656A9A2BEF9DC6FB4BDDAAB3D597765F7FB7DCC84000FD5CF','0D71CCA8558E48610A8B90C8A330D6D0EC1B7338E1174086E10B9AE5EEF395BA'),
    @('5E21C36421A5C1AD9217A46034F6FCF191A60854B8090AFF53BC657D6243EA87','8B25BF778552E8D2CC4A721A4DC8CD807D5A2E266107E61871E9A3972B69542B')
)){if(!$text.Contains($pair[0])){throw 'V2 run anchor missing'};$text=$text.Replace($pair[0],$pair[1])}
$t=$null;$e=$null;[void][Management.Automation.Language.Parser]::ParseInput($text,[ref]$t,[ref]$e);if($e.Count){throw 'V2 run syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
(Get-FileHash -LiteralPath $target).Hash
