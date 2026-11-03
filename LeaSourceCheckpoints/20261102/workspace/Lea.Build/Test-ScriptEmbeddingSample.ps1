param([switch]$KeepForegroundHost,[ValidateRange(1,9)][int]$Version=1)
$ErrorActionPreference='Stop'
$root='C:\Users\Elliot\Documents\Family Fun Friday Games Night copy'
$out='I:\LeaBuildStorage\script-embedding-sample-build-20261101-v'+$Version
$candidate=Join-Path $root ('continuity-evidence/sdk-script-embedding-v'+$Version)
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained foreground owner required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Test-Path $candidate) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/output and storage reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','SdkScriptLocal','Lea.ScriptEmbeddingSample' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$pin){
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($pin -and $hash -cne $pin){throw 'Script sample input differs'};$inputs.Add(@{path=$path;sha256=$hash});return $hash
}
function CopyVerified([string]$from,[string]$to,[string]$pin){
    $hash=Hold $from $pin;New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null
    Copy-Item -LiteralPath $from -Destination $to;$null=Hold $to $hash
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;compiled=$false;runtimeQualified=$false;completeSdk=$false;hostAdopted=$false;inputs=$inputs;runs=$runs}
try{
    $null=Hold $PSCommandPath
    $owned=Join-Path $root 'Lea.Build/OwnedTestProcess.cs';$null=Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $root 'Lea.Build/Invoke-OwnedTestRun.ps1';$null=Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $proof='I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-script-libraries-net48/result.json'
    $null=Hold $proof 'E3B542B8CAD81E81321E30EBCDF6682F2AC0E371A3DCF6BE0A5B299498793AD7'
    $build=Get-Content $proof -Raw|ConvertFrom-Json
    if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.outputs.Count -ne 2){throw 'Mature source build required'}
    $stage='I:\LeaBuildStorage\sdk-script-20261101-net48-v1';$manifest=Join-Path $stage 'source-manifest.json'
    $pins=@($build.inputs|Where-Object path -ceq $manifest|Select-Object -ExpandProperty sha256 -Unique)
    if($pins.Count -ne 1){throw 'Source manifest provenance required'};$null=Hold $manifest $pins[0]
    foreach($file in (Get-Content $manifest -Raw|ConvertFrom-Json).files|Where-Object {$_.path -match '\.dll$'}){CopyVerified (Join-Path $stage $file.path) (Join-Path $out ('bin/Release/'+[IO.Path]::GetFileName($file.path))) $file.sha256}
    foreach($file in $build.outputs){CopyVerified $file.path (Join-Path $out ('bin/Release/'+[IO.Path]::GetFileName($file.path))) $file.sha256}
    $workerProof='I:\LeaBuildStorage\sdk-script-worker-native-20261101-v1/result.json';$null=Hold $workerProof '81690529F6933FA641969386747B6C9549BBD14D91F29B5CF1D70BF908CE6F97'
    $worker=Get-Content $workerProof -Raw|ConvertFrom-Json
    if(!$worker.passed -or !$worker.inputsUnchanged -or $worker.ownerAttention -or $worker.outputs.Count -ne 6){throw 'Native worker source build required'}
    foreach($file in $worker.outputs){CopyVerified $file.path (Join-Path $out ('bin/Release/ScriptWorker/'+[IO.Path]::GetFileName($file.path))) $file.sha256}
    foreach($name in @('ScriptEmbeddingSample.cs','ScriptEmbeddingSample.csproj','script-embedding.html')){CopyVerified (Join-Path $root ('Lea.Sdk/'+$name)) (Join-Path $out ('Sample/'+$name)) ''}
    CopyVerified (Join-Path $root 'Lea.Sdk/ScriptHost.config') (Join-Path $out 'Sample/ScriptHost.config') ''
    $toolchain='C:\Program Files\dotnet\sdk\8.0.427';$dotnet='C:\Program Files\dotnet\dotnet.exe';$null=Hold $dotnet
    foreach($name in @('MSBuild.dll','Microsoft.Build.dll','Roslyn/bincore/csc.dll','Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll')){$null=Hold (Join-Path $toolchain $name)}
    $project=Join-Path $out 'Sample/ScriptEmbeddingSample.csproj'
    $args=@(('"'+(Join-Path $toolchain 'MSBuild.dll')+'"'),('"'+$project+'"'),'/nologo','/m:1','/nodeReuse:false','/t:Rebuild','/v:minimal','/p:Configuration=Release','/p:UseSharedCompilation=false')
    $compiled=Invoke-OwnedTestRun -Name 'build-script-sample' -Application $dotnet -Arguments $args -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compiled.passed){throw 'Script sample project compilation failed'};$result.compiled=$true
    $runtime=Join-Path $out 'Sample/bin/Release';$products=@(Get-ChildItem -LiteralPath $runtime -Recurse -File)
    foreach($file in Get-ChildItem (Join-Path $out 'bin/Release') -File -Filter '*.dll'){
        if((Get-FileHash (Join-Path $runtime $file.Name)).Hash -cne (Get-FileHash $file.FullName).Hash){throw 'Copied parent library identity differs'}
    }
    foreach($file in $worker.outputs){if((Get-FileHash (Join-Path $runtime ('ScriptWorker/'+[IO.Path]::GetFileName($file.path)))).Hash -cne $file.sha256){throw 'Copied worker identity differs'}}
    if(@(Get-ChildItem (Join-Path $runtime 'ScriptWorker') -File).Count -ne 6){throw 'Exact six worker files required'}
    if((Get-Volume -DriveLetter C).FileSystem -cne 'NTFS' -or (Get-PSDrive C).Free -lt (600MB+($products|Measure-Object Length -Sum).Sum)){throw 'Fresh NTFS runtime plus unchanged reserve required'}
    foreach($file in $products){CopyVerified $file.FullName (Join-Path $candidate ([IO.Path]::GetRelativePath($runtime,$file.FullName))) ''}
    $result.workerAclBefore=@(Get-ChildItem (Join-Path $candidate 'ScriptWorker') -File|ForEach-Object {@{path=$_.FullName;sddl=(Get-Acl $_.FullName).Sddl}})
    $exe=Join-Path $candidate 'Lea.ScriptEmbeddingSample.exe'
    $tested=Invoke-OwnedTestRun -Name 'test-script-sample' -Application $exe -Arguments @('--test',('"'+$candidate+'"')) -WorkingDirectory $candidate -OutputDirectory $out -Seconds 30 -Records $runs
    $result.passed=$tested.passed -and @(Get-Content $tested.stdout|Where-Object {$_ -ceq 'PASS LEA SDK SCRIPT SAMPLE 17'}).Count -eq 1
    $result.candidate=$candidate
}catch{$result.error=$_.Exception.ToString()}
finally{
    if($result.workerAclBefore){$result.workerAclRestored=@($result.workerAclBefore|Where-Object {(Get-Acl $_.path).Sddl -cne $_.sddl}).Count -eq 0;$result.passed=$result.passed -and $result.workerAclRestored}
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaScriptSampleLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object compiled,passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
