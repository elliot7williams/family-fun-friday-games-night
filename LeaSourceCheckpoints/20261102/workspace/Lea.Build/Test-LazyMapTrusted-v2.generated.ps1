param([switch]$KeepForegroundHost,[ValidateSet(1,2,3)][int]$Version=2,[switch]$Isolated)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$out=Join-Path $root 'continuity-evidence/lazy-map-trusted-v2'
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
    $buildPath='I:\LeaBuildStorage\lazy-map-build-20261102-v1/result.json';Pin $buildPath 'F4194E982B87835E9EC08F553136E558C0C0652C48961D97E1FA3A794547007C'
    $b=Get-Content $buildPath -Raw|ConvertFrom-Json
    if(!$b.passed -or !$b.inputsUnchanged -or $b.ownerAttention -or $b.sha256 -cne 'A17246FBF5F0EA68B931E7125ECD41189CC21ABA8D798396CB339C74F7728098'){throw 'Connectivity build incomplete'}
    $runtime=Join-Path $out 'runtime';New-Item -ItemType Directory -Path $runtime|Out-Null
    $dlls=@(Get-ChildItem (Split-Path $b.binary) -Filter '*.dll' -File)
    if($dlls.Count -ne 8){throw 'Eight-file trusted closure required'}
    if($Isolated){throw 'This campaign qualifies trusted scheduling only'}
    $stageBytes=[long](($dlls|Measure-Object Length -Sum).Sum)+857600+350720+256KB
    if((Get-PSDrive C).Free -lt (600MB+$stageBytes)){throw 'Scheduling reserve plus full staged closure required'}
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
    $oldTests=Join-Path $root 'continuity-evidence/connectivity-dom-regressions-v1/runtime/Lea.Script.Tests.exe'
    Pin $oldTests '6E46584EA1539D331D8FC17D73F8AC59081BF6BF98E31A7B89F0BF6375890D48'
    $testLibrary=Join-Path $runtime 'Lea.Script.Tests.exe';Copy-Item -LiteralPath $oldTests -Destination $testLibrary
    Pin $testLibrary '6E46584EA1539D331D8FC17D73F8AC59081BF6BF98E31A7B89F0BF6375890D48'
    $sdkPath='I:/LeaBuildStorage/sdk-reproducibility-20261101-v1/result.json';Pin $sdkPath '39955D0AC2D23B60360000414FDB3702F928B4CA1537C074D2C6007BD174B199'
    $sdk=Get-Content $sdkPath -Raw|ConvertFrom-Json
    foreach($name in @('Lea.Win.dll','Lea.Script.Win.dll')){
        $product=@($sdk.products|Where-Object path -CEQ ('bin/Release/'+$name))
        if($product.Count -ne 1 -or !$product[0].equal){throw 'Exact view dependency required'}
        Pin $product[0].first $product[0].sha256;Copy-Item -LiteralPath $product[0].first -Destination $runtime
    }
    $fixtureProof=Join-Path $root 'continuity-evidence/sdk-full-script-suite-v8/staging.json';Pin $fixtureProof '90FD0D60BA1CCE04C0923D6E18E7FB5D218F4F04C129DD49F97477AFFD2384CA'
    $manifest=Get-Content $fixtureProof -Raw|ConvertFrom-Json
    $row=@($manifest.files|Where-Object path -CEQ 'fixtures/LeaForge/test-pages/javascript-attributes.html')
    if($row.Count -ne 1){throw 'Original scheduling fixture required'}
    $fixture=$row[0].origin;Pin $fixture $row[0].sha256
    $source=Join-Path $root 'Lea.Tests/LazyMapTrustedMain.cs';Pin $source
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
    $result.candidateOnly=$true;$result.completed=$false;$result.cases=[Collections.Generic.List[object]]::new()
    $result.before=@(Get-ChildItem $runtime -Recurse -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}})
    foreach($name in @('ClassLists','FirstUse')){
        $run=Invoke-OwnedTestRun -Name ('trusted-'+$name) -Application $exe -Arguments @(('"'+$testLibrary+'"'),$name,('"'+$fixture+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
        $pattern='^PASS Lea lazy-map trusted '+$name+': ([0-9]+) checks\.$'
        $footers=@(Get-Content $run.stdout|Where-Object {$_ -cmatch $pattern})
        $count=if($footers.Count -eq 1 -and $footers[0] -cmatch $pattern){[int]$Matches[1]}else{0}
        $ok=$run.passed -and $count -gt 0
        $result.cases.Add(@{name=$name;passed=$ok;checks=$count})
        if(!$ok){throw ('Original trusted scheduling suite failed: '+$name)}
        foreach($file in $result.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Scheduling runtime drift prevents continuation'}}
        if($global:LeaCompositeProcessLeases.Count -or (Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)){throw 'Scheduling retirement incomplete'}
    }
    $result.completed=$result.cases.Count -eq 2;$result.passed=$result.completed
    $result.scope='Cold original trusted ClassLists.Suite followed by separate fresh-process captured-constructor/early-brand/revocation probe. No warmup or changed assertions/deadlines. Not isolated, timing, full-suite, host or release qualification.'
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
