$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$out='I:/LeaBuildStorage/resource-bootstrap-audit-20261102-v1'
if(Test-Path $out){throw 'Preserve resource bootstrap audit'}
$source='I:/LeaBuildStorage/trusted-bootstrap-profile-source-20261102-v4/source.json'
$build='I:/LeaBuildStorage/trusted-bootstrap-profile-build-20261102-v4/result.json'
$native=Join-Path $root 'continuity-evidence/trusted-bootstrap-profile-v4/result.json'
foreach($pin in @(@($source,'8A80C461B25495B17862096469E0CC17AC061947380CB59019C47CB8136F60EC'),@($build,'8849F3956D4BC6D6E97E444BFB6451C58102515F5EA8FE3B07B9DA19011FDEEE'),@($native,'2B084ABE6DD48B6C3B7D5AE7CA5EA4EEAFAD5C09CAAADD888E837D04854A59FA'))){if((Get-FileHash $pin[0]).Hash -cne $pin[1]){throw 'Pinned resource receipt differs'}}
$s=Get-Content $source -Raw|ConvertFrom-Json
$b=Get-Content $build -Raw|ConvertFrom-Json
$n=Get-Content $native -Raw|ConvertFrom-Json
if(!$s.verified -or !$s.diagnosticOnly -or !$s.resourceCounters -or $s.telemetryNegativeControls -ne 3 -or $s.budgetsChanged -or !$s.criticalOperationsVerified -or !$b.passed -or !$b.inputsUnchanged -or $b.ownerAttention){throw 'Diagnostic build boundary differs'}
foreach($file in $s.files){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Diagnostic source changed'}}
if((Get-FileHash $b.binary).Hash -cne '7F2373FE748206C2CE66366F6BC35B43B9AEBC82B3B5DA3565FCCE47D0CCC338'){throw 'Diagnostic binary changed'}
if(!$n.passed -or !$n.diagnosticOnly -or $n.runtimeQualified -or !$n.measurementCaptured -or !$n.inputsUnchanged -or $n.ownerAttention -or $n.changedFiles.Count -or $n.before.Count -ne 10 -or $n.runs.Count -ne 2 -or !$n.report.ok){throw 'Diagnostic execution boundary differs'}
foreach($item in @($b.inputs)+@($n.inputs)){if((Get-FileHash $item.path).Hash -cne $item.sha256){throw 'Diagnostic input changed'}}
foreach($item in $n.before){if((Get-FileHash $item.path).Hash -cne $item.sha256 -or (Get-Acl $item.path).Sddl -cne $item.sddl){throw 'Diagnostic runtime byte/ACL drift'}}
foreach($run in @($b.runs)+@($n.runs)){
    if(!$run.passed -or !$run.ownedJob -or !$run.exitObserved -or !$run.originalHandleCaptured -or !$run.childRetirementIndependentlyAudited -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or $run.timedOut -or $run.killRequested -or $run.unsafeToContinue -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Diagnostic ownership differs'}
}
. (Join-Path $PSScriptRoot 'Read-TrustedBootstrapProfile.ps1')
. (Join-Path $PSScriptRoot 'Read-BootstrapResources.ps1')
$lines=Get-Content $n.runs[1].stderr
$legacy=Read-TrustedBootstrapProfile $lines -Detailed
$frequency=[long]([regex]::Match(($lines|Where-Object {$_.StartsWith('LEA_TRUSTED_BOOT ')}|Select-Object -First 1),'wallFrequency=([0-9]+)').Groups[1].Value)
$resources=Read-BootstrapResources $lines @($legacy.rows|ForEach-Object label) $frequency
if($resources.markers -ne 21){throw 'Full paired resource trace required'}
$test=Join-Path $root 'Lea.Tests/BootstrapResources.test.ps1'
$parserOutput=& pwsh -NoProfile -File $test
if($LASTEXITCODE -or $parserOutput -cne 'PASS resource parser: 9 cases'){throw 'Resource parser controls failed'}
New-Item -ItemType Directory -Path $out|Out-Null
$resources|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'resources.json') -Encoding utf8
$copies=[Collections.Generic.List[object]]::new();$i=0
foreach($path in @($source,$build,$native,$n.runs[0].stdout,$n.runs[0].stderr,$n.runs[1].stdout,$n.runs[1].stderr)){
    $to=Join-Path $out ((++$i).ToString('D2')+'-'+[IO.Path]::GetFileName($path));Copy-Item -LiteralPath $path -Destination $to
    $hash=(Get-FileHash $path).Hash;if((Get-FileHash $to).Hash -cne $hash){throw 'Diagnostic evidence readback mismatch'}
    $copies.Add(@{original=$path;copy=$to;sha256=$hash})
}
@{verified=$true;diagnosticOnly=$true;startupFixed=$false;releaseGateClosed=$false;runtimeQualified=$false;pairedMarkers=21;parserControls=9;allThreeOwnedRunsRetired=$true;runtimeFilesUnchanged=10;resourcesSha256=(Get-FileHash (Join-Path $out 'resources.json')).Hash;parserSha256=(Get-FileHash (Join-Path $PSScriptRoot 'Read-BootstrapResources.ps1')).Hash;testSha256=(Get-FileHash $test).Hash;copies=$copies}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
