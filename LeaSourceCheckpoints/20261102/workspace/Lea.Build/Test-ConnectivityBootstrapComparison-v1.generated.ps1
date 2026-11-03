param([switch]$KeepForegroundHost,[ValidateSet(1,2,3)][int]$Version=2,[switch]$Isolated)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$out=Join-Path $root 'continuity-evidence/connectivity-bootstrap-comparison-v1'
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
    $buildPath='I:\LeaBuildStorage\is-connected-script-build-20261102-v1/result.json';Pin $buildPath '2596ED5B2B5E5210324DBE403C1526E029E4F99240080C4434DCD028445D46F9'
    $b=Get-Content $buildPath -Raw|ConvertFrom-Json
    if(!$b.passed -or !$b.inputsUnchanged -or $b.ownerAttention -or $b.sha256 -cne 'FBD50D216D724A4BFCA0488C53CB1B648BC659AAE19F800AE7E129B207EBBD4C'){throw 'Connectivity build incomplete'}
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
    $cases=Join-Path $root 'Lea.Tests/IsConnectedCases.js';Pin $cases
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
    if($Isolated){throw 'Trusted comparison only'}
    $old='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1\a\bin\Release\Lea.Script.dll'
    Pin $old 'D2685E2F9AC15A72373CEE7B2B5EA69A7DD46605E1C058CB32406A085D7F6518'
    $baseline=Join-Path $out 'baseline'
    Copy-Item -LiteralPath $runtime -Destination $baseline -Recurse
    Copy-Item -LiteralPath $old -Destination (Join-Path $baseline 'Lea.Script.dll')
    foreach($file in Get-ChildItem $runtime -File){
        $other=Join-Path $baseline $file.Name
        $expected=if($file.Name -ceq 'Lea.Script.dll'){'D2685E2F9AC15A72373CEE7B2B5EA69A7DD46605E1C058CB32406A085D7F6518'}else{(Get-FileHash $file.FullName).Hash}
        Pin $other $expected
    }
    $result.before=@(foreach($directory in @($runtime,$baseline)){Get-ChildItem $directory -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}}})
    $result.comparisonCompleted=$false;$result.observations=[Collections.Generic.List[object]]::new();$allPassed=$true;$iteration=0
    foreach($variant in @('baseline','candidate','candidate','baseline')){
        $iteration++;$directory=if($variant -ceq 'baseline'){$baseline}else{$runtime}
        $application=Join-Path $directory 'Lea.IsConnected.Tests.exe'
        $run=Invoke-OwnedTestRun -Name ('bootstrap-'+$iteration+'-'+$variant) -Application $application -Arguments @(('"'+$fixture+'"')) -WorkingDirectory $directory -OutputDirectory $out -Seconds 30 -Records $runs
        if($run.timedOut -or $run.killRequested -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $global:LeaCompositeProcessLeases.Count){throw 'Unsafe comparative attempt prevents continuation'}
        $report=@(Get-Content $run.stdout|Where-Object {$_.StartsWith('LEA_BOOTSTRAP_COMPARISON ')})
        if($report.Count -ne 1){throw 'Unique comparison report required'}
        $observation=$report[0].Substring('LEA_BOOTSTRAP_COMPARISON '.Length)|ConvertFrom-Json
        $result.observations.Add(@{iteration=$iteration;variant=$variant;passed=$run.passed;report=$observation})
        $allPassed=$allPassed -and $run.passed
        foreach($file in $result.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Comparative runtime drift prevents continuation'}}
    }
    $result.comparisonCompleted=$result.observations.Count -eq 4
    $result.scope='Fixed ABBA comparison, identical executable/dependencies/fixture/code except script DLL; original default script budget and per-process30s/5scleanup; not feature semantics or causal/reliability proof. Four preplanned attempts, no retries.'
    $result.passed=$allPassed -and $result.comparisonCompleted
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
