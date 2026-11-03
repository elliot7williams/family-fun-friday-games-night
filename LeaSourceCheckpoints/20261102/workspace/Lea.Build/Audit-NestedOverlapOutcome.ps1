$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$root=Join-Path $workspace 'continuity-evidence/nested-overlap-regression-v1'
$receipt=Join-Path $root 'result.json';$out=Join-Path $root 'outcome-observation.json'
if(Test-Path -LiteralPath $out){throw 'Preserve native outcome observation'}
if((Get-FileHash -LiteralPath $receipt).Hash -cne 'E60A49F48671E92B80279171E5FC274BB4D7CFF2020D185A86729B7A38F04B6E'){throw 'Run receipt differs'}
$r=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
if(!$r.behaviorPassed -or !$r.inputsUnchanged -or !$r.bytesUnchanged -or $r.ownerAttention -or !$r.nestedOverlapSetupInverseVerified){throw 'Run evidence incomplete'}
foreach($file in @($r.inputs)+@($r.before)){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'Input/candidate binding changed'}}
$runs=@($r.runs|Where-Object name -ceq 'full-core')
if($runs.Count -ne 1){throw 'Full run ambiguous'}
$run=$runs[0]
if(!$run.passed -or $run.exitCode -ne 0 -or $run.parentBudgetMs -ne 300000 -or $run.elapsedMs -gt 300000 -or $run.timedOut -or $run.killRequested -or $run.pending -or $run.retainedForOwner -or $run.unsafeToContinue -or !$run.originalHandleCaptured -or !$run.exitObserved -or !$run.stdoutComplete -or !$run.stderrComplete){throw 'Run lifetime/output contract differs'}
$source=[IO.File]::ReadAllText((Join-Path $root 'Smoke.cs'))
$footers=[regex]::Matches($source,'Console\.WriteLine\("(?<footer>Lea smoke passed: bounded sticky positioning with compact k,[^"\r\n]*)"\);')
if($footers.Count -ne 1 -or $footers[0].Groups['footer'].Value.Contains('\')){throw 'Unambiguous unescaped source footer required'}
$helper=Join-Path $PSScriptRoot 'Test-LeaSuiteFooter.ps1'
if((Get-FileHash -LiteralPath $helper).Hash -cne 'BFD1DD8FC9DACBCA1E7177AA740F8D7EA8974E5B4C4BB344C1DCD16055EE47AE'){throw 'Footer verifier differs'}
. $helper
if(!(Test-LeaSuiteFooter $run.stdout $footers[0].Groups['footer'].Value)){throw 'Exact unique terminal source footer missing'}
$acl=Get-Content -LiteralPath (Join-Path $root 'acl-observation.json') -Raw|ConvertFrom-Json
if(!$acl.classified -or $acl.filesystemAclModified -or $acl.productionPolicyBroadened -or $acl.observations.Count -ne 3){throw 'Private fixture ACL classification incomplete'}
foreach($file in $acl.observations){if((Get-Acl -LiteralPath $file.path).Sddl -cne $file.after -or !$file.inverseReconstructsBaseline){throw 'Observed ACL differs'}}
$report=@{nativeBehaviorPassed=$true;exactTerminalFooterVerified=$true;elapsedMs=$run.elapsedMs;originalParentBudgetMs=$run.parentBudgetMs;inputsAndRuntimeBytesHeld=$true;privateFixtureGrantCount=3;rawAuditPassed=$r.passed;filesystemAclModified=$false;productionPolicyBroadened=$false;profileDeletionProven=$false;hostScriptOrReleaseQualified=$false;receiptSha256=(Get-FileHash -LiteralPath $receipt).Hash;aclObservationSha256=(Get-FileHash -LiteralPath (Join-Path $root 'acl-observation.json')).Hash;scope='One full native execution, exact matched input/runtime binding and read-only attribution of three private fixture grants. Not startup reliability or full release qualification.'}
$report|ConvertTo-Json|Set-Content -LiteralPath $out
$report|ConvertTo-Json
