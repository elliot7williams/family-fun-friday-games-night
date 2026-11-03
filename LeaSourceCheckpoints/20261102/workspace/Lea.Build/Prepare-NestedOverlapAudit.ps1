$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$proof=Join-Path $workspace 'continuity-evidence/nested-overlap-regression-v1/result.json'
if((Get-FileHash -LiteralPath $proof).Hash -cne 'E60A49F48671E92B80279171E5FC274BB4D7CFF2020D185A86729B7A38F04B6E'){throw 'Full run receipt differs'}
$r=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
if(!$r.behaviorPassed -or !$r.fullSuiteFooter -or !$r.inputsUnchanged -or !$r.bytesUnchanged -or $r.ownerAttention -or !$r.nestedOverlapSetupInverseVerified){throw 'Full run incomplete'}
$source=Join-Path $PSScriptRoot 'Audit-LiteralFixtureAcls.generated.ps1'
if((Get-FileHash -LiteralPath $source).Hash -cne '6611C651FF2BA428639426D1BF88B517CA9A95E7B30C452E3FE70B054A823FD2'){throw 'Fixture audit template differs'}
$target=Join-Path $PSScriptRoot 'Audit-NestedOverlapAcls.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve ACL observation'}
$text=[IO.File]::ReadAllText($source)
$text=$text.Replace('literal-text-regression-v1','nested-overlap-regression-v1').Replace('5955288E6E8507594FDB52D0A6F4AB7A0C0A80D91EDC4BB1C1C91B71D3566CE5','E60A49F48671E92B80279171E5FC274BB4D7CFF2020D185A86729B7A38F04B6E').Replace('E1DBA967FE00875E4134CEFCD303601A43173486556A58D18E6117927AF2F906','C7B7A0A1DBEE94D3D4A9C0252F1C0DEAF4B3C393471D2D8F5D1A972AB6D326F0')
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
