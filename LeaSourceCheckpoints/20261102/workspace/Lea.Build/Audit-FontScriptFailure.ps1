$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$suite=Join-Path $root 'continuity-evidence/temporal-startup-v1/font-script-suite-v1'
$run=Join-Path $root 'continuity-evidence/temporal-startup-v1/font-script-owned-run-v1'
$output=Join-Path $run 'failure-audit.json'
if(Test-Path $output){throw 'Preserve prior failure audit'}
$pins=@{
    (Join-Path $suite 'result.json')='E87409ED36E23F7C968490DDF206AAEF2D9D990AC9FAED2E144C1712A72EEF7E'
    (Join-Path $suite 'binding-verification.json')='F5331A4688D6B32DDBB1E4DB0B74B44D0034F26C9ADB9DEE379EB2B60A4F3EC1'
    (Join-Path $run 'result.json')='60D6C8E23E088C8955B6F914A8C040FA2DB8335D43AF58323B03590EC40EC8EA'
    (Join-Path $root 'continuity-evidence/current-bootstrap-checkpoint-v2/result.json')='EA55E0A130E9F17295956519494992B0CF1FFC8111D88996955654AFED0DA9B9'
    (Join-Path $root 'Lea.Build/ReadBootstrapResource.cs')='3D1A6A9F165D4FE0F2A83851C851A9951B1D5BD7C333CF1B2CD0F6E725870728'
}
foreach($path in $pins.Keys){if((Get-FileHash $path).Hash -cne $pins[$path]){throw 'Pinned failure input differs'}}
$build=Get-Content (Join-Path $suite 'result.json') -Raw|ConvertFrom-Json
$result=Get-Content (Join-Path $run 'result.json') -Raw|ConvertFrom-Json
if(!$build.passed -or $result.passed -or !$result.completed -or !$result.inputsUnchanged -or $result.ownerAttention -or $result.changedFiles.Count -or !$result.traceActivationVerified -or !$result.traceEnvironmentRestored -or $result.runs.Count -ne 1){throw 'Expected safely retained failure differs'}
$process=$result.runs[0]
if($process.exitCode -ne 1 -or $process.timedOut -or $process.killRequested -or $process.activeAfterRetirement -ne 0 -or !$process.outputDrained){throw 'Failure lifecycle differs'}
foreach($file in $build.files){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Bound runtime changed'}}
$lines=@(Get-Content $process.stderr)
$phase=@($lines|Where-Object {$_ -match '^LEA_SUITE_PHASE '})
if($phase.Count -ne 17){throw 'Expected phases0-7 then8begin'}
for($i=0;$i -lt 8;$i++){
    if($phase[2*$i] -notmatch ('^LEA_SUITE_PHASE \d+ '+$i+' begin$') -or $phase[2*$i+1] -notmatch ('^LEA_SUITE_PHASE \d+ '+$i+' end$')){throw 'Failure prefix differs'}
}
if($phase[16] -notmatch '^LEA_SUITE_PHASE \d+ 8 begin$' -or @($lines|Where-Object {$_ -match '^System.InvalidOperationException: FAIL computed script: limit; Script budget exceeded\. phase=bootstrap; reason=TimeoutException; stage=document$'}).Count -ne 1){throw 'Computed bootstrap failure not uniquely identified'}
$parent=Join-Path $build.runtime 'Lea.Script.dll'
if((Get-FileHash $parent).Hash -cne 'E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5'){throw 'Parent checkpoint identity differs'}
Add-Type -Path (Join-Path $root 'Lea.Build/ReadBootstrapResource.cs')
$bytes=[ReadBootstrapResource]::Read($parent)
$resourceHash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes))
if($resourceHash -cne 'EC4B6513A77230C3B8439568CC3FED41B7712CA3779182D38076880DCC83BC8A'){throw 'Actual embedded bootstrap differs'}
$checkpoint=Get-Content (Join-Path $root 'continuity-evidence/current-bootstrap-checkpoint-v2/result.json') -Raw|ConvertFrom-Json
if(!$checkpoint.verified -or !$checkpoint.wrongResourceRejected -or !$checkpoint.markerReportsLastCompletedSection -or $checkpoint.causeProven){throw 'Checkpoint semantics audit differs'}
foreach($path in $pins.Keys){if((Get-FileHash $path).Hash -cne $pins[$path]){throw 'Audit input changed'}}
$audit=@{failureAudited=$true;suitePassed=$false;phase=8;lastCompletedBootstrapCheckpoint='document';exclusiveDocumentCostProven=$false;remainingTail=$checkpoint.remainingTail;bootstrapResourceSha256=$resourceHash;completedEarlierPhases=8;processElapsedMs=$process.elapsedMs;retirementMs=$process.retirementAndDrainElapsedMs;runtimeFilesVerified=$build.files.Count;inputPins=$pins;runnerSha256=(Get-FileHash $PSCommandPath).Hash;limitsChanged=$false;causeProven=$false}
[IO.File]::WriteAllText($output,($audit|ConvertTo-Json -Depth 5))
if(!(Get-Content $output -Raw|ConvertFrom-Json).failureAudited){throw 'Failure audit readback differs'}
$audit|Select-Object failureAudited,suitePassed,phase,lastCompletedBootstrapCheckpoint,exclusiveDocumentCostProven,remainingTail,runtimeFilesVerified|ConvertTo-Json
