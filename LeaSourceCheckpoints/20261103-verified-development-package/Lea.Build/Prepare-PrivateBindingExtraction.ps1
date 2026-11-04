$ErrorActionPreference='Stop'
$template=Join-Path $PSScriptRoot 'Test-DeterministicExtraction-v1.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrivateBindingExtraction-v1.generated.ps1'
$out='I:\LeaBuildStorage\private-binding-extraction-source-20261103-v1'
$package='I:\LeaBuildStorage\private-binding-firespider-package-20261103-v1'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve extraction source attempt'}
if((Get-FileHash $template).Hash -cne '0FE62B647EDF56BC3B01A5B730D666A113CFF38F753B074794E7203C6EFBB13E' -or (Get-FileHash (Join-Path $package 'result.json')).Hash -cne 'AF32F7320FB74F2CBAE418E5966196D93AEFFBE278A7A41E4B72BFC4F9762E7D'){throw 'Extraction projection inputs differ'}
$proof=Get-Content (Join-Path $package 'result.json') -Raw|ConvertFrom-Json
if(!$proof.verified -or !$proof.privateBindingPair -or $proof.entries -ne 402 -or $proof.sha256 -cne '2D684D53800618A2920E86B6051526DEB651D946EC6781A628F887CAD71B3D68' -or (Get-FileHash $proof.archive).Hash -cne $proof.sha256){throw 'Verified exact private package required'}
$pairs=@(
    @('continuity-evidence/deterministic-package-extraction-v1','continuity-evidence/private-binding-package-extraction-v1'),
    @('I:\LeaBuildStorage\deterministic-firespider-package-20261102-v1',$package),
    @('D6087D9CF774E9424916F632E13DD3DFF17EFA87232B8E44F23F969A13BAE5AE','AF32F7320FB74F2CBAE418E5966196D93AEFFBE278A7A41E4B72BFC4F9762E7D'),
    @('Firespider-Lea-deterministic-development-20261102.zip','Firespider-Lea-private-binding-development-20261103.zip'),
    @('73FDDCC2B2F4B1EC41C2C89691D57F91A89F56A845AE54C7A4CC0486F1E7A121','2D684D53800618A2920E86B6051526DEB651D946EC6781A628F887CAD71B3D68'),
    @('B1B290CA5E90B0F5DD2460BFA3A90DCB81EF8F0ADF5DA96599D437B36B3F416E','B1A707A23CEB1DEE7FBDE13102BE24FAE7369FC21FC43F0688D35522197B1F85'),
    @('Fresh deterministic ZIP extraction','Fresh private-binding ZIP extraction')
)
$original=[IO.File]::ReadAllText($template);$text=$original
foreach($pair in $pairs){if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1 -or $text.Contains($pair[1])){throw 'Unique extraction identity projection required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text
for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Extraction projection inverse differs'}
$tokens=$null;$parseErrors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$parseErrors)
if($parseErrors.Count){throw 'Generated extraction source invalid'}
New-Item -ItemType Directory -Path $out|Out-Null
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
@{verified=$true;source=$target;sha256=(Get-FileHash $target).Hash;templateSha256=(Get-FileHash $template).Hash;inverseVerified=$true;assertionsChanged=$false;settingsBudgetMs=30000;navigationBudgetMs=90000;actualExeBudgetMs=100000;retirementBudgetMs=5000;executed=$false;hostAdopted=$false;runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'source.json') -Encoding utf8
Get-FileHash (Join-Path $out 'source.json')
