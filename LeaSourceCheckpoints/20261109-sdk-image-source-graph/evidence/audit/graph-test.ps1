param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$base='I:\LeaBuildStorage\sdk-source-graph-20261109-v2\source'
$sdk='I:\LeaBuildStorage\github-source-checkpoint-20261109-v1\LeaSourceCheckpoints\20261102\mature-sdk'
$out='I:\LeaBuildStorage\sdk-image-source-graph-20261109-v1'
$source=Join-Path $out 'source'
$runtime=Join-Path $root 'continuity-evidence/sdk-image-source-graph-v1/runtime'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
$preflight=@{cFree=(Get-PSDrive C).Free;iFree=(Get-PSDrive I).Free;outputExists=(Test-Path $out);runtimeExists=(Test-Path $runtime);ownerExists=[bool](Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)}
if($preflight.cFree -lt 620MB -or $preflight.iFree -lt 128MB -or $preflight.outputExists -or $preflight.runtimeExists -or $preflight.ownerExists){$preflight|ConvertTo-Json;throw 'Fresh outputs/owner and original reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Pin([string]$Path,[string]$Expected){
    $walk=Get-Item -LiteralPath $Path
    while($walk){if($walk.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse input refused'};$walk=if($walk -is [IO.FileInfo]){$walk.Directory}else{$walk.Parent}}
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($stream);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($Expected -and $hash -cne $Expected){throw ('Input differs: '+$Path)};$inputs.Add(@{path=$Path;sha256=$hash})
}
function Copy-Pinned([string]$From,[string]$To,[string]$Expected){Pin $From $Expected;New-Item -ItemType Directory -Path (Split-Path $To -Parent) -Force|Out-Null;Copy-Item -LiteralPath $From -Destination $To;Pin $To (Get-FileHash -LiteralPath $From).Hash}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;before=@();completeSdk=$false;startupFixed=$false;scriptWorkerQualified=$false;hostAdopted=$false;productBinariesSeeded=$false;preflight=$preflight}
$oldTemp=$env:TEMP;$oldTmp=$env:TMP
try{
    Pin $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'test.ps1')
    $auditPath='I:\LeaBuildStorage\sdk-source-graph-audit-20261109-v1\audit.json';Pin $auditPath 'DFBE3C8A161FA6108926AEB5856E5F75F5F4962596111E981A065EB559E873CB'
    $audit=Get-Content -LiteralPath $auditPath -Raw|ConvertFrom-Json
    if(!$audit.verified -or $audit.sourceFiles.Count -ne 72){throw 'Verified mature source layout required'}
    foreach($file in $audit.sourceFiles){
        if([IO.Path]::IsPathRooted($file.path) -or $file.path.Contains(':') -or $file.path.Split('/') -contains '..'){throw 'Source scope escape'}
        $from=Join-Path $base $file.path;$hash=$file.sha256
        if($file.path -ceq 'Sample/EmbeddingSample.csproj'){Pin $from $hash;$from=Join-Path $root 'Lea.Sdk/EmbeddingSample.csproj';$hash=''}
        Copy-Pinned $from (Join-Path $source $file.path) $hash
    }
    foreach($entry in @(@{name='Mg.cs';hash='CDF6E334CF99742BC9F3427ECE57B14AB39A5433141040ED42120F43B7FA83F1'},@{name='Program.cs';hash='4B0EBB73E7ED355373DD75D45A83B6D160900F0BCB93F79536F4E7C5BC83DA44'})){
        Copy-Pinned (Join-Path $sdk ('Lea.Image/'+$entry.name)) (Join-Path $source ('Lea.Image/'+$entry.name)) $entry.hash
    }
    Copy-Pinned (Join-Path $root 'Lea.Sdk/Lea.Image.csproj') (Join-Path $source 'Lea.Image/Lea.Image.csproj') ''
    $priorPath='I:\LeaBuildStorage\sdk-source-graph-20261109-v2\result.json';Pin $priorPath 'E9F481961EDB427F255715F51F28F4ED0E9D675EEA79C10D5E97B772C339A8BF'
    $prior=Get-Content -LiteralPath $priorPath -Raw|ConvertFrom-Json
    foreach($file in Get-ChildItem -LiteralPath (Join-Path $base 'lib') -File){$pins=@($prior.inputs|Where-Object path -CEQ $file.FullName|Select-Object -ExpandProperty sha256 -Unique);if($pins.Count -ne 1){throw 'Dependency pin ambiguous'};Copy-Pinned $file.FullName (Join-Path $source ('lib/'+$file.Name)) $pins[0]}
    if(@(Get-ChildItem -LiteralPath $source -Recurse -File|Where-Object {$_.Extension -in @('.dll','.exe') -and !$_.FullName.StartsWith((Join-Path $source 'lib')+'\',[StringComparison]::OrdinalIgnoreCase)}).Count){throw 'Lea binary seeded into clean producer'}
    $owner=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Pin $owner 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owner
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Pin $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $dotnet='C:\Program Files\dotnet\dotnet.exe';Pin $dotnet
    $toolchain='C:\Program Files\dotnet\sdk\8.0.427';$msbuild=Join-Path $toolchain 'MSBuild.dll';Pin $msbuild
    foreach($name in @('Roslyn/bincore/csc.dll','Roslyn/bincore/csc.deps.json','Roslyn/bincore/csc.runtimeconfig.json','Roslyn/bincore/Microsoft.CodeAnalysis.dll','Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll')){Pin (Join-Path $toolchain $name)}
    $env:TEMP=Join-Path $out 'temp';$env:TMP=$env:TEMP;New-Item -ItemType Directory -Path $env:TEMP|Out-Null
    function Build-Case([string]$Name,[string]$Project,[string]$Folder,[string[]]$Extra){
        $args=@(('"'+$msbuild+'"'),('"'+$Project+'"'),'/nologo','/m:1','/nodeReuse:false','/t:Build','/v:minimal','/p:Configuration=Release','/p:UseSharedCompilation=false')+@($Extra)
        return Invoke-OwnedTestRun -Name $Name -Application $dotnet -Arguments $args -WorkingDirectory $Folder -OutputDirectory $out -Seconds 120 -Records $runs
    }
    $sampleBuild=Build-Case 'clean-source-sample' (Join-Path $source 'Sample/EmbeddingSample.csproj') $source @()
    if(!$sampleBuild.passed){throw 'Direct source sample build failed'}
    $adapterBuild=Build-Case 'complete-framework-libraries' (Join-Path $source 'Lea.Script.Win/Lea.Script.Win.csproj') $source @()
    if(!$adapterBuild.passed){throw 'Remaining framework source build failed'}
    $result.products=@(foreach($name in @('Lea.Engine.dll','Lea.Win.dll','Lea.Image.exe','Lea.Script.dll','Lea.Script.Win.dll')){$p=Join-Path $source ('bin/Release/'+$name);Pin $p;@{name=$name;path=$p;sha256=(Get-FileHash -LiteralPath $p).Hash}})
    $sampleBin=Join-Path $source 'Sample/bin/Release'
    foreach($name in @('Lea.Engine.dll','Lea.Win.dll','Lea.Image.exe')){if((Get-FileHash -LiteralPath (Join-Path $sampleBin $name)).Hash -cne (Get-FileHash -LiteralPath (Join-Path $source ('bin/Release/'+$name))).Hash){throw 'Sample component differs from source-built producer'}}
    $result.sourceSampleCopiesVerified=$true
    foreach($kind in @('old-consumer','binary-consumer')){
        $folder=Join-Path $out $kind
        foreach($name in @('EmbeddingSample.cs','embedding.html')){Copy-Pinned (Join-Path $base ('Sample/'+$name)) (Join-Path $folder ('Sample/'+$name)) ''}
        $template=if($kind -eq 'old-consumer'){Join-Path $base 'Sample/EmbeddingSample.csproj'}else{Join-Path $root 'Lea.Sdk/EmbeddingSample.csproj'}
        Copy-Pinned $template (Join-Path $folder 'Sample/EmbeddingSample.csproj') ''
        foreach($module in @('Lea.Engine','Lea.Win','Lea.Image')){
            $dir=Join-Path $folder $module;New-Item -ItemType Directory -Path $dir -Force|Out-Null
            $fake=Join-Path $dir ($module+'.csproj')
            [IO.File]::WriteAllText($fake,'<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003"><Target Name="Build"><Error Text="Explicit LeaSdkBin must not build source dependencies" /></Target></Project>');Pin $fake
        }
        $built=Build-Case $kind (Join-Path $folder 'Sample/EmbeddingSample.csproj') $folder @('/p:LeaSdkBin="'+(Join-Path $source 'bin/Release')+'"')
        if(!$built.passed){throw ('Binary consumer compatibility failed: '+$kind)}
        $worker=Join-Path $folder 'Sample/bin/Release/Lea.Image.exe'
        if($kind -eq 'old-consumer'){if(Test-Path -LiteralPath $worker){throw 'Original missing-worker deployment defect not reproduced'};$result.oldWorkerOmissionReproduced=$true}
        else{Pin $worker (Get-FileHash -LiteralPath (Join-Path $source 'bin/Release/Lea.Image.exe')).Hash;$result.binaryConsumerCopiesWorker=$true}
    }
    $closure=@(Get-ChildItem -LiteralPath $sampleBin -File)
    if((Get-PSDrive C).Free -lt (600MB+($closure|Measure-Object Length -Sum).Sum+512KB)){throw 'Actual image runtime allocation plus original reserve required'}
    New-Item -ItemType Directory -Path $runtime|Out-Null
    foreach($file in $closure){Copy-Pinned $file.FullName (Join-Path $runtime $file.Name) ''}
    $probeSource=Join-Path $root 'Lea.Tests/SdkImageViewMain.cs';Pin $probeSource;Copy-Item -LiteralPath $probeSource -Destination (Join-Path $out 'SdkImageViewMain.cs')
    $probe=Join-Path $runtime 'Lea.Sdk.ImageProbe.exe'
    $options=@('/nologo','/noconfig','/nostdlib+','/target:exe','/platform:x64','/warnaserror+',('/out:"'+$probe+'"'),('"'+$probeSource+'"'))
    $framework='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    foreach($name in @('mscorlib','System','System.Core','System.Drawing','System.Windows.Forms')){$p=Join-Path $framework ($name+'.dll');Pin $p;$options+=('/reference:"'+$p+'"')}
    foreach($name in @('Lea.Engine.dll','Lea.Win.dll')){$options+=('/reference:"'+(Join-Path $runtime $name)+'"')}
    $rsp=Join-Path $out 'image-probe.rsp';[IO.File]::WriteAllLines($rsp,$options);Pin $rsp
    $compile=Invoke-OwnedTestRun -Name 'compile-image-probe' -Application $dotnet -Arguments @(('"'+(Join-Path $toolchain 'Roslyn/bincore/csc.dll')+'"'),('@"'+$rsp+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compile.passed){throw 'Image-view probe compile failed'};Pin $probe
    $result.before=@(Get-ChildItem -LiteralPath $runtime -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    $text=Join-Path $out 'text';$assets=Join-Path $out 'assets';New-Item -ItemType Directory -Path $text,$assets|Out-Null
    $embedded=Invoke-OwnedTestRun -Name 'source-sample-text' -Application (Join-Path $runtime 'Lea.EmbeddingSample.exe') -Arguments @('--test',('"'+(Join-Path $runtime 'embedding.html')+'"'),('"'+$text+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$embedded.passed -or @(Get-Content -LiteralPath $embedded.stdout|Where-Object {$_ -ceq 'PASS LEA SDK EMBEDDING 14'}).Count -ne 1){throw 'Source sample text/selection failed'}
    $images=Invoke-OwnedTestRun -Name 'source-sample-images' -Application $probe -Arguments @(('"'+$assets+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$images.passed -or @(Get-Content -LiteralPath $images.stdout|Where-Object {$_ -ceq 'PASS SDK VIEW IMAGES 42'}).Count -ne 1){throw 'Source sample real image decoding failed'}
    $result.textChecks=14;$result.imageChecks=42;$result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $env:TEMP=$oldTemp;$env:TMP=$oldTmp
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@($result.before|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256 -or (Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention -and !$result.error
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaSdkImageGraphLeases=$holds}
    $result|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,oldWorkerOmissionReproduced,sourceSampleCopiesVerified,binaryConsumerCopiesWorker,textChecks,imageChecks,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
