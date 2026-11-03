param([ValidateRange(1,9999)][int]$Version=1)
$ErrorActionPreference='Stop'
$smoke=Join-Path $PSScriptRoot 'Test-DeterministicHostSmokes-v2.generated.ps1'
$campaign=Join-Path $PSScriptRoot 'Test-RelativeFontHostCampaign.generated.ps1'
if((Get-FileHash $smoke).Hash -cne 'D7E21BDABCBD872FD98D8983BA14B630783F207B7EFB7161C67AD1413E147825' -or (Get-FileHash $campaign).Hash -cne '3635C9AE9EC0468FB8212987082BD9C394898F5AFA1A2956B4953EAC2F68B5B8'){throw 'Pinned workflow templates differ'}
$to=Join-Path $PSScriptRoot "Test-DeterministicHostWorkflows-v$Version.generated.ps1"
if(Test-Path $to){throw 'Preserve generated workflow source'}
$text=[IO.File]::ReadAllText($smoke)
$old=[IO.File]::ReadAllText($campaign)
$text=$text.Replace('continuity-evidence/deterministic-host-smokes-v2',"continuity-evidence/deterministic-host-workflows-v$Version")
$start=$text.IndexOf("    `$run=Run 'navigation'")
$end=$text.IndexOf('    $startup=', $start)
if($start -lt 0 -or $end -le $start){throw 'Navigation block boundary differs'}
$text=$text.Remove($start,$end-$start)
$start=$text.IndexOf('    $nl=')
$end=$text.IndexOf('    $result.completed=', $start)
$caseStart=$old.IndexOf('    foreach($case in')
$caseEnd=$old.IndexOf('    $result.hostStagesPassed=', $caseStart)
if($start -lt 0 -or $end -le $start -or $caseStart -lt 0 -or $caseEnd -le $caseStart){throw 'Original workflow boundaries differ'}
$block=$old.Substring($caseStart,$caseEnd-$caseStart)
$text=$text.Substring(0,$start)+$block+$text.Substring($end)
$text=$text.Replace('$runs.Count -eq 3','$runs.Count -eq 15')
$text=$text.Replace('@($runs|Where-Object{!$_.passed}).Count','@($runs|Where-Object{!$_.passed -or ($_.ContainsKey(''exactFooterVerified'') -and !$_.exactFooterVerified)}).Count')
$text=$text.Replace('Exact deterministic host settings/navigation/actual clean EXE smokes on fresh NTFS; not full host campaign, clean-PC or release qualification','Exact deterministic host original 15 workflow cases on fresh NTFS; no transferred prerequisite passes, print-command/preview qualification pending; not clean-PC, reliability or release qualification')
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Workflow syntax invalid'}
if(!$text.Contains($block) -or !$text.Contains('$runs.Count -eq 15') -or !$text.Contains('!$_.exactFooterVerified')){throw 'Workflow assertions not retained'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
