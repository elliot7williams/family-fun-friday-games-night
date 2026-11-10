param([Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$ReceiptSha256)
$ErrorActionPreference='Stop'
$folder='I:\LeaBuildStorage\sdk-print-source-graph-20261109-v1'
$out='I:\LeaBuildStorage\sdk-print-source-graph-audit-20261109-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve print source audit'}
$receipt=Join-Path $folder 'result.json'
if((Get-FileHash -LiteralPath $receipt).Hash -cne $ReceiptSha256){throw 'Print graph receipt differs'}
$r=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.oldDependencyFailureReproduced -or !$r.explicitBinaryPathsPreserved -or !$r.capturePassed -or $r.rangeChecks -ne 102 -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.error -or $r.changedFiles.Count -or $r.products.Count -ne 6 -or $r.runs.Count -ne 8 -or $r.before.Count -ne 14 -or $r.testHarnessesRecompiled -or $r.productBinariesSeeded -or $r.completeSdk -or $r.startupFixed -or $r.hostAdopted -or $r.scriptWorkerQualified -or $r.printerDispatched){throw 'Print graph scope differs'}
if(!$r.friendly.passed -or $r.friendly.baselineChecks -ne 40 -or $r.friendly.friendlyChecks -ne 46 -or $r.friendly.browserRouted -or $r.friendly.printJobSubmitted -or !$r.live.passed -or $r.live.liveChecks -ne 250 -or $r.live.printJobSubmitted -or $r.live.hostRouted){throw 'Print live scope differs'}
foreach($file in $r.inputs){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw ('Input drift: '+$file.path)}}
foreach($file in $r.before){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256 -or (Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Runtime byte/ACL drift'}}
for($i=0;$i -lt $r.runs.Count;$i++){
    $run=$r.runs[$i];$budget=if($i -lt 5){120000}else{30000}
    if($run.timedOut -or $run.killRequested -or !$run.ownedJob -or !$run.originalHandleCaptured -or !$run.childRetirementIndependentlyAudited -or !$run.exitObserved -or !$run.outputDrained -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.captureOverflow -or $run.error){throw 'Ownership/deadline differs'}
    if($i -eq 0){if($run.exitCode -eq 0 -or $run.passed -or (Get-Content -LiteralPath $run.stdout -Raw) -notmatch 'Set LeaCorePath to the matching mature Lea.Engine.dll'){throw 'Old binary-only failure control differs'}}
    elseif(!$run.passed -or $run.exitCode -ne 0){throw 'Positive source/runtime case failed'}
}
foreach($pair in @(@($r.runs[5].stdout,'PASS Lea live styled capture and foundation'),@($r.runs[6].stdout,'PASS Lea print range: 37'),@($r.runs[6].stdout,'PASS Lea printer geometry: 16'),@($r.runs[6].stdout,'PASS Lea native printer selection: 27'),@($r.runs[6].stdout,'PASS Lea prepared printer ticket: 22'))){if(@(Get-Content -LiteralPath $pair[0]|Where-Object {$_ -ceq $pair[1]}).Count -ne 1){throw 'Terminal footer differs'}}
$source=Join-Path $folder 'source'
foreach($product in $r.products){if((Get-FileHash -LiteralPath $product.path).Hash -cne $product.sha256){throw 'Producer product differs'}}
$runtime=Split-Path $r.before[0].path -Parent
foreach($name in @('Lea.Engine.dll','Lea.Win.dll','Lea.Print.dll','Lea.Image.exe')){
    $product=@($r.products|Where-Object name -CEQ $name)
    if($product.Count -ne 1 -or (Get-FileHash -LiteralPath (Join-Path $runtime $name)).Hash -cne $product[0].sha256){throw 'Runtime is not source producer output'}
}
$sourceFiles=@(foreach($file in Get-ChildItem -LiteralPath $source -Recurse -File){$relative=[IO.Path]::GetRelativePath($source,$file.FullName).Replace('\','/');if($relative -match '(?i)(?:^|/)(?:bin|obj|lib)/'){continue};if($file.Extension -notin @('.cs','.csproj','.targets','.js','.html')){throw 'Unexpected source member'};@{path=$relative;sha256=(Get-FileHash -LiteralPath $file.FullName).Hash}})
if($sourceFiles.Count -ne 98){throw 'Mature source coverage differs'}
$priorAudit='I:\LeaBuildStorage\sdk-image-source-graph-audit-20261109-v1\audit.json'
if((Get-FileHash -LiteralPath $priorAudit).Hash -cne '6C654D97E50047ABEE9BB2F3FA6C3030B0732B0596F74127E4572B644DE12A4E'){throw 'Prior source inventory differs'}
foreach($file in (Get-Content -LiteralPath $priorAudit -Raw|ConvertFrom-Json).sourceFiles){if((Get-FileHash -LiteralPath (Join-Path $source $file.path)).Hash -cne $file.sha256){throw 'Preceding source changed'}}
$printSource=Join-Path (Split-Path (Split-Path $runtime -Parent) -Parent) 'relative-font-print-project-v1/source'
foreach($file in Get-ChildItem -LiteralPath (Join-Path $source 'Lea.Print') -Filter '*.cs'){
    $old=Join-Path $printSource $file.Name
    if((Get-FileHash -LiteralPath $old).Hash -cne (Get-FileHash -LiteralPath $file.FullName).Hash){throw 'Print behavior source changed'}
}
$project=Join-Path $source 'Lea.Print/Lea.Print.csproj'
$xml=[xml](Get-Content -LiteralPath $project -Raw)
$edges=@($xml.SelectNodes('//*[local-name()="ProjectReference"]')|ForEach-Object {$_.Include})
if($edges.Count -ne 2 -or '..\Lea.Engine\Lea.Engine.csproj' -cnotin $edges -or '..\Lea.Win\Lea.Win.csproj' -cnotin $edges){throw 'Print source dependency graph differs'}
New-Item -ItemType Directory -Path $out|Out-Null
$copies=@(foreach($file in Get-ChildItem -LiteralPath $folder -File|Where-Object {$_.Extension -in @('.json','.txt','.ps1')}){Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $out $file.Name);$hash=(Get-FileHash -LiteralPath $file.FullName).Hash;if((Get-FileHash -LiteralPath (Join-Path $out $file.Name)).Hash -cne $hash){throw 'Raw evidence copy differs'};@{path=(Join-Path $out $file.Name);sha256=$hash}})
$artifacts=@(Get-ChildItem -LiteralPath $runtime -File|Where-Object Extension -in @('.png','.xps','.json')|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;bytes=$_.Length}})
@{verified=$true;receiptSha256=$ReceiptSha256;ownedRuns=8;rangeChecks=102;baselineChecks=40;friendlyChecks=46;liveChecks=250;capturePassed=$true;sourceFiles=$sourceFiles;products=$r.products;copies=$copies;artifacts=$artifacts;testHarnessesRecompiled=$false;completeSdk=$false;startupFixed=$false;hostAdopted=$false;scriptWorkerQualified=$false;printerDispatched=$false;scriptSha256=(Get-FileHash -LiteralPath $PSCommandPath).Hash;scope='Clean direct print source graph, explicit binary references, unchanged pinned print harnesses on new source-built runtime; no physical print job or release qualification.'}|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'audit.json')|Format-List Hash
