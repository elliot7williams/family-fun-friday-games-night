$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$folder='I:\LeaBuildStorage\sdk-image-source-graph-20261109-v1'
$out='I:\LeaBuildStorage\sdk-image-source-graph-audit-20261109-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve image graph audit'}
function Read-Pinned([string]$Path,[string]$Hash){if((Get-FileHash -LiteralPath $Path).Hash -cne $Hash){throw ('Receipt differs: '+$Path)};return Get-Content -LiteralPath $Path -Raw|ConvertFrom-Json}
$g=Read-Pinned (Join-Path $folder 'result.json') '177FDA95290C6AEC605325072AEB598CC7A39A5F552910BA3FD1083DD093648F'
$r=Read-Pinned (Join-Path $root 'continuity-evidence/current-image-source-decoder-retirement-v1/result.json') '2B4E25C2043E047F53ED953EC0FF5F66DAD3B7F14EB627C36735B25E42C24CD1'
$s=Read-Pinned (Join-Path $root 'continuity-evidence/current-image-source-decoder-shutdown-race-v1/result.json') 'A1F6D0459DA0C59AF142824AED4243076373BE2D7C8EEF85D9D912A342A05E28'
if(!$g.passed -or !$g.oldWorkerOmissionReproduced -or !$g.sourceSampleCopiesVerified -or !$g.binaryConsumerCopiesWorker -or $g.textChecks -ne 14 -or $g.imageChecks -ne 42 -or $g.products.Count -ne 5 -or $g.before.Count -ne 6 -or $g.runs.Count -ne 7 -or $g.completeSdk -or $g.startupFixed -or $g.scriptWorkerQualified -or $g.hostAdopted -or $g.productBinariesSeeded){throw 'Image graph scope differs'}
foreach($state in @($g,$r,$s)){
    if(!$state.passed -or !$state.inputsUnchanged -or $state.ownerAttention -or $state.error -or $state.changedFiles.Count){throw 'Unresolved test state'}
    foreach($input in $state.inputs){if((Get-FileHash -LiteralPath $input.path).Hash -cne $input.sha256){throw 'Test input changed'}}
    foreach($file in $state.before){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256 -or (Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Image runtime byte/ACL drift'}}
}
foreach($state in @($r,$s)){
    if(!$state.imageSourceGraph -or !$state.decoderAclRestored -or $state.decoderProfileChanged -or $state.browserAdopted -or $state.runs.Count -ne 2){throw 'Decoder lifecycle scope differs'}
    $worker=Join-Path (Split-Path $state.runs[0].stdout -Parent) 'Lea.Image.exe'
    if((Get-Acl -LiteralPath $worker).Sddl -cne $state.decoderAclBefore -or (Get-FileHash -LiteralPath $worker).Hash -cne 'DA3BFA671C54EEFD78092A9B91CB080189D8681BF6433C2F9C75AC40118D4E91'){throw 'Decoder worker identity/descriptor differs'}
}
$runs=@($g.runs)+@($r.runs)+@($s.runs)
for($i=0;$i -lt $runs.Count;$i++){
    $run=$runs[$i];$budget=if($i -in @(5,6,8,10)){30000}else{120000}
    if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.ownedJob -or !$run.originalHandleCaptured -or !$run.childRetirementIndependentlyAudited -or !$run.exitObserved -or !$run.outputDrained -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.captureOverflow -or $run.error){throw 'Owned run/deadline differs'}
}
foreach($pair in @(@($g.runs[5].stdout,'PASS LEA SDK EMBEDDING 14'),@($g.runs[6].stdout,'PASS SDK VIEW IMAGES 42'),@($r.runs[1].stdout,'Decoder retirement passed: 81'),@($s.runs[1].stdout,'Decoder shutdown race passed: 14'))){if(@(Get-Content -LiteralPath $pair[0]|Where-Object {$_ -ceq $pair[1]}).Count -ne 1){throw 'Contract footer differs'}}
$source=Join-Path $folder 'source'
foreach($product in $g.products){if((Get-FileHash -LiteralPath $product.path).Hash -cne $product.sha256){throw 'Producer output differs'}}
foreach($name in @('Lea.Engine.dll','Lea.Win.dll','Lea.Image.exe')){
    $hash=(Get-FileHash -LiteralPath (Join-Path $source ('bin/Release/'+$name))).Hash
    foreach($base in @((Join-Path $source 'Sample/bin/Release'),(Join-Path $folder 'binary-consumer/Sample/bin/Release'))){if((Get-FileHash -LiteralPath (Join-Path $base $name)).Hash -cne $hash){throw 'Consumer deployed component differs'}}
}
if(Test-Path -LiteralPath (Join-Path $folder 'old-consumer/Sample/bin/Release/Lea.Image.exe')){throw 'Old omission control changed'}
$xml=[xml](Get-Content -LiteralPath (Join-Path $source 'Sample/EmbeddingSample.csproj') -Raw)
$image=@($xml.SelectNodes('//*[local-name()="ProjectReference"]')|Where-Object Include -CEQ '..\Lea.Image\Lea.Image.csproj')
if($image.Count -ne 1 -or $image[0].ReferenceOutputAssembly -cne 'false'){throw 'Decoder must remain a separate executable, not a managed UI reference'}
if((Get-FileHash -LiteralPath (Join-Path $source 'Lea.Image/Mg.cs')).Hash -cne 'CDF6E334CF99742BC9F3427ECE57B14AB39A5433141040ED42120F43B7FA83F1' -or (Get-FileHash -LiteralPath (Join-Path $source 'Lea.Image/Program.cs')).Hash -cne '4B0EBB73E7ED355373DD75D45A83B6D160900F0BCB93F79536F4E7C5BC83DA44'){throw 'Original decoder source changed'}
$sourceFiles=@(foreach($file in Get-ChildItem -LiteralPath $source -Recurse -File){$relative=[IO.Path]::GetRelativePath($source,$file.FullName).Replace('\','/');if($relative -match '(?i)(?:^|/)(?:bin|obj|lib)/'){continue};if($file.Extension -notin @('.cs','.csproj','.targets','.js','.html')){throw 'Unexpected source member'};@{path=$relative;sha256=(Get-FileHash -LiteralPath $file.FullName).Hash}})
if($sourceFiles.Count -ne 75){throw 'Source coverage differs'}
New-Item -ItemType Directory -Path $out|Out-Null
$copies=@(foreach($spec in @(@{folder=$folder;prefix='graph'},@{folder=(Split-Path $r.runs[0].stdout -Parent);prefix='retirement'},@{folder=(Split-Path $s.runs[0].stdout -Parent);prefix='race'})){foreach($file in Get-ChildItem -LiteralPath $spec.folder -File|Where-Object {$_.Extension -in @('.json','.txt','.rsp','.ps1','.cs','.html')}){$to=Join-Path $out ($spec.prefix+'-'+$file.Name);Copy-Item -LiteralPath $file.FullName -Destination $to;$hash=(Get-FileHash -LiteralPath $file.FullName).Hash;if((Get-FileHash -LiteralPath $to).Hash -cne $hash){throw 'Evidence readback differs'};@{path=$to;sha256=$hash}}})
$screenshots=@(Get-ChildItem -LiteralPath (Join-Path $folder 'assets') -Filter '*.png'|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash}})
@{verified=$true;ownedRuns=11;textChecks=14;imageChecks=42;retirementChecks=81;raceChecks=14;sourceFiles=$sourceFiles;products=$g.products;copies=$copies;screenshots=$screenshots;workerAclRestored=$true;completeSdk=$false;universalSecurityQualified=$false;startupFixed=$false;scriptWorkerQualified=$false;hostAdopted=$false;scriptSHA256=(Get-FileHash -LiteralPath $PSCommandPath).Hash;scope='Current source build, deployment compatibility, file/data PNG rendering and decoder lifecycle fixtures. Not all image formats, broad site/security/printing/script-worker/host or clean-machine qualification.'}|ConvertTo-Json -Depth 9|Set-Content -LiteralPath (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'audit.json')|Format-List Hash
