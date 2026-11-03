$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-LiteralTextRegression.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve literal text regression'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-NamedReferenceRegression.generated.ps1'))
foreach($pair in @(
    @('named-references-regression-v1','literal-text-regression-v1'),
    @('continuity-evidence/permission-contract-v1/retry-view-v1/Lea.Win.dll','continuity-evidence/named-reference-visuals-v4/runtime/Lea.Win.dll'),
    @('E26135F8CE6BA76A76515D2874DFE219E172B212037CD567AD37042DA516289B','9D9C7A3937B59206514D5631164A442744D9F5728CC43952AD643A86A5EB4867')
)){
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Literal regression anchor differs'}
    $text=$text.Replace($pair[0],$pair[1])
}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
