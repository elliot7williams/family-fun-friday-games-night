param([Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedReceipt)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$folder=Join-Path $root 'continuity-evidence/deterministic-host-workflows-v1'
$out='I:\LeaBuildStorage\deterministic-host-workflow-audit-20261102-v1'
if(Test-Path $out){throw 'Preserve audit output'}
$path=Join-Path $folder 'result.json'
if((Get-FileHash $path).Hash -cne $ExpectedReceipt){throw 'Receipt identity differs'}
$r=Get-Content $path -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.completed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.byteChanges.Count -or $r.aclChanges.Count -or $r.before.Count -ne 400 -or $r.runs.Count -ne 15){throw 'Campaign incomplete'}
$expected=@{'settings-path'=30}
foreach($case in @('clean','lea','navigation','second','split','edit','style-start','script')){$expected.Add('host-'+$case,90)}
foreach($case in @('clean','restored','locked','oversized','lazy-background','lazy-all')){$expected.Add('exe-'+$case,100)}
$seen=[Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach($run in $r.runs){
    if(!$expected.ContainsKey($run.name) -or !$seen.Add($run.name) -or !$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.ownedJob -or !$run.originalHandleCaptured -or !$run.exitObserved -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.parentBudgetMs -ne 1000*$expected[$run.name] -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw ('Owned run failed audit: '+$run.name)}
    if($run.name.StartsWith('host-')){
        $case=$run.name.Substring(5)
        if(!$run.exactFooterVerified -or @(Get-Content $run.stdout|Where-Object {$_.StartsWith('PASS Lea boot: '+$case+';')}).Count -ne 1){throw 'Host footer differs'}
    }
}
foreach($file in $r.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Runtime byte/ACL audit differs'}}
foreach($file in $r.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Input changed'}}
foreach($case in @('clean','restored','locked','oversized','lazy-background','lazy-all')){
    $e=Get-Content (Join-Path $folder ($case+'.json')) -Raw|ConvertFrom-Json
    if(!$e.Passed -or !$e.Responsive -or $e.LegacyEngine -or $e.ForcedCleanup -or $e.CleanupErrors.Count -or $e.Scenario -cne $case){throw ('EXE evidence incomplete: '+$case)}
}
if((Get-FileHash (Join-Path $r.runtime 'Firespider Internet 19.exe')).Hash -cne 'B1B290CA5E90B0F5DD2460BFA3A90DCB81EF8F0ADF5DA96599D437B36B3F416E'){throw 'Host differs'}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($file in Get-ChildItem $folder -File){Copy-Item -LiteralPath $file.FullName -Destination $out;if((Get-FileHash (Join-Path $out $file.Name)).Hash -cne (Get-FileHash $file.FullName).Hash){throw 'Evidence readback differs'}}
@{passed=$true;receiptSha256=$ExpectedReceipt;runs=15;actualExeScenarios=6;runtimeMembers=400;scope='Independent current-machine workflow audit; no print-command/preview, reliability, clean-PC, all-sites or release qualification'}|ConvertTo-Json|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
