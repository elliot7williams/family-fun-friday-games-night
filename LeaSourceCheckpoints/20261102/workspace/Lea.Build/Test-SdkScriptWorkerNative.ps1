param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\sdk-script-worker-native-20261101-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin owner required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/evidence and reserve required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$pin){$s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));if($pin -and $pin -cne $hash){throw ('Native worker input differs: '+$path)};$inputs.Add(@{path=$path;sha256=$hash})}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;workerExecuted=$false;runtimeQualified=$false;completeSdk=$false;compilerVersion='8.0.32';runtimeVersion='8.0.33'}
try{
    Hold $PSCommandPath
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $proof='I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-script-worker\result.json';Hold $proof '2DCE0B8004660AD5FD0A8621D748F9373D26547B7E1453C735EF965BE353EBD9'
    $build=Get-Content $proof -Raw|ConvertFrom-Json
    if(!$build.passed -or !$build.scriptWorkerIlCompiled -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.outputs.Count -ne 6){throw 'Worker IL build incomplete'}
    $runtime=Join-Path $out 'ScriptWorker';New-Item -ItemType Directory -Path $runtime|Out-Null
    foreach($file in $build.outputs){
        Hold $file.path $file.sha256;$name=[IO.Path]::GetFileName($file.path)
        if($name -ceq 'Lea.Script.Worker.dll'){$il=$file.path;continue}
        $to=Join-Path $runtime $name;Copy-Item $file.path $to;Hold $to $file.sha256
    }
    $toolProof=Join-Path $root 'continuity-evidence/modern-worker-v1/crossgen2-package-v4/result.json';Hold $toolProof '60D063F3D3F3F0C5E9036A4416C75F7590C701BAF84E15E964A0A1D53199B3EF'
    $tool=Get-Content $toolProof -Raw|ConvertFrom-Json
    if(!$tool.passed -or $tool.version -cne '8.0.32' -or $tool.verifyExitCode -ne 0){throw 'Crossgen package proof incomplete'}
    foreach($file in $tool.files|Where-Object {$_.path -match '\\tools\\'}){if(!$file.sha256){throw 'Unpinned compiler tool'};Hold $file.path $file.sha256}
    $compilers=@($tool.files|Where-Object {$_.path -match '\\tools\\crossgen2\.exe$'})
    if($compilers.Count -ne 1){throw 'Unique compiler missing'};$compiler=$compilers[0].path
    $native=Join-Path $runtime 'Lea.Script.Worker.dll'
    $arguments=@(('"'+$il+'"'),('-o:"'+$native+'"'),'--targetos:windows','--targetarch:x64','-O')
    $framework='C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.33'
    $references=@(Get-ChildItem -LiteralPath $framework -Filter *.dll -File)
    if($references.Count -lt 150 -or $references.Count -gt 256){throw 'Runtime reference coverage unexpected'}
    foreach($file in $references){Hold $file.FullName;$arguments+=('-r:"'+$file.FullName+'"')}
    foreach($name in @('Jint.dll','Acornima.dll')){$arguments+=('-r:"'+(Join-Path $runtime $name)+'"')}
    $recipe=Join-Path $out 'native.rsp';[IO.File]::WriteAllLines($recipe,$arguments,[Text.UTF8Encoding]::new($false));Hold $recipe
    $run=Invoke-OwnedTestRun -Name 'compile-native-worker' -Application $compiler -Arguments @('@"'+$recipe+'"') -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$run.passed){throw 'Native worker compilation failed'};Hold $native
    $reader=Join-Path $PSScriptRoot 'ReadReadyToRun.cs';Hold $reader;Add-Type -Path $reader
    $nativeChecks=@(foreach($name in @('Lea.Script.Worker.dll','Jint.dll','Acornima.dll')){$path=Join-Path $runtime $name;@{path=$path;x64ReadyToRun=[ReadReadyToRun]::IsX64($path)}})
    if(@($nativeChecks|Where-Object {!$_.x64ReadyToRun}).Count){throw 'Worker closure lacks x64 ReadyToRun headers'}
    $result.nativeHeaderChecks=$nativeChecks
    $result.outputs=@(Get-ChildItem $runtime -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash}})
    $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaNativeWorkerLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
