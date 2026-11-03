$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$directory=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/current-lazy-run-v2'
$out=Join-Path $directory 'outcome-classification.json'
if(Test-Path $out){throw 'Preserve outcome classification'}
$path=Join-Path $directory 'result.json'
if((Get-FileHash -LiteralPath $path).Hash -cne '161602583277153A105EFE8313A4B74309A4CCBA1D19A72209E93BE2C4318F24'){throw 'Run receipt changed'}
$r=Get-Content -LiteralPath $path -Raw|ConvertFrom-Json
if(!$r.completed -or $r.passed -or $r.behaviorPassed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.error -or $r.runs.Count -ne 1){throw 'Unexpected retained audit failure'}
$parent=$r.runs[0]
if(!$parent.passed -or $parent.exitCode -ne 0 -or $parent.timedOut -or $parent.killRequested -or !$parent.exitObserved -or !$parent.stdoutComplete -or !$parent.stderrComplete -or $parent.elapsedMs -gt 300000){throw 'Parent gate failed'}
$pattern='^PASS Lea\.Script: (\d+) checks; JavaScript, isolated worker, and typed DOM\.$'
if('PASS Lea.Script: ordinary assertion' -cmatch $pattern -or 'PASS Lea.Script: 1 checks; JavaScript, isolated worker, and typed DOM. extra' -cmatch $pattern){throw 'Footer parser accepts ordinary/trailing text'}
$lines=@(Get-Content -LiteralPath $parent.stdout)
$footers=@($lines|Where-Object {$_ -cmatch $pattern})
if($footers.Count -ne 1 -or $footers[0] -cne $lines[-1]){throw 'Missing, duplicate or nonterminal final footer'}
$null=$footers[0] -cmatch $pattern;$checks=[int]$Matches[1]
if($checks -ne 3320 -or @($lines|Where-Object {$_.StartsWith('PASS ')}).Count -ne $checks+1){throw 'Unexpected assertion count'}
$phasePath=Join-Path $directory 'phase-summary.json';$phases=Get-Content -LiteralPath $phasePath -Raw|ConvertFrom-Json
if($phases.runReceiptSha256 -cne '161602583277153A105EFE8313A4B74309A4CCBA1D19A72209E93BE2C4318F24' -or $phases.completedStages -ne 89 -or $phases.unfinishedStage){throw 'Timing stages incomplete'}
foreach($input in $r.inputFiles){if((Get-FileHash -LiteralPath $input.path).Hash -cne $input.sha256){throw 'Run input changed'}}
$changes=@(foreach($file in $r.before){
    if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'Candidate bytes changed'}
    $sddl=(Get-Acl -LiteralPath $file.path).Sddl
    if($sddl -cne $file.sddl){@{path=$file.path;before=$file.sddl;after=$sddl}}
})
if($changes.Count -ne 1 -or [IO.Path]::GetFileName($changes[0].path) -cne 'Lea.Image.exe'){throw 'Unexpected ACL delta'}
$view=Join-Path $r.candidate 'Lea.Win.dll'
if((Get-FileHash -LiteralPath $view).Hash -cne 'E26135F8CE6BA76A76515D2874DFE219E172B212037CD567AD37042DA516289B'){throw 'View identity changed'}
$type=[Reflection.Assembly]::LoadFrom($view).GetType('Lea.Win.Rp',$true)
$flags=[Reflection.BindingFlags]'Static,NonPublic'
$name=$type.GetField('ContainerName',$flags).GetRawConstantValue()
if($name -cne 'Lea.Image.Decoder'){throw 'Unexpected image profile'}
$argsForSid=[object[]]@($name,[IntPtr]::Zero)
if($type.GetMethod('DeriveAppContainerSidFromAppContainerName',$flags).Invoke($null,$argsForSid) -ne 0 -or $argsForSid[1] -eq [IntPtr]::Zero){throw 'SID derivation failed'}
try{$sid=[Security.Principal.SecurityIdentifier]::new([IntPtr]$argsForSid[1]).Value}finally{$null=$type.GetMethod('FreeSid',$flags).Invoke($null,[object[]]@($argsForSid[1]))}
$security=[Security.AccessControl.FileSecurity]::new();$security.SetSecurityDescriptorSddlForm($changes[0].after)
$rules=@($security.GetAccessRules($true,$false,[Security.Principal.SecurityIdentifier])|Where-Object {$_.IdentityReference.Value -ceq $sid})
$rights=[Security.AccessControl.FileSystemRights]::ReadAndExecute -bor [Security.AccessControl.FileSystemRights]::Synchronize
if($rules.Count -ne 1 -or $rules[0].FileSystemRights -ne $rights -or $rules[0].AccessControlType -ne 'Allow' -or $rules[0].InheritanceFlags -ne 'None' -or $rules[0].PropagationFlags -ne 'None' -or $rules[0].IsInherited){throw 'Unexpected ACE scope/rights'}
$security.RemoveAccessRuleSpecific($rules[0])
if($security.GetSecurityDescriptorSddlForm([Security.AccessControl.AccessControlSections]::All) -cne $changes[0].before){throw 'Additional ACL differences remain'}
$result=@{behavioralSuiteVerified=$true;checks=$checks;completedStages=89;parentElapsedMs=$parent.elapsedMs;exactFooterVerified=$true;ordinaryPrefixMatches=@($lines|Where-Object {$_.StartsWith('PASS Lea.Script: ')}).Count;rawAuditPassed=$false;rawReceiptUnmodified=$true;onlyAclDifferenceIsFixedDecoderReadExecuteAce=$true;aclChanges=$changes;candidateFilesUnchanged=$r.before.Count;inputsUnchanged=$true;filesystemAclModified=$false;profileDeletionProven=$false;releaseCertified=$false;phaseSummarySha256=(Get-FileHash -LiteralPath $phasePath).Hash}
[IO.File]::WriteAllText($out,($result|ConvertTo-Json -Depth 6),[Text.UTF8Encoding]::new($false))
$result|Select-Object behavioralSuiteVerified,checks,completedStages,parentElapsedMs,ordinaryPrefixMatches,rawAuditPassed,onlyAclDifferenceIsFixedDecoderReadExecuteAce,profileDeletionProven,releaseCertified|ConvertTo-Json
(Get-FileHash -LiteralPath $out).Hash
