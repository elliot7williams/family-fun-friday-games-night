$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-OrderedFlowRegression.v2.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve ordered regression'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-LiteralTextRegression.generated.ps1'))
foreach($pair in @(
    @('literal-text-regression-v1','ordered-flow-regression-v2'),
    @('continuity-evidence/named-references-v1/candidate/Lea.Engine.dll','continuity-evidence/ordered-flow-v4/candidate/Lea.Engine.dll'),
    @('9208520EE6A067FF38D39368B2E50E6B099E5120C977A071E8D3E09733A3AF17','D0A8061162EF716A9B5F940AC80119BF84E98F6EFFF3821737AA80541262E7F6')
)){
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Ordered regression anchor differs'}
    $text=$text.Replace($pair[0],$pair[1])
}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
