param([switch]$CurrentDetailed)
$ErrorActionPreference='Stop'
$base='I:/LeaBuildStorage/trusted-bootstrap-profile-source-20261102-v3/source.json'
$out='I:/LeaBuildStorage/trusted-bootstrap-profile-source-20261102-v4'
$baseHash='87E7699FF07EDD646DCEA936DFC61B87646C4EA19C6375AE68AD7565EAF9B406'
if($CurrentDetailed){
    $base='I:/LeaBuildStorage/detailed-bootstrap-profile-source-20261109-v1/source.json'
    $baseHash='BFBEEF565BE2EEEE0683C8A729E754DEDB17B49C458BA3C7782CED5C33C6926F'
    $out='I:/LeaBuildStorage/current-resource-bootstrap-source-20261109-v1'
}
if(Test-Path $out){throw 'Preserve resource profile'}
if((Get-FileHash $base).Hash -cne $baseHash){throw 'Detailed profile prerequisite differs'}
$plan=Get-Content $base -Raw|ConvertFrom-Json
$hostFile=@($plan.files|Where-Object name -CEQ '8.cs')[0]
if((Get-FileHash $hostFile.path).Hash -cne $hostFile.sha256){throw 'Profile host changed'}
$text=[IO.File]::ReadAllText($hostFile.path)
$nl=if($text.Contains("`r`n")){"`r`n"}else{"`n"}
$mark=@([regex]::Matches($text,'(?m)^            Action<string> profileMark = [^\r\n]+')|ForEach-Object Value)
if($mark.Count -ne 1){throw 'One buffered profile delegate required'}
$delegate=$mark[0].Replace('label => profileRows.Add(','label => { profileRows.Add(')
$delegate=$delegate.Substring(0,$delegate.Length-1)+'; resourceRows.Add(BootstrapResourceCounters.Capture(label, profileClock.ElapsedTicks)); };'
$flush='                    foreach (var row in profileRows) Console.Error.WriteLine("LEA_TRUSTED_BOOT wallFrequency=" + System.Diagnostics.Stopwatch.Frequency + " cpuFrequency=" + TimeSpan.TicksPerSecond + " " + row);'
$pairs=@(
    @('            var profileRows = new System.Collections.Generic.List<string>();',('            var profileRows = new System.Collections.Generic.List<string>();'+$nl+'            var resourceRows = new System.Collections.Generic.List<string>();')),
    @($mark[0],$delegate),
    @($flush,($flush+$nl+'                    foreach (var row in resourceRows) Console.Error.WriteLine(row);'))
)
$helperPath=Join-Path $PSScriptRoot 'BootstrapResourceCounters.cs'
$helperHash=(Get-FileHash -LiteralPath $helperPath).Hash
if($CurrentDetailed -and $helperHash -cne '2DDCDDD3A62231F1F3BA71BE9017F59AE7542123539FFEE22900F04E405C7C90'){throw 'Retained resource helper differs'}
$helper=[IO.File]::ReadAllText($helperPath)
function Project([string]$value){
    if($value.Contains('resourceRows')){throw 'Already resource-instrumented'}
    $original=$value
    foreach($pair in $pairs){if($pair.Count -ne 2 -or [regex]::Matches($value,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique resource profile boundary required'};$value=$value.Replace($pair[0],$pair[1])}
    $inverse=$value;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
    if($inverse -cne $original){throw 'Resource profile inverse differs'}
    foreach($operation in @('Vo.Of(1000)','factory(settings => Vo.At(settings, 1000))','var syntax = prepared.Value;','dispatch = vm.Evaluate(syntax);','markers.End();','bridge.Begin();')){if([regex]::Matches($value,[regex]::Escape($operation)).Count -ne [regex]::Matches($original,[regex]::Escape($operation)).Count){throw 'Original bootstrap operation changed'}}
    return $value+$nl+$helper
}
$updated=Project $text;$negative=0
foreach($pair in $pairs){$rejected=$false;try{$null=Project $text.Replace($pair[0],'MISSING_RESOURCE_BOUNDARY')}catch{$rejected=$true};if(!$rejected){throw 'Missing resource boundary accepted'};$negative++}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($file in $plan.files){
    if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Detailed profile input changed'}
    $to=Join-Path $out $file.name
    if($file.name -ceq '8.cs'){[IO.File]::WriteAllText($to,$updated,[Text.UTF8Encoding]::new($false))}else{Copy-Item -LiteralPath $file.path -Destination $to}
    $file.path=$to;$file.sha256=(Get-FileHash $to).Hash
}
$plan|Add-Member resourceCounters $true
$plan|Add-Member telemetryNegativeControls $negative
$plan|Add-Member previousSourceSHA256 $baseHash
$plan|Add-Member resourceHelperSHA256 $helperHash
$plan|Add-Member projectionScriptSHA256 ((Get-FileHash -LiteralPath $PSCommandPath).Hash)
if($CurrentDetailed){
    $node=Join-Path (Split-Path $base -Parent) 'node-test.json'
    if((Get-FileHash -LiteralPath $node).Hash -cne 'B950A1B4BD616AAD2754FF90D2997A962E1905E133997F9C5C794C6176B93CF2'){throw 'Existing unchanged-resource behavior proof differs'}
    Copy-Item -LiteralPath $node -Destination (Join-Path $out 'node-test.json')
}
$plan.scope='Diagnostic-only sequential thread/process CPU, process faults/I/O and thread cycles at existing buffered profile marks. Original trusted1000ms/bootstrap statements preserved; observer overhead, no hard-fault or disk-wait attribution.'
$plan|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'source.json') -Encoding utf8
Get-FileHash (Join-Path $out 'source.json')
