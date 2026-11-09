param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$out='I:\LeaBuildStorage\recovered-script-parent-build-20261109-v1'
$recovery='I:\LeaBuildStorage\signed-font-startup-investigation-20261108-v1'
$sdk='I:\LeaBuildStorage\github-source-checkpoint-20261109-v1\LeaSourceCheckpoints\20261102\mature-sdk'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Get-PSDrive I).Free -lt 64MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/output and original reserves required'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$expected){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $stream=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($stream)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($expected -and $hash -cne $expected){throw ('Recovered build input differs: '+$path)}
    $inputs.Add(@{path=$path;sha256=$hash})
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;compiled=$false;runtimeExecuted=$false;hostAdopted=$false;inputs=$inputs;runs=$runs;historicalMetadataRecovered=$true;expectedSha256='406C5634638D75C66892BD95429760CB4A5C74C94277913D1DF467147E30E75F'}
try{
    Pin $PSCommandPath
    $receipt=Join-Path $recovery 'parent-build.json';Pin $receipt 'D32732C0BF3B548DB649966100D4B074663E07544BC9C25A648A80D6723AB02C'
    $old=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
    $sourceProof=Join-Path $recovery 'parent-source-complete-20261109-v1.json';Pin $sourceProof '74C7A0C634DD9D64ED34BA67C8F652A720B68EE9B4E72360DC8B16FC31F58D02'
    $recovered=Get-Content -LiteralPath $sourceProof -Raw|ConvertFrom-Json
    if(!$recovered.verified -or $recovered.files -ne 15 -or !$recovered.historicalMetadataRecovered){throw 'All15exact source inputs required'}
    $manifest=Join-Path $sdk 'source-manifest.json';Pin $manifest 'B5A0283803C7062DA98A7F3331D94940360E00045A06038168FCA94214CD114A'
    $plan=Get-Content -LiteralPath $manifest -Raw|ConvertFrom-Json
    $source=Join-Path $out 'source';$projectDir=Join-Path $source 'Lea.Script'
    New-Item -ItemType Directory -Path $projectDir,(Join-Path $source 'lib'),(Join-Path $source 'bin/Release')|Out-Null
    foreach($file in $recovered.entries){
        Pin $file.path $file.sha256
        $to=Join-Path $projectDir $file.name;Copy-Item -LiteralPath $file.path -Destination $to;Pin $to $file.sha256
    }
    foreach($name in @('Directory.Build.targets','Lea.Script/Lea.Script.csproj')){
        $entry=@($plan.files|Where-Object path -CEQ $name)
        if($entry.Count -ne 1){throw 'Unique original project input required'}
        $from=Join-Path $sdk $name;Pin $from $entry[0].sha256
        $to=Join-Path $source $name;Copy-Item -LiteralPath $from -Destination $to;Pin $to $entry[0].sha256
    }
    $assets='I:\LeaBuildStorage\private-binding-full-assembly-20261103-v5-assets\fixtures\Lea.Script'
    foreach($entry in @(@{name='Dom.js';sha256='A228494E1D20D2669DC2FED38B93B7549EF8849FA2C7997F45EE32678117239A'},@{name='Dom.compact.js';sha256='196BCE962F2C2F375C77644D0AE19485820F599170D4D80013829A6ECD2886E3'})){
        $from=Join-Path $assets $entry.name;Pin $from $entry.sha256
        $to=Join-Path $projectDir $entry.name;Copy-Item -LiteralPath $from -Destination $to;Pin $to $entry.sha256
    }
    function OldPin([string]$name){
        $values=@($old.inputs|Where-Object {[IO.Path]::GetFileName($_.path) -ceq $name}|Select-Object -ExpandProperty sha256 -Unique)
        if($values.Count -ne 1){throw ('Original dependency identity ambiguous: '+$name)}
        return $values[0]
    }
    foreach($name in @('Acornima.dll','Jint.dll','System.Buffers.dll','System.Memory.dll','System.Numerics.Vectors.dll','System.Runtime.CompilerServices.Unsafe.dll')){
        $from=Join-Path (Split-Path $PSScriptRoot -Parent) ('continuity-evidence/private-binding-full-suite-v5/runtime/'+$name)
        $pin=OldPin $name;Pin $from $pin
        $to=Join-Path $source ('lib/'+$name);Copy-Item -LiteralPath $from -Destination $to;Pin $to $pin
    }
    $core='I:\LeaBuildStorage\private-binding-full-suite-v3-archive-20261109-v1\stage\runtime\Lea.Engine.dll';$corePin=OldPin 'Lea.Engine.dll';Pin $core $corePin
    $to=Join-Path $source 'bin/Release/Lea.Engine.dll';Copy-Item -LiteralPath $core -Destination $to;Pin $to $corePin
    $owner=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Pin $owner 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owner
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Pin $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $dotnet='C:\Program Files\dotnet\dotnet.exe';Pin $dotnet (OldPin 'dotnet.exe')
    $msbuild='C:\Program Files\dotnet\sdk\8.0.427\MSBuild.dll';Pin $msbuild (OldPin 'MSBuild.dll')
    $env:TEMP=Join-Path $out 'temp';$env:TMP=$env:TEMP;New-Item -ItemType Directory -Path $env:TEMP|Out-Null
    $project=Join-Path $projectDir 'Lea.Script.csproj'
    $arguments=@(('"'+$msbuild+'"'),('"'+$project+'"'),'/nologo','/m:1','/nodeReuse:false','/t:Rebuild','/v:minimal','/p:Configuration=Release','/p:UseSharedCompilation=false')
    $run=Invoke-OwnedTestRun -Name 'rebuild-recovered-parent' -Application $dotnet -Arguments $arguments -WorkingDirectory $source -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$run.passed){throw 'Recovered parent project build failed'}
    $result.compiled=$true;$result.binary=Join-Path $source 'bin/Release/Lea.Script.dll'
    Pin $result.binary;$result.sha256=(Get-FileHash -LiteralPath $result.binary).Hash
    $result.byteIdentical=$result.sha256 -ceq $result.expectedSha256
    if(!$result.byteIdentical){throw 'Compiled successfully, but original byte identity is not proved'}
    $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention -and !$result.error
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaRecoveredParentLeases=$holds}
    $result|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,compiled,byteIdentical,sha256,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
