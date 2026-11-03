$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Backup-TitleMetadataContractsV4.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve title geometry source backup recipe'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Backup-TitleMetadataContractsV2.ps1'))
$text=$text.Replace('title-metadata-regression-v2','title-metadata-regression-v4').Replace('title-metadata-contracts-v2.zip','title-metadata-contracts-v4.zip')
$text=$text.Replace('title-metadata-source-v1.zip','title-metadata-contracts-v2.zip').Replace('9FA3619F328E4B26E5CBFCF929D3136CF7D63C444B2FB51A65E2EBA7623791F1','C266C849919310C1EF43BE9183C92204F39B18DBFDD2DF2B92AEE40538657FFA')
$text=$text.Replace("'Backup-TitleMetadataContractsV2.ps1'","'Backup-TitleMetadataContractsV2.ps1','New-TitleGeometrySmokeSource.ps1','Prepare-TitleMetadataRegressionV3.ps1','Test-TitleMetadataRegressionV3.generated.ps1','Prepare-TitleMetadataRegressionV4.ps1','Test-TitleMetadataRegressionV4.generated.ps1','Prepare-TitleMetadataContractsV4Backup.ps1','Backup-TitleMetadataContractsV4.generated.ps1'")
$anchor='$prior=Join-Path $workspace'
$extra=@'
$failed=Join-Path $workspace 'continuity-evidence/title-metadata-regression-v3'
foreach($file in Get-ChildItem -LiteralPath $failed -File){$members.Add(@{path=$file.FullName;entry='compile-failure-v3/'+$file.Name})}
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Failed compilation archive anchor differs'}
$text=$text.Replace($anchor,$extra+"`r`n"+$anchor)
$anchor='foreach($member in $members){$member.sha256=(Get-FileHash -LiteralPath $member.path).Hash}'
$extra=@'
foreach($member in $members){
    if($member.entry -ceq 'recipes/New-TitleGeometrySmokeSource.ps1'){
        $member.path=Join-Path $root 'New-TitleGeometrySmokeSource.input.ps1'
    }
}
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Exact V4 helper input archive anchor differs'}
$text=$text.Replace($anchor,$extra+"`r`n"+$anchor)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
