$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$proof=Join-Path $workspace 'continuity-evidence/ordered-flow-v6/result.json'
$p=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
if(!$p.passed -or !$p.inputsUnchanged -or $p.ownerAttention){throw 'V6 source proof incomplete'}
$target=Join-Path $PSScriptRoot 'Backup-OrderedFlowV6.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve V6 backup recipe'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Backup-OrderedFlowSource.generated.ps1'))
$text=$text.Replace('ordered-flow-v4','ordered-flow-v6').Replace('FD5A70395712EC00EDE31D3D36077C80E99647BAC3B2345445E9486392F3D054',(Get-FileHash -LiteralPath $proof).Hash)
$text=$text.Replace("'Build-OrderedFlowCandidate.v4.generated.ps1'","'Build-OrderedFlowCandidate.v6.generated.ps1','Prepare-OrderedFlowV5.ps1','Prepare-OrderedFlowV6.ps1','Prepare-OrderedFlowV5Regression.ps1','Prepare-OrderedFlowV6Backup.ps1'")
$text=$text.Replace("'Backup-OrderedFlowSource.generated.ps1'","'Backup-OrderedFlowV6.generated.ps1'")
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
