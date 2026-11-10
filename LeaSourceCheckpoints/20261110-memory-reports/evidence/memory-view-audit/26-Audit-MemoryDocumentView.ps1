$ErrorActionPreference='Stop'
$base='I:\LeaBuildStorage\memory-document-view-20261110-v1'
$out='I:\LeaBuildStorage\memory-document-view-audit-20261110-v1'
$root=Split-Path $PSScriptRoot -Parent
if(Test-Path -LiteralPath $out){throw 'Preserve memory view audit'}
$receipt=Join-Path $base 'result.json'
if((Get-FileHash -LiteralPath $receipt).Hash -cne '436078461C4F889612B6AD1139FB255C3105E8E7F6BEB6851FA7396F2FF2108A'){throw 'Memory view receipt differs'}
$r=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.runs.Count -ne 6 -or $r.before.Count -ne 3 -or $r.byteChanges.Count -or $r.aclChanges.Count -or $r.hostIntegrated -or $r.privacyFixed){throw 'Memory view qualification boundary incomplete'}
$budgets=@{'compile-memory-core'=120000;'compile-memory-model'=60000;'memory-model'=30000;'compile-memory-view'=120000;'compile-memory-view-probe'=60000;'memory-view'=30000}
$seen=[Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach($run in $r.runs){if(!$seen.Add($run.name) -or !$budgets.ContainsKey($run.name) -or !$run.passed -or $run.exitCode -ne 0 -or !$run.ownedJob -or !$run.originalHandleCaptured -or !$run.exitObserved -or !$run.childRetirementIndependentlyAudited -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.timedOut -or $run.killRequested -or $run.unsafeToContinue -or $run.parentBudgetMs -ne $budgets[$run.name] -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Owned memory view run differs'}}
foreach($f in $r.inputs){if((Get-FileHash -LiteralPath $f.path).Hash -cne $f.sha256){throw 'Memory view input drift'}}
foreach($f in $r.before){if((Get-FileHash -LiteralPath $f.path).Hash -cne $f.sha256 -or (Get-Acl -LiteralPath $f.path).Sddl -cne $f.sddl){throw 'Memory view runtime drift'}}
foreach($pair in @(@('memory-model','PASS Lea memory documents: 12 model checks'),@('memory-view','PASS Lea memory view: 17 checks at720/360; no HTML in isolated TEMP'))){$run=@($r.runs|Where-Object name -CEQ $pair[0]);if(@((Get-Content -LiteralPath $run[0].stdout)|Where-Object {$_ -ceq $pair[1]}).Count -ne 1){throw 'Memory check footer missing'}}
$runtime=Join-Path $root 'continuity-evidence/memory-document-view-v1/runtime'
if((Get-FileHash -LiteralPath (Join-Path $runtime 'Lea.Engine.dll')).Hash -cne $r.coreSha256 -or (Get-FileHash -LiteralPath (Join-Path $runtime 'Lea.Win.dll')).Hash -cne $r.viewSha256){throw 'Memory product identity differs'}
$temp=Join-Path (Split-Path $runtime -Parent) 'private-temp'
if(@(Get-ChildItem -LiteralPath $temp -Recurse -File -Filter '*.html').Count){throw 'Unexpected report HTML in isolated TEMP'}
New-Item -ItemType Directory -Path $out|Out-Null
$paths=@((Get-ChildItem -LiteralPath $base -File|Select-Object -ExpandProperty FullName))
$paths+=@('Lea.Engine/MemoryDocument.fragment.cs','Lea.Win/MemoryDocument.fragment.cs','Lea.Tests/MemoryDocumentMain.cs','Lea.Tests/MemoryDocumentViewMain.cs','Lea.Build/Test-OwnedMemoryDocument.ps1','Lea.Build/Audit-MemoryDocumentView.ps1')|ForEach-Object {Join-Path $root $_}
$paths+=@(720,360)|ForEach-Object {Join-Path (Split-Path $runtime -Parent) ('memory-view-'+$_+'.png')}
$copies=@();$i=0
foreach($from in $paths){$to=Join-Path $out ((++$i).ToString('D2')+'-'+[IO.Path]::GetFileName($from));Copy-Item -LiteralPath $from -Destination $to;$h=(Get-FileHash -LiteralPath $from).Hash;if((Get-FileHash -LiteralPath $to).Hash -cne $h){throw 'Evidence copy differs'};$copies+=@{path=$to;sha256=$h}}
@{verified=$true;coreSha256=$r.coreSha256;viewSha256=$r.viewSha256;modelChecks=12;viewChecks=17;ownedRuns=6;runtimeBytesAndAclsStable=$true;isolatedTempHtmlAbsent=$true;hostIntegrated=$false;privateBrowserDefectFixed=$false;globalResourceConfinementQualified=$false;releaseQualified=$false;copies=$copies}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'audit.json')
