param([switch]$Net48,[switch]$BootstrapTailTrace,[switch]$BootstrapTimeline,[switch]$BootstrapTimelineExport,[switch]$SessionIoReuse,[switch]$PooledIo)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\sdk-script-20261031-v1'
if($Net48){$out='I:\LeaBuildStorage\sdk-script-20261101-net48-v1'}
if($BootstrapTailTrace){if(!$Net48){throw 'Tail trace requires separate net48 SDK candidate'};$out='I:\LeaBuildStorage\sdk-script-20261101-tail-trace-v1'}
if($BootstrapTimeline){if(!$BootstrapTailTrace){throw 'Timeline requires tail trace candidate'};$out='I:\LeaBuildStorage\sdk-script-20261101-timeline-v1'}
if($BootstrapTimelineExport){if(!$BootstrapTimeline){throw 'Timeline export requires timeline candidate'};$out='I:\LeaBuildStorage\sdk-script-20261101-timeline-export-v1'}
if($SessionIoReuse){if(!$Net48 -or $BootstrapTailTrace -or $BootstrapTimeline -or $BootstrapTimelineExport){throw 'Session I/O prototype requires separate uninstrumented net48 candidate'};$out='I:\LeaBuildStorage\sdk-script-20261101-session-io-v1'}
if($PooledIo){if(!$Net48 -or $BootstrapTailTrace -or $BootstrapTimeline -or $BootstrapTimelineExport -or $SessionIoReuse){throw 'Pooled I/O requires separate uninstrumented net48 candidate'};$out='I:\LeaBuildStorage\sdk-script-20261101-pooled-io-v2'}
if(Test-Path $out){throw 'Preserve script SDK staging'}
$rows=[Collections.Generic.List[object]]::new()
function Stage([string]$from,[string]$relative,[string]$pin){
    if([IO.Path]::IsPathRooted($relative) -or $relative.Contains(':') -or $relative.Split('/') -contains '..'){throw 'Script SDK path escape'}
    $item=Get-Item -LiteralPath $from
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    if((Get-FileHash $from).Hash -cne $pin){throw ('Script source pin differs: '+$relative)}
    $to=Join-Path $out $relative;New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null
    Copy-Item $from $to
    if((Get-FileHash $to).Hash -cne $pin){throw 'Script SDK copy differs'}
    $rows.Add(@{path=$relative;sha256=$pin;origin=$from})
}
$parentPath=Join-Path $root 'continuity-evidence/permission-contract-v1/early-limits-v1/result.json'
$parentPin='9AFFEF57588F16A4D84426B972AB8EA4E2E7F78A3F305CBE59D0402B0EDFB8AF'
if((Get-FileHash $parentPath).Hash -cne $parentPin){throw 'Parent source proof differs'}
$parent=Get-Content $parentPath -Raw|ConvertFrom-Json
if(!$parent.passed -or !$parent.inputsUnchanged -or $parent.sourceProofs.Count -ne 15 -or $parent.binarySha256 -cne 'E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5'){throw 'Parent source proof incomplete'}
foreach($file in $parent.sourceProofs){Stage $file.copy ('Lea.Script/'+[IO.Path]::GetFileName($file.copy)) $file.sha256}
$resource=Join-Path $root 'continuity-evidence/bootstrap-descriptor-batches-v1/generation-v1/preview/Lea.Script/Dom.compact.js'
$resourcePin='EC4B6513A77230C3B8439568CC3FED41B7712CA3779182D38076880DCC83BC8A'
$pins=@($parent.inputs|Where-Object path -ceq $resource|Select-Object -ExpandProperty sha256 -Unique)
if($pins.Count -ne 1 -or $pins[0] -cne $resourcePin){throw 'Bootstrap resource provenance differs'}
Stage $resource 'Lea.Script/Dom.compact.js' $resourcePin
$adapterPath=Join-Path $root 'continuity-evidence/script-adapter-source-build-v2/result.json'
$adapterPin='7178C265B7BCAF50738DC8F610432F4C6B282DAB77442D689D06437CD3BD3E35'
if((Get-FileHash $adapterPath).Hash -cne $adapterPin){throw 'Adapter proof differs'}
$adapter=Get-Content $adapterPath -Raw|ConvertFrom-Json
if(!$adapter.passed -or !$adapter.inputsUnchanged -or $adapter.ownerAttention){throw 'Adapter source proof incomplete'}
foreach($pair in @(@('W.cs','197F77D2A0085A0D00AE2B912E1C3ED06EC373B538392A81E3CBEB1B088B7509'),@('AssemblyInfo.cs','3559279408B0EA81966177EF430F1034ECEF7C2AD5B02B5CDDF4DFADBC3B3678'))){Stage (Join-Path (Split-Path $adapterPath) $pair[0]) ('Lea.Script.Win/'+$pair[0]) $pair[1]}
foreach($name in @('Lea.Script','Lea.Script.Win')){$template=Join-Path $root ('Lea.Sdk/'+$name+'.csproj');Stage $template ($name+'/'+$name+'.csproj') (Get-FileHash $template).Hash}
$libs=@('Jint.dll','Acornima.dll','System.Buffers.dll','System.Memory.dll','System.Numerics.Vectors.dll','System.Runtime.CompilerServices.Unsafe.dll')
foreach($name in $libs){
    $path=Join-Path $root ('continuity-evidence/bootstrap-deferred-task-v1/build-v1/runtime/'+$name)
    $pins=@($parent.inputs|Where-Object path -ceq $path|Select-Object -ExpandProperty sha256 -Unique)
    if($pins.Count -ne 1){throw 'Parent dependency pin missing'}
    Stage $path ('lib/'+$name) $pins[0]
}
foreach($pair in @(@('Lea.Engine.dll','5B8A501DA627467A6C261A355917DE5F73DF8F00758B49C1D240CF7D596E8A4A'),@('Lea.Win.dll','A622A1C266AD7CA27F437832E390E594F7E419CAFED700C9F466F0517970CE0D'))){Stage (Join-Path $root ('continuity-evidence/sdk-print-aggregate-source-v1/bundle/Sample/bin/Release/'+$pair[0])) ('bin/Release/'+$pair[0]) $pair[1]}
if($rows.Count -ne 28){throw 'Script staging inventory differs'}
if($Net48){
    foreach($name in @('Lea.Script','Lea.Script.Win')){
        $relative=$name+'/'+$name+'.csproj';$path=Join-Path $out $relative
        $xml=[xml](Get-Content $path -Raw)
        $node=$xml.DocumentElement.SelectSingleNode("*[local-name()='PropertyGroup']/*[local-name()='TargetFrameworkVersion']")
        if($node.InnerText -cne 'v4.6.2'){throw 'Original script target differs'}
        $node.InnerText='v4.8'
        if($name -ceq 'Lea.Script'){$xml.DocumentElement.SelectSingleNode("*[local-name()='PropertyGroup']/*[local-name()='DefineConstants']").InnerText='TRACE;RELEASE;NETFRAMEWORK;NET48'}
        $xml.Save($path)
        $row=@($rows|Where-Object path -ceq $relative)[0];$row.originalSha256=$row.sha256;$row.sha256=(Get-FileHash $path).Hash
    }
    foreach($relative in @('Lea.Script/14.cs','Lea.Script.Win/AssemblyInfo.cs')){
        $path=Join-Path $out $relative;$text=[IO.File]::ReadAllText($path)
        foreach($pair in @(@('.NETFramework,Version=v4.6.2','.NETFramework,Version=v4.8'),@('.NET Framework 4.6.2','.NET Framework 4.8'))){
            if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique original framework metadata required'}
            $text=$text.Replace($pair[0],$pair[1])
        }
        [IO.File]::WriteAllText($path,$text,[Text.UTF8Encoding]::new($false))
        $row=@($rows|Where-Object path -ceq $relative)[0];$row.originalSha256=$row.sha256;$row.sha256=(Get-FileHash $path).Hash
    }
}
$trace=$null
if($BootstrapTailTrace){
    . (Join-Path $PSScriptRoot 'New-SdkBootstrapTailTrace.ps1')
    $bootstrapPath=Join-Path $out 'Lea.Script/Dom.compact.js';$markersPath=Join-Path $out 'Lea.Script/2.cs'
    $bootstrap=[IO.File]::ReadAllText($bootstrapPath);$markers=[IO.File]::ReadAllText($markersPath)
    $trace=New-SdkBootstrapTailTrace $bootstrap $markers
    $negative=0
    foreach($bad in @($bootstrap.Replace('return canceled})}','return changed})}'),($bootstrap+$bootstrap),$trace.bootstrap)){
        $denied=$false;try{$null=New-SdkBootstrapTailTrace $bad $markers}catch{$denied=$true}
        if(!$denied){throw 'Tail trace negative control failed'};$negative++
    }
    foreach($spec in @(@('Lea.Script/Dom.compact.js',$trace.bootstrap),@('Lea.Script/2.cs',$trace.markers))){
        $path=Join-Path $out $spec[0];[IO.File]::WriteAllText($path,$spec[1],[Text.UTF8Encoding]::new($false))
        $row=@($rows|Where-Object path -ceq $spec[0])[0];$row.originalSha256=$row.sha256;$row.sha256=(Get-FileHash $path).Hash
    }
    $trace=@{inverseVerified=$trace.inverseVerified;negativeControls=$negative;addedCheckpoints=2;diagnosticOnly=$true;budgetMs=1000;helperSha256=(Get-FileHash (Join-Path $PSScriptRoot 'New-SdkBootstrapTailTrace.ps1')).Hash}
}
$timeline=$null
if($BootstrapTimeline){
    . (Join-Path $PSScriptRoot 'New-SdkBootstrapTimeline.ps1')
    $parentPath=Join-Path $out 'Lea.Script/8.cs';$markersPath=Join-Path $out 'Lea.Script/2.cs'
    $parentText=[IO.File]::ReadAllText($parentPath);$markerText=[IO.File]::ReadAllText($markersPath)
    $timed=New-SdkBootstrapTimeline $parentText $markerText
    $negative=0
    foreach($bad in @($parentText.Replace('var syntax = prepared.Value;','var syntax = changed.Value;'),($parentText+$parentText),$timed.parent)){
        $denied=$false;try{$null=New-SdkBootstrapTimeline $bad $markerText}catch{$denied=$true}
        if(!$denied){throw 'Timeline negative control failed'};$negative++
    }
    foreach($spec in @(@('Lea.Script/8.cs',$timed.parent),@('Lea.Script/2.cs',$timed.markers))){
        $path=Join-Path $out $spec[0];[IO.File]::WriteAllText($path,$spec[1],[Text.UTF8Encoding]::new($false))
        $row=@($rows|Where-Object path -ceq $spec[0])[0];$row.preTimelineSha256=$row.sha256;$row.sha256=(Get-FileHash $path).Hash
    }
    $timeline=@{inverseVerified=$timed.inverseVerified;negativeControls=$negative;budgetMs=1000;diagnosticOnly=$true;helperSha256=(Get-FileHash (Join-Path $PSScriptRoot 'New-SdkBootstrapTimeline.ps1')).Hash}
}
$export=$null
if($BootstrapTimelineExport){
    . (Join-Path $PSScriptRoot 'New-SdkBootstrapTimelineExport.ps1')
    $path=Join-Path $out 'Lea.Script/8.cs';$text=[IO.File]::ReadAllText($path)
    $projected=New-SdkBootstrapTimelineExport $text
    $negative=0
    foreach($bad in @($text.Replace('private void Start()','private void Changed()'),($text+$text),$projected.parent)){
        $denied=$false;try{$null=New-SdkBootstrapTimelineExport $bad}catch{$denied=$true}
        if(!$denied){throw 'Timeline export negative control failed'};$negative++
    }
    [IO.File]::WriteAllText($path,$projected.parent,[Text.UTF8Encoding]::new($false))
    $row=@($rows|Where-Object path -ceq 'Lea.Script/8.cs')[0];$row.preExportSha256=$row.sha256;$row.sha256=(Get-FileHash $path).Hash
    $export=@{inverseVerified=$projected.inverseVerified;negativeControls=$negative;diagnosticOnly=$true;budgetMs=1000}
}
$sessionIo=$null
if($SessionIoReuse){
    . (Join-Path $PSScriptRoot 'New-SdkSessionIoReuse.ps1')
    $path=Join-Path $out 'Lea.Script/6.cs';$text=[IO.File]::ReadAllText($path)
    $projected=New-SdkSessionIoReuse $text
    if(!$projected.inverseVerified -or $projected.negativeControls -ne 3 -or $projected.edits -ne 4 -or $projected.budgetsChanged -or $projected.nativeGuardsChanged -or $projected.publicApiChanged){throw 'Session I/O prototype projection incomplete'}
    [IO.File]::WriteAllText($path,$projected.parent,[Text.UTF8Encoding]::new($false))
    $row=@($rows|Where-Object path -ceq 'Lea.Script/6.cs')[0];$row.originalSha256=$row.sha256;$row.sha256=(Get-FileHash $path).Hash
    $sessionIo=@{inverseVerified=$true;negativeControls=3;edits=4;budgetsChanged=$false;nativeGuardsChanged=$false;publicApiChanged=$false;runtimeQualified=$false;helperSha256=(Get-FileHash (Join-Path $PSScriptRoot 'New-SdkSessionIoReuse.ps1')).Hash}
}
$pooled=$null
if($PooledIo){
    . (Join-Path $PSScriptRoot 'New-SdkPooledIo.ps1')
    $path=Join-Path $out 'Lea.Script/6.cs';$text=[IO.File]::ReadAllText($path)
    $projected=New-SdkPooledIo $text
    if(!$projected.inverseVerified -or $projected.negativeControls -ne 3 -or $projected.edits -ne 5 -or !$projected.completionRetirementUnchanged -or $projected.budgetsChanged -or $projected.nativeGuardsChanged -or $projected.publicApiChanged -or $projected.threadPoolSettingsChanged){throw 'Pooled I/O source projection incomplete'}
    [IO.File]::WriteAllText($path,$projected.parent,[Text.UTF8Encoding]::new($false))
    $row=@($rows|Where-Object path -ceq 'Lea.Script/6.cs')[0];$row.originalSha256=$row.sha256;$row.sha256=(Get-FileHash $path).Hash
    $pooled=@{inverseVerified=$true;negativeControls=3;edits=5;completionRetirementUnchanged=$true;budgetsChanged=$false;nativeGuardsChanged=$false;publicApiChanged=$false;threadPoolSettingsChanged=$false;runtimeQualified=$false;helperSha256=(Get-FileHash (Join-Path $PSScriptRoot 'New-SdkPooledIo.ps1')).Hash}
}
foreach($row in $rows){if((Get-FileHash (Join-Path $out $row.path)).Hash -cne $row.sha256){throw 'Script staging readback differs'}}
@{staged=$true;files=$rows;pooledIo=$pooled;sessionIoReuse=$sessionIo;timelineExport=$export;timeline=$timeline;tailTrace=$trace;net48Projection=[bool]$Net48;parentSourceReceiptSha256=$parentPin;adapterSourceReceiptSha256=$adapterPin;compiled=$false;runtimeQualified=$false;workerIncluded=$false;noticesComplete=$false;remoteScriptingEnabled=$false;completeSdk=$false}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'source-manifest.json') -Encoding utf8
Write-Output 'Script SDK staged: 20 source/resource/project files and eight pinned dependency/reference DLLs; not compiled or runtime qualified; worker still absent.'
