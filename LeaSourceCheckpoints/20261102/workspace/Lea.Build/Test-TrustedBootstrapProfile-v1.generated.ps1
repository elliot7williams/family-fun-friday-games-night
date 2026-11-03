param([switch]$KeepForegroundHost,[ValidateSet(1,2,3)][int]$Version=2,[switch]$Isolated)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$out=Join-Path $root 'continuity-evidence/trusted-bootstrap-profile-v1'
if($Isolated){if($Version -ne 3){throw 'Isolated connectivity requires fresh worker candidate'};$out=Join-Path $root 'continuity-evidence/is-connected-isolated-v1'}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Test-Path $out) -or (Get-PSDrive C).Free -lt 600MB -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/output/reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No test/browser overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new();$runs=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$hash){$s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);if(!$hash){$hash=(Get-FileHash $path).Hash};if((Get-FileHash $path).Hash -cne $hash){throw 'Connectivity test input differs'};$inputs.Add(@{path=$path;sha256=$hash})}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;before=@();isolatedWorkerTested=$false;browserAdopted=$false}
try{
    Pin $PSCommandPath
    $buildPath='I:\LeaBuildStorage\trusted-bootstrap-profile-build-20261102-v1/result.json';Pin $buildPath 'A9100D89DE8B062DB8CFD0CA1E6755FD62B422421AF4999BB49C363943BA29AF'
    $b=Get-Content $buildPath -Raw|ConvertFrom-Json
    if(!$b.passed -or !$b.inputsUnchanged -or $b.ownerAttention -or $b.sha256 -cne '7197F985895A8FA74B0AB7C81F382FD2BB1CFC3EFB994250D8DFD878F787CAFF'){throw 'Connectivity build incomplete'}
    $runtime=Join-Path $out 'runtime';New-Item -ItemType Directory -Path $runtime|Out-Null
    $dlls=@(Get-ChildItem (Split-Path $b.binary) -Filter '*.dll' -File)
    if($dlls.Count -ne 8){throw 'Eight-file trusted closure required'}
    foreach($file in $dlls){
        $expected=if($file.Name -ceq 'Lea.Script.dll'){$b.sha256}else{@($b.inputs|Where-Object {([IO.Path]::GetFileName($_.path)) -ceq $file.Name}|Select-Object -ExpandProperty sha256 -Unique)}
        if(@($expected).Count -ne 1){throw 'Unique trusted dependency identity required'}
        Pin $file.FullName $expected;Copy-Item -LiteralPath $file.FullName -Destination $runtime
    }
    if($Isolated){
        $workerProof='I:\LeaBuildStorage\is-connected-worker-build-20261102-v1/result.json';Pin $workerProof 'DB8F6F426B4773E821A7930B46BA74B59E46802B540BFFB89DD775B5550AFCB6'
        $worker=Get-Content $workerProof -Raw|ConvertFrom-Json
        if(!$worker.passed -or !$worker.inputsUnchanged -or $worker.ownerAttention -or $worker.products.Count -ne 6){throw 'Matching isolated worker build incomplete'}
        $workerDirectory=Join-Path $runtime 'ScriptWorker';New-Item -ItemType Directory -Path $workerDirectory|Out-Null
        foreach($file in $worker.products){Pin $file.path $file.sha256;Copy-Item -LiteralPath $file.path -Destination $workerDirectory}
    }
    $fixture=Join-Path $root 'continuity-evidence/deterministic-full-replay-v1/fixtures/LeaForge/test-pages/javascript-contains.html';Pin $fixture

    $source=Join-Path $root 'Lea.Tests/BootstrapComparisonMain.cs';Pin $source
    Pin (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    Pin (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    $exe=Join-Path $runtime 'Lea.IsConnected.Tests.exe'
    $config=Join-Path $root 'continuity-evidence/deterministic-full-replay-v1/runtime/Lea.Script.Tests.exe.config'
    Pin $config 'BBBBB19D16CE7AE2715A07FC065A55FAA0BB8DE5BC46E4F243D496320C9FC2EC'
    Copy-Item -LiteralPath $config -Destination ($exe+'.config')
    $options=[Collections.Generic.List[string]]::new()
    foreach($arg in @('/nologo','/noconfig','/nostdlib+','/target:exe','/langversion:12.0','/optimize+','/deterministic+',('/out:"'+$exe+'"'),('"'+$source+'"'))){$options.Add($arg)}
    foreach($name in @('mscorlib','System','System.Core','System.Drawing','System.Windows.Forms','System.Web.Extensions')){$path='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\'+$name+'.dll';Pin $path;$options.Add('/reference:"'+$path+'"')}
    foreach($name in @('Lea.Engine.dll','Lea.Script.dll')){$options.Add('/reference:"'+(Join-Path $runtime $name)+'"')}
    $rsp=Join-Path $out 'compile.rsp';[IO.File]::WriteAllLines($rsp,$options);Pin $rsp
    $dotnet='C:\Program Files\dotnet\dotnet.exe';Pin $dotnet
    $csc='C:\Program Files\dotnet\sdk\8.0.427\Roslyn\bincore\csc.dll';Pin $csc
    $compile=Invoke-OwnedTestRun -Name 'compile-connectivity-test' -Application $dotnet -Arguments @(('"'+$csc+'"'),('@"'+$rsp+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compile.passed){throw 'Connectivity harness compile failed'}
    if($Isolated){throw 'Trusted cold profile only'}
    $result.diagnosticOnly=$true;$result.runtimeQualified=$false
    $result.before=@(Get-ChildItem $runtime -Recurse -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}})
    $run=Invoke-OwnedTestRun -Name 'cold-trusted-profile' -Application $exe -Arguments @(('"'+$fixture+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
    $report=@(Get-Content $run.stdout|Where-Object {$_.StartsWith('LEA_BOOTSTRAP_COMPARISON ')})
    $rows=@(Get-Content $run.stderr|Where-Object {$_.StartsWith('LEA_TRUSTED_BOOT ')})
    $result.measurementCaptured=$report.Count -eq 1 -and $rows.Count -gt 0 -and $run.outputDrained -and $run.exitObserved
    if($report.Count -eq 1){$result.report=$report[0].Substring('LEA_BOOTSTRAP_COMPARISON '.Length)|ConvertFrom-Json}
    $result.profileRows=$rows.Count
    $result.passed=$run.passed -and $result.measurementCaptured -and $result.report.ok
    $result.scope='One cold diagnostic trusted bootstrap with buffered CPU/wall/GC stamps. Original script policy and30s/5s ownership retained. Observer effects possible; not a product fix/feature/reliability qualification.'
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@($result.before|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256 -or (Get-Acl $_.path).Sddl -cne $_.sddl})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention -and !$result.error
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaConnectivityLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json -Compress
}
