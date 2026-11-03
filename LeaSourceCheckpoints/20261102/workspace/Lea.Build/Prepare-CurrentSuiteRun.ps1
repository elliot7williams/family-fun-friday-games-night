$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Test-RelocatedSuitePhase.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '462240FF542BF65C6F700D8DB06CEA9DEE4D92E71A84308DE0FD6AFCE7473A87'){throw 'Original runner changed'}
$target=Join-Path $PSScriptRoot 'Test-CurrentTemporalSuite.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve current suite runner'}
$text=[IO.File]::ReadAllText($base)
foreach($pair in @(
    @("'suite-phase-run-v1'","'current-lazy-run-v1'"),
    @('suite-phase-relocated-v2/result.json','current-lazy-suite-v1/result.json'),
    @('1DF0860AAEF98A61FDBE997A31889228061A2E30D02E9D705C7F9503CA352A18','8D446126AB56CF9656A9A2BEF9DC6FB4BDDAAB3D597765F7FB7DCC84000FD5CF'),
    @('suite-phase-relocated-v2/relocation-verification.json','current-lazy-suite-v1/binding-verification.json'),
    @('30E76D099D28F8520DA4AC1FAE074EAE7D7DDF8D3386C0017951A3522AEAF831','5E21C36421A5C1AD9217A46034F6FCF191A60854B8090AFF53BC657D6243EA87'),
    @('Instrumented relocated baseline full-suite diagnostic; not current lazy release gate','Instrumented exact normal worker and lazy interpreter current full-suite attempt; not release certification')
)){
    if(!$text.Contains($pair[0])){throw ('Missing runner anchor: '+$pair[0])}
    $text=$text.Replace($pair[0],$pair[1])
}
$t=$null;$e=$null;[void][Management.Automation.Language.Parser]::ParseInput($text,[ref]$t,[ref]$e);if($e.Count){throw 'Runner syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
(Get-FileHash -LiteralPath $target).Hash
