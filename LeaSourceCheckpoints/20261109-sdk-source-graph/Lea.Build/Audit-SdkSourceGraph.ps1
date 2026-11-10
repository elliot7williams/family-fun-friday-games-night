$ErrorActionPreference='Stop'
$graph='I:\LeaBuildStorage\sdk-source-graph-20261109-v2'
$contracts='I:\LeaBuildStorage\sdk-source-graph-contracts-20261109-v1'
$out='I:\LeaBuildStorage\sdk-source-graph-audit-20261109-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve graph audit'}
function Read-Pinned([string]$Path,[string]$Hash){if((Get-FileHash -LiteralPath $Path).Hash -cne $Hash){throw ('Receipt differs: '+$Path)};return Get-Content -LiteralPath $Path -Raw|ConvertFrom-Json}
$g=Read-Pinned (Join-Path $graph 'result.json') 'E9F481961EDB427F255715F51F28F4ED0E9D675EEA79C10D5E97B772C339A8BF'
$c=Read-Pinned (Join-Path $contracts 'result.json') '71742BFD3760DF59952F4C7139CA562657F0600D32540F840CC80703E37D34D1'
$n=Read-Pinned 'I:\LeaBuildStorage\sdk-source-graph-20261109-v1\result.json' 'F201D5EF8C4C7C9FEA719D63AFCB73AE604786B12B01E84216ED4A2584DF1ACF'
if(!$g.passed -or !$g.originalDefectReproduced -or !$g.sourceGraphCompiled -or !$g.binaryModeCompiled -or !$g.embeddingPassed -or $g.productBinariesUsedForSourceBuild -or $g.outputs.Count -ne 4 -or $g.before.Count -ne 4 -or $g.runs.Count -ne 4 -or $g.completeSdk -or $g.startupFixed -or $g.hostAdopted){throw 'Graph scope differs'}
if(!$c.passed -or $c.componentChecks -ne 14 -or $c.fontChecks -ne 28 -or $c.scriptExecuted -or $c.nativeWorkerQualified -or $c.completeSdk -or $c.hostAdopted -or $c.before.Count -ne 12 -or $c.runs.Count -ne 4){throw 'Contract scope differs'}
foreach($r in @($g,$c)){
    if(!$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.error){throw 'Unresolved execution state'}
    foreach($input in $r.inputs){if((Get-FileHash -LiteralPath $input.path).Hash -cne $input.sha256){throw 'Source/recipe input changed'}}
    foreach($file in $r.before){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256 -or (Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Runtime bytes/ACL changed'}}
}
$positive=@($g.runs)+@($c.runs)
for($i=0;$i -lt $positive.Count;$i++){
    $run=$positive[$i];$budget=if($i -in @(3,6,7)){30000}else{120000}
    if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.originalHandleCaptured -or !$run.ownedJob -or !$run.childRetirementIndependentlyAudited -or !$run.exitObserved -or !$run.outputDrained -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.captureOverflow -or $run.error){throw 'Positive ownership/deadline differs'}
}
$negative=$n.runs[0]
if($n.passed -or !$n.inputsUnchanged -or $n.ownerAttention -or $n.runs.Count -ne 1 -or $negative.exitCode -ne 1 -or $negative.passed -or $negative.timedOut -or $negative.killRequested -or !$negative.ownedJob -or !$negative.originalHandleCaptured -or !$negative.childRetirementIndependentlyAudited -or !$negative.exitObserved -or !$negative.outputDrained -or !$negative.stdoutComplete -or !$negative.stderrComplete -or $negative.activeAfterRetirement -ne 0 -or $negative.parentBudgetMs -ne 120000 -or $negative.sharedRetirementDrainBudgetMs -ne 5000 -or $negative.retirementAndDrainElapsedMs -gt 5000 -or $negative.error){throw 'Negative control ownership differs'}
$negativeText=Get-Content -LiteralPath $negative.stdout -Raw
if($negativeText -notmatch 'error CS0234:.*Engine.*Lea' -or $negativeText -notmatch 'error CS0246' -or (Get-Item -LiteralPath $negative.stdout).Length -ne $negative.stdoutBytes){throw 'Missing dependency control differs'}
foreach($module in @('Lea.Script','Lea.Script.Win')){
    $xml=[xml](Get-Content -LiteralPath (Join-Path $graph ('source/'+$module+'/'+$module+'.csproj')) -Raw)
    $projects=@($xml.SelectNodes('//*[local-name()="ProjectReference"]'))
    $expected=if($module -eq 'Lea.Script'){@('Lea.Engine')}else{@('Lea.Engine','Lea.Win','Lea.Script')}
    if($projects.Count -ne $expected.Count){throw 'Source dependency graph differs'}
    foreach($dependency in $expected){if(@($projects|Where-Object {$_.Include -ceq ('..\'+$dependency+'\'+$dependency+'.csproj')}).Count -ne 1){throw 'Source project reference missing'}}
}
if(!(Test-Path -LiteralPath (Join-Path $graph 'binary/Lea.Engine/SourceGraphPoison.cs')) -or (Test-Path -LiteralPath (Join-Path $graph 'source/Lea.Engine/SourceGraphPoison.cs'))){throw 'Binary-mode poison isolation differs'}
foreach($entry in $g.outputs){if((Get-FileHash -LiteralPath $entry.path).Hash -cne $entry.sha256){throw 'Compiled product differs'}}
foreach($pair in @(@($g.runs[3].stdout,'PASS LEA SDK EMBEDDING 14'),@($c.runs[2].stdout,'PASS SDK SOURCE CONTRACT 14'),@($c.runs[3].stdout,'FONT_RELATIVE controls=10 cases=18 failures=0'))){if(@(Get-Content -LiteralPath $pair[0]|Where-Object {$_ -ceq $pair[1]}).Count -ne 1){throw 'Expected contract footer differs'}}
New-Item -ItemType Directory -Path $out|Out-Null
$copies=@(foreach($spec in @(@{folder=$graph;prefix='graph'},@{folder=$contracts;prefix='contracts'})){foreach($file in Get-ChildItem -LiteralPath $spec.folder -File|Where-Object {$_.Extension -in @('.json','.txt','.rsp','.ps1','.cs','.html','.png')} ){$to=Join-Path $out ($spec.prefix+'-'+$file.Name);Copy-Item -LiteralPath $file.FullName -Destination $to;$h=(Get-FileHash -LiteralPath $file.FullName).Hash;if((Get-FileHash -LiteralPath $to).Hash -cne $h){throw 'Audit copy readback differs'};@{path=$to;sha256=$h}}})
$sourceRoot=Join-Path $graph 'source'
$sourceFiles=@(foreach($file in Get-ChildItem -LiteralPath $sourceRoot -Recurse -File){$relative=[IO.Path]::GetRelativePath($sourceRoot,$file.FullName).Replace('\','/');if($relative -match '(?i)(?:^|/)(?:bin|obj|lib)/'){continue};if($file.Extension -notin @('.cs','.csproj','.targets','.js','.html')){throw 'Unexpected source export file'};@{path=$relative;sha256=(Get-FileHash -LiteralPath $file.FullName).Hash}})
if($sourceFiles.Count -ne 72){throw 'Current four-library source inventory differs'}
@{verified=$true;sourceGraphFixed=$true;positiveRuns=8;negativeRuns=1;embeddingChecks=14;componentChecks=14;fontChecks=28;outputs=$g.outputs;sourceFiles=$sourceFiles;copies=$copies;completeSdk=$false;startupFixed=$false;scriptExecutionQualified=$false;nativeWorkerQualified=$false;hostAdopted=$false;sourceArchiveIncludesDependencies=$false;scriptSHA256=(Get-FileHash -LiteralPath $PSCommandPath).Hash;scope='Fresh four-library source graph plus explicit binary-mode compatibility and focused consumers. No full scripting/worker/printing/Firespider/clean-machine or notices qualification.'}|ConvertTo-Json -Depth 9|Set-Content -LiteralPath (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'audit.json')|Format-List Hash
