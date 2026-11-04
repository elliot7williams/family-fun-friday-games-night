$ErrorActionPreference='Stop'
$smoke=Join-Path $PSScriptRoot 'Test-PrivateBindingHostSmokes-v1.generated.ps1'
$campaign=Join-Path $PSScriptRoot 'Test-RelativeFontHostCampaign.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrivateBindingHostWorkflows-v1.generated.ps1'
$out='I:\LeaBuildStorage\private-binding-host-workflow-source-20261103-v1'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve existing workflow source'}
if((Get-FileHash $smoke).Hash -cne '0AC6D53BA9C051AA359805F8C5938D269E1F69960FEEA9CB3D8A26BA9BD24D7C' -or (Get-FileHash $campaign).Hash -cne '3635C9AE9EC0468FB8212987082BD9C394898F5AFA1A2956B4953EAC2F68B5B8'){throw 'Workflow templates differ'}
$text=[IO.File]::ReadAllText($smoke);$originalCampaign=[IO.File]::ReadAllText($campaign)
$text=$text.Replace('continuity-evidence/private-binding-host-smokes-v1','continuity-evidence/private-binding-host-workflows-v1')
$start=$text.IndexOf("    `$run=Run 'navigation'");$end=$text.IndexOf('    $startup=', $start)
if($start -lt 0 -or $end -le $start){throw 'Navigation block boundaries differ'}
$text=$text.Remove($start,$end-$start)
$start=$text.IndexOf('    $nl=');$end=$text.IndexOf('    $result.completed=', $start)
$caseStart=$originalCampaign.IndexOf('    foreach($case in');$caseEnd=$originalCampaign.IndexOf('    $result.hostStagesPassed=', $caseStart)
if($start -lt 0 -or $end -le $start -or $caseStart -lt 0 -or $caseEnd -le $caseStart){throw 'Original campaign boundaries differ'}
$block=$originalCampaign.Substring($caseStart,$caseEnd-$caseStart)
$text=$text.Substring(0,$start)+$block+$text.Substring($end)
$text=$text.Replace('$runs.Count -eq 3','$runs.Count -eq 15')
$text=$text.Replace('@($runs|Where-Object{!$_.passed}).Count','@($runs|Where-Object{!$_.passed -or ($_.ContainsKey(''exactFooterVerified'') -and !$_.exactFooterVerified)}).Count')
$text=$text.Replace('Exact private-binding host settings/navigation/actual clean EXE smokes on fresh NTFS; not full host campaign, clean-PC or release qualification','Exact private-binding host original 15 workflow cases on fresh NTFS; print-command/preview qualification pending; not clean-PC, reliability or release qualification')
if(!$text.Contains($block) -or !$text.Contains('$runs.Count -eq 15') -or !$text.Contains('!$_.exactFooterVerified')){throw 'Original workflow assertions missing'}
$tokens=$null;$parseErrors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$parseErrors)
if($parseErrors.Count){throw 'Generated workflow syntax invalid'}
New-Item -ItemType Directory -Path $out|Out-Null
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
@{verified=$true;source=$target;sha256=(Get-FileHash $target).Hash;smokeSha256=(Get-FileHash $smoke).Hash;originalCampaignSha256=(Get-FileHash $campaign).Hash;originalWorkflowBlockRetained=$true;requiredCases=15;settingsBudgetMs=30000;hostBudgetMs=90000;actualExeBudgetMs=100000;retirementBudgetMs=5000;executed=$false;hostAdopted=$false;runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'source.json') -Encoding utf8
Get-FileHash (Join-Path $out 'source.json')
