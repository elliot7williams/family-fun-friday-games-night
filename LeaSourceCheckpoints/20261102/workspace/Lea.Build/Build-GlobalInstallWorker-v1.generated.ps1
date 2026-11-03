param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$out='I:\LeaBuildStorage\global-install-worker-build-20261102-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Test-Path $out) -or (Get-PSDrive C).Free -lt 600MB -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh output/owner/reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new();$runs=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$hash){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse worker input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);if(!$hash){$hash=(Get-FileHash $path).Hash};if((Get-FileHash $path).Hash -cne $hash){throw 'Worker build input differs'};$inputs.Add(@{path=$path;sha256=$hash})
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;workerExecuted=$false;runtimeQualified=$false;hostAdopted=$false;products=@();scope='Matching global-install/isConnected worker IL and ReadyToRun source build only; sandbox/protocol unchanged'}
try{
    Pin $PSCommandPath
    $source='I:\LeaBuildStorage\full-sdk-source-20261101-v3'
    $manifest=Join-Path $source 'source-manifest.json';Pin $manifest 'B5A0283803C7062DA98A7F3331D94940360E00045A06038168FCA94214CD114A'
    $plan=Get-Content $manifest -Raw|ConvertFrom-Json
    $feature='I:\LeaBuildStorage\global-install-source-20261102-v1\source.json';Pin $feature '8752B10BDD88F4AAFFFB016AFDC4EB0D49B4A56D054AA4738D461BAD415A46B5'
    $f=Get-Content $feature -Raw|ConvertFrom-Json
    $resource=@($f.files|Where-Object name -CEQ 'Dom.compact.js')
    if(!$f.verified -or !$f.inverseVerified -or $resource.Count -ne 1 -or !$f.candidateOnly -or $f.budgetsChanged -or $f.negativeControls -ne 6 -or $f.globalDescriptorCallsRemoved -ne 1){throw 'Connectivity resource provenance incomplete'}
    Pin $resource[0].path $resource[0].sha256
    $stage=Join-Path $out 'source';$overlays=0
    $files=@($plan.files|Where-Object {$_.path.StartsWith('Lea.Script.Worker/') -or $_.path.StartsWith('lib/worker/') -or $_.path -cin @('Build-NativeWorker.ps1','NativeWorker.Tools.cs','Directory.Build.targets')})
    foreach($file in $files){
        if([IO.Path]::IsPathRooted($file.path) -or $file.path.Contains(':') -or $file.path.Split('/') -contains '..'){throw 'Worker source escape'}
        $from=Join-Path $source $file.path;Pin $from $file.sha256;$hash=$file.sha256
        if($file.path -ceq 'Lea.Script.Worker/Dom.compact.js'){
            if($file.sha256 -cne $resource[0].sdkOriginalSha256){throw 'Worker resource differs from trusted original'}
            $from=$resource[0].path;$hash=$resource[0].sha256;$overlays++
        }
        $to=Join-Path $stage $file.path;New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null;Copy-Item -LiteralPath $from -Destination $to;Pin $to $hash
    }
    if($overlays -ne 1){throw 'Only matching DOM resource substitution required'}
    Pin (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    Pin (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    $dotnet='C:\Program Files\dotnet\dotnet.exe';Pin $dotnet
    $msbuild='C:\Program Files\dotnet\sdk\8.0.427\MSBuild.dll';Pin $msbuild
    $arguments=@(('"'+$msbuild+'"'),('"'+(Join-Path $stage 'Lea.Script.Worker/Lea.Script.Worker.csproj')+'"'),'/nologo','/m:1','/nodeReuse:false','/t:Restore;Build','/v:minimal','/p:Configuration=Release','/p:UseSharedCompilation=false')
    $il=Invoke-OwnedTestRun -Name 'build-global-install-worker-il' -Application $dotnet -Arguments $arguments -WorkingDirectory $stage -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$il.passed){throw 'Connectivity worker IL compile failed'}
    $toolchain='I:\LeaBuildStorage\portable-native-build-20261101-v1\toolchain.json';$toolchainHash='7B13FE166AD9AF8A3B142C40CB046EAABA1A33759B5A9C505F97B7A83678C003';Pin $toolchain $toolchainHash
    $native=Join-Path $out 'ScriptWorker';$pwsh=(Get-Process -Id $PID).Path;Pin $pwsh
    $arguments=@('-NoProfile','-NonInteractive','-File',('"'+(Join-Path $stage 'Build-NativeWorker.ps1')+'"'),'-Toolchain',('"'+$toolchain+'"'),'-ToolchainSha256',$toolchainHash,'-IlDirectory',('"'+(Join-Path $stage 'worker-il/Release')+'"'),'-OutputDirectory',('"'+$native+'"'))
    $run=Invoke-OwnedTestRun -Name 'build-global-install-worker-native' -Application $pwsh -Arguments $arguments -WorkingDirectory $stage -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$run.passed -or @(Get-Content $run.stdout|Where-Object {$_ -match 'LEA_NATIVE_BUILT'}).Count -ne 1){throw 'Connectivity native worker build failed'}
    $products=@(Get-ChildItem $native -File)
    $expected=@('Lea.Script.Worker.exe','Lea.Script.Worker.dll','Lea.Script.Worker.deps.json','Lea.Script.Worker.runtimeconfig.json','Jint.dll','Acornima.dll')
    if($products.Count -ne 6 -or @($products|Where-Object Name -CNotIn $expected).Count){throw 'Six-file worker closure required'}
    $options=(Get-Content (Join-Path $native 'Lea.Script.Worker.runtimeconfig.json') -Raw|ConvertFrom-Json).runtimeOptions
    if($options.framework.version -cne '8.0.33' -or $options.rollForward -cne 'Disable'){throw 'Exact runtime policy differs'}
    $result.products=@($products|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;bytes=$_.Length}});$result.resourceSha256=$resource[0].sha256;$result.sourceMembers=$files.Count;$result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaConnectedWorkerBuildLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json -Compress
}
