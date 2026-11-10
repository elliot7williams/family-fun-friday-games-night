param([Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExtractionSha256)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\current-firespider-delivery-audit-20261110-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve delivery audit'}
function Pin([string]$Path,[string]$Hash){if((Get-FileHash -LiteralPath $Path).Hash -cne $Hash){throw ('Delivery artifact differs: '+$Path)}}
$printPath=Join-Path $root 'continuity-evidence/current-source-host-print-v1/result.json'
$extractPath=Join-Path $root 'continuity-evidence/current-source-package-extraction-v1/result.json'
Pin $printPath '5C8963D350EB55A7D9B71303949699AAF626C07E95DF35F11F3451A5802EAFB2'
Pin $extractPath $ExtractionSha256
$p=Get-Content -LiteralPath $printPath -Raw|ConvertFrom-Json
$e=Get-Content -LiteralPath $extractPath -Raw|ConvertFrom-Json
$budgets=@{'print-command-preview'=90000;'settings-path'=30000;'navigation'=90000;'exe-clean'=100000}
$seen=[Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach($r in @($p,$e)){
 if(!$r.passed -or !$r.completed -or !$r.inputsUnchanged -or !$r.decoderPolicyAccepted -or $r.ownerAttention -or $r.byteChanges.Count -or $r.aclChanges.Count){throw 'Delivery test boundary incomplete'}
 foreach($run in $r.runs){
  if(!$seen.Add($run.name) -or !$budgets.ContainsKey($run.name) -or !$run.passed -or $run.exitCode -ne 0 -or !$run.ownedJob -or !$run.originalHandleCaptured -or !$run.exitObserved -or !$run.childRetirementIndependentlyAudited -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.killRequested -or $run.timedOut -or $run.unsafeToContinue -or $run.parentBudgetMs -ne $budgets[$run.name] -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Owned delivery contract differs'}
 }
 foreach($f in $r.before){Pin $f.path $f.sha256;if((Get-Acl -LiteralPath $f.path).Sddl -cne $f.sddl){throw 'Delivery ACL changed'}}
 foreach($f in $r.inputs){Pin $f.path $f.sha256}
}
if($p.before.Count -ne 403 -or $e.before.Count -ne 402 -or $p.runs.Count -ne 1 -or $e.runs.Count -ne 3 -or $seen.Count -ne 4){throw 'Delivery inventory differs'}
$actualPath=Join-Path (Split-Path $extractPath) 'clean.json'
$actual=Get-Content -LiteralPath $actualPath -Raw|ConvertFrom-Json
if(!$actual.Passed -or !$actual.Responsive -or !$actual.Lea -or $actual.LegacyEngine -or $actual.ForcedCleanup -or $actual.CleanupErrors.Count){throw 'Extracted actual EXE evidence differs'}
$printAuditPath='I:\LeaBuildStorage\current-firespider-print-audit-20261110-v1\audit.json'
Pin $printAuditPath 'A0974C0A8265963359B0A3FE93358CAB53C05692267E1CB4F8D0BE6890F0CCE6'
$printAudit=Get-Content -LiteralPath $printAuditPath -Raw|ConvertFrom-Json
if(!$printAudit.verified -or !$printAudit.previewPassed -or !$printAudit.printCommandPassed -or $printAudit.queueSubmitted){throw 'Print audit incomplete'}
$sourcePath='I:\LeaBuildStorage\current-firespider-extraction-source-20261110-v1\source.json'
Pin $sourcePath '5C960582EB29FA85731C02E7CD474664A630DC15E2A268F1AE7E3FD00CA0FDA7'
$source=Get-Content -LiteralPath $sourcePath -Raw|ConvertFrom-Json
Pin $source.source $source.sha256
$template=Join-Path $PSScriptRoot 'Test-PrivateBindingExtraction-v1.generated.ps1'
Pin $template $source.templateSha256
$inverse=[IO.File]::ReadAllText($source.source).Replace(("'"+$PSScriptRoot.Replace("'","''")+"'"),'$PSScriptRoot')
for($i=$source.replacements.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($source.replacements[$i][1],$source.replacements[$i][0])}
if(!$source.verified -or !$source.inverseVerified -or $source.assertionsChanged -or $inverse -cne [IO.File]::ReadAllText($template)){throw 'Extraction assertions changed'}
$proofPath='I:\LeaBuildStorage\current-firespider-package-20261110-v1\result.json'
$proof=Get-Content -LiteralPath $proofPath -Raw|ConvertFrom-Json
$bound=@($e.inputs|Where-Object path -CEQ $proofPath)
if($bound.Count -ne 1){throw 'Package not bound by extraction'};Pin $proofPath $bound[0].sha256
Pin $proof.archive '81CBAC6F685EC65DF45DF069C99F03FC263340683CB94F5908C5D1E9C0CE7482'
if(!$proof.verified -or $proof.entries -ne 402 -or $proof.members.Count -ne 402){throw 'Package identity differs'}
$zip=[IO.Compression.ZipFile]::OpenRead($proof.archive)
try{
 if($zip.Entries.Count -ne 402){throw 'Archive inventory differs'}
 foreach($row in $proof.members){$entry=$zip.GetEntry($row.path);if(!$entry -or $entry.Length -ne $row.bytes){throw 'Archive member differs'};$s=$entry.Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $row.sha256){throw 'Archive bytes differ'}}finally{$s.Dispose()};Pin (Join-Path $e.runtime $row.path) $row.sha256}
}finally{$zip.Dispose()}
Pin (Join-Path $e.runtime 'Firespider Internet 19.exe') '9513A0BD7C620589F4AB332CE19F155686C01104B1E8D99A5DECAAF1814F2586'
New-Item -ItemType Directory -Path $out|Out-Null
$files=@($printPath,$extractPath,$actualPath,$proofPath,$printAuditPath,$sourcePath,$source.source,$PSCommandPath)+@(@($p.runs)+@($e.runs)|ForEach-Object {$_.stdout;$_.stderr})
$copies=@();$index=0
foreach($from in $files){$to=Join-Path $out ((++$index).ToString('D2')+'-'+[IO.Path]::GetFileName($from));Copy-Item -LiteralPath $from -Destination $to;$hash=(Get-FileHash -LiteralPath $from).Hash;Pin $to $hash;$copies+=@{source=$from;copy=$to;sha256=$hash}}
@{verified=$true;extractionSha256=$ExtractionSha256;archiveSha256=$proof.sha256;runtimeBytesAndAclsUnchanged=$true;all402ArchiveMembersReadBack=$true;ownedRuns=4;actualExeResponsive=$true;legacyEngine=$false;allOwnedProcessesRetired=$true;assertionsChanged=$false;limitsChanged=$false;physicalPrintingQualified=$false;cleanMachineQualified=$false;releaseQualified=$false;copies=$copies}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'audit.json')
