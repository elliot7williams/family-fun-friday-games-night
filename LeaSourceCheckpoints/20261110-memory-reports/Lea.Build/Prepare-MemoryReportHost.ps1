$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$base='I:\LeaBuildStorage\accessibility-host-source-20261110-v1'
$out='I:\LeaBuildStorage\memory-report-host-source-20261110-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve memory host source'}
$planPath=Join-Path $base 'source.json'
if((Get-FileHash -LiteralPath $planPath).Hash -cne 'B9CD62FDD782B4549199B1A93590D589A5EF3E02DB6AFD11A1A6951ABDA891A9'){throw 'Host source baseline differs'}
$plan=Get-Content -LiteralPath $planPath -Raw|ConvertFrom-Json
$viewProof='I:\LeaBuildStorage\memory-document-view-audit-20261110-v1\audit.json'
if((Get-FileHash -LiteralPath $viewProof).Hash -cne 'C59876032BFC698B110F7B639A1CCF7FDB20B69D71325DEF1CB83F32618A50B5'){throw 'Memory view audit differs'}
$audit=Get-Content -LiteralPath $viewProof -Raw|ConvertFrom-Json
if(!$audit.verified -or $audit.modelChecks -ne 12 -or $audit.viewChecks -ne 17 -or !$audit.runtimeBytesAndAclsStable){throw 'Tested memory view required'}
foreach($file in $plan.files){if((Get-FileHash -LiteralPath (Join-Path $plan.source $file.path)).Hash -cne $file.sha256){throw 'Preserved host source drift'}}
$stage=Join-Path $out 'source'
New-Item -ItemType Directory -Path $stage|Out-Null
foreach($file in $plan.files){$to=Join-Path $stage $file.path;New-Item -ItemType Directory -Path (Split-Path $to -Parent) -Force|Out-Null;Copy-Item -LiteralPath (Join-Path $plan.source $file.path) -Destination $to;if((Get-FileHash -LiteralPath $to).Hash -cne $file.sha256){throw 'Host copy differs'}}
$oldRoute='I:\LeaBuildStorage\accessibility-host-build-20261110-v1\AccessibilityRoute.vb'
if((Get-FileHash -LiteralPath $oldRoute).Hash -cne '0BD787225A1991359A241403131C828ADB5136CBF4005AA634BD5A20C2C04A58'){throw 'Original route snapshot differs'}
$route=Join-Path $root 'Lea.Firespider/AccessibilityRoute.vb'
$adapter=Join-Path $stage 'FirespiderLea.vb';$original=[IO.File]::ReadAllText($adapter)
$old=[IO.File]::ReadAllText($oldRoute);$new=[IO.File]::ReadAllText($route)
if([regex]::Matches($original,[regex]::Escape($old)).Count -ne 1 -or $original.Contains($new)){throw 'Unique memory route projection required'}
$changed=$original.Replace($old,$new)
if($changed.Replace($new,$old) -cne $original){throw 'Route replacement inverse differs'}
[IO.File]::WriteAllText($adapter,$changed,[Text.UTF8Encoding]::new($false))
$runtime=Join-Path $root 'continuity-evidence/memory-document-view-v1/runtime'
foreach($pair in @(@('Lea.Engine.dll',$audit.coreSha256),@('Lea.Win.dll',$audit.viewSha256))){$from=Join-Path $runtime $pair[0];if((Get-FileHash -LiteralPath $from).Hash -cne $pair[1]){throw 'Memory library differs'};$to=Join-Path $stage ('Lea/'+$pair[0]);Copy-Item -LiteralPath $from -Destination $to;if((Get-FileHash -LiteralPath $to).Hash -cne $pair[1]){throw 'Library copy differs'}}
$rows=@(foreach($file in $plan.files){$hash=(Get-FileHash -LiteralPath (Join-Path $stage $file.path)).Hash;@{path=$file.path;sha256=$hash;baselineSha256=$file.baselineSha256;changed=$hash -cne $file.baselineSha256}})
if($rows.Count -ne 105 -or @($rows|Where-Object changed).Count -ne 4){throw 'Exact four-member candidate change required'}
$fragments=@(foreach($name in @('AccessibilityReport.vb','AccessibilityRoute.vb')){$p=Join-Path $root ('Lea.Firespider/'+$name);@{path=$p;sha256=(Get-FileHash -LiteralPath $p).Hash}})
@{verified=$true;memoryReport=$true;source=$stage;files=$rows;fragments=$fragments;routeInverseVerified=$true;adapterPrefixUnchanged=$true;baselineSourceSha256=(Get-FileHash -LiteralPath $planPath).Hash;memoryViewAuditSha256=(Get-FileHash -LiteralPath $viewProof).Hash;compiled=$false;runtimeVerified=$false;installedBrowserChanged=$false;completeSdkRebuilt=$false}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'source.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'source.json')
