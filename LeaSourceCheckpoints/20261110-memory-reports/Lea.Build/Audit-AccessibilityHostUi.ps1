param([Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$ResultSha256,[ValidateSet(2,3,4)][int]$Version=2)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$folder=Join-Path $root $(if($Version -eq 4){'continuity-evidence/memory-report-host-ui-v1'}else{'continuity-evidence/accessibility-host-ui-v'+$Version})
$out=if($Version -eq 4){'I:\LeaBuildStorage\memory-report-host-ui-audit-20261110-v1'}else{'I:\LeaBuildStorage\accessibility-host-ui-audit-20261110-v'+$Version}
if(Test-Path -LiteralPath $out){throw 'Preserve UI audit'}
$path=Join-Path $folder 'result.json'
if((Get-FileHash -LiteralPath $path).Hash -cne $ResultSha256){throw 'UI receipt differs'}
$r=Get-Content -LiteralPath $path -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.completed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.before.Count -ne 401 -or $r.byteChanges.Count -or $r.aclChanges.Count -or $r.runs.Count -ne 2){throw 'UI test boundary incomplete'}
$budgets=@{'compile-accessibility-probe'=60000;'accessibility-host-ui'=90000}
$seen=[Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach($run in $r.runs){if(!$seen.Add($run.name) -or !$budgets.ContainsKey($run.name) -or !$run.passed -or $run.exitCode -ne 0 -or !$run.originalHandleCaptured -or !$run.ownedJob -or !$run.exitObserved -or !$run.childRetirementIndependentlyAudited -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.timedOut -or $run.killRequested -or $run.unsafeToContinue -or $run.parentBudgetMs -ne $budgets[$run.name] -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Owned UI run contract differs'}}
foreach($f in $r.before){if((Get-FileHash -LiteralPath $f.path).Hash -cne $f.sha256 -or (Get-Acl -LiteralPath $f.path).Sddl -cne $f.sddl){throw 'Runtime bytes/ACL drift'}}
foreach($f in $r.inputs){if((Get-FileHash -LiteralPath $f.path).Hash -cne $f.sha256){throw 'UI input drift'}}
$hostHash=if($Version -eq 4){'86DB310DB3CAD186A046308A63FB9CDC352D42B10A9816E51A51FF6C59BAFF4D'}else{'61986B5044C5D0FED6B77C6895386813184177E4B5DB9F76C8FC5DA40FF90A1A'}
if((Get-FileHash -LiteralPath (Join-Path $r.runtime 'Firespider Internet 19.exe')).Hash -cne $hostHash){throw 'Host identity differs'}
$lines=Get-Content -LiteralPath $r.runs[1].stdout
foreach($footer in @('PASS Lea accessibility host: click/hover/escaping/empty-tab/no-legacy','PASS navigation: saved/default home, new Lea homepage, tab list, and local-file domain guard','PASS navigation: Lea/legacy snapshots, private opens, append/order, bounds, locked and empty sessions')){if(@($lines|Where-Object {$_ -ceq $footer}).Count -ne 1){throw 'UI/navigation footer missing'}}
if($Version -eq 3){
 if(@($lines|Where-Object {$_ -ceq 'PASS Lea accessibility states: loading/paused/no-selection guards; private-retention issue reproduced, NOT fixed'}).Count -ne 1){throw 'State diagnostic footer missing'}
 $privacy=Get-Content -LiteralPath (Join-Path $folder 'private-report-retention.json') -Raw|ConvertFrom-Json
 if(!$privacy.privateView -or !$privacy.reportRetainedAfterTabDisposal -or !$privacy.containsSourceUrl -or $privacy.privacyFixed){throw 'Private-retention observation differs'}
}
if($Version -eq 4){
 if(@($lines|Where-Object {$_ -ceq 'PASS Lea accessibility states: loading/paused/no-selection guards; private memory report/session checks'}).Count -ne 1){throw 'Memory report regression footer missing'}
 $privacy=Get-Content -LiteralPath (Join-Path $folder 'private-report-retention.json') -Raw|ConvertFrom-Json
 if(!$privacy.privateView -or $privacy.reportUrlIsFile -or $privacy.privateSourceOrReportInSession -or $privacy.globalPrivacyQualified){throw 'Memory report regression evidence differs'}
 foreach($pair in @(@('Lea.Engine.dll','F05E474C73BAAE19278EE4C80D9D617BD361432B3AADCD472BEA680280E9CBFD'),@('Lea.Win.dll','FEBA2DFD4C987ADDC47B53217947D05B112C60B2E4AE3C942F72FEAE79F3E77E'))){if((Get-FileHash -LiteralPath (Join-Path $r.runtime $pair[0])).Hash -cne $pair[1]){throw 'Memory report engine/view pair differs'}}
}
New-Item -ItemType Directory -Path $out|Out-Null
$copies=@(foreach($file in Get-ChildItem -LiteralPath $folder -File){$to=Join-Path $out $file.Name;Copy-Item -LiteralPath $file.FullName -Destination $to;$hash=(Get-FileHash -LiteralPath $file.FullName).Hash;if((Get-FileHash -LiteralPath $to).Hash -cne $hash){throw 'Evidence copy mismatch'};@{path=$to;sha256=$hash}})
@{verified=$true;resultSha256=$ResultSha256;hostSha256=$hostHash;ownedRuns=2;runtimeFiles=401;bytesAndAclStable=$true;clickHoverEscapingEmptyGuard=$true;originalNavigationChecksRetained=$true;privateReportFileRegressionPassed=($Version -eq 4);globalPrivacyQualified=$false;loadingDisposedPausedSplitQualified=$false;all15WorkflowsQualified=$false;releaseQualified=$false;copies=$copies}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'audit.json')
