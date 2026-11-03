$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Prepare-TitleMetadataV5AclAudit.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve authored field ACL audit preparation'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Prepare-TitleMetadataV4AclAudit.ps1'))
$text=$text.Replace('title-metadata-regression-v4','title-metadata-regression-v5').Replace('Audit-TitleMetadataV4Acls.generated.ps1','Audit-TitleMetadataV5Acls.generated.ps1')
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
