param([switch]$DataImages,[switch]$LayoutImages,[switch]$SelectionImages,[switch]$DocumentPrint,[switch]$ContiguousPrint)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$folder=Join-Path $root $(if($LayoutImages){'continuity-evidence/print-image-layout-live-v2'}elseif($DataImages){'continuity-evidence/print-data-image-live-v1'}else{'continuity-evidence/print-decoder-roundtrip-v2'})
$receipt=Join-Path $folder 'result.json';$output=Join-Path $folder 'acl-observation.json'
if($SelectionImages){
    $folder=Join-Path $root 'continuity-evidence/selection-image-live-v2'
    $receipt=Join-Path $folder 'result.json';$output=Join-Path $folder 'acl-observation.json'
}
if($DocumentPrint){$folder=Join-Path $root 'continuity-evidence/selection-image-live-v5';$receipt=Join-Path $folder 'result.json';$output=Join-Path $folder 'acl-observation.json'}
if($ContiguousPrint){$folder=Join-Path $root 'continuity-evidence/selection-image-live-v6';$receipt=Join-Path $folder 'result.json';$output=Join-Path $folder 'acl-observation.json'}
if(Test-Path $output){throw 'Preserve earlier ACL observation'}
$expectedReceipt=if($DataImages){'42A506FAB12844444563A0090BE1956DC786AC1FF91FB54A4B2FDB03FBA69CFC'}else{'74F8D56516299EABB482CF267602FC9120CC7EC4DBD6F8CBB6591552B9407789'}
if($LayoutImages){$expectedReceipt='47844E96DE6713B81439ECFB0E83C639D1BC3388BD206B52718DC6B8D7C78645'}
if($SelectionImages){$expectedReceipt='2FA8B1FC0F54F44594E47BA4F1C2E0B31ACC10261581F990FF9C48F65CE52D56'}
if($DocumentPrint){$expectedReceipt='9931D46201AC69D26B24092F230477B4A595544E66F5C4C1E4710243F9036073'}
if($ContiguousPrint){$expectedReceipt='1A901B8AB9AB389F28E89C62204D0A443E86C3DD149620BBFFC7FA2229C5D4E1'}
if((Get-FileHash $receipt).Hash -cne $expectedReceipt){throw 'Roundtrip receipt differs'}
$run=Get-Content $receipt -Raw|ConvertFrom-Json
if(!$run.passed -or !$run.inputsUnchanged -or $run.ownerAttention -or !$run.decoderAclChanged){throw 'Unexpected functional result'}
$worker=Join-Path $folder 'Lea.Image.exe'
if((Get-FileHash $worker).Hash -cne 'FE744BE1D11E99B36A35F4E6CF12C45229AB7FF89134434E61438DE8DED3E535' -or (Get-Acl $worker).Sddl -cne $run.decoderAclAfter){throw 'Worker bytes/current ACL differ'}
$sid='S-1-15-2-3597768014-2038560105-1077951562-531713394-1456559389-3372869253-1355503848'
$rights=[Security.AccessControl.FileSystemRights]::ReadAndExecute -bor [Security.AccessControl.FileSystemRights]::Synchronize
function Classify([string]$Before,[string]$After){
    $raw=[Security.AccessControl.RawSecurityDescriptor]::new($After)
    $aces=@($raw.DiscretionaryAcl|Where-Object {$_ -is [Security.AccessControl.KnownAce] -and $_.SecurityIdentifier.Value -ceq $sid})
    if($aces.Count -ne 1 -or $aces[0].AceFlags -ne 'None'){throw 'Unexpected decoder ACE shape'}
    $security=[Security.AccessControl.FileSecurity]::new();$security.SetSecurityDescriptorSddlForm($After)
    $rules=@($security.GetAccessRules($true,$true,[Security.Principal.SecurityIdentifier])|Where-Object {$_.IdentityReference.Value -ceq $sid})
    if($rules.Count -ne 1 -or $rules[0].FileSystemRights -ne $rights -or $rules[0].AccessControlType -ne 'Allow' -or $rules[0].IsInherited -or $rules[0].InheritanceFlags -ne 'None' -or $rules[0].PropagationFlags -ne 'None'){throw 'Decoder grant rights/scope differ'}
    $security.RemoveAccessRuleSpecific($rules[0])
    if($security.GetSecurityDescriptorSddlForm([Security.AccessControl.AccessControlSections]::All) -cne $Before){throw 'Unclassified ACL difference'}
}
Classify $run.decoderAclBefore $run.decoderAclAfter
$negative=0
$write=$run.decoderAclAfter.Replace('0x1200a9','0x1200ab')
$owner=[Security.AccessControl.FileSecurity]::new();$owner.SetSecurityDescriptorSddlForm($run.decoderAclAfter);$owner.SetOwner([Security.Principal.SecurityIdentifier]::new('S-1-5-32-544'))
$extra=[Security.AccessControl.FileSecurity]::new();$extra.SetSecurityDescriptorSddlForm($run.decoderAclAfter)
$extra.AddAccessRule([Security.AccessControl.FileSystemAccessRule]::new([Security.Principal.SecurityIdentifier]::new('S-1-1-0'),[Security.AccessControl.FileSystemRights]::WriteData,'Allow'))
foreach($bad in @($write,$owner.GetSecurityDescriptorSddlForm('All'),$extra.GetSecurityDescriptorSddlForm('All'))){
    if($bad -ceq $run.decoderAclAfter){throw 'Negative mutation missing'}
    $denied=$false;try{Classify $run.decoderAclBefore $bad}catch{$denied=$true}
    if(!$denied){throw 'ACL classifier accepted negative mutation'};$negative++
}
foreach($input in $run.inputs){if((Get-FileHash $input.path).Hash -cne $input.sha256){throw 'Run input changed'}}
[IO.File]::WriteAllText($output,(@{classified=$true;negativeControls=$negative;receiptSha256=(Get-FileHash $receipt).Hash;classifierSha256=(Get-FileHash $PSCommandPath).Hash;workerSha256=(Get-FileHash $worker).Hash;before=$run.decoderAclBefore;after=$run.decoderAclAfter;onlyDifference='One explicit fixed decoder SID ReadAndExecute+Synchronize Allow ACE';rawAuditPassed=$false;filesystemAclModified=$false;profileDeletionProven=$false;releaseCertified=$false;scope='Read-only attribution for this worker file only, not a filesystem-wide audit or security qualification'}|ConvertTo-Json -Depth 6))
Get-FileHash $output
