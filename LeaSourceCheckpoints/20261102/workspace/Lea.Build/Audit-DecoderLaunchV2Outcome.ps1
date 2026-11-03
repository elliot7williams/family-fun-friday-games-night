$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/decoder-launch-full-v2'
$out=Join-Path $root 'outcome-observation.json'
if(Test-Path -LiteralPath $out){throw 'Preserve outcome observation'}
$receipt=Join-Path $root 'result.json'
if((Get-FileHash -LiteralPath $receipt).Hash -cne 'A06A2CD21986FFBDF882753FB5D639CF4112B24F5686A27385F3A9268CEAC3DE'){throw 'Full receipt differs'}
$r=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
if($r.passed -or $r.fullBehaviorPassed -or !$r.inputsUnchanged -or !$r.candidateBytesUnchanged -or $r.ownerAttention){throw 'Raw outcome differs'}
$runs=@($r.runs|Where-Object name -ceq 'full-engine-traced')
if($runs.Count -ne 1){throw 'Full run ambiguous'}
$run=$runs[0]
if(!$run.passed -or $run.exitCode -ne 0 -or !$run.exitObserved -or $run.timedOut -or $run.killRequested -or $run.pending -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.stderrBytes -ne 0 -or $run.parentBudgetMs -ne 300000 -or $run.elapsedMs -ge 300000){throw 'Full process gate incomplete'}
$testReceipt=Join-Path $workspace 'continuity-evidence/ordered-flow-regression-v4/result.json'
if((Get-FileHash -LiteralPath $testReceipt).Hash -cne '9A1C1B073D72892D652542D9CEAF32C2D5620696B217C5C103309802A597C9C3'){throw 'Test receipt differs'}
$test=Get-Content -LiteralPath $testReceipt -Raw|ConvertFrom-Json
if(!$test.testOverlayInverseVerified -or !$test.nestedModelOverlayInverseVerified){throw 'Test overlay evidence incomplete'}
$source=Join-Path $workspace 'continuity-evidence/ordered-flow-regression-v4/Smoke.cs'
$sourcePin=@($test.inputs|Where-Object path -ceq $source)
if($sourcePin.Count -ne 1 -or (Get-FileHash -LiteralPath $source).Hash -cne $sourcePin[0].sha256){throw 'Matched test source differs'}
$footer=[regex]::Matches([IO.File]::ReadAllText($source),'Console\.WriteLine\("(Lea smoke passed: [^"\r\n]*)"\);')
if($footer.Count -ne 1 -or $footer[0].Groups[1].Value.Contains('\')){throw 'Expected simple literal footer differs'}
$lines=[IO.File]::ReadAllLines($run.stdout)
if($lines.Count -eq 0 -or $lines[-1] -cne $footer[0].Groups[1].Value -or @($lines|Where-Object {$_ -ceq $footer[0].Groups[1].Value}).Count -ne 1){throw 'Exact terminal source footer missing or duplicated'}
$runner=Join-Path $root 'runner.ps1'
$bug='[IO.File]::ReadAllText($run.stdout).Trim().StartsWith('
if(![IO.File]::ReadAllText($runner).Contains($bug) -or [IO.File]::ReadAllText($run.stdout).Trim().StartsWith($footer[0].Groups[1].Value,[StringComparison]::Ordinal)){throw 'Whole-log prefix defect not reproduced'}
$expected=@{
    'Lea.Engine.dll'='52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC'
    'Lea.Win.dll'='B2C1A05ABC9772857F69C9B6AE34C9E1CB69E51DFC35E8F03F60F70F4A523E6D'
    'Lea.Image.exe'='FE744BE1D11E99B36A35F4E6CF12C45229AB7FF89134434E61438DE8DED3E535'
    'Lea.Image.Handshake.exe'='79EAE62C778644F11468C079C8B2E7DCFEAE9643CDA55383E7EB7F76D2742C5E'
    'Lea.Tests.exe'='7728A2E5905FB3A5E22AD912FD24AB76F84C7A5B9F30FE98EA4B7FE9C54B80F4'
}
if($r.before.Count -ne 5){throw 'Candidate inventory differs'}
$sid='S-1-15-2-3597768014-2038560105-1077951562-531713394-1456559389-3372869253-1355503848'
$rights=[Security.AccessControl.FileSystemRights]::ReadAndExecute -bor [Security.AccessControl.FileSystemRights]::Synchronize
$grants=[Collections.Generic.List[object]]::new()
foreach($file in $r.before){
    $name=[IO.Path]::GetFileName($file.path)
    if(!$expected.ContainsKey($name) -or $file.path -cne (Join-Path $r.candidate $name) -or $file.sha256 -cne $expected[$name] -or (Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'Exact candidate binding differs'}
    $after=(Get-Acl -LiteralPath $file.path).Sddl
    if($after -ceq $file.sddl){continue}
    if($name -cnotin @('Lea.Image.exe','Lea.Image.Handshake.exe','Lea.Tests.exe')){throw 'Unexpected permission delta'}
    $raw=[Security.AccessControl.RawSecurityDescriptor]::new($after)
    $rawRules=@($raw.DiscretionaryAcl|Where-Object {$_ -is [Security.AccessControl.KnownAce] -and $_.SecurityIdentifier.Value -ceq $sid})
    if($rawRules.Count -ne 1 -or $rawRules[0].AceFlags -ne 'None'){throw 'Raw grant count or flags differ'}
    $security=[Security.AccessControl.FileSecurity]::new();$security.SetSecurityDescriptorSddlForm($after)
    $rules=@($security.GetAccessRules($true,$true,[Security.Principal.SecurityIdentifier])|Where-Object {$_.IdentityReference.Value -ceq $sid})
    if($rules.Count -ne 1 -or $rules[0].FileSystemRights -ne $rights -or $rules[0].AccessControlType -ne 'Allow' -or $rules[0].IsInherited -or $rules[0].InheritanceFlags -ne 'None' -or $rules[0].PropagationFlags -ne 'None'){throw 'Grant scope differs'}
    $security.RemoveAccessRuleSpecific($rules[0])
    if($security.GetSecurityDescriptorSddlForm([Security.AccessControl.AccessControlSections]::All) -cne $file.sddl){throw 'Other permission delta remains'}
    $grants.Add(@{path=$file.path;sha256=$file.sha256;before=$file.sddl;after=$after;inverseReconstructsBaseline=$true})
}
if($grants.Count -ne 3){throw 'Expected exactly three fixture grants'}
foreach($input in $r.inputs){if((Get-FileHash -LiteralPath $input.path).Hash -cne $input.sha256){throw 'Retained run input changed'}}
$observation=@{behaviorVerified=$true;rawReceiptSha256='A06A2CD21986FFBDF882753FB5D639CF4112B24F5686A27385F3A9268CEAC3DE';terminalFooterMatchesExactSource=$true;footerCheckDefect='Whole-output StartsWith used instead of terminal-line check';exitCode=0;elapsedMs=$run.elapsedMs;deadlineMs=300000;grants=$grants;rawAuditPassed=$false;filesystemAclModified=$false;productionPolicyBroadened=$false;profileDeletionProven=$false;startupReliabilityProven=$false;releaseCertified=$false;scope='One exact matched full engine/view execution and read-only attribution, not cold reliability, scripting, host qualification, or release approval'}
[IO.File]::WriteAllText($out,($observation|ConvertTo-Json -Depth 6))
Get-FileHash -LiteralPath $out
