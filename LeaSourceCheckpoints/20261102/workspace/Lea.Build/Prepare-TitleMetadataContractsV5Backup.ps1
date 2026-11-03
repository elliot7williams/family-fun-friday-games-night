$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Backup-TitleMetadataContractsV5.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve authored field source archive recipe'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Backup-TitleMetadataContractsV2.ps1'))
$text=$text.Replace('title-metadata-regression-v2','title-metadata-regression-v5').Replace('title-metadata-contracts-v2.zip','title-metadata-contracts-v5.zip')
$text=$text.Replace('title-metadata-source-v1.zip','title-metadata-contracts-v4.zip').Replace('9FA3619F328E4B26E5CBFCF929D3136CF7D63C444B2FB51A65E2EBA7623791F1','595950A5B5EA22C5B9B21A6AAD7E24A55B5B41CB27FAD4874B4CD313DA201D83')
$text=$text.Replace("'Backup-TitleMetadataContractsV2.ps1'","'Backup-TitleMetadataContractsV2.ps1','New-TitleGeometrySmokeSource.ps1','Prepare-TitleMetadataRegressionV3.ps1','Prepare-TitleMetadataRegressionV4.ps1','Prepare-TitleMetadataRegressionV5.ps1','Test-TitleMetadataRegressionV5.generated.ps1','Prepare-TitleMetadataV5AclAudit.ps1','Prepare-TitleMetadataContractsV5Backup.ps1','Backup-TitleMetadataContractsV5.generated.ps1'")
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
