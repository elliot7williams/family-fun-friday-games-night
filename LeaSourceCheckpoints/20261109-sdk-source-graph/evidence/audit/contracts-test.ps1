param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$graph='I:\LeaBuildStorage\sdk-source-graph-20261109-v2'
$out='I:\LeaBuildStorage\sdk-source-graph-contracts-20261109-v1'
$runtime=Join-Path $root 'continuity-evidence/sdk-source-graph-contracts-v1/runtime'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 620MB -or (Get-PSDrive I).Free -lt 32MB -or (Test-Path $out) -or (Test-Path $runtime) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/outputs and original reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Pin([string]$Path,[string]$Expected){
    $walk=Get-Item -LiteralPath $Path
    while($walk){if($walk.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse input refused'};$walk=if($walk -is [IO.FileInfo]){$walk.Directory}else{$walk.Parent}}
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($stream);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($Expected -and $hash -cne $Expected){throw ('Input differs: '+$Path)};$inputs.Add(@{path=$Path;sha256=$hash})
}
New-Item -ItemType Directory -Path $out,$runtime|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;before=@();scriptExecuted=$false;nativeWorkerQualified=$false;completeSdk=$false;hostAdopted=$false}
try{
    Pin $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'test.ps1')
    $receipt=Join-Path $graph 'result.json';Pin $receipt 'E9F481961EDB427F255715F51F28F4ED0E9D675EEA79C10D5E97B772C339A8BF'
    $g=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
    if(!$g.passed -or !$g.sourceGraphCompiled -or !$g.embeddingPassed -or !$g.inputsUnchanged -or $g.ownerAttention -or $g.outputs.Count -ne 4){throw 'Verified source graph required'}
    $names=@('Lea.Engine.dll','Lea.Win.dll','Lea.Script.dll','Lea.Script.Win.dll','Acornima.dll','Jint.dll','System.Buffers.dll','System.Memory.dll','System.Numerics.Vectors.dll','System.Runtime.CompilerServices.Unsafe.dll')
    foreach($name in $names){
        $row=@($g.outputs|Where-Object {($_.name+'.dll') -ceq $name})
        if($row.Count -eq 1){$hash=$row[0].sha256}else{
            $lib=Join-Path $graph ('source/lib/'+$name);$pins=@($g.inputs|Where-Object path -CEQ $lib|Select-Object -ExpandProperty sha256 -Unique)
            if($pins.Count -ne 1){throw 'Dependency pin ambiguous'};$hash=$pins[0]
        }
        $from=Join-Path $graph ('source/bin/Release/'+$name);Pin $from $hash
        $to=Join-Path $runtime $name;Copy-Item -LiteralPath $from -Destination $to;Pin $to $hash
    }
    $modelProof=Join-Path $root 'continuity-evidence/font-net48-relative-candidate-v1/result.json';Pin $modelProof 'E1A27B9A2ACCCAC6820393FEEBBD6145911D30E9ECF50459DE28E9AB5B4FC46A'
    $model=Get-Content -LiteralPath $modelProof -Raw|ConvertFrom-Json
    $modelFolder=Split-Path $modelProof -Parent
    foreach($name in @('FontRelativeMain.cs','font-shorthand.html')){
        $from=Join-Path $modelFolder $name;$pins=@($model.inputs|Where-Object path -CEQ $from|Select-Object -ExpandProperty sha256 -Unique)
        if($pins.Count -ne 1){throw 'Font contract source pin ambiguous'};Pin $from $pins[0]
        $to=Join-Path $out $name;Copy-Item -LiteralPath $from -Destination $to;Pin $to $pins[0]
    }
    $owner=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Pin $owner 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owner
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Pin $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $compiler='C:\Program Files\dotnet\dotnet.exe';Pin $compiler
    $toolchain='C:\Program Files\dotnet\sdk\8.0.427\Roslyn\bincore'
    foreach($name in @('csc.dll','csc.deps.json','csc.runtimeconfig.json','Microsoft.CodeAnalysis.dll','Microsoft.CodeAnalysis.CSharp.dll')){Pin (Join-Path $toolchain $name)}
    $refs=@();$framework='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    foreach($name in @('mscorlib','System','System.Core','System.Drawing','System.Windows.Forms','Facades/netstandard')){$path=Join-Path $framework ($name+'.dll');Pin $path;$refs+=('/reference:"'+$path+'"')}
    foreach($name in $names){$refs+=('/reference:"'+(Join-Path $runtime $name)+'"')}
    $cases=@(@{name='source-contract';source=(Join-Path $root 'Lea.Tests/SdkSourceContractMain.cs');exe='Lea.SourceContract.Tests.exe';footer='PASS SDK SOURCE CONTRACT 14';args=@()},@{name='signed-font-contract';source=(Join-Path $out 'FontRelativeMain.cs');exe='Lea.SourceFont.Tests.exe';footer='FONT_RELATIVE controls=10 cases=18 failures=0';args=@(('"'+(Join-Path $out 'font-shorthand.html')+'"'))})
    foreach($case in $cases){
        Pin $case.source;$exe=Join-Path $runtime $case.exe
        $options=@('/nologo','/noconfig','/nostdlib+','/target:exe','/optimize+','/langversion:12',('/out:"'+$exe+'"'),('"'+$case.source+'"'))+$refs
        $rsp=Join-Path $out ($case.name+'.rsp');[IO.File]::WriteAllLines($rsp,$options);Pin $rsp
        $compile=Invoke-OwnedTestRun -Name ('compile-'+$case.name) -Application $compiler -Arguments @(('"'+(Join-Path $toolchain 'csc.dll')+'"'),('@"'+$rsp+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 120 -Records $runs
        if(!$compile.passed){throw ('Contract compile failed: '+$case.name)};Pin $exe
    }
    $result.before=@(Get-ChildItem -LiteralPath $runtime -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    foreach($case in $cases){
        $run=Invoke-OwnedTestRun -Name $case.name -Application (Join-Path $runtime $case.exe) -Arguments $case.args -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
        if(!$run.passed -or @(Get-Content -LiteralPath $run.stdout|Where-Object {$_ -ceq $case.footer}).Count -ne 1){throw ('Source contract failed: '+$case.name)}
    }
    $result.componentChecks=14;$result.fontChecks=28;$result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@($result.before|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256 -or (Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention -and !$result.error
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaSdkContractLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,componentChecks,fontChecks,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
