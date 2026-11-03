$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Build-OrderedFlowCandidate.v5.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve ordered V5 candidate'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Build-OrderedFlowCandidate.v4.generated.ps1'))
$text=$text.Replace('ordered-flow-v4','ordered-flow-v5').Replace('Ordered flow: checks=40; failures=0','Ordered flow: checks=44; failures=0')
$text=$text.Replace('continuity-evidence/named-references-v1/candidate/Lea.Engine.dll','continuity-evidence/ordered-flow-v4/candidate/Lea.Engine.dll').Replace('9208520EE6A067FF38D39368B2E50E6B099E5120C977A071E8D3E09733A3AF17','D0A8061162EF716A9B5F940AC80119BF84E98F6EFFF3821737AA80541262E7F6')
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
