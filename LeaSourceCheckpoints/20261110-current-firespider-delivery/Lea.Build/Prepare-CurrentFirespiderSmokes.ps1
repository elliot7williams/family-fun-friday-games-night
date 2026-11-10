param([Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$ManifestSha256,[switch]$Workflows)
$ErrorActionPreference='Stop'
$template=Join-Path $PSScriptRoot 'Test-PrivateBindingHostSmokes-v1.generated.ps1'
$out='I:\LeaBuildStorage\current-firespider-smoke-source-20261110-v1'
if($Workflows){$template=Join-Path $PSScriptRoot 'Test-PrivateBindingHostWorkflows-v1.generated.ps1';$out='I:\LeaBuildStorage\current-firespider-workflow-source-20261110-v1'}
$manifestPath='I:\LeaBuildStorage\current-firespider-runtime-20261110-v1\runtime-manifest.json'
if(Test-Path -LiteralPath $out){throw 'Preserve current smoke source'}
$templateHash=if($Workflows){'AE4F6F346E736C28D60291FEBF0EA578D2CEF331CAC2260294F5BEA37564BFE0'}else{'0AC6D53BA9C051AA359805F8C5938D269E1F69960FEEA9CB3D8A26BA9BD24D7C'}
if((Get-FileHash -LiteralPath $template).Hash -cne $templateHash -or (Get-FileHash -LiteralPath $manifestPath).Hash -cne $ManifestSha256){throw 'Smoke projection inputs differ'}
$manifest=Get-Content -LiteralPath $manifestPath -Raw|ConvertFrom-Json
if(!$manifest.verified -or !$manifest.currentSourceSdk -or $manifest.files.Count -ne 400 -or $manifest.buildOutputsBound -ne 22 -or $manifest.hostSha256 -cne '9513A0BD7C620589F4AB332CE19F155686C01104B1E8D99A5DECAAF1814F2586'){throw 'Current coherent host runtime required'}
$original=[IO.File]::ReadAllText($template);$text=$original
$pairs=@(
    @('continuity-evidence/private-binding-host-smokes-v1','continuity-evidence/current-source-host-smokes-v1'),
    @('I:\LeaBuildStorage\private-binding-firespider-runtime-20261103-v1\runtime-manifest.json',$manifestPath),
    @('694C89BB4FDF24936EF5424CAFC09C57BCDB50ED7EC21483D0DBDBD3A3E122FA',$ManifestSha256),
    @('B1A707A23CEB1DEE7FBDE13102BE24FAE7369FC21FC43F0688D35522197B1F85',$manifest.hostSha256),
    @('Exact private-binding host settings/navigation/actual clean EXE smokes','Exact current-source SDK host settings/navigation/actual clean EXE smokes')
)
if($Workflows){$pairs[0]=@('continuity-evidence/private-binding-host-workflows-v1','continuity-evidence/current-source-host-workflows-v1');$pairs[4]=@('Exact private-binding host original 15 workflow cases','Exact current-source SDK host original 15 workflow cases')}
foreach($pair in $pairs){if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1 -or $text.Contains($pair[1])){throw 'Unique host identity boundary required'};$text=$text.Replace($pair[0],$pair[1])}
$buildPath="'"+$PSScriptRoot.Replace("'","''")+"'"
if($text.Contains($buildPath)){throw 'Relocation text collision'}
$scriptRootUses=[regex]::Matches($text,[regex]::Escape('$PSScriptRoot')).Count
if($scriptRootUses -lt 1){throw 'Original helper paths missing'}
$text=$text.Replace('$PSScriptRoot',$buildPath)
$inverse=$text.Replace($buildPath,'$PSScriptRoot');for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Identity/path-only smoke inverse differs'}
foreach($token in @("'settings-path'","'navigation'","'exe-clean'",' 30',' 90',' 100','600MB',$(if($Workflows){'$runs.Count -eq 15'}else{'$runs.Count -eq 3'}))){if([regex]::Matches($text,[regex]::Escape($token)).Count -ne [regex]::Matches($original,[regex]::Escape($token)).Count){throw 'Original smoke controls changed'}}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors);if($errors.Count){throw 'Projected smoke syntax invalid'}
New-Item -ItemType Directory -Path $out|Out-Null
$target=Join-Path $out $(if($Workflows){'Test-CurrentFirespiderWorkflows.ps1'}else{'Test-CurrentFirespider.ps1'});[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
if([IO.File]::ReadAllText($target) -cne $text){throw 'Smoke source readback differs'}
@{verified=$true;workflows=[bool]$Workflows;requiredCases=$(if($Workflows){15}else{3});templateSha256=(Get-FileHash -LiteralPath $template).Hash;manifestSha256=$ManifestSha256;source=$target;sha256=(Get-FileHash -LiteralPath $target).Hash;replacements=$pairs;helperPathReplacements=$scriptRootUses;inverseVerified=$true;assertionsChanged=$false;settingsBudgetMs=30000;navigationBudgetMs=90000;actualExeBudgetMs=100000;retirementBudgetMs=5000;executed=$false;hostAdopted=$false}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'source.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'source.json')|Format-List Hash
