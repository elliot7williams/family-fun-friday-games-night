param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$base='I:\LeaBuildStorage\sdk-image-source-graph-20261109-v1'
$out='I:\LeaBuildStorage\sdk-print-source-graph-20261109-v1'
$source=Join-Path $out 'source'
$runtime=Join-Path $root 'continuity-evidence/sdk-print-source-graph-v1/runtime'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 620MB -or (Get-PSDrive I).Free -lt 128MB -or (Test-Path -LiteralPath $out) -or (Test-Path -LiteralPath $runtime) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh outputs/owner and original reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Pin([string]$Path,[string]$Expected){
    $walk=Get-Item -LiteralPath $Path
    while($walk){if($walk.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse input refused'};$walk=if($walk -is [IO.FileInfo]){$walk.Directory}else{$walk.Parent}}
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($stream)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($Expected -and $hash -cne $Expected){throw ('Input differs: '+$Path)};$inputs.Add(@{path=$Path;sha256=$hash})
}
function Copy-Pinned([string]$From,[string]$To,[string]$Expected){Pin $From $Expected;New-Item -ItemType Directory -Path (Split-Path $To -Parent) -Force|Out-Null;Copy-Item -LiteralPath $From -Destination $To;Pin $To (Get-FileHash -LiteralPath $From).Hash}
function Read-Proof([string]$Path,[string]$Expected){Pin $Path $Expected;return Get-Content -LiteralPath $Path -Raw|ConvertFrom-Json}
function Copy-ProofMember($Proof,[string]$From,[string]$To){$pins=@($Proof.inputs|Where-Object path -CEQ $From|Select-Object -ExpandProperty sha256 -Unique);if($pins.Count -ne 1){throw ('Ambiguous source pin: '+$From)};Copy-Pinned $From $To $pins[0]}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;before=@();completeSdk=$false;startupFixed=$false;hostAdopted=$false;scriptWorkerQualified=$false;printerDispatched=$false;testHarnessesRecompiled=$false;productBinariesSeeded=$false}
$oldTemp=$env:TEMP;$oldTmp=$env:TMP
try{
    Pin $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'test.ps1')
    $audit=Read-Proof 'I:\LeaBuildStorage\sdk-image-source-graph-audit-20261109-v1\audit.json' '6C654D97E50047ABEE9BB2F3FA6C3030B0732B0596F74127E4572B644DE12A4E'
    if(!$audit.verified -or $audit.sourceFiles.Count -ne 75){throw 'Verified mature source layout required'}
    foreach($file in $audit.sourceFiles){
        if([IO.Path]::IsPathRooted($file.path) -or $file.path.Contains(':') -or $file.path.Split('/') -contains '..'){throw 'Source scope escape'}
        Copy-Pinned (Join-Path $base ('source/'+$file.path)) (Join-Path $source $file.path) $file.sha256
    }
    $prior=Read-Proof (Join-Path $base 'result.json') '177FDA95290C6AEC605325072AEB598CC7A39A5F552910BA3FD1083DD093648F'
    foreach($file in Get-ChildItem -LiteralPath (Join-Path $base 'source/lib') -File){Copy-ProofMember $prior $file.FullName (Join-Path $source ('lib/'+$file.Name))}
    $printRoot=Join-Path $root 'continuity-evidence/relative-font-print-project-v1'
    $print=Read-Proof (Join-Path $printRoot 'result.json') 'A71A3BC94E9584876DCE5480027EBAD4585A2900E6F01651C65D4869FA2D8002'
    if(!$print.passed -or !$print.inputsUnchanged -or $print.ownerAttention){throw 'Print source receipt incomplete'}
    $printFiles=@(Get-ChildItem -LiteralPath (Join-Path $printRoot 'source') -File|Where-Object Extension -in @('.cs','.csproj'))
    if($printFiles.Count -ne 23){throw 'Print source coverage differs'}
    foreach($file in $printFiles){
        Copy-ProofMember $print $file.FullName (Join-Path $out ('old-control/Lea.Print/'+$file.Name))
        if($file.Extension -eq '.cs'){Copy-ProofMember $print $file.FullName (Join-Path $source ('Lea.Print/'+$file.Name))}
    }
    Copy-Pinned (Join-Path $root 'Lea.Sdk/Lea.Print.csproj') (Join-Path $source 'Lea.Print/Lea.Print.csproj') ''
    if(@(Get-ChildItem -LiteralPath $source -Recurse -File|Where-Object {$_.Extension -in @('.dll','.exe') -and !$_.FullName.StartsWith((Join-Path $source 'lib')+'\',[StringComparison]::OrdinalIgnoreCase)}).Count){throw 'Lea binary seeded into producer'}
    $owner=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Pin $owner 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owner
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Pin $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $dotnet='C:\Program Files\dotnet\dotnet.exe';Pin $dotnet
    $toolchain='C:\Program Files\dotnet\sdk\8.0.427';$msbuild=Join-Path $toolchain 'MSBuild.dll';Pin $msbuild
    foreach($name in @('Roslyn/bincore/csc.dll','Roslyn/bincore/csc.deps.json','Roslyn/bincore/csc.runtimeconfig.json','Roslyn/bincore/Microsoft.CodeAnalysis.dll','Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll')){Pin (Join-Path $toolchain $name)}
    $env:TEMP=Join-Path $out 'temp';$env:TMP=$env:TEMP;New-Item -ItemType Directory -Path $env:TEMP|Out-Null
    function Build-Case([string]$Name,[string]$Project,[string]$Folder,[string[]]$Extra){
        $arguments=@(('"'+$msbuild+'"'),('"'+$Project+'"'),'/nologo','/m:1','/nodeReuse:false','/t:Build','/v:minimal','/p:Configuration=Release','/p:UseSharedCompilation=false')+@($Extra)
        return Invoke-OwnedTestRun -Name $Name -Application $dotnet -Arguments $arguments -WorkingDirectory $Folder -OutputDirectory $out -Seconds 120 -Records $runs
    }
    $old=Build-Case 'old-print-control' (Join-Path $out 'old-control/Lea.Print/Lea.Print.csproj') (Join-Path $out 'old-control') @()
    if($old.exitCode -eq 0 -or $old.timedOut -or $old.killRequested -or (Get-Content -LiteralPath $old.stdout -Raw) -notmatch 'Set LeaCorePath to the matching mature Lea.Engine.dll'){throw 'Original binary-only source-build failure not reproduced'}
    $result.oldDependencyFailureReproduced=$true
    $built=Build-Case 'clean-source-print' (Join-Path $source 'Lea.Print/Lea.Print.csproj') $source @()
    if(!$built.passed){throw 'Clean direct print source build failed'}
    foreach($name in @('Lea.Engine.dll','Lea.Win.dll','Lea.Print.dll')){Pin (Join-Path $source ('bin/Release/'+$name))}
    $sample=Build-Case 'source-sample-image' (Join-Path $source 'Sample/EmbeddingSample.csproj') $source @()
    if(!$sample.passed){throw 'Image/sample source build failed'}
    $adapter=Build-Case 'source-script-adapter' (Join-Path $source 'Lea.Script.Win/Lea.Script.Win.csproj') $source @()
    if(!$adapter.passed){throw 'Script library source build failed'}
    $result.products=@(foreach($name in @('Lea.Engine.dll','Lea.Win.dll','Lea.Image.exe','Lea.Print.dll','Lea.Script.dll','Lea.Script.Win.dll')){$path=Join-Path $source ('bin/Release/'+$name);Pin $path;@{name=$name;path=$path;sha256=(Get-FileHash -LiteralPath $path).Hash}})
    $binary=Join-Path $out 'binary-control'
    foreach($file in Get-ChildItem -LiteralPath (Join-Path $source 'Lea.Print') -File){Copy-Pinned $file.FullName (Join-Path $binary ('Lea.Print/'+$file.Name)) ''}
    foreach($name in @('Lea.Engine','Lea.Win')){
        $folder=Join-Path $binary $name;New-Item -ItemType Directory -Path $folder|Out-Null
        $fake=Join-Path $folder ($name+'.csproj')
        [IO.File]::WriteAllText($fake,'<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003"><Target Name="Build"><Error Text="Explicit print binary paths must not build source projects" /></Target></Project>');Pin $fake
    }
    $explicit=Build-Case 'explicit-binary-print' (Join-Path $binary 'Lea.Print/Lea.Print.csproj') $binary @(('/p:LeaCorePath="'+(Join-Path $source 'bin/Release/Lea.Engine.dll')+'"'),('/p:LeaViewPath="'+(Join-Path $source 'bin/Release/Lea.Win.dll')+'"'))
    if(!$explicit.passed){throw 'Explicit print binary compatibility failed'};$result.explicitBinaryPathsPreserved=$true
    if((Get-PSDrive C).Free -lt 620MB){throw 'Original runtime reserve required'}
    New-Item -ItemType Directory -Path $runtime|Out-Null
    foreach($name in @('Lea.Engine.dll','Lea.Win.dll','Lea.Print.dll','Lea.Image.exe')){Copy-Pinned (Join-Path $source ('bin/Release/'+$name)) (Join-Path $runtime $name) ''}
    foreach($name in @('PrintTextChecks.exe','Lea.Print.Range.Tests.exe','native-selection.html','print-document.html','print-styles.html','print-styled-document.html')){Copy-ProofMember $print (Join-Path $printRoot ('bin/'+$name)) (Join-Path $runtime $name)}
    $imageRoot=Join-Path $root 'continuity-evidence/relative-font-image-live-v1'
    $image=Read-Proof (Join-Path $imageRoot 'result.json') '41EB69CB05327FF8FE6C4226543EBE2ED75B8662E3DBB8B709B528B19D4BF1CA'
    if(!$image.passed -or !$image.inputsUnchanged -or !$image.decoderAclRestored -or $image.ownerAttention){throw 'Original image harness receipt incomplete'}
    Copy-ProofMember $image (Join-Path $imageRoot 'PrintFriendlyChecks.exe') (Join-Path $runtime 'PrintFriendlyChecks.exe')
    foreach($name in @('print-friendly.html','print-cache-image.html','document-selection.html')){Copy-ProofMember $image (Join-Path $root ('LeaForge/test-pages/'+$name)) (Join-Path $runtime $name)}
    $result.before=@(Get-ChildItem -LiteralPath $runtime -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    $capture=Invoke-OwnedTestRun -Name 'source-print-capture' -Application (Join-Path $runtime 'PrintTextChecks.exe') -Arguments @('"'+(Join-Path $runtime 'native-selection.html')+'"') -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$capture.passed -or @(Get-Content -LiteralPath $capture.stdout|Where-Object {$_ -ceq 'PASS Lea live styled capture and foundation'}).Count -ne 1){throw 'Source print capture/export failed'};$result.capturePassed=$true
    $range=Invoke-OwnedTestRun -Name 'source-print-range' -Application (Join-Path $runtime 'Lea.Print.Range.Tests.exe') -Arguments @() -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
    foreach($footer in @('PASS Lea print range: 37','PASS Lea printer geometry: 16','PASS Lea native printer selection: 27','PASS Lea prepared printer ticket: 22')){if(@(Get-Content -LiteralPath $range.stdout|Where-Object {$_ -ceq $footer}).Count -ne 1){throw 'Print range/device/ticket footer differs'}}
    if(!$range.passed){throw 'Print range suite failed'};$result.rangeChecks=102
    $images=Invoke-OwnedTestRun -Name 'source-print-images' -Application (Join-Path $runtime 'PrintFriendlyChecks.exe') -Arguments @(('"'+(Join-Path $runtime 'print-friendly.html')+'"'),('"'+$runtime+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
    $result.friendly=Get-Content -LiteralPath (Join-Path $runtime 'friendly.json') -Raw|ConvertFrom-Json
    $result.live=Get-Content -LiteralPath (Join-Path $runtime 'live.json') -Raw|ConvertFrom-Json
    if(!$images.passed -or !$result.friendly.passed -or $result.friendly.baselineChecks -ne 40 -or $result.friendly.friendlyChecks -ne 46 -or $result.friendly.browserRouted -or $result.friendly.printJobSubmitted -or !$result.live.passed -or $result.live.liveChecks -ne 250 -or $result.live.printJobSubmitted -or $result.live.hostRouted){throw 'Source print image/live checks failed'}
    $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $env:TEMP=$oldTemp;$env:TMP=$oldTmp
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@($result.before|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256 -or (Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention -and !$result.error
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaSdkPrintGraphLeases=$holds}
    $result|ConvertTo-Json -Depth 9|Set-Content -LiteralPath (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,oldDependencyFailureReproduced,explicitBinaryPathsPreserved,capturePassed,rangeChecks,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
