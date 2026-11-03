$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-NestedFlowVisuals.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve nested flow visual attempt'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-OrderedFlowVisuals.v2.generated.ps1'))
$text=$text.Replace('ordered-flow-visuals-v2','nested-flow-visuals-v1').Replace('NamedVisuals','NestedFlowVisuals')
$text=$text.Replace('continuity-evidence/ordered-flow-v4/candidate/Lea.Engine.dll','continuity-evidence/ordered-flow-v6/candidate/Lea.Engine.dll').Replace('D0A8061162EF716A9B5F940AC80119BF84E98F6EFFF3821737AA80541262E7F6','52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC')
$text=$text.Replace('continuity-evidence/named-reference-visuals-v4/runtime/Lea.Win.dll','continuity-evidence/decoder-owner-proof-v1/runtime/Lea.Win.dll').Replace('9D9C7A3937B59206514D5631164A442744D9F5728CC43952AD643A86A5EB4867','12A9CAF4880BA5A377018BECEF25FBE27740B8455F02CA3E16F5CA7FB642DF9C')
$text=$text.Replace('LeaForge/test-pages/named-references.html','continuity-evidence/ordered-flow-regression-v3/fixtures/LeaForge/test-pages/nested-flow.html').Replace('named-references.html','nested-flow.html')
$text=$text.Replace('Hold $fixture (Get-FileHash -LiteralPath $fixture).Hash',"Hold `$fixture '483F36D5FCE61EF0C0AC8C42B300E46111B04AD153A4BB1CDAA0D6237322719F'")
$start=$text.IndexOf("    `$baseline=Join-Path `$outRoot 'baseline'")
$end=$text.IndexOf('    $result.before=',$start)
if($start -lt 0 -or $end -le $start){throw 'Nested baseline span differs'}
$text=$text.Remove($start,$end-$start)
$text=$text.Replace("@('desktop-top','narrow-top','desktop','narrow','narrow-edited','desktop-edited')","@('nested-720','nested-320')").Replace('(980|420) 720','(720|320) 900').Replace('Named visuals: checks=45; failures=0','Nested flow visuals: checks=36; failures=0')
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
