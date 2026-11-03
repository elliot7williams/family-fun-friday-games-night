param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$stage='I:\LeaBuildStorage\full-sdk-source-20261101-v2'
$out='I:\LeaBuildStorage\portable-full-sdk-build-20261101-v1'
$candidate=Join-Path $root 'continuity-evidence/portable-full-sdk-samples-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Test-Path $candidate) -or (Test-Path (Join-Path $stage 'bin')) -or (Test-Path (Join-Path $stage 'worker-il')) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh source build/owner/NTFS samples and original reserve required'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','Lea.EmbeddingSample','Lea.ScriptEmbeddingSample','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'No test/browser overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$pin){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse exported SDK input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($pin -and $pin -cne $hash){throw ('Exported SDK input differs: '+$path)};$inputs.Add(@{path=$path;sha256=$hash})
}
function Proof([string]$path,[string]$hash){Hold $path $hash;return Get-Content -LiteralPath $path -Raw|ConvertFrom-Json}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;sourceBuildPassed=$false;fullSdkQualified=$false;hostAdopted=$false;inputs=$inputs;runs=$runs;workerAclBefore=@()}
try{
    Hold $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $manifest=Proof (Join-Path $stage 'source-manifest.json') 'E796F6ED2A1AA4C5A18962A1A03D75359EA951A4A35DC117DF1411EEDD8E3A2C'
    if(!$manifest.verified -or !$manifest.portableNativeBuildIncluded -or !$manifest.workerProjectInverseVerified -or $manifest.productBinariesIncluded -or $manifest.files.Count -ne 131 -or $manifest.thirdPartyDependencyFiles -ne 8){throw 'Source-only exported SDK incomplete'}
    foreach($file in $manifest.files){Hold (Join-Path $stage $file.path) $file.sha256}
    $portable=Proof 'I:\LeaBuildStorage\portable-native-build-20261101-v1/result.json' 'DB9683B3B9F271930993BC72036E093A948A48B0184DF998FE05D070F2FD17FA'
    if(!$portable.passed -or !$portable.identicalToQualifiedWorker -or !$portable.inputsUnchanged -or $portable.ownerAttention -or $portable.checks.Count -ne 8){throw 'Native orchestration controls incomplete'}
    $toolchain='I:\LeaBuildStorage\portable-native-build-20261101-v1/toolchain.json';Hold $toolchain '7B13FE166AD9AF8A3B142C40CB046EAABA1A33759B5A9C505F97B7A83678C003'
    $config=Get-Content $toolchain -Raw|ConvertFrom-Json
    foreach($file in $config.compilerFiles){Hold (Join-Path $config.compilerRoot $file.path) $file.sha256}
    foreach($file in $config.runtimeFiles){Hold (Join-Path $config.runtimeRoot $file.path) $file.sha256}
    $project=Join-Path $stage 'SDK.Build.proj';$xml=[xml](Get-Content $project -Raw)
    $aliases=@{Bl='BuildLibraries';Bi='BuildWorkerIl';Bn='BuildWorkerNative';Bs='BuildSamples';Ba='BuildAll'}
    foreach($name in $aliases.Keys){$target=@($xml.Project.Target|Where-Object Name -ceq $name);if($target.Count -ne 1 -or $target[0].DependsOnTargets -cne $aliases[$name]){throw 'Short build target mapping differs'}}
    $pwsh=(Get-Process -Id $PID).Path;Hold $pwsh
    $dotnet='C:\Program Files\dotnet\dotnet.exe';Hold $dotnet;$tools='C:\Program Files\dotnet\sdk\8.0.427'
    foreach($name in @('MSBuild.dll','Microsoft.Build.dll','Roslyn/bincore/csc.dll','Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll')){Hold (Join-Path $tools $name)}
    $args=@(('"'+(Join-Path $tools 'MSBuild.dll')+'"'),('"'+$project+'"'),'/nologo','/m:1','/nodeReuse:false','/t:Ba','/v:minimal','/p:Configuration=Release','/p:UseSharedCompilation=false',('/p:LeaPowerShell="'+$pwsh+'"'),('/p:LeaNativeToolchain="'+$toolchain+'"'),'/p:LeaNativeToolchainSha256=7B13FE166AD9AF8A3B142C40CB046EAABA1A33759B5A9C505F97B7A83678C003')
    $built=Invoke-OwnedTestRun -Name 'build-exported-sdk' -Application $dotnet -Arguments $args -WorkingDirectory $stage -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$built.passed){throw 'Complete exported source build failed'}
    $result.sourceBuildPassed=$true
    $productRoot=Join-Path $stage 'bin/Release'
    $result.libraryProducts=@(foreach($name in @('Lea.Engine.dll','Lea.Win.dll','Lea.Image.exe','Lea.Print.dll','Lea.Script.dll','Lea.Script.Win.dll')){$path=Join-Path $productRoot $name;Hold $path;@{path=$path;sha256=(Get-FileHash $path).Hash}})
    $nativeRoot=Join-Path $productRoot 'ScriptWorker'
    $result.nativeProducts=@(foreach($file in Get-ChildItem -LiteralPath $nativeRoot -File){Hold $file.FullName;@{path=$file.FullName;sha256=(Get-FileHash $file.FullName).Hash}})
    if($result.nativeProducts.Count -ne 6 -or @(Get-Content $built.stdout|Where-Object {$_ -match 'LEA_NATIVE_BUILT'}).Count -ne 1){throw 'Source native build closure absent'}
    $rows=[Collections.Generic.List[object]]::new()
    foreach($pair in @(@('Sample/bin/Release','text'),@('ScriptSample/bin/Release','script'))){
        $directory=Join-Path $stage $pair[0]
        foreach($file in Get-ChildItem -LiteralPath $directory -Recurse -File){$relative=$pair[1]+'/'+[IO.Path]::GetRelativePath($directory,$file.FullName);Hold $file.FullName;$rows.Add(@{path=$relative;origin=$file.FullName;sha256=(Get-FileHash $file.FullName).Hash;bytes=$file.Length})}
    }
    $bytes=($rows|Measure-Object bytes -Sum).Sum
    if((Get-Volume -DriveLetter C).FileSystem -cne 'NTFS' -or (Get-PSDrive C).Free -lt (600MB+$bytes)){throw 'Preserve original reserve after NTFS sample staging'}
    foreach($row in $rows){
        if([IO.Path]::IsPathRooted($row.path) -or $row.path.Contains(':') -or $row.path.Split([char[]]@('/','\')) -contains '..'){throw 'Sample staging path escape'}
        $to=Join-Path $candidate $row.path;New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null;Copy-Item -LiteralPath $row.origin -Destination $to;Hold $to $row.sha256
    }
    $scriptRoot=Join-Path $candidate 'script'
    $result.workerAclBefore=@(Get-ChildItem -LiteralPath (Join-Path $scriptRoot 'ScriptWorker') -File|ForEach-Object {@{path=$_.FullName;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    if($result.workerAclBefore.Count -ne 6){throw 'Six-file NTFS worker closure required'}
    $textRoot=Join-Path $candidate 'text'
    $text=Invoke-OwnedTestRun -Name 'test-text-consumer' -Application (Join-Path $textRoot 'Lea.EmbeddingSample.exe') -Arguments @('--test',('"'+(Join-Path $textRoot 'embedding.html')+'"'),('"'+$out+'"')) -WorkingDirectory $textRoot -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$text.passed -or @(Get-Content $text.stdout|Where-Object {$_ -ceq 'PASS LEA SDK EMBEDDING 14'}).Count -ne 1){throw 'Source-built text consumer failed'}
    $script=Invoke-OwnedTestRun -Name 'test-script-consumer' -Application (Join-Path $scriptRoot 'Lea.ScriptEmbeddingSample.exe') -Arguments @('--test',('"'+$scriptRoot+'"')) -WorkingDirectory $scriptRoot -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$script.passed -or @(Get-Content $script.stdout|Where-Object {$_ -ceq 'PASS LEA SDK SCRIPT SAMPLE 17'}).Count -ne 1){throw 'Source-built script consumer failed'}
    $result.samplesPassed=$true;$result.candidate=$candidate;$result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.workerAclRestored=@($result.workerAclBefore|Where-Object {(Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and $result.workerAclRestored -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaPortableFullSdkInputLeases=$holds}
    $result|ConvertTo-Json -Depth 9|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,sourceBuildPassed,samplesPassed,inputsUnchanged,workerAclRestored,ownerAttention,error|ConvertTo-Json
}
