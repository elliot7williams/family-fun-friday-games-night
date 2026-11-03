$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$proof=Join-Path $workspace 'continuity-evidence/named-references-v1/result.json'
if((Get-FileHash -LiteralPath $proof).Hash -cne 'D2B7756C2DE57DA73F6F32C7097CAE52F3FF0000D9F4DD4B05C6FF6D6205FF70'){throw 'Named candidate receipt differs'}
$r=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.inputsUnchanged -or $r.ownerAttention -or !$r.numericRegressionPassed){throw 'Named candidate incomplete'}
$target=Join-Path $PSScriptRoot 'Test-NamedReferenceRegression.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve named regression'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-NumericReferenceRegression.generated.ps1'))
foreach($pair in @(
    @('numeric-references-regression-v1','named-references-regression-v1'),
    @('continuity-evidence/numeric-references-v3/candidate/Lea.Engine.dll','continuity-evidence/named-references-v1/candidate/Lea.Engine.dll'),
    @('515121C456FEE03257695BB34BA7302CF11AF0FF868186A14FBB5E9CADF9DFC1','9208520EE6A067FF38D39368B2E50E6B099E5120C977A071E8D3E09733A3AF17')
)){
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Named regression anchor differs'}
    $text=$text.Replace($pair[0],$pair[1])
}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
