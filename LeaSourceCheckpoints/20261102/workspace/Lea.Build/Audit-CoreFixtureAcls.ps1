$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/rebuilt-core-regression-v4'
$receipt=Join-Path $root 'result.json'
$out=Join-Path $root 'acl-observation.json'
if(Test-Path -LiteralPath $out){throw 'Preserve observation'}
if((Get-FileHash -LiteralPath $receipt).Hash -cne 'B0CC2CAF715527EB68885F31860EAA7D3A01D7A66826000941BFA01D7FBAD631'){throw 'Receipt changed'}
$run=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
if($run.passed -or !$run.behaviorPassed -or !$run.fullSuiteFooter -or $run.ownerAttention -or !$run.inputsUnchanged -or !$run.bytesUnchanged){throw 'Unexpected run state'}
$expected=@{
    'Lea.Image.exe'='FE744BE1D11E99B36A35F4E6CF12C45229AB7FF89134434E61438DE8DED3E535'
    'Lea.Image.Handshake.exe'='79EAE62C778644F11468C079C8B2E7DCFEAE9643CDA55383E7EB7F76D2742C5E'
    'Lea.Tests.exe'='094D88DA0630BF414ED91F3ABFF24750F00239EB9409D2599E300F547035BE08'
}
$sid='S-1-15-2-3597768014-2038560105-1077951562-531713394-1456559389-3372869253-1355503848'
$rights=[Security.AccessControl.FileSystemRights]::ReadAndExecute -bor [Security.AccessControl.FileSystemRights]::Synchronize
$observations=[Collections.Generic.List[object]]::new()
foreach($file in $run.before){
    if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'Candidate bytes changed'}
    $after=(Get-Acl -LiteralPath $file.path).Sddl
    if($after -ceq $file.sddl){continue}
    $name=[IO.Path]::GetFileName($file.path)
    if(!$expected.ContainsKey($name) -or $file.path -cne (Join-Path $run.candidate $name) -or $file.sha256 -cne $expected[$name]){throw 'Unexpected file grant'}
    $raw=[Security.AccessControl.RawSecurityDescriptor]::new($after)
    $rawRules=@($raw.DiscretionaryAcl|Where-Object{$_ -is [Security.AccessControl.KnownAce] -and $_.SecurityIdentifier.Value -ceq $sid})
    if($rawRules.Count -ne 1 -or $rawRules[0].AceFlags -ne 'None'){throw 'Raw ACE count/flags differ'}
    $security=[Security.AccessControl.FileSecurity]::new();$security.SetSecurityDescriptorSddlForm($after)
    $rules=@($security.GetAccessRules($true,$true,[Security.Principal.SecurityIdentifier])|Where-Object{$_.IdentityReference.Value -ceq $sid})
    if($rules.Count -ne 1 -or $rules[0].FileSystemRights -ne $rights -or $rules[0].AccessControlType -ne 'Allow' -or $rules[0].IsInherited -or $rules[0].InheritanceFlags -ne 'None' -or $rules[0].PropagationFlags -ne 'None'){throw 'Grant rights/scope differ'}
    $security.RemoveAccessRuleSpecific($rules[0])
    if($security.GetSecurityDescriptorSddlForm([Security.AccessControl.AccessControlSections]::All) -cne $file.sddl){throw 'Unclassified ACL changes remain'}
    $observations.Add(@{path=$file.path;sha256=$file.sha256;before=$file.sddl;after=$after;onlyDifference='One explicit fixed decoder SID RX+Synchronize allow ACE';inverseReconstructsBaseline=$true;fixtureExecutable=$name -cne 'Lea.Image.exe'})
}
if($observations.Count -ne 3){throw 'Expected exactly three observed grants'}
foreach($input in $run.inputs){if((Get-FileHash -LiteralPath $input.path).Hash -cne $input.sha256){throw 'Run input changed'}}
[IO.File]::WriteAllText($out,(@{classified=$true;observations=$observations;rawAuditPassed=$false;filesystemAclModified=$false;productionPolicyBroadened=$false;profileDeletionProven=$false;releaseCertified=$false;scope='Read-only attribution of three exact private fixture grants; not a general permission exception or cleanup certification'}|ConvertTo-Json -Depth 6))
"PASS read-only fixture ACL attribution: $($observations.Count) exact grants; raw audit remains failed."
(Get-FileHash -LiteralPath $out).Hash
