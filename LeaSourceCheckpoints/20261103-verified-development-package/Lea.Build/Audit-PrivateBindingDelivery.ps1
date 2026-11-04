$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$package='I:\LeaBuildStorage\private-binding-firespider-package-20261103-v1'
$out='I:\LeaBuildStorage\private-binding-delivery-audit-20261103-v1'
if(Test-Path $out){throw 'Preserve delivery audit'}
function Pin([string]$path,[string]$hash){if((Get-FileHash $path).Hash -cne $hash){throw ('Delivery artifact differs: '+$path)}}
$printPath=Join-Path $root 'continuity-evidence/private-binding-host-print-v1/result.json'
$extractPath=Join-Path $root 'continuity-evidence/private-binding-package-extraction-v1/result.json'
Pin $printPath '293D69D23B6BC2CA7C6C5D9433E89D8166599088FA4BE213AD4179C731B47789'
Pin $extractPath '1196A596DFD41807A1A73BDE2501C15AC0088D8D34F173FD2EBB67879400F815'
$p=Get-Content $printPath -Raw|ConvertFrom-Json;$e=Get-Content $extractPath -Raw|ConvertFrom-Json
$budgets=@{'print-command-preview'=90000;'settings-path'=30000;'navigation'=90000;'exe-clean'=100000}
$seen=[Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach($r in @($p,$e)){
    if(!$r.passed -or !$r.completed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.byteChanges.Count -or $r.aclChanges.Count){throw 'Delivery test boundary incomplete'}
    foreach($run in $r.runs){
        if(!$seen.Add($run.name) -or !$budgets.ContainsKey($run.name) -or !$run.passed -or $run.exitCode -ne 0 -or !$run.ownedJob -or !$run.originalHandleCaptured -or !$run.exitObserved -or !$run.childRetirementIndependentlyAudited -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.killRequested -or $run.timedOut -or $run.unsafeToContinue -or $run.parentBudgetMs -ne $budgets[$run.name] -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Original owned delivery run differs'}
    }
    foreach($f in $r.before){Pin $f.path $f.sha256;if((Get-Acl $f.path).Sddl -cne $f.sddl){throw 'Delivery runtime ACL changed'}}
    foreach($f in $r.inputs){Pin $f.path $f.sha256}
}
if($p.before.Count -ne 403 -or $e.before.Count -ne 402 -or $seen.Count -ne 4){throw 'Delivery inventories differ'}
$printLog=Get-Content $p.runs[0].stdout
foreach($prefix in @('PASS Lea browser print preview:','PASS Lea browser print command:','PASS Lea boot: print-command;')){if(@($printLog|Where-Object {$_.StartsWith($prefix)}).Count -ne 1){throw 'Exact print footer missing'}}
$actual=Get-Content (Join-Path (Split-Path $extractPath) 'clean.json') -Raw|ConvertFrom-Json
if(!$actual.Passed -or !$actual.Responsive -or !$actual.Lea -or $actual.LegacyEngine -or $actual.ForcedCleanup -or $actual.CleanupErrors.Count){throw 'Actual extracted EXE evidence differs'}
$proofPath=Join-Path $package 'result.json';Pin $proofPath 'AF32F7320FB74F2CBAE418E5966196D93AEFFBE278A7A41E4B72BFC4F9762E7D'
$proof=Get-Content $proofPath -Raw|ConvertFrom-Json
Pin $proof.archive '2D684D53800618A2920E86B6051526DEB651D946EC6781A628F887CAD71B3D68'
if(!$proof.verified -or !$proof.privateBindingPair -or $proof.entries -ne 402 -or $proof.members.Count -ne 402){throw 'Package identity differs'}
$zip=[IO.Compression.ZipFile]::OpenRead($proof.archive)
try{foreach($row in $proof.members){$entry=$zip.GetEntry($row.path);if(!$entry -or $entry.Length -ne $row.bytes){throw 'Archive member differs'};$s=$entry.Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $row.sha256){throw 'Archive member hash differs'}}finally{$s.Dispose()};Pin (Join-Path $e.runtime $row.path) $row.sha256}}finally{$zip.Dispose()}
foreach($pair in @(@('Firespider Internet 19.exe','B1A707A23CEB1DEE7FBDE13102BE24FAE7369FC21FC43F0688D35522197B1F85'),@('Lea.Script.dll','406C5634638D75C66892BD95429760CB4A5C74C94277913D1DF467147E30E75F'),@('ScriptWorker/Lea.Script.Worker.dll','F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0'))){Pin (Join-Path $e.runtime $pair[0]) $pair[1]}
New-Item -ItemType Directory -Path $out|Out-Null
$files=@($printPath,$extractPath,$proofPath)+@(@($p.runs)+@($e.runs)|ForEach-Object {$_.stdout;$_.stderr})
$copies=@();$index=0
foreach($from in $files){$to=Join-Path $out ((++$index).ToString('D2')+'-'+[IO.Path]::GetFileName($from));Copy-Item -LiteralPath $from -Destination $to;$hash=(Get-FileHash $from).Hash;Pin $to $hash;$copies+=@{source=$from;copy=$to;sha256=$hash}}
@{verified=$true;printRuntimeFiles=403;extractedFiles=402;ownedRuns=4;printAndPreviewFootersVerified=$true;allArchiveMembersReadBack=$true;runtimeBytesAndAclsUnchanged=$true;actualExeResponsive=$true;legacyEngine=$false;allOwnedProcessesRetired=$true;physicalPrintingQualified=$false;cleanMachineQualified=$false;releaseQualified=$false;archiveSha256=$proof.sha256;copies=$copies;runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
