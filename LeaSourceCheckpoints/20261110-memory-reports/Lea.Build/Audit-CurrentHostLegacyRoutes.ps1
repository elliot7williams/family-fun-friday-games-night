$ErrorActionPreference='Stop'
$stage='I:\LeaBuildStorage\current-firespider-build-20261109-v1\source'
$out='I:\LeaBuildStorage\current-host-legacy-routes-20261110-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve route audit'}
$manifestPath=Join-Path (Split-Path $stage -Parent) 'candidate-manifest.json'
$plan=Get-Content -LiteralPath $manifestPath -Raw|ConvertFrom-Json
$source=@($plan.files|Where-Object kind -CEQ 'preserved-host-source')
if($source.Count -ne 87 -or !$plan.hostSourceUnchanged){throw 'Preserved source closure required'}
foreach($f in $plan.files){if((Get-FileHash -LiteralPath (Join-Path $stage $f.path)).Hash -cne $f.sha256){throw 'Qualified source closure drift'}}
[xml]$project=Get-Content -LiteralPath (Join-Path $stage 'Firespider Internet.vbproj') -Raw
$ns=[Xml.XmlNamespaceManager]::new($project.NameTable);$ns.AddNamespace('m',$project.DocumentElement.NamespaceURI)
$compiled=@($project.SelectNodes('//m:Compile',$ns)|ForEach-Object {$_.Include.Replace('\','/')})
$hits=[Collections.Generic.List[object]]::new();$menus=[Collections.Generic.List[object]]::new()
foreach($relative in $compiled){
 $path=Join-Path $stage $relative
 if(!(Test-Path -LiteralPath $path)){throw 'Compiled source missing'}
 $lines=[IO.File]::ReadAllLines($path);$method='';$start=0;$body=[Collections.Generic.List[string]]::new()
 for($i=0;$i -lt $lines.Count;$i++){
  $line=$lines[$i]
  if($line -match '^\s*(?:(?:Private|Public|Friend|Protected|Shared|Overrides|Overloads)\s+)*(?:Sub|Function)\s+(\w+)'){$method=$Matches[1];$start=$i+1;$body.Clear()}
  if($method){$body.Add($line)}
  if($line -match '\b(?:WebBrowser|HtmlDocument|HtmlElement|WebBrowserDocumentCompletedEventArgs)\b'){$hits.Add(@{file=$relative;line=$i+1;method=$method;text=$line.Trim()})}
  if($line -match 'qnewitem\("([^"]+)"\s*,\s*AddressOf\s+(\w+)\s*,\s*AddressOf\s+(\w+)'){$menus.Add(@{file=$relative;line=$i+1;label=$Matches[1];click=$Matches[2];hover=$Matches[3]})}
 }
}
$features=[IO.File]::ReadAllText((Join-Path $stage 'BrowserFeatures2.vb'))
$adapter=[IO.File]::ReadAllText((Join-Path $stage 'FirespiderLea.vb'))
foreach($symbol in @('q870','q872')){
 $block=[regex]::Match($features,'(?ms)^\s*Sub '+$symbol+'\(.*?^\s*End Sub')
 if(!$block.Success -or !$block.Value.Contains('CType(k.SelectedTab.Controls(0), WebBrowser)')){throw 'Known legacy route changed'}
 if([regex]::IsMatch($adapter,'\b'+$symbol+'\b')){throw 'Adapter route requires manual reassessment'}
}
if(!$adapter.Contains('New ToolStripMenuItem("New Legacy Tab")') -or !$adapter.Contains('AddHandler qleaMenu.Click, AddressOf qleaLegacyNewClick') -or !$adapter.Contains('q3(qntu())')){throw 'Explicit legacy creation route changed'}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{verified=$true;kind='Source-bound lexical route inventory, not runtime reachability or UI validation';sourceManifestSha256=(Get-FileHash -LiteralPath $manifestPath).Hash;preservedHostFiles=87;compiledFiles=$compiled.Count;legacyReferences=$hits;menuBindings=$menus;confirmedSourceGaps=@('Accessibility Inspector q870 casts selected control to WebBrowser; no q870 adapter replacement','Archive Current Page Locally q872 casts selected control to WebBrowser; no q872 adapter replacement','Explicit New Legacy Tab menu invokes legacy q3 factory');unknowns=@('Other menu replacement and reachability requires per-route analysis','Runtime click/error/report behavior not executed','About and UIA not qualified by lexical inventory');installedBrowserChanged=$false;releaseQualified=$false;scriptSha256=(Get-FileHash -LiteralPath $PSCommandPath).Hash}
$result|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $out 'audit.json') -Encoding utf8
$menus|Export-Csv -LiteralPath (Join-Path $out 'menu-bindings.csv') -NoTypeInformation
$hits|Export-Csv -LiteralPath (Join-Path $out 'legacy-references.csv') -NoTypeInformation
Copy-Item -LiteralPath $PSCommandPath -Destination $out
Get-FileHash -LiteralPath (Join-Path $out 'audit.json')
@{compiledFiles=$compiled.Count;lexicalReferences=$hits.Count;menuBindings=$menus.Count}|ConvertTo-Json
