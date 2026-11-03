param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\portable-native-build-20261101-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/output and original reserve required'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'No test/browser overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new();$checks=[Collections.Generic.List[string]]::new()
function Hold([string]$path,[string]$expected){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse portable build input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($expected -and $hash -cne $expected){throw ('Portable build input differs: '+$path)};$inputs.Add(@{path=$path;sha256=$hash})
}
function Proof([string]$path,[string]$hash){Hold $path $hash;return Get-Content -LiteralPath $path -Raw|ConvertFrom-Json}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;nativeExecuted=$false;fullSdkQualified=$false;inputs=$inputs;runs=$runs;checks=$checks}
try{
    Hold $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $native=Proof 'I:\LeaBuildStorage\full-sdk-native-build-20261101-v1/result.json' '20E2EE1F8A87BFC2B14CD581E48B59D1DE834E87EBA729DCB5A45CCF4E1A7893'
    $tool=Proof (Join-Path $root 'continuity-evidence/modern-worker-v1/crossgen2-package-v4/result.json') '60D063F3D3F3F0C5E9036A4416C75F7590C701BAF84E15E964A0A1D53199B3EF'
    if(!$native.passed -or !$native.inputsUnchanged -or !$native.workerAclRestored -or $native.ownerAttention -or $native.nativeProducts.Count -ne 6 -or !$tool.passed -or $tool.version -cne '8.0.32' -or $tool.verifyExitCode -ne 0){throw 'Qualified source IL and tool required'}
    $compiler=@($tool.files|Where-Object path -match '\\tools\\crossgen2\.exe$')
    if($compiler.Count -ne 1){throw 'Unique verified native compiler required'}
    foreach($file in $tool.files|Where-Object path -match '\\tools\\'){Hold $file.path $file.sha256}
    $il='I:\LeaBuildStorage\full-sdk-source-20261101-v1/worker-il/Release'
    $names=@('Lea.Script.Worker.exe','Lea.Script.Worker.dll','Lea.Script.Worker.runtimeconfig.json','Lea.Script.Worker.deps.json','Jint.dll','Acornima.dll')
    foreach($name in $names){$path=Join-Path $il $name;$pin=@($native.inputs|Where-Object path -ceq $path|Select-Object -ExpandProperty sha256 -Unique);if($pin.Count -ne 1){throw ('Qualified IL input pin missing: '+$path)};Hold $path $pin[0]}
    $runtime='C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.33'
    foreach($file in Get-ChildItem $runtime -File -Filter '*.dll'){$pin=@($native.inputs|Where-Object path -ceq $file.FullName|Select-Object -ExpandProperty sha256 -Unique);if($pin.Count -ne 1){throw 'Qualified runtime reference pin missing'};Hold $file.FullName $pin[0]}
    $pwsh=(Get-Process -Id $PID).Path;Hold $pwsh
    $project=Join-Path $root 'Lea.Sdk/SDK.Full.Build.proj';Hold $project
    foreach($name in @('New-NativeToolchain.ps1','Build-NativeWorker.ps1','NativeWorker.Tools.cs')){Hold (Join-Path $root ('Lea.Sdk/'+$name))}
    $manifest=Join-Path $out 'toolchain.json'
    $generated=Invoke-OwnedTestRun -Name 'record-toolchain' -Application $pwsh -Arguments @('-NoProfile','-File',('"'+(Join-Path $root 'Lea.Sdk/New-NativeToolchain.ps1')+'"'),'-CompilerRoot',('"'+(Split-Path $compiler[0].path)+'"'),'-RuntimeRoot',('"'+$runtime+'"'),'-Output',('"'+$manifest+'"')) -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$generated.passed){throw 'Toolchain snapshot failed'};Hold $manifest
    $manifestHash=(Get-FileHash $manifest).Hash
    $dotnet='C:\Program Files\dotnet\dotnet.exe';Hold $dotnet;$msbuild='C:\Program Files\dotnet\sdk\8.0.427/MSBuild.dll';Hold $msbuild
    function Build([string]$name,[string]$configuration,[string]$hash,[string]$ilPath,[string]$output,[bool]$omit=$false){
        $arguments=@(('"'+$msbuild+'"'),('"'+$project+'"'),'/nologo','/m:1','/nodeReuse:false','/t:CompileWorkerNative','/v:minimal',('/p:LeaPowerShell="'+$pwsh+'"'),('/p:LeaWorkerIlDirectory="'+$ilPath+'"'),('/p:LeaNativeOutput="'+$output+'"'))
        if(!$omit){$arguments+=@('/p:LeaNativeToolchain="'+$configuration+'"','/p:LeaNativeToolchainSha256='+$hash)}
        return Invoke-OwnedTestRun -Name $name -Application $dotnet -Arguments $arguments -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    }
    function Denied($run,[string]$output,[string]$message){
        if($run.passed -or $run.exitCode -ne 1 -or $run.timedOut -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or (Test-Path -LiteralPath $output)){throw ('Negative build was not safely rejected: '+$message)}
        $checks.Add($message)
    }
    $output=Join-Path $out 'positive/ScriptWorker'
    $built=Build 'compile-native' $manifest $manifestHash $il $output
    if(!$built.passed -or @(Get-Content $built.stdout|Where-Object {$_ -match 'LEA_NATIVE_BUILT'}).Count -ne 1){throw 'Portable MSBuild native target failed'}
    $receipt=Get-Content ($output+'.build.json') -Raw|ConvertFrom-Json
    if(!$receipt.succeeded -or $receipt.outputs.Count -ne 6){throw 'Native receipt incomplete'}
    $result.outputs=@(foreach($name in $names){$path=Join-Path $output $name;Hold $path;@{path=$path;sha256=(Get-FileHash $path).Hash}})
    $result.identicalToQualifiedWorker=@($result.outputs|Where-Object {$file=$_;$expected=@($native.nativeProducts|Where-Object {[IO.Path]::GetFileName($_.path) -ceq [IO.Path]::GetFileName($file.path)});$expected.Count -ne 1 -or $expected[0].sha256 -cne $file.sha256}).Count -eq 0
    $checks.Add('native target produces verified six-file closure')
    $cached=Build 'native-up-to-date' $manifest $manifestHash $il $output
    if(!$cached.passed -or @(Get-Content $cached.stdout|Where-Object {$_ -match 'LEA_NATIVE_UP_TO_DATE'}).Count -ne 1){throw 'Unchanged native build did not verify cache'};$checks.Add('unchanged inputs verify outputs without recompiling')
    $negative=Join-Path $out 'missing-manifest/ScriptWorker';Denied (Build 'deny-missing-manifest' '' '' $il $negative $true) $negative 'missing toolchain rejected'
    $negative=Join-Path $out 'wrong-hash/ScriptWorker';Denied (Build 'deny-wrong-hash' $manifest ('0'*64) $il $negative) $negative 'wrong toolchain SHA rejected'
    $config=Get-Content $manifest -Raw|ConvertFrom-Json;$config.runtimeFiles=@($config.runtimeFiles|Select-Object -SkipLast 1)
    $incomplete=Join-Path $out 'incomplete-toolchain.json';$config|ConvertTo-Json -Depth 6|Set-Content $incomplete -Encoding utf8;Hold $incomplete
    $negative=Join-Path $out 'missing-reference/ScriptWorker';Denied (Build 'deny-reference-inventory' $incomplete (Get-FileHash $incomplete).Hash $il $negative) $negative 'unrecorded runtime reference rejected'
    $badIl=Join-Path $out 'il-only-dependency';New-Item -ItemType Directory -Path $badIl|Out-Null
    foreach($name in $names){Copy-Item -LiteralPath (Join-Path $il $name) -Destination (Join-Path $badIl $name)}
    $frameworkJint=Join-Path $root 'continuity-evidence/sdk-full-script-suite-v7/runtime/Jint.dll';Hold $frameworkJint 'B54618A4E3FFE33344D478D3A1580658C4D91819B3B97A24326CB3626CB7FD1A'
    Copy-Item -LiteralPath $frameworkJint -Destination (Join-Path $badIl 'Jint.dll')
    $negative=Join-Path $out 'il-only-output/ScriptWorker';Denied (Build 'deny-il-dependency' $manifest $manifestHash $badIl $negative) $negative 'IL-only interpreter dependency rejected'
    $badPolicy=Join-Path $out 'bad-runtime-policy';New-Item -ItemType Directory -Path $badPolicy|Out-Null
    foreach($name in $names){Copy-Item -LiteralPath (Join-Path $il $name) -Destination (Join-Path $badPolicy $name)}
    $policyPath=Join-Path $badPolicy 'Lea.Script.Worker.runtimeconfig.json';$policy=Get-Content $policyPath -Raw|ConvertFrom-Json;$policy.runtimeOptions.rollForward='LatestPatch';$policy|ConvertTo-Json -Depth 6|Set-Content $policyPath -Encoding utf8
    $negative=Join-Path $out 'bad-policy-output/ScriptWorker';Denied (Build 'deny-runtime-policy' $manifest $manifestHash $badPolicy $negative) $negative 'relaxed runtime policy rejected'
    $tampered=Join-Path $out 'tampered/ScriptWorker';New-Item -ItemType Directory -Path $tampered|Out-Null
    foreach($name in $names){Copy-Item -LiteralPath (Join-Path $output $name) -Destination (Join-Path $tampered $name)}
    Copy-Item -LiteralPath ($output+'.build.json') -Destination ($tampered+'.build.json')
    $changed=Join-Path $tampered 'Lea.Script.Worker.dll';$bytes=[IO.File]::ReadAllBytes($changed);$bytes[-1]=$bytes[-1] -bxor 1;[IO.File]::WriteAllBytes($changed,$bytes)
    $denied=Build 'deny-tampered-cache' $manifest $manifestHash $il $tampered
    if($denied.passed -or $denied.exitCode -ne 1 -or $denied.timedOut -or $denied.activeAfterRetirement -ne 0 -or !$denied.outputDrained -or (Get-FileHash $changed).Hash -ceq ($result.outputs|Where-Object path -ceq (Join-Path $output 'Lea.Script.Worker.dll')).sha256){throw 'Tampered cache not rejected/preserved'}
    $checks.Add('modified cached image rejected without replacing it')
    $result.passed=$checks.Count -eq 8
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaPortableNativeBuildLeases=$holds}
    $result|ConvertTo-Json -Depth 9|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,identicalToQualifiedWorker,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
