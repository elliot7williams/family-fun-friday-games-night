param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$out='I:\LeaBuildStorage\trusted-bootstrap-profile-build-20261102-v4'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Test-Path $out) -or (Get-PSDrive C).Free -lt 600MB -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh output/owner/reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$hash){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
    if(!$hash){$hash=(Get-FileHash $path).Hash};if((Get-FileHash $path).Hash -cne $hash){throw 'Build input differs'};$inputs.Add(@{path=$path;sha256=$hash})
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;diagnosticOnly=$true;runtimeExecuted=$false;testedPackageChanged=$false}
try{
    Pin $PSCommandPath
    $source='I:\LeaBuildStorage\full-sdk-source-20261101-v3'
    $manifest=Join-Path $source 'source-manifest.json';Pin $manifest 'B5A0283803C7062DA98A7F3331D94940360E00045A06038168FCA94214CD114A'
    $plan=Get-Content $manifest -Raw|ConvertFrom-Json
    $trace='I:\LeaBuildStorage\trusted-bootstrap-profile-source-20261102-v4\source.json';Pin $trace '8A80C461B25495B17862096469E0CC17AC061947380CB59019C47CB8136F60EC'
    $t=Get-Content $trace -Raw|ConvertFrom-Json
    if(!$t.verified -or !$t.inverseVerified -or $t.negativeControls -ne 6 -or $t.files.Count -ne 3 -or !$t.resourceMarkerOrderVerified -or $t.markerNegativeControls -ne 3 -or !$t.diagnosticOnly -or $t.budgetsChanged -or !$t.criticalOperationsVerified -or $t.pairShapeControls -ne 1 -or !$t.resourceCounters -or $t.telemetryNegativeControls -ne 3){throw 'Diagnostic source boundary incomplete'}
    $stage=Join-Path $out 'source'
    foreach($file in $plan.files|Where-Object {$_.path.StartsWith('Lea.Script/') -or $_.path -ceq 'Directory.Build.targets' -or ($_.path.StartsWith('lib/') -and !$_.path.StartsWith('lib/worker/'))}){
        if([IO.Path]::IsPathRooted($file.path) -or $file.path.Contains(':') -or $file.path.Split('/') -contains '..'){throw 'Source path escape'}
        $from=Join-Path $source $file.path;Pin $from $file.sha256
        $overlay=@($t.files|Where-Object {('Lea.Script/'+$_.name) -ceq $file.path})
        if($overlay.Count){if($overlay.Count -ne 1 -or $overlay[0].originalSha256 -cne $file.sha256){throw 'Overlay original differs'};$from=$overlay[0].path;Pin $from $overlay[0].sha256}
        $to=Join-Path $stage $file.path;New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null;Copy-Item -LiteralPath $from -Destination $to;Pin $to (Get-FileHash $from).Hash
    }
    $core='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1\a\bin\Release\Lea.Engine.dll';Pin $core '9A968D02E8679D02F001B3B68C7E9A347D3DEB540AFFC04887E9238FFC87E5FB'
    $bin=Join-Path $stage 'bin/Release';New-Item -ItemType Directory -Path $bin -Force|Out-Null;Copy-Item -LiteralPath $core -Destination $bin
    Pin (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    Pin (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    $dotnet='C:\Program Files\dotnet\dotnet.exe';Pin $dotnet
    $msbuild='C:\Program Files\dotnet\sdk\8.0.427\MSBuild.dll';Pin $msbuild
    $args=@(('"'+$msbuild+'"'),('"'+(Join-Path $stage 'Lea.Script/Lea.Script.csproj')+'"'),'/nologo','/m:1','/nodeReuse:false','/t:Rebuild','/v:minimal','/p:Configuration=Release','/p:UseSharedCompilation=false')
    $run=Invoke-OwnedTestRun -Name 'compile-trusted-bootstrap-profile' -Application $dotnet -Arguments $args -WorkingDirectory $stage -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$run.passed){throw 'Diagnostic script compilation failed'}
    $result.binary=Join-Path $bin 'Lea.Script.dll';$result.sha256=(Get-FileHash $result.binary).Hash;$result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaRetirementBuildLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,sha256,inputsUnchanged,ownerAttention,error|ConvertTo-Json -Compress
}
