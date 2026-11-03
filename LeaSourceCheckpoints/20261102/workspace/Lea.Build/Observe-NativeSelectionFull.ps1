$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$root=Join-Path $workspace 'continuity-evidence/native-selection-full-v1'
$receipt=Join-Path $root 'result.json';$aclPath=Join-Path $root 'acl-observation.json';$out=Join-Path $root 'outcome-observation.json'
if(Test-Path $out){throw 'Preserve outcome observation'}
if((Get-FileHash $receipt).Hash -cne '328055ACF148C0E56996680594156F4AA247FEAA76D735EA42D3930EDBF2A078' -or (Get-FileHash $aclPath).Hash -cne '3C8C826EB1512716ABE40581036BB8449419C0345973C18D28BBA03C9B672A1C'){throw 'Native result or ACL observation differs'}
$r=Get-Content $receipt -Raw|ConvertFrom-Json;$acl=Get-Content $aclPath -Raw|ConvertFrom-Json
if($r.passed -or !$r.behaviorPassed -or !$r.fullSuiteFooter -or !$r.inputsUnchanged -or !$r.bytesUnchanged -or !$r.fixtureBytesUnchanged -or $r.ownerAttention -or $r.runs.Count -ne 1){throw 'Unexpected native result'}
$run=$r.runs[0]
if(!$run.passed -or !$run.originalHandleCaptured -or !$run.exitObserved -or !$run.childRetirementIndependentlyAudited -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.timedOut -or $run.killRequested -or $run.exitCode -ne 0 -or $run.parentBudgetMs -ne 300000 -or $run.elapsedMs -gt 300000){throw 'Native lifetime/output proof incomplete'}
if(!$acl.classified -or $acl.observations.Count -ne 3 -or $acl.filesystemAclModified -or $acl.productionPolicyBroadened -or $acl.profileDeletionProven){throw 'Read-only classifier proof incomplete'}
foreach($file in @($r.inputs)+@($r.before)+@($r.fixtureBefore)){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Recorded input bytes differ'}}
foreach($file in $r.before){
    $matches=@($acl.observations|Where-Object path -ceq $file.path)
    $expected=if($matches.Count -eq 1){$matches[0].after}else{$file.sddl}
    if($matches.Count -gt 1 -or (Get-Acl $file.path).Sddl -cne $expected){throw 'Current permissions differ from classified observation'}
}
$view=@($r.before|Where-Object{[IO.Path]::GetFileName($_.path) -ceq 'Lea.Win.dll'})
if($view.Count -ne 1 -or $view[0].sha256 -cne '2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC'){throw 'Wrong view candidate'}
$source=Join-Path $workspace 'continuity-evidence/nested-overlap-regression-v1/Smoke.cs'
$footer=[regex]::Matches([IO.File]::ReadAllText($source),'Console\.WriteLine\("(?<footer>Lea smoke passed: bounded sticky positioning with compact k,[^"\r\n]*)"\);')
$helper=Join-Path $PSScriptRoot 'Test-LeaSuiteFooter.ps1'
if($footer.Count -ne 1 -or (Get-FileHash $helper).Hash -cne 'BFD1DD8FC9DACBCA1E7177AA740F8D7EA8974E5B4C4BB344C1DCD16055EE47AE'){throw 'Footer source/reader differs'}
. $helper
if(!(Test-LeaSuiteFooter $run.stdout $footer[0].Groups['footer'].Value)){throw 'Exact terminal footer missing'}
@{nativeBehaviorPassed=$true;exactTerminalFooterVerified=$true;viewSha256=$view[0].sha256;elapsedMs=$run.elapsedMs;originalBudgetMs=300000;originalExitAndOwnedJobZero=$true;outputDrained=$true;inputsRuntimeAndFixturesHeld=$true;rawAuditPassed=$false;exactFixtureAclGrantsClassified=3;filesystemAclModified=$false;profileDeletionProven=$false;productionPolicyBroadened=$false;fullScriptHostOrReleaseQualified=$false;scope='One matched full native execution and read-only classification; not startup reliability, profile cleanup or adoption'}|ConvertTo-Json -Depth 4|Set-Content $out
Get-Content $out
