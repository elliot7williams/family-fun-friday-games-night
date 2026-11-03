param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\repro-sdk-consumers-20261101-v1'
$candidate=Join-Path $root 'continuity-evidence/repro-sdk-consumers-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Test-Path $candidate) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/evidence/NTFS samples and original reserve required'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','Lea.EmbeddingSample','Lea.ScriptEmbeddingSample','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'No test/browser overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$expected){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse deterministic consumer input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($expected -and $expected -cne $hash){throw ('Consumer input differs: '+$path)};$inputs.Add(@{path=$path;sha256=$hash})
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;fullSdkQualified=$false;hostAdopted=$false;inputs=$inputs;runs=$runs;workerAclBefore=@()}
try{
    Hold $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $proof='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1/result.json';Hold $proof '39955D0AC2D23B60360000414FDB3702F928B4CA1537C074D2C6007BD174B199'
    $build=Get-Content $proof -Raw|ConvertFrom-Json
    if(!$build.passed -or !$build.binaryReproducible -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.products.Count -ne 16 -or $build.differences.Count){throw 'Passing exact reproducible products required'}
    foreach($file in $build.products){Hold $file.first $file.sha256;Hold $file.second $file.secondSha256}
    $source=$build.roots[0];$rows=[Collections.Generic.List[object]]::new()
    foreach($pair in @(@('Sample/bin/Release','text'),@('ScriptSample/bin/Release','script'))){
        $directory=Join-Path $source $pair[0]
        foreach($file in Get-ChildItem -LiteralPath $directory -Recurse -File){Hold $file.FullName;$rows.Add(@{path=$pair[1]+'/'+[IO.Path]::GetRelativePath($directory,$file.FullName);origin=$file.FullName;sha256=(Get-FileHash $file.FullName).Hash;bytes=$file.Length})}
    }
    if((Get-Volume -DriveLetter C).FileSystem -cne 'NTFS' -or (Get-PSDrive C).Free -lt (600MB+($rows|Measure-Object bytes -Sum).Sum)){throw 'Original reserve after sample staging required'}
    foreach($row in $rows){
        if([IO.Path]::IsPathRooted($row.path) -or $row.path.Contains(':') -or $row.path.Split([char[]]@('/','\')) -contains '..'){throw 'Consumer staging path escape'}
        $to=Join-Path $candidate $row.path;New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null;Copy-Item -LiteralPath $row.origin -Destination $to;Hold $to $row.sha256
    }
    $textRoot=Join-Path $candidate 'text';$scriptRoot=Join-Path $candidate 'script'
    $result.candidate=$candidate
    $result.workerAclBefore=@(Get-ChildItem -LiteralPath (Join-Path $scriptRoot 'ScriptWorker') -File|ForEach-Object {@{path=$_.FullName;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    if($result.workerAclBefore.Count -ne 6){throw 'Exact worker ACL closure required'}
    $text=Invoke-OwnedTestRun -Name 'text-consumer' -Application (Join-Path $textRoot 'Lea.EmbeddingSample.exe') -Arguments @('--test',('"'+(Join-Path $textRoot 'embedding.html')+'"'),('"'+$out+'"')) -WorkingDirectory $textRoot -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$text.passed -or @(Get-Content $text.stdout|Where-Object {$_ -ceq 'PASS LEA SDK EMBEDDING 14'}).Count -ne 1){throw 'Deterministic text consumer failed'}
    $script=Invoke-OwnedTestRun -Name 'script-consumer' -Application (Join-Path $scriptRoot 'Lea.ScriptEmbeddingSample.exe') -Arguments @('--test',('"'+$scriptRoot+'"')) -WorkingDirectory $scriptRoot -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$script.passed -or @(Get-Content $script.stdout|Where-Object {$_ -ceq 'PASS LEA SDK SCRIPT SAMPLE 17'}).Count -ne 1){throw 'Deterministic script consumer failed'}
    $exe=Join-Path $scriptRoot 'ReproSdkWorkerGate.exe';$config=Join-Path $root 'Lea.Sdk/ScriptHost.config';Hold $config
    Copy-Item -LiteralPath $config -Destination ($exe+'.config');Hold ($exe+'.config') (Get-FileHash $config).Hash
    $fixture=Join-Path $root 'LeaForge/test-pages/sdk-script-local.html';Hold $fixture
    $targetFixture=Join-Path $scriptRoot 'worker-gate.html';Copy-Item -LiteralPath $fixture -Destination $targetFixture;Hold $targetFixture (Get-FileHash $fixture).Hash
    $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe';Hold $compiler
    $args=@('/nologo','/noconfig','/nostdlib+','/target:exe','/platform:x64','/warnaserror+',('/out:"'+$exe+'"'))
    $refs='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    foreach($name in @('mscorlib.dll','System.dll','System.Core.dll','System.Drawing.dll','System.Windows.Forms.dll')){$path=Join-Path $refs $name;Hold $path;$args+=('/reference:"'+$path+'"')}
    foreach($name in @('Lea.Engine.dll','Lea.Win.dll','Lea.Script.dll','Lea.Script.Win.dll','Jint.dll','Acornima.dll')){$args+=('/reference:"'+(Join-Path $scriptRoot $name)+'"')}
    $gateSource=Join-Path $root 'Lea.Tests/SdkScriptWorkerMain.cs';Hold $gateSource;$args+=('"'+$gateSource+'"')
    $compiled=Invoke-OwnedTestRun -Name 'compile-worker-gate' -Application $compiler -Arguments $args -WorkingDirectory $scriptRoot -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compiled.passed){throw 'Deterministic worker probe compilation failed'};Hold $exe
    $tested=Invoke-OwnedTestRun -Name 'worker-gate' -Application $exe -Arguments @(('"'+$targetFixture+'"')) -WorkingDirectory $scriptRoot -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$tested.passed -or @(Get-Content $tested.stdout|Where-Object {$_ -ceq 'PASS SDK WORKER 25'}).Count -ne 1){throw 'Deterministic worker isolation/lifecycle checks failed'}
    $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.workerAclRestored=@($result.workerAclBefore|Where-Object {(Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and $result.workerAclRestored -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaReproSdkConsumerLeases=$holds}
    $result|ConvertTo-Json -Depth 9|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,inputsUnchanged,workerAclRestored,ownerAttention,error|ConvertTo-Json
}
