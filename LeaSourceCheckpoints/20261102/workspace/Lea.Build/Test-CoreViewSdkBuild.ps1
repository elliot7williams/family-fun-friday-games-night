param([switch]$KeepForegroundHost,[ValidateRange(1,99)][int]$Version=1,[switch]$DotnetSdk,[ValidateRange(1,9)][int]$Attempt=1,[switch]$Embedding,[switch]$ImageWorker,[switch]$PrintLibrary,[switch]$ScriptLibraries,[switch]$ScriptNet48,[switch]$ScriptTailTrace,[switch]$ScriptTimeline,[switch]$ScriptTimelineExport,[switch]$ScriptWorker,[switch]$ScriptSessionIoReuse,[switch]$ScriptPooledIo)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$sdk='I:\LeaBuildStorage\core-view-sdk-20261031-v'+$Version
$out='I:\LeaBuildStorage\core-view-sdk-build-20261031-v'+$Version
if($Attempt -gt 1){$out+='-attempt'+$Attempt}
if($Embedding){if(!$DotnetSdk -or $Version -ne 3){throw 'Embedding requires the qualified modern SDK v3 build'};$out+='-embedding'}
if($ImageWorker){if(!$DotnetSdk -or $Embedding){throw 'Image worker requires separate modern compiler mode'};$out+='-image-worker'}
if($PrintLibrary){if(!$DotnetSdk -or $Embedding -or $ImageWorker){throw 'Print library requires separate modern compiler mode'};$out+='-print-library'}
if($ScriptLibraries){if(!$DotnetSdk -or $Embedding -or $ImageWorker -or $PrintLibrary){throw 'Script libraries require separate modern compiler mode'};$out+='-script-libraries'}
if($ScriptNet48){if(!$ScriptLibraries){throw 'Net48 script requires script library mode'};$out+='-net48'}
if($ScriptTailTrace){if(!$ScriptNet48){throw 'Tail trace requires net48 script mode'};$out+='-tail-trace'}
if($ScriptTimeline){if(!$ScriptTailTrace){throw 'Timeline requires tail trace mode'};$out+='-timeline'}
if($ScriptTimelineExport){if(!$ScriptTimeline){throw 'Timeline export requires timeline mode'};$out+='-export'}
if($ScriptSessionIoReuse){if(!$ScriptNet48 -or $ScriptTailTrace -or $ScriptTimeline -or $ScriptTimelineExport){throw 'Session I/O requires separate uninstrumented net48 script mode'};$out+='-session-io'}
if($ScriptPooledIo){if(!$ScriptNet48 -or $ScriptTailTrace -or $ScriptTimeline -or $ScriptTimelineExport -or $ScriptSessionIoReuse){throw 'Pooled I/O requires separate uninstrumented net48 script mode'};$out+='-pooled-io'}
if($ScriptWorker){if(!$DotnetSdk -or $Embedding -or $ImageWorker -or $PrintLibrary -or $ScriptLibraries -or $ScriptNet48){throw 'Worker requires separate modern SDK mode'};$out+='-script-worker'}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained TTY owner required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out)){throw 'Fresh output and original storage reserve required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$hash){
    $stream=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($stream)
    $actual=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($hash -and $hash -cne $actual){throw 'SDK input differs'};$inputs.Add(@{path=$path;sha256=$actual})
}
New-Item -ItemType Directory -Path $out | Out-Null
$result=@{passed=$false;runtimeQualified=$false;completeSdk=$false;inputs=$inputs;runs=$runs}
try{
    Hold $PSCommandPath
    Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'build.ps1')
    $source=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $source 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $source
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $manifestPath=Join-Path $sdk 'source-manifest.json';Hold $manifestPath
    $manifest=Get-Content $manifestPath -Raw|ConvertFrom-Json
    if($manifest.files.Count -ne 45){throw 'SDK inventory differs'}
    foreach($file in $manifest.files){Hold (Join-Path $sdk $file.path) $file.sha256}
    $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework/v4.0.30319/MSBuild.exe';Hold $compiler '151D0125C20CDDE578D02DE3F2A56EB904870A887BB7F1C59BF61471DA231916'
    $project=Join-Path $sdk 'Lea.Win/Lea.Win.csproj'
    if($ScriptWorker){
        $scriptWorkerRoot='I:\LeaBuildStorage\sdk-script-worker-20261101-v1'
        $workerManifest=Join-Path $scriptWorkerRoot 'source-manifest.json';Hold $workerManifest
        $workerSource=Get-Content $workerManifest -Raw|ConvertFrom-Json
        if(!$workerSource.staged -or $workerSource.files.Count -ne 22 -or $workerSource.sourceFiles -ne 17 -or $workerSource.runtimeVersion -cne '8.0.33' -or $workerSource.rollForward -cne 'Disable'){throw 'Worker SDK inventory incomplete'}
        foreach($file in $workerSource.files){Hold (Join-Path $scriptWorkerRoot $file.path) $file.sha256}
        $project=Join-Path $scriptWorkerRoot 'Lea.Script.Worker/Lea.Script.Worker.csproj'
    }
    if($ScriptLibraries){
        $scriptRoot='I:\LeaBuildStorage\sdk-script-20261031-v1'
        if($ScriptNet48){$scriptRoot='I:\LeaBuildStorage\sdk-script-20261101-net48-v1'}
        if($ScriptTailTrace){$scriptRoot='I:\LeaBuildStorage\sdk-script-20261101-tail-trace-v1'}
        if($ScriptTimeline){$scriptRoot='I:\LeaBuildStorage\sdk-script-20261101-timeline-v1'}
        if($ScriptTimelineExport){$scriptRoot='I:\LeaBuildStorage\sdk-script-20261101-timeline-export-v1'}
        if($ScriptSessionIoReuse){$scriptRoot='I:\LeaBuildStorage\sdk-script-20261101-session-io-v1'}
        if($ScriptPooledIo){$scriptRoot='I:\LeaBuildStorage\sdk-script-20261101-pooled-io-v2'}
        $scriptManifest=Join-Path $scriptRoot 'source-manifest.json';Hold $scriptManifest
        $script=Get-Content $scriptManifest -Raw|ConvertFrom-Json
        if($ScriptNet48 -and !$script.net48Projection){throw 'Explicit net48 source projection missing'}
        if($ScriptTailTrace -and (!$script.tailTrace.inverseVerified -or $script.tailTrace.negativeControls -ne 3 -or $script.tailTrace.addedCheckpoints -ne 2 -or $script.tailTrace.budgetMs -ne 1000)){throw 'Bounded tail trace projection incomplete'}
        if($ScriptTimeline -and (!$script.timeline.inverseVerified -or $script.timeline.negativeControls -ne 3 -or $script.timeline.budgetMs -ne 1000)){throw 'Bounded timeline projection incomplete'}
        if($ScriptTimelineExport -and (!$script.timelineExport.inverseVerified -or $script.timelineExport.negativeControls -ne 3 -or $script.timelineExport.budgetMs -ne 1000)){throw 'Timeline export projection incomplete'}
        if($ScriptSessionIoReuse -and (!$script.sessionIoReuse.inverseVerified -or $script.sessionIoReuse.negativeControls -ne 3 -or $script.sessionIoReuse.edits -ne 4 -or $script.sessionIoReuse.budgetsChanged -or $script.sessionIoReuse.nativeGuardsChanged -or $script.sessionIoReuse.publicApiChanged)){throw 'Session I/O candidate projection incomplete'}
        if($ScriptPooledIo -and (!$script.pooledIo.inverseVerified -or $script.pooledIo.negativeControls -ne 3 -or $script.pooledIo.edits -ne 5 -or !$script.pooledIo.completionRetirementUnchanged -or $script.pooledIo.budgetsChanged -or $script.pooledIo.nativeGuardsChanged -or $script.pooledIo.publicApiChanged -or $script.pooledIo.threadPoolSettingsChanged)){throw 'Pooled I/O source projection incomplete'}
        if(!$script.staged -or $script.files.Count -ne 28 -or $script.parentSourceReceiptSha256 -cne '9AFFEF57588F16A4D84426B972AB8EA4E2E7F78A3F305CBE59D0402B0EDFB8AF' -or $script.adapterSourceReceiptSha256 -cne '7178C265B7BCAF50738DC8F610432F4C6B282DAB77442D689D06437CD3BD3E35'){throw 'Script source inventory incomplete'}
        foreach($file in $script.files){Hold (Join-Path $scriptRoot $file.path) $file.sha256}
        $project=Join-Path $scriptRoot 'Lea.Script.Win/Lea.Script.Win.csproj'
    }
    if($PrintLibrary){
        $printRoot='I:\LeaBuildStorage\sdk-print-20261031-v2'
        $printManifest=Join-Path $printRoot 'source-manifest.json';Hold $printManifest
        $print=Get-Content $printManifest -Raw|ConvertFrom-Json
        if(!$print.staged -or $print.files.Count -ne 23 -or $print.references.Count -ne 2 -or $print.sourceReceiptSha256 -cne 'A71A3BC94E9584876DCE5480027EBAD4585A2900E6F01651C65D4869FA2D8002'){throw 'Print SDK source manifest incomplete'}
        foreach($file in @($print.files)+@($print.references)){Hold (Join-Path $printRoot $file.path) $file.sha256}
        $project=Join-Path $printRoot 'Lea.Print/Lea.Print.csproj'
    }
    if($ImageWorker){
        $imageRoot='I:\LeaBuildStorage\sdk-image-worker-20261031-v1'
        $imageManifest=Join-Path $imageRoot 'source-manifest.json';Hold $imageManifest
        $image=Get-Content $imageManifest -Raw|ConvertFrom-Json
        if(!$image.staged -or $image.files.Count -ne 3 -or $image.originalSourceProofSha256 -cne '09690529339278525ACE678608BBD702A2AA3A70846B7EA3D5698EDE4843CD5D'){throw 'Image source manifest incomplete'}
        foreach($file in $image.files){Hold (Join-Path $imageRoot $file.path) $file.sha256}
        $project=Join-Path $imageRoot 'Lea.Image/Lea.Image.csproj'
    }
    if($Embedding){
        if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
        $proofPath='I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-attempt2\result.json';Hold $proofPath '55595335946DB681D98788BDCB219900FE9CF9DCC75D60CE64FD03BD9ED4325A'
        $proof=Get-Content $proofPath -Raw|ConvertFrom-Json
        if(!$proof.passed -or !$proof.inputsUnchanged -or $proof.ownerAttention -or $proof.outputs.Count -ne 2){throw 'SDK build proof incomplete'}
        foreach($file in $proof.outputs){Hold $file.path $file.sha256}
        $sample=Join-Path $out 'sample';New-Item -ItemType Directory -Path $sample | Out-Null
        foreach($name in @('EmbeddingSample.cs','EmbeddingSample.csproj','embedding.html')){
            $from=Join-Path $root ('Lea.Sdk/'+$name);Hold $from
            $to=Join-Path $sample $name;Copy-Item -LiteralPath $from -Destination $to;Hold $to (Get-FileHash $from).Hash
        }
        $project=Join-Path $sample 'EmbeddingSample.csproj'
    }
    $arguments=@(('"'+$project+'"'),'/nologo','/m:1','/nodeReuse:false','/t:Rebuild','/v:minimal','/p:Configuration=Release','/p:UseSharedCompilation=false')
    if($ScriptWorker){$arguments+='/restore'}
    if($Embedding){$arguments+=('/p:LeaSdkBin="'+(Join-Path $sdk 'bin/Release')+'"')}
    if($DotnetSdk){
        $compiler='C:\Program Files\dotnet\dotnet.exe';Hold $compiler
        $toolchain='C:\Program Files\dotnet\sdk\8.0.427'
        foreach($name in @('MSBuild.dll','Microsoft.Build.dll','Roslyn/bincore/csc.dll','Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll')){Hold (Join-Path $toolchain $name)}
        $arguments=@(('"'+(Join-Path $toolchain 'MSBuild.dll')+'"'))+$arguments
        $result.compilerSdk='8.0.427'
    }
    $built=Invoke-OwnedTestRun -Name 'build-sdk' -Application $compiler -Arguments $arguments -WorkingDirectory $sdk -OutputDirectory $out -Seconds 120 -Records $runs
    $result.passed=$built.passed
    if($Embedding -and $built.passed){
        $runtime=Join-Path $sample 'bin/Release'
        foreach($file in $proof.outputs){Hold (Join-Path $runtime ([IO.Path]::GetFileName($file.path))) $file.sha256}
        $exe=Join-Path $runtime 'Lea.EmbeddingSample.exe';Hold $exe
        $fixture=Join-Path $runtime 'embedding.html';Hold $fixture (Get-FileHash (Join-Path $sample 'embedding.html')).Hash
        $tested=Invoke-OwnedTestRun -Name 'embed-sdk' -Application $exe -Arguments @('--test',('"'+$fixture+'"'),('"'+$out+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
        $result.passed=$tested.passed -and @((Get-Content $tested.stdout)|Where-Object {$_ -ceq 'PASS LEA SDK EMBEDDING 14'}).Count -eq 1
        $result.embeddingChecks=14;$result.localTextOnly=$true;$result.imageWorkerTested=$false;$result.scriptWorkerTested=$false
    }
    if($built.passed){$result.outputs=@(foreach($name in @('Lea.Engine.dll','Lea.Win.dll')){$path=Join-Path $sdk ('bin/Release/'+$name);@{path=$path;sha256=(Get-FileHash $path).Hash}})}
    if($ImageWorker -and $built.passed){$path=Join-Path $imageRoot 'bin/Release/Lea.Image.exe';$result.outputs=@(@{path=$path;sha256=(Get-FileHash $path).Hash});$result.imageWorkerCompiled=$true;$result.securityQualified=$false}
    if($PrintLibrary -and $built.passed){$path=Join-Path $printRoot 'bin/Release/Lea.Print.dll';$result.outputs=@(@{path=$path;sha256=(Get-FileHash $path).Hash});$result.printLibraryCompiled=$true;$result.printerDispatched=$false}
    if($ScriptLibraries -and $built.passed){$result.outputs=@(foreach($name in @('Lea.Script.dll','Lea.Script.Win.dll')){$path=Join-Path $scriptRoot ('bin/Release/'+$name);@{path=$path;sha256=(Get-FileHash $path).Hash}});$result.scriptLibrariesCompiled=$true;$result.workerIncluded=$false;$result.remoteScriptingEnabled=$false}
    if($ScriptWorker -and $built.passed){
        $workerRuntime=Join-Path $scriptWorkerRoot 'bin/Release/ScriptWorker'
        $result.outputs=@(foreach($name in @('Lea.Script.Worker.exe','Lea.Script.Worker.dll','Lea.Script.Worker.runtimeconfig.json','Lea.Script.Worker.deps.json','Jint.dll','Acornima.dll')){$path=Join-Path $workerRuntime $name;@{path=$path;sha256=(Get-FileHash $path).Hash}})
        $config=Get-Content (Join-Path $workerRuntime 'Lea.Script.Worker.runtimeconfig.json') -Raw|ConvertFrom-Json
        if($config.runtimeOptions.framework.version -cne '8.0.33' -or $config.runtimeOptions.rollForward -cne 'Disable'){throw 'Generated worker runtime policy differs'}
        $result.scriptWorkerIlCompiled=$true;$result.readyToRun=$false;$result.workerExecuted=$false
    }
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaSdkInputLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
