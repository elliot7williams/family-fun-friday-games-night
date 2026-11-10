$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$base='I:\LeaBuildStorage\current-firespider-build-20261109-v1'
$out='I:\LeaBuildStorage\accessibility-host-source-20261110-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve host source attempt'}
$planPath=Join-Path $base 'candidate-manifest.json'
$plan=Get-Content -LiteralPath $planPath -Raw|ConvertFrom-Json
if($plan.files.Count -ne 105 -or !$plan.hostSourceUnchanged){throw 'Exact host closure required'}
foreach($file in $plan.files){if((Get-FileHash -LiteralPath (Join-Path $base ('source/'+$file.path))).Hash -cne $file.sha256){throw 'Baseline source drift'}}
$stage=Join-Path $out 'source'
New-Item -ItemType Directory -Path $stage|Out-Null
foreach($file in $plan.files){$from=Join-Path $base ('source/'+$file.path);$to=Join-Path $stage $file.path;New-Item -ItemType Directory -Path (Split-Path $to -Parent) -Force|Out-Null;Copy-Item -LiteralPath $from -Destination $to;if((Get-FileHash -LiteralPath $to).Hash -cne $file.sha256){throw 'Source copy differs'}}
$feature=Join-Path $stage 'BrowserFeatures2.vb'
$original=[IO.File]::ReadAllText($feature)
$marker='    Sub q870(ByVal sender As Object, ByVal e As EventArgs)'
if([regex]::Matches($original,[regex]::Escape($marker)).Count -ne 1){throw 'Unique report route boundary required'}
$start=$original.IndexOf($marker,[StringComparison]::Ordinal)
$insert=$original.IndexOf('        Try',$start,[StringComparison]::Ordinal)+'        Try'.Length
if($insert -lt $start){throw 'Report try boundary missing'}
$hook=[Environment]::NewLine+'            If qleaaccessroute() Then Return'
$changed=$original.Insert($insert,$hook)
if($changed.Remove($insert,$hook.Length) -cne $original){throw 'Route inverse differs'}
[IO.File]::WriteAllText($feature,$changed,[Text.UTF8Encoding]::new($false))
$adapter=Join-Path $stage 'FirespiderLea.vb'
$adapterOriginal=[IO.File]::ReadAllText($adapter)
$fragments=@('AccessibilityReport.vb','AccessibilityRoute.vb')
$extension=[Environment]::NewLine+'Partial Class j'+[Environment]::NewLine
$fragmentRows=@(foreach($name in $fragments){$path=Join-Path $root ('Lea.Firespider/'+$name);$extension += [IO.File]::ReadAllText($path)+[Environment]::NewLine;@{path=$path;sha256=(Get-FileHash -LiteralPath $path).Hash}})
$extension+='End Class'+[Environment]::NewLine
[IO.File]::WriteAllText($adapter,$adapterOriginal+$extension,[Text.UTF8Encoding]::new($false))
if([IO.File]::ReadAllText($adapter).Substring(0,$adapterOriginal.Length) -cne $adapterOriginal){throw 'Original adapter changed'}
$rows=@(foreach($file in $plan.files){$path=Join-Path $stage $file.path;$hash=(Get-FileHash -LiteralPath $path).Hash;@{path=$file.path;sha256=$hash;baselineSha256=$file.sha256;changed=$hash -cne $file.sha256}})
if(@($rows|Where-Object changed).Count -ne 2){throw 'Only two source changes permitted'}
@{verified=$true;source=$stage;baselineManifestSha256=(Get-FileHash -LiteralPath $planPath).Hash;files=$rows;fragments=$fragmentRows;routeInverseVerified=$true;adapterPrefixUnchanged=$true;compiled=$false;runtimeVerified=$false;installedBrowserChanged=$false;privateReportRetentionReviewed=$false}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'source.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'source.json')
