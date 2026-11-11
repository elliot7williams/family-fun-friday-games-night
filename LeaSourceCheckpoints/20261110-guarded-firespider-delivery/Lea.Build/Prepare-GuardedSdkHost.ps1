$ErrorActionPreference='Stop'
$base='I:\LeaBuildStorage\memory-report-host-source-20261110-v1'
$out='I:\LeaBuildStorage\guarded-sdk-host-source-20261110-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve guarded host source'}
$basePath=Join-Path $base 'source.json'
if((Get-FileHash -LiteralPath $basePath).Hash -cne '712CD05FF0D4FDB864EF347A88D12F7EAF4BCF099451C7345A91CA4AC3903A4B'){throw 'Memory host source differs'}
$sdkPath='I:\LeaBuildStorage\memory-policy-sdk-audit-20261110-v1\audit.json'
$fullPath='I:\LeaBuildStorage\memory-policy-full-outcome-20261110-v1\audit.json'
foreach($p in @(@($sdkPath,'FD6BBBC7479A5F26F983E180D0C4BBA1EE75582C28F1F692F3D61C63DE52B3DC'),@($fullPath,'D4F27CA15CA471D79AD33078752890321E0CC8BEAD83CA198FBCF85601F5A2BA'))){if((Get-FileHash -LiteralPath $p[0]).Hash -cne $p[1]){throw 'Qualified SDK proof differs'}}
$sdk=Get-Content -LiteralPath $sdkPath -Raw|ConvertFrom-Json
$full=Get-Content -LiteralPath $fullPath -Raw|ConvertFrom-Json
if(!$sdk.verified -or $sdk.libraryProducts.Count -ne 6 -or $sdk.nativeProducts.Count -ne 6 -or !$full.fullSuitePassed -or !$full.runtimeBytesAndAclUnchanged -or !$full.traceEnvironmentRestored -or $full.completedPhases -ne 89 -or $full.quarantineRequired){throw 'Full qualified SDK required'}
$plan=Get-Content -LiteralPath $basePath -Raw|ConvertFrom-Json
if(!$plan.verified -or !$plan.memoryReport -or $plan.files.Count -ne 105){throw 'Preserved memory host closure required'}
$stage=Join-Path $out 'source'
foreach($file in $plan.files){$from=Join-Path $plan.source $file.path;if((Get-FileHash -LiteralPath $from).Hash -cne $file.sha256){throw 'Preserved host source drift'};$to=Join-Path $stage $file.path;New-Item -ItemType Directory -Path (Split-Path $to -Parent) -Force|Out-Null;Copy-Item -LiteralPath $from -Destination $to;if((Get-FileHash -LiteralPath $to).Hash -cne $file.sha256){throw 'Host source copy differs'}}
$bin='I:\LeaBuildStorage\memory-policy-sdk-source-20261110-v1\source\bin\Release'
$products=@(foreach($p in @($sdk.libraryProducts)+@($sdk.nativeProducts)){
 if((Get-FileHash -LiteralPath $p.path).Hash -cne $p.sha256){throw 'SDK product drift'}
 $relative=[IO.Path]::GetRelativePath($bin,$p.path).Replace('\','/')
 if([IO.Path]::IsPathRooted($relative) -or $relative.Contains(':') -or $relative.Split('/') -contains '..'){throw 'SDK product scope escape'}
 $target='Lea/'+$relative
 if(@($plan.files|Where-Object path -CEQ $target).Count -ne 1){throw 'Unexpected host SDK role'}
 Copy-Item -LiteralPath $p.path -Destination (Join-Path $stage $target)
 if((Get-FileHash -LiteralPath (Join-Path $stage $target)).Hash -cne $p.sha256){throw 'SDK binding copy differs'}
 @{path=$target;origin=$p.path;sha256=$p.sha256}
})
$rows=@(foreach($f in $plan.files){$hash=(Get-FileHash -LiteralPath (Join-Path $stage $f.path)).Hash;@{path=$f.path;sha256=$hash;baselineSha256=$f.baselineSha256;changed=$hash -cne $f.baselineSha256}})
if(@($rows|Where-Object changed).Count -ne 4 -or $products.Count -ne 12){throw 'Exact guarded host scope differs'}
@{verified=$true;guardedSourceSdk=$true;memoryReport=$true;source=$stage;files=$rows;fragments=$plan.fragments;sdkProducts=$products;routeInverseVerified=$plan.routeInverseVerified;adapterPrefixUnchanged=$plan.adapterPrefixUnchanged;fullSuiteAuditSha256=(Get-FileHash -LiteralPath $fullPath).Hash;inputs=@(@{path=$basePath;sha256=(Get-FileHash -LiteralPath $basePath).Hash},@{path=$sdkPath;sha256=(Get-FileHash -LiteralPath $sdkPath).Hash},@{path=$fullPath;sha256=(Get-FileHash -LiteralPath $fullPath).Hash});compiled=$false;runtimeQualified=$false;installedBrowserChanged=$false}|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $out 'source.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'source.json')
