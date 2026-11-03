$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$proof=Join-Path $workspace 'continuity-evidence/title-metadata-regression-v4/result.json'
$r=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
if(!$r.behaviorPassed -or !$r.fullSuiteFooter -or !$r.inputsUnchanged -or !$r.bytesUnchanged -or $r.ownerAttention -or !$r.titleCountOverlayInverseVerified -or !$r.titleGeometryOverlayInverseVerified){throw 'Title V4 behavior/source evidence incomplete'}
$rows=@($r.before|Where-Object {[IO.Path]::GetFileName($_.path) -ceq 'Lea.Tests.exe'})
if($rows.Count -ne 1 -or (Get-FileHash -LiteralPath $rows[0].path).Hash -cne $rows[0].sha256){throw 'Title V4 test binding ambiguous'}
$target=Join-Path $PSScriptRoot 'Audit-TitleMetadataV4Acls.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve title V4 ACL observation'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Audit-LiteralFixtureAcls.generated.ps1'))
$text=$text.Replace('literal-text-regression-v1','title-metadata-regression-v4').Replace('5955288E6E8507594FDB52D0A6F4AB7A0C0A80D91EDC4BB1C1C91B71D3566CE5',(Get-FileHash -LiteralPath $proof).Hash).Replace('E1DBA967FE00875E4134CEFCD303601A43173486556A58D18E6117927AF2F906',$rows[0].sha256)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
