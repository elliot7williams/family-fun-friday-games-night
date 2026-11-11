param([switch]$GuardedSdk,[string]$PackageSha256)
$ErrorActionPreference='Stop'
$template=Join-Path $PSScriptRoot 'Test-PrivateBindingExtraction-v1.generated.ps1'
$out='I:\LeaBuildStorage\current-firespider-extraction-source-20261110-v1'
$base='I:\LeaBuildStorage\current-firespider-package-20261110-v1'
$expectedPackage='81CBAC6F685EC65DF45DF069C99F03FC263340683CB94F5908C5D1E9C0CE7482'
$hostHash='9513A0BD7C620589F4AB332CE19F155686C01104B1E8D99A5DECAAF1814F2586'
$archiveName='Firespider-Lea-current-source-development-20261110.zip'
if($GuardedSdk){if($PackageSha256 -cnotmatch '^[A-F0-9]{64}$'){throw 'Exact guarded archive hash required'};$out='I:\LeaBuildStorage\guarded-sdk-host-extraction-source-20261110-v1';$base='I:\LeaBuildStorage\guarded-sdk-host-package-20261110-v1';$expectedPackage=$PackageSha256;$hostHash='1E390343125011CAA3F78582E91FAFC036E07A31F7E81B4B727ED8503FC39606';$archiveName='Firespider-Lea-guarded-development-20261110.zip'}
if(Test-Path -LiteralPath $out){throw 'Preserve extraction preparation'}
if((Get-FileHash -LiteralPath $template).Hash -cne '3DC6D7BF960039A18206CDC504046164C77C416AC373910E690A187C6A49DF46'){throw 'Extraction template differs'}
$proofPath=Join-Path $base 'result.json'
$proof=Get-Content -LiteralPath $proofPath -Raw|ConvertFrom-Json
if(!$proof.verified -or $proof.entries -ne 402 -or $proof.sha256 -cne $expectedPackage -or (Get-FileHash -LiteralPath $proof.archive).Hash -cne $proof.sha256){throw 'Exact current package required'}
$pairs=@(
 @('continuity-evidence/private-binding-package-extraction-v1','continuity-evidence/current-source-package-extraction-v1'),
 @('I:\LeaBuildStorage\private-binding-firespider-package-20261103-v1',$base),
 @('AF32F7320FB74F2CBAE418E5966196D93AEFFBE278A7A41E4B72BFC4F9762E7D',(Get-FileHash -LiteralPath $proofPath).Hash),
 @('Firespider-Lea-private-binding-development-20261103.zip',$archiveName),
 @('2D684D53800618A2920E86B6051526DEB651D946EC6781A628F887CAD71B3D68',$proof.sha256),
 @('B1A707A23CEB1DEE7FBDE13102BE24FAE7369FC21FC43F0688D35522197B1F85',$hostHash),
 @('Fresh private-binding ZIP extraction','Fresh current-source ZIP extraction')
)
if($GuardedSdk){$pairs[0][1]='continuity-evidence/guarded-sdk-package-extraction-v1';$pairs[6][1]='Fresh guarded SDK ZIP extraction'}
$original=[IO.File]::ReadAllText($template);$text=$original
foreach($pair in $pairs){if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1 -or $text.Contains($pair[1])){throw 'Unique extraction identity required'};$text=$text.Replace($pair[0],$pair[1])}
$literal="'"+$PSScriptRoot.Replace("'","''")+"'"
$text=$text.Replace('$PSScriptRoot',$literal);$inverse=$text.Replace($literal,'$PSScriptRoot')
for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Extraction assertions changed'}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Extraction syntax invalid'}
New-Item -ItemType Directory -Path $out|Out-Null
$target=Join-Path $out 'Test-CurrentFirespiderExtraction.ps1'
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
if([IO.File]::ReadAllText($target) -cne $text){throw 'Source readback differs'}
@{verified=$true;source=$target;sha256=(Get-FileHash -LiteralPath $target).Hash;templateSha256=(Get-FileHash -LiteralPath $template).Hash;replacements=$pairs;inverseVerified=$true;assertionsChanged=$false;settingsBudgetMs=30000;navigationBudgetMs=90000;actualExeBudgetMs=100000;retirementBudgetMs=5000;executed=$false;hostAdopted=$false}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'source.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'source.json')
