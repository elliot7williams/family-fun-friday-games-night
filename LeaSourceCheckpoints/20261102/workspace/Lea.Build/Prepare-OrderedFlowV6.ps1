$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Build-OrderedFlowCandidate.v6.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve ordered V6 candidate'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Build-OrderedFlowCandidate.v5.generated.ps1'))
$text=$text.Replace('ordered-flow-v5','ordered-flow-v6')
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
