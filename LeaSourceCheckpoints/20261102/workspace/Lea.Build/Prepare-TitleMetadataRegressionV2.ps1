$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-TitleMetadataRegressionV2.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve title count-contract run'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-OrderedFlowRegression.v4.generated.ps1'))
$text=$text.Replace('ordered-flow-regression-v4','title-metadata-regression-v2').Replace('continuity-evidence/decoder-owner-proof-v1/runtime/Lea.Win.dll','continuity-evidence/title-metadata-visuals-v1/runtime/Lea.Win.dll').Replace('12A9CAF4880BA5A377018BECEF25FBE27740B8455F02CA3E16F5CA7FB642DF9C','FF99C552ABD3AEFB2F6DD12ECF83A3B7EAF2A3FDD392571B286F9F804D2939EC')
$anchor="    `$overlay=Join-Path `$outRoot 'Smoke.cs';"
$extra=@'
    $titleProof=Join-Path $workspace 'continuity-evidence/title-metadata-visuals-v1/result.json';$null=Pin $titleProof '04CB8E02EAC3FDEF82D2006C7E6C504C3A8913467DB4926EDFA5701A6D2103D4'
    $title=Get-Content -LiteralPath $titleProof -Raw|ConvertFrom-Json
    if(!$title.passed -or !$title.inputsUnchanged -or !$title.candidateUnchanged -or $title.ownerAttention -or !$title.baselineRegressionReproduced -or !$title.viewBuild.titleOverlayInverseVerified){throw 'Title behavior proof incomplete'}
    $titleHelper=Join-Path $PSScriptRoot 'New-TitleMetadataSmokeSource.ps1';$null=Pin $titleHelper;. $titleHelper
    $updated=New-TitleMetadataSmokeSource $updated
    $result.titleCountOverlayInverseVerified=$true
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Title test overlay stage differs'}
$text=$text.Replace($anchor,$extra+"`r`n"+$anchor)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
